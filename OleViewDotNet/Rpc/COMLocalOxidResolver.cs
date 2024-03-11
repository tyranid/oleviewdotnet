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
using NtApiDotNet.Win32.Rpc.Transport;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Rpc.Clients;
using OleViewDotNet.Utilities;
using OleViewDotNet.Wrappers;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Rpc;

public static class COMLocalOxidResolver
{
    #region Private Members
    private static readonly Lazy<OxidResolverClient> m_client = new(CreateClient);
    private static readonly ConcurrentDictionary<ulong, COMResolveOxidResponse> m_resolved_oxids = new();
    private static COMPingSet m_ping_set;

    private static OxidResolverClient CreateClient()
    {
        OxidResolverClient client = new();
        RpcTransportSecurity transport_security = new()
        {
            AuthenticationLevel = RpcAuthenticationLevel.PacketPrivacy,
            AuthenticationType = RpcAuthenticationType.Negotiate
        };
        client.Connect("ncacn_ip_tcp:127.0.0.1[135]", transport_security);
        return client;
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

    private static COMDualStringArray AddAlpcBinding(Guid ipid, COMDualStringArray dsa)
    {
        dsa ??= new COMDualStringArray();
        if (dsa.StringBindings.Any(b => b.TowerId == RpcTowerId.LRPC))
            return dsa;

        string alpc_port = FindAlpcBinding(COMUtilities.GetProcessIdFromIPid(ipid));
        dsa.StringBindings.Insert(0, new COMStringBinding(RpcTowerId.LRPC, $"[{alpc_port}]"));
        return dsa;
    }

    private static COMResolveOxidResponse ResolveOxidInternal(ulong oxid)
    {
        int result = m_client.Value.ResolveOxid2(oxid, 0, Array.Empty<short>(),
                out DUALSTRINGARRAY? dsa, out Guid ipid, out int authn_hint, out COMVERSION ver);
        if (result != 0)
            throw new Win32Exception(result);
        return new(ver, AddAlpcBinding(ipid, dsa?.ToDSA()), authn_hint, ipid, oxid);
    }

    private static COMResolveOxidResponse ResolveOxid(ulong oxid)
    {
        return m_resolved_oxids.GetOrAdd(oxid, ResolveOxidInternal);
    }

    private static COMRemoteUnknown GetRemUnknown(ulong oxid)
    {
        var response = ResolveOxid(oxid);
        return new COMRemoteUnknown(response);
    }
    #endregion

    #region Public Static Members
    public static COMRemoteObject GetRemoteObject(COMObjRefStandard objref)
    {
        if (objref is null)
        {
            throw new ArgumentNullException(nameof(objref));
        }

        var rem_unknown = GetRemUnknown(objref.Oxid);
        m_ping_set ??= new COMPingSet(m_client.Value);
        m_ping_set.AddObject(objref.Oid);
        return new(rem_unknown, objref, m_ping_set);
    }

    public static COMRemoteObject GetRemoteObject(object obj, Guid iid = default)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (!Marshal.IsComObject(obj))
        {
            throw new ArgumentException("Object is not a COM object.", nameof(obj));
        }

        if (iid == Guid.Empty)
            iid = COMInterfaceEntry.IID_IUnknown;

        if (COMUtilities.MarshalObjectToObjRef(obj,
            iid, MSHCTX.LOCAL, MSHLFLAGS.NORMAL) is not COMObjRefStandard objref)
        {
            throw new ArgumentException("Object cannot be standard marshaled.", nameof(obj));
        }

        return GetRemoteObject(objref);
    }

    public static COMRemoteObject GetRemoteObject(BaseComWrapper wrapper)
    {
        if (wrapper is null)
        {
            throw new ArgumentNullException(nameof(wrapper));
        }

        return GetRemoteObject(wrapper.Unwrap(), wrapper.Iid);
    }

    public static COMRemoteObject GetRemoteObject(NdrInterfacePointer intf)
    {
        if (COMObjRef.FromArray(intf.Data) is not COMObjRefStandard objref)
        {
            throw new ArgumentException("COM object can not be standard marshaled.", nameof(intf));
        }

        return GetRemoteObject(objref);
    }
    #endregion
}
