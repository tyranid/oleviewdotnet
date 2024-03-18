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
using NtApiDotNet.Win32.Rpc;
using NtApiDotNet.Win32.Rpc.Transport;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Rpc.Clients;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;

namespace OleViewDotNet.Rpc;

internal sealed class COMOxidResolverInstance : IDisposable
{
    #region Private Members
    private readonly IOxidResolverClient m_client;
    private readonly COMPingSet m_ping_set;
    private readonly ConcurrentDictionary<ulong, COMRemoteUnknown> m_resolved_oxids = new();
    private readonly bool m_local;

    private COMOxidResolverInstance(IOxidResolverClient client, bool local)
    {
        m_client = client;
        m_ping_set = new(client);
        m_local = local;
    }

    private static string FindAlpcBinding(int process_id)
    {
        // Generally the remote resolver doesn't return ALPC binding information, so let's try and 
        // brute force it based on the PID in the IPID for the remote IUnknown.
        using var rpc_dir = NtDirectory.Open(@"\RPC Control", null, DirectoryAccessRights.Query);
        foreach (var entry in rpc_dir.Query())
        {
            if (entry.NtType == NtType.GetTypeByType<NtAlpc>() && entry.Name.StartsWith("OLE"))
            {
                using var port = NtAlpcClient.Connect(entry.FullPath, null, null,
                    AlpcMessageFlags.None, null, null, null, null, NtWaitTimeout.FromSeconds(2), false);
                if (!port.IsSuccess)
                    continue;
                if (port.Result.ServerProcessId == process_id)
                    return entry.Name;
            }
        }
        throw new InvalidOperationException($"Can't find ALPC port for process ID {process_id}.");
    }

    private COMRemoteUnknown ResolveOxidInternal(ulong oxid)
    {
        short[] proto_seq = m_local ? new short[0] : new short[1] { (short)RpcTowerId.Tcp };
        int result = m_client.ResolveOxid2(oxid, (short)proto_seq.Length, proto_seq,
                out DUALSTRINGARRAY? dsa, out Guid ipid, out int _, out COMVERSION ver);
        if (result != 0)
            throw new Win32Exception(result);

        COMStringBinding binding;
        if (m_local)
        {
            string alpc_port = FindAlpcBinding(COMUtilities.GetProcessIdFromIPid(ipid));
            binding = new COMStringBinding(RpcTowerId.LRPC, $"[{alpc_port}]");
        }
        else
        {
            COMDualStringArray binding_info = dsa?.ToDSA() ?? new COMDualStringArray();
            binding = binding_info.StringBindings.FirstOrDefault(b => b.TowerId == RpcTowerId.Tcp);
        }

        if (binding is null)
        {
            throw new InvalidOperationException("Can't find a binding for the COM client.");
        }

        return new(ver, binding, ipid, oxid);
    }

    private COMRemoteUnknown GetRemUnknown(ulong oxid)
    {
        return m_resolved_oxids.GetOrAdd(oxid, ResolveOxidInternal);
    }
    #endregion

    #region Public Static Members
    public static COMOxidResolverInstance Connect(RpcStringBinding binding, bool local)
    {
        RpcTransportSecurity transport_security = new()
        {
            AuthenticationLevel = RpcAuthenticationLevel.PacketPrivacy,
            AuthenticationType = RpcAuthenticationType.Negotiate
        };

        IOxidResolverClient client = new();
        client.Connect(binding.ToString(), transport_security);
        return new COMOxidResolverInstance(client, local);
    }

    public static COMOxidResolverInstance Connect()
    {
        return Connect(RpcStringBinding.Parse("ncacn_ip_tcp:127.0.0.1[135]"), true);
    }
    #endregion

    #region Public Methods
    public COMRemoteObject GetRemoteObject(COMObjRefStandard objref)
    {
        if (objref is null)
        {
            throw new ArgumentNullException(nameof(objref));
        }

        var rem_unknown = GetRemUnknown(objref.Oxid);
        m_ping_set.AddObject(objref.Oid);
        return new(rem_unknown, objref, m_ping_set);
    }

    public void Dispose()
    {
        foreach (COMRemoteUnknown obj in m_resolved_oxids.Values)
        {
            obj.Dispose();
        }
        m_client.Dispose();
        m_ping_set.Dispose();
    }
    #endregion
}
