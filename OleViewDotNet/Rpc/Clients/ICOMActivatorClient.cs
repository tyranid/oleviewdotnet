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

namespace OleViewDotNet.Rpc.Clients;

internal sealed class ICOMActivatorClient : RpcClientBase
{
    public ICOMActivatorClient(bool local) :
            base(local ? "00000136-0000-0000-c000-000000000046" : "000001a0-0000-0000-c000-000000000046", 0, 0)
    {
    }

    private NdrUnmarshalBuffer SendReceive(int p, NdrMarshalBuffer m)
    {
        var result = SendReceive(p, m.DataRepresentation, m.ToArray(), m.Handles);
        return new(result.NdrBuffer, result.Handles, result.DataRepresentation);
    }

    public int GetClassObject(MInterfacePointer? pActProperties, out MInterfacePointer? ppActProperties)
    {
        NdrMarshalBuffer m = new();
        m.WriteReferent(pActProperties, m.WriteStruct);
        NdrUnmarshalBuffer u = SendReceive(3, m);
        ppActProperties = u.ReadReferentValue(u.ReadStruct<MInterfacePointer>, false);
        return u.ReadInt32();
    }

    public int CreateInstance(MInterfacePointer? pUnkOuter, MInterfacePointer? pActProperties, out MInterfacePointer? ppActProperties)
    {
        NdrMarshalBuffer m = new();
        m.WriteReferent(pUnkOuter, m.WriteStruct);
        m.WriteReferent(pActProperties, m.WriteStruct);
        NdrUnmarshalBuffer u = SendReceive(4, m);
        ppActProperties = u.ReadReferentValue(u.ReadStruct<MInterfacePointer>, false);
        return u.ReadInt32();
    }
}