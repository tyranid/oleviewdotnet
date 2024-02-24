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

using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

public sealed class COMTypeLibEnum : COMTypeLibTypeInfo
{
    internal COMTypeLibEnum(COMTypeLibDocumentation doc, TYPEATTR attr)
       : base(doc, attr)
    {
    }

    private protected override void OnParse(COMTypeLibParser.TypeInfo type_info, TYPEATTR attr)
    {
        List<COMTypeLibEnumValue> values = new();
        for (int i = 0; i < attr.cVars; ++i)
        {
            var v = new COMTypeLibVariable(type_info, i);
            long l = 0;
            try
            {
                if (v.ConstValue is IConvertible conv)
                {
                    l = conv.ToInt64(null);
                }
            }
            catch
            {
            }

            values.Add(new(v.Name, l));
        }
        Values = values.AsReadOnly();
    }

    public IReadOnlyList<COMTypeLibEnumValue> Values { get; private set; }

    internal override void FormatInternal(COMSourceCodeBuilder builder)
    {
        builder.AppendLine($"typedef {GetTypeAttributes().FormatAttrs().TrimEnd()}");
        builder.AppendLine("enum {");
        using (builder.PushIndent(4))
        {
            builder.AppendList(Values.Select(v => $"{v.Name} = {v.Value}"));
        }
        builder.AppendLine($"}} {Name};");
    }
}
