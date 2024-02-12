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
    private readonly COMTypeLibInterface _intf;
    private readonly COMTypeLibDocumentation _doc;
    private readonly FUNCDESC _desc;

    private string FormatName()
    {
        return _desc.invkind switch
        {
            INVOKEKIND.INVOKE_FUNC => _doc.Name,
            INVOKEKIND.INVOKE_PROPERTYGET => $"get_{_doc.Name}",
            INVOKEKIND.INVOKE_PROPERTYPUT or INVOKEKIND.INVOKE_PROPERTYPUTREF => $"put_{_doc.Name}",
            _ => null,
        };
    }
    #endregion

    #region Public Properties
    public string Name => FormatName() ?? string.Empty;
    public string DocString => _doc.DocString ?? string.Empty;
    public int HelpContext => _doc.HelpContext;
    public string HelpFile => _doc.HelpFile ?? string.Empty;
    public IReadOnlyList<COMTypeLibParameter> Parameters { get; }
    public COMTypeLibTypeDesc ReturnValue { get; }
    #endregion

    #region Internal Members
    internal COMTypeLibMethod(COMTypeLibInterface intf, COMTypeLibParser.TypeInfo type_info, int index)
    {
        _intf = intf;
        using COMFuncDesc desc = type_info.GetFuncDesc(index);
        _desc = desc.Descriptor;
        _doc = type_info.GetDocumentation(desc.Descriptor.memid);
        string[] names = desc.GetNames();
        Parameters = _desc.lprgelemdescParam.ReadArray<ELEMDESC>(_desc.cParams)
            .Select((d, i) => new COMTypeLibParameter(names[i + 1], d, COMTypeLibTypeDesc.Parse(type_info, d.tdesc), i)).ToList().AsReadOnly();
        ReturnValue = COMTypeLibTypeDesc.Parse(type_info, _desc.elemdescFunc.tdesc);
    }
    #endregion
}
