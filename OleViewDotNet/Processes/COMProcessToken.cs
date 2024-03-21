//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017
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
using OleViewDotNet.Security;
using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet.Processes;

public class COMProcessToken
{
    public string User { get; }
    public COMSid UserSid { get; }
    public TokenIntegrityLevel IntegrityLevel { get; }
    public bool Sandboxed { get; }
    public bool AppContainer { get; }
    public bool Restricted { get; }
    public bool Elevated { get; }
    public bool LowPrivilegeAppContainer { get; }
    public AppxPackageName PackageName { get; }
    public ulong PackageHostId { get; }
    public IEnumerable<AppModelPolicy_PolicyValue> AppModelPolicies { get; }

    public COMProcessToken()
    {
        User = "UNKNOWN";
        UserSid = null;
        IntegrityLevel = TokenIntegrityLevel.Medium;
    }

    public COMProcessToken(NtProcess process) : this()
    {
        PackageName = AppxPackageName.FromProcess(process);

        using var result = NtToken.OpenProcessToken(process, TokenAccessRights.Query, false);
        if (result.IsSuccess)
        {
            NtToken token = result.Result;
            User = token.User.Sid.Name;
            UserSid = new COMSid(token.User.Sid);
            IntegrityLevel = token.IntegrityLevel;
            Sandboxed = token.IsSandbox;
            AppContainer = token.AppContainer;
            Restricted = token.Restricted;
            Elevated = token.Elevated;
            LowPrivilegeAppContainer = token.LowPrivilegeAppContainer;
            var pkghostid = token.GetSecurityAttributeByName("WIN://PKGHOSTID");
            if (pkghostid is not null && pkghostid.ValueType == ClaimSecurityValueType.UInt64 && pkghostid.Values.Any())
            {
                PackageHostId = (ulong)pkghostid.Values.First();
            }
            AppModelPolicies = token.AppModelPolicyDictionary.Values.ToList().AsReadOnly();
        }
    }

    public override string ToString()
    {
        return $"{User}";
    }
}
