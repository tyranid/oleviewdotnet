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
using System;
using System.Collections.Generic;

namespace OleViewDotNet
{
    public interface ICOMAccessSecurity
    {
        string DefaultAccessPermission { get; }
        string DefaultLaunchPermission { get; }
    }

    public class COMAccessCheck : IDisposable
    {
        private readonly Dictionary<string, bool> m_access_cache;
        private readonly Dictionary<string, bool> m_launch_cache;
        private readonly NtToken m_access_token;
        private readonly string m_principal;
        private readonly COMAccessRights m_access_rights;
        private readonly COMAccessRights m_launch_rights;
        private readonly bool m_ignore_default;

        public static string GetAccessPermission(ICOMAccessSecurity obj)
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

        public static string GetLaunchPermission(ICOMAccessSecurity obj)
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
            string principal,
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

        public bool AccessCheck(
            ICOMAccessSecurity obj)
        {
            if (obj == null)
            {
                return false;
            }

            string launch_sddl = m_ignore_default ? string.Empty : obj.DefaultLaunchPermission;
            string access_sddl = m_ignore_default ? string.Empty : obj.DefaultAccessPermission;
            bool check_launch = true;
            string principal = m_principal;

            if (obj is COMProcessEntry process)
            {
                access_sddl = process.AccessPermissions;
                principal = process.UserSid;
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
                    launch_sddl = appid.LaunchPermission;
                }

                if (appid.HasAccessPermission)
                {
                    access_sddl = appid.AccessPermission;
                }
            }
            else if (obj is COMRuntimeClassEntry runtime_class)
            {
                if (runtime_class.HasPermission)
                {
                    launch_sddl = runtime_class.Permissions;
                }
                else if (runtime_class.ActivationType == ActivationType.OutOfProcess && runtime_class.HasServerPermission)
                {
                    launch_sddl = runtime_class.ServerPermissions;
                }
                else if (runtime_class.TrustLevel == TrustLevel.PartialTrust)
                {
                    launch_sddl = COMRuntimeClassEntry.DefaultActivationPermission;
                }
                else
                {
                    // Set to denied access.
                    launch_sddl = "O:SYG:SYD:";
                }
                access_sddl = launch_sddl;
            }
            else if (obj is COMRuntimeServerEntry runtime_server)
            {
                if (runtime_server.HasPermission)
                {
                    launch_sddl = runtime_server.Permissions;
                }
                else
                {
                    launch_sddl = "O:SYG:SYD:";
                }
                access_sddl = launch_sddl;
            }
            else
            {
                return false;
            }

            if (!m_access_cache.ContainsKey(access_sddl))
            {
                if (m_access_rights == 0)
                {
                    m_access_cache[access_sddl] = true;
                }
                else
                {
                    m_access_cache[access_sddl] = COMSecurity.IsAccessGranted(access_sddl, 
                        principal, m_access_token, false, false, m_access_rights);
                }
            }

            if (check_launch && !m_launch_cache.ContainsKey(launch_sddl))
            {
                if (m_launch_rights == 0)
                {
                    m_launch_cache[launch_sddl] = true;
                }
                else
                {
                    m_launch_cache[launch_sddl] = COMSecurity.IsAccessGranted(launch_sddl, principal, m_access_token,
                        true, true, m_launch_rights);
                }
            }

            if (m_access_cache[access_sddl] && (!check_launch || m_launch_cache[launch_sddl]))
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
}
