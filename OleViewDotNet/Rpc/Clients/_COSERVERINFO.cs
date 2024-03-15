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

internal struct _COSERVERINFO : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(dwReserved1);
        m.WriteEmbeddedPointer(pwszName, m.WriteTerminatedString);
        m.WriteEmbeddedPointer(pAuthInfo, m.WriteStruct);
        m.WriteInt32(dwReserved2);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        dwReserved1 = u.ReadInt32();
        pwszName = u.ReadEmbeddedPointer(u.ReadConformantVaryingString, false);
        pAuthInfo = u.ReadEmbeddedPointer(u.ReadStruct<_COAUTHINFO>, false);
        dwReserved2 = u.ReadInt32();
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int dwReserved1;
    public NdrEmbeddedPointer<string> pwszName;
    public NdrEmbeddedPointer<_COAUTHINFO> pAuthInfo;
    public int dwReserved2;
    public static _COSERVERINFO CreateDefault()
    {
        return new _COSERVERINFO();
    }
    public _COSERVERINFO(int dwReserved1, string pwszName, _COAUTHINFO? pAuthInfo, int dwReserved2)
    {
        this.dwReserved1 = dwReserved1;
        this.pwszName = pwszName;
        this.pAuthInfo = pAuthInfo;
        this.dwReserved2 = dwReserved2;
    }
}
