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
using System;

namespace OleViewDotNet.Rpc.Clients;

internal struct ORPC_EXTENT : INdrConformantStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteGuid(id);
        m.WriteInt32(size);
        m.WriteConformantArray(RpcUtils.CheckNull(data, "data"), RpcUtils.OpBitwiseAnd(RpcUtils.OpPlus(size, 7), -8));
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        id = u.ReadGuid();
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
    public Guid id;
    public int size;
    public byte[] data;
}
