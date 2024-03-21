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
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

public class COMTypeLibParameter
{
    private readonly ELEMDESC _desc;

    private bool HasFlag(PARAMFLAG flag)
    {
        return _desc.desc.paramdesc.wParamFlags.HasFlag(flag);
    }

    public string Name { get; }
    public COMTypeLibTypeDesc Type { get; }
    public bool IsOptional => HasFlag(PARAMFLAG.PARAMFLAG_FOPT);
    public bool IsIn => HasFlag(PARAMFLAG.PARAMFLAG_FIN);
    public bool IsOut => HasFlag(PARAMFLAG.PARAMFLAG_FOUT);
    public bool IsRetVal => HasFlag(PARAMFLAG.PARAMFLAG_FRETVAL);
    public bool IsLcid => HasFlag(PARAMFLAG.PARAMFLAG_FLCID);
    public bool HasDefault => HasFlag(PARAMFLAG.PARAMFLAG_FHASDEFAULT);
    public object DefaultValue { get; }

    internal string FormatParameter()
    {
        List<string> attrs = new();
        if (IsIn)
            attrs.Add("in");
        if (IsOut)
            attrs.Add("out");
        if (IsRetVal)
            attrs.Add("retval");
        if (IsOptional)
            attrs.Add("optional");
        if (IsLcid)
            attrs.Add("lcid");
        if (HasDefault && DefaultValue is not null)
        {
            if (DefaultValue is string s)
            {
                attrs.Add($"defaultvalue(\"{s.Replace("\"", "\\\"")}\")");
            }
            else
            {
                attrs.Add($"defaultvalue({DefaultValue})");
            }
        }

        return $"{attrs.FormatAttrs()}{Type.FormatType()} {Name}{Type.FormatPostName()}";
    }

    internal COMTypeLibParameter(string name, ELEMDESC desc, COMTypeLibTypeDesc type, int index)
    {
        Name = name ?? $"p{index}";
        _desc = desc;
        Type = type;
        if (HasDefault)
        {
            DefaultValue = COMTypeLibUtils.ReadDefaultValue(_desc.desc.paramdesc.lpVarValue);
        }
    }
}
