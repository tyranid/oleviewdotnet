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

internal struct ORPC_EXTENT_ARRAY : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(size);
        m.WriteInt32(reserved);
        m.WriteEmbeddedPointer(extent, new Action<ORPC_EXTENT?[], long>(m.WriteConformantStructPointerArray),
            RpcUtils.OpBitwiseAnd(RpcUtils.OpPlus(size, 1), -2));
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        size = u.ReadInt32();
        reserved = u.ReadInt32();
        extent = u.ReadEmbeddedPointer(new Func<ORPC_EXTENT?[]>(() => u.ReadConformantStructPointerArray<ORPC_EXTENT>(false)), false);
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int size;
    public int reserved;
    public NdrEmbeddedPointer<ORPC_EXTENT?[]> extent;
}
