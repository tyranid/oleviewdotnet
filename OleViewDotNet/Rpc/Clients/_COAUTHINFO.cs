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

internal struct _COAUTHINFO : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(dwAuthnSvc);
        m.WriteInt32(dwAuthzSvc);
        m.WriteEmbeddedPointer(pwszServerPrincName, m.WriteTerminatedString);
        m.WriteInt32(dwAuthnLevel);
        m.WriteInt32(dwImpersonationLevel);
        m.WriteEmbeddedPointer(pAuthIdentityData, m.WriteStruct);
        m.WriteInt32(dwCapabilities);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        dwAuthnSvc = u.ReadInt32();
        dwAuthzSvc = u.ReadInt32();
        pwszServerPrincName = u.ReadEmbeddedPointer(u.ReadConformantVaryingString, false);
        dwAuthnLevel = u.ReadInt32();
        dwImpersonationLevel = u.ReadInt32();
        pAuthIdentityData = u.ReadEmbeddedPointer(u.ReadStruct<_COAUTHIDENTITY>, false);
        dwCapabilities = u.ReadInt32();
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int dwAuthnSvc;
    public int dwAuthzSvc;
    public NdrEmbeddedPointer<string> pwszServerPrincName;
    public int dwAuthnLevel;
    public int dwImpersonationLevel;
    public NdrEmbeddedPointer<_COAUTHIDENTITY> pAuthIdentityData;
    public int dwCapabilities;
    public static _COAUTHINFO CreateDefault()
    {
        return new _COAUTHINFO();
    }
    public _COAUTHINFO(int dwAuthnSvc, int dwAuthzSvc, string pwszServerPrincName, int dwAuthnLevel, int dwImpersonationLevel, _COAUTHIDENTITY? pAuthIdentityData, int dwCapabilities)
    {
        this.dwAuthnSvc = dwAuthnSvc;
        this.dwAuthzSvc = dwAuthzSvc;
        this.pwszServerPrincName = pwszServerPrincName;
        this.dwAuthnLevel = dwAuthnLevel;
        this.dwImpersonationLevel = dwImpersonationLevel;
        this.pAuthIdentityData = pAuthIdentityData;
        this.dwCapabilities = dwCapabilities;
    }
}
