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

using NtApiDotNet.Win32.Rpc;
using NtApiDotNet.Win32.Rpc.Transport;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Rpc.Clients;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace OleViewDotNet.Rpc;

public sealed class COMOxidResolver : IDisposable
{
    private readonly OxidResolverClient m_client;
    private long m_ref_count;

    private COMOxidResolver(OxidResolverClient client)
    {
        m_client = client;
        m_ref_count = 1;
    }

    public static COMOxidResolver Connect(RpcStringBinding binding, RpcTransportSecurity transport_security)
    {
        OxidResolverClient client = new();
        client.Connect(binding.ToString(), transport_security);
        return new COMOxidResolver(client);
    }

    public static COMOxidResolver Connect()
    {
        RpcTransportSecurity sec = new()
        {
            AuthenticationLevel = RpcAuthenticationLevel.PacketPrivacy,
            AuthenticationType = RpcAuthenticationType.Negotiate
        };
        return Connect(RpcStringBinding.Parse("ncacn_ip_tcp:127.0.0.1[135]"), sec);
    }

    public ServerAliveResponse ServerAlive2()
    {
        int result = m_client.ServerAlive2(out COMVERSION ver, out DUALSTRINGARRAY? bindings, out _);
        if (result != 0)
            throw new Win32Exception(result);
        return new(ver, bindings?.ToDSA());
    }

    public COMResolveOxidResponse ResolveOxid2(ulong oxid, params RpcTowerId[] request_protocol_seqs)
    {
        List<RpcTowerId> proto_seqs = new(request_protocol_seqs);
        int hr = m_client.ResolveOxid2(oxid, (short)proto_seqs.Count, proto_seqs.Select(t => (short)t).ToArray(),
            out DUALSTRINGARRAY? dsa, out Guid ipid, out int authn_hint, out COMVERSION ver);
        if (hr != 0)
            throw new Win32Exception(hr);
        return new(ver, dsa?.ToDSA(), authn_hint, ipid, oxid);
    }

    public ulong ComplexPing(ulong set_id, ushort seq_num, ulong[] add_to_set, ulong[] del_from_set)
    {
        ushort add_set_count = (ushort)(add_to_set?.Length ?? 0);
        ushort del_set_count = (ushort)(del_from_set?.Length ?? 0);

        int hr = m_client.ComplexPing(ref set_id, seq_num, add_set_count, del_set_count,
            add_to_set, del_from_set, out ushort backoff_factory);
        if (hr != 0)
            throw new Win32Exception(hr);
        return set_id;
    }

    public void SimplePing(ulong set_id)
    {
        int hr = m_client.SimplePing(set_id);
        if (hr != 0)
            throw new Win32Exception(hr);
    }

    public void Dispose()
    {
        if (Interlocked.Decrement(ref m_ref_count) == 0)
        {
            m_client.Dispose();
        }
    }
}
