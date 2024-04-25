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
using NtApiDotNet.Win32;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Processes.Types;
using OleViewDotNet.Security;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ActivationContext = OleViewDotNet.Interop.SxS.ActivationContext;

namespace OleViewDotNet.Processes;

public static class COMProcessParser
{
    private readonly static Dictionary<string, int> m_resolved_32 = new();
    private readonly static Dictionary<string, int> m_resolved_native = new();

    private static DllMachineType GetProcessMachineType(NtProcess process)
    {
        try
        {
            if (NativeMethods.IsWow64Process2(process.Handle, out DllMachineType process_machine, out DllMachineType native_machine))
            {
                return process_machine == DllMachineType.UNKNOWN ? native_machine : process_machine;
            }
        }
        catch
        {
        }
        return DllMachineType.UNKNOWN;
    }

    private static string GetFileMD5(string file)
    {
        using var stm = File.OpenRead(file);
        return BitConverter.ToString(MD5.Create().ComputeHash(stm)).Replace("-", string.Empty);
    }

    private static void AddToDictionary(Dictionary<string, int> base_dict, Dictionary<string, int> add_dict)
    {
        foreach (var pair in add_dict)
        {
            base_dict[pair.Key] = pair.Value;
        }
    }

    private static bool _cached_symbols_configured;

    private static void SetupCachedSymbols()
    {
        if (!_cached_symbols_configured)
        {
            _cached_symbols_configured = true;
            // Load any supported symbol files.
            if (Environment.Is64BitProcess)
            {
                AddToDictionary(m_resolved_native, GetSymbolFile(true));
                AddToDictionary(m_resolved_32, GetSymbolFile(false));
            }
            else
            {
                AddToDictionary(m_resolved_native, GetSymbolFile(true));
            }
        }
    }

    private static Dictionary<string, int> GetSymbolFile(bool native)
    {
        var ret = new Dictionary<string, int>();
        try
        {
            string system_path = native ? Environment.SystemDirectory : Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
            string dll_path = Path.Combine(system_path, $"{COMUtilities.GetCOMDllName()}.dll");
            string symbol_path = Path.Combine(AppUtilities.GetAppDirectory(), "symbol_cache", $"{GetFileMD5(dll_path)}.sym");
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

    private static List<COMIPIDEntry> ParseIPIDEntries<T>(NtProcess process, IntPtr ipid_table, ISymbolResolver resolver,
        COMProcessParserConfig config, COMRegistry registry, HashSet<Guid> ipid_set)
        where T : struct, IPIDEntryNativeInterface
    {
        List<COMIPIDEntry> entries = new();
        PageAllocator palloc = new(process, ipid_table);
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

            using var buf = new SafeHGlobalBuffer(data);
            for (int entry_index = 0; entry_index < palloc.EntriesPerPage; ++entry_index)
            {
                IPIDEntryNativeInterface ipid_entry = buf.Read<T>((ulong)(entry_index * palloc.EntrySize));
                if (ipid_entry.Flags != 0xF1EEF1EE && ipid_entry.Flags != 0)
                {
                    if (ipid_set.Count == 0 || ipid_set.Contains(ipid_entry.Ipid))
                    {
                        entries.Add(new COMIPIDEntry(ipid_entry, Guid.Empty, process, resolver, config, registry));
                    }
                }
            }
        }

        return entries;
    }

    private static readonly string _dllname = COMUtilities.GetCOMDllName();

    private static string GetSymbolName(string name)
    {
        return $"{_dllname}!{name}";
    }

    internal static string SymbolFromAddress(ISymbolResolver resolver, bool is64bit, IntPtr address)
    {
        return $"0x{address.ToInt64():X}";
    }

    private static List<COMIPIDEntry> ParseIPIDEntries(NtProcess process, ISymbolResolver resolver, COMProcessParserConfig config,
        COMRegistry registry, IEnumerable<Guid> ipids)
    {
        HashSet<Guid> ipid_set = new(ipids);
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

    private static COMSecurityDescriptor ReadSecurityDescriptorFromAddress(NtProcess process, IntPtr address)
    {
        try
        {
            return new(new SecurityDescriptor(process, address));
        }
        catch (NtException)
        {
            return null;
        }
    }

    private static COMSecurityDescriptor ReadSecurityDescriptor(NtProcess process, ISymbolResolver resolver, string symbol)
    {
        IntPtr sd = resolver.GetAddressOfSymbol(GetSymbolName(symbol));
        if (sd == IntPtr.Zero)
        {
            return null;
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
            return new(new SecurityDescriptor() { Dacl = new Acl() { NullAcl = true } });
        }

        return ReadSecurityDescriptorFromAddress(process, sd_ptr);
    }

    private static COMSecurityDescriptor GetProcessAccessSecurityDescriptor(NtProcess process, ISymbolResolver resolver)
    {
        return ReadSecurityDescriptor(process, resolver, "gSecDesc");
    }

    private static COMSecurityDescriptor GetLrpcSecurityDescriptor(NtProcess process, ISymbolResolver resolver)
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

        return chain.Select((s, i) => new SHashChainEntry(buckets + i * size, s)).ToArray();
    }

    private static List<U> ReadHashTable<T, U, I>(NtProcess process, string bucket_symbol,
        Func<I, NtProcess, ISymbolResolver, COMProcessParserConfig, COMRegistry, IEnumerable<U>> map,
        ISymbolResolver resolver, COMProcessParserConfig config, COMRegistry registry) where T : I, new() where I : class
    {
        int chain_offset = ChainOffsetAttribute.GetOffset(typeof(T));
        List<U> entries = new();
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
                if (objs is not null)
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
        if (stdid is not null && (stdid.GetFlags() & SMFLAGS.SMFLAGS_CLIENT_SIDE) != 0)
        {
            var ipid = stdid.GetFirstIpid(process);
            while (ipid is not null)
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
        return new COMRuntimeActivableClassEntry[] { new(obj, process, resolver, registry) };
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
            if (AppUtilities.IsWindows81OrLess)
            {
                return ReadHashTable<CWinRTLocalSvrClassEntryWin8, COMRuntimeActivableClassEntry, IWinRTLocalSvrClassEntry>(process, bucket_symbol, GetRuntimeServer, resolver, config, registry);
            }
            else if (AppUtilities.IsWindows10RS4OrLess)
            {
                return ReadHashTable<CWinRTLocalSvrClassEntry, COMRuntimeActivableClassEntry, IWinRTLocalSvrClassEntry>(process, bucket_symbol, GetRuntimeServer, resolver, config, registry);
            }
            return ReadHashTable<CWinRTLocalSvrClassEntryRS5, COMRuntimeActivableClassEntry, IWinRTLocalSvrClassEntry>(process, bucket_symbol, GetRuntimeServer, resolver, config, registry);
        }
        else
        {
            if (AppUtilities.IsWindows81OrLess)
            {
                return ReadHashTable<CWinRTLocalSvrClassEntry32Win8, COMRuntimeActivableClassEntry, IWinRTLocalSvrClassEntry>(process, bucket_symbol, GetRuntimeServer, resolver, config, registry);
            }
            else if (AppUtilities.IsWindows10RS4OrLess)
            {
                return ReadHashTable<CWinRTLocalSvrClassEntry32, COMRuntimeActivableClassEntry, IWinRTLocalSvrClassEntry>(process, bucket_symbol, GetRuntimeServer, resolver, config, registry);
            }
            return ReadHashTable<CWinRTLocalSvrClassEntryRS532, COMRuntimeActivableClassEntry, IWinRTLocalSvrClassEntry>(process, bucket_symbol, GetRuntimeServer, resolver, config, registry);
        }
    }

    private static ICLSvrClassEntry ReadCLSvrClassEntry(NtProcess process, IntPtr address)
    {
        return process.Is64Bit ? process.ReadStruct<CLSvrClassEntry>(address.ToInt64())
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
            if (class_entry is not null)
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
        while (next != IntPtr.Zero && next != base_address);
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
        if (AppUtilities.IsWindows10RS4OrLess)
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

        int reservedForOleOffset;
        if (process.Is64Bit)
        {
            reservedForOleOffset = 0x1758;
        }
        else
        {
            reservedForOleOffset = 0xF80;
            if (Environment.Is64BitProcess)
            {
                if (AppUtilities.IsWindows81OrLess)
                {
                    teb += 0x2000;  // teb32. Magic constant is taken from
                                    // https://github.com/DarthTon/Blackbone/blob/607e9a3be9ca01133de2b190f2efb17b3d51db40/src/BlackBone/Subsystem/NativeSubsystem.cpp#L378
                }
                else
                {
                    var wowTebOffset = process.ReadMemory<long>(teb.ToInt64() + 0x180C);
                    teb = new IntPtr(teb.ToInt64() + wowTebOffset);
                }
            }
        }

        return ReadPointer(process, teb + reservedForOleOffset);
    }

    private static string ReadUnicodeString(NtProcess process, IntPtr ptr)
    {
        StringBuilder builder = new();
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
        return string.Empty;
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

    internal static COMProcessEntry ParseProcess(int pid, COMProcessParserConfig config, 
        COMRegistry registry, IEnumerable<Guid> ipids, Func<bool, Dictionary<string, int>> get_resolved_cache)
    {
        try
        {
            SetupCachedSymbols();
            using var result = NtProcess.Open(pid, ProcessAccessRights.VmRead | ProcessAccessRights.QueryInformation, false);
            if (!result.IsSuccess)
            {
                return null;
            }

            NtProcess process = result.Result;

            if (process.Is64Bit && !Environment.Is64BitProcess)
            {
                return null;
            }

            var resolved = get_resolved_cache(process.Is64Bit);

            using ISymbolResolver resolver = new SymbolResolverWrapper(
                SymbolResolver.Create(process, ProgramSettings.DbgHelpPath, ProgramSettings.SymbolPath),
                resolved, GetProcessMachineType(process));
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
        catch (NtException)
        {
            return null;
        }
    }

    public static COMProcessEntry ParseProcess(int pid, COMProcessParserConfig config, COMRegistry registry, IEnumerable<Guid> ipids)
    {
        return ParseProcess(pid, config, registry, ipids, b => b ? m_resolved_native : m_resolved_32);
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
        List<COMProcessEntry> ret = new();
        NtToken.EnableDebugPrivilege();
        int total_count = procs.Count();
        int current_count = 0;
        foreach (Process p in procs)
        {
            try
            {
                progress?.Report(new Tuple<string, int>($"Parsing process {p.ProcessName}",
                        100 * current_count++ / total_count));
                COMProcessEntry proc = ParseProcess(p.Id,
                    config, registry, ipids);
                if (proc is not null)
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

    public static void GenerateSymbolFile(string symbol_dir)
    {
        COMProcessParserConfig config = new()
        {
            ParseStubMethods = true,
            ResolveMethodNames = true,
            ParseRegisteredClasses = true,
            ParseClients = true,
            ParseActivationContext = false,
        };

        Dictionary<string, int> entries = new();
        var proc = ParseProcess(Process.GetCurrentProcess().Id, config, 
            COMRegistry.Load(COMRegistryMode.UserOnly), Array.Empty<Guid>(), _ => entries);

        string dll_name = COMUtilities.GetCOMDllName();
        var symbols = entries.Where(p => p.Key.StartsWith(dll_name));

        if (!symbols.Any())
        {
            throw new ArgumentException("Couldn't parse the process information. Incorrect symbols?");
        }

        var module = SafeLoadLibraryHandle.GetModuleHandle(dll_name);
        string output_file = Path.Combine(symbol_dir, $"{GetFileMD5(module.FullPath)}.sym");
        List<string> lines = new();
        lines.Add($"# {Environment.OSVersion.VersionString} {module.FullPath} {FileVersionInfo.GetVersionInfo(module.FullPath).FileVersion}");
        lines.AddRange(symbols.Select(p => $"{p.Value} {p.Key}"));
        File.WriteAllLines(output_file, lines);
    }
}
