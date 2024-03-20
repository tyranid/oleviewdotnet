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
using System;

namespace OleViewDotNet.Rpc.Clients;

internal struct INTERNAL_OXID_INFO : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(dwTid);
        m.WriteInt32(dwPid);
        m.WriteInt32(dwAuthnHint);
        m.WriteStruct(version);
        m.WriteStruct(containerVersion);
        m.WriteGuid(ipidRemUnknown);
        m.WriteInt32(dwFlags);
        m.WriteEmbeddedPointer(psa, m.WriteStruct);
        m.WriteGuid(guidProcessIdentifier);
        m.WriteUInt64(processHostId);
        m.WriteEnum16(clientDependencyBehavior);
        m.WriteEmbeddedPointer(packageFullName, m.WriteHString);
        m.WriteEmbeddedPointer(userSid, m.WriteHString);
        m.WriteEmbeddedPointer(appcontainerSid, m.WriteHString);
        m.WriteUInt64(primaryOxid);
        m.WriteGuid(primaryIpidRemUnknown);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        dwTid = u.ReadInt32();
        dwPid = u.ReadInt32();
        dwAuthnHint = u.ReadInt32();
        version = u.ReadStruct<COMVERSION>();
        containerVersion = u.ReadStruct<CONTAINERVERSION>();
        ipidRemUnknown = u.ReadGuid();
        dwFlags = u.ReadInt32();
        psa = u.ReadEmbeddedPointer(u.ReadStruct<DUALSTRINGARRAY>, false);
        guidProcessIdentifier = u.ReadGuid();
        processHostId = u.ReadUInt64();
        clientDependencyBehavior = u.ReadEnum16();
        packageFullName = u.ReadEmbeddedPointer(u.ReadHString, false);
        userSid = u.ReadEmbeddedPointer(u.ReadHString, false);
        appcontainerSid = u.ReadEmbeddedPointer(u.ReadHString, false);
        primaryOxid = u.ReadUInt64();
        primaryIpidRemUnknown = u.ReadGuid();
    }

    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public int dwTid;
    public int dwPid;
    public int dwAuthnHint;
    public COMVERSION version;
    public CONTAINERVERSION containerVersion;
    public Guid ipidRemUnknown;
    public int dwFlags;
    public NdrEmbeddedPointer<DUALSTRINGARRAY> psa;
    public Guid guidProcessIdentifier;
    public ulong processHostId;
    public NdrEnum16 clientDependencyBehavior;
    public NdrEmbeddedPointer<string> packageFullName;
    public NdrEmbeddedPointer<string> userSid;
    public NdrEmbeddedPointer<string> appcontainerSid;
    public ulong primaryOxid;
    public Guid primaryIpidRemUnknown;
    public static INTERNAL_OXID_INFO CreateDefault()
    {
        return new INTERNAL_OXID_INFO();
    }
}
