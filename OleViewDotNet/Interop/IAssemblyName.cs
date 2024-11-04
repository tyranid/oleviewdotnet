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
using System.Runtime.InteropServices;
using System.Text;

namespace OleViewDotNet.Interop;

[ComImport, Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAssemblyName
{
    void SetProperty(
          int PropertyId,
          IntPtr pvProperty,
          int cbProperty);
    void GetProperty(
        int PropertyId,
        IntPtr pvProperty,
        ref int pcbProperty);

    void FinalizeCom();

    void GetDisplayName([In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder szDisplayName,
        ref int pccDisplayName,
        ASM_DISPLAY_FLAGS dwDisplayFlags);

    void Reserved();

    void GetName(ref int lpcwBuffer,
        [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwzName);

    void GetVersion(out int pdwVersionHi, out int pdwVersionLow);

    [PreserveSig]
    int IsEqual(IAssemblyName pName, int dwCmpFlags);

    IAssemblyName Clone();
}
