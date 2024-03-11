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

#region Marshal Helpers
internal class _IRemUnknown_Marshal_Helper : NdrMarshalBuffer
{
    public void Write_4(REMQIRESULT p0)
    {
        WriteStruct(p0);
    }
    public void Write_5(STDOBJREF p0)
    {
        WriteStruct(p0);
    }
    public void Write_6(REMINTERFACEREF p0)
    {
        WriteStruct(p0);
    }
    public void Write_8(byte[] p0, long p1)
    {
        WriteConformantArray(p0, p1);
    }
    public void Write_9(Guid[] p0, long p1)
    {
        WriteConformantArrayCallback(p0, new Action<Guid>(WriteGuid), p1);
    }
    public void Write_10(REMQIRESULT[] p0, long p1)
    {
        WriteConformantStructArray(p0, p1);
    }
    public void Write_11(REMINTERFACEREF[] p0, long p1)
    {
        WriteConformantArray(p0, p1);
    }
    public void Write_12(int[] p0, long p1)
    {
        WriteConformantArray(p0, p1);
    }
}
internal class _IRemUnknown_Unmarshal_Helper : NdrUnmarshalBuffer
{
    public _IRemUnknown_Unmarshal_Helper(RpcClientResponse r) :
            base(r.NdrBuffer, r.Handles, r.DataRepresentation)
    {
    }
    public COMVERSION Read_1()
    {
        return ReadStruct<COMVERSION>();
    }
    public REMQIRESULT Read_4()
    {
        return ReadStruct<REMQIRESULT>();
    }
    public STDOBJREF Read_5()
    {
        return ReadStruct<STDOBJREF>();
    }
    public REMINTERFACEREF Read_6()
    {
        return ReadStruct<REMINTERFACEREF>();
    }
    public byte[] Read_8()
    {
        return ReadConformantArray<byte>();
    }
    public Guid[] Read_9()
    {
        return ReadConformantArrayCallback(new Func<Guid>(ReadGuid));
    }
    public REMQIRESULT[] Read_10()
    {
        return ReadConformantStructArray<REMQIRESULT>();
    }
    public REMINTERFACEREF[] Read_11()
    {
        return ReadConformantArray<REMINTERFACEREF>();
    }
    public int[] Read_12()
    {
        return ReadConformantArray<int>();
    }
}

#endregion
#region Complex Types
public struct REMQIRESULT : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        Marshal((_IRemUnknown_Marshal_Helper)m);
    }
    private void Marshal(_IRemUnknown_Marshal_Helper m)
    {
        m.WriteInt32(hResult);
        m.Write_5(std);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        Unmarshal((_IRemUnknown_Unmarshal_Helper)u);
    }
    private void Unmarshal(_IRemUnknown_Unmarshal_Helper u)
    {
        hResult = u.ReadInt32();
        std = u.Read_5();
    }
    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public int hResult;
    public STDOBJREF std;
}
public struct STDOBJREF : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(flags);
        m.WriteInt32(cPublicRefs);
        m.WriteUInt64(oxid);
        m.WriteUInt64(oid);
        m.WriteGuid(ipid);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        flags = u.ReadInt32();
        cPublicRefs = u.ReadInt32();
        oxid = u.ReadUInt64();
        oid = u.ReadUInt64();
        ipid = u.ReadGuid();
    }

    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public int flags;
    public int cPublicRefs;
    public ulong oxid;
    public ulong oid;
    public Guid ipid;
}
public struct REMINTERFACEREF : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteGuid(ipid);
        m.WriteInt32(cPublicRefs);
        m.WriteInt32(cPrivateRefs);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        ipid = u.ReadGuid();
        cPublicRefs = u.ReadInt32();
        cPrivateRefs = u.ReadInt32();
    }

    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public Guid ipid;
    public int cPublicRefs;
    public int cPrivateRefs;
}
#endregion
#region Client Implementation
public sealed class IRemUnknownClient : RpcClientBase
{
    public IRemUnknownClient() :
            base("00000131-0000-0000-c000-000000000046", 0, 0)
    {
    }
    private _IRemUnknown_Unmarshal_Helper SendReceive(int p, _IRemUnknown_Marshal_Helper m)
    {
        return new _IRemUnknown_Unmarshal_Helper(SendReceive(p, m.DataRepresentation, m.ToArray(), m.Handles));
    }

    public int RemQueryInterface(Guid ripid, int cRefs, short cIids, Guid[] iids, out REMQIRESULT[] ppQIResults)
    {
        _IRemUnknown_Marshal_Helper m = new();
        m.WriteGuid(ripid);
        m.WriteInt32(cRefs);
        m.WriteInt16(cIids);
        m.Write_9(RpcUtils.CheckNull(iids, "iids"), cIids);
        _IRemUnknown_Unmarshal_Helper u = SendReceive(3, m);
        ppQIResults = u.ReadReferent(new Func<REMQIRESULT[]>(u.Read_10), false);
        return u.ReadInt32();
    }

    public int RemAddRef(short cInterfaceRefs, REMINTERFACEREF[] InterfaceRefs, out int[] pResults)
    {
        _IRemUnknown_Marshal_Helper m = new();
        m.WriteInt16(cInterfaceRefs);
        m.Write_11(RpcUtils.CheckNull(InterfaceRefs, "InterfaceRefs"), cInterfaceRefs);
        _IRemUnknown_Unmarshal_Helper u = SendReceive(4, m);
        pResults = u.Read_12();
        return u.ReadInt32();
    }

    public int RemRelease(short cInterfaceRefs, REMINTERFACEREF[] InterfaceRefs)
    {
        _IRemUnknown_Marshal_Helper m = new();
        m.WriteInt16(cInterfaceRefs);
        m.Write_11(RpcUtils.CheckNull(InterfaceRefs, "InterfaceRefs"), cInterfaceRefs);
        _IRemUnknown_Unmarshal_Helper u = SendReceive(5, m);
        return u.ReadInt32();
    }
}
#endregion

