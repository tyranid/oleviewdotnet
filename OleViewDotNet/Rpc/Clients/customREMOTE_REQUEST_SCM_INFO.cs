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

internal struct customREMOTE_REQUEST_SCM_INFO : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(ClientImpLevel);
        m.WriteInt16(cRequestedProtseqs);
        m.WriteEmbeddedPointer(pRequestedProtseqs, m.WriteConformantArray, (long)cRequestedProtseqs);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        ClientImpLevel = u.ReadInt32();
        cRequestedProtseqs = u.ReadInt16();
        pRequestedProtseqs = u.ReadEmbeddedPointer(u.ReadConformantArray<short>, false);
    }

    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int ClientImpLevel;
    public short cRequestedProtseqs;
    public NdrEmbeddedPointer<short[]> pRequestedProtseqs;
    public static customREMOTE_REQUEST_SCM_INFO CreateDefault()
    {
        return new customREMOTE_REQUEST_SCM_INFO();
    }
    public customREMOTE_REQUEST_SCM_INFO(int ClientImpLevel, short cRequestedProtseqs, short[] pRequestedProtseqs)
    {
        this.ClientImpLevel = ClientImpLevel;
        this.cRequestedProtseqs = cRequestedProtseqs;
        this.pRequestedProtseqs = pRequestedProtseqs;
    }
}
