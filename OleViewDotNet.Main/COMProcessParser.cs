//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017
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
using NtApiDotNet.Ndr;
using NtApiDotNet.Win32;
using OleViewDotNet.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace OleViewDotNet
{
    public class COMProcessParserConfig
    {
        public string DbgHelpPath { get; }
        public string SymbolPath { get; }
        public bool ParseStubMethods { get; }
        public bool ResolveMethodNames { get; }
        public bool ParseRegisteredClasses { get; }
        public bool ParseClients { get; }
        public bool ParseActivationContext { get; }

        public COMProcessParserConfig(string dbghelp_path, string symbol_path,
            bool parse_stubs_methods, bool resolve_method_names,
            bool parse_registered_classes, bool parse_clients,
            bool parse_activation_context)
        {
            DbgHelpPath = dbghelp_path;
            SymbolPath = symbol_path;
            ParseStubMethods = parse_stubs_methods;
            ResolveMethodNames = resolve_method_names;
            ParseRegisteredClasses = parse_registered_classes;
            ParseClients = parse_clients;
            ParseActivationContext = parse_activation_context;
        }
    }

    public class COMProcessToken
    {
        public string User { get; }
        public string UserSid { get; }
        public TokenIntegrityLevel IntegrityLevel { get; }
        public bool Sandboxed { get; }
        public bool AppContainer { get; }
        public bool Restricted { get; }
        public bool Elevated { get; }
        public bool LowPrivilegeAppContainer { get; }
        public AppxPackageName PackageName { get; }
        public ulong PackageHostId { get; }
        public IEnumerable<AppModelPolicy_PolicyValue> AppModelPolicies { get; }

        public COMProcessToken()
        {
            User = "UNKNOWN";
            UserSid = string.Empty;
            IntegrityLevel = TokenIntegrityLevel.Medium;
        }

        public COMProcessToken(NtProcess process) : this()
        {
            PackageName = AppxPackageName.FromProcess(process);

            using (var result = NtToken.OpenProcessToken(process, TokenAccessRights.Query, false))
            {
                if (result.IsSuccess)
                {
                    NtToken token = result.Result;
                    User = token.User.Sid.Name;
                    UserSid = token.User.Sid.ToString();
                    IntegrityLevel = token.IntegrityLevel;
                    Sandboxed = token.IsSandbox;
                    AppContainer = token.AppContainer;
                    Restricted = token.Restricted;
                    Elevated = token.Elevated;
                    LowPrivilegeAppContainer = token.LowPrivilegeAppContainer;
                    var pkghostid = token.GetSecurityAttributeByName("WIN://PKGHOSTID");
                    if (pkghostid != null && pkghostid.ValueType == ClaimSecurityValueType.UInt64 && pkghostid.Values.Any())
                    {
                        PackageHostId = (ulong)pkghostid.Values.First();
                    }
                    AppModelPolicies = token.AppModelPolicies.ToList().AsReadOnly();
                }
            }
        }

        public override string ToString()
        {
            return $"{User}";
        }
    }

    internal class SymbolResolverWrapper : ISymbolResolver
    {
        private readonly ISymbolResolver _resolver;
        private readonly SymbolLoadedModule _base_module;
        private readonly Dictionary<string, int> _resolved;

        static readonly Dictionary<string, int> _resolved_32bit = new Dictionary<string, int>();
        static readonly Dictionary<string, int> _resolved_64bit = new Dictionary<string, int>();
        static readonly string _dllname = COMUtilities.GetCOMDllName();
        static readonly string _dllprefix = $"{_dllname}!";

        public SymbolResolverWrapper(bool is64bit, ISymbolResolver resolver)
        {
            _resolver = resolver;
            foreach (var module in _resolver.GetLoadedModules())
            {
                if (module.Name.Equals(_dllname, StringComparison.OrdinalIgnoreCase))
                {
                    _base_module = module;
                    break;
                }
            }

            _resolved = is64bit ? _resolved_64bit : _resolved_32bit;
        }

        internal static Dictionary<string, int> GetResolved32Bit()
        {
            return _resolved_32bit;
        }

        internal static Dictionary<string, int> GetResolvedNative()
        {
            if (Environment.Is64BitProcess)
            {
                return _resolved_64bit;
            }
            else
            {
                return _resolved_32bit;
            }
        }

        public IEnumerable<SymbolLoadedModule> GetLoadedModules()
        {
            return _resolver.GetLoadedModules();
        }

        public IEnumerable<SymbolLoadedModule> GetLoadedModules(bool refresh)
        {
            return _resolver.GetLoadedModules(refresh);
        }

        public SymbolLoadedModule GetModuleForAddress(IntPtr address)
        {
            return _resolver.GetModuleForAddress(address);
        }

        public SymbolLoadedModule GetModuleForAddress(IntPtr address, bool refresh)
        {
            return _resolver.GetModuleForAddress(address, refresh);
        }

        public string GetModuleRelativeAddress(IntPtr address)
        {
            return _resolver.GetModuleRelativeAddress(address);
        }

        public string GetModuleRelativeAddress(IntPtr address, bool refresh)
        {
            return _resolver.GetModuleRelativeAddress(address, refresh);
        }

        public IntPtr GetAddressOfSymbol(string symbol)
        {
            if (_resolved.ContainsKey(symbol) && _base_module != null && symbol.StartsWith(_dllprefix))
            {
                return _base_module.BaseAddress + _resolved[symbol];
            }

            IntPtr ret = _resolver.GetAddressOfSymbol(symbol);
            if (ret != IntPtr.Zero && symbol.StartsWith(_dllprefix))
            {
                _resolved[symbol] = (int)(ret.ToInt64() - _base_module.BaseAddress.ToInt64());
            }

            return ret;
        }

        public string GetSymbolForAddress(IntPtr address)
        {
            return _resolver.GetSymbolForAddress(address);
        }

        public string GetSymbolForAddress(IntPtr address, bool generate_fake_symbol)
        {
            return _resolver.GetSymbolForAddress(address, generate_fake_symbol);
        }

        public string GetSymbolForAddress(IntPtr address, bool generate_fake_symbol, bool return_name_only)
        {
            return _resolver.GetSymbolForAddress(address, generate_fake_symbol, return_name_only);
        }

        public void Dispose()
        {
            _resolver.Dispose();
        }

        public void ReloadModuleList()
        {
            _resolver.ReloadModuleList();
        }

        public void LoadModule(string module_path, IntPtr base_address)
        {
            _resolver.LoadModule(module_path, base_address);
        }
    }

    public static class COMProcessParser
    {
        private static T ReadStruct<T>(this NtProcess process, long address) where T : new()
        {
            if (address <= 0)
            {
                return new T();
            }
            try
            {
                return process.ReadMemory<T>(address);
            }
            catch (NtException)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error reading address {0:X}", address));
                return new T();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PageEntry
        {
            public IntPtr pNext;
            public int dwFlag;
        };

        interface IPageAllocator
        {
            int Pages { get; }
            int EntrySize { get; }
            int EntriesPerPage { get; }
            IntPtr[] ReadPages(NtProcess handle);

        }

        [StructLayout(LayoutKind.Sequential)]
        struct CInternalPageAllocator : IPageAllocator
        {
            public int _cPages;
            public IntPtr _pPageListStart;
            public IntPtr _pPageListEnd;
            public int _dwFlags;
            public PageEntry _ListHead;
            public IntPtr _cEntries;
            public IntPtr _cbPerEntry;
            public ushort _cEntriesPerPage;
            public IntPtr _pLock;

            int IPageAllocator.Pages
            {
                get
                {
                    return _cPages;
                }
            }

            int IPageAllocator.EntrySize
            {
                get
                {
                    return _cbPerEntry.ToInt32();
                }
            }

            int IPageAllocator.EntriesPerPage
            {
                get
                {
                    return _cEntriesPerPage;
                }
            }

            IntPtr[] IPageAllocator.ReadPages(NtProcess process)
            {
                return process.ReadMemoryArray<IntPtr>(_pPageListStart.ToInt64(), _cPages);
            }
        };

        [StructLayout(LayoutKind.Sequential)]
        struct CPageAllocator
        {
            public CInternalPageAllocator _pgalloc;
            public IntPtr _hHeap;
            public int _cbPerEntry;
            public int _lNumEntries;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PageEntry32
        {
            public int pNext;
            public int dwFlag;
        };

        [StructLayout(LayoutKind.Sequential)]
        struct CInternalPageAllocator32 : IPageAllocator
        {
            public int _cPages;
            public int _pPageListStart;
            public int _pPageListEnd;
            public int _dwFlags;
            public PageEntry32 _ListHead;
            public int _cEntries;
            public int _cbPerEntry;
            public ushort _cEntriesPerPage;
            public int _pLock;

            int IPageAllocator.Pages
            {
                get
                {
                    return _cPages;
                }
            }

            int IPageAllocator.EntrySize
            {
                get
                {
                    return _cbPerEntry;
                }
            }

            int IPageAllocator.EntriesPerPage
            {
                get
                {
                    return _cEntriesPerPage;
                }
            }
            IntPtr[] IPageAllocator.ReadPages(NtProcess process)
            {
                return process.ReadMemoryArray<int>(_pPageListStart, _cPages).Select(i => new IntPtr(i)).ToArray();
            }
        };

        internal interface IPIDEntryNativeInterface
        {
            uint Flags { get; }
            IntPtr Interface { get; }
            IntPtr Stub { get; }
            Guid Ipid { get; }
            Guid Iid { get; }
            int StrongRefs { get; }
            int WeakRefs { get; }
            int PrivateRefs { get; }
            IOXIDEntry GetOxidEntry(NtProcess process);
            IPIDEntryNativeInterface GetNext(NtProcess process);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct IPIDEntryNative : IPIDEntryNativeInterface
        {
            public IntPtr pNextIPID;
            public uint dwFlags;
            public int cStrongRefs;
            public int cWeakRefs;
            public int cPrivateRefs;
            public IntPtr pv;
            public IntPtr pStub;
            public IntPtr pOXIDEntry;
            public Guid ipid;
            public Guid iid;
            public IntPtr pChnl;
            public IntPtr pIRCEntry;
            public IntPtr pOIDFLink;
            public IntPtr pOIDBLink;

            uint IPIDEntryNativeInterface.Flags
            {
                get
                {
                    return dwFlags;
                }
            }

            IntPtr IPIDEntryNativeInterface.Interface
            {
                get
                {
                    return pv;
                }
            }

            IntPtr IPIDEntryNativeInterface.Stub
            {
                get
                {
                    return pStub;
                }
            }

            Guid IPIDEntryNativeInterface.Ipid
            {
                get
                {
                    return ipid;
                }
            }

            Guid IPIDEntryNativeInterface.Iid
            {
                get
                {
                    return iid;
                }
            }

            int IPIDEntryNativeInterface.StrongRefs
            {
                get
                {
                    return cStrongRefs;
                }
            }

            int IPIDEntryNativeInterface.WeakRefs
            {
                get
                {
                    return cWeakRefs;
                }
            }

            int IPIDEntryNativeInterface.PrivateRefs
            {
                get
                {
                    return cPrivateRefs;
                }
            }

            IPIDEntryNativeInterface IPIDEntryNativeInterface.GetNext(NtProcess process)
            {
                if (pNextIPID == IntPtr.Zero)
                    return null;
                return process.ReadStruct<IPIDEntryNative>(pNextIPID.ToInt64());
            }

            IOXIDEntry IPIDEntryNativeInterface.GetOxidEntry(NtProcess process)
            {
                if (COMUtilities.IsWindows101909OrLess)
                    return process.ReadStruct<OXIDEntryNative>(pOXIDEntry.ToInt64());
                return process.ReadStruct<OXIDEntryNative2004>(pOXIDEntry.ToInt64());
            }
        };

        [StructLayout(LayoutKind.Sequential)]
        struct IPIDEntryNative32 : IPIDEntryNativeInterface
        {
            public int pNextIPID;
            public uint dwFlags;
            public int cStrongRefs;
            public int cWeakRefs;
            public int cPrivateRefs;
            public int pv;
            public int pStub;
            public int pOXIDEntry;
            public Guid ipid;
            public Guid iid;
            public int pChnl;
            public int pIRCEntry;
            public int pOIDFLink;
            public int pOIDBLink;

            uint IPIDEntryNativeInterface.Flags
            {
                get
                {
                    return dwFlags;
                }
            }

            IntPtr IPIDEntryNativeInterface.Interface
            {
                get
                {
                    return new IntPtr(pv);
                }
            }

            IntPtr IPIDEntryNativeInterface.Stub
            {
                get
                {
                    return new IntPtr(pStub);
                }
            }

            Guid IPIDEntryNativeInterface.Ipid
            {
                get
                {
                    return ipid;
                }
            }

            Guid IPIDEntryNativeInterface.Iid
            {
                get
                {
                    return iid;
                }
            }

            int IPIDEntryNativeInterface.StrongRefs
            {
                get
                {
                    return cStrongRefs;
                }
            }

            int IPIDEntryNativeInterface.WeakRefs
            {
                get
                {
                    return cWeakRefs;
                }
            }

            int IPIDEntryNativeInterface.PrivateRefs
            {
                get
                {
                    return cPrivateRefs;
                }
            }

            IPIDEntryNativeInterface IPIDEntryNativeInterface.GetNext(NtProcess process)
            {
                if (pNextIPID == 0)
                    return null;
                return process.ReadStruct<IPIDEntryNative32>(pNextIPID);
            }

            IOXIDEntry IPIDEntryNativeInterface.GetOxidEntry(NtProcess process)
            {
                if (COMUtilities.IsWindows101909OrLess)
                    return process.ReadStruct<OXIDEntryNative32>(pOXIDEntry);
                return process.ReadStruct<OXIDEntryNative2004_32>(pOXIDEntry);
            }
        };

        [StructLayout(LayoutKind.Sequential)]
        struct COMVERSION
        {
            public ushort MajorVersion;
            public ushort MinorVersion;
        }

        internal interface IOXIDEntry
        {
            int Pid { get; }
            int Tid { get; }
            Guid MOxid { get; }
            long Mid { get; }
            IntPtr ServerSTAHwnd { get; }
            COMDualStringArray GetBinding(NtProcess process);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct OXIDEntryNative : IOXIDEntry
        {
            public IntPtr _pNext;
            public IntPtr _pPrev;
            public int _dwPid;
            public int _dwTid;
            public Guid _moxid;
            public long _mid;
            public Guid _ipidRundown;
            public int _dwFlags;
            public IntPtr _hServerSTA;
            public IntPtr _pParentApt;
            public IntPtr _pSharedDefaultHandle;
            public IntPtr _pAuthId;
            public IntPtr _pBinding;
            public int _dwAuthnHint;
            public int _dwAuthnSvc;
            public IntPtr _pMIDEntry;
            public IntPtr _pRUSTA;
            public int _cRefs;
            public IntPtr _hComplete;
            public int _cCalls;
            public int _cResolverRef;
            public int _dwExpiredTime;
            COMVERSION _version;
            public IntPtr _pAppContainerServerSecurityDescriptor;
            public int _ulMarshaledTargetInfoLength;
            public IntPtr _pMarshaledTargetInfo;
            public IntPtr _pszServerPackageFullName;
            public Guid _guidProcessIdentifier;

            int IOXIDEntry.Pid
            {
                get
                {
                    return _dwPid;
                }
            }

            int IOXIDEntry.Tid
            {
                get
                {
                    return _dwTid;
                }
            }

            Guid IOXIDEntry.MOxid
            {
                get
                {
                    return _moxid;
                }
            }

            long IOXIDEntry.Mid
            {
                get
                {
                    return _mid;
                }
            }

            IntPtr IOXIDEntry.ServerSTAHwnd
            {
                get
                {
                    return _hServerSTA;
                }
            }

            COMDualStringArray IOXIDEntry.GetBinding(NtProcess process)
            {
                if (_pBinding == IntPtr.Zero)
                    return new COMDualStringArray();
                try
                {
                    return new COMDualStringArray(_pBinding, process, true);
                }
                catch (NtException)
                {
                    return new COMDualStringArray();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct OXIDEntryNative32 : IOXIDEntry
        {
            public int _pNext;
            public int _pPrev;
            public int _dwPid;
            public int _dwTid;
            public Guid _moxid;
            public long _mid;
            public Guid _ipidRundown;
            public int _dwFlags;
            public int _hServerSTA;
            public int _pParentApt;
            public int _pSharedDefaultHandle;
            public int _pAuthId;
            public int _pBinding;
            public int _dwAuthnHint;
            public int _dwAuthnSvc;
            public int _pMIDEntry;
            public int _pRUSTA;
            public int _cRefs;
            public int _hComplete;
            public int _cCalls;
            public int _cResolverRef;
            public int _dwExpiredTime;
            COMVERSION _version;
            public int _pAppContainerServerSecurityDescriptor;
            public int _ulMarshaledTargetInfoLength;
            public int _pMarshaledTargetInfo;
            public int _pszServerPackageFullName;
            public Guid _guidProcessIdentifier;

            int IOXIDEntry.Pid
            {
                get
                {
                    return _dwPid;
                }
            }

            int IOXIDEntry.Tid
            {
                get
                {
                    return _dwTid;
                }
            }

            Guid IOXIDEntry.MOxid
            {
                get
                {
                    return _moxid;
                }
            }

            long IOXIDEntry.Mid
            {
                get
                {
                    return _mid;
                }
            }

            IntPtr IOXIDEntry.ServerSTAHwnd
            {
                get
                {
                    return new IntPtr(_hServerSTA);
                }
            }

            COMDualStringArray IOXIDEntry.GetBinding(NtProcess process)
            {
                if (_pBinding == 0)
                    return new COMDualStringArray();
                try
                {
                    return new COMDualStringArray(new IntPtr(_pBinding), process, true);
                }
                catch (NtException)
                {
                    return new COMDualStringArray();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CONTAINERVERSION
        {
            public int version;
            public long capabilityFlags;
            public IntPtr extensions;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct OXIDInfoNative2004
        {
            public int _dwTid;
            public int _dwPid;
            public int _dwAuthnHint;
            public COMVERSION _dcomVersion;
            public CONTAINERVERSION _containerVersion;
            public Guid _ipidRemUnknown;
            public int _dwFlags;
            public IntPtr _psa;
            public Guid _guidProcessIdentifier;
            public long _processHostId;
            public int _clientDependencyBehavior;
            public IntPtr _packageFullName;
            public IntPtr _userSid;
            public IntPtr _appcontainerSid;
            public ulong _primaryOxid;
            public Guid _primaryIpidRemUnknown;
        }

        [StructLayout(LayoutKind.Sequential)]
        class OXIDEntryNative2004 : IOXIDEntry
        {
            public IntPtr _flink;
            public IntPtr _blink;
            public int m_isInList;
            public OXIDInfoNative2004 info;
            public long _mid;
            public Guid _ipidRundown;
            public Guid _moxid;
            public byte _registered;
            public byte _stopped;
            public byte _pendingRelease;
            public byte _remotingInitialized;
            public IntPtr _hServerSTA;

            int IOXIDEntry.Pid
            {
                get
                {
                    return info._dwPid;
                }
            }

            int IOXIDEntry.Tid
            {
                get
                {
                    return info._dwTid;
                }
            }

            Guid IOXIDEntry.MOxid
            {
                get
                {
                    return _moxid;
                }
            }

            long IOXIDEntry.Mid
            {
                get
                {
                    return _mid;
                }
            }

            IntPtr IOXIDEntry.ServerSTAHwnd
            {
                get
                {
                    return _hServerSTA;
                }
            }

            COMDualStringArray IOXIDEntry.GetBinding(NtProcess process)
            {
                if (info._psa == IntPtr.Zero)
                    return new COMDualStringArray();
                try
                {
                    return new COMDualStringArray(info._psa, process, true);
                }
                catch (NtException)
                {
                    return new COMDualStringArray();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct OXIDInfoNative2004_32
        {
            public int _dwTid;
            public int _dwPid;
            public int _dwAuthnHint;
            public COMVERSION _dcomVersion;
            public CONTAINERVERSION _containerVersion;
            public Guid _ipidRemUnknown;
            public int _dwFlags;
            public int _psa;
            public Guid _guidProcessIdentifier;
            public long _processHostId;
            public int _clientDependencyBehavior;
            public int _packageFullName;
            public int _userSid;
            public int _appcontainerSid;
            public ulong _primaryOxid;
            public Guid _primaryIpidRemUnknown;
        }

        [StructLayout(LayoutKind.Sequential)]
        class OXIDEntryNative2004_32 : IOXIDEntry
        {
            public int _flink;
            public int _blink;
            public int m_isInList;
            public OXIDInfoNative2004_32 info;
            public long _mid;
            public Guid _ipidRundown;
            public Guid _moxid;
            public byte _registered;
            public byte _stopped;
            public byte _pendingRelease;
            public byte _remotingInitialized;
            public int _hServerSTA;

            int IOXIDEntry.Pid
            {
                get
                {
                    return info._dwPid;
                }
            }

            int IOXIDEntry.Tid
            {
                get
                {
                    return info._dwTid;
                }
            }

            Guid IOXIDEntry.MOxid
            {
                get
                {
                    return _moxid;
                }
            }

            long IOXIDEntry.Mid
            {
                get
                {
                    return _mid;
                }
            }

            IntPtr IOXIDEntry.ServerSTAHwnd
            {
                get
                {
                    return new IntPtr(_hServerSTA);
                }
            }

            COMDualStringArray IOXIDEntry.GetBinding(NtProcess process)
            {
                if (info._psa == 0)
                    return new COMDualStringArray();
                try
                {
                    return new COMDualStringArray(new IntPtr(info._psa), process, true);
                }
                catch (NtException)
                {
                    return new COMDualStringArray();
                }
            }
        }

        interface IStdIdentity
        {
            int GetIPIDCount();
            IPIDEntryNativeInterface GetFirstIpid(NtProcess process);
            SMFLAGS GetFlags();
        }

        [Flags]
        enum STDID_FLAGS
        {
            STDID_SERVER = 0x0,
            STDID_CLIENT = 0x1,
            STDID_FREETHREADED = 0x2,
            STDID_HAVEID = 0x4,
            STDID_IGNOREID = 0x8,
            STDID_AGGREGATED = 0x10,
            STDID_INDESTRUCTOR = 0x100,
            STDID_LOCKEDINMEM = 0x200,
            STDID_DEFHANDLER = 0x400,
            STDID_AGGID = 0x800,
            STDID_STCMRSHL = 0x1000,
            STDID_REMOVEDFROMIDOBJ = 0x2000,
            STDID_SYSTEM = 0x4000,
            STDID_FTM = 0x8000,
            STDID_NOIEC = 0x10000,
            STDID_FASTRUNDOWN = 0x20000,
            STDID_AGILE_OOP_PROXY = 0x40000,
            STDID_SUPPRESS_WAKE_SET = 0x80000,
            STDID_IMPLEMENTS_IAGILEOBJECT = 0x100000,
            STDID_ALLOW_ASTA_TO_ASTA_DEADLOCK_RISK = 0x200000,
            STDID_DO_NOT_DISTURB_SET = 0x400000,
            STDID_DISABLE_ASYNC_REMOTING_FOR_WINRT_ASYNC = 0x800000,
            STDID_FAKE_OID_FOR_INTERNAL_PROXY = 0x1000000,
            STDID_RUNDOWN_OBJECT_OF_INTEREST = 0x2000000,
            STDID_ALL = 0x3FFFF1F,
        };

        [Flags]
        enum SMFLAGS
        {
            SMFLAGS_CLIENT_SIDE = 0x1,
            SMFLAGS_PENDINGDISCONNECT = 0x2,
            SMFLAGS_REGISTEREDOID = 0x4,
            SMFLAGS_DISCONNECTED = 0x8,
            SMFLAGS_FIRSTMARSHAL = 0x10,
            SMFLAGS_HANDLER = 0x20,
            SMFLAGS_WEAKCLIENT = 0x40,
            SMFLAGS_IGNORERUNDOWN = 0x80,
            SMFLAGS_CLIENTMARSHALED = 0x100,
            SMFLAGS_NOPING = 0x200,
            SMFLAGS_TRIEDTOCONNECT = 0x400,
            SMFLAGS_CSTATICMARSHAL = 0x800,
            SMFLAGS_USEAGGSTDMARSHAL = 0x1000,
            SMFLAGS_SYSTEM = 0x2000,
            SMFLAGS_DEACTIVATED = 0x4000,
            SMFLAGS_FTM = 0x8000,
            SMFLAGS_CLIENTPOLICYSET = 0x10000,
            SMFLAGS_APPDISCONNECT = 0x20000,
            SMFLAGS_SYSDISCONNECT = 0x40000,
            SMFLAGS_RUNDOWNDISCONNECT = 0x80000,
            SMFLAGS_CLEANEDUP = 0x100000,
            SMFLAGS_LIGHTNA = 0x200000,
            SMFLAGS_FASTRUNDOWN = 0x400000,
            SMFLAGS_IMPLEMENTS_IAGILEOBJECT = 0x800000,
            SMFLAGS_ALLOW_ASTA_TO_ASTA_DEADLOCK_RISK = 0x1000000,
            SMFLAGS_SAFE_TO_QI_DURING_DISCONNECT = 0x2000000,
            SMFLAGS_CHECKSUSPEND = 0x4000000,
            SMFLAGS_DISABLE_ASYNC_REMOTING_FOR_WINRT_ASYNC = 0x8000000,
            SMFLAGS_PROXY_TO_INPROC_OBJECT = 0x10000000,
            SMFLAGS_MADE_WINRT_ASYNC_CALL = 0x20000000,
            SMFLAGS_RUNDOWN_OBJECT_OF_INTEREST = 0x40000000,
            SMFLAGS_ALL = 0x7FFFFFFF,
        };

        [StructLayout(LayoutKind.Sequential)]
        struct CStdIdentity : IStdIdentity
        {
            public IntPtr VTablePtr;
            public IntPtr VTablePtr2;
            public SMFLAGS _dwFlags;
            public int _cIPIDs;
            public IntPtr _pFirstIPID; // tagIPIDEntry* 

            IPIDEntryNativeInterface IStdIdentity.GetFirstIpid(NtProcess process)
            {
                if (_pFirstIPID == IntPtr.Zero)
                    return null;
                return process.ReadStruct<IPIDEntryNative>(_pFirstIPID.ToInt64());
            }

            int IStdIdentity.GetIPIDCount()
            {
                return _cIPIDs;
            }

            SMFLAGS IStdIdentity.GetFlags()
            {
                return _dwFlags;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CStdIdentity32 : IStdIdentity
        {
            public int VTablePtr;
            public int VTablePtr2;
            public SMFLAGS _dwFlags;
            public int _cIPIDs;
            public int _pFirstIPID; // tagIPIDEntry* 

            IPIDEntryNativeInterface IStdIdentity.GetFirstIpid(NtProcess process)
            {
                if (_pFirstIPID == 0)
                    return null;
                return process.ReadStruct<IPIDEntryNative32>(_pFirstIPID);
            }

            int IStdIdentity.GetIPIDCount()
            {
                return _cIPIDs;
            }

            SMFLAGS IStdIdentity.GetFlags()
            {
                return _dwFlags;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CStdIdentity32RS4 : IStdIdentity
        {
            public int VTablePtr;
            public int DummyValue; // This seems to be here to ensure the rest of the struct is 8 byte aligned.
            public int VTablePtr2;
            public SMFLAGS _dwFlags;
            public int _cIPIDs;
            public int _pFirstIPID; // tagIPIDEntry* 

            IPIDEntryNativeInterface IStdIdentity.GetFirstIpid(NtProcess process)
            {
                if (_pFirstIPID == 0)
                    return null;
                return process.ReadStruct<IPIDEntryNative32>(_pFirstIPID);
            }

            int IStdIdentity.GetIPIDCount()
            {
                return _cIPIDs;
            }

            SMFLAGS IStdIdentity.GetFlags()
            {
                return _dwFlags;
            }
        }

        interface IIFaceEntry
        {
            IntPtr GetProxy();
            IIFaceEntry GetNext(NtProcess process);
            Guid GetIid();
        }

        [StructLayout(LayoutKind.Sequential)]
        struct IFaceEntry : IIFaceEntry
        {
            public IntPtr _pNext; // IFaceEntry*
            public IntPtr _pProxy; // void* 
            public IntPtr _pRpcProxy; // IRpcProxyBuffer* 
            public IntPtr _pRpcStub; // IRpcStubBuffer* 
            public IntPtr _pServer; // void* 
            public Guid _iid;
            public IntPtr _pCtxChnl; // CCtxChnl* 
            public IntPtr _pHead; // CtxEntry* 
            public IntPtr _pFreeList; // CtxEntry* 
            public IntPtr _pInterceptor; // ICallInterceptor*
            public IntPtr _pUnkInner; // IUnknown* 

            Guid IIFaceEntry.GetIid()
            {
                return _iid;
            }

            IIFaceEntry IIFaceEntry.GetNext(NtProcess process)
            {
                if (_pNext == IntPtr.Zero)
                    return null;
                return process.ReadStruct<IFaceEntry>(_pNext.ToInt64());
            }

            IntPtr IIFaceEntry.GetProxy()
            {
                return _pProxy;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct IFaceEntry32 : IIFaceEntry
        {
            public int _pNext; // IFaceEntry*
            public int _pProxy; // void* 
            public int _pRpcProxy; // IRpcProxyBuffer* 
            public int _pRpcStub; // IRpcStubBuffer* 
            public int _pServer; // void* 
            public Guid _iid;
            public int _pCtxChnl; // CCtxChnl* 
            public int _pHead; // CtxEntry* 
            public int _pFreeList; // CtxEntry* 
            public int _pInterceptor; // ICallInterceptor*
            public int _pUnkInner; // IUnknown* 

            Guid IIFaceEntry.GetIid()
            {
                return _iid;
            }

            IIFaceEntry IIFaceEntry.GetNext(NtProcess process)
            {
                if (_pNext == 0)
                    return null;
                return process.ReadStruct<IFaceEntry32>(_pNext);
            }

            IntPtr IIFaceEntry.GetProxy()
            {
                return new IntPtr(_pProxy);
            }
        }

        interface IStdWrapper
        {
            int GetIFaceCount();
            IntPtr GetVtableAddress();
            IIFaceEntry GetIFaceHead(NtProcess process);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CStdWrapper : IStdWrapper
        {
            public IntPtr VTablePtr;
            public uint _dwState;
            public int _cRefs;
            public int _cCalls;
            public int _cIFaces;
            public IntPtr _pIFaceHead; // IFaceEntry* 
            public IntPtr _pCtxEntryHead; // CtxEntry* 
            public IntPtr _pCtxFreeList; // CtxEntry* 
            public IntPtr _pServer; // IUnknown* 
            public IntPtr _pID; // CIDObject* 
            public IntPtr _pVtableAddress; // void* 

            int IStdWrapper.GetIFaceCount()
            {
                return _cIFaces;
            }

            IIFaceEntry IStdWrapper.GetIFaceHead(NtProcess process)
            {
                if (_pIFaceHead == IntPtr.Zero)
                    return null;
                return process.ReadStruct<IFaceEntry>(_pIFaceHead.ToInt64());
            }

            IntPtr IStdWrapper.GetVtableAddress()
            {
                return _pVtableAddress;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CStdWrapper32 : IStdWrapper
        {
            public int VTablePtr;
            public uint _dwState;
            public int _cRefs;
            public int _cCalls;
            public int _cIFaces;
            public int _pIFaceHead; // IFaceEntry* 
            public int _pCtxEntryHead; // CtxEntry* 
            public int _pCtxFreeList; // CtxEntry* 
            public int _pServer; // IUnknown* 
            public int _pID; // CIDObject* 
            public int _pVtableAddress; // void* 

            int IStdWrapper.GetIFaceCount()
            {
                return _cIFaces;
            }

            IIFaceEntry IStdWrapper.GetIFaceHead(NtProcess process)
            {
                if (_pIFaceHead == 0)
                    return null;
                return process.ReadStruct<IFaceEntry32>(_pIFaceHead);
            }

            IntPtr IStdWrapper.GetVtableAddress()
            {
                return new IntPtr(_pVtableAddress);
            }
        }

        interface IIDObject
        {
            Guid GetOid();
            IStdWrapper GetStdWrapper(NtProcess process);
            IIDObject GetNextOid(NtProcess process, IntPtr head_ptr);
            IStdIdentity GetStdIdentity(NtProcess process);
        }

        interface ISHashChain
        {
            IntPtr GetNext();
            IntPtr GetPrev();
            ISHashChain GetNextChain(NtProcess process);
            I GetNextObject<T, I>(NtProcess process, int offset) where I : class where T : I, new();
        }

        class SHashChainEntry
        {
            public IntPtr BaseAddress { get; }
            public ISHashChain StartEntry { get; }
            public SHashChainEntry(IntPtr base_address, ISHashChain start_entry)
            {
                BaseAddress = base_address;
                StartEntry = start_entry;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SHashChain : ISHashChain
        {
            public IntPtr pNext;
            public IntPtr pPrev;

            IntPtr ISHashChain.GetNext()
            {
                return pNext;
            }

            ISHashChain ISHashChain.GetNextChain(NtProcess process)
            {
                if (pNext == IntPtr.Zero)
                    return null;
                return process.ReadStruct<SHashChain>(pNext.ToInt64());
            }

            I ISHashChain.GetNextObject<T, I>(NtProcess process, int offset)
            {
                if (pNext == IntPtr.Zero)
                    return null;
                return process.ReadStruct<T>(pNext.ToInt64() - offset);
            }

            IntPtr ISHashChain.GetPrev()
            {
                return pPrev;
            }
        }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        sealed class ChainOffsetAttribute : Attribute
        {
            public static int GetOffset(Type t)
            {
                foreach (var field in t.GetFields())
                {
                    if (field.GetCustomAttributes(typeof(ChainOffsetAttribute), false).Length > 0)
                    {
                        return Marshal.OffsetOf(t, field.Name).ToInt32();
                    }
                }
                throw new ArgumentException("Invalid type, missing ChainOffset attribute");
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CIDObject : IIDObject
        {
            public IntPtr VTablePtr;
            public SHashChain _pidChain;
            [ChainOffset]
            public SHashChain _oidChain;
            public int _dwState;
            public int _cRefs;
            public IntPtr _pServer;
            public IntPtr _pServerCtx; // CObjectContext* 
            public Guid _oid;
            public int _aptID;
            public IntPtr _pStdWrapper; // CStdWrapper* 
            public IntPtr _pStdID; // CStdIdentity* 
            public int _cCalls;
            public int _cLocks;
            public SHashChain _oidUnpinReqChain;
            public int _dwOidUnpinReqState;
            public IntPtr _pvObjectTrackCookie; // void* 

            IIDObject IIDObject.GetNextOid(NtProcess process, IntPtr head_ptr)
            {
                if (_oidChain.pNext == head_ptr)
                    return null;
                return process.ReadStruct<CIDObject>(_oidChain.pNext.ToInt64());
            }

            Guid IIDObject.GetOid()
            {
                return _oid;
            }

            IStdIdentity IIDObject.GetStdIdentity(NtProcess process)
            {
                if (_pStdID == IntPtr.Zero)
                {
                    return null;
                }
                return process.ReadStruct<CStdIdentity>(_pStdID.ToInt64());
            }

            IStdWrapper IIDObject.GetStdWrapper(NtProcess process)
            {
                if (_pStdWrapper == IntPtr.Zero)
                    return null;
                return process.ReadStruct<CStdWrapper>(_pStdWrapper.ToInt64());
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SHashChain32 : ISHashChain
        {
            public int pNext;
            public int pPrev;

            IntPtr ISHashChain.GetNext()
            {
                return new IntPtr(pNext);
            }

            IntPtr ISHashChain.GetPrev()
            {
                return new IntPtr(pPrev);
            }

            ISHashChain ISHashChain.GetNextChain(NtProcess process)
            {
                if (pNext == 0)
                    return null;
                return process.ReadStruct<SHashChain32>(pNext);
            }

            I ISHashChain.GetNextObject<T, I>(NtProcess process, int offset)
            {
                if (pNext == 0)
                    return null;
                return process.ReadStruct<T>(pNext - offset);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CIDObject32 : IIDObject
        {
            public int VTablePtr;
            public SHashChain32 _pidChain;
            [ChainOffset]
            public SHashChain32 _oidChain;
            public int _dwState;
            public int _cRefs;
            public int _pServer;
            public int _pServerCtx; // CObjectContext* 
            public Guid _oid;
            public int _aptID;
            public int _pStdWrapper; // CStdWrapper* 
            public int _pStdID; // CStdIdentity* 
            public int _cCalls;
            public int _cLocks;
            public SHashChain32 _oidUnpinReqChain;
            public int _dwOidUnpinReqState;
            public int _pvObjectTrackCookie; // void* 

            IIDObject IIDObject.GetNextOid(NtProcess process, IntPtr head_ptr)
            {
                if (_oidChain.pNext == head_ptr.ToInt32())
                    return null;
                return process.ReadStruct<CIDObject>(_oidChain.pNext);
            }

            Guid IIDObject.GetOid()
            {
                return _oid;
            }

            IStdWrapper IIDObject.GetStdWrapper(NtProcess process)
            {
                if (_pStdWrapper == 0)
                    return null;
                return process.ReadStruct<CStdWrapper32>(_pStdWrapper);
            }

            IStdIdentity IIDObject.GetStdIdentity(NtProcess process)
            {
                if (_pStdID == 0)
                {
                    return null;
                }

                if (COMUtilities.IsWindows10RS3OrLess)
                {
                    return process.ReadStruct<CStdIdentity32>(_pStdID);
                }
                else
                {
                    return process.ReadStruct<CStdIdentity32RS4>(_pStdID);
                }
            }
        }

        private class PageAllocator
        {
            public IntPtr[] Pages { get; private set; }
            public int EntrySize { get; private set; }
            public int EntriesPerPage { get; private set; }

            void Init<T>(NtProcess process, IntPtr ipid_table) where T : IPageAllocator, new()
            {
                IPageAllocator page_alloc = process.ReadStruct<T>(ipid_table.ToInt64());
                Pages = page_alloc.ReadPages(process);
                EntrySize = page_alloc.EntrySize;
                EntriesPerPage = page_alloc.EntriesPerPage;
            }

            public PageAllocator(NtProcess process, IntPtr ipid_table)
            {
                if (process.Is64Bit)
                {
                    Init<CInternalPageAllocator>(process, ipid_table);
                }
                else
                {
                    Init<CInternalPageAllocator32>(process, ipid_table);
                }
            }
        }

        interface ICClassEntry
        {
            Guid[] GetGuids();
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CClassEntry : ICClassEntry
        {
            public IntPtr vfptr; // CClassCache::CCollectableVtbl* 
            public IntPtr _pNextCollectee; // CClassCache::CCollectable* 
            public ulong _qwTickLastTouched;

            // SMultiGUIDHashNode _hashNode;
            public IntPtr pNext; // SHashChain* 
            public IntPtr pPrev; // SHashChain* 
            public int cGUID;
            public IntPtr aGUID; // _GUID* 
            // END SMultiGUIDHashNode _hashNode;
            public Guid guids1;
            public Guid guids2;
            public uint _dwSig;
            public uint _dwFlags;
            public IntPtr _pTreatAsList; // CClassCache::CClassEntry* 
            public IntPtr _pBCEListFront; // CClassCache::CBaseClassEntry* 
            public IntPtr _pBCEListBack; // CClassCache::CBaseClassEntry* 
            public uint _cLocks;
            public uint _dwFailedContexts;
            public IntPtr _pCI; // IComClassInfo* 

            Guid[] ICClassEntry.GetGuids()
            {
                Guid[] ret = new Guid[2];
                ret[0] = guids1;
                ret[1] = guids2;
                return ret;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CClassEntry32 : ICClassEntry
        {
            public int vfptr; // CClassCache::CCollectableVtbl* 
            public int _pNextCollectee; // CClassCache::CCollectable* 
            public ulong _qwTickLastTouched;

            // SMultiGUIDHashNode _hashNode;
            public int pNext; // SHashChain* 
            public int pPrev; // SHashChain* 
            public int cGUID;
            public int aGUID; // _GUID* 
            // END SMultiGUIDHashNode _hashNode;
            public Guid guids1;
            public Guid guids2;
            public uint _dwSig;
            public uint _dwFlags;
            public int _pTreatAsList; // CClassCache::CClassEntry* 
            public int _pBCEListFront; // CClassCache::CBaseClassEntry* 
            public int _pBCEListBack; // CClassCache::CBaseClassEntry* 
            public uint _cLocks;
            public uint _dwFailedContexts;
            public int _pCI; // IComClassInfo* 

            Guid[] ICClassEntry.GetGuids()
            {
                Guid[] ret = new Guid[2];
                ret[0] = guids1;
                ret[1] = guids2;
                return ret;
            }
        }

        interface ICLSvrClassEntry
        {
            IntPtr GetNext();
            IntPtr GetIUnknown();
            ICClassEntry GetClassEntry(NtProcess process);
            REGCLS GetRegFlags();
            uint GetCookie();
            CLSCTX GetContext();
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CLSvrClassEntry : ICLSvrClassEntry
        {
            public IntPtr vfptr; // CClassCache::CBaseClassEntryVtbl*
            public IntPtr _pNext; // CClassCache::CBaseClassEntry*
            public IntPtr _pPrev; // CClassCache::CBaseClassEntry* 
            public IntPtr _pClassEntry; // CClassCache::CClassEntry* 
            public CLSCTX _dwContext;
            public int _dwSig;
            public IntPtr _pNextLSvr; // CClassCache::CLSvrClassEntry* 
            public IntPtr _pPrevLSvr; // CClassCache::CLSvrClassEntry*
            public IntPtr _pUnk; // IUnknown* 
            public REGCLS _dwRegFlags;
            public uint _dwFlags;
            public uint _dwScmReg;
            public uint _hApt;
            public IntPtr _hWndDdeServer;
            public IntPtr _pObjServer; // CObjServer*
            public uint _dwCookie;
            public uint _cUsing;
            public uint _ulServiceId;

            ICClassEntry ICLSvrClassEntry.GetClassEntry(NtProcess process)
            {
                if (_pClassEntry == IntPtr.Zero)
                {
                    return null;
                }
                return process.ReadStruct<CClassEntry>(_pClassEntry.ToInt64());
            }

            CLSCTX ICLSvrClassEntry.GetContext()
            {
                return _dwContext;
            }

            uint ICLSvrClassEntry.GetCookie()
            {
                return _dwCookie;
            }

            IntPtr ICLSvrClassEntry.GetIUnknown()
            {
                return _pUnk;
            }

            IntPtr ICLSvrClassEntry.GetNext()
            {
                return _pNextLSvr;
            }

            REGCLS ICLSvrClassEntry.GetRegFlags()
            {
                return _dwRegFlags;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CLSvrClassEntry32 : ICLSvrClassEntry
        {
            public int vfptr; // CClassCache::CBaseClassEntryVtbl*
            public int _pNext; // CClassCache::CBaseClassEntry*
            public int _pPrev; // CClassCache::CBaseClassEntry* 
            public int _pClassEntry; // CClassCache::CClassEntry* 
            public CLSCTX _dwContext;
            public int _dwSig;
            public int _pNextLSvr; // CClassCache::CLSvrClassEntry* 
            public int _pPrevLSvr; // CClassCache::CLSvrClassEntry*
            public int _pUnk; // IUnknown* 
            public REGCLS _dwRegFlags;
            public uint _dwFlags;
            public uint _dwScmReg;
            public uint _hApt;
            public int _hWndDdeServer;
            public int _pObjServer; // CObjServer*
            public uint _dwCookie;
            public uint _cUsing;
            public uint _ulServiceId;

            ICClassEntry ICLSvrClassEntry.GetClassEntry(NtProcess process)
            {
                if (_pClassEntry == 0)
                {
                    return null;
                }
                return process.ReadStruct<CClassEntry32>(_pClassEntry);
            }

            IntPtr ICLSvrClassEntry.GetIUnknown()
            {
                return new IntPtr(_pUnk);
            }

            IntPtr ICLSvrClassEntry.GetNext()
            {
                return new IntPtr(_pNextLSvr);
            }
            uint ICLSvrClassEntry.GetCookie()
            {
                return _dwCookie;
            }

            REGCLS ICLSvrClassEntry.GetRegFlags()
            {
                return _dwRegFlags;
            }

            CLSCTX ICLSvrClassEntry.GetContext()
            {
                return _dwContext;
            }
        }

        internal interface IWinRTLocalSvrClassEntry
        {
            Guid GetClsid();
            string GetActivatableClassId(NtProcess process);
            string GetPackageFullName(NtProcess process);
            IntPtr GetActivationFactoryCallback();
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SActivatableClassIdHashNode
        {
            public SHashChain chain;
            public IntPtr activatableClassId;
        };

        [StructLayout(LayoutKind.Sequential)]
        struct CWinRTLocalSvrClassEntry : IWinRTLocalSvrClassEntry
        {
            [ChainOffset]
            public SActivatableClassIdHashNode _hashNode;
            public IntPtr _pRegChain; // CClassCache::CWinRTLocalSvrClassEntry* 
            public IntPtr _pActivationFactoryCallback;
            public int _dwFlags;
            public int _dwScmReg;
            public int _hApt;
            public Guid _clsid;
            public int _dwSig;
            public int _cLocks;
            public IntPtr _pObjServer; // CObjServer* 
            public IntPtr _cookie;
            public bool _suspended;
            public int _ulServiceId;
            public IntPtr _activatableClassId;
            public IntPtr _packageFullName; // Microsoft::WRL::Wrappers::HString 

            string IWinRTLocalSvrClassEntry.GetActivatableClassId(NtProcess process)
            {
                if (_activatableClassId == IntPtr.Zero)
                    return string.Empty;
                return process.ReadHString(_activatableClassId);
            }

            IntPtr IWinRTLocalSvrClassEntry.GetActivationFactoryCallback()
            {
                return _pActivationFactoryCallback;
            }

            Guid IWinRTLocalSvrClassEntry.GetClsid()
            {
                return _clsid;
            }

            string IWinRTLocalSvrClassEntry.GetPackageFullName(NtProcess process)
            {
                if (_packageFullName == IntPtr.Zero)
                    return string.Empty;
                return process.ReadHString(_packageFullName);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CWinRTLocalSvrClassEntryRS5 : IWinRTLocalSvrClassEntry
        {
            [ChainOffset]
            public SActivatableClassIdHashNode _hashNode;
            public IntPtr _pRegChain; // CClassCache::CWinRTLocalSvrClassEntry* 
            public IntPtr _pActivationFactoryCallback;
            public int _dwFlags;
            public int _dwScmReg;
            public int _hApt;
            public int _dwSig;
            public int _cLocks;
            public IntPtr _pObjServer; // CObjServer* 
            public IntPtr _cookie;
            public bool _suspended;
            public int _ulServiceId;
            public IntPtr _activatableClassId;
            public IntPtr _packageFullName; // Microsoft::WRL::Wrappers::HString 

            string IWinRTLocalSvrClassEntry.GetActivatableClassId(NtProcess process)
            {
                if (_activatableClassId == IntPtr.Zero)
                    return string.Empty;
                return process.ReadHString(_activatableClassId);
            }

            IntPtr IWinRTLocalSvrClassEntry.GetActivationFactoryCallback()
            {
                return _pActivationFactoryCallback;
            }

            Guid IWinRTLocalSvrClassEntry.GetClsid()
            {
                return Guid.Empty;
            }

            string IWinRTLocalSvrClassEntry.GetPackageFullName(NtProcess process)
            {
                if (_packageFullName == IntPtr.Zero)
                    return string.Empty;
                return process.ReadHString(_packageFullName);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SActivatableClassIdHashNode32
        {
            public SHashChain32 chain;
            public int activatableClassId;
        };

        [StructLayout(LayoutKind.Sequential)]
        struct CWinRTLocalSvrClassEntry32 : IWinRTLocalSvrClassEntry
        {
            [ChainOffset]
            public SActivatableClassIdHashNode32 _hashNode;
            public int _pRegChain; // CClassCache::CWinRTLocalSvrClassEntry* 
            public int _pActivationFactoryCallback;
            public int _dwFlags;
            public int _dwScmReg;
            public int _hApt;
            public Guid _clsid;
            public int _dwSig;
            public int _cLocks;
            public int _pObjServer; // CObjServer* 
            public int _cookie;
            public bool _suspended;
            public int _ulServiceId;
            public int _activatableClassId;
            public int _packageFullName; // Microsoft::WRL::Wrappers::HString 

            string IWinRTLocalSvrClassEntry.GetActivatableClassId(NtProcess process)
            {
                if (_activatableClassId == 0)
                    return string.Empty;
                return process.ReadHString(new IntPtr(_activatableClassId));
            }

            IntPtr IWinRTLocalSvrClassEntry.GetActivationFactoryCallback()
            {
                return new IntPtr(_pActivationFactoryCallback);
            }

            Guid IWinRTLocalSvrClassEntry.GetClsid()
            {
                return _clsid;
            }

            string IWinRTLocalSvrClassEntry.GetPackageFullName(NtProcess process)
            {
                if (_packageFullName == 0)
                    return string.Empty;
                return process.ReadHString(new IntPtr(_packageFullName));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CWinRTLocalSvrClassEntryRS532 : IWinRTLocalSvrClassEntry
        {
            [ChainOffset]
            public SActivatableClassIdHashNode32 _hashNode;
            public int _pRegChain; // CClassCache::CWinRTLocalSvrClassEntry* 
            public int _pActivationFactoryCallback;
            public int _dwFlags;
            public int _dwScmReg;
            public int _hApt;
            public int _dwSig;
            public int _cLocks;
            public int _pObjServer; // CObjServer* 
            public int _cookie;
            public bool _suspended;
            public int _ulServiceId;
            public int _activatableClassId;
            public int _packageFullName; // Microsoft::WRL::Wrappers::HString 

            string IWinRTLocalSvrClassEntry.GetActivatableClassId(NtProcess process)
            {
                if (_activatableClassId == 0)
                    return string.Empty;
                return process.ReadHString(new IntPtr(_activatableClassId));
            }

            IntPtr IWinRTLocalSvrClassEntry.GetActivationFactoryCallback()
            {
                return new IntPtr(_pActivationFactoryCallback);
            }

            Guid IWinRTLocalSvrClassEntry.GetClsid()
            {
                return Guid.Empty;
            }

            string IWinRTLocalSvrClassEntry.GetPackageFullName(NtProcess process)
            {
                if (_packageFullName == 0)
                    return string.Empty;
                return process.ReadHString(new IntPtr(_packageFullName));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CWinRTLocalSvrClassEntryWin8 : IWinRTLocalSvrClassEntry
        {
            [ChainOffset]
            public SActivatableClassIdHashNode _hashNode;
            public IntPtr _pNextLSvr; // CClassCache::CWinRTLocalSvrClassEntry*
            public IntPtr _pPrevLSvr; // CClassCache::CWinRTLocalSvrClassEntry* 
            public IntPtr _pRegChain; // CClassCache::CWinRTLocalSvrClassEntry* 
            public IntPtr _pActivationFactoryCallback;
            public uint _dwFlags;
            public int _dwScmReg;
            public int _hApt;
            public Guid _clsid;
            public uint _dwSig;
            public int _cLocks;
            public IntPtr _pObjServer; // CObjServer* 
            public IntPtr _cookie; // $F1BCC8D2ED72627AE3E1D14940DBB08E* 
            public bool _suspended;
            public int _ulServiceId;
            public IntPtr _activatableClassId; // const wchar_t*

            string IWinRTLocalSvrClassEntry.GetActivatableClassId(NtProcess process)
            {
                if (_activatableClassId == IntPtr.Zero)
                    return string.Empty;
                return process.ReadZString(_activatableClassId.ToInt64());
            }

            IntPtr IWinRTLocalSvrClassEntry.GetActivationFactoryCallback()
            {
                return _pActivationFactoryCallback;
            }

            Guid IWinRTLocalSvrClassEntry.GetClsid()
            {
                return _clsid;
            }

            string IWinRTLocalSvrClassEntry.GetPackageFullName(NtProcess process)
            {
                return string.Empty;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CWinRTLocalSvrClassEntry32Win8 : IWinRTLocalSvrClassEntry
        {
            [ChainOffset]
            public SActivatableClassIdHashNode32 _hashNode;
            public int _pNextLSvr; // CClassCache::CWinRTLocalSvrClassEntry*
            public int _pPrevLSvr; // CClassCache::CWinRTLocalSvrClassEntry* 
            public int _pRegChain; // CClassCache::CWinRTLocalSvrClassEntry* 
            public int _pActivationFactoryCallback;
            public uint _dwFlags;
            public int _dwScmReg;
            public int _hApt;
            public Guid _clsid;
            public uint _dwSig;
            public int _cLocks;
            public int _pObjServer; // CObjServer* 
            public int _cookie; // $F1BCC8D2ED72627AE3E1D14940DBB08E* 
            public bool _suspended;
            public int _ulServiceId;
            public int _activatableClassId; // const wchar_t*

            string IWinRTLocalSvrClassEntry.GetActivatableClassId(NtProcess process)
            {
                if (_activatableClassId == 0)
                    return string.Empty;
                return process.ReadZString(_activatableClassId);
            }

            IntPtr IWinRTLocalSvrClassEntry.GetActivationFactoryCallback()
            {
                return new IntPtr(_pActivationFactoryCallback);
            }

            Guid IWinRTLocalSvrClassEntry.GetClsid()
            {
                return _clsid;
            }

            string IWinRTLocalSvrClassEntry.GetPackageFullName(NtProcess process)
            {
                return string.Empty;
            }
        }

        static List<COMIPIDEntry> ParseIPIDEntries<T>(NtProcess process, IntPtr ipid_table, ISymbolResolver resolver,
            COMProcessParserConfig config, COMRegistry registry, HashSet<Guid> ipid_set)
            where T : struct, IPIDEntryNativeInterface
        {
            List<COMIPIDEntry> entries = new List<COMIPIDEntry>();
            PageAllocator palloc = new PageAllocator(process, ipid_table);
            if (palloc.Pages.Length == 0 || palloc.EntrySize < Marshal.SizeOf(typeof(T)))
            {
                return entries;
            }

            foreach (IntPtr page in palloc.Pages)
            {
                int total_size = palloc.EntriesPerPage * palloc.EntrySize;
                var data = process.ReadMemory(page.ToInt64(), palloc.EntriesPerPage * palloc.EntrySize);
                if (data.Length < total_size)
                {
                    continue;
                }

                using (var buf = new SafeHGlobalBuffer(data))
                {
                    for (int entry_index = 0; entry_index < palloc.EntriesPerPage; ++entry_index)
                    {
                        IPIDEntryNativeInterface ipid_entry = buf.Read<T>((ulong)(entry_index * palloc.EntrySize));
                        if ((ipid_entry.Flags != 0xF1EEF1EE) && (ipid_entry.Flags != 0))
                        {
                            if (ipid_set.Count == 0 || ipid_set.Contains(ipid_entry.Ipid))
                            {
                                entries.Add(new COMIPIDEntry(ipid_entry, Guid.Empty, process, resolver, config, registry));
                            }
                        }
                    }
                }
            }

            return entries;
        }

        static readonly string _dllname = COMUtilities.GetCOMDllName();

        static string GetSymbolName(string name)
        {
            return $"{_dllname}!{name}";
        }

        internal static string SymbolFromAddress(ISymbolResolver resolver, bool is64bit, IntPtr address)
        {
            return String.Format("0x{0:X}", address.ToInt64());
        }

        private static List<COMIPIDEntry> ParseIPIDEntries(NtProcess process, ISymbolResolver resolver, COMProcessParserConfig config,
            COMRegistry registry, IEnumerable<Guid> ipids)
        {
            HashSet<Guid> ipid_set = new HashSet<Guid>(ipids);
            IntPtr ipid_table = resolver.GetAddressOfSymbol(GetSymbolName("CIPIDTable::_palloc"));
            if (ipid_table == IntPtr.Zero)
            {
                return new List<COMIPIDEntry>();
            }

            if (process.Is64Bit)
            {
                return ParseIPIDEntries<IPIDEntryNative>(process, ipid_table, resolver, config, registry, ipid_set);
            }
            else
            {
                return ParseIPIDEntries<IPIDEntryNative32>(process, ipid_table, resolver, config, registry, ipid_set);
            }
        }

        private static Guid GetProcessAppId(NtProcess process, ISymbolResolver resolver)
        {
            IntPtr appid = resolver.GetAddressOfSymbol(GetSymbolName("g_AppId"));
            if (appid == IntPtr.Zero)
            {
                return Guid.Empty;
            }
            return process.ReadStruct<Guid>(appid.ToInt64());
        }

        private static string ReadSecurityDescriptorFromAddress(NtProcess process, IntPtr address)
        {
            try
            {
                return new SecurityDescriptor(process, address).ToSddl();
            }
            catch (NtException)
            {
                return string.Empty;
            }
        }

        private static string ReadSecurityDescriptor(NtProcess process, ISymbolResolver resolver, string symbol)
        {
            IntPtr sd = resolver.GetAddressOfSymbol(GetSymbolName(symbol));
            if (sd == IntPtr.Zero)
            {
                return String.Empty;
            }
            IntPtr sd_ptr;
            if (process.Is64Bit)
            {
                sd_ptr = process.ReadStruct<IntPtr>(sd.ToInt64());
            }
            else
            {
                sd_ptr = new IntPtr(process.ReadStruct<int>(sd.ToInt64()));
            }

            if (sd_ptr == IntPtr.Zero)
            {
                return "D:NO_ACCESS_CONTROL";
            }

            return ReadSecurityDescriptorFromAddress(process, sd_ptr);
        }

        private static string GetProcessAccessSecurityDescriptor(NtProcess process, ISymbolResolver resolver)
        {
            return ReadSecurityDescriptor(process, resolver, "gSecDesc");
        }

        private static string GetLrpcSecurityDescriptor(NtProcess process, ISymbolResolver resolver)
        {
            return ReadSecurityDescriptor(process, resolver, "gLrpcSecurityDescriptor");
        }

        private static SHashChainEntry[] GetBuckets(NtProcess process, ISymbolResolver resolver, string name, int count)
        {
            IntPtr buckets = resolver.GetAddressOfSymbol(GetSymbolName(name));
            if (buckets == IntPtr.Zero)
                return new SHashChainEntry[0];
            int size = 0;
            IEnumerable<ISHashChain> chain = null;

            if (process.Is64Bit)
            {
                size = Marshal.SizeOf<SHashChain>();
                chain = process.ReadMemoryArray<SHashChain>(buckets.ToInt64(), count).Cast<ISHashChain>();
            }
            else
            {
                size = Marshal.SizeOf<SHashChain32>();
                chain = process.ReadMemoryArray<SHashChain32>(buckets.ToInt64(), count).Cast<ISHashChain>();
            }

            return chain.Select((s, i) => new SHashChainEntry(buckets + (i * size), s)).ToArray();
        }

        private static List<U> ReadHashTable<T, U, I>(NtProcess process, string bucket_symbol,
            Func<I, NtProcess, ISymbolResolver, COMProcessParserConfig, COMRegistry, IEnumerable<U>> map,
            ISymbolResolver resolver, COMProcessParserConfig config, COMRegistry registry) where T : I, new() where I : class
        {
            int chain_offset = ChainOffsetAttribute.GetOffset(typeof(T));
            List<U> entries = new List<U>();
            var buckets = GetBuckets(process, resolver, bucket_symbol, 23);
            foreach (var bucket in buckets)
            {
                // Nothing in this bucket.
                if (bucket.StartEntry.GetNext() == bucket.BaseAddress)
                {
                    continue;
                }

                var start_address = bucket.StartEntry.GetNext();
                var next_bucket = bucket.StartEntry.GetNextChain(process);
                var next_obj = bucket.StartEntry.GetNextObject<T, I>(process, chain_offset);
                do
                {
                    var objs = map(next_obj, process, resolver, config, registry);
                    if (objs != null)
                    {
                        entries.AddRange(objs);
                    }

                    next_obj = next_bucket.GetNextObject<T, I>(process, chain_offset);
                    next_bucket = next_bucket.GetNextChain(process);
                }
                while (next_bucket.GetNext() != start_address);
            }
            return entries;
        }

        private static IEnumerable<COMIPIDEntry> GetClientIpids(IIDObject obj, NtProcess process, ISymbolResolver resolver, COMProcessParserConfig config, COMRegistry registry)
        {
            int pid = process.ProcessId;
            var stdid = obj.GetStdIdentity(process);
            if (stdid != null && (stdid.GetFlags() & SMFLAGS.SMFLAGS_CLIENT_SIDE) != 0)
            {
                var ipid = stdid.GetFirstIpid(process);
                while (ipid != null)
                {
                    if (pid != COMUtilities.GetProcessIdFromIPid(ipid.Ipid))
                    {
                        yield return new COMIPIDEntry(ipid, obj.GetOid(), process, resolver, config, registry);
                    }
                    ipid = ipid.GetNext(process);
                }
            }
        }

        private static List<COMIPIDEntry> ReadClients(NtProcess process, ISymbolResolver resolver, COMProcessParserConfig config, COMRegistry registry)
        {
            const string bucket_symbol = "COIDTable::s_OIDBuckets";
            if (!config.ParseClients)
            {
                return new List<COMIPIDEntry>();
            }

            if (process.Is64Bit)
            {
                return ReadHashTable<CIDObject, COMIPIDEntry, IIDObject>(process, bucket_symbol, GetClientIpids, resolver, config, registry);
            }
            else
            {
                return ReadHashTable<CIDObject32, COMIPIDEntry, IIDObject>(process, bucket_symbol, GetClientIpids, resolver, config, registry);
            }
        }

        private static ActivationContext ReadActivationContext(NtProcess process, ISymbolResolver resolver, COMProcessParserConfig config, COMRegistry registry)
        {
            if (!config.ParseActivationContext)
            {
                return null;
            }
            return ActivationContext.FromProcess(process, false);
        }

        private static IEnumerable<COMRuntimeActivableClassEntry> GetRuntimeServer(IWinRTLocalSvrClassEntry obj, NtProcess process, ISymbolResolver resolver, COMProcessParserConfig config, COMRegistry registry)
        {
            return new COMRuntimeActivableClassEntry[] { new COMRuntimeActivableClassEntry(obj, process, resolver, registry) };
        }

        private static List<COMRuntimeActivableClassEntry> ReadRuntimeActivatableClasses(NtProcess process, ISymbolResolver resolver, COMProcessParserConfig config, COMRegistry registry)
        {
            const string bucket_symbol = "CClassCache::_LSvrActivatableClassEBuckets";
            if (!config.ParseRegisteredClasses)
            {
                return new List<COMRuntimeActivableClassEntry>();
            }

            if (process.Is64Bit)
            {
                if (COMUtilities.IsWindows81OrLess)
                {
                    return ReadHashTable<CWinRTLocalSvrClassEntryWin8, COMRuntimeActivableClassEntry, IWinRTLocalSvrClassEntry>(process, bucket_symbol, GetRuntimeServer, resolver, config, registry);
                }
                else if (COMUtilities.IsWindows10RS4OrLess)
                {
                    return ReadHashTable<CWinRTLocalSvrClassEntry, COMRuntimeActivableClassEntry, IWinRTLocalSvrClassEntry>(process, bucket_symbol, GetRuntimeServer, resolver, config, registry);
                }
                return ReadHashTable<CWinRTLocalSvrClassEntryRS5, COMRuntimeActivableClassEntry, IWinRTLocalSvrClassEntry>(process, bucket_symbol, GetRuntimeServer, resolver, config, registry);
            }
            else
            {
                if (COMUtilities.IsWindows81OrLess)
                {
                    return ReadHashTable<CWinRTLocalSvrClassEntry32Win8, COMRuntimeActivableClassEntry, IWinRTLocalSvrClassEntry>(process, bucket_symbol, GetRuntimeServer, resolver, config, registry);
                }
                else if (COMUtilities.IsWindows10RS4OrLess)
                {
                    return ReadHashTable<CWinRTLocalSvrClassEntry32, COMRuntimeActivableClassEntry, IWinRTLocalSvrClassEntry>(process, bucket_symbol, GetRuntimeServer, resolver, config, registry);
                }
                return ReadHashTable<CWinRTLocalSvrClassEntryRS532, COMRuntimeActivableClassEntry, IWinRTLocalSvrClassEntry>(process, bucket_symbol, GetRuntimeServer, resolver, config, registry);
            }
        }

        private static ICLSvrClassEntry ReadCLSvrClassEntry(NtProcess process, IntPtr address)
        {
            return process.Is64Bit ? (ICLSvrClassEntry)process.ReadStruct<CLSvrClassEntry>(address.ToInt64())
                : process.ReadStruct<CLSvrClassEntry32>(address.ToInt64());
        }

        private static void ReadRegisteredClasses(NtProcess process, ISymbolResolver resolver,
            IntPtr base_address, COMProcessClassApartment apartment,
            int thread_id, List<COMProcessClassRegistration> classes, COMRegistry registry)
        {
            if (base_address == IntPtr.Zero)
            {
                return;
            }

            IntPtr next = base_address;

            do
            {
                ICLSvrClassEntry entry = ReadCLSvrClassEntry(process, next);
                var class_entry = entry.GetClassEntry(process);
                if (class_entry != null)
                {
                    IntPtr vtable_ptr = ReadPointer(process, entry.GetIUnknown());
                    string vtable = resolver.GetModuleRelativeAddress(vtable_ptr);

                    classes.Add(new COMProcessClassRegistration(class_entry.GetGuids()[0],
                        entry.GetIUnknown(), vtable,
                        entry.GetRegFlags(), entry.GetCookie(), thread_id,
                        entry.GetContext(), apartment, registry));
                }

                next = entry.GetNext();
            }
            while (next != IntPtr.Zero && next != base_address );
        }

        private static List<COMProcessClassRegistration> GetRegisteredClasses(NtProcess process, ISymbolResolver resolver, COMProcessParserConfig config, COMRegistry registry)
        {
            var classes = new List<COMProcessClassRegistration>();
            if (!config.ParseRegisteredClasses)
            {
                return classes;
            }
            ReadRegisteredClasses(process, resolver, ReadPointer(process, resolver, "CClassCache::_MTALSvrsFront"), COMProcessClassApartment.MTA, -1, classes, registry);
            ReadRegisteredClasses(process, resolver, ReadPointer(process, resolver, "CClassCache::_NTALSvrsFront"), COMProcessClassApartment.NTA, 0, classes, registry);
            using (var list = process.GetThreads(ThreadAccessRights.QueryLimitedInformation).ToDisposableList())
            {
                foreach (var th in list)
                {
                    IntPtr sta = GetSTALSvrsFront(process, th);
                    if (sta == IntPtr.Zero)
                    {
                        continue;
                    }

                    ReadRegisteredClasses(process, resolver, sta, COMProcessClassApartment.STA, th.ThreadId, classes, registry);
                }
            }
            return classes;
        }

        // combase!tagSOleTlsData::pSTALSvrsFront
        private static int GetSTALSvrsOffset(NtProcess process)
        {
            if (COMUtilities.IsWindows10RS4OrLess)
            {
                return process.Is64Bit ? 0x118 : 0xa8;
            }
            return process.Is64Bit ? 0x110 : 0xa4;
        }

        private static IntPtr GetSTALSvrsFront(NtProcess process, NtThread thread)
        {
            IntPtr p = GetReservedForOle(process, thread);
            if (p == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            return ReadPointer(process, p + GetSTALSvrsOffset(process));
        }

        private static IntPtr GetReservedForOle(NtProcess process, NtThread thread)
        {
            IntPtr teb = thread.TebBaseAddress;
            if (teb == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            if (process.Is64Bit)
            {
                return ReadPointer(process, teb + 0x1758);
            }
            return ReadPointer(process, teb + 0xF80);
        }

        private static string ReadUnicodeString(NtProcess process, IntPtr ptr)
        {
            StringBuilder builder = new StringBuilder();
            int pos = 0;
            do
            {
                byte[] data = process.ReadMemory(ptr.ToInt64() + pos, 2);
                if (data.Length < 2)
                {
                    break;
                }
                char c = BitConverter.ToChar(data, 0);
                if (c == 0)
                {
                    break;
                }
                builder.Append(c);
                pos += 2;
            }
            while (true);
            return builder.ToString();
        }

        internal static string ReadString(NtProcess process, ISymbolResolver resolver, string symbol)
        {
            IntPtr str = resolver.GetAddressOfSymbol(GetSymbolName(symbol));
            if (str != IntPtr.Zero)
            {
                return ReadUnicodeString(process, str);
            }
            return String.Empty;
        }

        internal static int ReadInt(NtProcess process, ISymbolResolver resolver, string symbol)
        {
            IntPtr p = resolver.GetAddressOfSymbol(GetSymbolName(symbol));
            if (p != IntPtr.Zero)
            {
                return process.ReadStruct<int>(p.ToInt64());
            }
            return 0;
        }

        internal static T ReadEnum<T>(NtProcess process, ISymbolResolver resolver, string symbol)
        {
            int value = ReadInt(process, resolver, symbol);
            return (T)Enum.ToObject(typeof(T), value);
        }

        internal static IntPtr ReadPointer(NtProcess process, ISymbolResolver resolver, string symbol)
        {
            return ReadPointer(process, resolver.GetAddressOfSymbol(GetSymbolName(symbol)));
        }

        internal static Guid ReadGuid(NtProcess process, ISymbolResolver resolver, string symbol)
        {
            return ReadGuid(process, resolver.GetAddressOfSymbol(GetSymbolName(symbol)));
        }

        internal static IntPtr ReadPointer(NtProcess process, IntPtr p)
        {
            if (p != IntPtr.Zero)
            {
                if (process.Is64Bit)
                {
                    return process.ReadStruct<IntPtr>(p.ToInt64());
                }
                else
                {
                    return new IntPtr(process.ReadStruct<int>(p.ToInt64()));
                }
            }
            return IntPtr.Zero;
        }

        internal static Guid ReadGuid(NtProcess process, IntPtr p)
        {
            if (p != IntPtr.Zero)
            {
                return new Guid(process.ReadMemory(p.ToInt64(), 16));
            }
            return Guid.Empty;
        }

        internal static IntPtr[] ReadPointerArray(NtProcess process, IntPtr p, int count)
        {
            if (p == IntPtr.Zero)
            {
                return null;
            }
            try
            {
                if (process.Is64Bit)
                {
                    return process.ReadMemoryArray<IntPtr>(p.ToInt64(), count);
                }
                else
                {
                    var ptrs = process.ReadMemoryArray<int>(p.ToInt64(), count);
                    return ptrs.Select(i => new IntPtr(i)).ToArray();
                }
            }
            catch (NtException)
            {
                return null;
            }
        }

        private static string GetProcessFileName(NtProcess process)
        {
            return process.GetImageFilePath(false);
        }

        private static string ReadActivationFilterVTable(NtProcess process, ISymbolResolver resolver)
        {
            IntPtr vtable = ReadPointer(process, ReadPointer(process, resolver, "g_ActivationFilter"));
            if (vtable != IntPtr.Zero)
            {
                return resolver.GetModuleRelativeAddress(vtable);
            }
            return string.Empty;
        }

        public static COMProcessEntry ParseProcess(int pid, COMProcessParserConfig config, COMRegistry registry, IEnumerable<Guid> ipids)
        {
            try
            {
                using (var result = NtProcess.Open(pid, ProcessAccessRights.VmRead | ProcessAccessRights.QueryInformation, false))
                {
                    if (!result.IsSuccess)
                    {
                        return null;
                    }

                    NtProcess process = result.Result;

                    if (process.Is64Bit && !Environment.Is64BitProcess)
                    {
                        return null;
                    }

                    using (ISymbolResolver resolver = new SymbolResolverWrapper(process.Is64Bit,
                        SymbolResolver.Create(process, config.DbgHelpPath, config.SymbolPath)))
                    {
                        Sid user = process.User;

                        return new COMProcessEntry(
                            pid,
                            GetProcessFileName(process),
                            ParseIPIDEntries(process, resolver, config, registry, ipids),
                            process.Is64Bit,
                            GetProcessAppId(process, resolver),
                            GetProcessAccessSecurityDescriptor(process, resolver),
                            GetLrpcSecurityDescriptor(process, resolver),
                            ReadString(process, resolver, "gwszLRPCEndPoint"),
                            ReadEnum<EOLE_AUTHENTICATION_CAPABILITIES>(process, resolver, "gCapabilities"),
                            ReadEnum<RPC_AUTHN_LEVEL>(process, resolver, "gAuthnLevel"),
                            ReadEnum<RPC_IMP_LEVEL>(process, resolver, "gImpLevel"),
                            ReadEnum<GLOBALOPT_UNMARSHALING_POLICY_VALUES>(process, resolver, "g_GLBOPT_UnmarshalingPolicy"),
                            ReadPointer(process, resolver, "gAccessControl"),
                            ReadPointer(process, resolver, "ghwndOleMainThread"),
                            GetRegisteredClasses(process, resolver, config, registry),
                            ReadActivationFilterVTable(process, resolver),
                            ReadClients(process, resolver, config, registry),
                            ReadRuntimeActivatableClasses(process, resolver, config, registry),
                            new COMProcessToken(process),
                            ReadActivationContext(process, resolver, config, registry),
                            ReadPointer(process, resolver, "g_pMTAEmptyCtx"),
                            ReadGuid(process, resolver, "CProcessSecret::s_guidOle32Secret"));
                    }
                }
            }
            catch (NtException)
            {
                return null;
            }
        }

        public static COMProcessEntry ParseProcess(int pid, COMProcessParserConfig config, COMRegistry registry)
        {
            return ParseProcess(pid, config, registry, new Guid[0]);
        }

        public static IEnumerable<COMProcessEntry> GetProcesses(IEnumerable<COMObjRef> objrefs, COMProcessParserConfig config,
            IProgress<Tuple<string, int>> progress, COMRegistry registry)
        {
            var stdobjrefs = objrefs.OfType<COMObjRefStandard>();
            return GetProcesses(stdobjrefs.Select(o => o.ProcessId).Distinct().Select(pid => Process.GetProcessById(pid)),
                config, progress, registry, stdobjrefs.Select(i => i.Ipid));
        }

        public static IEnumerable<COMProcessEntry> GetProcesses(IEnumerable<Process> procs, COMProcessParserConfig config,
            IProgress<Tuple<string, int>> progress, COMRegistry registry)
        {
            return GetProcesses(procs, config, progress, registry, new Guid[0]);
        }

        public static IEnumerable<COMProcessEntry> GetProcesses(IEnumerable<string> services, COMProcessParserConfig config,
            IProgress<Tuple<string, int>> progress, COMRegistry registry)
        {
            return GetProcesses(services.Select(n => ServiceUtils.GetServiceProcessId(n)).Distinct().Where(i => i != 0).Select(pid => Process.GetProcessById(pid)),
                    config, progress, registry);
        }

        public static IEnumerable<COMProcessEntry> GetProcesses(IEnumerable<Process> procs,
            COMProcessParserConfig config, IProgress<Tuple<string, int>> progress,
            COMRegistry registry, IEnumerable<Guid> ipids)
        {
            List<COMProcessEntry> ret = new List<COMProcessEntry>();
            NtToken.EnableDebugPrivilege();
            int total_count = procs.Count();
            int current_count = 0;
            foreach (Process p in procs)
            {
                try
                {
                    if (progress != null)
                    {
                        progress.Report(new Tuple<string, int>(String.Format("Parsing process {0}", p.ProcessName),
                            100 * current_count++ / total_count));
                    }
                    COMProcessEntry proc = ParseProcess(p.Id,
                        config, registry, ipids);
                    if (proc != null)
                    {
                        ret.Add(proc);
                    }
                }
                catch (Win32Exception)
                {
                }
                finally
                {
                    p.Close();
                }
            }

            return ret;
        }
    }

    public enum COMProcessClassApartment
    {
        STA,
        MTA,
        NTA
    }

    public class COMProcessClassRegistration
    {
        public string Name => ClassEntry?.Name ?? string.Empty;
        public Guid Clsid { get; private set; }
        public COMCLSIDEntry ClassEntry { get; private set; }
        public IntPtr ClassFactory { get; private set; }
        public string VTable { get; private set; }
        public COMProcessClassApartment Apartment { get; private set; }
        public REGCLS RegFlags { get; private set; }
        public uint Cookie { get; private set; }
        public int ThreadId { get; private set; }
        public CLSCTX Context { get; private set; }
        public int ProcessID => Process.ProcessId;
        public string ProcessName => Process.Name;
        public bool Registered => ClassEntry != null;

        public COMProcessEntry Process { get; internal set; }

        internal COMProcessClassRegistration(
            Guid clsid, IntPtr class_factory, string vtable,
            REGCLS regflags, uint cookie, int thread_id,
            CLSCTX context, COMProcessClassApartment apartment,
            COMRegistry registry)
        {
            Clsid = clsid;
            ClassFactory = class_factory;
            VTable = vtable;
            Apartment = apartment;
            RegFlags = regflags;
            Cookie = cookie;
            ThreadId = thread_id;
            Context = context;
            ClassEntry = registry?.Clsids.GetGuidEntry(Clsid);
        }

        public override string ToString()
        {
            return $"Class: {Clsid}";
        }
    }

    public class COMProcessEntry : ICOMAccessSecurity
    {
        public int ProcessId { get; private set; }
        public string ExecutablePath { get; private set; }
        public string Name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(ExecutablePath);
            }
        }
        public IEnumerable<COMIPIDEntry> Ipids { get; private set; }
        public IEnumerable<COMIPIDEntry> RunningIpids => Ipids.Where(i => i.IsRunning);
        public bool Is64Bit { get; private set; }
        public Guid AppId { get; private set; }
        public string AccessPermissions { get; private set; }
        public string LRpcPermissions { get; private set; }
        public string User => Token.User;
        public string UserSid => Token.UserSid;
        public string RpcEndpoint { get; private set; }
        public EOLE_AUTHENTICATION_CAPABILITIES Capabilities { get; private set; }
        public RPC_AUTHN_LEVEL AuthnLevel { get; private set; }
        public RPC_IMP_LEVEL ImpLevel { get; private set; }
        public IntPtr AccessControl { get; private set; }
        public IntPtr STAMainHWnd { get; private set; }
        public IEnumerable<COMProcessClassRegistration> Classes { get; private set; }
        public GLOBALOPT_UNMARSHALING_POLICY_VALUES UnmarshalPolicy { get; private set; }
        public IEnumerable<COMIPIDEntry> Clients { get; private set; }
        public IEnumerable<COMRuntimeActivableClassEntry> ActivatableClasses { get; private set; }
        public COMProcessToken Token { get; private set; }
        public ActivationContext ActivationContext { get; }
        public IntPtr MTAContext { get; }
        public Guid ProcessSecret { get; }

        public string PackageId => Token.PackageName?.FullName ?? string.Empty;
        public string PackageName => Token.PackageName?.Name ?? string.Empty;
        public ulong PackageHostId => Token.PackageHostId;
        public bool Sandboxed => Token.Sandboxed;

        private bool CustomMarshalAllowedInternal(bool ac)
        {
            if ((Capabilities & EOLE_AUTHENTICATION_CAPABILITIES.NO_CUSTOM_MARSHAL) != 0)
            {
                return false;
            }

            switch (UnmarshalPolicy)
            {
                case GLOBALOPT_UNMARSHALING_POLICY_VALUES.NORMAL:
                    return true;
                case GLOBALOPT_UNMARSHALING_POLICY_VALUES.HYBRID:
                    return !ac;
                case GLOBALOPT_UNMARSHALING_POLICY_VALUES.STRONG:
                    return false;
                default:
                    return false;
            }
        }

        public bool CustomMarshalAllowed
        {
            get
            {
                return CustomMarshalAllowedInternal(false);
            }
        }

        public bool CustomMarshalAllowedFromAppContainer
        {
            get
            {
                return CustomMarshalAllowedInternal(true);
            }
        }

        public string ActivationFilterVTable
        {
            get;
        }

        string ICOMAccessSecurity.DefaultAccessPermission => string.Empty;

        string ICOMAccessSecurity.DefaultLaunchPermission => string.Empty;

        internal COMProcessEntry(int pid, string path, List<COMIPIDEntry> ipids,
            bool is64bit, Guid appid, string access_perm, string lrpc_perm, string rpc_endpoint, EOLE_AUTHENTICATION_CAPABILITIES capabilities,
            RPC_AUTHN_LEVEL authn_level, RPC_IMP_LEVEL imp_level, GLOBALOPT_UNMARSHALING_POLICY_VALUES unmarshal_policy,
            IntPtr access_control, IntPtr sta_main_hwnd, List<COMProcessClassRegistration> classes,
            string activation_filter_vtable, List<COMIPIDEntry> clients, List<COMRuntimeActivableClassEntry> activatable_classes,
            COMProcessToken token, ActivationContext activation_context, IntPtr mta_context, Guid process_secret)
        {
            ProcessId = pid;
            ExecutablePath = path;
            foreach (var ipid in ipids)
            {
                ipid.Process = this;
            }
            Ipids = ipids.AsReadOnly();
            Is64Bit = is64bit;
            AppId = appid;
            AccessPermissions = access_perm;
            LRpcPermissions = lrpc_perm;
            if (!string.IsNullOrWhiteSpace(rpc_endpoint))
            {
                RpcEndpoint = "OLE" + rpc_endpoint;
            }
            else
            {
                RpcEndpoint = string.Empty;
            }
            Capabilities = capabilities;
            AuthnLevel = authn_level;
            ImpLevel = imp_level;
            AccessControl = access_control;
            STAMainHWnd = sta_main_hwnd;
            Classes = classes.AsReadOnly();
            UnmarshalPolicy = unmarshal_policy;
            foreach (var c in Classes)
            {
                c.Process = this;
            }
            ActivationFilterVTable = activation_filter_vtable;
            foreach (var client in clients)
            {
                client.Process = this;
            }
            Clients = clients.AsReadOnly();
            ActivatableClasses = activatable_classes.AsReadOnly();
            Token = token;
            ActivationContext = activation_context;
            MTAContext = mta_context;
            ProcessSecret = process_secret;
        }

        public override string ToString()
        {
            return $"{ProcessId} {Name}";
        }
    }

    [Flags]
    public enum IPIDFlags : uint
    {
        IPIDF_CONNECTING = 0x1,
        IPIDF_DISCONNECTED = 0x2,
        IPIDF_SERVERENTRY = 0x4,
        IPIDF_NOPING = 0x8,
        IPIDF_COPY = 0x10,
        IPIDF_VACANT = 0x80,
        IPIDF_NONNDRSTUB = 0x100,
        IPIDF_NONNDRPROXY = 0x200,
        IPIDF_NOTIFYACT = 0x400,
        IPIDF_TRIED_ASYNC = 0x800,
        IPIDF_ASYNC_SERVER = 0x1000,
        IPIDF_DEACTIVATED = 0x2000,
        IPIDF_WEAKREFCACHE = 0x4000,
        IPIDF_STRONGREFCACHE = 0x8000,
        IPIDF_UNSECURECALLSALLOWED = 0x10000,
    }

    public class COMMethodEntry
    {
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string Symbol { get; private set; }
        public NdrProcedureDefinition Procedure { get; internal set; }

        internal COMMethodEntry(string name, string address, string symbol)
        {
            Name = name;
            Address = address;
            Symbol = symbol;
        }
    }

    public class COMIPIDEntry : IProxyFormatter, IComGuid
    {
        private readonly COMRegistry m_registry;

        public Guid Ipid { get; private set; }
        public Guid Iid { get; private set; }
        public string Name { get; private set; }
        public IPIDFlags Flags { get; private set; }
        public IntPtr Interface { get; private set; }
        public string InterfaceVTable { get; private set; }
        public IEnumerable<COMMethodEntry> Methods { get; private set; }
        public int MethodCount { get; private set; }
        public IEnumerable<NdrComplexTypeReference> ComplexTypes { get; private set; }
        public IntPtr Stub { get; private set; }
        public string StubVTable { get; private set; }
        public Guid Oxid { get; private set; }
        public int StrongRefs { get; private set; }
        public int WeakRefs { get; private set; }
        public int PrivateRefs { get; private set; }
        public IntPtr ServerSTAHwnd { get; private set; }
        public int ApartmentId
        {
            get
            {
                return COMUtilities.GetApartmentIdFromIPid(Ipid);
            }
        }

        public bool IsRunning
        {
            get
            {
                return (Flags & (IPIDFlags.IPIDF_DISCONNECTED | IPIDFlags.IPIDF_DEACTIVATED)) == 0;
            }
        }

        public COMProcessEntry Process { get; internal set; }

        private COMDualStringArray _stringarray;

        public Guid Oid { get; }
        public IEnumerable<COMStringBinding> StringBindings => _stringarray.StringBindings.AsReadOnly();
        public IEnumerable<COMSecurityBinding> SecurityBindings => _stringarray.SecurityBindings.AsReadOnly();
        public int ProcessId => COMUtilities.GetProcessIdFromIPid(Ipid);
        public string ProcessName => COMUtilities.GetProcessNameById(ProcessId);

        Guid IComGuid.ComGuid => Ipid;

        public byte[] ToObjref()
        {
            MemoryStream stm = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stm);
            writer.Write(Encoding.ASCII.GetBytes("MEOW"));
            writer.Write(1);
            writer.Write(Iid.ToByteArray());
            writer.Write(0);
            writer.Write(1);
            writer.Write(Oxid.ToByteArray(), 0, 8);
            byte[] oid = new byte[8];
            if (Oid == Guid.Empty)
            {
                RandomNumberGenerator rng = RandomNumberGenerator.Create();
                rng.GetBytes(oid);
            }
            else
            {
                Array.Copy(Oid.ToByteArray(), oid, 8);
            }
            writer.Write(oid);
            writer.Write(Ipid.ToByteArray());
            writer.Write(0);
            return stm.ToArray();
        }

        private Dictionary<IntPtr, COMMethodEntry> _method_cache = new Dictionary<IntPtr, COMMethodEntry>();

        private static int GetPointerSize(NtProcess process)
        {
            if (process.Is64Bit)
            {
                return 8;
            }
            else
            {
                return 4;
            }
        }

        private static string GetSymbolName(string symbol)
        {
            int last_index = symbol.LastIndexOf("::");
            if (last_index >= 0)
            {
                symbol = symbol.Substring(last_index + 2);
            }

            last_index = symbol.LastIndexOf("`");
            if (last_index >= 0)
            {
                symbol = symbol.Substring(last_index + 1);
            }
            return symbol;
        }

        private COMMethodEntry ResolveMethod(int index, IntPtr method_ptr, ISymbolResolver resolver,
            COMProcessParserConfig config)
        {
            if (!_method_cache.ContainsKey(method_ptr))
            {
                string address = resolver.GetModuleRelativeAddress(method_ptr);
                string symbol = config.ResolveMethodNames ? resolver.GetSymbolForAddress(method_ptr) : string.Empty;
                string name = index > 2 ? GetSymbolName(symbol) : string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                {
                    switch (index)
                    {
                        case 0:
                            name = "QueryInterface";
                            break;
                        case 1:
                            name = "AddRef";
                            break;
                        case 2:
                            name = "Release";
                            break;
                        default:
                            name = string.Format("Method{0}", index);
                            break;
                    }
                }
                _method_cache[method_ptr] = new COMMethodEntry(name, address, symbol);
            }
            return _method_cache[method_ptr];
        }

        internal COMIPIDEntry(COMProcessParser.IPIDEntryNativeInterface ipid, Guid oid, NtProcess process,
            ISymbolResolver resolver, COMProcessParserConfig config, COMRegistry registry)
        {
            Ipid = ipid.Ipid;
            Iid = ipid.Iid;
            Name = registry.InterfacesToNames.GetGuidEntry(Iid) ?? string.Empty;
            Flags = (IPIDFlags)ipid.Flags;
            Interface = ipid.Interface;
            Stub = ipid.Stub;
            var oxid = ipid.GetOxidEntry(process);
            Oxid = oxid.MOxid;
            Oid = oid;
            _stringarray = oxid.GetBinding(process);
            ServerSTAHwnd = oxid.ServerSTAHwnd;
            StrongRefs = ipid.StrongRefs;
            WeakRefs = ipid.WeakRefs;
            PrivateRefs = ipid.PrivateRefs;
            List<COMMethodEntry> methods = new List<COMMethodEntry>();
            List<NdrComplexTypeReference> complex_types = new List<NdrComplexTypeReference>();
            IntPtr stub_vptr = IntPtr.Zero;
            if (Stub != IntPtr.Zero)
            {
                stub_vptr = COMProcessParser.ReadPointer(process, Stub);
                StubVTable = resolver.GetModuleRelativeAddress(stub_vptr);
            }
            if (Interface != IntPtr.Zero)
            {
                IntPtr vtable_ptr = COMProcessParser.ReadPointer(process, Interface);
                InterfaceVTable = resolver.GetModuleRelativeAddress(vtable_ptr);
                long count = 0;
                IntPtr server_info = IntPtr.Zero;

                // For standard stubs the following exists before the vtable pointer:
                // PMIDL_SERVER_INFO ServerInfo - If ForwardingDispatchTable is NULL
                // DWORD_PTR DispatchTableCount
                // PVOID ForwardingDispatchTable - Used presumably when there's code implementation.
                if (stub_vptr != IntPtr.Zero)
                {
                    IntPtr base_ptr = new IntPtr(stub_vptr.ToInt64() - (GetPointerSize(process) * 3));
                    IntPtr[] stub_info = COMProcessParser.ReadPointerArray(process, base_ptr, 3);
                    if (stub_info != null)
                    {
                        if (stub_info[2] == IntPtr.Zero)
                        {
                            server_info = stub_info[0];
                        }
                        count = stub_info[1].ToInt64();
                    }
                }
                else if (registry.Interfaces.ContainsKey(Iid))
                {
                    count = registry.Interfaces[Iid].NumMethods;
                }

                // Sanity check, 256 methods should be enough for anyone ;-)
                if (count < 3 || count > 256)
                {
                    count = 3;
                }

                IntPtr[] method_ptrs = COMProcessParser.ReadPointerArray(process, vtable_ptr, (int)count);
                if (method_ptrs != null)
                {
                    methods.AddRange(method_ptrs.Select((p, i) => ResolveMethod(i, p, resolver, config)));
                    if (config.ParseStubMethods && server_info != IntPtr.Zero && count > 3)
                    {
                        NdrParser parser = new NdrParser(process, resolver);
                        var procs = parser.ReadFromMidlServerInfo(server_info, 3, (int)count, methods.Skip(3).Select(m => m.Name).ToList()).ToArray();
                        for (int i = 0; i < procs.Length; ++i)
                        {
                            methods[i + 3].Procedure = procs[i];
                        }
                        complex_types.AddRange(parser.ComplexTypes);
                    }
                }
            }
            Methods = methods.AsReadOnly();
            MethodCount = methods.Count;
            ComplexTypes = complex_types.AsReadOnly();
            m_registry = registry;
        }

        internal COMProxyInstance ToProxyInstance()
        {
            if (!Methods.Any())
            {
                return null;
            }
            NdrComProxyDefinition entry = NdrComProxyDefinition.FromProcedures(Name, Iid, COMInterfaceEntry.IID_IUnknown,
                Methods.Count(), Methods.SkipWhile(m => m.Procedure == null).Select(m => m.Procedure));
            return new COMProxyInstance(new NdrComProxyDefinition[] { entry }, ComplexTypes, m_registry);
        }

        public override string ToString()
        {
            return $"IPID: {Ipid} {Name}";
        }

        public string FormatText(ProxyFormatterFlags flags)
        {
            if (!Methods.Any())
            {
                return string.Empty;
            }
            return ToProxyInstance().FormatText(flags);
        }

        public string FormatText()
        {
            return FormatText(ProxyFormatterFlags.None);
        }
    }

    public class COMRuntimeActivableClassEntry
    {
        private readonly COMRegistry m_registry;

        public Guid Clsid { get; }
        public string ActivatableClassId { get; }
        public string FullPackageName { get; }
        public string ActivationFactoryCallback { get; }
        public string ActivationFactoryCallbackPointer { get; }
        public COMRuntimeClassEntry ClassEntry
        {
            get
            {
                if (m_registry.RuntimeClasses.ContainsKey(ActivatableClassId))
                {
                    return m_registry.RuntimeClasses[ActivatableClassId];
                }
                return null;
            }
        }

        internal COMRuntimeActivableClassEntry(COMProcessParser.IWinRTLocalSvrClassEntry entry, NtProcess process, ISymbolResolver resolver, COMRegistry registry)
        {
            Clsid = entry.GetClsid();
            ActivatableClassId = entry.GetActivatableClassId(process);
            FullPackageName = entry.GetPackageFullName(process);
            IntPtr callback = entry.GetActivationFactoryCallback();
            ActivationFactoryCallback = resolver.GetSymbolForAddress(callback, true);
            ActivationFactoryCallbackPointer = resolver.GetModuleRelativeAddress(callback);
            m_registry = registry;
        }

        public override string ToString()
        {
            return $"ActivatableClass: {ActivatableClassId}";
        }
    }
}
