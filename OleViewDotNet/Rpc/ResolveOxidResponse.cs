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


using NtApiDotNet;
using OleViewDotNet.Database;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet.Rpc;

public sealed class ResolveOxidResponse
{
    public COMVersion Version { get; }
    public IReadOnlyList<COMStringBinding> StringBindings { get; }
    public IReadOnlyList<COMSecurityBinding> SecurityBindings { get; }
    public int AuthenticationHint { get; }
    public Guid IpidRemUnknown { get; }
    public int ProcessId => COMUtilities.GetProcessIdFromIPid(IpidRemUnknown);

    internal ResolveOxidResponse(COMVERSION ver, COMDualStringArray dsa, int auth_hint, Guid ipid_rem_unknown)
    {
        Version = new(ver.MajorVersion, ver.MinorVersion);
        StringBindings = (dsa?.StringBindings ?? new List<COMStringBinding>()).AsReadOnly();
        SecurityBindings = (dsa?.SecurityBindings ?? new List<COMSecurityBinding>()).AsReadOnly();
        AuthenticationHint = auth_hint;
        IpidRemUnknown = ipid_rem_unknown;
    }

    public string FindAlpcBinding()
    {
        var ret = StringBindings.FirstOrDefault(b => b.TowerId == RpcTowerId.LRPC)?.NetworkAddr;
        if (!string.IsNullOrEmpty(ret))
            return ret;

        // Generally the remote resolver doesn't return ALPC binding information, so let's try and 
        // brute force it based on the PID in the IPID for the remote IUnknown.
        using var rpc_dir = NtDirectory.Open(@"\RPC Control", null, DirectoryAccessRights.Query, false);
        if (!rpc_dir.IsSuccess)
            throw new InvalidOperationException("Can't enumerate RPC object directory.");

        foreach (var entry in rpc_dir.Result.Query())
        {
            if (entry.NtType == NtType.GetTypeByType<NtAlpc>() && entry.Name.StartsWith("OLE"))
            {
                using var port = NtAlpcClient.Connect(entry.FullPath, null, null, 
                    AlpcMessageFlags.None, null, null, null, null, NtWaitTimeout.Infinite, false);
                if (!port.IsSuccess)
                    continue;
                if (port.Result.ServerProcessId == ProcessId)
                    return entry.FullPath;
            }
        }
        throw new InvalidOperationException("Can't find ALPC port hosted by process.");
    }
}