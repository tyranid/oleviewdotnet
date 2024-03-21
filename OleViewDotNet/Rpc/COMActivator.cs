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
using OleViewDotNet.Rpc.ActivationProperties;
using OleViewDotNet.Rpc.Clients;
using OleViewDotNet.Rpc.Transport;
using System;
using System.ComponentModel;

namespace OleViewDotNet.Rpc;

public sealed class COMActivator : IDisposable
{
    private readonly ICOMActivatorClient m_client;

    private COMActivator(ICOMActivatorClient client)
    {
        m_client = client;
    }

    public ActivationPropertiesOut GetClassObject(ActivationPropertiesIn properties)
    {
        if (properties is null)
        {
            throw new ArgumentNullException(nameof(properties));
        }

        COMObjRefCustom objref = properties.ToObjRef();
        int result = m_client.GetClassObject(objref.ToNullable(), out MInterfacePointer? act_out);
        if (result != 0)
            throw new Win32Exception(result);
        if (!act_out.HasValue)
            throw new InvalidOperationException("No properties returned from the activation call.");
        objref = COMObjRef.FromArray(act_out.Value.abData) as COMObjRefCustom 
            ?? throw new InvalidOperationException("Output properties were not custom marshaled.");
        return new(objref);
    }

    public ActivationPropertiesOut CreateInstance(COMObjRef unknown_outer, ActivationPropertiesIn properties)
    {
        if (properties is null)
        {
            throw new ArgumentNullException(nameof(properties));
        }

        COMObjRefCustom objref = properties.ToObjRef();
        int result = m_client.CreateInstance(unknown_outer.ToNullable(), objref.ToNullable(), out MInterfacePointer? act_out);
        if (result != 0)
            throw new Win32Exception(result);
        if (!act_out.HasValue)
            throw new InvalidOperationException("No properties returned from the activation call.");
        objref = COMObjRef.FromArray(act_out.Value.abData) as COMObjRefCustom 
            ?? throw new InvalidOperationException("Output properties were not custom marshaled.");
        return new(objref);
    }

    /// <summary>
    /// Connect to the local activator.
    /// </summary>
    /// <returns>The local activator.</returns>
    public static COMActivator Connect()
    {
        RpcCOMClientTransportFactory.SetupFactory();
        ICOMActivatorClient client = new(true);
        RpcTransportSecurity transport_security = new();
        transport_security.Configuration = new RpcCOMClientTransportConfiguration(RpcCOMClientTransportFactory.SupportedVersion, null);
        client.Connect(RpcCOMClientTransportFactory.COMAlpcProtocol, "epmapper", transport_security);
        return new COMActivator(client);
    }

    /// <summary>
    /// Connect to a remote activator.
    /// </summary>
    /// <param name="hostname">The hostname to connect to.</param>
    /// <returns>The remote activator.</returns>
    public static COMActivator Connect(string hostname)
    {
        RpcCOMClientTransportFactory.SetupFactory();
        ICOMActivatorClient client = new(false);
        RpcTransportSecurity transport_security = new()
        {
            AuthenticationType = RpcAuthenticationType.Negotiate,
            AuthenticationLevel = RpcAuthenticationLevel.PacketPrivacy,
            Configuration = new RpcCOMClientTransportConfiguration(RpcCOMClientTransportFactory.SupportedVersion, null)
        };
        client.Connect(RpcCOMClientTransportFactory.COMTcpProtocol, "135", hostname, transport_security);
        return new COMActivator(client);
    }

    public string ProtocolSequence => m_client.Transport.ProtocolSequence;
    public string Endpoint => m_client.Transport.Endpoint;

    public void Dispose()
    {
        m_client.Dispose();
    }
}