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
using OleViewDotNet.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace OleViewDotNet
{
    [Flags]
    public enum COMAccessRights : uint
    {
        Execute = 1,
        ExecuteLocal = 2,
        ExecuteRemote = 4,
        ActivateLocal = 8,
        ActivateRemote = 16,
        ExecuteContainer = 32,
        ActivateContainer = 64,
        GenericRead = GenericAccessRights.GenericRead,
        GenericWrite = GenericAccessRights.GenericWrite,
        GenericExecute = GenericAccessRights.GenericExecute,
        GenericAll = GenericAccessRights.GenericAll,
    }

    public static class COMSecurity
    {
        public static void ViewSecurity(COMRegistry registry, string name, string sddl, bool access)
        {
            if (!string.IsNullOrWhiteSpace(sddl))
            {
                SecurityDescriptor sd = new SecurityDescriptor(sddl);
                AccessMask valid_access = access ? 0x7 : 0x1F;

                SecurityDescriptorViewerControl control = new SecurityDescriptorViewerControl();
                EntryPoint.GetMainForm(registry).HostControl(control, name);
                control.SetSecurityDescriptor(sd, typeof(COMAccessRights), new GenericMapping()
                    { GenericExecute = valid_access, GenericRead = valid_access,
                    GenericWrite = valid_access, GenericAll = valid_access }, valid_access);
            }
        }

        public static void ViewSecurity(COMRegistry registry, COMAppIDEntry appid, bool access)
        {
            ViewSecurity(registry, string.Format("{0} {1}", appid.Name, access ? "Access" : "Launch"),
                    access ? appid.AccessPermission : appid.LaunchPermission, access);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true)]
        private extern static IntPtr LocalFree(IntPtr hMem);

        enum COMSD
        {
            SD_LAUNCHPERMISSIONS = 0,       // Machine wide launch permissions
            SD_ACCESSPERMISSIONS = 1,       // Machine wide acesss permissions
            SD_LAUNCHRESTRICTIONS = 2,      // Machine wide launch limits
            SD_ACCESSRESTRICTIONS = 3       // Machine wide access limits
        }

        [DllImport("ole32.dll")]
        private extern static int CoGetSystemSecurityPermissions(COMSD comSDType, out IntPtr ppSD);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true, SetLastError = true)]
        private extern static int GetSecurityDescriptorLength(IntPtr sd);
        
        internal static string GetSaclForSddl(string sddl)
        {
            return GetStringSDForSD(GetSDForStringSD(sddl), SecurityInformation.Label);
        }

        public static bool IsAccessGranted(string sddl, string principal, NtToken token, bool launch, bool check_il, COMAccessRights desired_access)
        {
            try
            {
                COMAccessRights maximum_rights;

                if (check_il)
                {
                    string sacl = GetSaclForSddl(sddl);
                    if (string.IsNullOrEmpty(sacl))
                    {
                        // Add medium NX SACL
                        sddl += "S:(ML;;NX;;;ME)";
                    }
                }

                if (!GetGrantedAccess(sddl, principal, token, launch, out maximum_rights))
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

        private static bool GetGrantedAccess(string sddl, string principal, NtToken token, bool launch, out COMAccessRights maximum_rights)
        {
            GenericMapping mapping = new GenericMapping
            {
                GenericExecute = (uint)(COMAccessRights.Execute | COMAccessRights.ExecuteLocal | COMAccessRights.ExecuteRemote | COMAccessRights.ExecuteContainer)
            };
            if (launch)
            {
                mapping.GenericExecute |= (uint)(COMAccessRights.ActivateLocal | COMAccessRights.ActivateRemote | COMAccessRights.ActivateContainer);
            }

            // If SD is only a NULL DACL we get maximum rights.
            if (sddl == "D:NO_ACCESS_CONTROL")
            {
                maximum_rights = mapping.GenericExecute.ToSpecificAccess<COMAccessRights>();
                return true;
            }

            AccessMask mask;

            if (!string.IsNullOrWhiteSpace(principal))
            {
                mask = NtSecurity.GetMaximumAccess(new SecurityDescriptor(sddl), token, new Sid(principal), mapping);
            }
            else
            {
                mask = NtSecurity.GetMaximumAccess(new SecurityDescriptor(sddl), token, mapping);
            }

            mask &= 0xFFFF;

            maximum_rights = mask.ToSpecificAccess<COMAccessRights>();

            return mask != 0;
        }

        private static string GetSecurityPermissions(COMSD sdtype)
        {
            IntPtr sd = IntPtr.Zero;
            try
            {
                int hr = CoGetSystemSecurityPermissions(sdtype, out sd);
                if (hr != 0)
                {
                    throw new Win32Exception(hr);
                }

                int length = GetSecurityDescriptorLength(sd);
                byte[] ret = new byte[length];
                Marshal.Copy(sd, ret, 0, length);
                return GetStringSDForSD(ret);
            }
            finally
            {
                if (sd != IntPtr.Zero)
                {
                    LocalFree(sd);
                }
            }
        }

        public static string GetDefaultLaunchPermissions()
        {
            return GetSecurityPermissions(COMSD.SD_LAUNCHPERMISSIONS);
        }

        public static string GetDefaultAccessPermissions()
        {
            return GetSecurityPermissions(COMSD.SD_ACCESSPERMISSIONS);
        }

        public static string GetDefaultLaunchRestrictions()
        {
            return GetSecurityPermissions(COMSD.SD_LAUNCHRESTRICTIONS);
        }

        public static string GetDefaultAccessRestrictions()
        {
            return GetSecurityPermissions(COMSD.SD_ACCESSRESTRICTIONS);
        }

        public static string GetStringSDForSD(byte[] sd, SecurityInformation info)
        {
            try
            {
                if (sd == null || sd.Length == 0)
                {
                    return string.Empty;
                }

                return new SecurityDescriptor(sd).ToSddl(info);
            }
            catch (NtException)
            {
                return string.Empty;
            }
        }

        public static string GetStringSDForSD(byte[] sd)
        {
            return GetStringSDForSD(sd, SecurityInformation.AllBasic);
        }

        public static byte[] GetSDForStringSD(string sddl)
        {
            try
            {
                return new SecurityDescriptor(sddl).ToByteArray();
            }
            catch (NtException)
            {
                return new byte[0];
            }
        }

        public static TokenIntegrityLevel GetILForSD(string sddl)
        {
            if (string.IsNullOrWhiteSpace(sddl))
            {
                return TokenIntegrityLevel.Medium;
            }

            try
            {
                SecurityDescriptor sd = new SecurityDescriptor(sddl);
                return sd.IntegrityLevel;
            }
            catch (NtException)
            {
                return TokenIntegrityLevel.Medium;
            }
        }

        private static bool SDHasAllowedAce(string sddl, bool allow_null_dacl, Func<Ace, bool> check_func)
        {
            if (string.IsNullOrWhiteSpace(sddl))
            {
                return allow_null_dacl;
            }

            try
            {
                SecurityDescriptor sd = new SecurityDescriptor(sddl);
                if (allow_null_dacl && sd.Dacl == null && sd.Dacl.NullAcl)
                {
                    return true;
                }
                foreach (var ace in sd.Dacl)
                {
                    if (ace.Type == NtApiDotNet.AceType.Allowed)
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

        public static bool SDHasAC(string sddl)
        {
            SidIdentifierAuthority authority = new SidIdentifierAuthority(SecurityAuthority.Package);
            return SDHasAllowedAce(sddl, false, ace => ace.Sid.Authority.Equals(authority));
        }

        public static bool SDHasRemoteAccess(string sddl)
        {
            return SDHasAllowedAce(sddl, true, a => a.Mask == COMAccessRights.Execute || 
                (a.Mask & (COMAccessRights.ExecuteRemote | COMAccessRights.ActivateRemote)) != 0);
        }

        enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,              // User logged on to WinStation
            WTSConnected,           // WinStation connected to client
            WTSConnectQuery,        // In the process of connecting to client
            WTSShadow,              // Shadowing another WinStation
            WTSDisconnected,        // WinStation logged on without client
            WTSIdle,                // Waiting for client to connect
            WTSListen,              // WinStation is listening for connection
            WTSReset,               // WinStation is being reset
            WTSDown,                // WinStation is down due to error
            WTSInit,                // WinStation in initialization
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WTS_SESSION_INFO
        {
            public int SessionId;
            public IntPtr pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSEnumerateSessions(
                IntPtr hServer,
                int Reserved,
                int Version,
                out IntPtr ppSessionInfo,
                out int pCount);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern void WTSFreeMemory(IntPtr memory);

        public static IEnumerable<int> GetSessionIds()
        {
            List<int> sids = new List<int>();
            IntPtr pSessions = IntPtr.Zero;
            int dwSessionCount = 0;
            try
            {
                if (WTSEnumerateSessions(IntPtr.Zero, 0, 1, out pSessions, out dwSessionCount))
                {
                    IntPtr current = pSessions;
                    for (int i = 0; i < dwSessionCount; ++i)
                    {
                        WTS_SESSION_INFO session_info = (WTS_SESSION_INFO)Marshal.PtrToStructure(current, typeof(WTS_SESSION_INFO));
                        sids.Add(session_info.SessionId);
                        current += Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                    }
                }
            }
            finally
            {
                if (pSessions != IntPtr.Zero)
                {
                    WTSFreeMemory(pSessions);
                }
            }

            return sids;
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
    }
}
