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
    [StructLayout(LayoutKind.Sequential)]
    struct SiObjectInfo
    {
        public SiObjectInfoFlags dwFlags;
        public IntPtr hInstance;
        public IntPtr pszServerName;
        public IntPtr pszObjectName;
        public IntPtr pszPageTitle;
        public Guid guidObjectType;
    }

    enum SiObjectInfoFlags : uint
    {
        SI_EDIT_PERMS = 0x00000000, // always implied
        SI_EDIT_OWNER = 0x00000001,
        SI_EDIT_AUDITS = 0x00000002,
        SI_CONTAINER = 0x00000004,
        SI_READONLY = 0x00000008,
        SI_ADVANCED = 0x00000010,
        SI_RESET = 0x00000020, //equals to SI_RESET_DACL|SI_RESET_SACL|SI_RESET_OWNER
        SI_OWNER_READONLY = 0x00000040,
        SI_EDIT_PROPERTIES = 0x00000080,
        SI_OWNER_RECURSE = 0x00000100,
        SI_NO_ACL_PROTECT = 0x00000200,
        SI_NO_TREE_APPLY = 0x00000400,
        SI_PAGE_TITLE = 0x00000800,
        SI_SERVER_IS_DC = 0x00001000,
        SI_RESET_DACL_TREE = 0x00004000,
        SI_RESET_SACL_TREE = 0x00008000,
        SI_OBJECT_GUID = 0x00010000,
        SI_EDIT_EFFECTIVE = 0x00020000,
        SI_RESET_DACL = 0x00040000,
        SI_RESET_SACL = 0x00080000,
        SI_RESET_OWNER = 0x00100000,
        SI_NO_ADDITIONAL_PERMISSION = 0x00200000,
        SI_VIEW_ONLY = 0x00400000,
        SI_PERMS_ELEVATION_REQUIRED = 0x01000000,
        SI_AUDITS_ELEVATION_REQUIRED = 0x02000000,
        SI_OWNER_ELEVATION_REQUIRED = 0x04000000,
        SI_SCOPE_ELEVATION_REQUIRED = 0x08000000,
        SI_MAY_WRITE = 0x10000000, //not sure if user can write permission
        SI_ENABLE_EDIT_ATTRIBUTE_CONDITION = 0x20000000,
        SI_ENABLE_CENTRAL_POLICY = 0x40000000,
        SI_DISABLE_DENY_ACE = 0x80000000,
        SI_EDIT_ALL = (SI_EDIT_PERMS | SI_EDIT_OWNER | SI_EDIT_AUDITS)
    }

    struct SiAccess
    {
        public IntPtr pguid; // Guid
        public uint mask;
        public IntPtr pszName;
        public SiAccessFlags dwFlags;
    }

    enum SiAccessFlags
    {
        SI_ACCESS_SPECIFIC = 0x00010000,
        SI_ACCESS_GENERAL = 0x00020000,
        SI_ACCESS_CONTAINER = 0x00040000,
        SI_ACCESS_PROPERTY = 0x00080000,
    }

    [Flags]
    public enum SecurityInformation
    {
        Owner = 1,
        Group = 2,
        Dacl = 4,
        Label = 0x10,
        All = Owner | Group | Dacl | Label
    }

    public enum SecurityIntegrityLevel
    {
        Untrusted = 0,
        Low = 0x1000,
        Medium = 0x2000,
        High = 0x3000,
        System = 0x4000,
    }


    [ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    class SecurityInformationImpl : ISecurityInformation, IDisposable
    {
        private List<IntPtr> _names;
        private IntPtr _access_map; // SI_ACCESS
        private IntPtr _obj_name;
        private byte[] _sd;

        private static Dictionary<uint, string> GetNames(bool access)
        {
            string typename = access ? "Access" : "Launch";
            Dictionary<uint, string> names = new Dictionary<uint, string>();
            names[(uint)COMAccessRights.Execute] = typename;
            names[(uint)COMAccessRights.ExecuteLocal] = String.Format("Local {0}", typename);
            names[(uint)COMAccessRights.ExecuteRemote] = String.Format("Remote {0}", typename);
            if (!access)
            {
                names[(uint)COMAccessRights.ActivateLocal] = "Local Activate";
                names[(uint)COMAccessRights.ActivateRemote] = "Remote Activate";
            }
            return names;
        }

        public SecurityInformationImpl(string obj_name, string sddl, bool access)
        {
            Dictionary<uint, string> names = GetNames(access);
            _sd = COMSecurity.GetSDForStringSD(sddl);
            _obj_name = Marshal.StringToBSTR(obj_name);
            _access_map = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SiAccess)) * names.Count);
            SiAccess[] sis = new SiAccess[names.Count];
            _names = new List<IntPtr>();
            int i = 0;
            IntPtr current = _access_map;
            foreach (KeyValuePair<uint, string> pair in names)
            {
                _names.Add(Marshal.StringToBSTR(pair.Value));
                SiAccess si = new SiAccess();
                si.dwFlags = SiAccessFlags.SI_ACCESS_SPECIFIC | SiAccessFlags.SI_ACCESS_GENERAL;
                si.mask = pair.Key;
                si.pszName = _names[i];
                si.pguid = IntPtr.Zero;
                sis[i] = si;
                i++;
            }
            IntPtr current_ptr = _access_map;
            for (i = 0; i < sis.Length; ++i)
            {
                Marshal.StructureToPtr(sis[i], current_ptr, false);
                current_ptr += Marshal.SizeOf(typeof(SiAccess));
            }
        }

        public void GetAccessRights(ref Guid pguidObjectType, SiObjectInfoFlags dwFlags, out IntPtr ppAccess, out uint pcAccesses, out uint piDefaultAccess)
        {
            ppAccess = _access_map;
            pcAccesses = (uint)_names.Count;
            piDefaultAccess = 0;
        }

        public void GetInheritTypes(out IntPtr ppInheritTypes, out uint pcInheritTypes)
        {
            ppInheritTypes = IntPtr.Zero;
            pcInheritTypes = 0;
        }

        public void GetObjectInformation(IntPtr pObjectInfo)
        {
            SiObjectInfo object_info = new SiObjectInfo();
            object_info.dwFlags = SiObjectInfoFlags.SI_READONLY | SiObjectInfoFlags.SI_ADVANCED 
                | SiObjectInfoFlags.SI_EDIT_OWNER | SiObjectInfoFlags.SI_EDIT_PROPERTIES | SiObjectInfoFlags.SI_NO_ADDITIONAL_PERMISSION;
            object_info.pszObjectName = _obj_name;
            Marshal.StructureToPtr(object_info, pObjectInfo, false);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalAlloc(int flags, IntPtr size);

        public void GetSecurity(SecurityInformation RequestedInformation, out IntPtr ppSecurityDescriptor, [MarshalAs(UnmanagedType.Bool)] bool fDefault)
        {
            IntPtr ret = LocalAlloc(0, new IntPtr(_sd.Length));
            Marshal.Copy(_sd, 0, ret, _sd.Length);
            ppSecurityDescriptor = ret;
        }

        public void MapGeneric(ref Guid pguidObjectType, IntPtr pAceFlags, ref uint pMask)
        {
            // Do nothing.
        }

        public void PropertySheetPageCallback(IntPtr hwnd, uint uMsg, int uPage)
        {
            // Do nothing.
        }

        public void SetSecurity(SecurityInformation SecurityInformation, IntPtr pSecurityDescriptor)
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (_names != null)
                {
                    foreach (IntPtr p in _names)
                    {
                        Marshal.FreeBSTR(p);
                    }
                    _names.Clear();
                }
                if (_access_map != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_access_map);
                    _access_map = IntPtr.Zero;
                }
                if (_obj_name != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(_obj_name);
                    _obj_name = IntPtr.Zero;
                }
                
                disposedValue = true;
            }
        }

        ~SecurityInformationImpl()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }


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
        [DllImport("aclui.dll")]
        static extern bool EditSecurity(IntPtr hwndOwner, ISecurityInformation psi);

        public static void ViewSecurity(IWin32Window parent, string name, string sddl, bool access)
        {
            if (!String.IsNullOrWhiteSpace(sddl))
            {
                using (SecurityInformationImpl si = new SecurityInformationImpl(name, sddl, access))
                {
                    EditSecurity(parent != null ? parent.Handle : IntPtr.Zero, si);
                }
            }
        }

        public static void ViewSecurity(IWin32Window parent, COMAppIDEntry appid, bool access)
        {
            ViewSecurity(parent, String.Format("{0} {1}", appid.Name, access ? "Access" : "Launch"),
                    access ? appid.AccessPermission : appid.LaunchPermission, access);
        }

        const uint SDDL_REVISION_1 = 1;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true, SetLastError = true)]
        private extern static bool ConvertSecurityDescriptorToStringSecurityDescriptor(byte[] sd, uint rev, SecurityInformation secinfo, out IntPtr str, out int length);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true, SetLastError = true)]
        private extern static bool ConvertStringSecurityDescriptorToSecurityDescriptor(string StringSecurityDescriptor, 
            uint StringSDRevision, out IntPtr SecurityDescriptor, out int SecurityDescriptorSize);

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

        [StructLayout(LayoutKind.Sequential)]
        struct GenericMapping
        {
            public uint GenericRead;
            public uint GenericWrite;
            public uint GenericExecute;
            public uint GenericAll;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PrivilegeSet
        {
            public int PrivilegeCount;
            public int Control;
            public int LowLuid;
            public int HighLuid;
            public int Attributes;
        }
        
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool AccessCheckByType(
          byte[] pSecurityDescriptor,
          byte[] PrincipalSelfSid,
          SafeTokenHandle ClientToken,
          uint DesiredAccess,
          IntPtr ObjectTypeList,
          int ObjectTypeListLength,
          ref GenericMapping GenericMapping,
          ref PrivilegeSet PrivilegeSet,
          ref int PrivilegeSetLength,
          out uint GrantedAccess,
          out bool AccessStatus
        );

        const uint MaximumAllowed = 0x02000000;

        internal static string GetSaclForSddl(string sddl)
        {
            return GetStringSDForSD(GetSDForStringSD(sddl), SecurityInformation.Label);
        }

        internal static bool IsAccessGranted(string sddl, string principal, SafeTokenHandle token, bool launch, bool check_il, COMAccessRights desired_access)
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

        private static bool GetGrantedAccess(string sddl, string principal, SafeTokenHandle token, bool launch, out COMAccessRights maximum_rights)
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
                maximum_rights = (COMAccessRights) mapping.GenericExecute;
                return true;
            }

            byte[] princ_bytes = null;
            if (!String.IsNullOrWhiteSpace(principal))
            {
                SecurityIdentifier sid = new SecurityIdentifier(principal);
                princ_bytes = new byte[sid.BinaryLength];
                sid.GetBinaryForm(princ_bytes, 0);
            }

            maximum_rights = 0;
            PrivilegeSet priv_set = new PrivilegeSet();
            int priv_length = Marshal.SizeOf(priv_set);
            uint granted_access = 0;
            bool access_status = false;
            byte[] sd = GetSDForStringSD(sddl);
            if (!AccessCheckByType(sd, princ_bytes, token, MaximumAllowed, IntPtr.Zero, 
                0, ref mapping, ref priv_set, ref priv_length, out granted_access, out access_status))
            {
                throw new Win32Exception(sddl);
            }
            if (access_status)
            {
                maximum_rights = (COMAccessRights)(granted_access & 0x1F);
            }
            return access_status;
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
            IntPtr sddl = IntPtr.Zero;
            int length;

            try
            {
                if (ConvertSecurityDescriptorToStringSecurityDescriptor(sd, SDDL_REVISION_1,
                    info,
                    out sddl, out length))
                {
                    return Marshal.PtrToStringUni(sddl);
                }
                else
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                if (sddl != IntPtr.Zero)
                {
                    LocalFree(sddl);
                }
            }
        }

        public static string GetStringSDForSD(byte[] sd)
        {
            return GetStringSDForSD(sd, SecurityInformation.All);
        }

        public static byte[] GetSDForStringSD(string sddl)
        {
            IntPtr sd = IntPtr.Zero;
            int length;

            try
            {
                if (ConvertStringSecurityDescriptorToSecurityDescriptor(sddl, SDDL_REVISION_1, out sd, out length))
                {
                    byte[] ret = new byte[length];
                    Marshal.Copy(sd, ret, 0, length);
                    return ret;
                }
                else
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                if (sd != IntPtr.Zero)
                {
                    LocalFree(sd);
                }
            }
        }

        public static SecurityIntegrityLevel GetILFromSid(SecurityIdentifier sid)
        {
            int last_index = sid.Value.LastIndexOf('-');
            int il;
            if (int.TryParse(sid.Value.Substring(last_index + 1), out il))
            {
                return (SecurityIntegrityLevel)il;
            }

            return SecurityIntegrityLevel.Medium;
        }

        public static SecurityIntegrityLevel GetILForSD(string sddl)
        {
            if (String.IsNullOrWhiteSpace(sddl))
            {
                return SecurityIntegrityLevel.Medium;
            }

            string sacl = GetSaclForSddl(sddl);
            if (!sacl.StartsWith("S:", StringComparison.OrdinalIgnoreCase))
            {
                return SecurityIntegrityLevel.Medium;
            }

            Regex label_re = new Regex(@"\(ML;;[^;]*;;;([^)]+)\)");
            Match m = label_re.Match(sacl);
            if (!m.Success || m.Groups.Count < 2)
            {
                return SecurityIntegrityLevel.Medium;
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
                    if (common_ace.AceType == AceType.AccessAllowed 
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
