//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017
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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace OleViewDotNet
{
    internal class SafeHGlobalBuffer : SafeBuffer
    {
        public SafeHGlobalBuffer(int length) : base(true)
        {
            Length = length;
            Initialize((ulong)length);
            SetHandle(Marshal.AllocHGlobal(length));
        }

        public int Length
        {
            get; private set;
        }

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                Marshal.FreeHGlobal(handle);
                handle = IntPtr.Zero;
            }
            return true;
        }

        private SafeHGlobalBuffer() : base(false)
        {
            SetHandle(IntPtr.Zero);
            Initialize(0);
        }

        public static SafeHGlobalBuffer Null { get { return new SafeHGlobalBuffer(); } }
    }

    internal class SafeStructureBuffer<T> : SafeHGlobalBuffer where T : new()
    {
        public SafeStructureBuffer(T obj, int additional_size)
            : base(Marshal.SizeOf(obj) + additional_size)
        {
            Marshal.StructureToPtr(obj, handle, false);
        }

        public SafeStructureBuffer() : this(new T(), 0)
        {
        }

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                Marshal.DestroyStructure(handle, typeof(T));
            }
            return base.ReleaseHandle();
        }

        public virtual T Result
        {
            get
            {
                if (IsClosed || IsInvalid)
                    throw new ObjectDisposedException("handle");

                return Marshal.PtrToStructure<T>(handle);
            }
        }
    }

    internal class SafeKernelObjectHandle : SafeHandle
    {
        private SafeKernelObjectHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public SafeKernelObjectHandle(IntPtr handle, bool owns_handle)
          : base(IntPtr.Zero, owns_handle)
        {
            SetHandle(handle);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            if (CloseHandle(this.handle))
            {
                this.handle = IntPtr.Zero;
                return true;
            }
            return false;
        }

        public override bool IsInvalid
        {
            get
            {
                return this.handle.ToInt64() <= 0;
            }
        }

        public static SafeKernelObjectHandle Null
        {
            get { return new SafeKernelObjectHandle(IntPtr.Zero, false); }
        }
    }

    [Flags]
    internal enum ProcessAccessRights : uint
    {
        None = 0,
        CreateProcess = 0x0080,
        CreateThread = 0x0002,
        DupHandle = 0x0040,
        QueryInformation = 0x0400,
        QueryLimitedInformation = 0x1000,
        SetInformation = 0x0200,
        SetQuota = 0x0100,
        SuspendResume = 0x0800,
        Terminate = 0x0001,
        VmOperation = 0x0008,
        VmRead = 0x0010,
        VmWrite = 0x0020,
        GenericRead = 0x80000000,
        GenericWrite = 0x40000000,
        GenericExecute = 0x20000000,
        GenericAll = 0x10000000,
        Delete = 0x00010000,
        ReadControl = 0x00020000,
        WriteDac = 0x00040000,
        WriteOwner = 0x00080000,
        Synchronize = 0x00100000,
        MaximumAllowed = 0x02000000,
    };

    enum SecurityImpersonationLevel
    {
        SecurityAnonymous,
        SecurityIdentification,
        SecurityImpersonation,
        SecurityDelegation
    }

    internal class SafeTokenHandle : SafeKernelObjectHandle
    {
        private SafeTokenHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public SafeTokenHandle(IntPtr handle, bool owns_handle)
          : base(IntPtr.Zero, owns_handle)
        {
            SetHandle(handle);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DuplicateToken(
            SafeTokenHandle ExistingTokenHandle,
            SecurityImpersonationLevel ImpersonationLevel,
            out SafeTokenHandle DuplicateTokenHandle
        );

        public SafeTokenHandle DuplicateImpersonation(SecurityImpersonationLevel imp_level)
        {
            SafeTokenHandle token;
            if (!DuplicateToken(this, imp_level, out token))
            {
                throw new Win32Exception();
            }
            return token;
        }

        enum TokenInformationClass
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            TokenIsAppContainer,
            TokenCapabilities,
            TokenAppContainerSid,
            TokenAppContainerNumber,
            TokenUserClaimAttributes,
            TokenDeviceClaimAttributes,
            TokenRestrictedUserClaimAttributes,
            TokenRestrictedDeviceClaimAttributes,
            TokenDeviceGroups,
            TokenRestrictedDeviceGroups,
            TokenSecurityAttributes,
            TokenIsRestricted,
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(
          SafeTokenHandle TokenHandle,
          TokenInformationClass TokenInformationClass,
          IntPtr TokenInformation,
          int TokenInformationLength,
          out int ReturnLength
        );
    }

    internal class SafeProcessHandle : SafeKernelObjectHandle
    {
        private SafeProcessHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public SafeProcessHandle(IntPtr handle, bool owns_handle)
          : base(IntPtr.Zero, owns_handle)
        {
            SetHandle(handle);
        }

        public static SafeProcessHandle Current
        {
            get { return new SafeProcessHandle(new IntPtr(-1), false); }
        }

        private bool? _is64bit;

        public bool Is64Bit
        {
            get
            {
                if (!_is64bit.HasValue)
                {
                    _is64bit = Is64bitProcess(this);
                }
                return _is64bit.Value;
            }
        }

        private int? _pid;
        public int Pid
        {
            get
            {
                if (!_pid.HasValue)
                {
                    _pid = GetProcessId(handle);
                }
                return _pid.Value;
            }
        }

        private string _image_name;
        public string ImageName
        {
            get
            {
                if (_image_name == null)
                {
                    StringBuilder builder = new StringBuilder(260);
                    int size = builder.Capacity;
                    if (QueryFullProcessImageName(handle, 0, builder, ref size))
                    {
                        _image_name = builder.ToString();
                    }
                    else
                    {
                        _image_name = "Unknown";
                    }
                }
                return _image_name;
            }

        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
              SafeKernelObjectHandle hProcess,
              IntPtr lpBaseAddress,
              SafeBuffer lpBuffer,
              IntPtr nSize,
              out IntPtr lpNumberOfBytesRead
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
              SafeKernelObjectHandle hProcess,
              IntPtr lpBaseAddress,
              byte[] lpBuffer,
              IntPtr nSize,
              out IntPtr lpNumberOfBytesRead
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetProcessId(IntPtr Process);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageName(
          IntPtr hProcess,
          int dwFlags,
          StringBuilder lpExeName,
          ref int lpdwSize
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool IsWow64Process(SafeKernelObjectHandle hProcess,
            [MarshalAs(UnmanagedType.Bool)] out bool Wow64Process);

        private static bool Is64bitProcess(SafeKernelObjectHandle process)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                bool wow64 = false;
                if (!IsWow64Process(process, out wow64))
                {
                    throw new Win32Exception();
                }

                return !wow64;
            }
            else
            {
                return false;
            }
        }

        public T[] ReadArray<T>(IntPtr ptr, int count) where T : struct
        {
            using (var buf = ReadBuffer(ptr, count * Marshal.SizeOf(typeof(T))))
            {
                T[] ret = new T[count];
                if (buf != null)
                {
                    buf.ReadArray(0, ret, 0, ret.Length);
                }
                return ret;
            }
        }

        public SafeHGlobalBuffer ReadBuffer(IntPtr ptr, int length)
        {
            SafeHGlobalBuffer buf = new SafeHGlobalBuffer(length);
            bool success = false;

            try
            {
                IntPtr out_length;
                success = ReadProcessMemory(this, ptr, buf, new IntPtr(buf.Length), out out_length);
                if (success)
                {
                    return buf;
                }
            }
            finally
            {
                if (!success)
                {
                    buf.Close();
                }
            }

            return null;
        }

        public T ReadStruct<T>(IntPtr ptr) where T : new()
        {
            using (SafeStructureBuffer<T> buf = new SafeStructureBuffer<T>())
            {
                IntPtr out_length;
                if (ReadProcessMemory(this, ptr, buf, new IntPtr(buf.Length), out out_length))
                {
                    return buf.Result;
                }

                return default(T);
            }
        }

        public string ReadUnicodeString(IntPtr ptr)
        {
            byte[] data = new byte[2];
            StringBuilder builder = new StringBuilder();
            IntPtr out_length;
            int pos = 0;
            while (ReadProcessMemory(this, ptr + pos, data, new IntPtr(data.Length), out out_length))
            {
                char c = BitConverter.ToChar(data, 0);
                if (c == 0)
                {
                    break;
                }
                builder.Append(c);
                pos += 2;
            }
            return builder.ToString();
        }


        private const int MaximumAllowed = 0x02000000;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool OpenProcessToken(
              IntPtr ProcessHandle,
              int DesiredAccess,
              out SafeTokenHandle TokenHandle
            );

        public SafeTokenHandle OpenToken()
        {
            SafeTokenHandle token;
            if (!OpenProcessToken(handle, MaximumAllowed, out token))
            {
                throw new Win32Exception();
            }

            return token;
        }

        public SafeTokenHandle OpenTokenAsImpersonation(SecurityImpersonationLevel imp_level)
        {
            using (SafeTokenHandle token = OpenToken())
            {
                return token.DuplicateImpersonation(imp_level);
            }
        }

        public string GetProcessUser()
        {
            try
            {
                using (SafeTokenHandle token = OpenToken())
                {
                    using (WindowsIdentity id = new WindowsIdentity(token.DangerousGetHandle()))
                    {
                        SecurityIdentifier sid = id.User;
                        try
                        {
                            NTAccount account = (NTAccount)sid.Translate(typeof(NTAccount));
                            return account.Value;
                        }
                        catch (IdentityNotMappedException)
                        {
                            return sid.Value;
                        }
                    }
                }
            }
            catch (Win32Exception)
            {
                return "Unknown";
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeProcessHandle OpenProcess(ProcessAccessRights dwDesiredAccess,
                                                                 bool bInheritHandle,
                                                                 int dwProcessId
                                                                );

        public static SafeProcessHandle Open(int pid, ProcessAccessRights desired_access)
        {
            return OpenProcess(desired_access, false, pid);
        }
    }
}
