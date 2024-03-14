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
using System;

namespace OleViewDotNet.Rpc.Clients;

internal sealed class IActivationClient : RpcClientBase
{
    public IActivationClient() : 
            base("4d9f4ab8-7d1c-11cf-861e-0020af6e7c57", 0, 0)
    {
    }
    private NdrUnmarshalBuffer SendReceive(int p, NdrMarshalBuffer m)
    {
        var result = SendReceive(p, m.DataRepresentation, m.ToArray(), m.Handles);
        return new(result.NdrBuffer, result.Handles, result.DataRepresentation);
    }
    public uint RemoteActivation(
                ORPCTHIS ORPCthis, 
                out ORPCTHAT ORPCthat,
                Guid Clsid, 
                string pwszObjectName, 
                MInterfacePointer? pObjectStorage, 
                int ClientImpLevel, 
                int Mode, 
                int Interfaces, 
                Guid[] pIIDs, 
                short cRequestedProtseqs, 
                short[] aRequestedProtseqs, 
                out long pOxid, 
                out DUALSTRINGARRAY? ppdsaOxidBindings, 
                out Guid pipidRemUnknown, 
                out int pAuthnHint, 
                out COMVERSION pServerVersion, 
                out int phr, 
                out MInterfacePointer?[] ppInterfaceData, 
                out int[] pResults)
    {
        NdrMarshalBuffer m = new();
        m.WriteStruct(ORPCthis);
        m.WriteGuid(Clsid);
        m.WriteReferent(pwszObjectName, m.WriteTerminatedString);
        m.WriteReferent(pObjectStorage, m.WriteStruct);
        m.WriteInt32(ClientImpLevel);
        m.WriteInt32(Mode);
        m.WriteInt32(Interfaces);
        m.WriteReferent(pIIDs, (g, l) => m.WriteConformantArrayCallback(g, m.WriteGuid, l), Interfaces);
        m.WriteInt16(cRequestedProtseqs);
        m.WriteConformantArray(RpcUtils.CheckNull(aRequestedProtseqs, "aRequestedProtseqs"), cRequestedProtseqs);
        NdrUnmarshalBuffer u = SendReceive(0, m);
        ORPCthat = u.ReadStruct<ORPCTHAT>();
        pOxid = u.ReadInt64();
        ppdsaOxidBindings = u.ReadReferentValue(u.ReadStruct<DUALSTRINGARRAY>, false);
        pipidRemUnknown = u.ReadGuid();
        pAuthnHint = u.ReadInt32();
        pServerVersion = u.ReadStruct<COMVERSION>();
        phr = u.ReadInt32();
        ppInterfaceData = u.ReadConformantStructPointerArray<MInterfacePointer>(false);
        pResults = u.ReadConformantArray<int>();
        return u.ReadUInt32();
    }
}
