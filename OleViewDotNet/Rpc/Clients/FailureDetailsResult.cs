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

public struct FailureDetailsResult : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(pointOfFailure);
        m.WriteInt32(hrFailure);
        m.WriteInt32(sizeOfMarshaledErrorInfo);
        m.WriteInt32(reserved);
        m.WriteEmbeddedPointer(pMarshaledErrorInfo, (b, l) => m.WriteConformantArray(b, l), RpcUtils.OpBitwiseAnd(RpcUtils.OpPlus(sizeOfMarshaledErrorInfo, 7), -8));
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        pointOfFailure = u.ReadInt32();
        hrFailure = u.ReadInt32();
        sizeOfMarshaledErrorInfo = u.ReadInt32();
        reserved = u.ReadInt32();
        pMarshaledErrorInfo = u.ReadEmbeddedPointer(u.ReadConformantArray<byte>, false);
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int pointOfFailure;
    public int hrFailure;
    public int sizeOfMarshaledErrorInfo;
    public int reserved;
    public NdrEmbeddedPointer<byte[]> pMarshaledErrorInfo;
    public static FailureDetailsResult CreateDefault()
    {
        return new FailureDetailsResult();
    }
    public FailureDetailsResult(int Member0, int Member4, int Member8, int MemberC, byte[] Member10)
    {
        this.pointOfFailure = Member0;
        this.hrFailure = Member4;
        this.sizeOfMarshaledErrorInfo = Member8;
        this.reserved = MemberC;
        this.pMarshaledErrorInfo = Member10;
    }
}
