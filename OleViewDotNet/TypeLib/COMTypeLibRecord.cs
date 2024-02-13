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
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

public sealed class COMTypeLibRecord : COMTypeLibTypeInfo
{
    internal COMTypeLibRecord(COMTypeLibDocumentation doc, TYPEATTR attr)
       : base(doc, attr)
    {
    }

    private protected override void OnParse(COMTypeLibParser.TypeInfo type_info, TYPEATTR attr)
    {
        List<COMTypeLibVariable> fields = new();
        for (int i = 0; i < attr.cVars; ++i)
        {
            fields.Add(new COMTypeLibVariable(type_info, i));
        }
        Fields = fields.AsReadOnly();
    }

    public IReadOnlyList<COMTypeLibVariable> Fields { get; private set; }

    internal override void Format(SourceCodeBuilder builder)
    {
        builder.AppendLine($"typedef {GetTypeAttributes().FormatAttrs()} {{");
        using (builder.PushIndent(4))
        {
            foreach (var v in Fields)
            {
                builder.AppendLine($"{v.Type.FormatType()} {v.Name}{v.Type.FormatPostName()};");
            }
        }
        builder.AppendLine($"}} {Name};");
    }
}