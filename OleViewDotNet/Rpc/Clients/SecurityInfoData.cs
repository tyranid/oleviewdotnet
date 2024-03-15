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

internal struct SecurityInfoData : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(dwAuthnFlags);
        m.WriteEmbeddedPointer(pServerInfo, m.WriteStruct);
        m.WriteEmbeddedPointer(pAuthIdentityInfo, m.WriteStruct);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        dwAuthnFlags = u.ReadInt32();
        pServerInfo = u.ReadEmbeddedPointer(u.ReadStruct<_COSERVERINFO>, false);
        pAuthIdentityInfo = u.ReadEmbeddedPointer(u.ReadStruct<_COAUTHIDENTITY>, false);
    }

    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int dwAuthnFlags;
    public NdrEmbeddedPointer<_COSERVERINFO> pServerInfo;
    public NdrEmbeddedPointer<_COAUTHIDENTITY> pAuthIdentityInfo;

    public static SecurityInfoData CreateDefault()
    {
        return new SecurityInfoData();
    }
    public SecurityInfoData(int dwAuthnFlags, _COSERVERINFO? pServerInfo, _COAUTHIDENTITY? pAuthIdentityInfo)
    {
        this.dwAuthnFlags = dwAuthnFlags;
        this.pServerInfo = pServerInfo;
        this.pAuthIdentityInfo = pAuthIdentityInfo;
    }
}
