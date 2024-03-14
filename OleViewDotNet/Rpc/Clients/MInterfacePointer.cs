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

internal struct MInterfacePointer : INdrConformantStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(ulCntData);
        m.WriteConformantArray(RpcUtils.CheckNull(abData, "abData"), ulCntData);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        ulCntData = u.ReadInt32();
        abData = u.ReadConformantArray<byte>();
    }
    int INdrConformantStructure.GetConformantDimensions()
    {
        return 1;
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int ulCntData;
    public byte[] abData;
    public static MInterfacePointer CreateDefault()
    {
        MInterfacePointer ret = new MInterfacePointer();
        ret.abData = new byte[0];
        return ret;
    }
    public MInterfacePointer(int ulCntData, byte[] abData)
    {
        this.ulCntData = ulCntData;
        this.abData = abData;
    }
}
