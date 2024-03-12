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
using OleViewDotNet.Rpc.Clients;
using OleViewDotNet.Rpc.Transport;
using System;
using System.ComponentModel;

namespace OleViewDotNet.Rpc;

internal sealed class COMRemoteUnknown : IDisposable
{
    #region Private Members
    private readonly IRemUnknownClient m_client;
    #endregion

    #region Internal Members
    internal COMRemoteUnknown(COMVERSION version, COMStringBinding binding, Guid ipid_rem_unknown, ulong oxid)
    {
        Version = version;
        string proto_seq = binding.TowerId switch
        {
            RpcTowerId.LRPC => RpcCOMClientTransportFactory.COMAlpcProtocol,
            RpcTowerId.Tcp => RpcCOMClientTransportFactory.COMTcpProtocol,
            _ => throw new ArgumentException("Unsupported COM string binding."),
        };

        StringBinding = $"{proto_seq}:{binding.NetworkAddr}";
        IpidRemUnknown = ipid_rem_unknown;
        Oxid = oxid;
        m_client = new IRemUnknownClient();
        TransportSecurity = new RpcTransportSecurity()
        {
            AuthenticationType = RpcAuthenticationType.Negotiate,
            AuthenticationLevel = RpcAuthenticationLevel.PacketPrivacy,
            Configuration = new RpcCOMClientTransportConfiguration(Version, null)
        };
        m_client.Connect(StringBinding, TransportSecurity);
        m_client.ObjectUuid = IpidRemUnknown;
    }
    #endregion

    #region Public Properties
    public string StringBinding { get; }
    public RpcTransportSecurity TransportSecurity { get; }
    public Guid IpidRemUnknown { get; }
    public ulong Oxid { get; }
    public COMVERSION Version { get; }
    #endregion

    #region Public Methods
    public COMObjRefStandard RemQueryInterface(Guid ipid, Guid iid)
    {
        int hr = m_client.RemQueryInterface(ipid, 1, 1, new[] { iid }, out REMQIRESULT[] results);
        if (hr != 0)
            throw new Win32Exception(hr);
        if (results.Length != 1)
            throw new InvalidOperationException("Result list is invalid.");
        if (results[0].hResult != 0)
            throw new Win32Exception(results[0].hResult);
        return new()
        {
            Iid = iid,
            Ipid = results[0].std.ipid,
            Oid = results[0].std.oid,
            Oxid = results[0].std.oxid,
            PublicRefs = results[0].std.cPublicRefs,
            StdFlags = (COMStdObjRefFlags)results[0].std.flags
        };
    }

    public void RemAddRef(Guid ipid, int public_refs, int private_refs)
    {
        REMINTERFACEREF[] intfs = new REMINTERFACEREF[1];
        intfs[0] = new()
        {
            ipid = ipid,
            cPublicRefs = public_refs,
            cPrivateRefs = private_refs
        };

        int hr = m_client.RemAddRef(1, intfs, out int[] results);
        if (hr != 0)
            throw new Win32Exception(hr);
        if (results[0] != 0)
            throw new Win32Exception(results[0]);
    }

    public void RemRelease(Guid ipid, int public_refs, int private_refs)
    {
        REMINTERFACEREF[] intfs = new REMINTERFACEREF[1];
        intfs[0] = new()
        {
            ipid = ipid,
            cPublicRefs = public_refs,
            cPrivateRefs = private_refs
        };

        int hr = m_client.RemRelease(1, intfs);
        if (hr != 0)
            throw new Win32Exception(hr);
    }

    public void Dispose()
    {
        m_client.Dispose();
    }
    #endregion
}