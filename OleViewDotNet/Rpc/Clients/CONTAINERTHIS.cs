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

public struct CONTAINERTHIS : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(dcomVersionUnused);
        m.WriteInt32(version);
        m.WriteInt64(capabilityFlags);
        m.WriteInt64(requestFlags);
        m.WriteGuid(causalityId);
        m.WriteGuid(unassignedSyncPassthroughGuid);
        m.WriteGuid(unassignedAlwaysPassthroughGuid_1);
        m.WriteGuid(unassignedAlwaysPassthroughGuid_2);
        m.WriteGuid(reservedGuid_1);
        m.WriteGuid(reservedGuid_2);
        m.WriteGuid(reservedGuid_3);
        m.WriteGuid(reservedGuid_4);
        m.WriteInt64(unassignedSyncPassthroughUint64_1);
        m.WriteInt64(unassignedSyncPassthroughUint64_2);
        m.WriteInt64(unassignedAlwaysPassthroughUint64_1);
        m.WriteInt64(unassignedAlwaysPassthroughUint64_2);
        m.WriteInt64(reservedUint64_1);
        m.WriteInt64(reservedUint64_2);
        m.WriteInt64(reservedUint64_3);
        m.WriteInt64(reservedUint64_4);
        m.WriteInt32(reservedUint32);
        m.WriteEmbeddedPointer(MemberE0, m.WriteStruct);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        throw new NotImplementedException();
    }
    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public int dcomVersionUnused;
    public int version;
    public long capabilityFlags;
    public long requestFlags;
    public Guid causalityId;
    public Guid unassignedSyncPassthroughGuid;
    public Guid unassignedAlwaysPassthroughGuid_1;
    public Guid unassignedAlwaysPassthroughGuid_2;
    public Guid reservedGuid_1;
    public Guid reservedGuid_2;
    public Guid reservedGuid_3;
    public Guid reservedGuid_4;
    public long unassignedSyncPassthroughUint64_1;
    public long unassignedSyncPassthroughUint64_2;
    public long unassignedAlwaysPassthroughUint64_1;
    public long unassignedAlwaysPassthroughUint64_2;
    public long reservedUint64_1;
    public long reservedUint64_2;
    public long reservedUint64_3;
    public long reservedUint64_4;
    public int reservedUint32;
    public NdrEmbeddedPointer<CONTAINER_EXTENT_ARRAY> MemberE0;
    public static CONTAINERTHIS CreateDefault()
    {
        return new CONTAINERTHIS();
    }
    public CONTAINERTHIS(
                int Member0,
                int Member4,
                long Member8,
                long Member10,
                Guid Member18,
                Guid Member28,
                Guid Member38,
                Guid Member48,
                Guid Member58,
                Guid Member68,
                Guid Member78,
                Guid Member88,
                long Member98,
                long MemberA0,
                long MemberA8,
                long MemberB0,
                long MemberB8,
                long MemberC0,
                long MemberC8,
                long MemberD0,
                int MemberD8,
                CONTAINER_EXTENT_ARRAY? MemberE0)
    {
        this.dcomVersionUnused = Member0;
        this.version = Member4;
        this.capabilityFlags = Member8;
        this.requestFlags = Member10;
        this.causalityId = Member18;
        this.unassignedSyncPassthroughGuid = Member28;
        this.unassignedAlwaysPassthroughGuid_1 = Member38;
        this.unassignedAlwaysPassthroughGuid_2 = Member48;
        this.reservedGuid_1 = Member58;
        this.reservedGuid_2 = Member68;
        this.reservedGuid_3 = Member78;
        this.reservedGuid_4 = Member88;
        this.unassignedSyncPassthroughUint64_1 = Member98;
        this.unassignedSyncPassthroughUint64_2 = MemberA0;
        this.unassignedAlwaysPassthroughUint64_1 = MemberA8;
        this.unassignedAlwaysPassthroughUint64_2 = MemberB0;
        this.reservedUint64_1 = MemberB8;
        this.reservedUint64_2 = MemberC0;
        this.reservedUint64_3 = MemberC8;
        this.reservedUint64_4 = MemberD0;
        this.reservedUint32 = MemberD8;
        this.MemberE0 = MemberE0;
    }
}
