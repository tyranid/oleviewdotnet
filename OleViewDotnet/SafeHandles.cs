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
    }
}
