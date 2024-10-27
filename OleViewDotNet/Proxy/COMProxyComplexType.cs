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
    #region Private Members
    private readonly COMProxyInterface m_intf;
    #endregion

    #region Public Properties
    public override string Name { get => Entry.Name; set => Entry.Name = m_intf?.CheckName(Entry.Name, value) ?? value ?? Entry.Name; }

    public NdrComplexTypeReference Entry { get; }

    public int Size => Entry.GetSize();

    public bool IsUnion => Entry is NdrUnionTypeReference;

    public IReadOnlyList<COMProxyComplexTypeMember> Members { get; }

    bool ICOMSourceCodeFormattable.IsFormattable => true;

    bool ICOMSourceCodeEditable.IsEditable => true;

    IReadOnlyList<ICOMSourceCodeEditable> ICOMSourceCodeEditable.Members => Members;
    #endregion

    #region Internal Members
    internal COMProxyComplexType(NdrComplexTypeReference entry, COMProxyInterface intf = null)
    {
        Entry = entry;
        if (Entry is NdrUnionTypeReference union)
        {
            Members = union.Arms.Arms.Select(a => new COMProxyComplexTypeUnionArm(a, m_intf)).ToList().AsReadOnly();
        }
        else if (Entry is NdrBaseStructureTypeReference st)
        {
            Members = st.Members.Select(m => new COMProxyComplexTypeStructMember(m, m_intf)).ToList().AsReadOnly();
        }
        m_intf = intf;
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
