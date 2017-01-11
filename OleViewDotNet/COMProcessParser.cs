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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace OleViewDotNet
{
    internal static class COMProcessParser
    {
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
            IntPtr[] ReadPages(SafeKernelObjectHandle handle);

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

            IntPtr[] IPageAllocator.ReadPages(SafeKernelObjectHandle process)
            {
                return COMProcessParser.ReadArray<IntPtr>(process, _pPageListStart, _cPages);
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
            IntPtr[] IPageAllocator.ReadPages(SafeKernelObjectHandle process)
            {
                return COMProcessParser.ReadArray<int>(process, new IntPtr(_pPageListStart), _cPages).Select(i => new IntPtr(i)).ToArray();
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
            IOXIDEntry GetOxidEntry(SafeKernelObjectHandle process);
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

            IOXIDEntry IPIDEntryNativeInterface.GetOxidEntry(SafeKernelObjectHandle process)
            {
                return COMProcessParser.ReadStruct<OXIDEntryNative>(process, pOXIDEntry);
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

            IOXIDEntry IPIDEntryNativeInterface.GetOxidEntry(SafeKernelObjectHandle process)
            {
                return COMProcessParser.ReadStruct<OXIDEntryNative32>(process, new IntPtr(pOXIDEntry));
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
        }

        internal class SafeHGlobalBuffer : SafeBuffer
        {
            public SafeHGlobalBuffer(int length) : base(true)
            {
                Length = length;
                Initialize((ulong)length);
                SetHandle(Marshal.AllocHGlobal(length));
            }

            public int Length
            {
                get; private set;
            }

            protected override bool ReleaseHandle()
            {
                if (!IsInvalid)
                {
                    Marshal.FreeHGlobal(handle);
                    handle = IntPtr.Zero;
                }
                return true;
            }

            private SafeHGlobalBuffer() : base(false)
            {
                SetHandle(IntPtr.Zero);
                Initialize(0);
            }

            public static SafeHGlobalBuffer Null { get { return new SafeHGlobalBuffer(); } }
        }

        internal class SafeStructureBuffer<T> : SafeHGlobalBuffer where T : new()
        {
            public SafeStructureBuffer(T obj, int additional_size)
                : base(Marshal.SizeOf(obj) + additional_size)
            {
                Marshal.StructureToPtr(obj, handle, false);
            }

            public SafeStructureBuffer() : this(new T(), 0)
            {
            }

            protected override bool ReleaseHandle()
            {
                if (!IsInvalid)
                {
                    Marshal.DestroyStructure(handle, typeof(T));
                }
                return base.ReleaseHandle();
            }

            public virtual T Result
            {
                get
                {
                    if (IsClosed || IsInvalid)
                        throw new ObjectDisposedException("handle");

                    return Marshal.PtrToStructure<T>(handle);
                }
            }
        }

        internal sealed class SafeKernelObjectHandle : SafeHandle
        {
            private SafeKernelObjectHandle()
                : base(IntPtr.Zero, true)
            {
            }

            public SafeKernelObjectHandle(IntPtr handle, bool owns_handle)
              : base(IntPtr.Zero, owns_handle)
            {
                SetHandle(handle);
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                if (CloseHandle(this.handle))
                {
                    this.handle = IntPtr.Zero;
                    return true;
                }
                return false;
            }

            public override bool IsInvalid
            {
                get
                {
                    return this.handle.ToInt64() <= 0;
                }
            }

            public static SafeKernelObjectHandle Null
            {
                get { return new SafeKernelObjectHandle(IntPtr.Zero, false); }
            }
        }

        [Flags]
        enum ProcessAccessRights : uint
        {
            None = 0,
            CreateProcess = 0x0080,
            CreateThread = 0x0002,
            DupHandle = 0x0040,
            QueryInformation = 0x0400,
            QueryLimitedInformation = 0x1000,
            SetInformation = 0x0200,
            SetQuota = 0x0100,
            SuspendResume = 0x0800,
            Terminate = 0x0001,
            VmOperation = 0x0008,
            VmRead = 0x0010,
            VmWrite = 0x0020,
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,
            Delete = 0x00010000,
            ReadControl = 0x00020000,
            WriteDac = 0x00040000,
            WriteOwner = 0x00080000,
            Synchronize = 0x00100000,
            MaximumAllowed = 0x02000000,
        };

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeKernelObjectHandle OpenProcess(ProcessAccessRights dwDesiredAccess,
                                                                 bool bInheritHandle,
                                                                 int dwProcessId
                                                                );
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
              SafeKernelObjectHandle hProcess,
              IntPtr lpBaseAddress,
              SafeBuffer lpBuffer,
              IntPtr nSize,
              out IntPtr lpNumberOfBytesRead
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
              SafeKernelObjectHandle hProcess,
              IntPtr lpBaseAddress,
              byte[] lpBuffer,
              IntPtr nSize,
              out IntPtr lpNumberOfBytesRead
            );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool IsWow64Process(SafeKernelObjectHandle hProcess,
            [MarshalAs(UnmanagedType.Bool)] out bool Wow64Process);
        
        static bool Is64bitProcess(SafeKernelObjectHandle process)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                bool wow64 = false;
                if (!IsWow64Process(process, out wow64))
                {
                    throw new Win32Exception();
                }

                return !wow64;
            }
            else
            {
                return false;
            }
        }

        internal static T[] ReadArray<T>(SafeKernelObjectHandle process, IntPtr ptr, int count) where T : struct
        {
            using (var buf = ReadBuffer(process, ptr, count * Marshal.SizeOf(typeof(T))))
            {
                T[] ret = new T[count];
                if (buf != null)
                {
                    buf.ReadArray(0, ret, 0, ret.Length);
                }
                return ret;
            }
        }

        internal static SafeHGlobalBuffer ReadBuffer(SafeKernelObjectHandle process, IntPtr ptr, int length)
        {
            SafeHGlobalBuffer buf = new SafeHGlobalBuffer(length);
            bool success = false;

            try
            {
                IntPtr out_length;
                success = ReadProcessMemory(process, ptr, buf, new IntPtr(buf.Length), out out_length);
                if (success)
                {
                    return buf;
                }
            }
            finally
            {
                if (!success)
                {
                    buf.Close();
                }
            }

            return null;
        }

        internal static T ReadStruct<T>(SafeKernelObjectHandle process, IntPtr ptr) where T : new()
        {
            using (SafeStructureBuffer<T> buf = new SafeStructureBuffer<T>())
            {
                IntPtr out_length;
                if (ReadProcessMemory(process, ptr, buf, new IntPtr(buf.Length), out out_length))
                {
                    return buf.Result;
                }

                return default(T);
            }
        }

        internal static string ReadUnicodeString(SafeKernelObjectHandle process, IntPtr ptr)
        {
            byte[] data = new byte[2];
            StringBuilder builder = new StringBuilder();
            IntPtr out_length;
            int pos = 0;
            while (ReadProcessMemory(process, ptr + pos, data, new IntPtr(data.Length), out out_length))
            {
                char c = BitConverter.ToChar(data, 0);
                if (c == 0)
                {
                    break;
                }
                builder.Append(c);
                pos += 2;
            }
            return builder.ToString();
        }

        private class PageAllocator
        {
            public IntPtr[] Pages { get; private set; }
            public int EntrySize { get; private set; }
            public int EntriesPerPage { get; private set; }

            void Init<T>(SafeKernelObjectHandle process, IntPtr ipid_table) where T : IPageAllocator, new()
            {
                IPageAllocator page_alloc = ReadStruct<T>(process, ipid_table);
                Pages = page_alloc.ReadPages(process);
                EntrySize = page_alloc.EntrySize;
                EntriesPerPage = page_alloc.EntriesPerPage;
            }

            public PageAllocator(SafeKernelObjectHandle process, IntPtr ipid_table)
            {
                if (Is64bitProcess(process))
                {
                    Init<CInternalPageAllocator>(process, ipid_table);
                }
                else
                {
                    Init<CInternalPageAllocator32>(process, ipid_table);
                }
            }
        }

        static HashSet<uint> _flags = new HashSet<uint>();

        static List<COMIPIDEntry> ParseIPIDEntries<T>(SafeKernelObjectHandle process, IntPtr ipid_table) 
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
                using (var buf = ReadBuffer(process, page, palloc.EntriesPerPage * palloc.EntrySize))
                {
                    if (buf == null)
                    {
                        continue;
                    }
                    for (int entry_index = 0; entry_index < palloc.EntriesPerPage; ++entry_index)
                    {
                        IPIDEntryNativeInterface ipid_entry = buf.Read<T>((ulong)(entry_index * palloc.EntrySize));
                        if (_flags.Add(ipid_entry.Flags))
                        {
                            System.Diagnostics.Trace.WriteLine(String.Format("Flags: {0:X}", ipid_entry.Flags));
                        }
                        if ((ipid_entry.Flags != 0xF1EEF1EE) && (ipid_entry.Flags != 0))
                        {
                            entries.Add(new COMIPIDEntry(ipid_entry, process));
                        }
                    }
                }
            }
            
            return entries;
        }

        static Dictionary<string, IntPtr> _resolved_32bit = new Dictionary<string, IntPtr>();
        static Dictionary<string, IntPtr> _resolved_64bit = new Dictionary<string, IntPtr>();

        static string GetDllName()
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
        static string _dllname = GetDllName();

        static string GetSymbolName(string name)
        {
            return String.Format("{0}!{1}", _dllname, name);
        }

        static IntPtr ResolveAddress(SymbolResolver resolver, bool is64bit, string symbol)
        {
            Dictionary<string, IntPtr> resolved = is64bit ? _resolved_64bit : _resolved_32bit;
            if (resolved.ContainsKey(symbol))
            {
                return resolved[symbol];
            }

            IntPtr ret = resolver.GetAddressOfSymbol(symbol);
            if (ret != IntPtr.Zero)
            {
                resolved[symbol] = ret;
            }

            return ret;
        }

        static List<COMIPIDEntry> ParseIPIDEntries(SafeKernelObjectHandle process, SymbolResolver resolver)
        {
            IntPtr ipid_table = ResolveAddress(resolver, Is64bitProcess(process), GetSymbolName("CIPIDTable::_palloc"));
            if (ipid_table == IntPtr.Zero)
            {
                return new List<COMIPIDEntry>();
            }

            if (Is64bitProcess(process))
            {
                return ParseIPIDEntries<IPIDEntryNative>(process, ipid_table);
            }
            else
            {
                return ParseIPIDEntries<IPIDEntryNative32>(process, ipid_table);
            }
        }

        private static Guid GetProcessAppId(SafeKernelObjectHandle process, SymbolResolver resolver)
        {
            IntPtr appid = ResolveAddress(resolver, Is64bitProcess(process), GetSymbolName("g_AppId"));
            if (appid == IntPtr.Zero)
            {
                return Guid.Empty;
            }
            return ReadStruct<Guid>(process, appid);
        }

        const uint SDDL_REVISION_1 = 1;

        [Flags]
        public enum SecurityInformation
        {
            Owner = 1,
            Group = 2,
            Dacl = 4,
            Label = 0x10,
            All = Owner | Group | Dacl | Label
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true, SetLastError = true)]
        private extern static bool ConvertSecurityDescriptorToStringSecurityDescriptor(IntPtr sd, uint rev, SecurityInformation secinfo, out IntPtr str, out int length);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true, SetLastError = true)]
        private extern static bool ConvertSecurityDescriptorToStringSecurityDescriptor(ref SecurityDescriptorAbsolute sd, 
            uint rev, SecurityInformation secinfo, out IntPtr str, out int length);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true)]
        private extern static IntPtr LocalFree(IntPtr hMem);

        [Flags]
        enum SecurityDescriptorControl : ushort
        {
            OwnerDefaulted = 0x0001,
            GroupDefaulted = 0x0002,
            DaclPresent = 0x0004,
            DaclDefaulted = 0x0008,
            SaclPresent = 0x0010,
            SaclDefaulted = 0x0020,
            DaclAutoInheritReq = 0x0100,
            SaclAutoInheritReq = 0x0200,
            DaclAutoInherited = 0x0400,
            SaclAutoInherited = 0x0800,
            DaclProtected = 0x1000,
            SaclProtected = 0x2000,
            RmControlValid = 0x4000,
            SelfRelative = 0x8000,
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SecurityDescriptorHeader
        {
            public byte Revision;
            public byte Sbz1;
            public SecurityDescriptorControl Control;

            public bool HasFlag(SecurityDescriptorControl control)
            {
                return (control & Control) == control;
            }
        }

        interface ISecurityDescriptor
        {
            IntPtr GetOwner(IntPtr base_address);
            IntPtr GetGroup(IntPtr base_address);
            IntPtr GetSacl(IntPtr base_address);
            IntPtr GetDacl(IntPtr base_address);
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SecurityDescriptorRelative : ISecurityDescriptor
        {
            public SecurityDescriptorHeader Header;
            public int Owner;
            public int Group;
            public int Sacl;
            public int Dacl;

            IntPtr ISecurityDescriptor.GetOwner(IntPtr base_address)
            {
                if (Owner == 0)
                {
                    return IntPtr.Zero;
                }

                return base_address + Owner;
            }

            IntPtr ISecurityDescriptor.GetGroup(IntPtr base_address)
            {
                if (Group == 0)
                {
                    return IntPtr.Zero;
                }

                return base_address + Group;
            }

            IntPtr ISecurityDescriptor.GetSacl(IntPtr base_address)
            {
                if (Sacl == 0)
                {
                    return IntPtr.Zero;
                }

                return base_address + Sacl;
            }

            IntPtr ISecurityDescriptor.GetDacl(IntPtr base_address)
            {
                if (Dacl == 0)
                {
                    return IntPtr.Zero;
                }

                return base_address + Dacl;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SecurityDescriptorAbsolute : ISecurityDescriptor
        {
            public SecurityDescriptorHeader Header;
            public IntPtr Owner;
            public IntPtr Group;
            public IntPtr Sacl;
            public IntPtr Dacl;

            IntPtr ISecurityDescriptor.GetOwner(IntPtr base_address)
            {
                return Owner;
            }

            IntPtr ISecurityDescriptor.GetGroup(IntPtr base_address)
            {
                return Group;
            }

            IntPtr ISecurityDescriptor.GetSacl(IntPtr base_address)
            {
                return Sacl;
            }

            IntPtr ISecurityDescriptor.GetDacl(IntPtr base_address)
            {
                return Dacl;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SecurityDescriptorAbsolute32 : ISecurityDescriptor
        {
            public SecurityDescriptorHeader Header;
            public int Owner;
            public int Group;
            public int Sacl;
            public int Dacl;

            IntPtr ISecurityDescriptor.GetOwner(IntPtr base_address)
            {
                return new IntPtr(Owner);
            }

            IntPtr ISecurityDescriptor.GetGroup(IntPtr base_address)
            {
                return new IntPtr(Group);
            }

            IntPtr ISecurityDescriptor.GetSacl(IntPtr base_address)
            {
                return new IntPtr(Sacl);
            }

            IntPtr ISecurityDescriptor.GetDacl(IntPtr base_address)
            {
                return new IntPtr(Dacl);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SidHeader
        {
            public byte Revision;
            public byte RidCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct AclHeader
        {
            public byte AclRevision;
            public byte Sbz1;
            public ushort AclSize;
            public ushort AceCount;
            public ushort Sbz2;
        }

        private static SafeBuffer ReadSid(SafeKernelObjectHandle process, IntPtr address)
        {
            SidHeader header = ReadStruct<SidHeader>(process, address);
            if (header.Revision != 1)
            {
                return SafeHGlobalBuffer.Null;
            }

            return ReadBuffer(process, address, 8 + header.RidCount * 4);
        }

        private static SafeBuffer ReadAcl(SafeKernelObjectHandle process, IntPtr address)
        {
            AclHeader header = ReadStruct<AclHeader>(process, address);
            if (header.AclRevision > 4)
            {
                return SafeHGlobalBuffer.Null;
            }

            if (header.AclSize < Marshal.SizeOf(typeof(AclHeader)))
            {
                return SafeHGlobalBuffer.Null;
            }

            return ReadBuffer(process, address, header.AclSize);
        }

        private static string ReadSecurityDescriptorFromAddress(SafeKernelObjectHandle process, IntPtr address)
        {
            SecurityDescriptorHeader header = ReadStruct<SecurityDescriptorHeader>(process, address);
            if (header.Revision != 1)
            {
                return String.Empty;
            }

            ISecurityDescriptor sd = null;
            if (header.HasFlag(SecurityDescriptorControl.SelfRelative))
            {
                sd = ReadStruct<SecurityDescriptorRelative>(process, address);
            }
            else if (Is64bitProcess(process))
            {
                sd = ReadStruct<SecurityDescriptorAbsolute>(process, address);
            }
            else
            {
                sd = ReadStruct<SecurityDescriptorAbsolute32>(process, address);
            }

            SecurityDescriptorAbsolute new_sd = new SecurityDescriptorAbsolute();
            new_sd.Header = header;
            new_sd.Header.Control = header.Control & ~SecurityDescriptorControl.SelfRelative;
            List<SafeBuffer> buffers = new List<SafeBuffer>();
            try
            {
                if (!header.HasFlag(SecurityDescriptorControl.OwnerDefaulted))
                {
                    SafeBuffer buf = ReadSid(process, sd.GetOwner(address));
                    if (buf != null)
                    {
                        buffers.Add(buf);
                        new_sd.Owner = buf.DangerousGetHandle();
                    }
                }
                if (!header.HasFlag(SecurityDescriptorControl.OwnerDefaulted))
                {
                    SafeBuffer buf = ReadSid(process, sd.GetGroup(address));
                    if (buf != null)
                    {
                        buffers.Add(buf);
                        new_sd.Group = buf.DangerousGetHandle();
                    }
                }
                if (header.HasFlag(SecurityDescriptorControl.DaclPresent))
                {
                    SafeBuffer buf = ReadAcl(process, sd.GetDacl(address));
                    if (buf != null)
                    {
                        buffers.Add(buf);
                        new_sd.Dacl = buf.DangerousGetHandle();
                    }
                }
                if (header.HasFlag(SecurityDescriptorControl.SaclPresent))
                {
                    SafeBuffer buf = ReadAcl(process, sd.GetSacl(address));
                    if (buf != null)
                    {
                        buffers.Add(buf);
                        new_sd.Sacl = buf.DangerousGetHandle();
                    }
                }

                IntPtr str;
                int length;
                if (ConvertSecurityDescriptorToStringSecurityDescriptor(ref new_sd, SDDL_REVISION_1,
                    SecurityInformation.All, out str, out length))
                {
                    string ret = Marshal.PtrToStringUni(str);
                    LocalFree(str);
                    return ret;
                }
            }
            finally
            {
                foreach (SafeBuffer buf in buffers)
                {
                    buf.Close();
                }
            }

            return String.Empty;
        }

        private static string ReadSecurityDescriptor(SafeKernelObjectHandle process, SymbolResolver resolver, string symbol)
        {
            IntPtr sd = ResolveAddress(resolver, Is64bitProcess(process), GetSymbolName(symbol));
            if (sd == IntPtr.Zero)
            {
                return String.Empty;
            }
            IntPtr sd_ptr;
            if (Is64bitProcess(process))
            {
                sd_ptr = ReadStruct<IntPtr>(process, sd);
            }
            else
            {
                sd_ptr = new IntPtr(ReadStruct<int>(process, sd));
            }

            if (sd_ptr == IntPtr.Zero)
            {
                return "D:NO_ACCESS_CONTROL";
            }

            return ReadSecurityDescriptorFromAddress(process, sd_ptr);
        }

        private static string GetProcessAccessSecurityDescriptor(SafeKernelObjectHandle process, SymbolResolver resolver)
        {
            return ReadSecurityDescriptor(process, resolver, "gSecDesc");
        }

        private static string GetLrpcSecurityDescriptor(SafeKernelObjectHandle process, SymbolResolver resolver)
        {
            return ReadSecurityDescriptor(process, resolver, "gLrpcSecurityDescriptor");
        }

        private static string ReadString(SafeKernelObjectHandle process, SymbolResolver resolver, string symbol)
        {
            IntPtr str = ResolveAddress(resolver, Is64bitProcess(process), GetSymbolName(symbol));
            if (str != IntPtr.Zero)
            {
                return ReadUnicodeString(process, str);
            }
            return String.Empty;
        }

        public static int ReadInt(SafeKernelObjectHandle process, SymbolResolver resolver, string symbol)
        {
            IntPtr p = ResolveAddress(resolver, Is64bitProcess(process), GetSymbolName(symbol));
            if (p != IntPtr.Zero)
            {
                return ReadStruct<int>(process, p);
            }
            return 0;
        }

        public static T ReadEnum<T>(SafeKernelObjectHandle process, SymbolResolver resolver, string symbol)
        {
            int value = ReadInt(process, resolver, symbol);
            return (T)Enum.ToObject(typeof(T), value);
        }

        public static IntPtr ReadPointer(SafeKernelObjectHandle process, SymbolResolver resolver, string symbol)
        {
            bool is64bit = Is64bitProcess(process);
            IntPtr p = ResolveAddress(resolver, is64bit, GetSymbolName(symbol));
            if (p != IntPtr.Zero)
            {
                if (is64bit)
                {
                    return ReadStruct<IntPtr>(process, p);
                }
                else
                {
                    return new IntPtr(ReadStruct<int>(process, p));
                }
            }
            return IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Luid
        {
            public int LowPart;
            public int HighPart;
        }

        const int SE_PRIVILEGE_ENABLED = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public Luid Luid;
            public int Attributes;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool LookupPrivilegeValue(
          string lpSystemName,
          string lpName,
          out Luid lpLuid
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(
              SafeKernelObjectHandle TokenHandle,
              bool DisableAllPrivileges,
              ref TOKEN_PRIVILEGES NewState,
              int BufferLength,
              IntPtr PreviousState,
              IntPtr ReturnLength
            );

        private const int MaximumAllowed = 0x02000000;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool OpenProcessToken(
              IntPtr ProcessHandle,
              int DesiredAccess,
              out SafeKernelObjectHandle TokenHandle
            );

        private static bool EnableDebugPrivilege()
        {
            SafeKernelObjectHandle token;
            if (!OpenProcessToken(new IntPtr(-1), MaximumAllowed, out token))
            {
                throw new Win32Exception();
            }

            using (token)
            {
                TOKEN_PRIVILEGES privs = new TOKEN_PRIVILEGES();
                if (!LookupPrivilegeValue(null, "SeDebugPrivilege", out privs.Luid))
                {
                    throw new Win32Exception();
                }

                privs.PrivilegeCount = 1;
                privs.Attributes = SE_PRIVILEGE_ENABLED;
                if (AdjustTokenPrivileges(token, false, ref privs, 0, IntPtr.Zero, IntPtr.Zero))
                {
                    return Marshal.GetLastWin32Error() == 0;
                }
                return false;
            }   
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool QueryFullProcessImageName(
            SafeKernelObjectHandle hProcess,
            int dwFlags,
            [Out] StringBuilder lpExeName,
            ref int lpdwSize
        );

        private static string GetProcessFileName(SafeKernelObjectHandle process)
        {
            StringBuilder builder = new StringBuilder(260);
            int size = builder.Capacity;
            if (QueryFullProcessImageName(process, 0, builder, ref size))
            {
                return builder.ToString();
            }
            return String.Empty;
        }

        private static string GetProcessUser(SafeKernelObjectHandle process)
        {
            SafeKernelObjectHandle token;
            if (OpenProcessToken(process.DangerousGetHandle(), MaximumAllowed, out token))
            {
                using (token)
                {
                    using (WindowsIdentity id = new WindowsIdentity(token.DangerousGetHandle()))
                    {
                        SecurityIdentifier sid = id.User;
                        try
                        {
                            NTAccount account = (NTAccount)sid.Translate(typeof(NTAccount));
                            return account.Value;
                        }
                        catch (IdentityNotMappedException)
                        {
                            return sid.Value;
                        }
                    }
                }
            }
            return "Unknown";
        }

        public static COMProcessEntry ParseProcess(int pid, string dbghelp_path, string symbol_path)
        {
            using (SafeKernelObjectHandle process = OpenProcess(ProcessAccessRights.VmRead | ProcessAccessRights.QueryInformation, false, pid))
            {
                if (process.IsInvalid)
                {
                    return null;
                }

                bool is64bit = Is64bitProcess(process);
                if (is64bit && !Environment.Is64BitProcess)
                {
                    return null;
                }

                using (SymbolResolver resolver = new SymbolResolver(dbghelp_path, process.DangerousGetHandle(), symbol_path))
                {
                    return new COMProcessEntry(
                        pid,
                        GetProcessFileName(process),
                        ParseIPIDEntries(process, resolver),
                        is64bit,
                        GetProcessAppId(process, resolver),
                        GetProcessAccessSecurityDescriptor(process, resolver),
                        GetLrpcSecurityDescriptor(process, resolver),
                        GetProcessUser(process),
                        ReadString(process, resolver, "gwszLRPCEndPoint"),
                        ReadEnum<EOLE_AUTHENTICATION_CAPABILITIES>(process, resolver, "gCapabilities"),
                        ReadEnum<RPC_AUTHN_LEVEL>(process, resolver, "gAuthnLevel"),
                        ReadEnum<RPC_IMP_LEVEL>(process, resolver, "gImpLevel"),
                        ReadPointer(process, resolver, "gAccessControl"));
                }
            }
        }

        public static IEnumerable<COMProcessEntry> GetProcesses(string dbghelp_path, string symbol_path, IProgress<Tuple<string, int>> progress)
        {
            List<COMProcessEntry> ret = new List<COMProcessEntry>();
            int current_pid = Process.GetCurrentProcess().Id;
            EnableDebugPrivilege();
            IEnumerable<Process> procs = Process.GetProcesses().Where(p => p.Id != current_pid).OrderBy(p => p.ProcessName);
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
                    COMProcessEntry proc = COMProcessParser.ParseProcess(p.Id,
                        dbghelp_path, symbol_path);
                    if (proc != null)
                    {
                        ret.Add(COMProcessParser.ParseProcess(p.Id,
                            dbghelp_path, symbol_path));
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

    public class COMProcessEntry
    {
        public int Pid { get; private set; }
        public string ExecutablePath { get; private set; }
        public string Name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(ExecutablePath);
            }
        }
        public IEnumerable<COMIPIDEntry> Ipids { get; private set; }
        public bool Is64Bit { get; private set; }
        public Guid AppId { get; private set; }
        public string AccessPermissions { get; private set; }
        public string LRpcPermissions { get; private set; }
        public string User { get; private set; }
        public string RpcEndpoint { get; private set; }
        public EOLE_AUTHENTICATION_CAPABILITIES Capabilities { get; private set; }
        public RPC_AUTHN_LEVEL AuthnLevel { get; private set; }
        public RPC_IMP_LEVEL ImpLevel { get; private set; }
        public IntPtr AccessControl { get; private set; }

        internal COMProcessEntry(int pid, string path, List<COMIPIDEntry> ipids, 
            bool is64bit, Guid appid, string access_perm, string lrpc_perm, string user,
            string rpc_endpoint, EOLE_AUTHENTICATION_CAPABILITIES capabilities,
            RPC_AUTHN_LEVEL authn_level, RPC_IMP_LEVEL imp_level,
            IntPtr access_control)
        {
            Pid = pid;
            ExecutablePath = path;
            Ipids = ipids.AsReadOnly();
            Is64Bit = is64bit;
            AppId = appid;
            AccessPermissions = access_perm;
            LRpcPermissions = lrpc_perm;
            User = user;
            if (!String.IsNullOrWhiteSpace(rpc_endpoint))
            {
                RpcEndpoint = "OLE" + rpc_endpoint;
            }
            else
            {
                RpcEndpoint = String.Empty;
            }
            Capabilities = capabilities;
            AuthnLevel = authn_level;
            ImpLevel = imp_level;
            AccessControl = access_control;
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

    public class COMIPIDEntry
    {
        public Guid Ipid { get; private set; }
        public Guid Iid { get; private set; }
        public IPIDFlags Flags { get; private set; }
        public IntPtr Interface { get; private set; }
        public IntPtr Stub { get; private set; }
        public Guid Oxid { get; private set; }
        public int StrongRefs { get; private set; }
        public int WeakRefs { get; private set; }
        public int PrivateRefs { get; private set; }
        public bool IsRunning
        {
            get
            {
                return (Flags & (IPIDFlags.IPIDF_DISCONNECTED | IPIDFlags.IPIDF_DEACTIVATED)) == 0;
            }
        }

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
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] oid = new byte[8];
            rng.GetBytes(oid);
            writer.Write(oid);
            writer.Write(Ipid.ToByteArray());
            writer.Write(0);
            return stm.ToArray();
        }

        internal COMIPIDEntry(COMProcessParser.IPIDEntryNativeInterface ipid, COMProcessParser.SafeKernelObjectHandle process)
        {
            Ipid = ipid.Ipid;
            Iid = ipid.Iid;
            Flags = (IPIDFlags)ipid.Flags;
            Interface = ipid.Interface;
            Stub = ipid.Stub;
            var oxid = ipid.GetOxidEntry(process);
            Oxid = oxid.MOxid;
            StrongRefs = ipid.StrongRefs;
            WeakRefs = ipid.WeakRefs;
            PrivateRefs = ipid.PrivateRefs;
        }
    }
}
