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

public struct CONTAINERTHAT : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        responseFlags = u.ReadInt64();
        unassignedPassthroughGuid_1 = u.ReadGuid();
        unassignedPassthroughGuid_2 = u.ReadGuid();
        unassignedPassthroughGuid_3 = u.ReadGuid();
        unassignedPassthroughGuid_4 = u.ReadGuid();
        reservedGuid_1 = u.ReadGuid();
        reservedGuid_2 = u.ReadGuid();
        reservedGuid_3 = u.ReadGuid();
        reservedGuid_4 = u.ReadGuid();
        unassignedPassthroughUint64_1 = u.ReadInt64();
        unassignedPassthroughUint64_2 = u.ReadInt64();
        unassignedPassthroughUint64_3 = u.ReadInt64();
        unassignedPassthroughUint64_4 = u.ReadInt64();
        reservedUint64_1 = u.ReadInt64();
        reservedUint64_2 = u.ReadInt64();
        reservedUint64_3 = u.ReadInt64();
        reservedUint64_4 = u.ReadInt64();
        reservedUint32 = u.ReadInt32();
        extensions = u.ReadEmbeddedPointer(u.ReadStruct<CONTAINER_EXTENT_ARRAY>, false);
    }
    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public long responseFlags;
    public Guid unassignedPassthroughGuid_1;
    public Guid unassignedPassthroughGuid_2;
    public Guid unassignedPassthroughGuid_3;
    public Guid unassignedPassthroughGuid_4;
    public Guid reservedGuid_1;
    public Guid reservedGuid_2;
    public Guid reservedGuid_3;
    public Guid reservedGuid_4;
    public long unassignedPassthroughUint64_1;
    public long unassignedPassthroughUint64_2;
    public long unassignedPassthroughUint64_3;
    public long unassignedPassthroughUint64_4;
    public long reservedUint64_1;
    public long reservedUint64_2;
    public long reservedUint64_3;
    public long reservedUint64_4;
    public int reservedUint32;
    public NdrEmbeddedPointer<CONTAINER_EXTENT_ARRAY> extensions;
}