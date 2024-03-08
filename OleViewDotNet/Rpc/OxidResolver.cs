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


using NtApiDotNet.Win32.Rpc.Transport;
using OleViewDotNet.Marshaling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OleViewDotNet.Rpc;

public sealed class OxidResolver : IDisposable
{
    private readonly OxidResolverClient m_client;

    private OxidResolver(OxidResolverClient client)
    {
        m_client = client;
    }

    public static OxidResolver Connect(string protocol_seq, string endpoint, string network_address, RpcTransportSecurity transport_security)
    {
        OxidResolverClient client = new();
        client.Connect(protocol_seq, endpoint, network_address, transport_security);
        return new OxidResolver(client);
    }

    public static OxidResolver Connect()
    {
        RpcTransportSecurity sec = new()
        {
            AuthenticationLevel = RpcAuthenticationLevel.PacketPrivacy,
            AuthenticationType = RpcAuthenticationType.Negotiate
        };
        return Connect("ncacn_ip_tcp", "135", "127.0.0.1", sec);
    }

    public ServerAliveResponse ServerAlive2()
    {
        uint result = m_client.ServerAlive2(out COMVERSION ver, out DUALSTRINGARRAY? bindings, out int reserver);
        if (result != 0)
            throw new Win32Exception((int)result);
        return new(ver, bindings?.ToDSA());
    }

    public ResolveOxidResponse ResolveOxid2(ulong oxid, params RpcTowerId[] request_protocol_seqs)
    {
        List<RpcTowerId> proto_seqs = new(request_protocol_seqs);
        uint result = m_client.ResolveOxid2(oxid, (short)proto_seqs.Count, proto_seqs.Select(t => (short)t).ToArray(),
            out DUALSTRINGARRAY? dsa, out Guid ipid, out int authn_hint, out COMVERSION ver);
        if (result != 0)
            throw new Win32Exception((int)result);
        return new(ver, dsa?.ToDSA(), authn_hint, ipid);
    }

    public void Dispose()
    {
        m_client.Dispose();
    }
}
