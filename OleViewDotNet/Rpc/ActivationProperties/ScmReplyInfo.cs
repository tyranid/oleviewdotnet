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

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class ScmReplyInfo : IActivationProperty
{
    private ScmReplyInfoData m_inner;

    internal ScmReplyInfo(byte[] data)
    {
        data.Deserialize(out m_inner);
        if (m_inner.remoteReply is not null)
        {
            RemoteInfo = new(m_inner.remoteReply);
        }
        if (m_inner.pResolverInfo is not null)
        {
            PrivateInfo = new(m_inner.pResolverInfo);
        }
    }

    public RemoteScmReplyInfo RemoteInfo { get; }
    public PrivateScmReplyInfo PrivateInfo { get; }

    public Guid PropertyClsid => ActivationGuids.CLSID_ScmReplyInfo;

    public byte[] Serialize()
    {
        return m_inner.Serialize();
    }
}
