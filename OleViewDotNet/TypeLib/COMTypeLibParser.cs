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
using OleViewDotNet.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

internal sealed class COMTypeLibParser : IDisposable
{
    #region Private Members
    private readonly ITypeLib _type_lib;
    private readonly ConcurrentDictionary<Guid, COMTypeLibInterface> _parsed_intfs = new();
    private readonly ConcurrentDictionary<Guid, COMTypeLibDispatch> _parsed_disp = new();
    private readonly ConcurrentDictionary<Tuple<string, TYPEKIND>, COMTypeLibTypeInfo> _named_types = new();
    private readonly TYPELIBATTR _attr;
    private readonly string _path;
    #endregion

    #region Internal Members
    internal COMTypeLibParser(ITypeLib type_lib)
    {
        _type_lib = type_lib;
        type_lib.GetLibAttr(out IntPtr ptr);
        try
        {
            _attr = ptr.GetStructure<TYPELIBATTR>();
        }
        finally
        {
            type_lib.ReleaseTLibAttr(ptr);
        }
    }

    internal COMTypeLibParser(string path) 
        : this(NativeMethods.LoadTypeLibEx(path, RegKind.RegKind_Default))
    {
        _path = path;
    }

    internal TypeInfo GetTypeInfo(int index)
    {
        _type_lib.GetTypeInfo(index, out ITypeInfo type_info);
        return new TypeInfo(this, type_info);
    }

    internal COMTypeLib Parse()
    {
        List<COMTypeLibTypeInfo> types = new();
        int count = _type_lib.GetTypeInfoCount();
        for (int i = 0; i < count; ++i)
        {
            using var type_info = GetTypeInfo(i);
            types.Add(type_info.Parse());
        }

        return new COMTypeLib(_path, new(_type_lib), _attr, types);
    }

    internal sealed class TypeInfo : IDisposable
    {
        private readonly COMTypeLibParser _type_lib;
        private readonly ITypeInfo _type_info;
        private readonly TYPEATTR _attr;

        internal COMTypeLibInterface ParseInterface()
        {
            var ret = _type_lib._parsed_intfs.GetOrAdd(_attr.guid,
                new COMTypeLibInterface(new(_type_info), _attr));
            ret.Parse(this);
            return ret;
        }

        internal COMTypeLibDispatch ParseDispatch()
        {
            var ret = _type_lib._parsed_disp.GetOrAdd(_attr.guid,
                new COMTypeLibDispatch(new(_type_info), _attr));
            ret.Parse(this);
            return ret;
        }

        internal COMTypeLibTypeInfo ParseType(COMTypeLibTypeInfo type) 
        {
            var key = Tuple.Create(type.Name, type.Kind);
            var ret = _type_lib._named_types.GetOrAdd(key, type);
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
            var ret = _type_lib._named_types.GetOrAdd(key, new COMTypeLibTypeInfo(doc, _attr));
            ret.Parse(this);
            return ret;
        }

        public COMFuncDesc GetFuncDesc(int index)
        {
            _type_info.GetFuncDesc(index, out IntPtr ptr);
            return new COMFuncDesc(_type_info, ptr);
        }

        public COMVarDesc GetVarDesc(int index)
        {
            _type_info.GetVarDesc(index, out IntPtr ptr);
            return new COMVarDesc(_type_info, ptr);
        }

        public TypeInfo(COMTypeLibParser type_lib, ITypeInfo type_info)
        {
            _type_lib = type_lib;
            _type_info = type_info;
            type_info.GetTypeAttr(out IntPtr ptr);
            try
            {
                _attr = ptr.GetStructure<TYPEATTR>();
            }
            finally
            {
                type_info.ReleaseTypeAttr(ptr);
            }
        }

        public TypeInfo GetRefTypeInfoOfImplType(int index)
        {
            _type_info.GetRefTypeOfImplType(index, out int href);
            return GetRefTypeInfo(href);
        }

        public TypeInfo GetRefTypeInfo(int href)
        {
            _type_info.GetRefTypeInfo(href, out ITypeInfo ref_type_info);
            return new(_type_lib, ref_type_info);
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
            _type_info.GetImplTypeFlags(index, out IMPLTYPEFLAGS flags);
            return new(flags, intf);
        }

        public COMTypeLibDocumentation GetDocumentation(int index = -1)
        {
            return new COMTypeLibDocumentation(_type_info, index);
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

        public bool IsDispatch => _attr.typekind == TYPEKIND.TKIND_DISPATCH;
    
        void IDisposable.Dispose()
        {
            _type_info.ReleaseComObject();
        }
    }

    void IDisposable.Dispose()
    {
        _type_lib.ReleaseComObject();
    }
    #endregion
}

