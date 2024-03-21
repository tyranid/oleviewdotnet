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
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

public sealed class COMTypeLibCoClass : COMTypeLibTypeInfo
{
    internal COMTypeLibCoClass(COMTypeLibDocumentation doc, TYPEATTR attr)
       : base(doc, attr)
    {
    }

    private protected override void OnParse(COMTypeLibParser.TypeInfo type_info, TYPEATTR attr)
    {
        List<COMTypeLibCoClassInterface> impl_intfs = new();
        for (int i = 0; i < attr.cImplTypes; ++i)
        {
            impl_intfs.Add(type_info.ParseCoClassInterface(i));
        }
        ImplementedInterfaces = impl_intfs.AsReadOnly();
    }

    public IReadOnlyList<COMTypeLibCoClassInterface> ImplementedInterfaces { get; private set; }

    internal override void FormatInternal(COMSourceCodeBuilder builder)
    {
        builder.AppendAttributes(GetTypeAttributes("odl"));
        builder.AppendLine($"coclass {Name} {{");
        using (builder.PushIndent(4))
        {
            foreach (var intf in ImplementedInterfaces)
            {
                List<string> attrs = new();
                if (intf.Flags.HasFlag(IMPLTYPEFLAGS.IMPLTYPEFLAG_FDEFAULT))
                    attrs.Add("default");
                if (intf.Flags.HasFlag(IMPLTYPEFLAGS.IMPLTYPEFLAG_FDEFAULTVTABLE))
                    attrs.Add("defaultvtable");
                if (intf.Flags.HasFlag(IMPLTYPEFLAGS.IMPLTYPEFLAG_FRESTRICTED))
                    attrs.Add("restricted");
                if (intf.Flags.HasFlag(IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE))
                    attrs.Add("source");
                if (intf.Interface is COMTypeLibDispatch disp && disp.DualInterface is null)
                {
                    builder.AppendLine($"{attrs.FormatAttrs()}dispinterface {intf.Interface.Name};");
                }
                else
                {
                    builder.AppendLine($"{attrs.FormatAttrs()}interface {intf.Interface.Name};");
                }
            }
        }
        builder.AppendLine("};");
    }
}