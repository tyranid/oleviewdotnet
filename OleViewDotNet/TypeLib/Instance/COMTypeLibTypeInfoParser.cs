//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
//
//    OleViewDotNet is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    OleViewDotNet is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with OleViewDotNet.  If not, see <http://www.gnu.org/licenses/>.

using NtApiDotNet;
using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib.Instance;

internal sealed class COMTypeLibTypeInfoParser : IDisposable
{
    private readonly COMTypeLibParserContext _context;
    private readonly COMTypeInfoInstance _type_info;
    private readonly TYPEATTR _attr;

    internal COMTypeLibInterface ParseInterface()
    {
        var ret = _context.ParsedIntfs.GetOrAdd(_attr.guid,
            new COMTypeLibInterface(_type_info.Documentation, _attr));
        ret.Parse(this);
        return ret;
    }

    internal COMTypeLibDispatch ParseDispatch()
    {
        var ret = _context.ParsedDisp.GetOrAdd(_attr.guid,
            new COMTypeLibDispatch(_type_info.Documentation, _attr));
        ret.Parse(this);
        return ret;
    }

    internal COMTypeLibTypeInfo ParseType(COMTypeLibTypeInfo type)
    {
        var key = Tuple.Create(type.Name, type.Kind);
        var ret = _context.NamedTypes.GetOrAdd(key, type);
        ret.Parse(this);
        return ret;
    }

    internal COMTypeLibTypeInfo Parse()
    {
        var doc = GetDocumentation();
        return _attr.typekind switch
        {
            TYPEKIND.TKIND_INTERFACE => ParseInterface(),
            TYPEKIND.TKIND_DISPATCH => ParseDispatch(),
            TYPEKIND.TKIND_ALIAS => ParseType(new COMTypeLibAlias(doc, _attr)),
            TYPEKIND.TKIND_ENUM => ParseType(new COMTypeLibEnum(doc, _attr)),
            TYPEKIND.TKIND_RECORD => ParseType(new COMTypeLibRecord(doc, _attr)),
            TYPEKIND.TKIND_COCLASS => ParseType(new COMTypeLibCoClass(doc, _attr)),
            TYPEKIND.TKIND_UNION => ParseType(new COMTypeLibUnion(doc, _attr)),
            TYPEKIND.TKIND_MODULE => ParseType(new COMTypeLibModule(doc, _attr)),
            _ => GetDefault(),
        };
    }

    internal COMTypeLibTypeInfo GetDefault()
    {
        var doc = GetDocumentation();
        var key = Tuple.Create(doc.Name, _attr.typekind);
        var ret = _context.NamedTypes.GetOrAdd(key, new COMTypeLibTypeInfo(doc, _attr));
        ret.Parse(this);
        return ret;
    }

    public COMTypeFunctionDescriptor GetFuncDesc(int index)
    {
        return _type_info.GetFuncDesc(index);
    }

    public COMTypeVariableDescriptor GetVarDesc(int index)
    {
        return _type_info.GetVarDesc(index);
    }

    public COMTypeLibTypeInfoParser(COMTypeLibParserContext context, COMTypeInfoInstance type_info)
    {
        _context = context;
        _type_info = type_info;
        _attr = type_info.TypeAttr;
    }

    public COMTypeLibTypeInfoParser GetRefTypeInfoOfImplType(int index)
    {
        return GetRefTypeInfo(_type_info.GetRefTypeOfImplType(index));
    }

    public COMTypeLibTypeInfoParser GetRefTypeInfo(int href)
    {
        return new(_context, _type_info.GetRefTypeInfo(href));
    }

    public COMTypeLibInterface ParseRefInterface(int index)
    {
        using var type_info = GetRefTypeInfoOfImplType(index);
        return type_info.ParseInterface();
    }

    public COMTypeLibCoClassInterface ParseCoClassInterface(int index)
    {
        using var type_info = GetRefTypeInfoOfImplType(index);
        COMTypeLibInterfaceBase intf = type_info._attr.typekind == TYPEKIND.TKIND_DISPATCH
            ? type_info.ParseDispatch() : type_info.ParseInterface();
        return new(_type_info.GetImplTypeFlags(index), intf);
    }

    public COMTypeDocumentation GetDocumentation(int index = -1)
    {
        return _type_info.GetDocumentation(index);
    }

    public Tuple<string, string, int> GetDllEntry(int memid, INVOKEKIND kind)
    {
        try
        {
            using var buffer = new SafeHGlobalBuffer(IntPtr.Size * 3);
            _type_info.GetDllEntry(memid, kind, buffer.DangerousGetHandle(),
                buffer.DangerousGetHandle() + IntPtr.Size, buffer.DangerousGetHandle() + IntPtr.Size * 2);
            IntPtr dll_name = buffer.Read<IntPtr>(0);
            IntPtr entry_point = buffer.Read<IntPtr>((ulong)IntPtr.Size);
            int ordinal = buffer.Read<ushort>((ulong)(IntPtr.Size * 2));
            return Tuple.Create(COMTypeLibUtils.ReadBstr(dll_name), COMTypeLibUtils.ReadBstr(entry_point), ordinal);
        }
        catch
        {
            return null;
        }
    }

    public TYPEATTR GetAttr()
    {
        return _attr;
    }

    public COMTypeLibReference GetTypeLibReference()
    {
        using var type_lib = _type_info.GetContainingTypeLib();
        return _context.GetTypeLibReference(type_lib);
    }

    public bool IsDispatch => _attr.typekind == TYPEKIND.TKIND_DISPATCH;

    public ITypeInfo Instance => _type_info.Instance;

    public string[] GetNames(int memid, int max_names)
    {
        return _type_info.GetNames(memid, max_names).ToArray();
    }

    void IDisposable.Dispose()
    {
        _type_info.Dispose();
    }
}