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

namespace OleViewDotNet.Rpc.Clients;
public struct BLOB : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(cbSize);
        m.WriteEmbeddedPointer(pBlobData, (b, l) => m.WriteConformantArray(b, l), cbSize);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        cbSize = u.ReadInt32();
        pBlobData = u.ReadEmbeddedPointer(u.ReadConformantArray<byte>, false);
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int cbSize;
    public NdrEmbeddedPointer<byte[]> pBlobData;
    public static BLOB CreateDefault()
    {
        return new BLOB();
    }
    public BLOB(int cbSize, byte[] pBlobData)
    {
        this.cbSize = cbSize;
        this.pBlobData = pBlobData;
    }
}
