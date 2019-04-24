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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace OleViewDotNet.Database
{
    public enum AppxPackageArchitecture
    {
        X86 = 0,
        ARM = 5,
        X64 = 9,
        Neutral = 11,
        ARM64 = 12,
        X86OnARM = 14,
        Unknown = 0xFFFF,
    }

    public class AppxPackageName
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct PACKAGE_VERSION
        {
            public ushort Revision;
            public ushort Build;
            public ushort Minor;
            public ushort Major;

            public Version ToVersion()
            {
                return new Version(Major, Minor, Build, Revision);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class PACKAGE_ID
        {
            public uint reserved;
            public AppxPackageArchitecture processorArchitecture;
            public PACKAGE_VERSION version;
            public IntPtr name;
            public IntPtr publisher;
            public IntPtr resourceId;
            public IntPtr publisherId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class PACKAGE_ID_ALLOC
        {
            public uint reserved;
            public AppxPackageArchitecture processorArchitecture;
            public PACKAGE_VERSION version;
            public string name;
            public string publisher;
            public string resourceId;
            public string publisherId;
        }

        private const int PACKAGE_INFORMATION_BASIC = 0;
        private const int PACKAGE_INFORMATION_FULL = 0x00000100;
        private const int ERROR_INSUFFICIENT_BUFFER = 0x7A;
        private const int ERROR_NOT_FOUND = 0x490;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int PackageIdFromFullName(string packageFullName, int flags,
            ref int bufferLength,
            SafeHGlobalBuffer buffer
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPackageFullName(
            SafeKernelObjectHandle hProcess,
            ref int packageFullNameLength,
            StringBuilder packageFullName
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern int PackageFullNameFromId(PACKAGE_ID_ALLOC packageId, 
            ref int packageFullNameLength, StringBuilder packageFullName);

        private AppxPackageName(string package_id, PACKAGE_ID package)
        {
            FullName = package_id;
            Architecture = package.processorArchitecture;
            Version = package.version.ToVersion();
            Name = package.name != IntPtr.Zero ? Marshal.PtrToStringUni(package.name) : string.Empty;
            ResourceId = package.resourceId != IntPtr.Zero ? Marshal.PtrToStringUni(package.resourceId) : string.Empty;
            PublisherId = package.publisherId != IntPtr.Zero ? Marshal.PtrToStringUni(package.publisherId) : string.Empty;
            Publisher = package.publisher != IntPtr.Zero ? Marshal.PtrToStringUni(package.publisher) : string.Empty;
        }

        private static Dictionary<string, AppxPackageName> _name_cache = new Dictionary<string, AppxPackageName>();

        private static AppxPackageName FromFullNameInternal(string package_id, int flags)
        {
            if (string.IsNullOrWhiteSpace(package_id))
            {
                return null;
            }

            int length = 0;
            int err = PackageIdFromFullName(package_id, flags, ref length, SafeHGlobalBuffer.Null);
            if (err != ERROR_INSUFFICIENT_BUFFER)
            {
                if (err == ERROR_NOT_FOUND && flags == PACKAGE_INFORMATION_FULL)
                {
                    return FromFullNameInternal(package_id, PACKAGE_INFORMATION_BASIC);
                }
                return null;
            }

            using (var buffer = new SafeStructureInOutBuffer<PACKAGE_ID>(length, false))
            {
                length = buffer.Length;
                err = PackageIdFromFullName(package_id, flags, ref length, buffer);
                if (err != 0)
                {
                    if (err == ERROR_NOT_FOUND && flags == PACKAGE_INFORMATION_FULL)
                    {
                        return FromFullNameInternal(package_id, PACKAGE_INFORMATION_BASIC);
                    }

                    return null;
                }
                return new AppxPackageName(package_id, buffer.Result);
            }
        }

        public static AppxPackageName FromFullName(string package_id)
        {
            if (!_name_cache.ContainsKey(package_id))
            {
                _name_cache[package_id] = FromFullNameInternal(package_id, PACKAGE_INFORMATION_FULL);
            }

            return _name_cache[package_id];
        }

        public static AppxPackageName FromProcess(NtProcess process)
        {
            int length = 0;
            int err = GetPackageFullName(process.Handle, ref length, null);
            if (err != ERROR_INSUFFICIENT_BUFFER)
            {
                return null;
            }

            StringBuilder builder = new StringBuilder(length);
            err = GetPackageFullName(process.Handle, ref length, builder);
            if (err != 0)
            {
                return null;
            }

            return FromFullName(builder.ToString());
        }

        public static string GetPublisherIdFromPublisher(string publisher)
        {
            PACKAGE_ID_ALLOC pkgid = new PACKAGE_ID_ALLOC();
            pkgid.publisher = publisher;
            pkgid.name = "Temp";
            int length = 0;
            int err = PackageFullNameFromId(pkgid, ref length, null);
            if (err != ERROR_INSUFFICIENT_BUFFER)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(length);
            err = PackageFullNameFromId(pkgid, ref length, builder);
            if (err != 0)
            {
                return null;
            }
            return FromFullName(builder.ToString())?.PublisherId;
        }

        public AppxPackageArchitecture Architecture { get; }
        public Version Version { get; }
        public string Name { get; }
        public string ResourceId { get; }
        public string Publisher { get; }
        public string PublisherId { get; }
        public string FullName { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
