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

internal struct PropsOutInfo : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(cIfs);
        m.WriteEmbeddedPointer(piid, (g, l) => m.WriteConformantArrayCallback(g, m.WriteGuid, l), cIfs);
        m.WriteEmbeddedPointer(phresults, m.WriteConformantArray, (long)cIfs);
        m.WriteEmbeddedPointer(ppIntfData, m.WriteConformantStructPointerArray, (long)cIfs);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        cIfs = u.ReadInt32();
        piid = u.ReadEmbeddedPointer(() => u.ReadConformantArrayCallback(u.ReadGuid), false);
        phresults = u.ReadEmbeddedPointer(u.ReadConformantArray<int>, false);
        ppIntfData = u.ReadEmbeddedPointer(() => u.ReadConformantStructPointerArray<MInterfacePointer>(false), false);
    }

    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int cIfs;
    public NdrEmbeddedPointer<Guid[]> piid;
    public NdrEmbeddedPointer<int[]> phresults;
    public NdrEmbeddedPointer<MInterfacePointer?[]> ppIntfData;

    public static PropsOutInfo CreateDefault()
    {
        return new PropsOutInfo();
    }
    public PropsOutInfo(int cIfs, Guid[] piid, int[] phresults, MInterfacePointer?[] ppIntfData)
    {
        this.cIfs = cIfs;
        this.piid = piid;
        this.phresults = phresults;
        this.ppIntfData = ppIntfData;
    }
}
