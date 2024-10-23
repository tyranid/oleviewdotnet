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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace OleViewDotNet.Proxy.Editor;

[DataContract]
public sealed class COMProxyStructureNameData
{
    [DataMember]
    public int Index { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public List<COMProxyStructureMemberNameData> Members { get; set; }

    public COMProxyStructureNameData()
    {
    }

    internal COMProxyStructureNameData(NdrBaseStructureTypeReference type, int index)
    {
        Index = index;
        Name = type.Name;
        Members = type.Members.Select((m, i) => new COMProxyStructureMemberNameData(m, i)).ToList();
    }

    internal void UpdateNames(NdrBaseStructureTypeReference type, ref bool updated)
    {
        if (Name is not null && type.Name != Name)
        {
            type.Name = Name;
            updated = true;
        }

        if (Members is not null)
        {
            var members = type.Members.ToList();
            foreach (var member in Members)
            {
                if (members.Count > member.Index)
                {
                    if (member.Name is not null && members[member.Index].Name != member.Name)
                    {
                        members[member.Index].Name = member.Name;
                        updated = true;
                    }
                }
            }
        }
    }
}

