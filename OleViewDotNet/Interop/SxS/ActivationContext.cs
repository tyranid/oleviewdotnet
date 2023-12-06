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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Interop.SxS;

public sealed class ActivationContext
{
    private const uint ACTCTX_MAGIC = 0x78746341;
    private const int ACTCTX_VERSION = 1;
    private const uint STRING_SECTION_MAGIC = 0x64487353;
    private const uint GUID_SECTION_MAGIC = 0x64487347;

    private readonly List<ActCtxComServerRedirection> _com_servers = new();
    private readonly List<ActCtxComProgIdRedirection> _com_progids = new();
    private readonly List<ActCtxDllRedirection> _dll_redir = new();
    private readonly List<ActCtxAssemblyRoster> _asm_roster = new();
    private readonly List<ActCtxComInterfaceRedirection> _com_interfaces = new();
    private readonly List<ActCtxComTypeLibraryRedirection> _com_typelibs = new();

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
        List<StringSectionEntry<T>> ret = new();
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
        List<GuidSectionEntry<T>> ret = new();
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

    private const int ACTCTX_PEB_OFFSET_32 = 0x1F8;
    private const int ACTCTX_PEB_OFFSET_64 = 0x2F8;
    private const int DEFAULT_ACTCTX_PEB_OFFSET_32 = 0x200;
    private const int DEFAULT_ACTCTX_PEB_OFFSET_64 = 0x308;

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
        using ReadHandle handle = new(actctx);
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
