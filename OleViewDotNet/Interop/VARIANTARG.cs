//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
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
using System.Runtime.InteropServices;

namespace OleViewDotNet.Interop;

[StructLayout(LayoutKind.Explicit)]
internal struct VARIANTARG
{
    [StructLayout(LayoutKind.Sequential)]
    private struct TypeUnion
    {
        private ushort _vt;

        private ushort _wReserved1;

        private ushort _wReserved2;

        private ushort _wReserved3;

        private UnionTypes _unionTypes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Record
    {
        private IntPtr _record;

        private IntPtr _recordInfo;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct UnionTypes
    {
        [FieldOffset(0)]
        private long _i8;

        [FieldOffset(0)]
        private Record _record;
    }

    [FieldOffset(0)]
    private TypeUnion _typeUnion;

    [FieldOffset(0)]
    private decimal _decimal;

    public readonly object ToObject()
    {
        using var buffer = new SafeStructureInOutBuffer<VARIANTARG>(this);
        return Marshal.GetObjectForNativeVariant(buffer.DangerousGetHandle());
    }
}
