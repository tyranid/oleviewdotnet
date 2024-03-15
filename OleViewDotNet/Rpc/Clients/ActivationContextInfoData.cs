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

internal struct ActivationContextInfoData : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(clientOK);
        m.WriteInt32(bReserved1);
        m.WriteInt32(dwReserved1);
        m.WriteInt32(dwReserved2);
        m.WriteEmbeddedPointer(pIFDClientCtx, m.WriteStruct);
        m.WriteEmbeddedPointer(pIFDPrototypeCtx, m.WriteStruct);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        clientOK = u.ReadInt32();
        bReserved1 = u.ReadInt32();
        dwReserved1 = u.ReadInt32();
        dwReserved2 = u.ReadInt32();
        pIFDClientCtx = u.ReadEmbeddedPointer(u.ReadStruct<MInterfacePointer>, false);
        pIFDPrototypeCtx = u.ReadEmbeddedPointer(u.ReadStruct<MInterfacePointer>, false);
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int clientOK;
    public int bReserved1;
    public int dwReserved1;
    public int dwReserved2;
    public NdrEmbeddedPointer<MInterfacePointer> pIFDClientCtx;
    public NdrEmbeddedPointer<MInterfacePointer> pIFDPrototypeCtx;
    public static ActivationContextInfoData CreateDefault()
    {
        return new ActivationContextInfoData();
    }
    public ActivationContextInfoData(int clientOK, int bReserved1, int dwReserved1, int dwReserved2, MInterfacePointer? pIFDClientCtx, MInterfacePointer? pIFDPrototypeCtx)
    {
        this.clientOK = clientOK;
        this.bReserved1 = bReserved1;
        this.dwReserved1 = dwReserved1;
        this.dwReserved2 = dwReserved2;
        this.pIFDClientCtx = pIFDClientCtx;
        this.pIFDPrototypeCtx = pIFDPrototypeCtx;
    }
}
