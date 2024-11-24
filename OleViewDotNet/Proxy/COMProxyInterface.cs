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

using NtApiDotNet.Ndr;
using NtApiDotNet.Win32.Rpc;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Proxy.Editor;
using OleViewDotNet.TypeLib;
using OleViewDotNet.TypeLib.Instance;
using OleViewDotNet.TypeManager;
using OleViewDotNet.Utilities;
using OleViewDotNet.Utilities.Format;
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
    private bool m_names_from_type;
    private bool m_modified;

    private static COMProxyInterface GetFromTypeLibrary(COMTypeLibInstance type_lib, Guid iid, COMCLSIDEntry proxy_class, bool cache)
    {
        using var info = type_lib.GetTypeInfoOfGuid(iid);

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
        using COMTypeLibInstance type_lib = COMTypeLibInstance.FromFile(intf.TypeLibVersionEntry.NativePath);
        return GetFromTypeLibrary(type_lib, intf.Iid, intf.ProxyClassEntry, true);
    }

    private void UpdateNames(Type type)
    {
        if (Iid != type.GUID)
        {
            return;
        }

        Entry.Name = type.FullName;
        var methods = type.GetMethods();
        if (methods.Length != Procedures.Count)
        {
            return;
        }
        for (int i = 0; i < methods.Length; ++i)
        {
            Procedures[i].Name = methods[i].Name;
            var ps_names = methods[i].GetParameters().Select(p => p.Name).ToList();
            if (methods[i].ReturnType != typeof(void))
            {
                ps_names.Add("retval");
            }
            if (ps_names.Count != Procedures[i].Parameters.Count)
            {
                continue;
            }
            for (int j = 0; j < ps_names.Count; ++j)
            {
                Procedures[i].Parameters[j].Name = ps_names[j];
            }
        }
    }

    private void UpdateFromFile()
    {
        try
        {
            COMProxyInterfaceNameData names = COMProxyInterfaceNameData.LoadFromCache(Iid);
            names.UpdateNames(this);
            m_modified = false;
        }
        catch
        {
        }
    }

    private void CheckNameUpdate(COMInterfaceEntry ent)
    {
        if (m_names_from_type)
        {
            return;
        }
        m_names_from_type = true;
        if (ent.TryGetRuntimeType(out Type type))
        {
            UpdateNames(type);
        }
        else
        {
            UpdateFromFile();
        }
        m_modified = false;
    }

    private void SetModified()
    {
        m_modified = true;
        COMProxyInterfaceClientBuilder.FlushIidType(Iid);
        COMTypeManager.FlushIidType(Iid);
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// The name of the proxy interface.
    /// </summary>
    public override string Name
    {
        get => Entry.Name;
        set => Entry.Name = CheckName(Entry.Name, value);
    }
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
    public IReadOnlyList<COMProxyInterfaceProcedure> Procedures { get; }

    public NdrComProxyDefinition Entry => RpcProxy.Proxy;

    public IReadOnlyList<COMProxyComplexType> ComplexTypes { get; }

    public string Path => ClassEntry.DefaultServer;

    public Guid Clsid => ClassEntry.Clsid;

    public COMCLSIDEntry ClassEntry { get; }

    public COMProxyFile ProxyFile { get; }

    bool ICOMSourceCodeFormattable.IsFormattable => true;

    Guid ICOMGuid.ComGuid => Iid;

    bool ICOMSourceCodeEditable.IsEditable => true;

    void ICOMSourceCodeEditable.Update()
    {
        SetModified();
    }
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
            Entry.Name = WinRTNameUtils.DemangleName(Entry.Name, Iid);
        }
        if (cache && !m_proxies.ContainsKey(Iid))
        {
            m_proxies.Add(Iid, this);
        }
        Procedures = Entry.Procedures.Select(p => new COMProxyInterfaceProcedure(this, p)).ToList().AsReadOnly();
        ComplexTypes = rpc_proxy.ComplexTypes.Select(c => new COMProxyComplexType(c, this)).ToList().AsReadOnly();
    }

    internal string CheckName(string name, string new_name)
    {
        if (string.IsNullOrEmpty(new_name) || (name == new_name))
        {
            return name;
        }

        SetModified();
        return new_name;
    }

    internal static IEnumerable<COMProxyInterface> GetModifiedProxies() => m_proxies.Values.Where(p => p.m_modified);

    internal RpcClientBase CreateClient(bool scripting = false)
    {
        Type type = CreateClientType(scripting);
        return (RpcClientBase)Activator.CreateInstance(type.BaseType);
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
            instance.CheckNameUpdate(intf);
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

        instance.CheckNameUpdate(intf);
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

        using COMTypeLibInstance type_lib = COMTypeLibInstance.FromFile(path);
        COMCLSIDEntry proxy_class = new(registry, COMKnownGuids.CLSID_PSAutomation, COMServerType.InProcServer32);
        return GetFromTypeLibrary(type_lib, iid, proxy_class, false);
    }
    #endregion

    #region Public Methods
    public Type CreateClientType(bool scripting)
    {
        return COMProxyInterfaceClientBuilder.CreateClientType(this, scripting);
    }

    public string BuildClientSource(bool scripting = false)
    {
        return COMProxyInterfaceClientBuilder.BuildClientSource(this, scripting);
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

        Name = names.Name;
        names.UpdateNames(this);
    }

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        INdrFormatter formatter = builder.GetNdrFormatter();
        if (formatter is INdrFormatterBuilder format_builder)
        {
            if (!builder.InterfacesOnly && ComplexTypes.Count > 0)
            {
                foreach (var type in ComplexTypes)
                {
                    format_builder.FormatComplexType(builder, type.Entry);
                }
                builder.AppendLine();
            }

            format_builder.FormatComProxy(builder, Entry);
            builder.AppendLine();
        }
        else
        {
            if (!builder.InterfacesOnly && ComplexTypes.Count > 0)
            {
                foreach (var type in ComplexTypes)
                {
                    builder.AppendLine(formatter.FormatComplexType(type.Entry).TrimEnd());
                }
                builder.AppendLine();
            }

            builder.AppendLine(formatter.FormatComProxy(Entry).TrimEnd());
            builder.AppendLine();
        }
    }
    #endregion
}
