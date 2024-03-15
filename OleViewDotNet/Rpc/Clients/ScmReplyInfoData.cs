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

namespace OleViewDotNet.Rpc.Clients;

internal struct ScmReplyInfoData : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteEmbeddedPointer(pResolverInfo, m.WriteStruct);
        m.WriteEmbeddedPointer(remoteReply, m.WriteStruct);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        pResolverInfo = u.ReadEmbeddedPointer(u.ReadStruct<CustomPrivResolverInfo>, false);
        remoteReply = u.ReadEmbeddedPointer(u.ReadStruct<customREMOTE_REPLY_SCM_INFO>, false);
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public NdrEmbeddedPointer<CustomPrivResolverInfo> pResolverInfo;
    public NdrEmbeddedPointer<customREMOTE_REPLY_SCM_INFO> remoteReply;

    public static ScmReplyInfoData CreateDefault()
    {
        return new ScmReplyInfoData();
    }
    public ScmReplyInfoData(CustomPrivResolverInfo? pResolverInfo, customREMOTE_REPLY_SCM_INFO? remoteReply)
    {
        this.pResolverInfo = pResolverInfo;
        this.remoteReply = remoteReply;
    }
}
