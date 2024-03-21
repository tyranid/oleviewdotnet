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
using OleViewDotNet.Interop;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Processes.Types;
using OleViewDotNet.Proxy;
using OleViewDotNet.Utilities;
using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OleViewDotNet.Processes;

public class COMIPIDEntry : IProxyFormatter, ICOMGuid, ICOMSourceCodeFormattable
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
    public int ApartmentId => COMUtilities.GetApartmentIdFromIPid(Ipid);

    public bool IsRunning => (Flags & (IPIDFlags.IPIDF_DISCONNECTED | IPIDFlags.IPIDF_DEACTIVATED)) == 0;

    public COMProcessEntry Process { get; internal set; }

    private readonly COMDualStringArray _stringarray;

    public Guid Oid { get; }
    public IEnumerable<COMStringBinding> StringBindings => _stringarray.StringBindings.AsReadOnly();
    public IEnumerable<COMSecurityBinding> SecurityBindings => _stringarray.SecurityBindings.AsReadOnly();
    public int ProcessId => COMUtilities.GetProcessIdFromIPid(Ipid);
    public string ProcessName => MiscUtilities.GetProcessNameById(ProcessId);

    Guid ICOMGuid.ComGuid => Ipid;

    bool ICOMSourceCodeFormattable.IsFormattable => Methods.Any();

    public byte[] ToObjref()
    {
        MemoryStream stm = new();
        BinaryWriter writer = new(stm);
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

    private readonly Dictionary<IntPtr, COMMethodEntry> _method_cache = new();

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
            symbol = symbol.Substring(0, last_index);
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
                name = index switch
                {
                    0 => "QueryInterface",
                    1 => "AddRef",
                    2 => "Release",
                    _ => $"Method{index}",
                };
            }
            _method_cache[method_ptr] = new COMMethodEntry(name, address, symbol);
        }
        return _method_cache[method_ptr];
    }

    internal COMIPIDEntry(IPIDEntryNativeInterface ipid, Guid oid, NtProcess process,
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
        List<COMMethodEntry> methods = new();
        List<NdrComplexTypeReference> complex_types = new();
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
                IntPtr base_ptr = new(stub_vptr.ToInt64() - GetPointerSize(process) * 3);
                IntPtr[] stub_info = COMProcessParser.ReadPointerArray(process, base_ptr, 3);
                if (stub_info is not null)
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
            if (method_ptrs is not null)
            {
                methods.AddRange(method_ptrs.Select((p, i) => ResolveMethod(i, p, resolver, config)));
                if (config.ParseStubMethods && server_info != IntPtr.Zero && count > 3)
                {
                    NdrParser parser = new(process, resolver);
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

    internal COMProxyFile ToProxyInstance()
    {
        if (!Methods.Any())
        {
            return null;
        }
        NdrComProxyDefinition entry = NdrComProxyDefinition.FromProcedures(Name, Iid, COMKnownGuids.IID_IUnknown,
            Methods.Count(), Methods.SkipWhile(m => m.Procedure is null).Select(m => m.Procedure));
        return new COMProxyFile(new NdrComProxyDefinition[] { entry }, ComplexTypes, m_registry, $"IPID Proxy: {Ipid}");
    }

    public override string ToString()
    {
        return $"IPID: {Ipid} {Name}";
    }

    public string FormatText(ProxyFormatterFlags flags = ProxyFormatterFlags.None)
    {
        if (!Methods.Any())
        {
            return string.Empty;
        }
        return ToProxyInstance().FormatText(flags);
    }

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        ICOMSourceCodeFormattable formattable = ToProxyInstance();
        formattable?.Format(builder);
    }
}
