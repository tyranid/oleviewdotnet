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
using OleViewDotNet.Marshaling;
using System;
using System.IO;

namespace OleViewDotNet.Rpc;


#region Marshal Helpers
internal class _OxidResolver_Marshal_Helper : NdrMarshalBuffer
{
    public void Write_2(short[] p0, long p1)
    {
        WriteConformantArray(p0, p1);
    }
    public void Write_3(short[] p0, long p1)
    {
        WriteConformantArray(p0, p1);
    }
    public void Write_4(long[] p0, long p1)
    {
        WriteConformantArray(p0, p1);
    }
    public void Write_5(long[] p0, long p1)
    {
        WriteConformantArray(p0, p1);
    }
}
internal class _OxidResolver_Unmarshal_Helper : NdrUnmarshalBuffer
{
    public _OxidResolver_Unmarshal_Helper(RpcClientResponse r) : 
            base(r.NdrBuffer, r.Handles, r.DataRepresentation)
    {
    }
    public DUALSTRINGARRAY Read_0()
    {
        return ReadStruct<DUALSTRINGARRAY>();
    }
}
#endregion
#region Complex Types
internal struct DUALSTRINGARRAY : INdrConformantStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        Marshal((_OxidResolver_Marshal_Helper)m);
    }
    private void Marshal(_OxidResolver_Marshal_Helper m)
    {
        m.WriteInt16(wNumEntries);
        m.WriteInt16(wSecurityOffset);
        m.Write_2(RpcUtils.CheckNull(aStringArray, "aStringArray"), wNumEntries);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        Unmarshal((_OxidResolver_Unmarshal_Helper)u);
    }
    private void Unmarshal(_OxidResolver_Unmarshal_Helper u)
    {
        wNumEntries = u.ReadInt16();
        wSecurityOffset = u.ReadInt16();
        aStringArray = u.ReadConformantArray<short>();
    }
    int INdrConformantStructure.GetConformantDimensions()
    {
        return 1;
    }
    int INdrStructure.GetAlignment()
    {
        return 2;
    }
    public short wNumEntries;
    public short wSecurityOffset;
    public short[] aStringArray;

    public COMDualStringArray ToDSA()
    {
        MemoryStream stm = new();
        BinaryWriter writer = new(stm);
        writer.Write(wNumEntries);
        writer.Write(wSecurityOffset);
        foreach (var a in aStringArray)
        {
            writer.Write(a);
        }
        stm.Position = 0;

        return new(new BinaryReader(stm));
    }
}
internal struct COMVERSION : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        Marshal((_OxidResolver_Marshal_Helper)m);
    }
    private void Marshal(_OxidResolver_Marshal_Helper m)
    {
        m.WriteInt16(MajorVersion);
        m.WriteInt16(MinorVersion);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        Unmarshal((_OxidResolver_Unmarshal_Helper)u);
    }
    private void Unmarshal(_OxidResolver_Unmarshal_Helper u)
    {
        MajorVersion = u.ReadInt16();
        MinorVersion = u.ReadInt16();
    }
    int INdrStructure.GetAlignment()
    {
        return 2;
    }
    public short MajorVersion;
    public short MinorVersion;
}
#endregion
#region Client Implementation
internal sealed class OxidResolverClient : RpcClientBase
{
    public OxidResolverClient() : 
            base("99fcfec4-5260-101b-bbcb-00aa0021347a", 0, 0)
    {
    }
    private _OxidResolver_Unmarshal_Helper SendReceive(int p, _OxidResolver_Marshal_Helper m)
    {
        return new _OxidResolver_Unmarshal_Helper(SendReceive(p, m.DataRepresentation, m.ToArray(), m.Handles));
    }
    public uint ResolveOxid(ulong pOxid, short cRequestedProtseqs, short[] arRequestedProtseqs, out DUALSTRINGARRAY? ppdsaOxidBindings, out Guid pipidRemUnknown, out int pAuthnHint)
    {
        _OxidResolver_Marshal_Helper m = new();
        m.WriteUInt64(pOxid);
        m.WriteInt16(cRequestedProtseqs);
        m.Write_3(RpcUtils.CheckNull(arRequestedProtseqs, "arRequestedProtseqs"), cRequestedProtseqs);
        _OxidResolver_Unmarshal_Helper u = SendReceive(0, m);
        ppdsaOxidBindings = u.ReadReferentValue(new Func<DUALSTRINGARRAY>(u.Read_0), false);
        pipidRemUnknown = u.ReadGuid();
        pAuthnHint = u.ReadInt32();
        return u.ReadUInt32();
    }
    public uint SimplePing(long pSetId)
    {
        _OxidResolver_Marshal_Helper m = new();
        m.WriteInt64(pSetId);
        _OxidResolver_Unmarshal_Helper u = SendReceive(1, m);
        return u.ReadUInt32();
    }
    public uint ComplexPing(ref long pSetId, short SequenceNum, short cAddToSet, short cDelFromSet, 
        long[] AddToSet, long[] DelFromSet, out short pPingBackoffFactor)
    {
        _OxidResolver_Marshal_Helper m = new();
        m.WriteInt64(pSetId);
        m.WriteInt16(SequenceNum);
        m.WriteInt16(cAddToSet);
        m.WriteInt16(cDelFromSet);
        m.WriteReferent(AddToSet, new Action<long[], long>(m.Write_4), cAddToSet);
        m.WriteReferent(DelFromSet, new Action<long[], long>(m.Write_5), cDelFromSet);
        _OxidResolver_Unmarshal_Helper u = SendReceive(2, m);
        pSetId = u.ReadInt64();
        pPingBackoffFactor = u.ReadInt16();
        return u.ReadUInt32();
    }
    public uint ServerAlive()
    {
        _OxidResolver_Marshal_Helper m = new();
        _OxidResolver_Unmarshal_Helper u = SendReceive(3, m);
        return u.ReadUInt32();
    }
    public uint ResolveOxid2(ulong pOxid, short cRequestedProtseqs, short[] arRequestedProtseqs, 
        out DUALSTRINGARRAY? ppdsaOxidBindings, out Guid pipidRemUnknown, out int pAuthnHint, out COMVERSION pComVersion)
    {
        _OxidResolver_Marshal_Helper m = new();
        m.WriteUInt64(pOxid);
        m.WriteInt16(cRequestedProtseqs);
        m.Write_3(RpcUtils.CheckNull(arRequestedProtseqs, "arRequestedProtseqs"), cRequestedProtseqs);
        _OxidResolver_Unmarshal_Helper u = SendReceive(4, m);
        ppdsaOxidBindings = u.ReadReferentValue(new Func<DUALSTRINGARRAY>(u.Read_0), false);
        pipidRemUnknown = u.ReadGuid();
        pAuthnHint = u.ReadInt32();
        pComVersion = u.ReadStruct<COMVERSION>();
        return u.ReadUInt32();
    }
    public uint ServerAlive2(out COMVERSION pComVersion, out DUALSTRINGARRAY? ppdsaOrBindings, out int pReserved)
    {
        _OxidResolver_Marshal_Helper m = new();
        _OxidResolver_Unmarshal_Helper u = SendReceive(5, m);
        pComVersion = u.ReadStruct<COMVERSION>();
        ppdsaOrBindings = u.ReadReferentValue(new Func<DUALSTRINGARRAY>(u.Read_0), false);
        pReserved = u.ReadInt32();
        return u.ReadUInt32();
    }
}

#endregion
