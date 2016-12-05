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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
    enum SECURITY_INFORMATION
    {
        OWNER_SECURITY_INFORMATION = 1,
        GROUP_SECURITY_INFORMATION = 2,
        DACL_SECURITY_INFORMATION = 4,
        LABEL_SECURITY_INFORMATION = 0x10,
    }
    
    [Guid("965FC360-16FF-11d0-91CB-00AA00BBB723"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
    interface ISecurityInformation
    {
        // *** ISecurityInformation methods ***
        void GetObjectInformation(IntPtr pObjectInfo);
        void GetSecurity(SECURITY_INFORMATION RequestedInformation,
                        out IntPtr ppSecurityDescriptor,
                        [MarshalAs(UnmanagedType.Bool)] bool fDefault);

        void SetSecurity(SECURITY_INFORMATION SecurityInformation,
                        IntPtr pSecurityDescriptor);

        void GetAccessRights(ref Guid pguidObjectType,
                            SiObjectInfoFlags dwFlags, // SI_EDIT_AUDITS, SI_EDIT_PROPERTIES
                            out IntPtr ppAccess,
                            out uint pcAccesses,
                            out uint piDefaultAccess);

        void MapGeneric(ref Guid pguidObjectType,
                        IntPtr pAceFlags,
                        ref uint pMask);

        void GetInheritTypes(out IntPtr ppInheritTypes,
                            out uint pcInheritTypes);
        void PropertySheetPageCallback(IntPtr hwnd, uint uMsg, int uPage);
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

        public SecurityInformationImpl(string obj_name, byte[] sd, bool access)
        {
            Dictionary<uint, string> names = GetNames(access);
            _sd = sd;
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
            object_info.dwFlags = SiObjectInfoFlags.SI_READONLY | SiObjectInfoFlags.SI_ADVANCED | SiObjectInfoFlags.SI_EDIT_AUDITS 
                | SiObjectInfoFlags.SI_EDIT_OWNER | SiObjectInfoFlags.SI_EDIT_PROPERTIES | SiObjectInfoFlags.SI_NO_ADDITIONAL_PERMISSION;
            object_info.pszObjectName = _obj_name;
            Marshal.StructureToPtr(object_info, pObjectInfo, false);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalAlloc(int flags, IntPtr size);

        public void GetSecurity(SECURITY_INFORMATION RequestedInformation, out IntPtr ppSecurityDescriptor, [MarshalAs(UnmanagedType.Bool)] bool fDefault)
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

        public void SetSecurity(SECURITY_INFORMATION SecurityInformation, IntPtr pSecurityDescriptor)
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

        public static void ViewSecurity(IWin32Window parent, string name, byte[] sd, bool access)
        {
            using (SecurityInformationImpl si = new SecurityInformationImpl(name, sd, access))
            {
                EditSecurity(parent != null ? parent.Handle : IntPtr.Zero, si);
            }
        }

        const uint SDDL_REVISION_1 = 1;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true, SetLastError = true)]
        private extern static bool ConvertSecurityDescriptorToStringSecurityDescriptor(byte[] sd, uint rev, SECURITY_INFORMATION secinfo, out IntPtr str, out int length);

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

        private static byte[] GetSecurityPermissions(COMSD sdtype)
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
                return ret;
            }
            finally
            {
                if (sd != IntPtr.Zero)
                {
                    LocalFree(sd);
                }
            }
        }

        public static byte[] GetDefaultLaunchPermissions()
        {
            return GetSecurityPermissions(COMSD.SD_LAUNCHPERMISSIONS);
        }

        public static byte[] GetDefaultAccessPermissions()
        {
            return GetSecurityPermissions(COMSD.SD_ACCESSPERMISSIONS);
        }

        public static byte[] GetDefaultLaunchRestrictions()
        {
            return GetSecurityPermissions(COMSD.SD_LAUNCHRESTRICTIONS);
        }

        public static byte[] GetDefaultAccessRestrictions()
        {
            return GetSecurityPermissions(COMSD.SD_ACCESSRESTRICTIONS);
        }

        public static string GetStringSDForSD(byte[] sd)
        {
            IntPtr sddl;
            int length;

            if (ConvertSecurityDescriptorToStringSecurityDescriptor(sd, SDDL_REVISION_1,
                SECURITY_INFORMATION.DACL_SECURITY_INFORMATION 
                | SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION 
                | SECURITY_INFORMATION.LABEL_SECURITY_INFORMATION,
                out sddl, out length))
            {
                string ret = Marshal.PtrToStringUni(sddl);

                LocalFree(sddl);

                return ret;
            }
            else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public static string GetILForSD(byte[] sd)
        {
            if ((sd != null) && (sd.Length > 0))
            {
                IntPtr sddl;
                int length;

                if (ConvertSecurityDescriptorToStringSecurityDescriptor(sd, SDDL_REVISION_1,
                    SECURITY_INFORMATION.LABEL_SECURITY_INFORMATION,
                    out sddl, out length))
                {
                    string ret = Marshal.PtrToStringUni(sddl, length);

                    LocalFree(sddl);

                    return ret;
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            else
            {
                return null;
            }
        }

        public static bool SDHasAC(byte[] sd)
        {
            if (sd == null || sd.Length == 0)
            {
                return false;
            }

            string sddl = GetStringSDForSD(sd);
            return sddl.Contains("S-1-15-") || sddl.Contains(";AC)");
        }
    }
}
