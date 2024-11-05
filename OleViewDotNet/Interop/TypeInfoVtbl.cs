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

[StructLayout(LayoutKind.Sequential)]
internal struct TypeInfoVtbl
{
    public int cRefs;
    public Guid iid;
    public int fIsDual;
    public MIDL_STUB_DESC stubDesc;
    public MIDL_SERVER_INFO stubInfo;
    public CInterfaceStubVtbl stubVtbl;
    public MIDL_STUBLESS_PROXY_INFO proxyInfo;
    public CInterfaceProxyVtbl proxyVtbl;
};

[StructLayout(LayoutKind.Sequential)]
internal struct MIDL_STUB_DESC
{
    public IntPtr RpcInterfaceInformation;
    public IntPtr pfnAllocate;
    public IntPtr pfnFree;
    public IntPtr pGenericBindingInfo;
    public IntPtr apfnNdrRundownRoutines;
    public IntPtr aGenericBindingRoutinePairs;
    public IntPtr apfnExprEval;
    public IntPtr aXmitQuintuple;
    public IntPtr pFormatTypes;
    public int fCheckBounds;
    /* Ndr library version. */
    public int Version;
    public IntPtr pMallocFreeStruct;
    public int MIDLVersion;
    public IntPtr CommFaultOffsets;
    // New fields for version 3.0+
    public IntPtr aUserMarshalQuadruple;
    // Notify routines - added for NT5, MIDL 5.0
    public IntPtr NotifyRoutineTable;
    public IntPtr mFlags;
    // International support routines - added for 64bit post NT5
    public IntPtr CsRoutineTables;
    public IntPtr ProxyServerInfo;
    public IntPtr pExprInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MIDL_SERVER_INFO
{
    public IntPtr pStubDesc;
    public IntPtr DispatchTable;
    public IntPtr ProcString;
    public IntPtr FmtStringOffset;
    public IntPtr ThunkTable;
    public IntPtr pTransferSyntax;
    public IntPtr nCount;
    public IntPtr pSyntaxInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MIDL_STUBLESS_PROXY_INFO
{
    public IntPtr pStubDesc;
    public IntPtr ProcFormatString;
    public IntPtr FormatStringOffset;
    public IntPtr pTransferSyntax;
    public IntPtr nCount;
    public IntPtr pSyntaxInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct CInterfaceStubHeader
{
    public IntPtr piid;
    public IntPtr pServerInfo;
    public int DispatchTableCount;
    public IntPtr pDispatchTable;
};


[StructLayout(LayoutKind.Sequential)]
internal struct IRpcStubBufferVtbl
{
    public IntPtr QueryInterface;
    public IntPtr AddRef;
    public IntPtr Release;
    public IntPtr Connect;
    public IntPtr Disconnect;
    public IntPtr Invoke;
    public IntPtr IsIIDSupported;
    public IntPtr CountRefs;
    public IntPtr DebugServerQueryInterface;
    public IntPtr DebugServerRelease;
};

[StructLayout(LayoutKind.Sequential)]
internal struct CInterfaceProxyHeader
{
    public IntPtr piid;
};

[StructLayout(LayoutKind.Sequential)]
internal struct CInterfaceProxyVtbl
{
    public CInterfaceProxyHeader header;
    public IntPtr Vtbl;
};

[StructLayout(LayoutKind.Sequential)]
internal struct CInterfaceStubVtbl
{
    public CInterfaceStubHeader header;
    public IRpcStubBufferVtbl Vtbl;
};