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
using OleViewDotNet.Database;
using OleViewDotNet.Processes;
using System;
using System.Collections.Generic;

namespace OleViewDotNet.Security;

public sealed class COMAccessCheck : IDisposable
{
    private readonly Dictionary<string, COMAccessRights> m_access_cache;
    private readonly Dictionary<string, COMAccessRights> m_launch_cache;
    private readonly NtToken m_access_token;
    private readonly COMSid m_principal;
    private readonly COMAccessRights m_access_rights;
    private readonly COMAccessRights m_launch_rights;
    private readonly bool m_ignore_default;

    private static string GetCacheKey(COMSid principal, COMSecurityDescriptor sd)
    {
        return $"{principal}:{sd?.ToBase64() ?? string.Empty}";
    }

    private static COMAccessRights GetGrantedAccess(COMSecurityDescriptor desc, COMSid principal, NtToken token, bool launch)
    {
        if (desc is null)
            return 0;

        SecurityDescriptor sd = desc.SecurityDescriptor.Clone();

        if (launch || !sd.HasMandatoryLabelAce)
        {
            sd.AddMandatoryLabel(TokenIntegrityLevel.Medium, MandatoryLabelPolicy.NoExecuteUp);
        }

        GenericMapping mapping = new()
        {
            GenericExecute = (uint)(COMAccessRights.Execute | COMAccessRights.ExecuteLocal | COMAccessRights.ExecuteRemote | COMAccessRights.ExecuteContainer)
        };
        if (launch)
        {
            mapping.GenericExecute |= (uint)(COMAccessRights.ActivateLocal | COMAccessRights.ActivateRemote | COMAccessRights.ActivateContainer);
        }

        // If SD is only a NULL DACL we get maximum rights.
        if (sd.DaclNull)
        {
            return mapping.GenericExecute.ToSpecificAccess<COMAccessRights>();
        }

        AccessMask mask = NtSecurity.GetMaximumAccess(sd, token, principal?.Sid, mapping) & 0xFFFF;
        return mask.ToSpecificAccess<COMAccessRights>();
    }

    public COMAccessCheck(COMAccessToken token,
        COMSid principal,
        COMAccessRights access_rights,
        COMAccessRights launch_rights,
        bool ignore_default)
    {
        m_access_cache = new Dictionary<string, COMAccessRights>();
        m_launch_cache = new Dictionary<string, COMAccessRights>();
        m_access_token = token.Token.DuplicateToken(SecurityImpersonationLevel.Identification);
        m_principal = principal;
        m_access_rights = access_rights;
        m_launch_rights = launch_rights;
        m_ignore_default = ignore_default;
    }

    public COMAccessCheckResult GetMaximumAccess(ICOMAccessSecurity obj)
    {
        if (obj is null)
        {
            return default;
        }

        COMSecurityDescriptor launch_sd = m_ignore_default ? null : obj.DefaultLaunchPermission;
        COMSecurityDescriptor access_sd = m_ignore_default ? null : obj.DefaultAccessPermission;
        bool check_launch = true;
        COMSid access_principal = m_principal;
        COMSid launch_principal = null;

        if (obj is COMProcessEntry process)
        {
            access_sd = process.AccessPermissions;
            access_principal ??= process.UserSid;
            check_launch = false;
        }
        else if (obj is COMAppIDEntry || obj is COMCLSIDEntry)
        {
            COMAppIDEntry appid = obj as COMAppIDEntry;
            if (appid is null && obj is COMCLSIDEntry clsid)
            {
                appid = clsid.AppIDEntry;
                if (appid is null)
                {
                    return default;
                }
            }

            if (appid.HasLaunchPermission)
            {
                launch_sd = appid.LaunchPermission;
            }

            if (appid.HasAccessPermission)
            {
                access_sd = appid.AccessPermission;
            }

            if (appid.Flags.HasFlag(COMAppIDFlags.IUServerSelfSidInLaunchPermission))
            {
                if (appid.IsService)
                {
                    COMSid.TryParse(appid.LocalService.UserName, out launch_principal);
                }
                else if (appid.HasRunAs && !appid.IsInteractiveUser)
                {
                    COMSid.TryParse(appid.RunAs, out launch_principal);
                }
                else
                {
                    launch_principal = COMSid.CurrentUser;
                }
            }
        }
        else if (obj is COMRuntimeClassEntry runtime_class)
        {
            if (runtime_class.HasPermission)
            {
                launch_sd = runtime_class.Permissions;
            }
            else if (runtime_class.ActivationType == ActivationType.OutOfProcess && runtime_class.HasServerPermission)
            {
                launch_sd = runtime_class.ServerPermissions;
            }
            else if (runtime_class.TrustLevel == TrustLevel.PartialTrust)
            {
                launch_sd = new(COMRuntimeClassEntry.DefaultActivationPermission);
            }
            else
            {
                // Set to denied access.
                launch_sd = new("O:SYG:SYD:");
            }
            access_sd = launch_sd;
        }
        else if (obj is COMRuntimeServerEntry runtime_server)
        {
            if (runtime_server.HasPermission)
            {
                launch_sd = runtime_server.Permissions;
            }
            else
            {
                launch_sd = new("O:SYG:SYD:");
            }
            access_sd = launch_sd;
        }
        else
        {
            return default;
        }

        access_principal ??= COMSid.CurrentUser;

        string access_str = GetCacheKey(access_principal, access_sd);
        if (!m_access_cache.ContainsKey(access_str))
        {
            m_access_cache[access_str] = GetGrantedAccess(access_sd,
                access_principal, m_access_token, false);
        }

        string launch_str = GetCacheKey(launch_principal, launch_sd);
        if (check_launch && !m_launch_cache.ContainsKey(launch_str))
        {
            m_launch_cache[launch_str] = GetGrantedAccess(launch_sd, launch_principal, m_access_token,
                    true);
        }

        return new COMAccessCheckResult(m_access_cache[access_str], 
            check_launch ? m_launch_cache[launch_str] : 0, obj, 
            access_sd, launch_sd, check_launch);
    }

    public bool AccessCheck(ICOMAccessSecurity obj)
    {
        COMAccessCheckResult result = GetMaximumAccess(obj);
        if (!result.IsValid)
            return false;

        bool access_allowed = m_access_rights == 0 || result.IsAccessGranted(m_access_rights);
        bool launch_allowed = m_launch_rights == 0 || result.IsLaunchGranted(m_launch_rights);
        return access_allowed && launch_allowed;
    }

    public void Dispose()
    {
        m_access_token.Dispose();
    }
}
