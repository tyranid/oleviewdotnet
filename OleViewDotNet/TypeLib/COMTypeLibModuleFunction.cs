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

using OleViewDotNet.Utilities;
using System.Collections.Generic;

namespace OleViewDotNet.TypeLib;

public sealed class COMTypeLibModuleFunction : COMTypeLibMethod
{
    #region Private Members
    private protected override List<string> GetAttributes(bool is_dispatch)
    {
        var attrs = base.GetAttributes(is_dispatch);
        if (!string.IsNullOrEmpty(EntryPoint))
        {
            attrs.Add($"entry(\"{EntryPoint.EscapeString()}\")");
        }
        else if (Ordinal.HasValue)
        {
            attrs.Add($"entry({Ordinal})");
        }
        return attrs;
    }
    #endregion

    #region Public Properties
    public string DllName { get; private set; }
    public int? Ordinal { get; private set; }
    public string EntryPoint { get; private set; }
    #endregion

    #region Internal Members
    internal COMTypeLibModuleFunction(COMTypeLibParser.TypeInfo type_info, int index)
        : base(type_info, index)
    {
        var dll_entry = type_info.GetDllEntry(_desc.memid, _desc.invkind);
        DllName = dll_entry?.Item1 ?? string.Empty;
        EntryPoint = dll_entry?.Item2 ?? string.Empty;
        Ordinal = dll_entry?.Item3;
    }
    #endregion
}
