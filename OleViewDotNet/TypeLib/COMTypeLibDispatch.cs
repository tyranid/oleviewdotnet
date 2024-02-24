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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

public sealed class COMTypeLibDispatch : COMTypeLibInterfaceBase
{
    #region Private Members
    private protected override void OnParse(COMTypeLibParser.TypeInfo type_info, TYPEATTR attr)
    {
        base.OnParse(type_info, attr);
        if (attr.wTypeFlags.HasFlag(TYPEFLAGS.TYPEFLAG_FDUAL))
        {
            DualInterface = type_info.ParseRefInterface(-1);
        }
        List<COMTypeLibVariable> props = new();
        for (int i = 0; i < attr.cVars; ++i)
        {
            props.Add(new COMTypeLibVariable(type_info, i));
        }
        Properties = props.AsReadOnly();
    }
    #endregion

    #region Public Properties
    public COMTypeLibInterface DualInterface { get; private set; }
    public IReadOnlyList<COMTypeLibVariable> Properties { get; private set; }
    #endregion

    #region Internal Members
    internal COMTypeLibDispatch(COMTypeLibDocumentation doc, TYPEATTR attr)
        : base(doc, attr)
    {
    }

    internal override void FormatInternal(COMSourceCodeBuilder builder)
    {
        builder.AppendAttributes(GetTypeAttributes("odl"));
        builder.AppendLine($"dispinterface {Name} {{");
        using (builder.PushIndent(4))
        {
            builder.AppendLine("properties:");
            using (builder.PushIndent(4))
            {
                foreach (var prop in Properties)
                {
                    string attrs = prop.FormatAttributes();
                    if (!string.IsNullOrEmpty(attrs))
                    {
                        builder.AppendLine(attrs);
                    }
                    builder.AppendLine($"{prop.Type.FormatType()} {prop.Name}{prop.Type.FormatPostName()};");
                }
            }
            builder.AppendLine("methods:");
            using (builder.PushIndent(4))
            {
                foreach (var method in Methods.Skip(HasTypeFlag(TYPEFLAGS.TYPEFLAG_FDUAL) ? 7 : 0))
                {
                    string attrs = method.FormatAttributes(true);
                    if (!string.IsNullOrEmpty(attrs))
                    {
                        builder.AppendLine(attrs);
                    }
                    builder.AppendLine(method.FormatMethod());
                }
            }
        }
        builder.AppendLine("};");
    }
    #endregion
}
