//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2024
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

using NtApiDotNet.Ndr.Marshal;
using System;

namespace OleViewDotNet.Rpc.Clients;

internal struct STDOBJREF : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(flags);
        m.WriteInt32(cPublicRefs);
        m.WriteUInt64(oxid);
        m.WriteUInt64(oid);
        m.WriteGuid(ipid);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        flags = u.ReadInt32();
        cPublicRefs = u.ReadInt32();
        oxid = u.ReadUInt64();
        oid = u.ReadUInt64();
        ipid = u.ReadGuid();
    }

    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public int flags;
    public int cPublicRefs;
    public ulong oxid;
    public ulong oid;
    public Guid ipid;
}
