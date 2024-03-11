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
using OleViewDotNet.Marshaling;
using OleViewDotNet.Proxy;
using System;

namespace OleViewDotNet.Rpc;

public class COMRemoteObject : IDisposable
{
    private readonly COMRemoteUnknown m_rem_unknown;
    private readonly COMObjRefStandard m_objref;
    private readonly COMPingSet m_ping_set;

    internal COMRemoteObject(COMRemoteUnknown rem_unknown, COMObjRefStandard objref, COMPingSet ping_set)
    {
        m_rem_unknown = rem_unknown;
        m_objref = objref;
        m_ping_set = ping_set;
    }

    public Guid Iid => m_objref.Iid;
    public Guid Ipid => m_objref.Ipid;
    public ulong Oid => m_objref.Oid;

    public COMRemoteObject QueryInterface(Guid iid)
    {
        return COMOxidResolver.GetRemoteObject(m_rem_unknown.RemQueryInterface(Ipid, iid));
    }

    public RpcClientBase CreateClient(COMProxyInterface proxy)
    {
        if (proxy is null)
        {
            throw new ArgumentNullException(nameof(proxy));
        }

        Guid ipid = Ipid;
        if (proxy.Iid != Iid)
        {
            using var obj = QueryInterface(proxy.Iid);
            m_rem_unknown.RemAddRef(obj.Ipid, 1, 0);
            ipid = obj.Ipid;
        }

        var client = proxy.CreateClient();
        client.Connect(m_rem_unknown.StringBinding, m_rem_unknown.TransportSecurity);
        client.ObjectUuid = ipid;
        return client;
    }

    public void Dispose()
    {
        m_ping_set.DeleteObject(m_objref.Oid);
        m_rem_unknown.RemRelease(Ipid, 1, 0);
    }
}