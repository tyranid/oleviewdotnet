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
using OleViewDotNet.Rpc.Clients;
using System;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class ScmRequestInfo : IActivationProperty
{
    private ScmRequestInfoData m_inner;

    public ScmRequestInfo(NdrPickledType pickled_type)
    {
        m_inner = new NdrUnmarshalBuffer(pickled_type).ReadStruct<ScmRequestInfoData>();
        PrivateScmInfo = m_inner.pScmInfo != null ? new(m_inner.pScmInfo) : null;
        RemoteRequestScmInfo = m_inner.remoteRequest != null ? new(m_inner.remoteRequest) : null;
    }

    public ScmRequestInfo()
    {
    }

    public Guid PropertyClsid => ActivationGuids.CLSID_ScmRequestInfo;

    public PrivateScmInfo PrivateScmInfo { get; set; }
    public RemoteRequestScmInfo RemoteRequestScmInfo { get; set; }

    public byte[] Serialize()
    {
        NdrMarshalBuffer m = new();
        m_inner.pScmInfo = PrivateScmInfo?.ToStruct();
        m_inner.remoteRequest = RemoteRequestScmInfo?.ToStruct();
        m.WriteStruct(m_inner);
        return m.ToPickledType().ToArray();
    }
}
