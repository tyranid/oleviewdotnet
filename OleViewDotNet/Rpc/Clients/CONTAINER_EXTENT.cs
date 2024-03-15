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
using NtApiDotNet.Win32.Rpc;

namespace OleViewDotNet.Rpc.Clients;

public struct CONTAINER_EXTENT : INdrConformantStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(id);
        m.WriteInt32(version);
        m.WriteInt32(size);
        m.WriteConformantArray(RpcUtils.CheckNull(data, "MemberC"), RpcUtils.OpBitwiseAnd(RpcUtils.OpPlus(size, 7), -8));
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        id = u.ReadInt32();
        version = u.ReadInt32();
        size = u.ReadInt32();
        data = u.ReadConformantArray<byte>();
    }

    int INdrConformantStructure.GetConformantDimensions()
    {
        return 1;
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int id;
    public int version;
    public int size;
    public byte[] data;
    public static CONTAINER_EXTENT CreateDefault()
    {
        CONTAINER_EXTENT ret = new CONTAINER_EXTENT();
        ret.data = new byte[0];
        return ret;
    }
}
