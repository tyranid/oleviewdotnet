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
using System;
using System.Collections.Generic;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class LocalOxidResolverInfo
{
    public int ThreadId { get; }
    public int ProcessId { get; }
    public RPC_AUTHN_LEVEL AuthenticationHint { get; }
    public COMVersion Version { get; }
    public Guid IpidRemUnknown { get; }
    public int Flags { get; }
    public IReadOnlyList<COMStringBinding> StringBindings { get; }
    public IReadOnlyList<COMSecurityBinding> SecurityBindings { get; }
    public Guid ProcessIdentifier { get; }
    public ulong ProcessHostId { get; }
    public OxidClientDependency ClientDependencyBehavior { get; }
    public string PackageFullName { get; }
    public string UserSid { get; }
    public string AppcontainerSid { get; }
    public ulong PrimaryOxid { get; }
    public Guid PrimaryIpidRemUnknown { get; }

    internal LocalOxidResolverInfo(INTERNAL_OXID_INFO info)
    {
        ThreadId = info.dwTid;
        ProcessId = info.dwPid;
        AuthenticationHint = (RPC_AUTHN_LEVEL)info.dwAuthnHint;
        Version = info.version.ToVersion();
        IpidRemUnknown = info.ipidRemUnknown;
        Flags = info.dwFlags;
        if (info.psa is not null)
        {
            COMDualStringArray dsa = info.psa.GetValue().ToDSA();
            StringBindings = dsa.StringBindings.AsReadOnly();
            SecurityBindings = dsa.SecurityBindings.AsReadOnly();
        }
        else
        {
            StringBindings = Array.Empty<COMStringBinding>();
            SecurityBindings = Array.Empty<COMSecurityBinding>();
        }
        ProcessIdentifier = info.guidProcessIdentifier;
        ProcessHostId = info.processHostId;
        ClientDependencyBehavior = (OxidClientDependency)info.clientDependencyBehavior.Value;
        PackageFullName = info.packageFullName;
        UserSid = info.userSid;
        AppcontainerSid = info.appcontainerSid;
        PrimaryOxid = info.primaryOxid;
        PrimaryIpidRemUnknown = info.primaryIpidRemUnknown;
    }
}
