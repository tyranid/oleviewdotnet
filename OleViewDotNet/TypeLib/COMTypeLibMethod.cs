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

public sealed class COMTypeLibMethod
{
    #region Private Members
    private readonly COMTypeLibDocumentation _doc;
    private readonly FUNCDESC _desc;
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
    internal string FormatName()
    {
        return _desc.invkind switch
        {
            INVOKEKIND.INVOKE_FUNC => _doc.Name,
            INVOKEKIND.INVOKE_PROPERTYGET => $"get_{_doc.Name}",
            INVOKEKIND.INVOKE_PROPERTYPUT or INVOKEKIND.INVOKE_PROPERTYPUTREF => $"put_{_doc.Name}",
            _ => null,
        };
    }

    internal COMTypeLibMethod(COMTypeLibParser.TypeInfo type_info, int index)
    {
        using COMFuncDesc desc = type_info.GetFuncDesc(index);
        _desc = desc.Descriptor;
        _doc = type_info.GetDocumentation(desc.Descriptor.memid);
        string[] names = desc.GetNames();
        Parameters = _desc.lprgelemdescParam.ReadArray<ELEMDESC>(_desc.cParams)
            .Select((d, i) => new COMTypeLibParameter(names[i + 1], d, COMTypeLibTypeDesc.Parse(type_info, d.tdesc), i)).ToList().AsReadOnly();
        ReturnValue = COMTypeLibTypeDesc.Parse(type_info, _desc.elemdescFunc.tdesc);
    }

    private List<string> GetAttributes(bool is_dispatch)
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

        return attrs;
    }

    internal string FormatAttributes(bool is_dispatch)
    {
        return GetAttributes(is_dispatch).FormatAttrs();
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
