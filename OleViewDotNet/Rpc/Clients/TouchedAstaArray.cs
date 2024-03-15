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

public struct TouchedAstaArray : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(size);
        m.WriteInt32(reserved);
        m.WriteEmbeddedPointer(pAstaOxids, (a, l) => m.WriteConformantArray(a, l), size);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        size = u.ReadInt32();
        reserved = u.ReadInt32();
        pAstaOxids = u.ReadEmbeddedPointer(u.ReadConformantArray<long>, false);
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int size;
    public int reserved;
    public NdrEmbeddedPointer<long[]> pAstaOxids;
    public static TouchedAstaArray CreateDefault()
    {
        TouchedAstaArray ret = new TouchedAstaArray();
        ret.pAstaOxids = new long[0];
        return ret;
    }
    public TouchedAstaArray(int size, int reserved, long[] pAstaOxids)
    {
        this.size = size;
        this.reserved = reserved;
        this.pAstaOxids = pAstaOxids;
    }
}
