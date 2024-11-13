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

[ComImport, Guid("D5F56AFC-593B-101A-B569-08002B2DBF7A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IRpcStubBuffer 
{
    [PreserveSig]
    int Connect([MarshalAs(UnmanagedType.IUnknown)] object pUnkServer);
    void Disconnect();
    [PreserveSig]
    int Invoke(ref RPCOLEMESSAGE prpcmsg,
            [MarshalAs(UnmanagedType.Interface)] IRpcChannelBuffer pRpcChannelBuffer);
    [PreserveSig]
    IRpcStubBuffer IsIIDSupported(in Guid riid);
    [PreserveSig]
    int CountRefs();
    [PreserveSig]
    int DebugServerQueryInterface(out IntPtr ppv);
    [PreserveSig]
    void DebugServerRelease(IntPtr pv);
}