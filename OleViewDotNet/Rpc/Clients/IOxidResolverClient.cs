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

internal sealed class IOxidResolverClient : RpcClientBase
{
    public IOxidResolverClient() :
            base("99fcfec4-5260-101b-bbcb-00aa0021347a", 0, 0)
    {
    }
    private NdrUnmarshalBuffer SendReceive(int p, NdrMarshalBuffer m)
    {
        var result = SendReceive(p, m.DataRepresentation, m.ToArray(), m.Handles);
        return new NdrUnmarshalBuffer(result.NdrBuffer, result.Handles, result.DataRepresentation);
    }
    public int ResolveOxid(ulong pOxid, short cRequestedProtseqs, short[] arRequestedProtseqs, out DUALSTRINGARRAY? ppdsaOxidBindings, out Guid pipidRemUnknown, out int pAuthnHint)
    {
        NdrMarshalBuffer m = new();
        m.WriteUInt64(pOxid);
        m.WriteInt16(cRequestedProtseqs);
        m.WriteConformantArray(RpcUtils.CheckNull(arRequestedProtseqs, "arRequestedProtseqs"), cRequestedProtseqs);
        NdrUnmarshalBuffer u = SendReceive(0, m);
        ppdsaOxidBindings = u.ReadReferentValue(new Func<DUALSTRINGARRAY>(u.ReadStruct<DUALSTRINGARRAY>), false);
        pipidRemUnknown = u.ReadGuid();
        pAuthnHint = u.ReadInt32();
        return u.ReadInt32();
    }
    public int SimplePing(ulong pSetId)
    {
        NdrMarshalBuffer m = new();
        m.WriteUInt64(pSetId);
        NdrUnmarshalBuffer u = SendReceive(1, m);
        return u.ReadInt32();
    }
    public int ComplexPing(ref ulong pSetId, ushort SequenceNum, ushort cAddToSet, ushort cDelFromSet,
        ulong[] AddToSet, ulong[] DelFromSet, out ushort pPingBackoffFactor)
    {
        NdrMarshalBuffer m = new();
        m.WriteUInt64(pSetId);
        m.WriteUInt16(SequenceNum);
        m.WriteUInt16(cAddToSet);
        m.WriteUInt16(cDelFromSet);
        m.WriteReferent(AddToSet, new Action<ulong[], long>(m.WriteConformantArray), cAddToSet);
        m.WriteReferent(DelFromSet, new Action<ulong[], long>(m.WriteConformantArray), cDelFromSet);
        NdrUnmarshalBuffer u = SendReceive(2, m);
        pSetId = u.ReadUInt64();
        pPingBackoffFactor = u.ReadUInt16();
        return u.ReadInt32();
    }
    public int ServerAlive()
    {
        NdrMarshalBuffer m = new();
        NdrUnmarshalBuffer u = SendReceive(3, m);
        return u.ReadInt32();
    }
    public int ResolveOxid2(ulong pOxid, short cRequestedProtseqs, short[] arRequestedProtseqs,
        out DUALSTRINGARRAY? ppdsaOxidBindings, out Guid pipidRemUnknown, out int pAuthnHint, out COMVERSION pComVersion)
    {
        NdrMarshalBuffer m = new();
        m.WriteUInt64(pOxid);
        m.WriteInt16(cRequestedProtseqs);
        m.WriteConformantArray(RpcUtils.CheckNull(arRequestedProtseqs, "arRequestedProtseqs"), cRequestedProtseqs);
        NdrUnmarshalBuffer u = SendReceive(4, m);
        ppdsaOxidBindings = u.ReadReferentValue(new Func<DUALSTRINGARRAY>(u.ReadStruct<DUALSTRINGARRAY>), false);
        pipidRemUnknown = u.ReadGuid();
        pAuthnHint = u.ReadInt32();
        pComVersion = u.ReadStruct<COMVERSION>();
        return u.ReadInt32();
    }
    public int ServerAlive2(out COMVERSION pComVersion, out DUALSTRINGARRAY? ppdsaOrBindings, out int pReserved)
    {
        NdrMarshalBuffer m = new();
        NdrUnmarshalBuffer u = SendReceive(5, m);
        pComVersion = u.ReadStruct<COMVERSION>();
        ppdsaOrBindings = u.ReadReferentValue(new Func<DUALSTRINGARRAY>(u.ReadStruct<DUALSTRINGARRAY>), false);
        pReserved = u.ReadInt32();
        return u.ReadInt32();
    }
}
