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

using OleViewDotNet.Interop;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Rpc.Clients;
using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class RemoteScmRequestInfo
{
    private customREMOTE_REQUEST_SCM_INFO m_inner;

    public RPC_IMP_LEVEL ClientImpLevel
    {
        get => (RPC_IMP_LEVEL)m_inner.ClientImpLevel;
        set => m_inner.ClientImpLevel = (int)value;
    }

    public List<RpcTowerId> ProtocolSequences { get; }

    public RemoteScmRequestInfo() : this(default)
    {
    }

    internal RemoteScmRequestInfo(customREMOTE_REQUEST_SCM_INFO inner)
    {
        m_inner = inner;
        ProtocolSequences = new();
        var proto_seqs = m_inner.pRequestedProtseqs?.GetValue();
        if (proto_seqs is not null)
        {
            ProtocolSequences.AddRange(proto_seqs.Select(p => (RpcTowerId)p));
        }
    }

    internal customREMOTE_REQUEST_SCM_INFO ToStruct()
    {
        m_inner.cRequestedProtseqs = (short)ProtocolSequences.Count;
        m_inner.pRequestedProtseqs = m_inner.cRequestedProtseqs != 0 ? ProtocolSequences.Select(p => (short)p).ToArray() : null;
        return m_inner;
    }
}