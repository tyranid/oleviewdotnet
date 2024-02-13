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

using OleViewDotNet.Proxy;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace OleViewDotNet.TypeLib;

public sealed class COMTypeLibDispatch : COMTypeLibInterfaceBase, IProxyFormatter
{
    #region Private Members
    private protected override void OnParse(COMTypeLibParser.TypeInfo type_info, TYPEATTR attr)
    {
        base.OnParse(type_info, attr);
        if (attr.wTypeFlags.HasFlag(TYPEFLAGS.TYPEFLAG_FDUAL))
        {
            DualInterface = type_info.ParseRefInterface(-1);
        }
    }
    #endregion

    #region Public Properties
    public COMTypeLibInterface DualInterface { get; private set; }
    #endregion

    #region Internal Members
    internal COMTypeLibDispatch(COMTypeLibDocumentation doc, TYPEATTR attr)
        : base(doc, attr)
    {
    }
    #endregion

    #region IProxyFormatter Implementation
    public string FormatText(ProxyFormatterFlags flags = ProxyFormatterFlags.None)
    {
        StringBuilder builder = new();
        builder.AppendLine(GetTypeAttributes(true).FormatAttrs());
        builder.AppendLine($"dispinterface {Name} {{");
        builder.AppendLine("    properties:");
        builder.AppendLine("    methods:");
        foreach (var method in Methods)
        {
            string attrs = method.FormatAttributes(true);
            if (!string.IsNullOrEmpty(attrs))
            {
                builder.Append("    ").AppendLine(attrs);
            }
            builder.Append("    ").AppendLine(method.FormatMethod());
        }
        builder.AppendLine("};");

        return builder.ToString();
    }
    #endregion
}
