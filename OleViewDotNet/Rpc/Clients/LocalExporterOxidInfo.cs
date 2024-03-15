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

internal struct LocalExporterOxidInfo : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(dwTid);
        m.WriteInt32(dwPid);
        m.WriteInt32(dwAuthnHint);
        m.WriteStruct(version);
        m.WriteGuid(ipidRemUnknown);
        m.WriteInt32(dwFlags);
        m.WriteEmbeddedPointer(psa, m.WriteStruct);
        m.WriteGuid(guidProcessIdentifier);
        m.WriteInt64(processHostId);
        m.WriteEnum16(clientDependencyBehavior);
        m.WriteEmbeddedPointer(packageFullName, m.WriteHString);
        m.WriteEmbeddedPointer(userSid, m.WriteHString);
        m.WriteEmbeddedPointer(appcontainerSid, m.WriteHString);
        m.WriteInt64(primaryOxid);
        m.WriteGuid(primaryIpidRemUnknown);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        dwTid = u.ReadInt32();
        dwPid = u.ReadInt32();
        dwAuthnHint = u.ReadInt32();
        version = u.ReadStruct<COMVERSION>();
        ipidRemUnknown = u.ReadGuid();
        dwFlags = u.ReadInt32();
        psa = u.ReadEmbeddedPointer(u.ReadStruct<DUALSTRINGARRAY>, false);
        guidProcessIdentifier = u.ReadGuid();
        processHostId = u.ReadInt64();
        clientDependencyBehavior = u.ReadEnum16();
        packageFullName = u.ReadEmbeddedPointer(u.ReadHString, false);
        userSid = u.ReadEmbeddedPointer(u.ReadHString, false);
        appcontainerSid = u.ReadEmbeddedPointer(u.ReadHString, false);
        primaryOxid = u.ReadInt64();
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
    public Guid ipidRemUnknown;
    public int dwFlags;
    public NdrEmbeddedPointer<DUALSTRINGARRAY> psa;
    public Guid guidProcessIdentifier;
    public long processHostId;
    public NdrEnum16 clientDependencyBehavior;
    public NdrEmbeddedPointer<string> packageFullName;
    public NdrEmbeddedPointer<string> userSid;
    public NdrEmbeddedPointer<string> appcontainerSid;
    public long primaryOxid;
    public Guid primaryIpidRemUnknown;
    public static LocalExporterOxidInfo CreateDefault()
    {
        return new LocalExporterOxidInfo();
    }
    public LocalExporterOxidInfo(int dwTid, int dwPid, int dwAuthnHint, COMVERSION version, Guid ipidRemUnknown, int dwFlags, DUALSTRINGARRAY? psa, Guid guidProcessIdentifier, long processHostId, NdrEnum16 clientDependencyBehavior, string packageFullName, string userSid, string appcontainerSid, long primaryOxid, Guid primaryIpidRemUnknown)
    {
        this.dwTid = dwTid;
        this.dwPid = dwPid;
        this.dwAuthnHint = dwAuthnHint;
        this.version = version;
        this.ipidRemUnknown = ipidRemUnknown;
        this.dwFlags = dwFlags;
        this.psa = psa;
        this.guidProcessIdentifier = guidProcessIdentifier;
        this.processHostId = processHostId;
        this.clientDependencyBehavior = clientDependencyBehavior;
        this.packageFullName = packageFullName;
        this.userSid = userSid;
        this.appcontainerSid = appcontainerSid;
        this.primaryOxid = primaryOxid;
        this.primaryIpidRemUnknown = primaryIpidRemUnknown;
    }
}
