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
using NtApiDotNet.Win32.Rpc.Transport;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Rpc.Clients;
using OleViewDotNet.Rpc.Transport;
using System;

namespace OleViewDotNet.Rpc;

public static class RpcComUtils
{
    internal static RpcStringBinding GetRpcStringBinding(this COMStringBinding binding, bool epmapper = false)
    {
        string protocol_sequence = binding.TowerId switch
        {
            RpcTowerId.Tcp => RpcProtocolSequence.Tcp,
            RpcTowerId.NamedPipe => RpcProtocolSequence.NamedPipe,
            RpcTowerId.LRPC => RpcProtocolSequence.LRPC,
            RpcTowerId.Container => RpcProtocolSequence.Container,
            _ => throw new ArgumentException("Unsupported tower ID."),
        };

        string endpoint = string.Empty;
        string hostname = binding.NetworkAddr;
        if (epmapper)
        {
            int index = hostname.IndexOf('[');
            if (index >= 0)
            {
                hostname = hostname.Substring(0, index);
            }

            endpoint = binding.TowerId switch
            {
                RpcTowerId.Tcp => "[135]",
                RpcTowerId.NamedPipe => @"[\\pipe\\epmapper]",
                RpcTowerId.LRPC => "[epmapper]",
                RpcTowerId.Container => "[DA32E281-383E-49A1-900A-AF3B74B90B0E]",
                _ => throw new ArgumentException("Unsupported tower ID."),
            };
        }

        return RpcStringBinding.Parse($"{protocol_sequence}:{hostname}{endpoint}");
    }

    internal static RpcTransportSecurity GetRpcTransportSecurity(this COMSecurityBinding binding)
    {
        return new()
        {
            AuthenticationType = (RpcAuthenticationType)binding.AuthnSvc,
            ServicePrincipalName = binding.PrincName
        };
    }

    internal static COMVersion ToVersion(this COMVERSION ver)
    {
        return new(ver.MajorVersion, ver.MinorVersion);
    }

    internal static COMVERSION ToVersion(this COMVersion ver)
    {
        return new(ver.Major, ver.Minor);
    }

    internal static COMObjRef ToObjRef(this NdrEmbeddedPointer<MInterfacePointer> pointer)
    {
        if (pointer is null)
            return null;
        return COMObjRef.FromArray(pointer.GetValue().abData);
    }

    internal static NdrEmbeddedPointer<MInterfacePointer> ToPointer(this COMObjRef objref)
    {
        if (objref is null)
            return null;
        byte[] ba = objref.ToArray();
        MInterfacePointer p = new(ba.Length, ba);
        return p;
    }

    internal static MInterfacePointer? ToNullable(this COMObjRef objref)
    {
        if (objref is null)
            return null;
        byte[] ba = objref.ToArray();
        MInterfacePointer p = new(ba.Length, ba);
        return p;
    }

    public static RpcClientBase CreateClient(Type type, object obj, COMRegistry database)
    {
        return (RpcClientBase)Activator.CreateInstance(type, obj, database);
    }

    public static void ConnectClient(RpcClientBase client, object obj, COMRegistry registry)
    {
        client.Connect(new RpcChannelBufferClientTransport(obj, client.InterfaceId, registry));
    }

    public static object Unwrap(this RpcClientBase client)
    {
        if (client.Transport is not RpcChannelBufferClientTransport transport)
        {
            throw new ArgumentException("Unsupported RPC transport.");
        }
        return transport.GetObject();
    }
}