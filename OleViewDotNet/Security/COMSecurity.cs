//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
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
using NtApiDotNet.Forms;
using NtApiDotNet.Win32;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Processes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OleViewDotNet.Security;

public static class COMSecurity
{
    internal static void SetupSecurityDescriptorControl(SecurityDescriptorViewerControl control, SecurityDescriptor sd, bool access)
    {
        SetupSecurityDescriptorControl(control, new COMSecurityDescriptor(sd), access);
    }

    internal static void SetupSecurityDescriptorControl(SecurityDescriptorViewerControl control, COMSecurityDescriptor sd, bool access)
    {
        AccessMask valid_access = access ? 0x7 : 0x1F;
        if (sd.HasContainerAccess)
        {
            valid_access |= access ? 0x20 : 0x60;
        }

        control.SetSecurityDescriptor(sd.SecurityDescriptor, typeof(COMAccessRights), new GenericMapping()
        {
            GenericExecute = valid_access,
            GenericRead = valid_access,
            GenericWrite = valid_access,
            GenericAll = valid_access
        }, valid_access);
    }

    public static void ViewSecurity(COMRegistry registry, string name, COMSecurityDescriptor sd, bool access)
    {
        if (sd is null)
            return;

        SecurityDescriptorViewerControl control = new();
        EntryPoint.GetMainForm(registry).HostControl(control, name);
        SetupSecurityDescriptorControl(control, sd, access);
    }

    public static void ViewSecurity(COMRegistry registry, COMAppIDEntry appid, bool access)
    {
        ViewSecurity(registry, $"{appid.Name} {(access ? "Access" : "Launch")}",
                access ? appid.AccessPermission : appid.LaunchPermission, access);
    }

    private static COMSecurityDescriptor GetSecurityPermissions(COMSD sdtype)
    {
        IntPtr sd = IntPtr.Zero;
        try
        {
            int hr = NativeMethods.CoGetSystemSecurityPermissions(sdtype, out sd);
            if (hr != 0)
            {
                throw new Win32Exception(hr);
            }

            return new(new SecurityDescriptor(sd));
        }
        finally
        {
            if (sd != IntPtr.Zero)
            {
                NativeMethods.LocalFree(sd);
            }
        }
    }

    public static COMSecurityDescriptor GetDefaultLaunchPermissions()
    {
        return GetSecurityPermissions(COMSD.SD_LAUNCHPERMISSIONS);
    }

    public static COMSecurityDescriptor GetDefaultAccessPermissions()
    {
        return GetSecurityPermissions(COMSD.SD_ACCESSPERMISSIONS);
    }

    public static COMSecurityDescriptor GetDefaultLaunchRestrictions()
    {
        return GetSecurityPermissions(COMSD.SD_LAUNCHRESTRICTIONS);
    }

    public static COMSecurityDescriptor GetDefaultAccessRestrictions()
    {
        return GetSecurityPermissions(COMSD.SD_ACCESSRESTRICTIONS);
    }

    public static TokenIntegrityLevel GetILForSD(COMSecurityDescriptor sd)
    {
        return sd?.SecurityDescriptor.IntegrityLevel ?? TokenIntegrityLevel.Medium;
    }

    public static COMSecurityDescriptor GetAccessPermission(ICOMAccessSecurity obj)
    {
        if (obj is COMProcessEntry process)
        {
            return process.AccessPermissions;
        }
        else if (obj is COMAppIDEntry || obj is COMCLSIDEntry)
        {
            COMAppIDEntry appid = obj as COMAppIDEntry;
            if (appid is null && obj is COMCLSIDEntry clsid)
            {
                appid = clsid.AppIDEntry;
                if (appid is null)
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

    public static COMSecurityDescriptor GetLaunchPermission(ICOMAccessSecurity obj)
    {
        if (obj is COMAppIDEntry || obj is COMCLSIDEntry)
        {
            COMAppIDEntry appid = obj as COMAppIDEntry;
            if (appid is null && obj is COMCLSIDEntry clsid)
            {
                appid = clsid.AppIDEntry;
                if (appid is null)
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

    private static bool SDHasAllowedAce(COMSecurityDescriptor desc, bool allow_null_dacl, Func<Ace, bool> check_func)
    {
        if (desc is null)
        {
            return allow_null_dacl;
        }

        var sd = desc.SecurityDescriptor;

        try
        {
            if (allow_null_dacl && (sd.Dacl is null || sd.Dacl.NullAcl))
            {
                return true;
            }
            foreach (var ace in sd.Dacl)
            {
                if (ace.Type == AceType.Allowed)
                {
                    if (check_func(ace))
                    {
                        return true;
                    }
                }
            }
        }
        catch (NtException)
        {
        }
        return false;
    }

    public static bool SDHasAC(COMSecurityDescriptor sd)
    {
        SidIdentifierAuthority authority = new(SecurityAuthority.Package);
        return SDHasAllowedAce(sd, false, ace => ace.Sid.Authority.Equals(authority));
    }

    public static bool SDHasRemoteAccess(COMSecurityDescriptor sd)
    {
        return SDHasAllowedAce(sd, true, a => a.Mask == COMAccessRights.Execute ||
            (a.Mask & (COMAccessRights.ExecuteRemote | COMAccessRights.ActivateRemote)) != 0);
    }

    public static IEnumerable<int> GetSessionIds()
    {
        return Win32Utils.GetConsoleSessions().Select(c => c.SessionId).ToArray();
    }

    public static Sid UserToSid(string username)
    {
        try
        {
            return NtSecurity.LookupAccountName(username);
        }
        catch (NtException)
        {
            return new Sid(username);
        }
    }

    internal static int GetSDHashCode(this COMSecurityDescriptor sd)
    {
        if (sd is null)
            return 0;
        return sd.ToBase64().GetHashCode();
    }

    internal static bool SDIsEqual(this COMSecurityDescriptor left, COMSecurityDescriptor right)
    {
        string left_str = left?.ToBase64();
        string right_str = right?.ToBase64();
        return left_str == right_str;
    }

    internal static bool IsAccessGranted(this COMAccessRights maximum_rights, COMAccessRights desired_access)
    {
        // If old style SD then all accesses are granted.
        if (maximum_rights == COMAccessRights.Execute)
        {
            return true;
        }

        return (maximum_rights & desired_access) == desired_access;
    }
}
