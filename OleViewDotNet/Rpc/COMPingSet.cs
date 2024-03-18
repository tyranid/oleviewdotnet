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

using OleViewDotNet.Rpc.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OleViewDotNet.Rpc;

internal sealed class COMPingSet : IDisposable
{
    private readonly Dictionary<ulong, long> m_oids = new();
    private readonly HashSet<ulong> m_add = new();
    private readonly HashSet<ulong> m_del = new();
    private readonly IOxidResolverClient m_client;
    private readonly Timer m_timer;
    private ushort m_seq_num = 1;
    private ulong m_set_id;

    public COMPingSet(IOxidResolverClient client)
    {
        m_client = client;
        int timeout = 2 * 60 * 1000;
        m_timer = new(PingServer, null, timeout, timeout);
    }

    private void PingServer(object _)
    {
        ulong[] add_to_set = null;
        ulong[] del_from_set = null;

        lock (this)
        {
            add_to_set = m_add.ToArray();
            m_add.Clear();
            del_from_set = m_del.ToArray();
            m_del.Clear();
        }

        ushort add_set_count = (ushort)add_to_set.Length;
        ushort del_set_count = (ushort)del_from_set.Length;

        if (add_set_count != 0 || del_set_count != 0)
        {
            if (m_client.ComplexPing(ref m_set_id, m_seq_num++, add_set_count, del_set_count,
                    add_set_count > 0 ? add_to_set : null, 
                    del_set_count > 0 ? del_from_set : null, out ushort _) != 0)
            {
                m_timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
        else if (m_set_id != 0)
        {
            if (m_client.SimplePing(m_set_id) != 0)
            {
                m_timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
    }

    public void AddObject(ulong oid)
    {
        lock (this)
        {
            if (!m_oids.ContainsKey(oid))
            {
                m_add.Add(oid);
                m_oids[oid] = 1;
            }
            else
            {
                m_oids[oid] += 1;
            }
        }
    }

    public void DeleteObject(ulong oid)
    {
        lock (this)
        {
            if (m_oids.ContainsKey(oid))
            {
                m_oids[oid] -= 1;
                if (m_oids[oid] <= 0)
                {
                    m_oids.Remove(oid);
                    m_del.Add(oid);
                }
            }
        }
    }

    public void Dispose()
    {
        lock (this)
        {
            m_timer.Dispose();
        }
    }
}