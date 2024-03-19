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

using NtApiDotNet.Win32;
using NtApiDotNet.Win32.Rpc;
using NtApiDotNet.Win32.Rpc.Transport;
using OleViewDotNet.Rpc.Clients;
using System;

namespace OleViewDotNet.Rpc.Transport;

internal sealed class RpcCOMClientTransportFactory : IRpcClientTransportFactory
{
    public const string COMAlpcProtocol = "com_lrpc";
    public const string COMTcpProtocol = "com_tcp";
    private static bool m_setup_factory;

    internal static void SetupFactory()
    {
        if (m_setup_factory)
            return;
        m_setup_factory = true;
        RpcClientTransportFactory.AddFactory(COMAlpcProtocol, new RpcCOMClientTransportFactory());
        RpcClientTransportFactory.AddFactory(COMTcpProtocol, new RpcCOMClientTransportFactory());
    }

    public IRpcClientTransport Connect(RpcEndpoint endpoint, RpcTransportSecurity transport_security)
    {
        string protoseq = endpoint.ProtocolSequence switch
        {
            COMAlpcProtocol => RpcProtocolSequence.LRPC,
            COMTcpProtocol => RpcProtocolSequence.Tcp,
            _ => throw new ArgumentException("Unsupported COM RPC protocol sequence."),
        };
        RpcStringBinding curr_binding = endpoint.Binding;
        string new_binding = RpcStringBinding.Compose(protoseq, curr_binding.NetworkAddress, curr_binding.Endpoint, curr_binding.NetworkOptions);
        endpoint = new RpcEndpoint(Guid.Empty, new Version(), RpcStringBinding.Parse(new_binding));

        var config = transport_security.Configuration as RpcCOMClientTransportConfiguration ?? throw new ArgumentException("Must specify a transport configuration.");
        transport_security.Configuration = config.InnerConfig;

        var transport = RpcClientTransportFactory.ConnectEndpoint(endpoint, transport_security);
        return new RpcCOMClientTransport(transport, transport is RpcAlpcClientTransport, config.Version, config.RemoteObject);
    }

    public static COMVERSION SupportedVersion = new(5, 7);
}
