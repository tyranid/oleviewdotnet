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
using System.Runtime.InteropServices;

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

    internal sealed class SafeKernelObjectHandle : SafeHandle
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
}
