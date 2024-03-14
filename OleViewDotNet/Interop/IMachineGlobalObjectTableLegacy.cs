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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Interop;

[Guid("0113f843-62de-462a-a6a8-99d11546034f"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IMachineGlobalObjectTableLegacy
{
    IntPtr RegisterObject(
        MGOTFLAGS flags,
        in Guid clsid,
        [MarshalAs(UnmanagedType.HString)]
        string identifier,
        [MarshalAs(UnmanagedType.IUnknown)] object obj);

    IList<string> EnumerateObjectIdentifiers(in Guid clsid);

    [return: MarshalAs(UnmanagedType.IUnknown)]
    object GetObject(
       in Guid clsid,
       [MarshalAs(UnmanagedType.HString)]
       string identifier,
       in Guid riid);

    void RevokeObject(IntPtr token);
}