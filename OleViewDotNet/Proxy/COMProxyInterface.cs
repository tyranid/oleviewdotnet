//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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
using NtApiDotNet.Ndr;
using NtApiDotNet.Win32.Rpc;
using NtApiDotNet.Win32.Rpc.Transport;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Proxy.Editor;
using OleViewDotNet.Rpc.Transport;
using OleViewDotNet.TypeLib;
using OleViewDotNet.TypeLib.Parser;
using OleViewDotNet.Utilities;
using OleViewDotNet.Utilities.Format;
using OleViewDotNet.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Proxy;

public sealed class COMProxyInterface : COMProxyTypeInfo, IProxyFormatter, ICOMSourceCodeFormattable, ICOMGuid, ICOMSourceCodeEditable
{
    #region Private Members
    private static readonly Dictionary<Guid, COMProxyInterface> m_proxies = new();
    private readonly COMRegistry m_registry;

    private sealed class EditableParameter : COMSourceCodeEditableObject
    {
        public EditableParameter(NdrProcedureParameter p) 
            : base(() => p.Name, n => p.Name = n)
        {
        }
    }

    private sealed class EditableProcedure : COMSourceCodeEditableObject
    {
        public EditableProcedure(NdrProcedureDefinition proc) 
            : base(() => proc.Name, n => proc.Name = n, proc.Params.Select(p => new EditableParameter(p)))
        {
        }
    }

    private static COMProxyInterface GetFromTypeLibrary(COMTypeLibParser type_lib, Guid iid, COMCLSIDEntry proxy_class, bool cache)
    {
        using var info = type_lib.GetTypeInfoFromGuid(iid);

        var parsed_intf = info.Parse() as COMTypeLibInterfaceBase;

        NativeMethods.CreateProxyFromTypeInfo(info.Instance, null, iid, out IntPtr proxy, out IntPtr ptr);
        TypeInfoVtbl type_info = (ptr.GetStructure<IntPtr>() - Marshal.SizeOf<TypeInfoVtbl>()).GetStructure<TypeInfoVtbl>();
        if (type_info.iid != iid)
        {
            throw new ArgumentException("Invalid COM automation proxy vtable.");
        }

        NdrParser parser = new();
        var stub = type_info.stubVtbl.header;
        IEnumerable<string> names;
        int skip = 3;

        if (parsed_intf.Methods.Count == stub.DispatchTableCount)
        {
            names = parsed_intf.Methods.Skip(3).Select(m => m.Name);
        }
        else if (parsed_intf.Methods.Count < stub.DispatchTableCount)
        {
            names = parsed_intf.Methods.Select(m => m.Name);
            skip = stub.DispatchTableCount - parsed_intf.Methods.Count;
        }
        else
        {
            names = null;
        }

        var result = parser.ReadFromMidlServerInfo(stub.pServerInfo, skip, stub.DispatchTableCount, names?.ToList());

        NdrComProxyDefinition proxy_def = NdrComProxyDefinition.FromProcedures(parsed_intf.Name, iid, COMKnownGuids.IID_IUnknown, stub.DispatchTableCount, result);

        RpcComProxy com_proxy = new(proxy_def);

        return new COMProxyInterface(proxy_class, com_proxy, proxy_class.Database, null, cache);
    }

    private static COMProxyInterface GetFromTypeLibrary(COMInterfaceEntry intf)
    {
        using COMTypeLibParser type_lib = new(intf.TypeLibVersionEntry.NativePath);
        return GetFromTypeLibrary(type_lib, intf.Iid, intf.ProxyClassEntry, true);
    }

    private RpcClientBuilderArguments CreateBuilderArgs(bool scripting)
    {
        RpcClientBuilderArguments args = new();
        args.Flags = RpcClientBuilderFlags.UnsignedChar |
            RpcClientBuilderFlags.NoNamespace | RpcClientBuilderFlags.MarshalComObjects;
        args.ClientName = $"{Name.Replace('.', '_')}_RpcClient";
        if (scripting)
        {
            args.Flags |= RpcClientBuilderFlags.GenerateConstructorProperties |
                RpcClientBuilderFlags.StructureReturn |
                RpcClientBuilderFlags.HideWrappedMethods;
        }
        return args;
    }

    #endregion

    #region Public Properties
    /// <summary>
    /// The name of the proxy interface.
    /// </summary>
    public override string Name => Entry.Name;
    /// <summary>
    /// The IID of the proxy interface.
    /// </summary>
    public Guid Iid => Entry.Iid;
    /// <summary>
    /// The base IID of the proxy interface.
    /// </summary>
    public Guid BaseIid => Entry.BaseIid;
    /// <summary>
    /// Get name of the base IID if known.
    /// </summary>
    public string BaseIidName => m_registry.MapIidToInterface(BaseIid).Name;
    /// <summary>
    /// The number of dispatch methods on the interface.
    /// </summary>
    public int DispatchCount => Entry.DispatchCount;
    /// <summary>
    /// List of parsed procedures for the interface.
    /// </summary>
    public IList<NdrProcedureDefinition> Procedures => Entry.Procedures;

    public NdrComProxyDefinition Entry => RpcProxy.Proxy;

    public IReadOnlyList<NdrComplexTypeReference> ComplexTypes => RpcProxy.ComplexTypes.ToList().AsReadOnly();

    public string Path => ClassEntry.DefaultServer;

    public Guid Clsid => ClassEntry.Clsid;

    public COMCLSIDEntry ClassEntry { get; }

    public COMProxyFile ProxyFile { get; }

    bool ICOMSourceCodeFormattable.IsFormattable => true;

    Guid ICOMGuid.ComGuid => Iid;

    string ICOMSourceCodeEditable.Name { get => Entry.Name; set => Entry.Name = value; }

    bool ICOMSourceCodeEditable.IsEditable => true;

    IReadOnlyList<ICOMSourceCodeEditable> ICOMSourceCodeEditable.Members =>
        Procedures.Select(p => new EditableProcedure(p)).ToList().AsReadOnly();
    #endregion

    #region Internal Members
    internal RpcComProxy RpcProxy { get; }

    internal COMProxyInterface(COMCLSIDEntry clsid, RpcComProxy rpc_proxy, COMRegistry registry, COMProxyFile proxy, bool cache)
    {
        ClassEntry = clsid;
        RpcProxy = rpc_proxy;
        ProxyFile = proxy;
        m_registry = registry;
        if (string.IsNullOrWhiteSpace(Entry.Name))
        {
            Entry.Name = m_registry.MapIidToInterface(Iid).Name;
        }
        else
        {
            Entry.Name = COMUtilities.DemangleWinRTName(Entry.Name, Iid);
        }
        if (cache && !m_proxies.ContainsKey(Iid))
        {
            m_proxies.Add(Iid, this);
        }
    }
    #endregion

    #region Public Static Methods
    public static bool TryGetFromIID(COMInterfaceEntry intf, out COMProxyInterface proxy)
    {
        return m_proxies.TryGetValue(intf.Iid, out proxy);
    }

    public static COMProxyInterface GetFromIID(COMInterfaceEntry intf, bool parse_automation = false)
    {
        if (intf is null || !intf.HasProxy)
        {
            throw new ArgumentException($"Interface {intf.Name} doesn't have a registered proxy");
        }

        if (m_proxies.TryGetValue(intf.Iid, out COMProxyInterface instance))
        {
            return instance;
        }

        COMCLSIDEntry clsid = intf.ProxyClassEntry;
        if (intf.IsAutomationProxy)
        {
            if (parse_automation && intf.TypeLibVersionEntry != null)
            {
                return GetFromTypeLibrary(intf);
            }
            else
            {
                throw new ArgumentException("Can't get proxy for automation interfaces.");
            }
        }

        COMProxyFile.GetFromCLSID(clsid);
        if (!m_proxies.TryGetValue(intf.Iid, out instance))
        {
            throw new ArgumentException($"No Proxy Found for IID {intf.Iid}");
        }

        return instance;
    }

    public static COMProxyInterface GetFromIID(COMInterfaceInstance intf, bool parse_automation = false)
    {
        return GetFromIID(intf.InterfaceEntry, parse_automation);
    }

    public static COMProxyInterface GetFromTypeLibrary(string path, Guid iid, COMRegistry registry)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (registry is null)
        {
            throw new ArgumentNullException(nameof(registry));
        }

        using COMTypeLibParser type_lib = new(path);
        COMCLSIDEntry proxy_class = new(registry, COMKnownGuids.CLSID_PSAutomation, COMServerType.InProcServer32);
        return GetFromTypeLibrary(type_lib, iid, proxy_class, false);
    }
    #endregion

    #region Public Methods
    public RpcClientBase CreateClient(bool scripting = false)
    {
        var args = CreateBuilderArgs(scripting);
        return RpcClientBuilder.CreateClient(RpcProxy, args);
    }

    public RpcClientBase ConnectClient(object obj, bool scripting = false)
    {
        RpcCOMClientTransportFactory.SetupFactory();
        RpcClientBase client = CreateClient(scripting);
        RpcChannelBufferClientTransportConfiguration config = new() { Instance = obj };
        client.Connect($"{RpcCOMClientTransportFactory.COMBufferProtocol}:[proxy]", new RpcTransportSecurity() { Configuration = config });
        return client;
    }

    public Type CreateClientType(bool scripting = false)
    {
        var args = CreateBuilderArgs(scripting);
        return RpcClientBuilder.BuildAssembly(RpcProxy, args, provider: new CSharpCodeProvider()).GetTypes().Where(t => typeof(RpcClientBase).IsAssignableFrom(t)).First();
    }

    public string BuildClientSource(bool scripting = false)
    {
        var args = CreateBuilderArgs(scripting);
        return RpcClientBuilder.BuildSource(RpcProxy, args, provider: new CSharpCodeProvider());
    }

    public string FormatText(ProxyFormatterFlags flags = ProxyFormatterFlags.None)
    {
        COMSourceCodeBuilder builder = new(m_registry);
        builder.HideComments = flags.HasFlag(ProxyFormatterFlags.RemoveComments);
        builder.InterfacesOnly = flags.HasFlag(ProxyFormatterFlags.RemoveComplexTypes);
        ((ICOMSourceCodeFormattable)this).Format(builder);
        return builder.ToString();
    }

    public COMProxyInterfaceNameData GetNames()
    {
        return new(this);
    }

    public void UpdateNames(COMProxyInterfaceNameData names)
    {
        if (Iid != names.Iid)
        {
            throw new ArgumentException("Names object doesn't match the proxy identity");
        }

        bool updated = false;

        if (names.Name is not null && Entry.Name != names.Name)
        {
            Entry.Name = names.Name;
            updated = true;
        }

        names.UpdateNames(this, ref updated);

        if (updated)
        {
            COMWrapperFactory.FlushProxyType(Iid);
        }
    }

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        INdrFormatter formatter = builder.GetNdrFormatter();
        if (!builder.InterfacesOnly && ComplexTypes.Count > 0)
        {
            foreach (var type in ComplexTypes)
            {
                builder.AppendLine(formatter.FormatComplexType(type).TrimEnd());
            }
            builder.AppendLine();
        }

        builder.AppendLine(formatter.FormatComProxy(Entry).TrimEnd());
        builder.AppendLine();
    }
    #endregion
}
