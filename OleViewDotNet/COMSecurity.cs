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
using NtApiDotNet.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace OleViewDotNet
{
    //[Flags]
    //public enum SecurityInformation
    //{
    //    Owner = 1,
    //    Group = 2,
    //    Dacl = 4,
    //    Label = 0x10,
    //    All = Owner | Group | Dacl | Label
    //}

    [Flags]
    public enum COMAccessRights : uint
    {
        Execute = 1,
        ExecuteLocal = 2,
        ExecuteRemote = 4,
        ActivateLocal = 8,
        ActivateRemote = 16,
    }

    public static class COMSecurity
    {
        public static void ViewSecurity(IWin32Window parent, string name, string sddl, bool access)
        {
            if (!String.IsNullOrWhiteSpace(sddl))
            {
                SecurityDescriptor sd = new SecurityDescriptor(sddl);
                AccessMask valid_access = access ? 0x7 : 0x1F;
                Win32Utils.EditSecurity(parent != null ? parent.Handle : IntPtr.Zero, name, sd, typeof(COMAccessRights), valid_access, new GenericMapping());
            }
        }

        public static void ViewSecurity(IWin32Window parent, COMAppIDEntry appid, bool access)
        {
            ViewSecurity(parent, String.Format("{0} {1}", appid.Name, access ? "Access" : "Launch"),
                    access ? appid.AccessPermission : appid.LaunchPermission, access);
        }

        //const uint SDDL_REVISION_1 = 1;

        //[DllImport("advapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true, SetLastError = true)]
        //private extern static bool ConvertSecurityDescriptorToStringSecurityDescriptor(byte[] sd, uint rev, SecurityInformation secinfo, out IntPtr str, out int length);

        //[DllImport("advapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true, SetLastError = true)]
        //private extern static bool ConvertStringSecurityDescriptorToSecurityDescriptor(string StringSecurityDescriptor, 
        //    uint StringSDRevision, out IntPtr SecurityDescriptor, out int SecurityDescriptorSize);

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

        internal static bool IsAccessGranted(string sddl, string principal, NtToken token, bool launch, bool check_il, COMAccessRights desired_access)
        {
            try
            {
                COMAccessRights maximum_rights;

                if (check_il)
                {
                    string sacl = GetSaclForSddl(sddl);
                    if (String.IsNullOrEmpty(sacl))
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
            GenericMapping mapping = new GenericMapping();
            mapping.GenericExecute = (uint)(COMAccessRights.Execute | COMAccessRights.ExecuteLocal | COMAccessRights.ExecuteRemote);
            if (launch)
            {
                mapping.GenericExecute = mapping.GenericExecute | (uint)(COMAccessRights.ActivateLocal | COMAccessRights.ActivateRemote);
            }

            // If SD is only a NULL DACL we get maximum rights.
            if (sddl == "D:NO_ACCESS_CONTROL")
            {
                maximum_rights = mapping.GenericExecute.ToSpecificAccess<COMAccessRights>();
                return true;
            }

            if (!String.IsNullOrWhiteSpace(principal))
            {
                maximum_rights = NtSecurity.GetMaximumAccess(new SecurityDescriptor(sddl), token, new Sid(principal), mapping).ToSpecificAccess<COMAccessRights>();
            }
            else
            {
                maximum_rights = NtSecurity.GetMaximumAccess(new SecurityDescriptor(sddl), token, mapping).ToSpecificAccess<COMAccessRights>();
            }

            return maximum_rights != 0;
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

        public static TokenIntegrityLevel GetILFromSid(SecurityIdentifier sid)
        {
            int last_index = sid.Value.LastIndexOf('-');
            int il;
            if (int.TryParse(sid.Value.Substring(last_index + 1), out il))
            {
                return (TokenIntegrityLevel)il;
            }

            return TokenIntegrityLevel.Medium;
        }

        public static TokenIntegrityLevel GetILForSD(string sddl)
        {
            if (String.IsNullOrWhiteSpace(sddl))
            {
                return TokenIntegrityLevel.Medium;
            }

            string sacl = GetSaclForSddl(sddl);
            if (!sacl.StartsWith("S:", StringComparison.OrdinalIgnoreCase))
            {
                return TokenIntegrityLevel.Medium;
            }

            Regex label_re = new Regex(@"\(ML;;[^;]*;;;([^)]+)\)");
            Match m = label_re.Match(sacl);
            if (!m.Success || m.Groups.Count < 2)
            {
                return TokenIntegrityLevel.Medium;
            }

            SecurityIdentifier sid = new SecurityIdentifier(m.Groups[1].Value);
            return GetILFromSid(sid);
        }

        public static bool SDHasAC(string sddl)
        {
            if (String.IsNullOrWhiteSpace(sddl))
            {
                return false;
            }

            RawSecurityDescriptor sd = new RawSecurityDescriptor(sddl);

            foreach (var ace in sd.DiscretionaryAcl)
            {
                CommonAce common_ace = ace as CommonAce;
                if (common_ace != null)
                {
                    if (common_ace.AceType == System.Security.AccessControl.AceType.AccessAllowed 
                        && common_ace.SecurityIdentifier.Value.StartsWith("S-1-15-"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool SDHasRemoteAccess(string sddl)
        {
            if (String.IsNullOrWhiteSpace(sddl))
            {
                // Asssume defaults give _someone_ remote access.
                return true;
            }

            RawSecurityDescriptor sd = new RawSecurityDescriptor(sddl);

            foreach (var ace in sd.DiscretionaryAcl)
            {
                CommonAce common_ace = ace as CommonAce;
                if (common_ace != null)
                {
                    COMAccessRights access = (COMAccessRights)(uint)common_ace.AccessMask;
                    if ((access == COMAccessRights.Execute) 
                        || (access & (COMAccessRights.ExecuteRemote | COMAccessRights.ActivateRemote)) != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
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

        public static string UserToSid(string username)
        {
            SecurityIdentifier sid;
            if (username.StartsWith("S-", StringComparison.OrdinalIgnoreCase))
            {
                sid = new SecurityIdentifier(username);
            }
            else
            {
                NTAccount account = new NTAccount(username);
                sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
            }
            return sid.Value;
        }
    }
}
