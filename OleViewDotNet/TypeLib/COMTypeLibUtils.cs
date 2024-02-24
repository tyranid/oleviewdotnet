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

using OleViewDotNet.Interop;
using OleViewDotNet.Utilities;
using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;
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

    public static string FormatAttrs(this ICollection<string> attrs)
    {
        return attrs.Count > 0 ? $"[{string.Join(", ", attrs)}] " : string.Empty;
    }

    public static object ReadDefaultValue(IntPtr base_ptr)
    {
        if (base_ptr == IntPtr.Zero)
            return null;
        int offset = Marshal.OffsetOf<PARAMDESCEX>("varDefaultValue").ToInt32();
        return GetVariant(base_ptr + offset);
    }

    public static object GetVariant(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            return null;
        return Marshal.GetObjectForNativeVariant(ptr);
    }

    public static string FormatAttr(string name, object value)
    {
        if (value is string s)
        {
            return $"{name}(\"{s.EscapeString()}\")";
        }
        else
        {
            return $"{name}({value})";
        }
    }

    public static int GetTypeSize<T>()
    {
        return Marshal.SizeOf<T>();
    }

    public static string ReadBstr(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            return null;
        try
        {
            return Marshal.PtrToStringBSTR(ptr);
        }
        finally
        {
            Marshal.FreeBSTR(ptr);
        }
    }
}
