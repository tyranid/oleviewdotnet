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

using OleViewDotNet.Marshaling;
using OleViewDotNet.Rpc.Clients;
using System;
using System.Collections.Generic;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class PrivateScmReplyInfo
{
    public ulong OxidServer { get; }
    public IReadOnlyList<COMStringBinding> ServerORStringBindings { get; }
    public IReadOnlyList<COMSecurityBinding> ServerORSecurityBindings { get; }
    public LocalOxidResolverInfo OxidInfo { get; }
    public ulong LocalMidOfRemote { get; }
    public int DllServerModel { get; }
    public string DllServer { get; }
    public bool FoundInROT { get; }

    internal PrivateScmReplyInfo(CustomPrivResolverInfo info)
    {
        OxidServer = info.OxidServer;
        OxidInfo = new(info.OxidInfo);
        if (info.pServerORBindings is not null)
        {
            COMDualStringArray dsa = info.pServerORBindings.GetValue().ToDSA();
            ServerORStringBindings = dsa.StringBindings.AsReadOnly();
            ServerORSecurityBindings = dsa.SecurityBindings.AsReadOnly();
        }
        else
        {
            ServerORStringBindings = Array.Empty<COMStringBinding>();
            ServerORSecurityBindings = Array.Empty<COMSecurityBinding>();
        }
        LocalMidOfRemote = info.LocalMidOfRemote;
        DllServerModel = info.DllServerModel;
        DllServer = info.pwszDllServer?.GetValue();
        FoundInROT = info.FoundInROT != 0;
    }
}