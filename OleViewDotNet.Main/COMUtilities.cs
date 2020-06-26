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

using Microsoft.CSharp;
using Microsoft.Win32;
using NtApiDotNet;
using NtApiDotNet.Ndr;
using NtApiDotNet.Win32;
using OleViewDotNet.Database;
using OleViewDotNet.Forms;
using OleViewDotNet.Wrappers;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OleViewDotNet
{
    class TypeLibCallback : ITypeLibImporterNotifySink
    {
        public Assembly ResolveRef(object tl)
        {
            return COMUtilities.ConvertTypeLibToAssembly((ITypeLib)tl, _progress);
        }

        public void ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg)
        {
            if ((eventKind == ImporterEventKind.NOTIF_TYPECONVERTED) && (_progress != null))
            {
                _progress.Report(new Tuple<string, int>(eventMsg, -1));
            }
        }

        public TypeLibCallback(IProgress<Tuple<string, int>> progress)
        {
            _progress = progress;
        }

        private IProgress<Tuple<string, int>> _progress;
    }

    public class RegistryValue
    {
        public string Name { get; }
        public object Value { get; }

        internal RegistryValue(string name, object value)
        {
            Name = name;
            Value = value ?? string.Empty;
        }
    }

    [Flags]
    public enum CLSCTX : uint
    {
        INPROC_SERVER = 0x1,
        INPROC_HANDLER = 0x2,
        LOCAL_SERVER = 0x4,
        INPROC_SERVER16 = 0x8,
        REMOTE_SERVER = 0x10,
        INPROC_HANDLER16 = 0x20,
        RESERVED1 = 0x40,
        RESERVED2 = 0x80,
        RESERVED3 = 0x100,
        RESERVED4 = 0x200,
        NO_CODE_DOWNLOAD = 0x400,
        RESERVED5 = 0x800,
        NO_CUSTOM_MARSHAL = 0x1000,
        ENABLE_CODE_DOWNLOAD = 0x2000,
        NO_FAILURE_LOG = 0x4000,
        DISABLE_AAA = 0x8000,
        ENABLE_AAA = 0x10000,
        FROM_DEFAULT_CONTEXT = 0x20000,
        ACTIVATE_32_BIT_SERVER = 0x40000,
        ACTIVATE_64_BIT_SERVER = 0x80000,
        ENABLE_CLOAKING = 0x100000,
        APPCONTAINER = 0x400000,
        ACTIVATE_AAA_AS_IU = 0x800000,
        ACTIVATE_NATIVE_SERVER = 0x1000000,
        ACTIVATE_ARM32_SERVER = 0x2000000,
        PS_DLL = 0x80000000,
        SERVER = INPROC_SERVER | LOCAL_SERVER | REMOTE_SERVER,
        ALL = INPROC_SERVER | INPROC_HANDLER | LOCAL_SERVER | REMOTE_SERVER
    }

    [Flags]
    public enum REGCLS
    {
        SINGLEUSE = 0,
        MULTIPLEUSE = 1,
        MULTI_SEPARATE = 2,
        SUSPENDED = 4,
        SURROGATE = 8,
        AGILE = 0x10,
    }

    [Flags]
    public enum STGM
    {
        READ = 0x00000000,
        WRITE = 0x00000001,
        READWRITE = 0x00000002,
        SHARE_DENY_NONE = 0x00000040,
        SHARE_DENY_READ = 0x00000030,
        SHARE_DENY_WRITE = 0x00000020,
        SHARE_EXCLUSIVE = 0x00000010,
        PRIORITY = 0x00040000,
        CREATE = 0x00001000,
        CONVERT = 0x00020000,
        FAILIFTHERE = READ,
        DIRECT = READ,
        TRANSACTED = 0x00010000,
        NOSCRATCH = 0x00100000,
        NOSNAPSHOT = 0x00200000,
        SIMPLE = 0x08000000,
        DIRECT_SWMR = 0x00400000,
        DELETEONRELEASE = 0x04000000
    }

    public enum STGC
    {
        DEFAULT,
        OVERWRITE,
        ONLYIFCURRENT,
        DANGEROUSLYCOMMITMERELYTODISKCACHE,
        CONSOLIDATE
    }

    [Flags]
    public enum EOLE_AUTHENTICATION_CAPABILITIES
    {
        NONE = 0,
        MUTUAL_AUTH = 0x1,
        STATIC_CLOAKING = 0x20,
        DYNAMIC_CLOAKING = 0x40,
        ANY_AUTHORITY = 0x80,
        MAKE_FULLSIC = 0x100,
        DEFAULT = 0x800,
        SECURE_REFS = 0x2,
        ACCESS_CONTROL = 0x4,
        APPID = 0x8,
        DYNAMIC = 0x10,
        REQUIRE_FULLSIC = 0x200,
        AUTO_IMPERSONATE = 0x400,
        NO_CUSTOM_MARSHAL = 0x2000,
        DISABLE_AAA = 0x1000
    }

    public enum RPC_AUTHN_LEVEL
    {
        DEFAULT       = 0,
        NONE          = 1,
        CONNECT       = 2,
        CALL          = 3,
        PKT           = 4,
        PKT_INTEGRITY = 5,
        PKT_PRIVACY   = 6,
    }

    public enum RPC_IMP_LEVEL
    {
        DEFAULT      = 0,
        ANONYMOUS    = 1,
        IDENTIFY     = 2,
        IMPERSONATE  = 3,
        DELEGATE     = 4,
    }

    public enum GLOBALOPT_UNMARSHALING_POLICY_VALUES
    {
        NORMAL = 0,
        STRONG = 1,
        HYBRID = 2
    }

    public enum MSHCTX
    {
        LOCAL = 0,
        NOSHAREDMEM = 1,
        DIFFERENTMACHINE = 2,
        INPROC = 3,
        CROSSCTX = 4
    }

    public enum MSHLFLAGS
    {
        NORMAL = 0,
        TABLESTRONG = 1,
        TABLEWEAK = 2,
        NOPING = 4
    }

    public enum GuidFormat
    {
        String,
        Structure,
        Object,
        HexString,
        CSGuid,
        CSGuidAttribute,
        RpcUuid
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OptionalGuid : IDisposable
    {
        IntPtr pGuid;

        void IDisposable.Dispose()
        {
            if (pGuid != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pGuid);
                pGuid = IntPtr.Zero;
            }
        }

        public OptionalGuid(Guid guid)
        {
            pGuid = Marshal.AllocCoTaskMem(16);
            Marshal.Copy(guid.ToByteArray(), 0, pGuid, 16);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MULTI_QI : IDisposable
    {
        OptionalGuid pIID;
        IntPtr pItf;
        int hr;

        public object GetObject()
        {
            if (pItf == IntPtr.Zero)
            {
                return null;
            }
            else
            {
                return Marshal.GetObjectForIUnknown(pItf);
            }
        }

        public IntPtr GetObjectPointer()
        {
            if (pItf != IntPtr.Zero)
            {
                Marshal.AddRef(pItf);
            }
            return pItf;
        }

        public int HResult()
        {
            return hr;
        }

        void IDisposable.Dispose()
        {
            ((IDisposable)pIID).Dispose();
            if (pItf != IntPtr.Zero)
            {
                Marshal.Release(pItf);
                pItf = IntPtr.Zero;
            }
        }

        public MULTI_QI(Guid iid)
        {
            pIID = new OptionalGuid(iid);
            pItf = IntPtr.Zero;
            hr = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public sealed class COSERVERINFO : IDisposable
    {
        int dwReserved1;
        [MarshalAs(UnmanagedType.LPWStr)]
        string pwszName;
        IntPtr pAuthInfo;
        int dwReserved2;

        void IDisposable.Dispose()
        {
            if (pAuthInfo != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pAuthInfo);
            }
        }

        public COSERVERINFO(string name)
        {
            pwszName = name;
        }   
    }

    [StructLayout(LayoutKind.Sequential)]
    public class BIND_OPTS3
    {
        int cbStruct;
        public int grfFlags;
        public int grfMode;
        public int dwTickCountDeadline;
        public int dwTrackFlags;
        public CLSCTX dwClassContext;
        public int locale;
        public IntPtr pServerInfo;
        public IntPtr hwnd;

        public BIND_OPTS3()
        {
            cbStruct = Marshal.SizeOf(this);
        }
    }

    [Flags]
    public enum CreateUrlMonikerFlags
    {
        Legacy = 0,
        Uniform = 1,
        NoCanonicalize = 2,
    }

    public enum STGFMT
    {
        Storage = 0,
        File = 3,
        Any = 4,
        Docfile = 5
    }

    public enum STGTY
    {
        None = 0,
        Storage = 1,
        Stream = 2,
        Lockbytes = 3,
        Property = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    public class STGOPTIONS
    {
        public short usVersion;
        public short reserved;
        public int ulSectorSize;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pwcsTemplateFile;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServerInformation
    {
        public int dwServerPid;
        public int dwServerTid;
        public long ui64ServerAddress;
    }

    public static class COMUtilities
    {
        private enum RegKind
        {
            RegKind_Default = 0,
            RegKind_Register = 1,
            RegKind_None = 2
        }

        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void LoadTypeLibEx(String strTypeLibName, RegKind regKind,
            [MarshalAs(UnmanagedType.Interface)] out ITypeLib typeLib);
        [DllImport("ole32.dll")]
        public static extern int CoCreateInstance(ref Guid rclsid, IntPtr pUnkOuter, CLSCTX dwClsContext, ref Guid riid, out IntPtr ppv);
        [DllImport("ole32.dll")]
        public static extern int CoCreateInstanceEx(ref Guid rclsid, IntPtr punkOuter, CLSCTX dwClsCtx, [In] COSERVERINFO pServerInfo, int dwCount, [In, Out] MULTI_QI[] pResults);
        [DllImport("ole32.dll")]
        public static extern int CoGetClassObject(ref Guid rclsid, CLSCTX dwClsContext, [In] COSERVERINFO pServerInfo, ref Guid riid, out IntPtr ppv);
        [DllImport("ole32.dll", PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.IUnknown)]
        public static extern object CoUnmarshalInterface(IStream stm, ref Guid riid);

        [DllImport("combase.dll", CharSet = CharSet.Unicode)]
        public static extern int RoGetActivationFactory([MarshalAs(UnmanagedType.HString)] string activatableClassId,
            ref Guid iid,
            out IntPtr factory
        );

        [DllImport("combase.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern ulong CoRegisterConsoleHandles(SafeHandle stdInputHandle, SafeHandle stdOutputHandle, SafeHandle stdErrorHandle);

        [DllImport("combase.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern ulong CoRegisterRacActivationToken(SafeKernelObjectHandle racActivationToken);

        [DllImport("combase.dll", EntryPoint = "#65", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern IExtensionRegistration RoGetExtensionRegistration([MarshalAs(UnmanagedType.HString)]string contractId,
                [MarshalAs(UnmanagedType.HString)]string packageId,
                [MarshalAs(UnmanagedType.HString)]string activatableClassId);

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
              ref Guid riid
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
              ref Guid riid
            );

        [DllImport("ole32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = false)]
        public static extern void CoGetObject(string pszName, BIND_OPTS3 pBindOptions, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

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
        public static extern void CoMarshalInterface(IStream pStm, ref Guid riid,
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
        static extern int PackageIdFromFullName(
          string packageFullName,
          int flags,
          ref int bufferLength,
          SafeBuffer buffer
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern int GetPackagePath(
          SafeBuffer packageId,
          int reserved,
          ref int pathLength,
          StringBuilder path
        );

        private static Dictionary<Guid, Assembly> m_typelibs;
        private static Dictionary<string, Assembly> m_typelibsname;
        private static Dictionary<Guid, Type> m_iidtypes;

        static COMUtilities()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (m_typelibsname != null)
            {
                lock (m_typelibsname)
                {
                    if (m_typelibsname.ContainsKey(args.Name))
                    {
                        return m_typelibsname[args.Name];
                    }
                }
            }

            return null;
        }

        public static object ReadObject(this RegistryKey rootKey, string keyName = null, string valueName = null, RegistryValueOptions options = RegistryValueOptions.None)
        {
            RegistryKey key = rootKey;

            try
            {
                if (keyName != null)
                {
                    key = rootKey.OpenSubKey(keyName);
                }

                if (key != null)
                {
                    return key.GetValue(valueName, null, options);
                }
            }
            finally
            {
                if (key != null && key != rootKey)
                {
                    key.Close();
                }
            }
            return null;
        }

        public static string ReadString(this RegistryKey rootKey, string keyName = null, string valueName = null, RegistryValueOptions options = RegistryValueOptions.None)
        {
            object valueObject = ReadObject(rootKey, keyName, valueName, options);
            string valueString = string.Empty;
            if (valueObject != null)
            {
                valueString = valueObject.ToString();
            }

            int first_nul = valueString.IndexOf('\0');
            if (first_nul >= 0)
            {
                valueString = valueString.Substring(0, first_nul);
            }

            return valueString;
        }

        public static string ReadStringPath(this RegistryKey rootKey, string basePath, string keyName = null, string valueName = null, RegistryValueOptions options = RegistryValueOptions.None)
        {
            string filePath = ReadString(rootKey, keyName, valueName, options);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return string.Empty;
            }
            return Path.Combine(basePath, filePath);
        }

        public static int ReadInt(this RegistryKey rootKey, string keyName, string valueName)
        {
            object obj = rootKey.ReadObject(keyName, valueName, RegistryValueOptions.None);
            if (obj == null)
            {
                return 0;
            }

            if (obj is int i)
            {
                return i;
            }

            if (obj is uint u)
            {
                return (int)u;
            }

            if (int.TryParse(obj.ToString(), out int ret))
            {
                return ret;
            }

            return 0;
        }

        public static bool ReadBool(this RegistryKey rootKey, string valueName = null, string keyName = null)
        {
            return ReadInt(rootKey, keyName, valueName) != 0;
        }

        public static Guid ReadGuid(this RegistryKey rootKey, string keyName, string valueName)
        {
            string guid = rootKey.ReadString(keyName, valueName);
            Guid ret = Guid.Empty;
            if (guid != null && Guid.TryParse(guid, out ret))
            {
                return ret;
            }

            return Guid.Empty;
        }

        public static string ReadSddl(this RegistryKey rootKey, string valueName = null, string keyName = null)
        {
            return COMSecurity.GetStringSDForSD(rootKey.ReadObject(keyName, valueName) as byte[]);
        }

        public static IEnumerable<RegistryValue> ReadValues(this RegistryKey rootKey, string keyName = null)
        {
            RegistryKey key = rootKey;

            try
            {
                if (keyName != null)
                {
                    key = rootKey.OpenSubKey(keyName);
                }

                if (key != null)
                {
                    yield return new RegistryValue("", key.GetValue(null));
                    foreach (var valueName in key.GetValueNames())
                    {
                        yield return new RegistryValue(valueName, key.GetValue(valueName));
                    }
                }
            }
            finally
            {
                if (key != null && key != rootKey)
                {
                    key.Close();
                }
            }
        }

        public static IEnumerable<string> ReadValueNames(this RegistryKey rootKey, string keyName = null)
        {
            RegistryKey key = rootKey;

            try
            {
                if (keyName != null)
                {
                    key = rootKey.OpenSubKey(keyName);
                }

                if (key != null)
                {
                    return key.GetValueNames();
                }
            }
            finally
            {
                if (key != null && key != rootKey)
                {
                    key.Close();
                }
            }

            return new string[0];
        }

        public static Guid? ReadOptionalGuid(string value)
        {
            if (Guid.TryParse(value, out Guid result))
            {
                return result;
            }
            return null;
        }

        public static COMRegistryEntrySource GetSource(this RegistryKey key)
        {
            using (NtKey native_key = NtKey.FromHandle(key.Handle.DangerousGetHandle(), false))
            {
                string full_path = native_key.FullPath;
                if (full_path.StartsWith(@"\Registry\Machine\", StringComparison.OrdinalIgnoreCase))
                {
                    return COMRegistryEntrySource.LocalMachine;
                }
                else if (full_path.StartsWith(@"\Registry\User\", StringComparison.OrdinalIgnoreCase))
                {
                    return COMRegistryEntrySource.User;
                }
                return COMRegistryEntrySource.Unknown;
            }
        }

        public static RegistryKey OpenSubKeySafe(this RegistryKey rootKey, string keyName)
        {
            try
            {
                return rootKey.OpenSubKey(keyName);
            }
            catch (SecurityException)
            {
                return null;
            }
        }

        public static string GetCategoryName(Guid catid)
        {
            Guid clsid = new Guid("{0002E005-0000-0000-C000-000000000046}");
            Guid iid = typeof(ICatInformation).GUID;
            IntPtr pCatMgr;
            string strDesc = String.Empty;

            if (CoCreateInstance(ref clsid, IntPtr.Zero, CLSCTX.INPROC_SERVER, ref iid, out pCatMgr) == 0)
            {
                ICatInformation catInfo = (ICatInformation)Marshal.GetObjectForIUnknown(pCatMgr);
                IntPtr pStrDesc;

                try
                {
                    catInfo.GetCategoryDesc(ref catid, 0, out pStrDesc);
                    strDesc = Marshal.PtrToStringUni(pStrDesc);
                    Marshal.FreeCoTaskMem(pStrDesc);
                }
                catch (COMException)
                {
                }

                Marshal.ReleaseComObject(catInfo);
                Marshal.Release(pCatMgr);
            }

            if (String.IsNullOrWhiteSpace(strDesc))
            {
                if (catid == new Guid("59fb2056-d625-48d0-a944-1a85b5ab2640"))
                {
                    strDesc = "AppContainer Compatible";
                }
                else
                {
                    strDesc = catid.FormatGuid();
                }
            }

            return strDesc;
        }

        public static string GetAppDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
        }

        public static string Get32bitExePath()
        {
            string path = Path.Combine(GetAppDirectory(), "OleViewDotNet32.exe");
            if (!File.Exists(path))
            {
                path = GetExePath();
            }
            return path;
        }

        public static string GetExePath()
        {
            return Path.Combine(GetAppDirectory(), "OleViewDotNet.exe");
        }

        public static string GetExePathForCurrentBitness()
        {
            if (!Environment.Is64BitOperatingSystem || Environment.Is64BitProcess)
            {
                return GetExePath();
            }
            return Get32bitExePath();
        }

        public static string GetAppDataDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OleViewDotNet");
        }

        public static string GetTypeLibDirectory()
        {
            return Path.Combine(GetAppDataDirectory(), "typelib");
        }

        public static string GetPluginDirectory()
        {
            return Path.Combine(GetAppDirectory(), "plugin");
        }

        public static string GetAutoSaveLoadPath()
        {
            string autosave = Path.Combine(GetAppDataDirectory(), "autosave");
            Directory.CreateDirectory(autosave);
            return Path.Combine(autosave, Environment.Is64BitProcess ? "com64.db" : "com32.db");
        }

        public static Dictionary<string, int> GetSymbolFile(bool native)
        {
            var ret = new Dictionary<string, int>();
            try
            {
                string system_path = native ? Environment.SystemDirectory : Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
                string dll_path = Path.Combine(system_path, $"{GetCOMDllName()}.dll");
                string symbol_path = Path.Combine(GetAppDirectory(), "symbol_cache", $"{GetFileMD5(dll_path)}.sym");
                foreach (var line in File.ReadAllLines(symbol_path).Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith("#")))
                {
                    string[] parts = line.Split(new char[] { ' ' }, 2);
                    if (parts.Length != 2)
                    {
                        continue;
                    }
                    if (!int.TryParse(parts[0], out int address))
                    {
                        continue;
                    }
                    ret[parts[1]] = address;
                }
            }
            catch
            {
            }
            return ret;
        }

        private static void AddToDictionary(Dictionary<string, int> base_dict, Dictionary<string, int> add_dict)
        {
            foreach (var pair in add_dict)
            {
                base_dict[pair.Key] = pair.Value;
            }
        }

        private static bool _cached_symbols_configured;

        public static void SetupCachedSymbols()
        {
            if (!_cached_symbols_configured)
            {
                _cached_symbols_configured = true;
                // Load any supported symbol files.
                AddToDictionary(SymbolResolverWrapper.GetResolvedNative(), GetSymbolFile(true));
                if (Environment.Is64BitProcess)
                {
                    AddToDictionary(SymbolResolverWrapper.GetResolved32Bit(), GetSymbolFile(false));
                }
            }
        }

        public static void ClearCachedSymbols()
        {
            _cached_symbols_configured = false;
            SymbolResolverWrapper.GetResolvedNative().Clear();
            SymbolResolverWrapper.GetResolved32Bit().Clear();
        }

        private static void RegisterTypeInterfaces(Assembly a)
        {
            Type[] types = a.GetTypes();

            foreach (Type t in types)
            {
                if (t.IsInterface && t.IsPublic)
                {
                    InterfaceViewers.InterfaceViewers.AddFactory(new InterfaceViewers.InstanceTypeViewerFactory(t));
                    if (!m_iidtypes.ContainsKey(t.GUID))
                    {
                        m_iidtypes.Add(t.GUID, t);
                    }
                }
            }
        }

        private static void LoadBuiltinTypes(Assembly asm)
        {
            foreach (Type t in asm.GetTypes().Where(x => x.IsPublic && x.IsInterface && IsComImport(x)))
            {
                if (t.GetCustomAttribute<ObsoleteAttribute>() != null)
                {
                    continue;
                }
                if (!m_iidtypes.ContainsKey(t.GUID))
                {
                    m_iidtypes.Add(t.GUID, t);
                }
            }
        }

        public static Type GetInterfaceType(Guid iid, COMRegistry registry)
        {
            if (registry != null && registry.Interfaces.ContainsKey(iid))
            {
                return GetInterfaceType(registry.Interfaces[iid]);
            }

            return GetInterfaceType(iid);
        }

        public static Type GetInterfaceType(Guid iid)
        {
            if (m_iidtypes == null)
            {
                LoadTypeLibAssemblies();
            }

            if (m_iidtypes.ContainsKey(iid))
            {
                return m_iidtypes[iid];
            }

            return null;
        }

        public static Type GetInterfaceType(COMInterfaceEntry intf)
        {
            if (intf == null)
            {
                return null;
            }

            Type type = GetInterfaceType(intf.Iid);
            if (type != null)
            {
                return type;
            }

            if (intf.ProxyClassEntry == null)
            {
                return null;
            }

            ConvertProxyToAssembly(COMProxyInterfaceInstance.GetFromIID(intf, null), null);
            return GetInterfaceType(intf.Iid);
        }

        public static Type GetInterfaceType(COMIPIDEntry ipid)
        {
            if (ipid == null)
            {
                return null;
            }

            Type type = GetInterfaceType(ipid.Iid);
            if (type != null)
            {
                return type;
            }

            COMProxyInstance proxy = ipid.ToProxyInstance();

            if (proxy == null)
            {
                return null;
            }

            ConvertProxyToAssembly(proxy, null);
            return GetInterfaceType(ipid.Iid);
        }

        public static void LoadTypesFromAssembly(Assembly assembly)
        {
            if (m_iidtypes == null)
            {
                LoadTypeLibAssemblies();
            }

            LoadBuiltinTypes(assembly);
        }

        public static void LoadTypeLibAssemblies()
        {
            if (m_typelibs == null)
            {
                try
                {
                    string strTypeLibDir = GetTypeLibDirectory();
                    Directory.CreateDirectory(strTypeLibDir);
                    string[] files = Directory.GetFiles(strTypeLibDir, "*.dll");

                    m_typelibs = new Dictionary<Guid, Assembly>();
                    m_iidtypes = new Dictionary<Guid, Type>();
                    m_typelibsname = new Dictionary<string, Assembly>();

                    foreach (string f in files)
                    {
                        try
                        {
                            Assembly a = Assembly.LoadFrom(f);
                            if (!m_typelibs.ContainsKey(Marshal.GetTypeLibGuidForAssembly(a)))
                            {
                                m_typelibs.Add(Marshal.GetTypeLibGuidForAssembly(a), a);

                                lock (m_typelibsname)
                                {
                                    m_typelibsname[a.FullName] = a;
                                }

                                RegisterTypeInterfaces(a);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }

                LoadBuiltinTypes(Assembly.GetExecutingAssembly());
                LoadBuiltinTypes(typeof(int).Assembly);
            }
        }

        public static Assembly LoadTypeLib(string path, IProgress<Tuple<string, int>> progress)
        {
            ITypeLib typeLib = null;

            try
            {
                LoadTypeLibEx(path, RegKind.RegKind_Default, out typeLib);

                return ConvertTypeLibToAssembly(typeLib, progress);
            }
            finally
            {
                if (typeLib != null)
                {
                    Marshal.ReleaseComObject(typeLib);
                }
            }
        }

        public static Assembly LoadTypeLib(ITypeLib typeLib, IProgress<Tuple<string, int>> progress)
        {
            try
            {
                return ConvertTypeLibToAssembly(typeLib, progress);
            }
            finally
            {
                if (typeLib != null)
                {
                    Marshal.ReleaseComObject(typeLib);
                }
            }
        }

        public static Assembly ConvertTypeLibToAssembly(ITypeLib typeLib, IProgress<Tuple<string, int>> progress)
        {
            if (m_typelibs == null)
            {
                if (progress != null)
                {
                    progress.Report(new Tuple<string, int>("Initializing Global Libraries", -1));
                }
                LoadTypeLibAssemblies();
            }

            if (m_typelibs.ContainsKey(Marshal.GetTypeLibGuid(typeLib)))
            {
                return m_typelibs[Marshal.GetTypeLibGuid(typeLib)];
            }
            else
            {
                string strAssemblyPath = GetTypeLibDirectory();
                strAssemblyPath = Path.Combine(strAssemblyPath, Marshal.GetTypeLibGuid(typeLib).ToString() + ".dll");

                TypeLibConverter conv = new TypeLibConverter();
                AssemblyBuilder asm = conv.ConvertTypeLibToAssembly(typeLib, strAssemblyPath, TypeLibImporterFlags.ReflectionOnlyLoading,
                                        new TypeLibCallback(progress), null, null, null, null);
                asm.Save(Path.GetFileName(strAssemblyPath));
                Assembly a = Assembly.LoadFile(strAssemblyPath);

                m_typelibs[Marshal.GetTypeLibGuid(typeLib)] = a;
                lock (m_typelibsname)
                {
                    m_typelibsname[a.FullName] = a;
                }
                RegisterTypeInterfaces(a);

                return a;
            }
        }

        public static void ConvertProxyToAssembly(IEnumerable<NdrComProxyDefinition> entries, string output_path, IProgress<Tuple<string, int>> progress)
        {
            if (m_typelibs == null)
            {
                if (progress != null)
                {
                    progress.Report(Tuple.Create("Initializing Global Libraries", -1));
                }
                LoadTypeLibAssemblies();
            }

            COMProxyInstanceConverter converter = new COMProxyInstanceConverter(output_path, progress);
            converter.AddProxy(entries);
            converter.Save();
        }

        public static void ConvertProxyToAssembly(COMProxyInstance proxy, string output_path, IProgress<Tuple<string, int>> progress)
        {
            ConvertProxyToAssembly(proxy.Entries, output_path, progress);
        }

        public static void ConvertProxyToAssembly(COMProxyInterfaceInstance proxy, string output_path, IProgress<Tuple<string, int>> progress)
        {
            ConvertProxyToAssembly(new[] { proxy.Entry }, output_path, progress);
        }

        public static void ConvertProxyToAssembly(COMIPIDEntry ipid, string output_path, IProgress<Tuple<string, int>> progress)
        {
            ConvertProxyToAssembly(ipid.ToProxyInstance(), output_path, progress);
        }

        public static Assembly ConvertProxyToAssembly(IEnumerable<NdrComProxyDefinition> entries, IProgress<Tuple<string, int>> progress)
        {
            if (m_typelibs == null)
            {
                if (progress != null)
                {
                    progress.Report(new Tuple<string, int>("Initializing Global Libraries", -1));
                }
                LoadTypeLibAssemblies();
            }

            COMProxyInstanceConverter converter = new COMProxyInstanceConverter($"{Guid.NewGuid()}.dll", progress);
            converter.AddProxy(entries);
            RegisterTypeInterfaces(converter.BuiltAssembly);
            return converter.BuiltAssembly;
        }

        public static Assembly ConvertProxyToAssembly(COMProxyInstance proxy, IProgress<Tuple<string, int>> progress)
        {
            return ConvertProxyToAssembly(proxy.Entries, progress);
        }

        public static Assembly ConvertProxyToAssembly(COMProxyInterfaceInstance proxy, IProgress<Tuple<string, int>> progress)
        {
            return ConvertProxyToAssembly(new[] { proxy.Entry }, progress);
        }

        public static Assembly ConvertProxyToAssembly(COMIPIDEntry ipid, IProgress<Tuple<string, int>> progress)
        {
            return ConvertProxyToAssembly(ipid.ToProxyInstance(), progress);
        }

        public static Type GetDispatchTypeInfo(IWin32Window parent, object comObj)
        {
            Type ret = null;

            try
            {
                if (!comObj.GetType().IsCOMObject)
                {
                    ret = comObj.GetType();
                }
                else
                {
                    IntPtr typeInfo = IntPtr.Zero;

                    try
                    {
                        IDispatch disp = (IDispatch)comObj;

                        disp.GetTypeInfo(0, 0x409, out typeInfo);

                        ITypeInfo ti = (ITypeInfo)Marshal.GetObjectForIUnknown(typeInfo);
                        ITypeLib tl = null;
                        int iIndex = 0;
                        ti.GetContainingTypeLib(out tl, out iIndex);
                        Guid typelibGuid = Marshal.GetTypeLibGuid(tl);
                        Assembly asm = LoadTypeLib(parent, tl);

                        if (asm != null)
                        {
                            string name = Marshal.GetTypeInfoName(ti);
                            ret = asm.GetTypes().First(t => t.Name == name);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        if (typeInfo != IntPtr.Zero)
                        {
                            Marshal.Release(typeInfo);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return ret;
        }

        public static bool IsComImport(Type t)
        {
            return t.GetCustomAttributes(typeof(ComImportAttribute), false).Length > 0 ||
                t.GetCustomAttributes(typeof(InterfaceTypeAttribute), false).Length > 0;
        }

        private static Dictionary<Type, Type> _wrappers = new Dictionary<Type, Type>();

        private static CodeParameterDeclarationExpression GetParameter(ParameterInfo pi)
        {
            Type baseType = pi.ParameterType;

            if (baseType.IsByRef)
            {
                string name = baseType.FullName.TrimEnd('&');

                baseType = baseType.Assembly.GetType(name);
            }

            CodeParameterDeclarationExpression p = new CodeParameterDeclarationExpression(baseType, pi.Name);
            FieldDirection d = FieldDirection.In;

            if ((pi.Attributes & ParameterAttributes.Out) == ParameterAttributes.Out)
            {
                d = FieldDirection.Out;
            }

            if ((pi.Attributes & ParameterAttributes.In) == ParameterAttributes.In)
            {
                if (d == FieldDirection.Out)
                {
                    d = FieldDirection.Ref;
                }
                else
                {
                    d = FieldDirection.In;
                }
            }

            p.Direction = d;

            return p;
        }

        private static CodeMemberMethod CreateForwardingMethod(MethodInfo mi)
        {
            CodeMemberMethod method = new CodeMemberMethod
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = mi.Name,
                ReturnType = new CodeTypeReference(mi.ReturnType)
            };

            List<CodeExpression> parameters = new List<CodeExpression>();

            foreach (ParameterInfo pi in mi.GetParameters())
            {
                CodeParameterDeclarationExpression p = GetParameter(pi);
                method.Parameters.Add(p);
                parameters.Add(new CodeDirectionExpression(p.Direction, new CodeVariableReferenceExpression(pi.Name)));
            }

            CodeMethodInvokeExpression invokeExpr = new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_target"),
                mi.Name, parameters.ToArray());

            if (mi.ReturnType != typeof(void))
            {
                method.Statements.Add(new CodeMethodReturnStatement(invokeExpr));
            }
            else
            {
                method.Statements.Add(invokeExpr);
            }

            return method;
        }

        private static CodeMemberProperty CreateForwardingProperty(PropertyInfo pi)
        {
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            prop.Name = pi.Name;
            prop.Type = new CodeTypeReference(pi.PropertyType);

            CodePropertyReferenceExpression propExpr = new CodePropertyReferenceExpression(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_target"),
                pi.Name);

            if (pi.CanRead)
            {
                prop.GetStatements.Add(new CodeMethodReturnStatement(propExpr));
            }

            if (pi.CanWrite)
            {
                prop.SetStatements.Add(new CodeAssignStatement(propExpr, new CodeVariableReferenceExpression("value")));
            }

            return prop;
        }

        private static CodeTypeDeclaration CreateWrapperTypeDeclaration(Type t)
        {
            CodeTypeDeclaration type = new CodeTypeDeclaration(t.Name + "Wrapper");
            CodeTypeReference typeRef = new CodeTypeReference(t);

            type.IsClass = true;
            type.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            type.BaseTypes.Add(typeRef);

            type.Members.Add(new CodeMemberField(typeRef, "_target"));

            CodeConstructor defaultConstructor = new CodeConstructor();
            defaultConstructor.Attributes = MemberAttributes.Public;
            defaultConstructor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object)), "target"));
            defaultConstructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_target"), new CodeCastExpression(typeRef, new CodeVariableReferenceExpression("target"))));
            type.Members.Add(defaultConstructor);

            foreach (MethodInfo mi in t.GetMethods())
            {
                if (!mi.IsSpecialName)
                {
                    type.Members.Add(CreateForwardingMethod(mi));
                }
            }

            foreach (PropertyInfo pi in t.GetProperties())
            {
                type.Members.Add(CreateForwardingProperty(pi));
            }

            return type;
        }


        private static Type CreateWrapper(Type t)
        {
            Type ret = null;
            CodeCompileUnit unit = new CodeCompileUnit();
            CodeNamespace ns = new CodeNamespace();

            CSharpCodeProvider provider = new CSharpCodeProvider();

            CodeTypeDeclaration type = CreateWrapperTypeDeclaration(t);

            ns.Types.Add(type);
            unit.Namespaces.Add(ns);

            StringBuilder builder = new StringBuilder();
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.IndentString = "    ";
            options.BlankLinesBetweenMembers = false;

            TextWriter writer = new StringWriter(builder);

            provider.GenerateCodeFromCompileUnit(unit, writer, options);


            writer.Close();

            File.WriteAllText("dump.cs", builder.ToString());

            try
            {
                CompilerParameters compileParams = new CompilerParameters();
                TempFileCollection tempFiles = new TempFileCollection(Path.GetTempPath(), false);

                compileParams.GenerateExecutable = false;
                compileParams.GenerateInMemory = true;
                compileParams.IncludeDebugInformation = true;
                compileParams.TempFiles = tempFiles;
                compileParams.ReferencedAssemblies.Add("System.dll");
                compileParams.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
                compileParams.ReferencedAssemblies.Add("System.Core.dll");
                compileParams.ReferencedAssemblies.Add(t.Assembly.Location);

                CompilerResults results = provider.CompileAssemblyFromDom(compileParams, unit);

                if (results.Errors.HasErrors)
                {
                    foreach (CompilerError e in results.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                    }
                }
                else
                {
                    ret = results.CompiledAssembly.GetType(t.Name + "Wrapper");
                    if (ret != null)
                    {
                        lock (_wrappers)
                        {
                            _wrappers[t] = ret;
                        }
                    }
                }

            }
            catch (Exception)
            {
            }

            return ret;
        }

        public static dynamic CreateDynamicCallWrapper(object target, Type t)
        {
            Type instanceType = null;

            lock (_wrappers)
            {
                if (_wrappers.ContainsKey(t))
                {
                    instanceType = _wrappers[t];
                }
            }

            if (instanceType == null)
            {
                instanceType = CreateWrapper(t);
            }

            if (instanceType != null)
            {
                return Activator.CreateInstance(instanceType, target);
            }
            else
            {
                return null;
            }
        }

        public static void SaveObjectToStream(object obj, Stream stm)
        {
            IStreamImpl istm = new IStreamImpl(stm);

            IPersistStream ps = obj as IPersistStream;

            if (ps != null)
            {
                ps.Save(istm, false);
            }
            else
            {
                IPersistStreamInit psi = (IPersistStreamInit)obj;

                psi.Save(istm, false);
            }
        }

        public static void LoadObjectFromStream(object obj, Stream stm)
        {
            IStreamImpl istm = new IStreamImpl(stm);

            IPersistStream ps = obj as IPersistStream;

            if (ps != null)
            {
                ps.Load(istm);
            }
            else
            {
                IPersistStreamInit psi = (IPersistStreamInit)obj;

                psi.InitNew();
                psi.Load(istm);
            }
        }

        public static void OleSaveToStream(object obj, Stream stm)
        {
            using (BinaryWriter writer = new BinaryWriter(stm))
            {
                Guid clsid = GetObjectClass(obj);

                writer.Write(clsid.ToByteArray());

                SaveObjectToStream(obj, stm);
            }
        }

        public static object OleLoadFromStream(Stream stm, out Guid clsid)
        {
            using (BinaryReader reader = new BinaryReader(stm))
            {
                clsid = new Guid(reader.ReadBytes(16));

                Guid unk = COMInterfaceEntry.IID_IUnknown;
                IntPtr pObj;
                object ret;

                int iError = COMUtilities.CoCreateInstance(ref clsid, IntPtr.Zero, CLSCTX.SERVER,
                    ref unk, out pObj);

                if (iError != 0)
                {
                    Marshal.ThrowExceptionForHR(iError);
                }

                ret = Marshal.GetObjectForIUnknown(pObj);
                Marshal.Release(pObj);

                LoadObjectFromStream(ret, stm);

                return ret;
            }
        }

        public static object CreateFromMoniker(string moniker, BIND_OPTS3 bind_opts)
        {
            object ret;
            Guid iid = COMInterfaceEntry.IID_IUnknown;
            CoGetObject(moniker, bind_opts, ref iid, out ret);
            return ret;
        }

        public static object CreateFromMoniker(string moniker, CLSCTX clsctx)
        {
            BIND_OPTS3 bind_opts = new BIND_OPTS3();
            bind_opts.dwClassContext = clsctx;
            return CreateFromMoniker(moniker, bind_opts);
        }

        public static object UnmarshalObject(Stream stm, Guid iid)
        {
            return CoUnmarshalInterface(new IStreamImpl(stm), ref iid);
        }

        public static object UnmarshalObject(byte[] objref)
        {
            return UnmarshalObject(new MemoryStream(objref), COMInterfaceEntry.IID_IUnknown);
        }

        public static object UnmarshalObject(COMObjRef objref)
        {
            return UnmarshalObject(objref.ToArray());
        }

        public static Guid GetObjectClass(object p)
        {
            Guid ret = Guid.Empty;

            try
            {
                if (p is IPersist)
                {
                    ((IPersist)p).GetClassID(out ret);
                }
                else if (p is IPersistStream)
                {
                    ((IPersistStream)p).GetClassID(out ret);
                }
                else if (p is IPersistStreamInit)
                {
                    ((IPersistStreamInit)p).GetClassID(out ret);
                }
                else if (p is IPersistFile)
                {
                    ((IPersistFile)p).GetClassID(out ret);
                }
                else if (p is IPersistMoniker)
                {
                    ((IPersistMoniker)p).GetClassID(out ret);
                }
                else if (p is IPersistStorage)
                {
                    ((IPersistStorage)p).GetClassID(out ret);
                }
            }
            catch
            {
            }

            return ret;
        }

        public static string GetMonikerDisplayName(IMoniker pmk)
        {
            string strDisplayName;
            IBindCtx bindCtx = CreateBindCtx(0);

            pmk.GetDisplayName(bindCtx, null, out strDisplayName);

            Marshal.ReleaseComObject(bindCtx);

            return strDisplayName;
        }

        public static byte[] MarshalObject(object obj, Guid iid, MSHCTX mshctx, MSHLFLAGS mshflags)
        {
            MemoryStream stm = new MemoryStream();
            CoMarshalInterface(new IStreamImpl(stm), ref iid, obj, mshctx, IntPtr.Zero, mshflags);
            return stm.ToArray();
        }

        public static byte[] MarshalObject(object obj)
        {
            return MarshalObject(obj, COMInterfaceEntry.IID_IUnknown, MSHCTX.DIFFERENTMACHINE, MSHLFLAGS.NORMAL);
        }

        public static COMObjRef MarshalObjectToObjRef(object obj, Guid iid, MSHCTX mshctx, MSHLFLAGS mshflags)
        {
            return COMObjRef.FromArray(MarshalObject(obj, iid, mshctx, mshflags));
        }

        public static COMObjRef MarshalObjectToObjRef(object obj)
        {
            return MarshalObjectToObjRef(obj, COMInterfaceEntry.IID_IUnknown, MSHCTX.DIFFERENTMACHINE, MSHLFLAGS.NORMAL);
        }

        private static string ConvertTypeToName(Type t)
        {
            if (t == typeof(string))
            {
                return "string";
            }
            else if (t == typeof(byte))
            {
                return "byte";
            }
            else if (t == typeof(sbyte))
            {
                return "sbyte";
            }
            else if (t == typeof(short))
            {
                return "short";
            }
            else if (t == typeof(ushort))
            {
                return "ushort";
            }
            else if (t == typeof(int))
            {
                return "int";
            }
            else if (t == typeof(uint))
            {
                return "uint";
            }
            else if (t == typeof(long))
            {
                return "long";
            }
            else if (t == typeof(ulong))
            {
                return "ulong";
            }
            else if (t == typeof(void))
            {
                return "void";
            }
            else if (t == typeof(object))
            {
                return "object";
            }
            else if (t == typeof(bool))
            {
                return "bool";
            }

            return t.Name;
        }

        private static string FormatParameters(IEnumerable<ParameterInfo> pis)
        {
            List<string> pars = new List<string>();
            foreach (ParameterInfo pi in pis)
            {
                List<string> dirs = new List<string>();

                if (pi.IsOut)
                {
                    dirs.Add("Out");
                    if (pi.IsIn)
                    {
                        dirs.Add("In");
                    }
                }

                if (pi.IsRetval)
                {
                    dirs.Add("Retval");
                }

                if (pi.IsOptional)
                {
                    dirs.Add("Optional");
                }

                string text = String.Format("{0} {1}", ConvertTypeToName(pi.ParameterType), pi.Name);

                if (dirs.Count > 0)
                {
                    text = String.Format("[{0}] {1}", string.Join(",", dirs), text);
                }
                pars.Add(text);
            }
            return String.Join(", ", pars);
        }

        public static string MemberInfoToString(MemberInfo member)
        {
            if (member is MethodInfo mi)
            {
                return String.Format("{0} {1}({2});",
                    ConvertTypeToName(mi.ReturnType),
                    mi.Name, FormatParameters(mi.GetParameters()));
            }
            else if (member is PropertyInfo prop)
            {
                List<string> propdirs = new List<string>();
                if (prop.CanRead)
                {
                    propdirs.Add("get;");
                }

                if (prop.CanWrite)
                {
                    propdirs.Add("set;");
                }

                ParameterInfo[] index_params = prop.GetIndexParameters();
                string ps = String.Empty;
                if (index_params.Length > 0)
                {
                    ps = String.Format("({0})", FormatParameters(index_params));
                }

                return String.Format("{0} {1}{2} {{ {3} }}", ConvertTypeToName(prop.PropertyType), prop.Name, ps, string.Join(" ", propdirs));
            }
            else if (member is FieldInfo fi)
            {
                return String.Format("{0} {1}", ConvertTypeToName(fi.FieldType), fi.Name);
            }
            else if (member is EventInfo ei)
            {
                return string.Format("event {0} {1}", ei.EventHandlerType, ei.Name);
            }
            else
            {
                return null;
            }
        }

        public static bool HasSubkey(this RegistryKey key, string name)
        {
            using (RegistryKey subkey = key.OpenSubKey(name))
            {
                return subkey != null;
            }
        }

        internal static int GetSafeHashCode<T>(this T obj) where T : class
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetHashCode();
        }

        internal static int GetEnumHashCode<T>(this IEnumerable<T> e)
        {
            return e.Aggregate(0, (s, o) => s ^ o.GetHashCode());
        }

        internal static T[] EnumeratePointerList<T>(IntPtr p, Func<IntPtr, T> load_type)
        {
            List<T> ret = new List<T>();

            if (p == IntPtr.Zero)
            {
                return new T[0];
            }

            IntPtr curr = p;
            IntPtr value = IntPtr.Zero;
            while ((value = Marshal.ReadIntPtr(curr)) != IntPtr.Zero)
            {
                ret.Add(load_type(value));
                curr += IntPtr.Size;
            }
            return ret.ToArray();
        }

        internal static T[] EnumeratePointerList<T>(IntPtr p) where T : struct
        {
            return EnumeratePointerList(p, i => Marshal.PtrToStructure<T>(i));
        }

        internal static T[] ReadPointerArray<T>(IntPtr p, int count, Func<IntPtr, T> load_type)
        {
            T[] ret = new T[count];
            if (p == IntPtr.Zero)
            {
                return ret;
            }

            for (int i = 0; i < count; ++i)
            {
                IntPtr curr = Marshal.ReadIntPtr(p, i * IntPtr.Size);
                if (curr == IntPtr.Zero)
                {
                    ret[i] = default(T);
                }
                else
                {
                    ret[i] = load_type(curr);
                }
            }
            return ret;
        }

        internal static T[] ReadPointerArray<T>(IntPtr p, int count) where T : struct
        {
            return ReadPointerArray(p, count, i => Marshal.PtrToStructure<T>(i));
        }

        internal static Guid ReadGuid(IntPtr p)
        {
            if (p == IntPtr.Zero)
            {
                return COMInterfaceEntry.IID_IUnknown;
            }
            byte[] guid = new byte[16];
            Marshal.Copy(p, guid, 0, 16);
            return new Guid(guid);
        }

        internal static Guid ReadGuidFromArray(IntPtr p, int index)
        {
            if (p == IntPtr.Zero)
            {
                return Guid.Empty;
            }

            IntPtr guid_ptr = Marshal.ReadIntPtr(p, index * IntPtr.Size);
            return ReadGuid(guid_ptr);
        }

        internal static byte[] ReadAll(this BinaryReader reader, int length)
        {
            byte[] ret = reader.ReadBytes(length);
            if (ret.Length != length)
            {
                throw new EndOfStreamException();
            }
            return ret;
        }

        internal static Guid ReadGuid(this BinaryReader reader)
        {
            return new Guid(reader.ReadAll(16));
        }

        internal static char ReadUnicodeChar(this BinaryReader reader)
        {
            return BitConverter.ToChar(reader.ReadAll(2), 0);
        }

        internal static void Write(this BinaryWriter writer, Guid guid)
        {
            writer.Write(guid.ToByteArray());
        }

        internal static string ReadZString(this BinaryReader reader)
        {
            StringBuilder builder = new StringBuilder();
            char ch = reader.ReadUnicodeChar();
            while (ch != 0)
            {
                builder.Append(ch);
                ch = reader.ReadUnicodeChar();
            }
            return builder.ToString();
        }

        internal static void WriteZString(this BinaryWriter writer, string str)
        {
            writer.Write(Encoding.Unicode.GetBytes(str + "\0"));
        }

        private static string GetNextToken(string name, out string token)
        {
            token = null;
            if (name.Length == 0)
            {
                return name;
            }
            int end_index = name.IndexOf('_');
            if (end_index < 0)
            {
                token = name;
            }
            else
            {
                token = name.Substring(0, end_index);
            }
            return name.Substring(end_index + 1).TrimStart('_');
        }

        private static string GetNextToken(string name, out int token)
        {
            if (name.Length == 0 || !char.IsDigit(name[0]))
            {
                throw new InvalidDataException("Expected an integer");
            }
            int length = 0;
            while (char.IsDigit(name[length]))
            {
                length++;
            }

            token = int.Parse(name.Substring(0, length));

            return name.Substring(length).TrimStart('_');
        }

        private static string ReadType(ref string name)
        {
            string token;
            name = GetNextToken(name, out token);
            if (String.IsNullOrEmpty(token))
            {
                throw new InvalidDataException("Expected a type name");
            }

            if (char.IsLetter(token[0]))
            {
                return token;
            }
            else if (token[0] == '~')
            {
                StringBuilder builder = new StringBuilder();
                int type_count;

                name = GetNextToken(name, out type_count);
                builder.Append(token.Substring(1));
                builder.Append("<");
                List<string> types = new List<string>();
                for (int i = 0; i < type_count; ++i)
                {
                    types.Add(ReadType(ref name));
                }
                builder.Append(String.Join(",", types));
                builder.Append(">");
                return builder.ToString();
            }
            else
            {
                throw new InvalidDataException("Expected a type name or a generic type");
            }
        }

        private static ConcurrentDictionary<string, string> _demangled_names = new ConcurrentDictionary<string, string>();

        private static string DemangleGenericType(string name)
        {
            name = name.Replace("__F", "~").Replace("__C", "::");
            return ReadType(ref name);
        }

        // TODO: This isn't exactly correct, but can't find any good documentation.
        internal static string DemangleWinRTName(string name)
        {
            name = name.Trim();
            string result;
            if (_demangled_names.TryGetValue(name, out result))
            {
                return result;
            }

            result = name;

            if (name.StartsWith("__x_") || name.StartsWith("___x_"))
            {
                result = name.Substring(4).Replace("_C", "::");
            }
            else if (name.StartsWith("__F"))
            {
                try
                {
                    result = DemangleGenericType(name);
                }
                catch (InvalidDataException)
                {
                }
            }

            return _demangled_names.GetOrAdd(name, result);
        }

        internal static COMRegistry LoadRegistry(IWin32Window window,
            Func<IProgress<Tuple<string, int>>, CancellationToken, object> worker)
        {
            using (WaitingDialog loader = new WaitingDialog(worker))
            {
                if (loader.ShowDialog(window) == DialogResult.OK)
                {
                    return loader.Result as COMRegistry;
                }
                else
                {
                    throw loader.Error;
                }
            }
        }

        internal static COMRegistry LoadRegistry(IWin32Window window, COMRegistryMode mode)
        {
            if (mode == COMRegistryMode.Diff)
            {
                throw new ArgumentException("Can't load a diff registry");
            }
            return LoadRegistry(window, (progress, token) => COMRegistry.Load(mode, null, progress));
        }

        internal static COMRegistry LoadRegistry(IWin32Window window, string database_file)
        {
            return LoadRegistry(window, (progress, token) => COMRegistry.Load(database_file, progress));
        }

        internal static COMRegistry DiffRegistry(IWin32Window window, COMRegistry left, COMRegistry right, COMRegistryDiffMode mode)
        {
            return LoadRegistry(window, (progress, token) => COMRegistry.Diff(left, right, mode, progress));
        }

        internal static Assembly LoadTypeLib(IWin32Window window, string path)
        {
            using (WaitingDialog dlg = new WaitingDialog((progress, token) => COMUtilities.LoadTypeLib(path, progress), s => s))
            {
                dlg.Text = String.Format("Loading TypeLib {0}", path);
                dlg.CancelEnabled = false;
                if (dlg.ShowDialog(window) == DialogResult.OK)
                {
                    return (Assembly)dlg.Result;
                }
                else if ((dlg.Error != null) && !(dlg.Error is OperationCanceledException))
                {
                    EntryPoint.ShowError(window, dlg.Error);
                }
                return null;
            }
        }

        internal static Assembly LoadTypeLib(IWin32Window window, ITypeLib typelib)
        {
            using (WaitingDialog dlg = new WaitingDialog((progress, token) => LoadTypeLib(typelib, progress), s => s))
            {
                dlg.Text = "Loading TypeLib";
                dlg.CancelEnabled = false;
                if (dlg.ShowDialog(window) == DialogResult.OK)
                {
                    return (Assembly)dlg.Result;
                }
                else if ((dlg.Error != null) && !(dlg.Error is OperationCanceledException))
                {
                    EntryPoint.ShowError(window, dlg.Error);
                }
                return null;
            }
        }

        internal static COMProcessParserConfig GetProcessParserConfig()
        {
            string dbghelp = Environment.Is64BitProcess
                    ? Properties.Settings.Default.DbgHelpPath64
                    : Properties.Settings.Default.DbgHelpPath32;
            string symbol_path = Properties.Settings.Default.SymbolPath;
            bool parse_stub_methods = Properties.Settings.Default.ParseStubMethods;
            bool resolve_method_names = Properties.Settings.Default.ResolveMethodNames;
            bool parse_registered_classes = Properties.Settings.Default.ParseRegisteredClasses;
            bool parse_clients = Properties.Settings.Default.ParseClients;
            bool parse_activation_context = Properties.Settings.Default.ParseActivationContext;

            return new COMProcessParserConfig(dbghelp, symbol_path, parse_stub_methods,
                resolve_method_names, parse_registered_classes, parse_clients, parse_activation_context);
        }

        internal static IEnumerable<COMProcessEntry> LoadProcesses(IEnumerable<Process> procs, IWin32Window window, COMRegistry registry)
        {
            using (WaitingDialog dlg = new WaitingDialog((progress, token) => COMProcessParser.GetProcesses(procs, GetProcessParserConfig(), progress, registry), s => s))
            {
                dlg.Text = "Loading Processes";
                if (dlg.ShowDialog(window) == DialogResult.OK)
                {
                    return (IEnumerable<COMProcessEntry>)dlg.Result;
                }
                else if ((dlg.Error != null) && !(dlg.Error is OperationCanceledException))
                {
                    EntryPoint.ShowError(window, dlg.Error);
                }
                return null;
            }
        }

        internal static IEnumerable<COMProcessEntry> LoadProcesses(IEnumerable<int> pids, IWin32Window window, COMRegistry registry)
        {
            return LoadProcesses(pids.Select(p => Process.GetProcessById(p)), window, registry);
        }

        internal static IEnumerable<COMProcessEntry> LoadProcesses(IWin32Window window, COMRegistry registry)
        {
            int current_pid = Process.GetCurrentProcess().Id;
            var procs = Process.GetProcesses().Where(p => p.Id != current_pid).OrderBy(p => p.ProcessName);
            return LoadProcesses(procs, window, registry);
        }

        private class ReportQueryProgress
        {
            private int _total_count;
            private int _current;
            private IProgress<Tuple<string, int>> _progress;

            const int MINIMUM_REPORT_SIZE = 25;

            public ReportQueryProgress(IProgress<Tuple<string, int>> progress, int total_count)
            {
                _total_count = total_count;
                _progress = progress;
            }

            public void Report()
            {
                int current = Interlocked.Increment(ref _current);
                if ((current % MINIMUM_REPORT_SIZE) == 1)
                {
                    _progress.Report(new Tuple<string, int>(String.Format("Querying Interfaces: {0} of {1}", current, _total_count),
                        (100 * current) / _total_count));
                }
            }
        }

        private static bool QueryAllInterfaces(IEnumerable<COMCLSIDEntry> clsids, IProgress<Tuple<string, int>> progress, CancellationToken token, int concurrent_queries)
        {
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = token;
            po.MaxDegreeOfParallelism = concurrent_queries;

            ReportQueryProgress query_progress = new ReportQueryProgress(progress, clsids.Count());

            Parallel.ForEach(clsids, po, clsid =>
            {
                po.CancellationToken.ThrowIfCancellationRequested();
                try
                {
                    query_progress.Report();
                    clsid.LoadSupportedInterfaces(false, null);
                }
                catch
                {
                }
            });

            return true;
        }

        internal static bool QueryAllInterfaces(IWin32Window parent, IEnumerable<COMCLSIDEntry> clsids, IEnumerable<COMServerType> server_types, int concurrent_queries, bool refresh_interfaces)
        {
            using (WaitingDialog dlg = new WaitingDialog(
                (p, t) => COMUtilities.QueryAllInterfaces(clsids.Where(c => (refresh_interfaces || !c.InterfacesLoaded) && server_types.Contains(c.DefaultServerType)),
                            p, t, concurrent_queries),
                s => s))
            {
                dlg.Text = "Querying Interfaces";
                return dlg.ShowDialog(parent) == DialogResult.OK;
            }
        }

        internal static string FormatGuid(this Guid guid)
        {
            return guid.ToString(Properties.Settings.Default.GuidFormat).ToUpper();
        }

        internal static string FormatComClassNameAsCIdentifier(string comClassName)
        {
            string re = "CLSID_" + Regex.Replace(comClassName, @"[^a-zA-Z0-9]", "_");
            re = Regex.Replace(re, "__+", "_");
            return re;
        }
        internal static string FormatGuidAsCStruct(string comClassName, Guid guidToFormat)
        {
            string id = FormatComClassNameAsCIdentifier(comClassName);
            string re = GuidToString(guidToFormat, GuidFormat.Structure);
            return re.Replace("guidObject", id);
        }

        internal static string FormatGuidDefault(this Guid guid)
        {
            return guid.ToString().ToUpper();
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SERVICE_STATUS_PROCESS
        {
            public int dwServiceType;
            public int dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
            public int dwProcessId;
            public int dwServiceFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ENUM_SERVICE_STATUS_PROCESS
        {
            public IntPtr lpServiceName;
            public IntPtr lpDisplayName;
            public SERVICE_STATUS_PROCESS ServiceStatusProcess;

            public string GetName()
            {
                if (lpServiceName != IntPtr.Zero)
                {
                    return Marshal.PtrToStringUni(lpServiceName);
                }
                return String.Empty;
            }
        }

        const int SC_MANAGER_CONNECT = 0x0001;
        const int SC_MANAGER_ENUMERATE_SERVICE = 0x0004;
        const int SERVICE_WIN32 = 0x00000030;
        const int SERVICE_ACTIVE = 0x00000001;
        const int ERROR_MORE_DATA = 234;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr OpenSCManager(
              string lpMachineName,
              string lpDatabaseName,
              int dwDesiredAccess
            );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CloseServiceHandle(
                  IntPtr hSCObject
                );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool EnumServicesStatusEx(
              IntPtr hSCManager,
              int InfoLevel,
              int dwServiceType,
              int dwServiceState,
              SafeBuffer lpServices,
              int cbBufSize,
              out int pcbBytesNeeded,
              out int lpServicesReturned,
              IntPtr lpResumeHandle,
              string pszGroupName
            );

        internal static Dictionary<int, HashSet<string>> GetServicePids()
        {
            Dictionary<int, HashSet<string>> ret = new Dictionary<int, HashSet<string>>();
            IntPtr hSC = OpenSCManager(null, null, SC_MANAGER_CONNECT | SC_MANAGER_ENUMERATE_SERVICE);
            try
            {
                int bytes_needed = 0;
                int service_count = 0;
                EnumServicesStatusEx(hSC, 0, SERVICE_WIN32, SERVICE_ACTIVE, SafeHGlobalBuffer.Null, 0, out bytes_needed, out service_count, IntPtr.Zero, null);
                if (Marshal.GetLastWin32Error() != ERROR_MORE_DATA || bytes_needed <= 0)
                {
                    return ret;
                }

                int repeat_count = 5;
                while (repeat_count > 0)
                {
                    using (SafeHGlobalBuffer buf = new SafeHGlobalBuffer(bytes_needed))
                    {
                        if (EnumServicesStatusEx(hSC, 0, SERVICE_WIN32, SERVICE_ACTIVE, buf, buf.Length, out bytes_needed, out service_count, IntPtr.Zero, null))
                        {
                            ENUM_SERVICE_STATUS_PROCESS[] services = new ENUM_SERVICE_STATUS_PROCESS[service_count];
                            buf.ReadArray(0, services, 0, service_count);
                            foreach (var service in services)
                            {
                                string name = service.GetName();
                                if (!String.IsNullOrWhiteSpace(name))
                                {
                                    if (!ret.ContainsKey(service.ServiceStatusProcess.dwProcessId))
                                    {
                                        ret[service.ServiceStatusProcess.dwProcessId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                    }

                                    ret[service.ServiceStatusProcess.dwProcessId].Add(name);
                                }
                            }
                        }
                        --repeat_count;
                    }
                }
            }
            finally
            {
                if (hSC != IntPtr.Zero)
                {
                    CloseServiceHandle(hSC);
                }
            }

            return ret;
        }

        internal static bool IsAdministrator()
        {
            using (WindowsIdentity id = WindowsIdentity.GetCurrent())
            {
                return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        internal static string GetCOMDllName()
        {
            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "combase.dll")))
            {
                return "combase";
            }
            else
            {
                return "ole32";
            }
        }

        internal static int GetProcessIdFromIPid(Guid ipid)
        {
            return BitConverter.ToUInt16(ipid.ToByteArray(), 4);
        }

        internal static int GetApartmentIdFromIPid(Guid ipid)
        {
            return BitConverter.ToInt16(ipid.ToByteArray(), 6);
        }

        internal static string GetApartmentIdStringFromIPid(Guid ipid)
        {
            int appid = GetApartmentIdFromIPid(ipid);
            switch (appid)
            {
                case 0:
                    return "NTA";
                case -1:
                    return "MTA";
                default:
                    return String.Format("STA (Thread ID {0})", appid);
            }
        }

        public static ServerInformation GetServerInformation(object obj)
        {
            IntPtr intf = Marshal.GetIUnknownForObject(obj);
            IntPtr proxy = IntPtr.Zero;
            try
            {
                Guid iid = COMInterfaceEntry.IID_IDispatch;
                if (Marshal.QueryInterface(intf, ref iid, out proxy) != 0)
                {
                    ServerInformation info = new ServerInformation();
                    int hr = CoDecodeProxy(Process.GetCurrentProcess().Id, proxy.ToInt64(), out info);
                    if (hr == 0)
                    {
                        return info;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                Marshal.Release(intf);
                if (proxy != IntPtr.Zero)
                {
                    Marshal.Release(proxy);
                }
            }
            return new ServerInformation();
        }

        static Dictionary<string, Assembly> _cached_reflection_assemblies = new Dictionary<string, Assembly>();

        private static Assembly ResolveAssembly(string base_path, string name)
        {
            if (_cached_reflection_assemblies.ContainsKey(name))
            {
                return _cached_reflection_assemblies[name];
            }

            Assembly asm = null;
            if (name.Contains(","))
            {
                asm = Assembly.ReflectionOnlyLoad(name);
            }
            else
            {
                string full_path = Path.Combine(base_path, string.Format("{0}.winmd", name));
                if (File.Exists(full_path))
                {
                    asm = Assembly.ReflectionOnlyLoadFrom(full_path);
                }
                else
                {
                    int last_index = name.LastIndexOf('.');
                    if (last_index < 0)
                    {
                        return null;
                    }
                    asm = ResolveAssembly(base_path, name.Substring(0, last_index));
                }
            }

            _cached_reflection_assemblies[name] = asm;
            return _cached_reflection_assemblies[name];
        }

        private static void WindowsRuntimeMetadata_ReflectionOnlyNamespaceResolve(string base_path, NamespaceResolveEventArgs e)
        {
            e.ResolvedAssemblies.Add(ResolveAssembly(base_path, e.NamespaceName));
        }

        private static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(string base_path, ResolveEventArgs args)
        {
            return ResolveAssembly(base_path, args.Name);
        }

        private static Dictionary<Guid, Type> m_runtime_interface_metadata;

        public static IReadOnlyDictionary<Guid, Type> RuntimeInterfaceMetadata
        {
            get
            {
                if (m_runtime_interface_metadata == null)
                {
                    LoadRuntimeMetadata();
                }
                return m_runtime_interface_metadata;
            }
        }

        private static Dictionary<string, Type> m_runtime_class_metadata;

        public static IReadOnlyDictionary<string, Type> RuntimeClassMetadata
        {
            get
            {
                if (m_runtime_class_metadata == null)
                {
                    LoadRuntimeMetadata();
                }
                return m_runtime_class_metadata;
            }
        }

        private static void LoadRuntimeMetadata()
        {
            string base_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WinMetaData");
            m_runtime_interface_metadata = new Dictionary<Guid, Type>();
            m_runtime_class_metadata = new Dictionary<string, Type>();
            if (!Directory.Exists(base_path))
            {
                return;
            }

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (s, a) => CurrentDomain_ReflectionOnlyAssemblyResolve(base_path, a);
            WindowsRuntimeMetadata.ReflectionOnlyNamespaceResolve += (s, a) => WindowsRuntimeMetadata_ReflectionOnlyNamespaceResolve(base_path, a);
            DirectoryInfo dir = new DirectoryInfo(base_path);
            foreach (FileInfo file in dir.GetFiles("*.winmd"))
            {
                try
                {
                    Assembly asm = Assembly.ReflectionOnlyLoadFrom(file.FullName);
                    Type type = asm.GetTypes().FirstOrDefault();
                    if (type == null)
                    {
                        continue;
                    }
                    // Convert to a non-reflection only assembly.
                    Assembly new_asm = Type.GetType(type.AssemblyQualifiedName)?.Assembly;
                    Type[] types = (new_asm ?? asm).GetTypes();
                    foreach (Type t in types)
                    {
                        if (t.IsInterface)
                        {
                            foreach (var attr in t.GetCustomAttributesData())
                            {
                                if (attr.AttributeType.FullName == "Windows.Foundation.Metadata.GuidAttribute")
                                {
                                    m_runtime_interface_metadata[t.GUID] = t;
                                }
                            }
                        }
                        else if (t.IsClass && t.IsPublic)
                        {
                            m_runtime_class_metadata[t.FullName] = t;
                        }
                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
        }

        private static Lazy<string> _assembly_version = new Lazy<string>(() =>
        {
            Assembly asm = Assembly.GetCallingAssembly();
            return asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        });

        public static string GetVersion()
        {
            return _assembly_version.Value;
        }

        public static string FormatBitness(bool is64bit)
        {
            return is64bit ? "64 bit" : "32 bit";
        }

        public static string GetFileMD5(string file)
        {
            using (var stm = File.OpenRead(file))
            {
                return BitConverter.ToString(MD5Cng.Create().ComputeHash(stm)).Replace("-", string.Empty);
            }
        }

        public static T GetGuidEntry<T>(this IDictionary<Guid, T> dict, Guid guid)
        {
            if (dict.ContainsKey(guid))
            {
                return dict[guid];
            }
            return default(T);
        }

        public static IntPtr CreateInstance(Guid clsid, Guid iid, CLSCTX context, string server)
        {
            IntPtr pInterface = IntPtr.Zero;
            int hr = 0;
            if (!string.IsNullOrWhiteSpace(server))
            {
                MULTI_QI[] qis = new MULTI_QI[1];
                qis[0] = new MULTI_QI(iid);
                COSERVERINFO server_info = new COSERVERINFO(server);
                try
                {
                    hr = COMUtilities.CoCreateInstanceEx(ref clsid, IntPtr.Zero, CLSCTX.REMOTE_SERVER, server_info, 1, qis);
                    if (hr == 0)
                    {
                        hr = qis[0].HResult();
                        if (hr == 0)
                        {
                            pInterface = qis[0].GetObjectPointer();
                        }
                    }
                }
                finally
                {
                    ((IDisposable)qis[0]).Dispose();
                }
            }
            else
            {
                hr = COMUtilities.CoCreateInstance(ref clsid, IntPtr.Zero, context, ref iid, out pInterface);
            }

            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return pInterface;
        }

        public static object CreateInstanceAsObject(Guid clsid, Guid iid, CLSCTX context, string server)
        {
            IntPtr pObject = CreateInstance(clsid, iid, context, server);
            object ret = null;

            if (pObject != IntPtr.Zero)
            {
                ret = Marshal.GetObjectForIUnknown(pObject);
                Marshal.Release(pObject);
            }

            return ret;
        }

        public static object CreateInstanceFromFactory(IClassFactoryWrapper factory, Guid iid)
        {
            factory.CreateInstance(null, ref iid, out object ret);
            return ret;
        }

        public static object CreateClassFactory(Guid clsid, Guid iid, CLSCTX context, string server)
        {
            IntPtr obj;

            COSERVERINFO server_info = !string.IsNullOrWhiteSpace(server) ? new COSERVERINFO(server) : null;

            int hr = CoGetClassObject(ref clsid, server_info != null ? CLSCTX.REMOTE_SERVER
                : context, server_info, ref iid, out obj);
            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            object ret = Marshal.GetObjectForIUnknown(obj);
            Marshal.Release(obj);
            return ret;
        }

        private static Guid CLSID_NewMoniker = new Guid("ecabafc6-7f19-11d2-978e-0000f8757e2a");

        private static IMoniker ParseMoniker(IBindCtx bind_context, string moniker_string)
        {
            if (moniker_string == "new")
            {
                Guid IID_IUnknown = COMInterfaceEntry.IID_IUnknown;
                IntPtr unk;
                int hr = CoCreateInstance(ref CLSID_NewMoniker, IntPtr.Zero, 
                    CLSCTX.INPROC_SERVER, ref IID_IUnknown, out unk);
                if (hr != 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                try
                {
                    return (IMoniker)Marshal.GetObjectForIUnknown(unk);
                }
                finally
                {
                    Marshal.Release(unk);
                }
            }
            else
            {
                if (moniker_string.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
                    moniker_string.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                    moniker_string.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
                {
                    IMoniker moniker;
                    int hr = CreateURLMonikerEx(null, moniker_string, out moniker, CreateUrlMonikerFlags.Uniform);
                    if (hr != 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }
                    return moniker;
                }

                int eaten = 0;
                return MkParseDisplayName(bind_context, moniker_string, out eaten);
            }
        }

        private static IMoniker ParseMoniker(IBindCtx bind_context, string moniker_string, bool composite)
        {
            if (composite)
            {
                IMoniker ret_moniker = null;
                foreach (string m in moniker_string.Split('!'))
                {
                    IMoniker moniker = ParseMoniker(bind_context, m);
                    if (ret_moniker != null)
                    {
                        ret_moniker.ComposeWith(moniker, false, out moniker);
                    }
                    ret_moniker = moniker;
                }
                return ret_moniker;
            }
            else
            {
                return ParseMoniker(bind_context, moniker_string);
            }
        }

        public static IMoniker ParseMoniker(string moniker_string, bool composite)
        {
            IBindCtx bind_context = CreateBindCtx(0);
            return ParseMoniker(bind_context, moniker_string, composite);
        }

        public static object ParseAndBindMoniker(string moniker_string, bool composite)
        {
            IBindCtx bind_context = CreateBindCtx(0);
            IMoniker moniker = ParseMoniker(bind_context, moniker_string, composite);
            Guid iid = COMInterfaceEntry.IID_IUnknown;
            moniker.BindToObject(bind_context, null, ref iid, out object comObj);
            return comObj;
        }

        public static void GenerateSymbolFile(string symbol_dir, string dbghelp_path, string symbol_path)
        {
            COMProcessParserConfig config = new COMProcessParserConfig(dbghelp_path, symbol_path, true, true, true, true, false);
            var proc = COMProcessParser.ParseProcess(Process.GetCurrentProcess().Id, config, COMRegistry.Load(COMRegistryMode.UserOnly));
            Dictionary<string, int> entries;
            if (Environment.Is64BitProcess)
            {
                entries = SymbolResolverWrapper.GetResolvedNative();
            }
            else
            {
                entries = SymbolResolverWrapper.GetResolved32Bit();
            }

            string dll_name = GetCOMDllName();
            var symbols = entries.Where(p => p.Key.StartsWith(dll_name));

            if (!symbols.Any())
            {
                throw new ArgumentException("Couldn't parse the process information. Incorrect symbols?");
            }

            var module = SafeLoadLibraryHandle.GetModuleHandle(dll_name);
            string output_file = Path.Combine(symbol_dir, $"{GetFileMD5(module.FullPath)}.sym");
            List<string> lines = new List<string>();
            lines.Add($"# {Environment.OSVersion.VersionString} {module.FullPath} {FileVersionInfo.GetVersionInfo(module.FullPath).FileVersion}");
            lines.AddRange(symbols.Select(p => $"{p.Value} {p.Key}"));
            File.WriteAllLines(output_file, lines);
        }

        public static string FormatProxy(COMRegistry registry, IEnumerable<NdrComplexTypeReference> complex_types, 
            IEnumerable<NdrComProxyDefinition> proxies, ProxyFormatterFlags flags)
        {
            bool remove_comments = (flags & ProxyFormatterFlags.RemoveComments) != 0;
            INdrFormatter formatter = DefaultNdrFormatter.Create(registry.InterfacesToNames,
                    DemangleWinRTName,
                    remove_comments ? DefaultNdrFormatterFlags.RemoveComments : DefaultNdrFormatterFlags.None);
            StringBuilder builder = new StringBuilder();

            if ((flags & ProxyFormatterFlags.RemoveComplexTypes) == 0)
            {
                foreach (var type in complex_types)
                {
                    builder.AppendLine(formatter.FormatComplexType(type));
                }
                builder.AppendLine();
            }

            foreach (var proxy in proxies)
            {
                builder.AppendLine(formatter.FormatComProxy(proxy));
            }

            return builder.ToString();
        }

        static void EmitMember(StringBuilder builder, MemberInfo mi)
        {
            string name = MemberInfoToString(mi);
            if (!string.IsNullOrWhiteSpace(name))
            {
                builder.Append("   ");
                if (mi is FieldInfo)
                {
                    builder.AppendFormat("{0};", name).AppendLine();
                }
                else
                {
                    builder.AppendLine(name);
                }
            }
        }

        static Dictionary<MethodInfo, string> MapMethodNamesToCOM(IEnumerable<MethodInfo> mis)
        {
            HashSet<string> matched_names = new HashSet<string>();
            Dictionary<MethodInfo, string> ret = new Dictionary<MethodInfo, string>();
            foreach (MethodInfo mi in mis.Reverse())
            {
                int count = 2;
                string name = mi.Name;
                while (!matched_names.Add(name))
                {
                    name = String.Format("{0}_{1}", mi.Name, count++);
                }
                ret.Add(mi, name);
            }
            return ret;
        }

        static Dictionary<string, object> GetEnumValues(Type enum_type)
        {
            return enum_type.GetFields(BindingFlags.Public | BindingFlags.Static).ToDictionary(e => e.Name, e => e.GetRawConstantValue());
        }

        public static void FormatComType(StringBuilder builder, Type t)
        {
            try
            {
                if (t.IsInterface)
                {
                    builder.AppendFormat("[Guid(\"{0}\")]", t.GUID).AppendLine();
                    builder.AppendFormat("interface {0}", t.Name).AppendLine();
                }
                else if (t.IsEnum)
                {
                    builder.AppendFormat("enum {0}", t.Name).AppendLine();
                }
                else if (t.IsClass)
                {
                    builder.AppendFormat("[Guid(\"{0}\")]", t.GUID).AppendLine();
                    ClassInterfaceAttribute class_attr = t.GetCustomAttribute<ClassInterfaceAttribute>();
                    if (class_attr != null)
                    {
                        builder.AppendFormat("[ClassInterface(ClassInterfaceType.{0})]", class_attr.Value).AppendLine();
                    }
                    builder.AppendFormat("class {0}", t.Name).AppendLine();
                }
                else
                {
                    builder.AppendFormat("struct {0}", t.Name).AppendLine();
                }
                builder.AppendLine("{");

                if (t.IsInterface || t.IsClass)
                {
                    MethodInfo[] methods = t.GetMethods().Where(m => !m.IsStatic && (m.Attributes & MethodAttributes.SpecialName) == 0).ToArray();
                    if (methods.Length > 0)
                    {
                        builder.AppendLine("   /* Methods */");

                        Dictionary<MethodInfo, string> name_mapping = new Dictionary<MethodInfo, string>();
                        if (t.IsClass)
                        {
                            name_mapping = MapMethodNamesToCOM(methods);
                        }

                        foreach (MethodInfo mi in methods)
                        {
                            if (name_mapping.ContainsKey(mi) && name_mapping[mi] != mi.Name)
                            {
                                builder.AppendFormat("    /* Exposed as {0} */", name_mapping[mi]).AppendLine();
                            }

                            EmitMember(builder, mi);
                        }
                    }

                    var props = t.GetProperties().Where(p => !(p.GetMethod?.IsStatic ?? false));
                    if (props.Any())
                    {
                        builder.AppendLine("   /* Properties */");
                        foreach (PropertyInfo pi in props)
                        {
                            EmitMember(builder, pi);
                        }
                    }

                    var evs = t.GetEvents();
                    if (evs.Length > 0)
                    {
                        builder.AppendLine("   /* Events */");
                        foreach (EventInfo ei in evs)
                        {
                            EmitMember(builder, ei);
                        }
                    }
                }
                else if (t.IsEnum)
                {
                    foreach (var pair in GetEnumValues(t))
                    {
                        builder.Append("   ");
                        try
                        {
                            builder.AppendFormat("{0} = {1},", pair.Key, pair.Value);
                        }
                        catch
                        {
                            builder.AppendFormat("{0},");
                        }
                        builder.AppendLine();
                    }
                }
                else
                {
                    FieldInfo[] fields = t.GetFields();
                    if (fields.Length > 0)
                    {
                        builder.AppendLine("   /* Fields */");
                        foreach (FieldInfo fi in fields)
                        {
                            EmitMember(builder, fi);
                        }
                    }
                }

                builder.AppendLine("}");
            }
            catch (InvalidOperationException)
            {
            }
        }

        public static string FormatComType(Type t)
        {
            StringBuilder builder = new StringBuilder();
            FormatComType(builder, t);
            return builder.ToString();
        }
        
        public static IEnumerable<Type> GetComClasses(Assembly typelib, bool com_visible)
        {
            return GetComTypes(typelib.GetTypes().Where(t => t.IsClass), com_visible);
        }

        public static IEnumerable<Type> GetComInterfaces(Assembly typelib, bool com_visible)
        {
            return GetComTypes(typelib.GetTypes().Where(t => t.IsInterface), com_visible);
        }

        private static IEnumerable<Type> GetComTypes(IEnumerable<Type> types, bool com_visible)
        {
            if (com_visible)
            {
                return types.Where(t => Marshal.IsTypeVisibleFromCom(t));
            }
            else
            {
                return types.Where(t => Attribute.IsDefined(t, typeof(ComImportAttribute)));
            }
        }

        public static IEnumerable<Type> GetComStructs(Assembly typelib, bool com_visible)
        {
            var types = typelib.GetTypes().Where(t => t.IsValueType && !t.IsEnum);
            if (com_visible)
            {
                types = types.Where(t => Marshal.IsTypeVisibleFromCom(t));
            }
            return types;
        }

        public static IEnumerable<Type> GetComEnums(Assembly typelib, bool com_visible)
        {
            var types = typelib.GetTypes().Where(t => t.IsEnum);
            if (com_visible)
            {
                types = types.Where(t => Marshal.IsTypeVisibleFromCom(t));
            }
            return types;
        }

        private static void FormatComTypes(StringBuilder builder, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                FormatComType(builder, type);
                builder.AppendLine();
            }
            builder.AppendLine();
        }

        public static string FormatComAssembly(Assembly assembly, bool interfaces_only)
        {
            StringBuilder builder = new StringBuilder();
            if (!interfaces_only)
            {
                FormatComTypes(builder, GetComStructs(assembly, false));
                FormatComTypes(builder, GetComEnums(assembly, false));
                FormatComTypes(builder, GetComClasses(assembly, false));
            }
            FormatComTypes(builder, GetComInterfaces(assembly, false));
            return builder.ToString();
        }

        public static void CopyTextToClipboard(string text)
        {
            int tries = 10;
            while (tries > 0)
            {
                try
                {
                    Clipboard.SetText(text);
                    break;
                }
                catch (ExternalException)
                {
                }
                System.Threading.Thread.Sleep(100);
                tries--;
            }
        }

        public static string GuidToString(Guid guid, GuidFormat format_type)
        {
            switch (format_type)
            {
                case GuidFormat.Object:
                    return $"<object id=\"obj\" classid=\"clsid:{guid}\">NO OBJECT</object>";
                case GuidFormat.String:
                    return guid.FormatGuid();
                case GuidFormat.Structure:
                    return $"GUID guidObject = {guid:X};";
                case GuidFormat.HexString:
                    {
                        byte[] data = guid.ToByteArray();
                        return string.Join(" ", data.Select(b => $"{b:X02}"));
                    }
                case GuidFormat.CSGuid:
                    return $"Guid guidObject = new Guid(\"{guid}\");";
                case GuidFormat.CSGuidAttribute:
                    return $"[Guid(\"{guid}\")]";
                case GuidFormat.RpcUuid:
                    return $"[uuid(\"{guid}\")]";
                default:
                    throw new ArgumentException("Invalid guid string type", nameof(format_type));
            }
        }

        public static void CopyGuidToClipboard(Guid guid, GuidFormat guid_format)
        {
            CopyTextToClipboard(GuidToString(guid, guid_format));
        }

        public static bool IsProxy(object obj)
        {
            return obj is IRpcOptions;
        }

        public static RPCOPT_SERVER_LOCALITY_VALUES GetServerLocality(object obj)
        {
            if (obj is IRpcOptions opts)
            {
                opts.Query(obj, RPCOPT_PROPERTIES.SERVER_LOCALITY, out IntPtr value);
                return (RPCOPT_SERVER_LOCALITY_VALUES)value.ToInt32();
            }
            return RPCOPT_SERVER_LOCALITY_VALUES.PROCESS_LOCAL;
        }

        public static StorageWrapper CreateStorage(string name, STGM mode, STGFMT format)
        {
            Guid iid = typeof(IStorage).GUID;
            return new StorageWrapper(StgCreateStorageEx(name, mode, format, 0, null, IntPtr.Zero, ref iid));
        }

        public static StorageWrapper CreateReadOnlyStorage(string name)
        {
            return CreateStorage(name, STGM.SHARE_EXCLUSIVE | STGM.READ, STGFMT.Storage);
        }

        public static StorageWrapper OpenStorage(string name, STGM mode, STGFMT format)
        {
            Guid iid = typeof(IStorage).GUID;
            return new StorageWrapper(StgOpenStorageEx(name, mode, format, 0, null, IntPtr.Zero, ref iid));
        }

        public static StorageWrapper OpenReadOnlyStorage(string name)
        {
            return OpenStorage(name, STGM.SHARE_EXCLUSIVE | STGM.READ, STGFMT.Storage);
        }

        public static string GetProcessNameById(int pid)
        {
            try
            {
                return Process.GetProcessById(pid).ProcessName;
            }
            catch
            {
                return string.Empty;
            }
        }


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate uint InspectHStringCallback2(IntPtr context, long readAddress, int length, IntPtr buffer);

        [DllImport("combase.dll")]
        static extern int WindowsInspectString2(long targetHString, int machine, InspectHStringCallback2 callback, 
            IntPtr context, out int length, out long targetStringAddress);

        public static string ReadHStringFull(this NtProcess process, long address)
        {
            InspectHStringCallback2 callback = (c, r, l, ba) =>
            {
                try
                {
                    byte[] data = process.ReadMemory(r, l, true);
                    Marshal.Copy(data, 0, ba, l);
                    return 0;
                }
                catch (NtException)
                {
                    return 0x8000FFFF;
                }
            };

            int machine = process.Is64Bit ? 0x8664 : 0x14C;

            if (WindowsInspectString2(address, machine, callback, IntPtr.Zero, out int length, out long target_addr) == 0)
            {
                return Encoding.Unicode.GetString(process.ReadMemory(target_addr, length * 2));
            }

            return string.Empty;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate uint InspectHStringCallback(IntPtr context, IntPtr readAddress, int length, IntPtr buffer);

        [DllImport("combase.dll")]
        static extern int WindowsInspectString(IntPtr targetHString, int machine, InspectHStringCallback callback,
            IntPtr context, out int length, out IntPtr targetStringAddress);

        public static string ReadHString(this NtProcess process, IntPtr address)
        {
            InspectHStringCallback callback = (c, r, l, ba) =>
            {
                try
                {
                    byte[] data = process.ReadMemory(r.ToInt64(), l, true);
                    Marshal.Copy(data, 0, ba, l);
                    return 0;
                }
                catch (NtException)
                {
                    return 0x8000FFFF;
                }
            };

            int machine = process.Is64Bit ? 0x8664 : 0x14C;

            if (WindowsInspectString(address, machine, callback, IntPtr.Zero, out int length, out IntPtr target_addr) == 0)
            {
                return Encoding.Unicode.GetString(process.ReadMemory(target_addr.ToInt64(), length * 2));
            }

            return string.Empty;
        }

        public static string ReadZString(this NtProcess process, long address)
        {
            if (address == 0)
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            char c = process.ReadMemory<char>(address);
            while (c != 0)
            {
                builder.Append(c);
                address += 2;
                c = process.ReadMemory<char>(address);
            }
            return builder.ToString();
        }

        public static string GetFileName(string path)
        {
            int index = path.LastIndexOf('\\');
            if (index < 0)
            {
                index = path.LastIndexOf('/');
            }
            if (index < 0)
            {
                return path;
            }
            return path.Substring(index + 1);
        }

        private static string GetNativeLibraryDirectory()
        {
            if (Environment.Is64BitProcess)
            {
                return Path.Combine(GetAppDirectory(), "x64");
            }

            return Path.Combine(GetAppDirectory(), "x86");
        }

        public static string GetDefaultDbgHelp()
        {
            string path = Path.Combine(GetNativeLibraryDirectory(), "dbghelp.dll");
            if (File.Exists(path))
            {
                return path;
            }

            path = Environment.GetEnvironmentVariable($"_DBGHELP_PATH_{(Environment.Is64BitProcess ? "X64" : "X86")}");
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                return path;
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "dbghelp.dll");
        }

        internal static bool EqualsDictionary<K, V>(IReadOnlyDictionary<K, V> left, IReadOnlyDictionary<K, V> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            foreach (var pair in left)
            {
                if (!right.ContainsKey(pair.Key))
                {
                    return false;
                }

                if (!right[pair.Key].Equals(pair.Value))
                {
                    return false;
                }
            }

            return true;
        }

        internal static int GetHashCodeDictionary<K, V>(IReadOnlyDictionary<K, V> dict)
        {
            int hash_code = 0;
            foreach (var pair in dict)
            {
                hash_code ^= pair.Key.GetHashCode() ^ pair.Value.GetHashCode();
            }
            return hash_code;
        }

        internal static readonly bool IsWindows81OrLess = Environment.OSVersion.Version < new Version(6, 4);
        internal static readonly bool IsWindows10RS2OrLess = Environment.OSVersion.Version < new Version(10, 0, 16299);
        internal static readonly bool IsWindows10RS3OrLess = Environment.OSVersion.Version < new Version(10, 0, 17134);
        internal static readonly bool IsWindows10RS4OrLess = Environment.OSVersion.Version < new Version(10, 0, 17763);
        internal static readonly bool IsWindows101909OrLess = Environment.OSVersion.Version < new Version(10, 0, 19041);

        public static string GetPackagePath(string packageId)
        {
            int length = 0;
            int result = PackageIdFromFullName(packageId, 0, ref length, SafeHGlobalBuffer.Null);
            if (result != 122)
            {
                return string.Empty;
            }

            using (var buffer = new SafeHGlobalBuffer(length))
            {
                result = PackageIdFromFullName(packageId,
                0, ref length, buffer);
                if (result != 0)
                {
                    return string.Empty;
                }

                StringBuilder builder = new StringBuilder(260);
                length = builder.Capacity;
                result = GetPackagePath(buffer, 0, ref length, builder);
                if (result != 0)
                {
                    return string.Empty;
                }

                return builder.ToString();
            }
        }
    }
}
