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

internal struct CustomHeader : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(totalSize);
        m.WriteInt32(headerSize);
        m.WriteInt32(cOpaqueData);
        m.WriteInt32(destCtx);
        m.WriteInt32(cIfs);
        m.WriteGuid(classInfoClsid);
        m.WriteEmbeddedPointer(pclsid, (g, l) => m.WriteConformantArrayCallback(g, m.WriteGuid, l), cIfs);
        m.WriteEmbeddedPointer(pSizes, m.WriteConformantArray, (long)cIfs);
        m.WriteEmbeddedPointer(opaqueData, m.WriteConformantStructArray, (long)cOpaqueData);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        totalSize = u.ReadInt32();
        headerSize = u.ReadInt32();
        cOpaqueData = u.ReadInt32();
        destCtx = u.ReadInt32();
        cIfs = u.ReadInt32();
        classInfoClsid = u.ReadGuid();
        pclsid = u.ReadEmbeddedPointer(() => u.ReadConformantArrayCallback(u.ReadGuid), false);
        pSizes = u.ReadEmbeddedPointer(u.ReadConformantArray<int>, false);
        opaqueData = u.ReadEmbeddedPointer(u.ReadConformantStructArray<CustomOpaqueData>, false);
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int totalSize;
    public int headerSize;
    public int cOpaqueData;
    public int destCtx;
    public int cIfs;
    public Guid classInfoClsid;
    public NdrEmbeddedPointer<Guid[]> pclsid;
    public NdrEmbeddedPointer<int[]> pSizes;
    public NdrEmbeddedPointer<CustomOpaqueData[]> opaqueData;
    public static CustomHeader CreateDefault()
    {
        return new CustomHeader();
    }
    public CustomHeader(int totalSize, int headerSize, int cOpaqueData, int destCtx, int cIfs, 
        Guid classInfoClsid, Guid[] pclsid, int[] pSizes, CustomOpaqueData[] opaqueData)
    {
        this.totalSize = totalSize;
        this.headerSize = headerSize;
        this.cOpaqueData = cOpaqueData;
        this.destCtx = destCtx;
        this.cIfs = cIfs;
        this.classInfoClsid = classInfoClsid;
        this.pclsid = pclsid;
        this.pSizes = pSizes;
        this.opaqueData = opaqueData;
    }
}
