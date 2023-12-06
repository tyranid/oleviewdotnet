//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2019
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

namespace OleViewDotNet.Interop.SxS;

internal sealed class ReadHandle : IDisposable
{
    private readonly GCHandle _handle;
    private readonly ReadHandle _root;

    private void VerifyOffsetAndLength(int offset, int length)
    {
        if (offset < 0 || offset > Length)
        {
            throw new ArgumentException("Invalid offset value", nameof(offset));
        }

        if (length < 0 || offset + length > Length)
        {
            throw new ArgumentException("Invalid length value", nameof(length));
        }
    }

    public T ReadStructure<T>(int offset, int length)
    {
        VerifyOffsetAndLength(offset, length);
        if (typeof(T) == typeof(byte[]))
        {
            byte[] ret = new byte[length];
            Marshal.Copy(_handle.AddrOfPinnedObject() + BaseOffset + offset, ret, 0, length);
            return (T)(object)ret;
        }

        if (Marshal.SizeOf<T>() > length)
        {
            throw new ArgumentException("Size too small for structure");
        }
        return Marshal.PtrToStructure<T>(_handle.AddrOfPinnedObject() + offset);
    }

    public T ReadStructure<T>(int offset)
    {
        return ReadStructure<T>(offset, Marshal.SizeOf<T>());
    }

    public string ReadString(int offset, int length)
    {
        VerifyOffsetAndLength(offset, length);
        return Marshal.PtrToStringUni(_handle.AddrOfPinnedObject() + BaseOffset + offset, length / 2).TrimEnd('\0');
    }

    public T[] ReadArray<T>(int offset, int count)
    {
        int element_size = Marshal.SizeOf<T>();
        VerifyOffsetAndLength(offset, count * element_size);
        T[] ret = new T[count];
        for (int i = 0; i < count; ++i)
        {
            ret[i] = ReadStructure<T>(offset + i * element_size);
        }
        return ret;
    }

    public ReadHandle At(int offset)
    {
        return new ReadHandle(_handle, this, offset, TotalLength);
    }

    public void Dispose()
    {
        if (_root == this)
        {
            _handle.Free();
        }
    }

    public int TotalLength { get; }
    public int Length => TotalLength - BaseOffset;
    public int BaseOffset { get; }
    public ReadHandle Root { get; }

    private ReadHandle(GCHandle handle, ReadHandle root, int base_offset, int total_length)
    {
        _handle = handle;
        _root = root;
        BaseOffset = base_offset;
        TotalLength = total_length;
    }

    public ReadHandle(byte[] data)
        : this(GCHandle.Alloc(data, GCHandleType.Pinned),
              null, 0, data.Length)
    {
        Root = this;
    }
}
