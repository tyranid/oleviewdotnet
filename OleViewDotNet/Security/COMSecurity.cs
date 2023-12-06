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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OleViewDotNet.Security;

public static class COMSecurity
{
    public static void ViewSecurity(COMRegistry registry, string name, SecurityDescriptor sd, bool access)
    {
        if (sd == null)
            return;

        AccessMask valid_access = access ? 0x7 : 0x1F;

        SecurityDescriptorViewerControl control = new();
        EntryPoint.GetMainForm(registry).HostControl(control, name);
        control.SetSecurityDescriptor(sd, typeof(COMAccessRights), new GenericMapping()
        {
            GenericExecute = valid_access,
            GenericRead = valid_access,
            GenericWrite = valid_access,
            GenericAll = valid_access
        }, valid_access);
    }

    public static void ViewSecurity(COMRegistry registry, COMAppIDEntry appid, bool access)
    {
        ViewSecurity(registry, string.Format("{0} {1}", appid.Name, access ? "Access" : "Launch"),
                access ? appid.AccessPermission : appid.LaunchPermission, access);
    }

    public static bool IsAccessGranted(SecurityDescriptor sd, string principal, NtToken token, bool launch, bool check_il, COMAccessRights desired_access)
    {
        try
        {
            if (sd == null)
                return false;

            sd = sd.Clone();

            if (check_il || !sd.HasMandatoryLabelAce)
            {
                sd.AddMandatoryLabel(TokenIntegrityLevel.Medium, MandatoryLabelPolicy.NoExecuteUp);
            }

            if (!GetGrantedAccess(sd, principal, token, launch, out COMAccessRights maximum_rights))
            {
                return false;
            }

            // If old style SD then all accesses are granted.
            if (maximum_rights == COMAccessRights.Execute)
            {
                return true;
            }

            return (maximum_rights & desired_access) == desired_access;
        }
        catch (Win32Exception)
        {
            return false;
        }
    }

    private static bool GetGrantedAccess(SecurityDescriptor sd, string principal, NtToken token, bool launch, out COMAccessRights maximum_rights)
    {
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
            maximum_rights = mapping.GenericExecute.ToSpecificAccess<COMAccessRights>();
            return true;
        }

        AccessMask mask;

        if (!string.IsNullOrWhiteSpace(principal))
        {
            mask = NtSecurity.GetMaximumAccess(sd, token, new Sid(principal), mapping);
        }
        else
        {
            mask = NtSecurity.GetMaximumAccess(sd, token, mapping);
        }

        mask &= 0xFFFF;

        maximum_rights = mask.ToSpecificAccess<COMAccessRights>();

        return mask != 0;
    }

    private static SecurityDescriptor GetSecurityPermissions(COMSD sdtype)
    {
        IntPtr sd = IntPtr.Zero;
        try
        {
            int hr = NativeMethods.CoGetSystemSecurityPermissions(sdtype, out sd);
            if (hr != 0)
            {
                throw new Win32Exception(hr);
            }

            return new SecurityDescriptor(sd);
        }
        finally
        {
            if (sd != IntPtr.Zero)
            {
                NativeMethods.LocalFree(sd);
            }
        }
    }

    public static SecurityDescriptor GetDefaultLaunchPermissions()
    {
        return GetSecurityPermissions(COMSD.SD_LAUNCHPERMISSIONS);
    }

    public static SecurityDescriptor GetDefaultAccessPermissions()
    {
        return GetSecurityPermissions(COMSD.SD_ACCESSPERMISSIONS);
    }

    public static SecurityDescriptor GetDefaultLaunchRestrictions()
    {
        return GetSecurityPermissions(COMSD.SD_LAUNCHRESTRICTIONS);
    }

    public static SecurityDescriptor GetDefaultAccessRestrictions()
    {
        return GetSecurityPermissions(COMSD.SD_ACCESSRESTRICTIONS);
    }

    public static TokenIntegrityLevel GetILForSD(SecurityDescriptor sd)
    {
        return sd?.IntegrityLevel ?? TokenIntegrityLevel.Medium;
    }

    private static bool SDHasAllowedAce(SecurityDescriptor sd, bool allow_null_dacl, Func<Ace, bool> check_func)
    {
        if (sd == null)
        {
            return allow_null_dacl;
        }

        try
        {
            if (allow_null_dacl && (sd.Dacl == null || sd.Dacl.NullAcl))
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

    public static bool SDHasAC(SecurityDescriptor sd)
    {
        SidIdentifierAuthority authority = new(SecurityAuthority.Package);
        return SDHasAllowedAce(sd, false, ace => ace.Sid.Authority.Equals(authority));
    }

    public static bool SDHasRemoteAccess(SecurityDescriptor sd)
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

    internal static int GetSDHashCode(this SecurityDescriptor sd)
    {
        if (sd == null)
            return 0;
        return sd.ToBase64().GetHashCode();
    }

    internal static bool SDIsEqual(this SecurityDescriptor left, SecurityDescriptor right)
    {
        string left_str = left?.ToBase64();
        string right_str = right?.ToBase64();
        return left_str == right_str;
    }
}
