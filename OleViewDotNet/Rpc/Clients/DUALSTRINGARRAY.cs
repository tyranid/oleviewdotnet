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
using OleViewDotNet.Marshaling;
using System.IO;

namespace OleViewDotNet.Rpc.Clients;

internal struct DUALSTRINGARRAY : INdrConformantStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt16(wNumEntries);
        m.WriteInt16(wSecurityOffset);
        m.WriteConformantArray(RpcUtils.CheckNull(aStringArray, "aStringArray"), wNumEntries);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        wNumEntries = u.ReadInt16();
        wSecurityOffset = u.ReadInt16();
        aStringArray = u.ReadConformantArray<short>();
    }

    int INdrConformantStructure.GetConformantDimensions()
    {
        return 1;
    }
    int INdrStructure.GetAlignment()
    {
        return 2;
    }
    public short wNumEntries;
    public short wSecurityOffset;
    public short[] aStringArray;

    internal COMDualStringArray ToDSA()
    {
        MemoryStream stm = new();
        BinaryWriter writer = new(stm);
        writer.Write(wNumEntries);
        writer.Write(wSecurityOffset);
        foreach (var a in aStringArray)
        {
            writer.Write(a);
        }
        stm.Position = 0;

        return new(new BinaryReader(stm));
    }
}

