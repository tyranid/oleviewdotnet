﻿//    This file is part of OleViewDotNet.
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

using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.TypeLib;

internal static class COMTypeLibUtils
{
    public static T GetStructure<T>(this IntPtr ptr)
    {
        return Marshal.PtrToStructure<T>(ptr);
    }

    public static T[] ReadArray<T>(this IntPtr ptr, int count)
    {
        T[] ret = new T[count];
        int size = Marshal.SizeOf<T>();
        for (int i = 0; i < count; ++i)
        {
            ret[i] = ptr.GetStructure<T>();
            ptr += size;
        }
        return ret;
    }

    public static void ReleaseComObject(this object obj)
    {
        Marshal.ReleaseComObject(obj);
    }
}