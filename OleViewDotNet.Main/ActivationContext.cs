//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2019
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
using OleViewDotNet.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OleViewDotNet
{
    // Structure definitions taken from https://github.com/deroko/activationcontext/blob/master/sxstypes.h
    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA
    {
        public uint Magic;
        public int HeaderSize;
        public int FormatVersion;
        public int TotalSize;
        public int DefaultTocOffset;
        public int ExtendedTocOffset;
        public int AssemblyRosterOffset;
        public int Flags;
    }

    [Flags]
    enum ACTIVATION_CONTEXT_DATA_TOC_HEADER_FLAGS
    {
        NONE = 0,
        DENSE = 1,
        INORDER = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_TOC_HEADER
    {
        public int HeaderSize;
        public int EntryCount;
        public int FirstEntryOffset; // from ACTIVATION_CONTEXT_DATA base
        public ACTIVATION_CONTEXT_DATA_TOC_HEADER_FLAGS Flags;
    }

    public enum ACTIVATION_CONTEXT_SECTION_FORMAT
    {
        UNKNOWN = 0,
        STRING_TABLE = 1,
        GUID_TABLE = 2,
    }

    public enum ACTIVATION_CONTEXT_SECTION_ID
    {
        UNKNOWN = 0,
        ASSEMBLY_INFORMATION = 1,
        DLL_REDIRECTION = 2,
        CLASS_REDIRECTION = 3,
        COM_SERVER_REDIRECTION = 4,
        COM_INTERFACE_REDIRECTION = 5,
        COM_TYPE_LIBRARY_REDIRECTION = 6,
        COM_PROGID_REDIRECTION = 7,
        GLOBAL_OBJECT_RENAME_TABLE = 8,
        CLR_SURROGATES = 9,
        APPLICATION_SETTINGS = 10,
        COMPATIBILITY_INFO = 11,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_TOC_ENTRY
    {
        public ACTIVATION_CONTEXT_SECTION_ID Id;
        public int Offset;            // from ACTIVATION_CONTEXT_DATA base
        public int Length;           // in bytes
        public ACTIVATION_CONTEXT_SECTION_FORMAT Format;           // ACTIVATION_CONTEXT_SECTION_FORMAT_*
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_EXTENDED_TOC_HEADER
    {
        public int HeaderSize;
        public int EntryCount;
        public int FirstEntryOffset;     // from ACTIVATION_CONTEXT_DATA base
        public int Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_EXTENDED_TOC_ENTRY
    {
        public Guid ExtensionGuid;
        public int TocOffset;            // from ACTIVATION_CONTEXT_DATA base
        public int Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_ASSEMBLY_ROSTER_HEADER
    {
        public int HeaderSize;
        public uint HashAlgorithm;
        public int EntryCount;               // Entry 0 is reserved; this is the number of assemblies plus 1.
        public int FirstEntryOffset;         // From ACTIVATION_CONTEXT_DATA base
        public int AssemblyInformationSectionOffset; // Offset from the ACTIVATION_CONTEXT_DATA base to the
                                                     // header of the assembly information string section.  Needed because
                                                     // the roster entries contain the offsets from the ACTIVATION_CONTEXT_DATA
                                                     // to the assembly information structs, but those structs contain offsets
                                                     // from their section base to the strings etc.
    }

    enum ACTIVATION_CONTEXT_DATA_ASSEMBLY_ROSTER_ENTRY_FLAGS
    {
        NONE = 0,
        INVALID = 1,
        ROOT = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_ASSEMBLY_ROSTER_ENTRY
    {
        public ACTIVATION_CONTEXT_DATA_ASSEMBLY_ROSTER_ENTRY_FLAGS Flags;
        public uint PseudoKey;                // case-insentively-hashed assembly name
        public int AssemblyNameOffset;       // from ACTIVATION_CONTEXT_DATA base
        public int AssemblyNameLength;       // length in bytes
        public int AssemblyInformationOffset; // from ACTIVATION_CONTEXT_DATA base to ACTIVATION_CONTEXT_DATA_ASSEMBLY_INFORMATION
        public int AssemblyInformationLength; // length in bytes
    }

    [Flags]
    enum ACTIVATION_CONTEXT_DATA_ASSEMBLY_INFORMATION_FLAGS
    {
        ROOT_ASSEMBLY = 0x00000001,
        POLICY_APPLIED = 0x00000002,
        ASSEMBLY_POLICY_APPLIED = 0x00000004,
        ROOT_POLICY_APPLIED = 0x00000008,
        PRIVATE_ASSEMBLY = 0x00000010,
    }

    enum ACTCTX_REQUESTED_RUN_LEVEL
    {
        UNSPECIFIED,
        AS_INVOKER,
        HIGHEST_AVAILABLE,
        REQUIRE_ADMIN,
        NUMBERS
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct ACTIVATION_CONTEXT_DATA_ASSEMBLY_INFORMATION
    {
        public int Size;                                 // size of this structure, in bytes
        public ACTIVATION_CONTEXT_DATA_ASSEMBLY_INFORMATION_FLAGS Flags;
        public int EncodedAssemblyIdentityLength;        // in bytes
        public int EncodedAssemblyIdentityOffset;        // offset from section header base

        public int ManifestPathType;
        public int ManifestPathLength;                   // in bytes
        public int ManifestPathOffset;                   // offset from section header base
        public long ManifestLastWriteTime;
        public int PolicyPathType;
        public int PolicyPathLength;                     // in bytes
        public int PolicyPathOffset;                     // offset from section header base
        public long PolicyLastWriteTime;
        public int MetadataSatelliteRosterIndex;
        public int Unused2;
        public int ManifestVersionMajor;
        public int ManifestVersionMinor;
        public int PolicyVersionMajor;
        public int PolicyVersionMinor;
        public int AssemblyDirectoryNameLength; // in bytes
        public int AssemblyDirectoryNameOffset; // from section header base
        public int NumOfFilesInAssembly;
        public int LanguageLength; // in bytes
        public int LanguageOffset; // from section header base
        ACTCTX_REQUESTED_RUN_LEVEL RunLevel;
        public int UiAccess;
    }

    public class ActCtxAssemblyRoster
    {
        public string AssemblyName { get; }
        public string AssemblyDirectoryName { get; }
        public string FullPath { get; }

        private static readonly string SXS_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "WinSxS");

        internal ActCtxAssemblyRoster(ACTIVATION_CONTEXT_DATA_ASSEMBLY_ROSTER_ENTRY entry, ReadHandle handle, int base_offset)
        {
            AssemblyName = string.Empty;
            AssemblyDirectoryName = string.Empty;
            FullPath = string.Empty;

            if ((entry.Flags & ACTIVATION_CONTEXT_DATA_ASSEMBLY_ROSTER_ENTRY_FLAGS.INVALID) != 0)
            {
                return;
            }

            AssemblyName = handle.ReadString(entry.AssemblyNameOffset, entry.AssemblyNameLength);
            if (entry.AssemblyInformationOffset == 0)
            {
                return;
            }

            var info = handle.ReadStructure<ACTIVATION_CONTEXT_DATA_ASSEMBLY_INFORMATION>(entry.AssemblyInformationOffset);
            AssemblyDirectoryName = handle.ReadString(base_offset + info.AssemblyDirectoryNameOffset, info.AssemblyDirectoryNameLength);
            FullPath = Path.Combine(SXS_FOLDER, AssemblyDirectoryName);
        }
    }

    [Flags]
    enum ACTIVATION_CONTEXT_STRING_SECTION_FLAGS
    {
        NONE = 0,
        CASE_INSENSITIVE = 1,
        PSEUDOKEY_ORDER = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_STRING_SECTION_HEADER
    {
        public uint Magic;
        public int HeaderSize;               // in bytes
        public int FormatVersion;
        public int DataFormatVersion;
        public ACTIVATION_CONTEXT_STRING_SECTION_FLAGS Flags;
        public int ElementCount;
        public int ElementListOffset;        // offset from section header
        public int HashAlgorithm;
        public int SearchStructureOffset;    // offset from section header
        public int UserDataOffset;           // offset from section header
        public int UserDataSize;             // in bytes
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_STRING_SECTION_HASH_TABLE
    {
        public int BucketTableEntryCount;
        public int BucketTableOffset;        // offset from section header
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_STRING_SECTION_HASH_BUCKET
    {
        public int ChainCount;
        public int ChainOffset;              // offset from section header
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_STRING_SECTION_ENTRY
    {
        public int PseudoKey;
        public int KeyOffset;            // offset from the section header
        public int KeyLength;            // in bytes
        public int Offset;               // offset from the section header
        public int Length;               // in bytes
        public int AssemblyRosterIndex;  // 1-based index into the assembly roster for the assembly that
                                         // provided this entry.  If the entry is not associated with
                                         // an assembly, zero.
    }

    [Flags]
    enum ACTIVATION_CONTEXT_GUID_SECTION_FLAGS
    {
        NONE = 0,
        ENTRIES_IN_ORDER = 0x00000001
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_GUID_SECTION_HEADER
    {
        public uint Magic;
        public int HeaderSize;               // in bytes
        public int FormatVersion;
        public int DataFormatVersion;
        public ACTIVATION_CONTEXT_GUID_SECTION_FLAGS Flags;
        public int ElementCount;
        public int ElementListOffset;        // offset from section header
        public int SearchStructureOffset;    // offset from section header
        public int UserDataOffset;           // offset from section header
        public int UserDataSize;             // in bytes
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_GUID_SECTION_HASH_TABLE
    {
        public int BucketTableEntryCount;
        public int BucketTableOffset;        // offset from section header
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_GUID_SECTION_HASH_BUCKET
    {
        public int ChainCount;
        public int ChainOffset;              // offset from section header
    }

    // The hash table bucket chain is then a list of offsets from the section header to
    // the section entries for the chain.
    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_GUID_SECTION_ENTRY
    {
        public Guid Guid;
        public int Offset;               // offset from the section header
        public int Length;               // in bytes
        public int AssemblyRosterIndex;  // 1-based index into the assembly roster for the assembly that
                                         // provided this entry.  If the entry is not associated with
                                         // an assembly, zero.
    }

    enum ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL
    {
        INVALID = 0,
        APARTMENT = 1,
        FREE = 2,
        SINGLE = 3,
        BOTH = 4,
        NEUTRAL = 5,
    }

    [Flags]
    enum ACTIVATION_CONTEXT_DATA_COM_SERVER_FLAGS
    {
        HAS_DEFAULT = 0x01 << 8,
        HAS_ICON = 0x02 << 8,
        HAS_CONTENT = 0x04 << 8,
        HAS_THUMBNAIL = 0x08 << 8,
        HAS_DOCPRINT = 0x10 << 8,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION
    {
        public int Size;
        public ACTIVATION_CONTEXT_DATA_COM_SERVER_FLAGS Flags;
        public ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL ThreadingModel;
        public Guid ReferenceClsid;
        public Guid ConfiguredClsid;
        public Guid ImplementedClsid;
        public Guid TypeLibraryId;
        public int ModuleLength; // in bytes
        public int ModuleOffset; // offset from section base because this can be shared across multiple entries
        public int ProgIdLength; // in bytes
        public int ProgIdOffset; // offset from ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION because this is never shared
        public int ShimDataLength; // in bytes
        public int ShimDataOffset; // offset from ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION because this is not shared
        public int MiscStatusDefault;
        public int MiscStatusContent;
        public int MiscStatusThumbnail;
        public int MiscStatusIcon;
        public int MiscStatusDocPrint;
    }

    public class ActCtxComServerRedirection
    {
        public Guid Clsid { get; }
        public Guid ReferenceClsid { get; }
        public Guid ConfiguredClsid { get; }
        public Guid ImplementedClsid { get; }
        public Guid TypeLibraryId { get; }
        public string Module { get; }
        public string FullPath { get; }
        public string ProgId { get; }
        public COMThreadingModel ThreadingModel { get; }

        private static COMThreadingModel FromActCtxThreadingModel(ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL threading_model)
        {
            switch (threading_model)
            {
                case ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL.FREE:
                    return COMThreadingModel.Free;
                case ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL.BOTH:
                    return COMThreadingModel.Both;
                case ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL.INVALID:
                    return COMThreadingModel.None;
                case ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL.NEUTRAL:
                    return COMThreadingModel.Neutral;
                default:
                    return COMThreadingModel.Apartment;
            }
        }

        internal ActCtxComServerRedirection(GuidSectionEntry<ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION> entry, ReadHandle handle, int base_offset, int struct_offset)
        {
            Clsid = entry.Key;
            ReferenceClsid = entry.Entry.ReferenceClsid;
            ConfiguredClsid = entry.Entry.ConfiguredClsid;
            ImplementedClsid = entry.Entry.ImplementedClsid;
            TypeLibraryId = entry.Entry.TypeLibraryId;
            Module = handle.ReadString(base_offset + entry.Entry.ModuleOffset, entry.Entry.ModuleLength);
            ProgId = handle.ReadString(struct_offset + entry.Entry.ProgIdOffset, entry.Entry.ProgIdLength);
            ThreadingModel = FromActCtxThreadingModel(entry.Entry.ThreadingModel);
            if (!string.IsNullOrWhiteSpace(entry.RosterEntry.FullPath))
            {
                FullPath = Path.Combine(entry.RosterEntry.FullPath, Module);
            }
            else
            {
                FullPath = Module;
            }
        }
    }

    public class ActCtxComProgIdRedirection
    {
        public string ProgId { get; }
        public Guid Clsid { get; }

        internal ActCtxComProgIdRedirection(StringSectionEntry<ACTIVATION_CONTEXT_DATA_COM_PROGID_REDIRECTION> entry, ReadHandle handle, int base_offset)
        {
            ProgId = entry.Key;
            Clsid = handle.ReadStructure<Guid>(base_offset + entry.Entry.ConfiguredClsidOffset);
        }
    }

    enum ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_SHIM_TYPE
    {
        UNKNOWN = 0,
        OTHER = 1,
        CLR_CLASS = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_SHIM
    {
        public int Size;
        public int Flags;
        ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_SHIM_TYPE Type;
        public int ModuleLength; // in bytes
        public int ModuleOffset; // offset from section base
        public int TypeLength; // in bytes
        public int TypeOffset; // offset from ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_SHIM
        public int ShimVersionLength; // in bytes
        public int ShimVersionOffset; // offset from ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_SHIM
        public int DataLength; // in bytes
        public int DataOffset; // offset from ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_SHIM
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_COM_PROGID_REDIRECTION
    {
        public int Size;
        public int Flags;
        public int ConfiguredClsidOffset; // offset from section header
    }


    [Flags]
    public enum ACTIVATION_CONTEXT_DATA_DLL_REDIRECTION_PATH_FLAGS
    {
        INCLUDES_BASE_NAME = 0x00000001,
        OMITS_ASSEMBLY_ROOT = 0x00000002,
        EXPAND = 0x00000004,
        SYSTEM_DEFAULT_REDIRECTED_SYSTEM32_DLL = 0x00000008,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_DLL_REDIRECTION
    {
        public int Size;
        public ACTIVATION_CONTEXT_DATA_DLL_REDIRECTION_PATH_FLAGS Flags;
        public int TotalPathLength; // bytewise length of concatenated segments only
        public int PathSegmentCount;
        public int PathSegmentOffset; // offset from section base header so that entries can share
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_DLL_REDIRECTION_PATH_SEGMENT
    {
        public int Length; // in bytes
        public int Offset; // from section header so that individual entries can share
    }

    public class ActCtxDllRedirection
    {
        public string Name { get; }
        public string Path { get; }
        public ACTIVATION_CONTEXT_DATA_DLL_REDIRECTION_PATH_FLAGS Flags { get; }
        internal ActCtxDllRedirection(StringSectionEntry<ACTIVATION_CONTEXT_DATA_DLL_REDIRECTION> entry, ReadHandle handle, int base_offset)
        {
            Name = entry.Key;
            Path = string.Join(@"\",
                handle.ReadArray<ACTIVATION_CONTEXT_DATA_DLL_REDIRECTION_PATH_SEGMENT>(base_offset + entry.Entry.PathSegmentOffset,
                entry.Entry.PathSegmentCount).Select(e => handle.ReadString(base_offset + e.Offset, e.Length)));
            Flags = entry.Entry.Flags;
        }
    }

    [Flags]
    enum ACTIVATION_CONTEXT_DATA_COM_INTERFACE_REDIRECTION_FLAGS
    {
        NONE = 0,
        NUM_METHODS_VALID = 1,
        BASE_INTERFACE_VALID = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_COM_INTERFACE_REDIRECTION
    {
        public int Size;
        public ACTIVATION_CONTEXT_DATA_COM_INTERFACE_REDIRECTION_FLAGS Flags;
        public Guid ProxyStubClsid32;
        public int NumMethods;
        public Guid TypeLibraryId;
        public Guid BaseInterface;
        public int NameLength; // in bytes
        public int NameOffset; // offset from ACTIVATION_CONTEXT_DATA_COM_INTERFACE_REDIRECTION because this is not shared
    }

    public class ActCtxComInterfaceRedirection
    {
        public Guid Iid { get; }
        public Guid ProxyStubClsid32 { get; }
        public int NumMethods { get; }
        public Guid TypeLibraryId { get; }
        public Guid BaseInterface { get; }
        public string Name { get; }

        internal ActCtxComInterfaceRedirection(GuidSectionEntry<ACTIVATION_CONTEXT_DATA_COM_INTERFACE_REDIRECTION> entry, ReadHandle handle, int base_offset)
        {
            Iid = entry.Key;
            var ent = entry.Entry;
            ProxyStubClsid32 = ent.ProxyStubClsid32;
            NumMethods = ent.NumMethods;
            TypeLibraryId = ent.TypeLibraryId;
            BaseInterface = ent.BaseInterface;
            Name = handle.ReadString(entry.Offset + ent.NameOffset, ent.NameLength);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ACTIVATION_CONTEXT_DATA_COM_TYPE_LIBRARY_REDIRECTION
    {
        public int Size;
        public int Flags;
        public int NameLength; // in bytes
        public int NameOffset; // offset from section header
        public ushort ResourceId; // Resource ID of type library resource in PE
        public ushort LibraryFlags; // flags, as defined by the LIBFLAGS enumeration in oaidl.h
        public int HelpDirLength; // in bytes
        public int HelpDirOffset; // offset from ACTIVATION_CONTEXT_DATA_COM_TYPE_LIBRARY_REDIRECTION
        public ushort MajorVersion;
        public ushort MinorVersion;
    }

    public class ActCtxComTypeLibraryRedirection
    {
        public Guid TypeLibraryId { get; }
        public string Name { get; }
        public string HelpDir { get; }
        public Version Version { get; }
        public int ResourceId { get; }
        public System.Runtime.InteropServices.ComTypes.LIBFLAGS LibraryFlags { get; }
        public string FullPath { get; }

        internal ActCtxComTypeLibraryRedirection(GuidSectionEntry<ACTIVATION_CONTEXT_DATA_COM_TYPE_LIBRARY_REDIRECTION> entry, ReadHandle handle, int base_offset)
        {
            TypeLibraryId = entry.Key;
            var ent = entry.Entry;
            Name = handle.ReadString(base_offset + ent.NameOffset, ent.NameLength);
            HelpDir = handle.ReadString(entry.Offset + ent.HelpDirOffset, ent.HelpDirLength);
            LibraryFlags = (System.Runtime.InteropServices.ComTypes.LIBFLAGS)ent.LibraryFlags;
            ResourceId = ent.ResourceId;
            Version = new Version(ent.MajorVersion, ent.MinorVersion);
            if (!string.IsNullOrWhiteSpace(entry.RosterEntry.FullPath))
            {
                FullPath = Path.Combine(entry.RosterEntry.FullPath, Name);
            }
            else
            {
                FullPath = Name;
            }
        }
    }

    class StringSectionEntry<T>
    {
        public string Key { get; }
        public T Entry { get; }
        public int Offset { get; }
        public ActCtxAssemblyRoster RosterEntry { get; }

        public StringSectionEntry(string key, T entry, int offset, ActCtxAssemblyRoster roster_entry)
        {
            Key = key;
            Entry = entry;
            Offset = offset;
            RosterEntry = roster_entry;
        }
    }

    class GuidSectionEntry<T>
    {
        public Guid Key { get; }
        public T Entry { get; }
        public int Offset { get; }
        public ActCtxAssemblyRoster RosterEntry { get; }

        public GuidSectionEntry(Guid key, T entry, int offset, ActCtxAssemblyRoster roster_entry)
        {
            Key = key;
            Entry = entry;
            Offset = offset;
            RosterEntry = roster_entry;
        }
    }

    sealed class ReadHandle : IDisposable
    {
        private readonly GCHandle _handle;
        private readonly ReadHandle _root;

        private void VerifyOffsetAndLength(int offset, int length)
        {
            if (offset < 0 || offset > Length)
            {
                throw new ArgumentException("Invalid offset value", nameof(offset));
            }

            if (length < 0 || (offset + length) > Length)
            {
                throw new ArgumentException("Invalid length value", nameof(length));
            }
        }

        public T ReadStructure<T>(int offset, int length)
        {
            VerifyOffsetAndLength(offset, length);
            if (typeof(T) == typeof(byte[]))
            {
                byte[] ret = new byte[length];
                Marshal.Copy(_handle.AddrOfPinnedObject() + BaseOffset + offset, ret, 0, length);
                return (T)(object)ret;
            }

            if (Marshal.SizeOf<T>() > length)
            {
                throw new ArgumentException("Size too small for structure");
            }
            return Marshal.PtrToStructure<T>(_handle.AddrOfPinnedObject() + offset);
        }

        public T ReadStructure<T>(int offset)
        {
            return ReadStructure<T>(offset, Marshal.SizeOf<T>());
        }

        public string ReadString(int offset, int length)
        {
            VerifyOffsetAndLength(offset, length);
            return Marshal.PtrToStringUni(_handle.AddrOfPinnedObject() + BaseOffset + offset, length / 2).TrimEnd('\0');
        }

        public T[] ReadArray<T>(int offset, int count)
        {
            int element_size = Marshal.SizeOf<T>();
            VerifyOffsetAndLength(offset, count * element_size);
            T[] ret = new T[count];
            for (int i = 0; i < count; ++i)
            {
                ret[i] = ReadStructure<T>(offset + i * element_size);
            }
            return ret;
        }

        public ReadHandle At(int offset)
        {
            return new ReadHandle(_handle, this, offset, TotalLength);
        }

        public void Dispose()
        {
            if (_root == this)
            {
                _handle.Free();
            }
        }

        public int TotalLength { get; }
        public int Length => TotalLength - BaseOffset;
        public int BaseOffset { get; }
        public ReadHandle Root { get; }

        private ReadHandle(GCHandle handle, ReadHandle root, int base_offset, int total_length)
        {
            _handle = handle;
            _root = root;
            BaseOffset = base_offset;
            TotalLength = total_length;
        }

        public ReadHandle(byte[] data)
            : this(GCHandle.Alloc(data, GCHandleType.Pinned),
                  null, 0, data.Length)
        {
            Root = this;
        }
    }

    public sealed class ActivationContext
    {
        const uint ACTCTX_MAGIC = 0x78746341;
        const int ACTCTX_VERSION = 1;
        const uint STRING_SECTION_MAGIC = 0x64487353;
        const uint GUID_SECTION_MAGIC = 0x64487347;

        private List<ActCtxComServerRedirection> _com_servers = new List<ActCtxComServerRedirection>();
        private List<ActCtxComProgIdRedirection> _com_progids = new List<ActCtxComProgIdRedirection>();
        private List<ActCtxDllRedirection> _dll_redir = new List<ActCtxDllRedirection>();
        private List<ActCtxAssemblyRoster> _asm_roster = new List<ActCtxAssemblyRoster>();
        private List<ActCtxComInterfaceRedirection> _com_interfaces = new List<ActCtxComInterfaceRedirection>();
        private List<ActCtxComTypeLibraryRedirection> _com_typelibs = new List<ActCtxComTypeLibraryRedirection>();

        public IReadOnlyCollection<ActCtxComServerRedirection> ComServers => _com_servers;
        public IReadOnlyCollection<ActCtxComProgIdRedirection> ComProgIds => _com_progids;
        public IReadOnlyCollection<ActCtxDllRedirection> DllRedirection => _dll_redir;
        public IReadOnlyCollection<ActCtxComInterfaceRedirection> ComInterfaces => _com_interfaces;
        public IReadOnlyCollection<ActCtxComTypeLibraryRedirection> ComTypeLibs => _com_typelibs;

        private ActCtxAssemblyRoster GetAssemblyRosterEntry(int index)
        {
            if (index < 0 || index >= _asm_roster.Count)
            {
                return _asm_roster[0];
            }
            return _asm_roster[index];
        }

        private IEnumerable<StringSectionEntry<T>> ReadStringSection<T>(ReadHandle handle, int base_offset, int length)
        {
            List<StringSectionEntry<T>> ret = new List<StringSectionEntry<T>>();
            if (length < Marshal.SizeOf<ACTIVATION_CONTEXT_STRING_SECTION_HEADER>())
            {
                return ret;
            }

            var header = handle.ReadStructure<ACTIVATION_CONTEXT_STRING_SECTION_HEADER>(base_offset);
            if (header.Magic != STRING_SECTION_MAGIC || header.FormatVersion != 1)
            {
                return ret;
            }

            foreach (var entry in handle.ReadArray<ACTIVATION_CONTEXT_STRING_SECTION_ENTRY>(base_offset + header.ElementListOffset, header.ElementCount))
            {
                string key = handle.ReadString(base_offset + entry.KeyOffset, entry.KeyLength);
                T value = handle.ReadStructure<T>(base_offset + entry.Offset, entry.Length);
                ret.Add(new StringSectionEntry<T>(key, value, base_offset + entry.Offset, GetAssemblyRosterEntry(entry.AssemblyRosterIndex)));
            }

            return ret;
        }

        private IEnumerable<GuidSectionEntry<T>> ReadGuidSection<T>(ReadHandle handle, int base_offset, int length)
        {
            List<GuidSectionEntry<T>> ret = new List<GuidSectionEntry<T>>();
            if (length < Marshal.SizeOf<ACTIVATION_CONTEXT_GUID_SECTION_HEADER>())
            {
                return ret;
            }

            var header = handle.ReadStructure<ACTIVATION_CONTEXT_GUID_SECTION_HEADER>(base_offset);
            if (header.Magic != GUID_SECTION_MAGIC || header.FormatVersion != 1)
            {
                return ret;
            }

            for (int i = 0; i < header.ElementCount; ++i)
            {
                var entry = handle.ReadStructure<ACTIVATION_CONTEXT_GUID_SECTION_ENTRY>(base_offset + header.ElementListOffset + i * Marshal.SizeOf<ACTIVATION_CONTEXT_GUID_SECTION_ENTRY>());
                T value = handle.ReadStructure<T>(base_offset + entry.Offset, entry.Length);
                ret.Add(new GuidSectionEntry<T>(entry.Guid, value, base_offset + entry.Offset, GetAssemblyRosterEntry(entry.AssemblyRosterIndex)));
            }
            return ret;
        }

        const int ACTCTX_PEB_OFFSET_32 = 0x1F8;
        const int ACTCTX_PEB_OFFSET_64 = 0x2F8;
        const int DEFAULT_ACTCTX_PEB_OFFSET_32 = 0x200;
        const int DEFAULT_ACTCTX_PEB_OFFSET_64 = 0x308;

        public static ActivationContext FromProcess()
        {
            return FromProcess(NtProcess.Current, true);
        }

        public static ActivationContext FromProcess(NtProcess process, bool default_actctx)
        {
            try
            {
                bool is_64bit = process.Is64Bit || Environment.Is64BitOperatingSystem;

                int offset;
                if (default_actctx)
                {
                    offset = is_64bit ? DEFAULT_ACTCTX_PEB_OFFSET_64 : DEFAULT_ACTCTX_PEB_OFFSET_32;
                }
                else
                {
                    offset = is_64bit ? ACTCTX_PEB_OFFSET_64 : ACTCTX_PEB_OFFSET_32;
                }

                long peb_base = process.PebAddress.ToInt64();
                long actctx_base;
                if (is_64bit)
                {
                    actctx_base = process.ReadMemory<long>(peb_base + offset);
                }
                else
                {
                    actctx_base = process.ReadMemory<uint>(peb_base + offset);
                }

                if (actctx_base == 0)
                {
                    return null;
                }

                return FromProcess(process, actctx_base);
            }
            catch (NtException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static ActivationContext FromProcess(NtProcess process, long actctx_base)
        {
            try
            {
                var header = process.ReadMemory<ACTIVATION_CONTEXT_DATA>(actctx_base);
                if (header.Magic != ACTCTX_MAGIC && header.FormatVersion != ACTCTX_VERSION)
                {
                    return null;
                }

                return new ActivationContext(process.ReadMemory(actctx_base, header.TotalSize));
            }
            catch (NtException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public ActivationContext(byte[] actctx)
        {
            using (ReadHandle handle = new ReadHandle(actctx))
            {
                ACTIVATION_CONTEXT_DATA header = handle.ReadStructure<ACTIVATION_CONTEXT_DATA>(0);
                if (header.Magic != ACTCTX_MAGIC && header.FormatVersion != ACTCTX_VERSION)
                {
                    throw new ArgumentException("Invalid header format");
                }
                
                ACTIVATION_CONTEXT_DATA_ASSEMBLY_ROSTER_HEADER roster_header = handle.ReadStructure<ACTIVATION_CONTEXT_DATA_ASSEMBLY_ROSTER_HEADER>(header.AssemblyRosterOffset);
                _asm_roster.AddRange(handle.ReadArray<ACTIVATION_CONTEXT_DATA_ASSEMBLY_ROSTER_ENTRY>(roster_header.FirstEntryOffset, roster_header.EntryCount).Select(e => new ActCtxAssemblyRoster(e, handle, roster_header.AssemblyInformationSectionOffset)));

                ACTIVATION_CONTEXT_DATA_TOC_HEADER toc_header = handle.ReadStructure<ACTIVATION_CONTEXT_DATA_TOC_HEADER>(header.DefaultTocOffset);
                int base_offset = toc_header.FirstEntryOffset;
                for (int i = 0; i < toc_header.EntryCount; ++i)
                {
                    ACTIVATION_CONTEXT_DATA_TOC_ENTRY toc_entry = handle.ReadStructure<ACTIVATION_CONTEXT_DATA_TOC_ENTRY>(base_offset + i * Marshal.SizeOf<ACTIVATION_CONTEXT_DATA_TOC_ENTRY>());
                    if (toc_entry.Format == ACTIVATION_CONTEXT_SECTION_FORMAT.STRING_TABLE)
                    {
                        switch (toc_entry.Id)
                        {
                            case ACTIVATION_CONTEXT_SECTION_ID.COM_PROGID_REDIRECTION:
                                _com_progids.AddRange(ReadStringSection<ACTIVATION_CONTEXT_DATA_COM_PROGID_REDIRECTION>(handle,
                                    toc_entry.Offset, toc_entry.Length).Select(e => new ActCtxComProgIdRedirection(e, handle, toc_entry.Offset)));
                                break;
                            case ACTIVATION_CONTEXT_SECTION_ID.DLL_REDIRECTION:
                                _dll_redir.AddRange(ReadStringSection<ACTIVATION_CONTEXT_DATA_DLL_REDIRECTION>(handle, toc_entry.Offset, toc_entry.Length)
                                    .Select(e => new ActCtxDllRedirection(e, handle, toc_entry.Offset)));
                                break;
                        }
                    }
                    else if (toc_entry.Format == ACTIVATION_CONTEXT_SECTION_FORMAT.GUID_TABLE)
                    {
                        switch (toc_entry.Id)
                        {
                            case ACTIVATION_CONTEXT_SECTION_ID.COM_SERVER_REDIRECTION:
                                _com_servers.AddRange(ReadGuidSection<ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION>(handle, toc_entry.Offset,
                                    toc_entry.Length).Select(e => new ActCtxComServerRedirection(e, handle, toc_entry.Offset, e.Offset)));
                                break;
                            case ACTIVATION_CONTEXT_SECTION_ID.COM_INTERFACE_REDIRECTION:
                                _com_interfaces.AddRange(ReadGuidSection<ACTIVATION_CONTEXT_DATA_COM_INTERFACE_REDIRECTION>(handle, toc_entry.Offset,
                                    toc_entry.Length).Select(e => new ActCtxComInterfaceRedirection(e, handle, toc_entry.Offset)));
                                break;
                            case ACTIVATION_CONTEXT_SECTION_ID.COM_TYPE_LIBRARY_REDIRECTION:
                                _com_typelibs.AddRange(ReadGuidSection<ACTIVATION_CONTEXT_DATA_COM_TYPE_LIBRARY_REDIRECTION>(handle, toc_entry.Offset,
                                    toc_entry.Length).Select(e => new ActCtxComTypeLibraryRedirection(e, handle, toc_entry.Offset)));
                                break;
                        }
                    }
                }
            }
        }
    }
}
