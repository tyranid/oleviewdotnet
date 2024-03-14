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

public sealed class IRemUnknownClient : RpcClientBase
{
    public IRemUnknownClient() :
            base("00000131-0000-0000-c000-000000000046", 0, 0)
    {
    }
    private NdrUnmarshalBuffer SendReceive(int p, NdrMarshalBuffer m)
    {
        var result = SendReceive(p, m.DataRepresentation, m.ToArray(), m.Handles);
        return new(result.NdrBuffer, result.Handles, result.DataRepresentation);
    }

    public int RemQueryInterface(Guid ripid, int cRefs, short cIids, Guid[] iids, out REMQIRESULT[] ppQIResults)
    {
        NdrMarshalBuffer m = new();
        m.WriteGuid(ripid);
        m.WriteInt32(cRefs);
        m.WriteInt16(cIids);
        m.WriteConformantArrayCallback(RpcUtils.CheckNull(iids, "iids"), new Action<Guid>(m.WriteGuid), cIids);
        NdrUnmarshalBuffer u = SendReceive(3, m);
        ppQIResults = u.ReadReferent(new Func<REMQIRESULT[]>(u.ReadConformantStructArray<REMQIRESULT>), false);
        return u.ReadInt32();
    }

    public int RemAddRef(short cInterfaceRefs, REMINTERFACEREF[] InterfaceRefs, out int[] pResults)
    {
        NdrMarshalBuffer m = new();
        m.WriteInt16(cInterfaceRefs);
        m.WriteConformantArray(RpcUtils.CheckNull(InterfaceRefs, "InterfaceRefs"), cInterfaceRefs);
        NdrUnmarshalBuffer u = SendReceive(4, m);
        pResults = u.ReadConformantArray<int>();
        return u.ReadInt32();
    }

    public int RemRelease(short cInterfaceRefs, REMINTERFACEREF[] InterfaceRefs)
    {
        NdrMarshalBuffer m = new();
        m.WriteInt16(cInterfaceRefs);
        m.WriteConformantArray(RpcUtils.CheckNull(InterfaceRefs, "InterfaceRefs"), cInterfaceRefs);
        NdrUnmarshalBuffer u = SendReceive(5, m);
        return u.ReadInt32();
    }
}
