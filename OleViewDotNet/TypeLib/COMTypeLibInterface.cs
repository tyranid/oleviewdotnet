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
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

public sealed class COMTypeLibInterface : COMTypeLibInterfaceBase
{
    #region Internal Members
    internal COMTypeLibInterface(COMTypeLibDocumentation doc, TYPEATTR attr) 
        : base(doc, attr)
    {
    }

    internal override void FormatInternal(COMSourceCodeBuilder builder)
    {
        bool is_dispatch = HasTypeFlag(TYPEFLAGS.TYPEFLAG_FDISPATCHABLE);
        var base_interface = ImplementedInterfaces.FirstOrDefault();
        builder.AppendAttributes(GetTypeAttributes("odl"));
        int last_offset = base_interface?.Methods.LastOrDefault()?.VTableOffset ?? -1;
        if (base_interface is null)
        {
            builder.AppendLine($"interface {Name} {{");
        }
        else
        {
            builder.AppendLine($"interface {Name} : {base_interface.Name} {{");
        }
        using (builder.PushIndent(4))
        {
            foreach (var method in Methods.SkipWhile(m => m.VTableOffset <= last_offset))
            {
                string attrs = method.FormatAttributes(is_dispatch);
                if (!string.IsNullOrEmpty(attrs))
                {
                    builder.AppendLine(attrs);
                }
                builder.AppendLine(method.FormatMethod());
            }
        }
        builder.AppendLine("};");
    }
    #endregion
}
