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

using NtApiDotNet;
using NtApiDotNet.Ndr.Marshal;
using NtApiDotNet.Win32.Rpc;
using NtApiDotNet.Win32.Rpc.Transport;
using OleViewDotNet.Rpc.Clients;
using System;
using System.Collections.Generic;

namespace OleViewDotNet.Rpc.Transport;

internal sealed class RpcCOMClientTransport : IRpcClientTransport
{
    private readonly IRpcClientTransport m_transport;
    private readonly bool m_local;
    private readonly COMVERSION m_version;

    public RpcCOMClientTransport(IRpcClientTransport transport, bool local, COMVERSION version)
    {
        m_transport = transport;
        m_local = local;
        m_version = version;
    }

    public bool Connected => m_transport.Connected;

    public string Endpoint => m_transport.Endpoint;

    public string ProtocolSequence => m_transport.ProtocolSequence;

    public bool Authenticated => m_transport.Authenticated;

    public RpcAuthenticationType AuthenticationType => m_transport.AuthenticationType;

    public RpcAuthenticationLevel AuthenticationLevel => m_transport.AuthenticationLevel;

    public RpcServerProcessInformation ServerProcess => m_transport.ServerProcess;

    public int CallId => m_transport.CallId;

    public bool SupportsMultipleSecurityContexts => m_transport.SupportsMultipleSecurityContexts;

    public IReadOnlyList<RpcTransportSecurityContext> SecurityContext => m_transport.SecurityContext;

    public RpcTransportSecurityContext CurrentSecurityContext { get => m_transport.CurrentSecurityContext; set => m_transport.CurrentSecurityContext = value; }

    public bool SupportsSynchronousPipes => m_transport.SupportsSynchronousPipes;

    public RpcTransportSecurityContext AddSecurityContext(RpcTransportSecurity transport_security)
    {
        return m_transport.AddSecurityContext(transport_security);
    }

    public void Bind(Guid interface_id, Version interface_version, Guid transfer_syntax_id, Version transfer_syntax_version)
    {
        m_transport.Bind(interface_id, interface_version, transfer_syntax_id, transfer_syntax_version);
    }

    public void Disconnect()
    {
        m_transport.Disconnect();
    }

    public void Dispose()
    {
        m_transport.Dispose();
    }

    public RpcClientResponse SendReceive(int proc_num, Guid objuuid, NdrDataRepresentation data_representation, byte[] ndr_buffer, IReadOnlyCollection<NtObject> handles)
    {
        _ThisAndThat_Marshal_Helper marshal = new();
        ORPCTHIS orpc_this = new();
        orpc_this.flags = m_local ? 1 : 0;
        orpc_this.cid = Guid.NewGuid();
        orpc_this.version = m_version;
        orpc_this.reserved1 = 0;
        // TODO: Implement channel hooks?
        orpc_this.extensions = null;
        marshal.WriteStruct(orpc_this);
        if (m_local)
        {
            LocalThis local_this = new();
            local_this.passthroughTraceActivity = Guid.NewGuid();
            local_this.callId = Guid.NewGuid();
            local_this.dwClientThread = NtThread.Current.ThreadId;
            marshal.WriteStruct(local_this);
        }
        marshal.WriteBytes(ndr_buffer);

        var result = m_transport.SendReceive(proc_num, objuuid, data_representation, marshal.ToArray(), handles);

        _ThisAndThat_Unmarshal_Helper unmarshal = new(result);
        unmarshal.ReadStruct<ORPCTHAT>();
        if (m_local)
        {
            unmarshal.ReadStruct<LocalThat>();
        }
        return new RpcClientResponse(unmarshal.ReadRemaining(), result.Handles);
    }
}
