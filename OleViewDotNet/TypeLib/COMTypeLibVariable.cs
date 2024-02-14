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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

public sealed class COMTypeLibVariable
{
    #region Private Members
    private readonly COMTypeLibDocumentation _doc;
    private readonly VARDESC _desc;
    private readonly VARFLAGS _flags;

    private List<string> GetAttributes()
    {
        List<string> attrs = new();
        if (_desc.varkind == VARKIND.VAR_DISPATCH)
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

        if (_flags.HasFlag(VARFLAGS.VARFLAG_FBINDABLE))
            attrs.Add("bindable");
        if (_flags.HasFlag(VARFLAGS.VARFLAG_FDEFAULTBIND))
            attrs.Add("defaultbind");
        if (_flags.HasFlag(VARFLAGS.VARFLAG_FDEFAULTCOLLELEM))
            attrs.Add("defaultcollelem");
        if (_flags.HasFlag(VARFLAGS.VARFLAG_FDISPLAYBIND))
            attrs.Add("displaybind");
        if (_flags.HasFlag(VARFLAGS.VARFLAG_FHIDDEN))
            attrs.Add("hidden");
        if (_flags.HasFlag(VARFLAGS.VARFLAG_FIMMEDIATEBIND))
            attrs.Add("immediatebind");
        if (_flags.HasFlag(VARFLAGS.VARFLAG_FNONBROWSABLE))
            attrs.Add("nonbrowsable");
        if (_flags.HasFlag(VARFLAGS.VARFLAG_FREQUESTEDIT))
            attrs.Add("requestedit");
        if (_flags.HasFlag(VARFLAGS.VARFLAG_FRESTRICTED))
            attrs.Add("restricted");
        if (_flags.HasFlag(VARFLAGS.VARFLAG_FSOURCE))
            attrs.Add("source");
        if (_flags.HasFlag(VARFLAGS.VARFLAG_FUIDEFAULT))
            attrs.Add("uidefault");
        if (_flags.HasFlag(VARFLAGS.VARFLAG_FREPLACEABLE))
            attrs.Add("replaceable");
        if (_flags.HasFlag(VARFLAGS.VARFLAG_FREADONLY))
            attrs.Add("readonly");
        return attrs;
    }

    #endregion

    #region Public Properties
    public string Name => _doc.Name ?? string.Empty;
    public string DocString => _doc.DocString ?? string.Empty;
    public int HelpContext => _doc.HelpContext;
    public string HelpFile => _doc.HelpFile ?? string.Empty;
    public object ConstValue { get; }
    public COMTypeLibTypeDesc Type { get; }
    #endregion

    #region Internal Members
    internal COMTypeLibVariable(COMTypeLibParser.TypeInfo type_info, int index)
    {
        using COMVarDesc desc = type_info.GetVarDesc(index);
        _desc = desc.Descriptor;
        _doc = type_info.GetDocumentation(_desc.memid);
        if (_desc.varkind == VARKIND.VAR_CONST)
        {
            ConstValue = COMTypeLibUtils.GetVariant(_desc.desc.lpvarValue);
        }
        Type = COMTypeLibTypeDesc.Parse(type_info, _desc.elemdescVar.tdesc);
        _flags = (VARFLAGS)_desc.wVarFlags;
    }

    internal string FormatAttributes()
    {
        return GetAttributes().FormatAttrs().TrimEnd();
    }
    #endregion
}
