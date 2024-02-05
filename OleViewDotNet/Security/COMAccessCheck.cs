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

public class COMAccessCheck : IDisposable
{
    private readonly Dictionary<string, bool> m_access_cache;
    private readonly Dictionary<string, bool> m_launch_cache;
    private readonly NtToken m_access_token;
    private readonly COMSid m_principal;
    private readonly COMAccessRights m_access_rights;
    private readonly COMAccessRights m_launch_rights;
    private readonly bool m_ignore_default;

    private static string GetCacheKey(COMSid principal, SecurityDescriptor sd)
    {
        return $"{principal}:{sd?.ToBase64() ?? string.Empty}";
    }

    public static SecurityDescriptor GetAccessPermission(ICOMAccessSecurity obj)
    {
        if (obj is COMProcessEntry process)
        {
            return process.AccessPermissions;
        }
        else if (obj is COMAppIDEntry || obj is COMCLSIDEntry)
        {
            COMAppIDEntry appid = obj as COMAppIDEntry;
            if (appid == null && obj is COMCLSIDEntry clsid)
            {
                appid = clsid.AppIDEntry;
                if (appid == null)
                {
                    throw new ArgumentException("No AppID available for class");
                }
            }

            if (appid.HasAccessPermission)
            {
                return appid.AccessPermission;
            }
            throw new ArgumentException("AppID doesn't have an access permission");
        }

        throw new ArgumentException("Can't get access permission for object");
    }

    public static SecurityDescriptor GetLaunchPermission(ICOMAccessSecurity obj)
    {
        if (obj is COMAppIDEntry || obj is COMCLSIDEntry)
        {
            COMAppIDEntry appid = obj as COMAppIDEntry;
            if (appid == null && obj is COMCLSIDEntry clsid)
            {
                appid = clsid.AppIDEntry;
                if (appid == null)
                {
                    throw new ArgumentException("No AppID available for class");
                }
            }

            if (appid.HasLaunchPermission)
            {
                return appid.LaunchPermission;
            }
            throw new ArgumentException("AppID doesn't have an launch permission");
        }
        else if (obj is COMRuntimeClassEntry runtime_class)
        {
            if (runtime_class.HasPermission)
            {
                return runtime_class.Permissions;
            }
            else if (runtime_class.ActivationType == ActivationType.OutOfProcess && runtime_class.HasServerPermission)
            {
                return runtime_class.ServerPermissions;
            }
            throw new ArgumentException("RuntimeClass doesn't have an launch permission");
        }
        else if (obj is COMRuntimeServerEntry runtime_server)
        {
            if (runtime_server.HasPermission)
            {
                return runtime_server.Permissions;
            }
            throw new ArgumentException("RuntimeServer doesn't have an launch permission");
        }
        throw new ArgumentException("Can't get launch permission for object");
    }

    public COMAccessCheck(NtToken token,
        COMSid principal,
        COMAccessRights access_rights,
        COMAccessRights launch_rights,
        bool ignore_default)
    {
        m_access_cache = new Dictionary<string, bool>();
        m_launch_cache = new Dictionary<string, bool>();
        m_access_cache[string.Empty] = false;
        m_launch_cache[string.Empty] = false;
        m_access_token = token.DuplicateToken(SecurityImpersonationLevel.Identification);
        m_principal = principal;
        m_access_rights = access_rights;
        m_launch_rights = launch_rights;
        m_ignore_default = ignore_default;
    }

    public bool AccessCheck(ICOMAccessSecurity obj)
    {
        if (obj == null)
        {
            return false;
        }

        SecurityDescriptor launch_sd = m_ignore_default ? null : obj.DefaultLaunchPermission;
        SecurityDescriptor access_sd = m_ignore_default ? null : obj.DefaultAccessPermission;
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
            if (appid == null && obj is COMCLSIDEntry clsid)
            {
                appid = clsid.AppIDEntry;
                if (appid == null)
                {
                    return false;
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
                    if (appid.LocalService.UserName == "LocalSystem")
                        launch_principal = new COMSid(KnownSids.LocalSystem);
                    else
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
            return false;
        }

        access_principal ??= COMSid.CurrentUser;

        string access_str = GetCacheKey(access_principal, access_sd);
        if (!m_access_cache.ContainsKey(access_str))
        {
            if (m_access_rights == 0)
            {
                m_access_cache[access_str] = true;
            }
            else
            {
                m_access_cache[access_str] = COMSecurity.IsAccessGranted(access_sd,
                    access_principal, m_access_token, false, false, m_access_rights);
            }
        }

        string launch_str = GetCacheKey(launch_principal, launch_sd);
        if (check_launch && !m_launch_cache.ContainsKey(launch_str))
        {
            if (m_launch_rights == 0)
            {
                m_launch_cache[launch_str] = true;
            }
            else
            {
                m_launch_cache[launch_str] = COMSecurity.IsAccessGranted(launch_sd, launch_principal, m_access_token,
                    true, true, m_launch_rights);
            }
        }

        if (m_access_cache[access_str] && (!check_launch || m_launch_cache[launch_str]))
        {
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        m_access_token.Dispose();
    }
}
