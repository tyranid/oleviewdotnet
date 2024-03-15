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
using OleViewDotNet.Rpc.Clients;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class ScmRequestInfo : IActivationProperty
{
    private readonly List<RpcTowerId> m_proto_seqs = new();
    private customREMOTE_REQUEST_SCM_INFO m_inner = new();

    public ScmRequestInfo(NdrPickledType pickled_type)
    {
        ScmRequestInfoData request_info = new NdrUnmarshalBuffer(pickled_type).ReadStruct<ScmRequestInfoData>();
        m_inner = request_info.remoteRequest;
        m_proto_seqs = new();
        if (m_inner.pRequestedProtseqs != null)
        {
            m_proto_seqs.AddRange(m_inner.pRequestedProtseqs.GetValue().Select(p => (RpcTowerId)p));
        }
    }

    public ScmRequestInfo()
    {
    }

    public Guid PropertyClsid => ActivationGuids.CLSID_ScmRequestInfo;

    public RPC_IMP_LEVEL ClientImpLevel
    {
        get => (RPC_IMP_LEVEL)m_inner.ClientImpLevel;
        set => m_inner.ClientImpLevel = (int)value;
    }

    public List<RpcTowerId> ProtocolSequences => m_proto_seqs;

    public byte[] Serialize()
    {
        m_inner.cRequestedProtseqs = (short)m_proto_seqs.Count;
        m_inner.pRequestedProtseqs = m_proto_seqs.Select(p => (short)p).ToArray();

        ScmRequestInfoData data = new();
        data.remoteRequest = m_inner;
        NdrMarshalBuffer m = new();
        m.WriteStruct(data);
        return m.ToPickledType().ToArray();
    }
}
