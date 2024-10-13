//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
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

[StructLayout(LayoutKind.Sequential)]
public struct RPC_MESSAGE
{
    public IntPtr Handle;
    public int DataRepresentation;
    public IntPtr Buffer;
    public int BufferLength;
    public int ProcNum;
    public IntPtr TransferSyntax;
    public IntPtr RpcInterfaceInformation;
    public IntPtr ReservedForRuntime;
    public IntPtr ManagerEpv;
    public IntPtr ImportContext;
    public int RpcFlags;
}
