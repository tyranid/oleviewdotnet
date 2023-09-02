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
using System.Runtime.InteropServices.WindowsRuntime;

namespace OleViewDotNet.PowerShell
{
    public static class PowerShellUtils
    {
        static NtToken GetProcessAccessToken(NtProcess process)
        {
            return process.OpenToken();
        }

        public static NtToken GetAccessToken(NtToken token, NtProcess process, int process_id)
        {
            if (token != null)
            {
                return token.DuplicateToken(SecurityImpersonationLevel.Identification);
            }
            else if (process != null)
            {
                return GetProcessAccessToken(process);
            }
            else
            {
                using (var p = NtProcess.Open(process_id, ProcessAccessRights.QueryLimitedInformation))
                {
                    return GetProcessAccessToken(p);
                }
            }
        }

        public static COMAccessCheck GetAccessCheck(NtToken token,
            string principal,
            COMAccessRights access_rights,
            COMAccessRights launch_rights,
            bool ignore_default)
        {
            return new COMAccessCheck(
                token,
                principal,
                access_rights,
                launch_rights,
                ignore_default);
        }

        public static COMAccessCheck GetAccessCheck(NtProcess process,
            string principal,
            COMAccessRights access_rights,
            COMAccessRights launch_rights,
            bool ignore_default)
        {
            return new COMAccessCheck(
                process.OpenToken(),
                principal,
                access_rights,
                launch_rights,
                ignore_default);
        }

        public static COMAccessCheck GetAccessCheck(int process_id,
            string principal,
            COMAccessRights access_rights,
            COMAccessRights launch_rights,
            bool ignore_default)
        {
            using (var process = NtProcess.Open(process_id, ProcessAccessRights.QueryLimitedInformation))
            {
                return GetAccessCheck(process, principal, access_rights, launch_rights, ignore_default);
            }
        }

        public static Type GetFactoryType(ICOMClassEntry cls)
        {
            if (cls is COMRuntimeClassEntry)
            {
                return typeof(IActivationFactory);
            }
            return typeof(IClassFactory);
        }

        public static Guid GuidFromInts(int[] ints)
        {
            if (ints.Length != 4)
            {
                throw new ArgumentException("Must provide 4 integers to convert to a GUID");
            }

            byte[] bytes = new byte[16];
            Buffer.BlockCopy(ints, 0, bytes, 0, 16);
            return new Guid(bytes);
        }
    }
}
