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
using OleViewDotNet.Rpc.Clients;
using System;

namespace OleViewDotNet.Rpc.Transport;

// The LocalThis and LocalThat structures can be different on different versions of Windows.
#region Marshal Helpers
internal class _ThisAndThat_Marshal_Helper : NdrMarshalBuffer
{
    public void Write_2(ORPC_EXTENT_ARRAY p0)
    {
        WriteStruct(p0);
    }
    public void Write_5(LocalThisAsyncRequestBlock p0)
    {
        WriteStruct(p0);
    }
    public void Write_8(ORPC_EXTENT?[] p0, long p1)
    {
        WriteConformantStructPointerArray(p0, p1);
    }
    public void Write_9(byte[] p0, long p1)
    {
        WriteConformantArray(p0, p1);
    }
}
internal class _ThisAndThat_Unmarshal_Helper : NdrUnmarshalBuffer
{
    public _ThisAndThat_Unmarshal_Helper(NtApiDotNet.Win32.Rpc.RpcClientResponse r) : 
            base(r.NdrBuffer, r.Handles, r.DataRepresentation)
    {
    }
    public _ThisAndThat_Unmarshal_Helper(byte[] ba) : 
            base(ba)
    {
    }
    public ORPC_EXTENT_ARRAY Read_2()
    {
        return ReadStruct<ORPC_EXTENT_ARRAY>();
    }
    public LocalThisAsyncRequestBlock Read_5()
    {
        return ReadStruct<LocalThisAsyncRequestBlock>();
    }
    public ORPC_EXTENT?[] Read_8()
    {
        return ReadConformantStructPointerArray<ORPC_EXTENT>(false);
    }
    public byte[] Read_9()
    {
        return ReadConformantArray<byte>();
    }
}
#endregion
#region Complex Types
internal struct ORPCTHIS : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        Marshal((_ThisAndThat_Marshal_Helper)m);
    }
    private void Marshal(_ThisAndThat_Marshal_Helper m)
    {
        m.WriteStruct(version);
        m.WriteInt32(flags);
        m.WriteInt32(reserved1);
        m.WriteGuid(cid);
        m.WriteEmbeddedPointer(extensions, new Action<ORPC_EXTENT_ARRAY>(m.Write_2));
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        throw new NotImplementedException();
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public COMVERSION version;
    public int flags;
    public int reserved1;
    public Guid cid;
    public NdrEmbeddedPointer<ORPC_EXTENT_ARRAY> extensions;
}
public struct ORPC_EXTENT_ARRAY : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        Marshal((_ThisAndThat_Marshal_Helper)m);
    }
    private void Marshal(_ThisAndThat_Marshal_Helper m)
    {
        m.WriteInt32(size);
        m.WriteInt32(reserved);
        m.WriteEmbeddedPointer(extent, new Action<ORPC_EXTENT?[], long>(m.Write_8), RpcUtils.OpBitwiseAnd(RpcUtils.OpPlus(size, 1), -2));
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        Unmarshal((_ThisAndThat_Unmarshal_Helper)u);
    }
    private void Unmarshal(_ThisAndThat_Unmarshal_Helper u)
    {
        size = u.ReadInt32();
        reserved = u.ReadInt32();
        extent = u.ReadEmbeddedPointer(new Func<ORPC_EXTENT?[]>(u.Read_8), false);
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int size;
    public int reserved;
    public NdrEmbeddedPointer<ORPC_EXTENT?[]> extent;
}
public struct ORPC_EXTENT : INdrConformantStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        Marshal((_ThisAndThat_Marshal_Helper)m);
    }
    private void Marshal(_ThisAndThat_Marshal_Helper m)
    {
        m.WriteGuid(id);
        m.WriteInt32(size);
        m.Write_9(RpcUtils.CheckNull(data, "data"), RpcUtils.OpBitwiseAnd(RpcUtils.OpPlus(size, 7), -8));
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        Unmarshal((_ThisAndThat_Unmarshal_Helper)u);
    }
    private void Unmarshal(_ThisAndThat_Unmarshal_Helper u)
    {
        id = u.ReadGuid();
        size = u.ReadInt32();
        data = u.Read_9();
    }
    int INdrConformantStructure.GetConformantDimensions()
    {
        return 1;
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public Guid id;
    public int size;
    public byte[] data;
}
public struct LocalThis : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        Marshal((_ThisAndThat_Marshal_Helper)m);
    }
    private void Marshal(_ThisAndThat_Marshal_Helper m)
    {
        m.WriteInt32(dwFlags);
        m.WriteInt32(dwClientThread);
        m.WriteGuid(passthroughTraceActivity);
        m.WriteGuid(callId);
        m.Write_5(asyncRequestBlock);
        m.WriteEmbeddedPointer(pTouchedAstaArray, new Action<int>(m.WriteInt32));
        m.WriteEmbeddedPointer(containerPassthroughData, new Action<int>(m.WriteInt32));
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        throw new NotImplementedException();
    }
    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public int dwFlags;
    public int dwClientThread;
    public Guid passthroughTraceActivity;
    public Guid callId;
    public LocalThisAsyncRequestBlock asyncRequestBlock;
    public NdrEmbeddedPointer<int> pTouchedAstaArray;
    public NdrEmbeddedPointer<int> containerPassthroughData;
    public static LocalThis CreateDefault()
    {
        return new LocalThis();
    }
    public LocalThis(int dwFlags, int dwClientThread, Guid passthroughTraceActivity, Guid callId, LocalThisAsyncRequestBlock asyncRequestBlock, int? pTouchedAstaArray, int? containerPassthroughData)
    {
        this.dwFlags = dwFlags;
        this.dwClientThread = dwClientThread;
        this.passthroughTraceActivity = passthroughTraceActivity;
        this.callId = callId;
        this.asyncRequestBlock = asyncRequestBlock;
        this.pTouchedAstaArray = pTouchedAstaArray;
        this.containerPassthroughData = containerPassthroughData;
    }
}
public struct LocalThisAsyncRequestBlock : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteGuid(asyncOperationId);
        m.WriteInt64(oxidClientProcessNA);
        m.WriteGuid(originalClientLogicalThreadId);
        m.WriteInt64(uClientCausalityTraceId);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        throw new NotImplementedException();
    }
    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public Guid asyncOperationId;
    public long oxidClientProcessNA;
    public Guid originalClientLogicalThreadId;
    public long uClientCausalityTraceId;
}
public struct ORPCTHAT : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        Unmarshal((_ThisAndThat_Unmarshal_Helper)u);
    }
    private void Unmarshal(_ThisAndThat_Unmarshal_Helper u)
    {
        flags = u.ReadInt32();
        extensions = u.ReadEmbeddedPointer(new Func<ORPC_EXTENT_ARRAY>(u.Read_2), false);
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int flags;
    public NdrEmbeddedPointer<ORPC_EXTENT_ARRAY> extensions;
}
public struct LocalThat : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        Unmarshal((_ThisAndThat_Unmarshal_Helper)u);
    }
    private void Unmarshal(_ThisAndThat_Unmarshal_Helper u)
    {
        marshalingSetId = u.ReadInt64();
        reserved = u.ReadInt32();
        pAsyncResponseBlock = u.ReadEmbeddedPointer(new Func<int>(u.ReadInt32), false);
        containerErrorInformation = u.ReadEmbeddedPointer(new Func<int>(u.ReadInt32), false);
        containerPassthroughData = u.ReadEmbeddedPointer(new Func<int>(u.ReadInt32), false);
    }
    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public long marshalingSetId;
    public int reserved;
    public NdrEmbeddedPointer<int> pAsyncResponseBlock;
    public NdrEmbeddedPointer<int> containerErrorInformation;
    public NdrEmbeddedPointer<int> containerPassthroughData;
}
#endregion
