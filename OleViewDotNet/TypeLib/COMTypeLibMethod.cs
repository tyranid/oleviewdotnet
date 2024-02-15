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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

public class COMTypeLibMethod
{
    #region Private Members
    private readonly COMTypeLibDocumentation _doc;
    private readonly FUNCFLAGS _flags;
    private protected readonly FUNCDESC _desc;
    #endregion

    #region Public Properties
    public string Name => _doc.Name ?? string.Empty;
    public string DocString => _doc.DocString ?? string.Empty;
    public int HelpContext => _doc.HelpContext;
    public string HelpFile => _doc.HelpFile ?? string.Empty;
    public IReadOnlyList<COMTypeLibParameter> Parameters { get; }
    public COMTypeLibTypeDesc ReturnValue { get; }
    public int VTableOffset => _desc.oVft;
    #endregion

    #region Internal Members
    internal COMTypeLibMethod(COMTypeLibParser.TypeInfo type_info, int index)
    {
        using COMFuncDesc desc = type_info.GetFuncDesc(index);
        _desc = desc.Descriptor;
        _doc = type_info.GetDocumentation(desc.Descriptor.memid);
        string[] names = desc.GetNames();
        Parameters = _desc.lprgelemdescParam.ReadArray<ELEMDESC>(_desc.cParams)
            .Select((d, i) => new COMTypeLibParameter(names[i + 1], d, COMTypeLibTypeDesc.Parse(type_info, d.tdesc), i)).ToList().AsReadOnly();
        ReturnValue = COMTypeLibTypeDesc.Parse(type_info, _desc.elemdescFunc.tdesc);
        _flags = (FUNCFLAGS)_desc.wFuncFlags;
    }

    private protected virtual List<string> GetAttributes(bool is_dispatch)
    {
        List<string> attrs = new();
        if (is_dispatch)
        {
            uint id = (uint)_desc.memid;
            if (id < 256)
            {
                attrs.Add($"id({id})");
            }
            else
            {
                attrs.Add($"id(0x{id:X08})");
            }
        }

        switch (_desc.invkind)
        {
            case INVOKEKIND.INVOKE_PROPERTYGET:
                attrs.Add("propget");
                break;
            case INVOKEKIND.INVOKE_PROPERTYPUT:
            case INVOKEKIND.INVOKE_PROPERTYPUTREF:
                attrs.Add("propput");
                break;
        }

        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FBINDABLE))
            attrs.Add("bindable");
        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FDEFAULTBIND))
            attrs.Add("defaultbind");
        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FDEFAULTCOLLELEM))
            attrs.Add("defaultcollelem");
        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FDISPLAYBIND))
            attrs.Add("displaybind");
        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FHIDDEN))
            attrs.Add("hidden");
        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FIMMEDIATEBIND))
            attrs.Add("immediatebind");
        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FNONBROWSABLE))
            attrs.Add("nonbrowsable");
        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FREQUESTEDIT))
            attrs.Add("requestedit");
        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FRESTRICTED))
            attrs.Add("restricted");
        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FSOURCE))
            attrs.Add("source");
        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FUIDEFAULT))
            attrs.Add("uidefault");
        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FUSESGETLASTERROR))
            attrs.Add("usesgetlasterror");
        if (_flags.HasFlag(FUNCFLAGS.FUNCFLAG_FREPLACEABLE))
            attrs.Add("replaceable");

        return attrs;
    }

    internal string FormatAttributes(bool is_dispatch)
    {
        return GetAttributes(is_dispatch).FormatAttrs().TrimEnd();
    }

    internal string FormatMethod()
    {
        List<string> ps = new();
        foreach (var p in Parameters)
        {
            ps.Add(p.FormatParameter());
        }

        return $"{ReturnValue.FormatType()} {Name}({string.Join(", ", ps)});";
    }
    #endregion
}
