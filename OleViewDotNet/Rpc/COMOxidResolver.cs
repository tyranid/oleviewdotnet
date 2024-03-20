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
using OleViewDotNet.Interop;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Utilities;
using OleViewDotNet.Wrappers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Rpc;

public static class COMOxidResolver
{
    #region Private Members
    private static readonly ConcurrentDictionary<COMStringBinding, COMOxidResolverInstance> m_resolvers = new();
    private static readonly Lazy<HashSet<string>> m_local_hosts = new(GetLocalHosts);

    private static HashSet<string> GetLocalHosts()
    {
        HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
        string hostname = Dns.GetHostName();
        names.Add(hostname);
        names.Add("127.0.0.1");
        names.Add("::1");

        try
        {
            var entry = Dns.GetHostEntry(hostname);
            foreach (var addr in entry.AddressList)
            {
                names.Add(addr.ToString());
            }
        }
        catch
        {
        }
        return names;
    }

    private static COMStringBinding GetTargetBinding(COMObjRefStandard objref)
    {
        if (objref.StringBindings.Count == 0)
            return new COMStringBinding(RpcTowerId.LRPC, string.Empty);

        foreach (var binding in objref.StringBindings)
        {
            if (binding.TowerId == RpcTowerId.LRPC)
                return new COMStringBinding(RpcTowerId.LRPC, string.Empty);

            string name = binding.NetworkAddr;
            int index = name.IndexOf('[');
            if (index >= 0)
            {
                name = name.Substring(0, index);
            }
            if (m_local_hosts.Value.Contains(name))
            {
                return new COMStringBinding(RpcTowerId.LRPC, string.Empty);
            }
        }

        return objref.StringBindings[0];
    }

    private static COMOxidResolverInstance CreateOxidResolver(COMStringBinding binding)
    {
        if (binding.TowerId == RpcTowerId.LRPC)
            return COMOxidResolverInstance.Connect();
        return COMOxidResolverInstance.Connect(binding.GetRpcStringBinding(true), false);
    }
    #endregion

    #region Public Static Members
    public static COMRemoteObject GetRemoteObject(COMObjRefStandard objref)
    {
        if (objref is null)
        {
            throw new ArgumentNullException(nameof(objref));
        }

        var resolver = m_resolvers.GetOrAdd(GetTargetBinding(objref), s => CreateOxidResolver(s));
        return resolver.GetRemoteObject(objref);
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
            iid = COMKnownGuids.IID_IUnknown;

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
