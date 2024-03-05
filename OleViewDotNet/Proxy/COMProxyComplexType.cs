//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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

using NtApiDotNet.Ndr;
using OleViewDotNet.Utilities.Format;
using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet.Proxy;

public sealed class COMProxyComplexType : COMProxyTypeInfo, ICOMSourceCodeFormattable, ICOMSourceCodeEditable
{
    #region Public Properties
    public override string Name => Entry.Name;

    public NdrComplexTypeReference Entry { get; }

    public int Size => Entry.GetSize();

    public bool IsUnion => Entry is NdrUnionTypeReference;

    bool ICOMSourceCodeFormattable.IsFormattable => true;

    string ICOMSourceCodeEditable.Name { get => Entry.Name; set => Entry.Name = value; }

    IReadOnlyList<ICOMSourceCodeEditable> ICOMSourceCodeEditable.Members
    {
        get
        {
            List<ICOMSourceCodeEditable> ret = new();
            if (Entry is NdrBaseStructureTypeReference struct_type)
            {
                ret.AddRange(struct_type.Members.Select(m => new COMSourceCodeEditableObject(() => m.Name, n => m.Name = n)));
            }
            else if (Entry is NdrUnionTypeReference union_type)
            {
                ret.AddRange(union_type.Arms.Arms.Select(m => new COMSourceCodeEditableObject(() => m.Name, n => m.Name = n)));
            }
            return ret.AsReadOnly();
        }
    }
    #endregion

    #region Internal Members
    internal COMProxyComplexType(NdrComplexTypeReference entry)
    {
        Entry = entry;
    }
    #endregion

    #region Public Methods
    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        INdrFormatter formatter = builder.GetNdrFormatter();
        builder.AppendLine(formatter.FormatComplexType(Entry).TrimEnd());
        builder.AppendLine();
    }
    #endregion
}
