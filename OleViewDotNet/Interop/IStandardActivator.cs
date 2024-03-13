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

namespace OleViewDotNet.Interop;

[Guid("000001B8-0000-0000-c000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IStandardActivator
{
    [return: MarshalAs(UnmanagedType.IUnknown)]
    object StandardGetClassObject(in Guid rclsid, CLSCTX dwContext, [In] COSERVERINFO pServerInfo, in Guid riid);
    void StandardCreateInstance(in Guid Clsid, IntPtr punkOuter, CLSCTX dwClsCtx, [In] COSERVERINFO pServerInfo, int dwCount, [In, Out][MarshalAs(UnmanagedType.LPArray)] MULTI_QI[] pResults);
    void StandardGetInstanceFromFile([In] COSERVERINFO pServerInfo, OptionalGuidClass pclsidOverride,
        IntPtr punkOuter, CLSCTX dwClsCtx, STGM grfMode, [MarshalAs(UnmanagedType.LPWStr)] string pwszName, int dwCount, [In, Out][MarshalAs(UnmanagedType.LPArray)] MULTI_QI[] pResults);
    void StandardGetInstanceFromIStorage([In] COSERVERINFO pServerInfo, OptionalGuidClass pclsidOverride,
        IntPtr punkOuter, CLSCTX dwClsCtx, [MarshalAs(UnmanagedType.IUnknown)] IStorage pstg, int dwCount, 
        [In, Out][MarshalAs(UnmanagedType.LPArray)] MULTI_QI[] pResults);
    void Reset();
}