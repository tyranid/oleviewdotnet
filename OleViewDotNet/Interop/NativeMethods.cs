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

using NtApiDotNet;
using NtApiDotNet.Win32;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace OleViewDotNet.Interop;

internal static class NativeMethods
{
    [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    [return: MarshalAs(UnmanagedType.Interface)]
    public static extern ITypeLib LoadTypeLibEx(string strTypeLibName, RegKind regKind);

    [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    [return: MarshalAs(UnmanagedType.Interface)]
    public static extern ITypeLib LoadRegTypeLib(
        in Guid rguid,
        ushort wVerMajor,
        ushort wVerMinor,
        int lcid
    );

    [DllImport("ole32.dll")]
    public static extern int CoCreateInstance(in Guid rclsid, IntPtr pUnkOuter, CLSCTX dwClsContext, in Guid riid, out IntPtr ppv);
    [DllImport("ole32.dll")]
    public static extern int CoCreateInstanceEx(in Guid rclsid, IntPtr punkOuter, CLSCTX dwClsCtx, [In] COSERVERINFO pServerInfo, int dwCount, [In, Out] MULTI_QI[] pResults);
    [DllImport("ole32.dll")]
    public static extern int CoGetClassObject(in Guid rclsid, CLSCTX dwClsContext, [In] COSERVERINFO pServerInfo, in Guid riid, out IntPtr ppv);
    [DllImport("ole32.dll", PreserveSig = false)]
    [return: MarshalAs(UnmanagedType.IUnknown)]
    public static extern object CoUnmarshalInterface(IStream stm, in Guid riid);

    [DllImport("combase.dll", CharSet = CharSet.Unicode)]
    public static extern int RoGetActivationFactory([MarshalAs(UnmanagedType.HString)] string activatableClassId,
        in Guid iid,
        out IntPtr factory
    );

    [DllImport("combase.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern ulong CoRegisterConsoleHandles(SafeHandle stdInputHandle, SafeHandle stdOutputHandle, SafeHandle stdErrorHandle);

    [DllImport("combase.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern ulong CoRegisterRacActivationToken(SafeKernelObjectHandle racActivationToken);

    [DllImport("combase.dll", EntryPoint = "#65", CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern IExtensionRegistration RoGetExtensionRegistration([MarshalAs(UnmanagedType.HString)] string contractId,
            [MarshalAs(UnmanagedType.HString)] string packageId,
            [MarshalAs(UnmanagedType.HString)] string activatableClassId);

    [DllImport("combase.dll", CharSet = CharSet.Unicode)]
    public static extern int RoActivateInstance(
        [MarshalAs(UnmanagedType.HString)] string activatableClassId,
        out IntPtr instance);

    [DllImport("ole32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern IStorage StgOpenStorageEx(
          string pwcsName,
          STGM grfMode,
          STGFMT stgfmt,
          int grfAttrs,
          [In, Out] STGOPTIONS pStgOptions,
          IntPtr reserved2,
          in Guid riid
        );

    [DllImport("ole32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern IStorage StgOpenStorage(
          string pwcsName,
          IStorage pstgPriority,
          STGM grfMode,
          IntPtr snbExclude,
          int reserved
        );

    [DllImport("ole32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern IStorage StgCreateStorageEx(
          string pwcsName,
          STGM grfMode,
          STGFMT stgfmt,
          int grfAttrs,
          [In] STGOPTIONS pStgOptions,
          IntPtr pSecurityDescriptor,
          in Guid riid
        );

    [DllImport("ole32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = false)]
    public static extern void CoGetObject(string pszName, BIND_OPTS3 pBindOptions, in Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

    [return: MarshalAs(UnmanagedType.Interface)]
    [DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false)]
    public static extern IBindCtx CreateBindCtx([In] uint reserved);

    [DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false)]
    public static extern IMoniker CreateObjrefMoniker([MarshalAs(UnmanagedType.Interface)] object punk);

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = false)]
    public extern static void SHCreateStreamOnFile(string pszFile, STGM grfMode, out IntPtr ppStm);

    [DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int CoGetInstanceFromFile(
        IntPtr pServerInfo,
        [In] OptionalGuid pClsid,
        [In, MarshalAs(UnmanagedType.IUnknown)] object punkOuter,
        CLSCTX dwClsCtx,
        STGM grfMode,
        string pwszName,
        int dwCount,
        [In, Out] MULTI_QI[] pResults
    );

    [DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int CoGetInstanceFromIStorage(
        IntPtr pServerInfo,
        [In] OptionalGuid pClsid,
        [In, MarshalAs(UnmanagedType.IUnknown)] object punkOuter,
        CLSCTX dwClsCtx,
        IStorage pstg,
        int dwCount,
        [In, Out] MULTI_QI[] pResults
    );

    [DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern void GetClassFile(string szFilename, out Guid clsid);

    [DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern IMoniker MkParseDisplayName(IBindCtx pbc, string szUserName, out int pchEaten);

    [DllImport("urlmon.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int CreateURLMonikerEx(IMoniker pMkCtx,
                                                string szURL,
                                                out IMoniker ppmk,
                                                CreateUrlMonikerFlags dwFlags);

    [DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int CLSIDFromProgID(string lpszProgID, out Guid lpclsid);

    [DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern void CoMarshalInterface(IStream pStm, in Guid riid,
        [MarshalAs(UnmanagedType.Interface)] object pUnk, MSHCTX dwDestContext, IntPtr pvDestContext, MSHLFLAGS mshlflags);

    [DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, PreserveSig = false)]
    public static extern void CoReleaseMarshalData(
          IStream pStm
        );

    [DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern int CoRegisterActivationFilter(IActivationFilter pActivationFilter);

    [DllImport("ole32.dll")]
    public static extern int CoDecodeProxy(int dwClientPid, long ui64ProxyAddress, out ServerInformation pServerInformation);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern int PackageIdFromFullName(
      string packageFullName,
      int flags,
      ref int bufferLength,
      SafeBuffer buffer
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern Win32Error GetPackagePath(
      SafeBuffer packageId,
      int reserved,
      ref int pathLength,
      StringBuilder path
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern Win32Error PackageIdFromFullName(string packageFullName, int flags,
        ref int bufferLength,
        SafeHGlobalBuffer buffer
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern Win32Error GetPackageFullName(
        SafeKernelObjectHandle hProcess,
        ref int packageFullNameLength,
        StringBuilder packageFullName
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern Win32Error PackageFullNameFromId(PACKAGE_ID_ALLOC packageId,
        ref int packageFullNameLength, StringBuilder packageFullName);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint InspectHStringCallback2(IntPtr context, long readAddress, int length, IntPtr buffer);

    [DllImport("combase.dll")]
    public static extern int WindowsInspectString2(long targetHString, DllMachineType machine, InspectHStringCallback2 callback,
        IntPtr context, out int length, out long targetStringAddress);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint InspectHStringCallback(IntPtr context, IntPtr readAddress, int length, IntPtr buffer);

    [DllImport("combase.dll")]
    public static extern int WindowsInspectString(IntPtr targetHString, DllMachineType machine, InspectHStringCallback callback,
        IntPtr context, out int length, out IntPtr targetStringAddress);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true)]
    public extern static IntPtr LocalFree(IntPtr hMem);

    [DllImport("ole32.dll")]
    public extern static int CoGetSystemSecurityPermissions(COMSD comSDType, out IntPtr ppSD);

    [DllImport("kernel32.dll", SetLastError = true)]
    public extern static bool IsWow64Process2(SafeKernelObjectHandle hProcess, out DllMachineType pProcessMachine, out DllMachineType pNativeMachine);
}
