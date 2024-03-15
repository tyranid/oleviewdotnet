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

internal struct CustomOpaqueData : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteGuid(guid);
        m.WriteInt32(dataLength);
        m.WriteInt32(reserved1);
        m.WriteInt32(reserved2);
        m.WriteEmbeddedPointer(data, m.WriteConformantArray, (long)dataLength);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        guid = u.ReadGuid();
        dataLength = u.ReadInt32();
        reserved1 = u.ReadInt32();
        reserved2 = u.ReadInt32();
        data = u.ReadEmbeddedPointer(u.ReadConformantArray<byte>, false);
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public Guid guid;
    public int dataLength;
    public int reserved1;
    public int reserved2;
    public NdrEmbeddedPointer<byte[]> data;
    public static CustomOpaqueData CreateDefault()
    {
        return new CustomOpaqueData();
    }
    public CustomOpaqueData(Guid guid, int dataLength, int reserved1, int reserved2, byte[] data)
    {
        this.guid = guid;
        this.dataLength = dataLength;
        this.reserved1 = reserved1;
        this.reserved2 = reserved2;
        this.data = data;
    }
}
