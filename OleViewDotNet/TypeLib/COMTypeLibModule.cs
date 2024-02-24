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
using OleViewDotNet.Utilities.Format;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

public sealed class COMTypeLibModule : COMTypeLibTypeInfo
{
    public IReadOnlyList<COMTypeLibModuleFunction> Functions { get; private set; }
    public IReadOnlyList<COMTypeLibVariable> Constants { get; private set; }

    internal COMTypeLibModule(COMTypeLibDocumentation doc, TYPEATTR attr)
       : base(doc, attr)
    {
    }

    private protected override void OnParse(COMTypeLibParser.TypeInfo type_info, TYPEATTR attr)
    {
        List<COMTypeLibModuleFunction> functions = new();
        for (int i = 0; i < attr.cFuncs; ++i)
        {
            functions.Add(new(type_info, i));
        }
        Functions = functions.AsReadOnly();
        List<COMTypeLibVariable> constants = new();
        for (int i = 0; i < attr.cVars; ++i)
        {
            constants.Add(new(type_info, i));
        }
        Constants = constants.AsReadOnly();
    }

    internal override void FormatInternal(COMSourceCodeBuilder builder)
    {
        string dll_name = Functions.FirstOrDefault()?.DllName ?? "<no entry points>";
        builder.AppendAttributes(GetTypeAttributes($"dllname(\"{dll_name.EscapeString()}\")"));
        builder.AppendLine($"module {Name} {{");
        using (builder.PushIndent(4))
        {
            foreach (var con in Constants)
            {
                string attrs = con.FormatAttributes();
                if (!string.IsNullOrEmpty(attrs))
                {
                    builder.AppendLine(attrs);
                }
                if (con.ConstValue is string val)
                {
                    val = $"\"{val.EscapeString()}\"";
                }
                else
                {
                    val = con.ConstValue?.ToString() ?? "\"\"";
                }
                builder.AppendLine($"const {con.Type.FormatType()} {con.Name} = {val};");
            }

            foreach (var func in Functions)
            {
                string attrs = func.FormatAttributes(false);
                if (!string.IsNullOrEmpty(attrs))
                {
                    builder.AppendLine(attrs);
                }
                builder.AppendLine(func.FormatMethod());
            }
        }
        builder.AppendLine("};");
    }
}