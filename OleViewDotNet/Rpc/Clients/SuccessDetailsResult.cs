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

public struct SuccessDetailsResult : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(sizeOfMarshaledResults);
        m.WriteInt32(reserved);
        m.WriteEmbeddedPointer(pMarshaledResults, (b, l) => m.WriteConformantArray(b, l), RpcUtils.OpBitwiseAnd(RpcUtils.OpPlus(sizeOfMarshaledResults, 7), -8));
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        sizeOfMarshaledResults = u.ReadInt32();
        reserved = u.ReadInt32();
        pMarshaledResults = u.ReadEmbeddedPointer(u.ReadConformantArray<byte>, false);
    }

    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int sizeOfMarshaledResults;
    public int reserved;
    public NdrEmbeddedPointer<byte[]> pMarshaledResults;
    public static SuccessDetailsResult CreateDefault()
    {
        SuccessDetailsResult ret = new SuccessDetailsResult();
        ret.pMarshaledResults = new byte[0];
        return ret;
    }
}
