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

public sealed class RemoteScmReplyInfo
{
    public long Oxid { get; }
    public IReadOnlyList<COMStringBinding> OxidStringBindings { get; }
    public IReadOnlyList<COMSecurityBinding> OxidSecurityBindings { get; }
    public Guid IpidRemUnknown { get; }
    public RPC_AUTHN_LEVEL AuthenticationHint { get; }
    public COMVersion ServerVersion { get; }

    internal RemoteScmReplyInfo(customREMOTE_REPLY_SCM_INFO info)
    {
        Oxid = info.Oxid;
        if (info.pdsaOxidBindings is not null)
        {
            COMDualStringArray dsa = info.pdsaOxidBindings.GetValue().ToDSA();
            OxidStringBindings = dsa.StringBindings.AsReadOnly();
            OxidSecurityBindings = dsa.SecurityBindings.AsReadOnly();
        }
        else
        {
            OxidStringBindings = Array.Empty<COMStringBinding>();
            OxidSecurityBindings = Array.Empty<COMSecurityBinding>();
        }
        IpidRemUnknown = info.ipidRemUnknown;
        AuthenticationHint = (RPC_AUTHN_LEVEL)info.authnHint;
        ServerVersion = info.serverVersion.ToVersion();
    }
}