//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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
using OleViewDotNet.Interop;
using OleViewDotNet.Marshaling;
using System;

namespace OleViewDotNet.Security;

public sealed class COMAuthInfo
{
    public RpcAuthnService AuthnSvc { get; set; }
    public string ServerPrincName { get; set; }
    public RPC_AUTHN_LEVEL AuthnLevel { get; set; }
    public RPC_IMP_LEVEL ImpersonationLevel { get; set; }
    public RPC_C_QOS_CAPABILITIES Capabilities { get; set; }
    public COMCredentials AuthIdentityData { get; set; }

    public COMAuthInfo()
    {
        AuthnSvc = RpcAuthnService.GSS_Negotiate;
        AuthnLevel = RPC_AUTHN_LEVEL.PKT_PRIVACY;
        ImpersonationLevel = RPC_IMP_LEVEL.IMPERSONATE;
    }

    internal SafeStructureInOutBuffer<COAUTHINFO> ToBuffer(DisposableList list)
    {
        var auth_id = AuthIdentityData?.ToBuffer(list);
        if (auth_id is not null)
            list.Add(auth_id);

        COAUTHINFO auth_info = new()
        {
            dwAuthnSvc = AuthnSvc,
            dwAuthzSvc = 0,
            pwszServerPrincName = string.IsNullOrEmpty(ServerPrincName) ? null : ServerPrincName,
            dwAuthnLevel = AuthnLevel,
            dwImpersonationLevel = ImpersonationLevel,
            pAuthIdentityData = auth_id?.DangerousGetHandle() ?? IntPtr.Zero,
            dwCapabilities = Capabilities
        };
        return auth_info.ToBuffer();
    }
}