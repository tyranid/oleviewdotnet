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
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Processes;
using OleViewDotNet.Proxy;
using OleViewDotNet.Security;
using OleViewDotNet.TypeLib;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

/// <summary>
/// Form to view the COM registration information
/// </summary>
public partial class COMRegistryViewer : UserControl
{
    private readonly COMRegistry m_registry;
    private readonly HashSet<FilterType> m_filter_types;
    private readonly DisplayMode m_mode;
    private readonly IEnumerable<COMProcessEntry> m_processes;
    private RegistryViewerFilter m_filter;
    private TreeNode[] m_originalNodes;

    private sealed class DynamicTreeNode : TreeNode
    {
        public bool IsGenerated { get; set; }

        public DynamicTreeNode() : base()
        {
        }

        public DynamicTreeNode(string text) : base(text)
        {
        }
    }

    /// <summary>
    /// Enumeration to indicate what to display
    /// </summary>
    public enum DisplayMode
    {
        CLSIDs,
        ProgIDs,
        CLSIDsByName,
        CLSIDsByServer,
        CLSIDsByLocalServer,
        CLSIDsWithSurrogate,
        Interfaces,
        InterfacesByName,
        ImplementedCategories,
        PreApproved,
        IELowRights,
        LocalServices,
        AppIDs,
        Typelibs,
        AppIDsWithIL,
        MimeTypes,
        AppIDsWithAC,
        ProxyCLSIDs,
        Processes,
        RuntimeClasses,
        RuntimeServers,
    }

    private const string FolderKey = "folder.ico";
    private const string InterfaceKey = "interface.ico";
    private const string ClassKey = "class.ico";
    private const string FolderOpenKey = "folderopen.ico";
    private const string ProcessKey = "process.ico";
    private const string ApplicationKey = "application.ico";

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="reg">The COM registry</param>
    /// <param name="mode">The display mode</param>
    public COMRegistryViewer(COMRegistry reg, DisplayMode mode, IEnumerable<COMProcessEntry> processes) 
        : this(reg, mode, processes, SetupTree(reg, mode, processes), GetFilterTypes(mode), GetDisplayName(mode))
    {
    }

    private static string GetDisplayName(DisplayMode mode)
    {
        return mode switch
        {
            DisplayMode.CLSIDsByName => "CLSIDs by Name",
            DisplayMode.CLSIDs => "CLSIDs",
            DisplayMode.ProgIDs => "ProgIDs",
            DisplayMode.CLSIDsByServer => "CLSIDs by Server",
            DisplayMode.CLSIDsByLocalServer => "CLSIDs by Local Server",
            DisplayMode.CLSIDsWithSurrogate => "CLSIDs with Surrogate",
            DisplayMode.Interfaces => "Interfaces",
            DisplayMode.InterfacesByName => "Interfaces by Name",
            DisplayMode.ImplementedCategories => "Implemented Categories",
            DisplayMode.PreApproved => "Explorer Pre-Approved",
            DisplayMode.IELowRights => "IE Low Rights Policy",
            DisplayMode.LocalServices => "Local Services",
            DisplayMode.AppIDs => "AppIDs",
            DisplayMode.AppIDsWithIL => "AppIDs with IL",
            DisplayMode.AppIDsWithAC => "AppIDs with AC",
            DisplayMode.Typelibs => "TypeLibs",
            DisplayMode.MimeTypes => "MIME Types",
            DisplayMode.ProxyCLSIDs => "Proxy CLSIDs",
            DisplayMode.Processes => "COM Processes",
            DisplayMode.RuntimeClasses => "Runtime Classes",
            DisplayMode.RuntimeServers => "Runtime Servers",
            _ => throw new ArgumentException("Invalid mode value"),
        };
    }

    private static IEnumerable<FilterType> GetFilterTypes(DisplayMode mode)
    {
        HashSet<FilterType> filter_types = new();
        switch (mode)
        {
            case DisplayMode.CLSIDsByName:
            case DisplayMode.CLSIDs:
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Interface);
                break;
            case DisplayMode.ProgIDs:
                filter_types.Add(FilterType.ProgID);
                filter_types.Add(FilterType.Interface);
                break;
            case DisplayMode.CLSIDsByServer:
            case DisplayMode.CLSIDsByLocalServer:
            case DisplayMode.CLSIDsWithSurrogate:
            case DisplayMode.ProxyCLSIDs:
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Server);
                filter_types.Add(FilterType.Interface);
                break;
            case DisplayMode.Interfaces:
            case DisplayMode.InterfacesByName:
                filter_types.Add(FilterType.Interface);
                break;
            case DisplayMode.ImplementedCategories:
                filter_types.Add(FilterType.Category);
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Interface);
                break;
            case DisplayMode.PreApproved:
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Interface);
                break;
            case DisplayMode.IELowRights:
                filter_types.Add(FilterType.LowRights);
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Interface);
                break;
            case DisplayMode.AppIDs:
            case DisplayMode.AppIDsWithIL:
            case DisplayMode.AppIDsWithAC:
            case DisplayMode.LocalServices:
                filter_types.Add(FilterType.AppID);
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Interface);
                break;
            case DisplayMode.Typelibs:
                filter_types.Add(FilterType.TypeLib);
                break;
            case DisplayMode.MimeTypes:
                filter_types.Add(FilterType.MimeType);
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Interface);
                break;
            case DisplayMode.Processes:
                filter_types.Add(FilterType.Process);
                filter_types.Add(FilterType.Ipid);
                filter_types.Add(FilterType.AppID);
                break;
            case DisplayMode.RuntimeClasses:
                filter_types.Add(FilterType.RuntimeClass);
                break;
            case DisplayMode.RuntimeServers:
                filter_types.Add(FilterType.RuntimeServer);
                break;
            default:
                throw new ArgumentException("Invalid mode value");
        }
        return filter_types;
    }

    private void UpdateStatusLabel()
    {
        toolStripStatusLabelCount.Text = $"Showing {treeComRegistry.Nodes.Count} of {m_originalNodes.Length} entries";
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="reg">The COM registry</param>
    /// <param name="mode">The display mode</param>
    public COMRegistryViewer(COMRegistry reg, DisplayMode mode, IEnumerable<COMProcessEntry> processes, IEnumerable<TreeNode> nodes, IEnumerable<FilterType> filter_types, string text)
    {
        InitializeComponent();
        m_registry = reg;
        m_filter_types = new HashSet<FilterType>(filter_types);
        m_filter = new RegistryViewerFilter();
        m_mode = mode;
        m_processes = processes;
        treeImageList.Images.Add(ApplicationKey, SystemIcons.Application);
        sourceCodeViewerControl.SetRegistry(reg);

        foreach (FilterMode filter in Enum.GetValues(typeof(FilterMode)))
        {
            comboBoxMode.Items.Add(filter);
        }
        comboBoxMode.SelectedIndex = 0;

        Text = text;

        m_originalNodes = nodes.ToArray();
        UpdateStatusLabel();
    }

    private static TreeNode CreateNode(string text, string image_key, object tag)
    {
        bool dynamic = false;
        if (tag is ICOMClassEntry || tag is COMTypeLibVersionEntry || tag is COMTypeLibCoClass || tag is COMProxyFormatter)
        {
            dynamic = true;
        }

        TreeNode node = dynamic ? new DynamicTreeNode(text) : new TreeNode(text);
        node.ImageKey = image_key;
        node.SelectedImageKey = image_key;
        node.Tag = tag;
        if (dynamic)
        {
            node.Nodes.Add(new TreeNode("DUMMY"));
        }
        return node;
    }

    private static IEnumerable<TreeNode> SetupTree(COMRegistry registry, DisplayMode mode, IEnumerable<COMProcessEntry> processes)
    {   
        try
        {
            switch (mode)
            {
                case DisplayMode.CLSIDsByName:
                    return LoadCLSIDsByNames(registry);
                case DisplayMode.CLSIDs:
                    return LoadCLSIDs(registry);
                case DisplayMode.ProgIDs:
                    return LoadProgIDs(registry);
                case DisplayMode.CLSIDsByServer:
                    return LoadCLSIDByServer(registry, ServerType.None);
                case DisplayMode.CLSIDsByLocalServer:
                    return LoadCLSIDByServer(registry, ServerType.Local);
                case DisplayMode.CLSIDsWithSurrogate:
                    return LoadCLSIDByServer(registry, ServerType.Surrogate);
                case DisplayMode.Interfaces:
                    return LoadInterfaces(registry, false);
                case DisplayMode.InterfacesByName:
                    return LoadInterfaces(registry, true);
                case DisplayMode.ImplementedCategories:
                    return LoadImplementedCategories(registry);
                case DisplayMode.PreApproved:
                    return LoadPreApproved(registry);
                case DisplayMode.IELowRights:
                    return LoadIELowRights(registry);
                case DisplayMode.LocalServices:
                    return LoadLocalServices(registry);
                case DisplayMode.AppIDs:
                    return LoadAppIDs(registry, false, false);
                case DisplayMode.AppIDsWithIL:
                    return LoadAppIDs(registry, true, false);
                case DisplayMode.AppIDsWithAC:
                    return LoadAppIDs(registry, false, true);
                case DisplayMode.Typelibs:
                    return LoadTypeLibs(registry);
                case DisplayMode.MimeTypes:
                    return LoadMimeTypes(registry);
                case DisplayMode.ProxyCLSIDs:
                    return LoadCLSIDByServer(registry, ServerType.Proxies);
                case DisplayMode.Processes:
                    return LoadProcesses(registry, processes);
                case DisplayMode.RuntimeClasses:
                    return LoadRuntimeClasses(registry);
                case DisplayMode.RuntimeServers:
                    return LoadRuntimeServers(registry);
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(null, ex);
        }

        return new TreeNode[0];
    }

    /// <summary>
    /// Build a tooltip for a CLSID entry
    /// </summary>
    /// <param name="ent">The CLSID entry to build the tool tip from</param>
    /// <returns>A string tooltip</returns>
    private static string BuildCLSIDToolTip(COMRegistry registry, COMCLSIDEntry ent)
    {
        StringBuilder strRet = new();

        strRet.AppendLine($"CLSID: {ent.Clsid.FormatGuid()}");
        strRet.AppendLine($"Name: {ent.Name}");
        strRet.AppendLine($"{ent.DefaultServerType}: {ent.DefaultServer}");
        IEnumerable<string> progids = registry.GetProgIdsForClsid(ent.Clsid).Select(p => p.ProgID);
        if (progids.Any())
        {
            strRet.AppendLine("ProgIDs:");
            foreach (string progid in progids)
            {
                strRet.AppendLine($"{progid}");
            }
        }
        if (ent.AppID != Guid.Empty)
        {
            strRet.AppendLine($"AppID: {ent.AppID.FormatGuid()}");
        }
        if (ent.TypeLib != Guid.Empty)
        {
            strRet.AppendLine($"TypeLib: {ent.TypeLib.FormatGuid()}");
        }

        COMInterfaceEntry[] proxies = registry.GetProxiesForClsid(ent);
        if (proxies.Length > 0)
        {
            strRet.AppendLine($"Interface Proxies: {proxies.Length}");
        }

        if (ent.InterfacesLoaded)
        {
            strRet.AppendLine($"Instance Interfaces: {ent.Interfaces.Count()}");
            strRet.AppendLine($"Factory Interfaces: {ent.FactoryInterfaces.Count()}");
        }
        if (ent.DefaultServerType == COMServerType.InProcServer32)
        {
            COMCLSIDServerEntry server = ent.Servers[COMServerType.InProcServer32];
            if (server.HasDotNet)
            {
                strRet.AppendLine($"Assembly: {server.DotNet.AssemblyName}");
                strRet.AppendLine($"Class: {server.DotNet.ClassName}");
                if (!string.IsNullOrWhiteSpace(server.DotNet.CodeBase))
                {
                    strRet.AppendLine($"Codebase: {server.DotNet.CodeBase}");
                }
                if (!string.IsNullOrWhiteSpace(server.DotNet.RuntimeVersion))
                {
                    strRet.AppendLine($"Runtime Version: {server.DotNet.RuntimeVersion}");
                }
            }
        }

        return strRet.ToString();
    }

    /// <summary>
    /// Build a ProgID entry tooltip
    /// </summary>
    /// <param name="ent">The ProgID entry</param>
    /// <returns>The ProgID tooltip</returns>
    private static string BuildProgIDToolTip(COMRegistry registry, COMProgIDEntry ent)
    {
        string strRet;
        COMCLSIDEntry entry = registry.MapClsidToEntry(ent.Clsid);
        if (entry != null)
        {
            strRet = BuildCLSIDToolTip(registry, entry);
        }
        else
        {
            strRet = $"CLSID: {ent.Clsid.FormatGuid()}\n";
        }

        return strRet;
    }

    private static string BuildInterfaceToolTip(COMInterfaceEntry ent, COMInterfaceInstance instance)
    {
        StringBuilder builder = new();

        builder.AppendLine($"Name: {ent.Name}");
        builder.AppendLine($"IID: {ent.Iid.FormatGuid()}");
        if (ent.ProxyClsid != Guid.Empty)
        {
            builder.AppendLine($"ProxyCLSID: {ent.ProxyClsid.FormatGuid()}");
        }
        if (instance != null && instance.Module != null)
        {
            builder.AppendLine($"VTable Address: {instance.Module}+0x{instance.VTableOffset:X}");
        }
        if (ent.HasTypeLib)
        {
            builder.AppendLine($"TypeLib: {ent.TypeLib.FormatGuid()}");
        }

        return builder.ToString();
    }

    private static TreeNode CreateCLSIDNode(COMRegistry registry, COMCLSIDEntry ent)
    {
        TreeNode nodeRet = CreateNode($"{ent.Clsid.FormatGuid()} - {ent.Name}", ClassKey, ent);
        nodeRet.ToolTipText = BuildCLSIDToolTip(registry, ent);
        return nodeRet;
    }

    private static TreeNode CreateInterfaceNode(COMRegistry registry, COMInterfaceEntry ent)
    {
        TreeNode nodeRet = CreateNode($"{ent.Iid.FormatGuid()} - {ent.Name}", InterfaceKey, ent);
        nodeRet.ToolTipText = BuildInterfaceToolTip(ent, null);

        return nodeRet;
    }

    private static TreeNode CreateInterfaceNameNode(COMRegistry registry, COMInterfaceEntry ent, COMInterfaceInstance instance)
    {
        TreeNode nodeRet = CreateNode(ent.Name, InterfaceKey, ent);
        nodeRet.ToolTipText = BuildInterfaceToolTip(ent, instance);

        return nodeRet;
    }

    private static IEnumerable<TreeNode> LoadCLSIDs(COMRegistry registry)
    {
        int i = 0;
        TreeNode[] clsidNodes = new TreeNode[registry.Clsids.Count];
        foreach (COMCLSIDEntry ent in registry.Clsids.Values)
        {
            clsidNodes[i] = CreateCLSIDNode(registry, ent);
            i++;
        }
        return clsidNodes;
    }

    private static TreeNode CreateRuntimeClassNode(COMRuntimeClassEntry ent)
    {
        return CreateNode(ent.Name, ClassKey, ent);
    }

    private static IEnumerable<TreeNode> LoadRuntimeClasses(COMRegistry registry)
    {
        return registry.RuntimeClasses.Select(p => CreateRuntimeClassNode(p.Value));
    }

    private static IEnumerable<TreeNode> LoadRuntimeServers(COMRegistry registry)
    {
        List<TreeNode> serverNodes = new(registry.Clsids.Count);
        foreach (var group in registry.RuntimeClasses.Values.GroupBy(p => p.Server.ToLower()))
        {
            COMRuntimeServerEntry server = registry.MapServerNameToEntry(group.Key);
            if (server == null)
            {
                continue;
            }
            TreeNode node = CreateNode(server.Name, FolderKey, server);
            node.Nodes.AddRange(group.Select(p => CreateRuntimeClassNode(p)).ToArray());
            serverNodes.Add(node);
        }
        return serverNodes.OrderBy(n => n.Text);
    }

    private static IEnumerable<TreeNode> LoadProgIDs(COMRegistry registry)
    {
        int i = 0;
        TreeNode[] progidNodes = new TreeNode[registry.Progids.Count];
        foreach (COMProgIDEntry ent in registry.Progids.Values)
        {
            progidNodes[i] = CreateNode(ent.ProgID, ClassKey, ent);
            progidNodes[i].ToolTipText = BuildProgIDToolTip(registry, ent);
            i++;
        }
        return progidNodes;
    }

    private static IEnumerable<TreeNode> LoadCLSIDsByNames(COMRegistry registry)
    {
        List<TreeNode> nodes = new(registry.Clsids.Count);
        foreach (COMCLSIDEntry ent in registry.Clsids.Values)
        {
            TreeNode node = CreateNode(ent.Name, ClassKey, ent);
            node.ToolTipText = BuildCLSIDToolTip(registry, ent);
            nodes.Add(node);
        }
        
        return nodes.OrderBy(n => n.Text);
    }

    private static string BuildCOMProcessTooltip(COMProcessEntry proc)
    {
        StringBuilder builder = new();
        builder.AppendLine($"Path: {proc.ExecutablePath}");
        builder.AppendLine($"User: {proc.User}");
        if (proc.AppId != Guid.Empty)
        {
            builder.AppendLine($"AppID: {proc.AppId}");
        }
        builder.AppendLine($"Access Permissions: {proc.AccessPermissions}");
        builder.AppendLine($"LRPC Permissions: {proc.LRpcPermissions}");
        if (!string.IsNullOrEmpty(proc.RpcEndpoint))
        {
            builder.AppendLine($"LRPC Endpoint: {proc.RpcEndpoint}");
        }
        builder.AppendLine($"Capabilities: {proc.Capabilities}");
        builder.AppendLine($"Authn Level: {proc.AuthnLevel}");
        builder.AppendLine($"Imp Level: {proc.ImpLevel}");
        if (proc.AccessControl != IntPtr.Zero)
        {
            builder.AppendLine($"Access Control: 0x{proc.AccessControl.ToInt64():X}");
        }
        builder.AppendLine(COMUtilities.FormatBitness(proc.Is64Bit));
        return builder.ToString();
    }

    private static string BuildCOMIpidTooltip(COMIPIDEntry ipid)
    {
        StringBuilder builder = new();
        builder.AppendLine($"Interface: 0x{ipid.Interface.ToInt64():X}");
        if (!string.IsNullOrWhiteSpace(ipid.InterfaceVTable))
        {
            builder.AppendLine($"Interface VTable: {ipid.InterfaceVTable}");
        }
        builder.AppendLine($"Stub: 0x{ipid.Stub.ToInt64():X}");
        if (!string.IsNullOrWhiteSpace(ipid.StubVTable))
        {
            builder.AppendLine($"Stub VTable: {ipid.StubVTable}");
        }
        builder.AppendLine($"Flags: {ipid.Flags}");
        builder.AppendLine($"Strong Refs: {ipid.StrongRefs}");
        builder.AppendLine($"Weak Refs: {ipid.WeakRefs}");
        builder.AppendLine($"Private Refs: {ipid.PrivateRefs}");
        
        return builder.ToString();
    }

    private static string BuildCOMProcessName(COMProcessEntry proc)
    {
        return $"{proc.ProcessId,-8} - {proc.Name} - {proc.User}";
    }

    private static void PopulatorIpids(COMRegistry registry, TreeNode node, COMProcessEntry proc)
    {
        foreach (COMIPIDEntry ipid in proc.Ipids.Where(i => i.IsRunning))
        {
            COMInterfaceEntry intf = registry.MapIidToInterface(ipid.Iid);
            TreeNode ipid_node = CreateNode($"IPID: {ipid.Ipid.FormatGuid()} - IID: {intf.Name}", InterfaceKey, ipid);
            ipid_node.ToolTipText = BuildCOMIpidTooltip(ipid);
            node.Nodes.Add(ipid_node);
        }
    }

    private static TreeNode CreateCOMProcessNode(COMRegistry registry, COMProcessEntry proc, 
        IDictionary<int, IEnumerable<COMAppIDEntry>> appIdsByPid, IDictionary<Guid, IEnumerable<COMCLSIDEntry>> clsidsByAppId)
    {
        TreeNode node = CreateNode(BuildCOMProcessName(proc), ApplicationKey, proc);
        node.ToolTipText = BuildCOMProcessTooltip(proc);

        if (appIdsByPid.ContainsKey(proc.ProcessId) && appIdsByPid[proc.ProcessId].Count() > 0)
        {
            TreeNode services_node = CreateNode("Services", FolderKey, null);
            foreach (COMAppIDEntry appid in appIdsByPid[proc.ProcessId])
            {
                if (clsidsByAppId.ContainsKey(appid.AppId))
                {
                    services_node.Nodes.Add(CreateLocalServiceNode(registry, appid, clsidsByAppId[appid.AppId]));
                }
            }
            node.Nodes.Add(services_node);
        }

        var server_classes = proc.Classes.Where(c => (c.Context & CLSCTX.LOCAL_SERVER) != 0);

        if (server_classes.Any())
        {
            TreeNode classes_node = CreateNode("Classes", FolderKey, null);
            foreach (var c in server_classes)
            {
                classes_node.Nodes.Add(CreateCLSIDNode(registry, registry.MapClsidToEntry(c.Clsid)));
            }

            node.Nodes.Add(classes_node);
        }

        PopulatorIpids(registry, node, proc);
        return node;
    }

    private static IEnumerable<TreeNode> LoadProcesses(COMRegistry registry, IEnumerable<COMProcessEntry> processes)
    {
        var servicesById = COMUtilities.GetServicePids();
        var appidsByService = registry.AppIDs.Values.Where(a => a.IsService).
            GroupBy(a => a.LocalService.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g, StringComparer.OrdinalIgnoreCase);
        var clsidsByAppId = registry.ClsidsByAppId;
        var appsByPid = servicesById.ToDictionary(p => p.Key, p => p.Value.Where(v => appidsByService.ContainsKey(v)).SelectMany(v => appidsByService[v]));

        return processes.Where(p => p.Ipids.Any()).Select(p => CreateCOMProcessNode(registry, p, appsByPid, clsidsByAppId));
    }

    private enum ServerType
    {
        None,
        Local,
        Surrogate,
        Proxies,
    }

    private static bool IsProxyClsid(COMRegistry registry, COMCLSIDEntry ent)
    {
        return ent.DefaultServerType == COMServerType.InProcServer32 && registry.GetProxiesForClsid(ent).Length > 0;
    }

    private static bool HasSurrogate(COMRegistry registry, COMCLSIDEntry ent)
    {
        return registry.AppIDs.ContainsKey(ent.AppID) && !string.IsNullOrWhiteSpace(registry.AppIDs[ent.AppID].DllSurrogate);
    }

    private class COMCLSIDServerEqualityComparer : IEqualityComparer<COMCLSIDServerEntry>
    {
        public bool Equals(COMCLSIDServerEntry x, COMCLSIDServerEntry y)
        {
            return x.Server.Equals(y.Server, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(COMCLSIDServerEntry obj)
        {
            return obj.Server.ToLower().GetHashCode();
        }
    }

    private static IEnumerable<TreeNode> LoadCLSIDByServer(COMRegistry registry, ServerType serverType)
    {
        IEnumerable<KeyValuePair<COMCLSIDServerEntry, List<COMCLSIDEntry>>> servers = null;

        if (serverType == ServerType.Surrogate)
        {
            servers = registry.Clsids.Values.Where(c => HasSurrogate(registry, c))
                .GroupBy(c => registry.AppIDs[c.AppID].DllSurrogate, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => new COMCLSIDServerEntry(COMServerType.LocalServer32, g.Key), g => g.AsEnumerable().ToList());
        }
        else
        {
            Dictionary<COMCLSIDServerEntry, List<COMCLSIDEntry>> dict = 
                new(new COMCLSIDServerEqualityComparer());
            IEnumerable<COMCLSIDEntry> clsids = registry.Clsids.Values.Where(e => e.Servers.Count > 0);
            if (serverType == ServerType.Proxies)
            {
                clsids = clsids.Where(c => IsProxyClsid(registry, c));
            }

            foreach (COMCLSIDEntry entry in clsids)
            {
                foreach (COMCLSIDServerEntry server in entry.Servers.Values)
                {
                    if (!dict.ContainsKey(server))
                    {
                        dict[server] = new List<COMCLSIDEntry>();
                    }
                    dict[server].Add(entry);
                }
            }

            servers = dict;

            if (serverType == ServerType.Local)
            {
                servers = servers.Where(p => p.Key.ServerType == COMServerType.LocalServer32);
            }
        }

        List<TreeNode> serverNodes = new(registry.Clsids.Count);
        foreach (var pair in servers)
        {
            TreeNode node = CreateNode(pair.Key.Server, FolderKey, pair.Key);
            node.ToolTipText = pair.Key.Server;
            node.Nodes.AddRange(pair.Value.OrderBy(c => c.Name).Select(c => CreateClsidNode(registry, c)).ToArray());
            serverNodes.Add(node);
        }

        return serverNodes.OrderBy(n => n.Text);
    }

    private static IEnumerable<TreeNode> LoadInterfaces(COMRegistry registry, bool by_name)
    {
        IEnumerable<TreeNode> intfs = null;
        if (by_name)
        {
            intfs = registry.Interfaces.Values.OrderBy(i => i.Name).Select(i => CreateInterfaceNameNode(registry, i, null));
        }
        else
        {
            intfs = registry.Interfaces.Values.Select(i => CreateInterfaceNode(registry, i));
        }
        return intfs;
    }

    private static TreeNode CreateClsidNode(COMRegistry registry, COMCLSIDEntry ent)
    {
        TreeNode currNode = CreateNode(ent.Name, ClassKey, ent);
        currNode.ToolTipText = BuildCLSIDToolTip(registry, ent);
        return currNode;
    }

    private static TreeNode CreateLocalServiceNode(COMRegistry registry, COMAppIDEntry appidEnt, IEnumerable<COMCLSIDEntry> clsids)
    {
        string name = appidEnt.LocalService.DisplayName;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = appidEnt.LocalService.Name;
        }

        TreeNode node = CreateNode(name, FolderKey, appidEnt);
        node.ToolTipText = BuildAppIdTooltip(appidEnt);
        node.Nodes.AddRange(clsids.OrderBy(c => c.Name).Select(c => CreateClsidNode(registry, c)).ToArray());
        return node;
    }

    private static IEnumerable<TreeNode> LoadLocalServices(COMRegistry registry)
    {
        var clsidsByAppId = registry.ClsidsByAppId;
        var appids = registry.AppIDs;

        List<TreeNode> serverNodes = new();
        foreach (var pair in clsidsByAppId)
        {   
            if(appids.ContainsKey(pair.Key) && appids[pair.Key].IsService)
            {
                serverNodes.Add(CreateLocalServiceNode(registry, appids[pair.Key], pair.Value));
            }
        }

        return serverNodes.OrderBy(n => n.Text);
    }

    private static string LimitString(string s, int max)
    {
        if (s == null)
            return string.Empty;
        if (s.Length > max)
        {
            return s.Substring(0, max) + "...";
        }
        return s;
    }

    private static string BuildAppIdTooltip(COMAppIDEntry appidEnt)
    {
        StringBuilder builder = new();

        builder.AppendLine($"AppID: {appidEnt.AppId}");
        if (!string.IsNullOrWhiteSpace(appidEnt.RunAs))
        {
            builder.AppendLine($"RunAs: {appidEnt.RunAs}");
        }

        if (appidEnt.IsService)
        {
            COMAppIDServiceEntry service = appidEnt.LocalService;
            builder.AppendLine($"Service Name: {service.Name}");
            if (!string.IsNullOrWhiteSpace(service.DisplayName))
            {
                builder.AppendLine($"Display Name: {service.DisplayName}");
            }
            if (!string.IsNullOrWhiteSpace(service.UserName))
            {
                builder.AppendLine($"Service User: {service.UserName}");
            }
            builder.AppendLine($"Image Path: {service.ImagePath}");
            if (!string.IsNullOrWhiteSpace(service.ServiceDll))
            {
                builder.AppendLine($"Service DLL: {service.ServiceDll}");
            }
        }

        if (appidEnt.HasLaunchPermission)
        {
            builder.AppendLine($"Launch: {LimitString(appidEnt.LaunchPermission?.ToSddl(), 64)}");
        }

        if (appidEnt.HasAccessPermission)
        {
            builder.AppendLine($"Access: {LimitString(appidEnt.AccessPermission?.ToSddl(), 64)}");
        }

        if (appidEnt.RotFlags != COMAppIDRotFlags.None)
        {
            builder.AppendLine($"RotFlags: {appidEnt.RotFlags}");
        }

        if (!string.IsNullOrWhiteSpace(appidEnt.DllSurrogate))
        {
            builder.AppendLine($"DLL Surrogate: {appidEnt.DllSurrogate}");
        }

        if (appidEnt.Flags != COMAppIDFlags.None)
        {
            builder.AppendLine($"Flags: {appidEnt.Flags}");
        }

        return builder.ToString();
    }

    private static IEnumerable<TreeNode> LoadAppIDs(COMRegistry registry, bool filterIL, bool filterAC)
    {
        var clsidsByAppId = registry.ClsidsByAppId;
        var appids = registry.AppIDs;

        List<TreeNode> serverNodes = new();
        foreach (var pair in appids)
        {
            COMAppIDEntry appidEnt = appids[pair.Key];
                
            if (filterIL && COMSecurity.GetILForSD(appidEnt.AccessPermission) == TokenIntegrityLevel.Medium &&
                COMSecurity.GetILForSD(appidEnt.LaunchPermission) == TokenIntegrityLevel.Medium)
            {
                continue;
            }

            if (filterAC && !appidEnt.HasACAccess && !appidEnt.HasACLaunch)
            {
                continue;
            }

            TreeNode node = CreateNode(appidEnt.Name, FolderKey, appidEnt);
            node.ToolTipText = BuildAppIdTooltip(appidEnt);

            if (clsidsByAppId.ContainsKey(pair.Key))
            {
                node.Nodes.AddRange(clsidsByAppId[pair.Key].OrderBy(c => c.Name).Select(c => CreateClsidNode(registry, c)).ToArray());
            }

            serverNodes.Add(node);
        }


        return serverNodes.OrderBy(n => n.Text);
    }

    private static IEnumerable<TreeNode> LoadImplementedCategories(COMRegistry registry)
    {
        int i = 0;
        SortedDictionary<string, TreeNode> sortedNodes = new();

        foreach (var cat in registry.ImplementedCategories.Values)
        {
            TreeNode currNode = CreateNode(cat.Name, FolderKey, cat);
            currNode.ToolTipText = $"CATID: {cat.CategoryID.FormatGuid()}";
            sortedNodes.Add(currNode.Text, currNode);

            IEnumerable<COMCLSIDEntry> clsids = cat.ClassEntries.OrderBy(c => c.Name);
            IEnumerable<TreeNode> clsidNodes = clsids.Select(n => CreateClsidNode(registry, n));
            currNode.Nodes.AddRange(clsidNodes.ToArray());
        }

        TreeNode[] catNodes = new TreeNode[sortedNodes.Count];
        i = 0;
        foreach (KeyValuePair<string, TreeNode> pair in sortedNodes)
        {
            catNodes[i++] = pair.Value;
        }

        return catNodes;
    }

    private static IEnumerable<TreeNode> LoadPreApproved(COMRegistry registry)
    {
        List<TreeNode> nodes = new();
        foreach (COMCLSIDEntry ent in registry.PreApproved)
        {
            nodes.Add(CreateCLSIDNode(registry, ent));
        }

        return nodes;
    }

    private static IEnumerable<TreeNode> LoadIELowRights(COMRegistry registry)
    {
        List<TreeNode> clsidNodes = new();
        foreach (COMIELowRightsElevationPolicy ent in registry.LowRights)
        {
            StringBuilder tooltip = new();
            List<COMCLSIDEntry> clsids = new();
            COMCLSIDEntry entry = ent.ClassEntry;
            if (entry != null)
            {
                clsids.Add(entry);
            }

            if (!string.IsNullOrWhiteSpace(ent.AppPath) && registry.ClsidsByServer.ContainsKey(ent.AppPath))
            {
                clsids.AddRange(registry.ClsidsByServer[ent.AppPath]);
                tooltip.AppendLine($"{ent.AppPath}");
            }

            if (clsids.Count == 0)
            {
                continue;
            }

            TreeNode currNode = CreateNode(ent.Name, FolderKey, ent);
            clsidNodes.Add(currNode);

            foreach (COMCLSIDEntry cls in clsids)
            {
                currNode.Nodes.Add(CreateCLSIDNode(registry, cls));
            }

            tooltip.AppendLine($"Policy: {ent.Policy}");
            currNode.ToolTipText = tooltip.ToString();
        }

        return clsidNodes;
    }

    private static IEnumerable<TreeNode> LoadMimeTypes(COMRegistry registry)
    {
        List<TreeNode> nodes = new(registry.MimeTypes.Count());
        foreach (COMMimeType ent in registry.MimeTypes)
        {
            TreeNode node = CreateNode(ent.MimeType, FolderKey, ent);
            if (registry.Clsids.ContainsKey(ent.Clsid))
            {
                node.Nodes.Add(CreateCLSIDNode(registry, registry.Clsids[ent.Clsid]));
            }

            if (!string.IsNullOrWhiteSpace(ent.Extension))
            {
                node.ToolTipText = $"Extension {ent.Extension}";
            }
            nodes.Add(node);
        }

        return nodes;
    }

    private static TreeNode CreateTypelibVersionNode(COMTypeLibVersionEntry entry)
    {
        TreeNode node = CreateNode($"{entry.Name} : Version {entry.Version}", 
            ClassKey, entry);
        List<string> entries = new();
        if(!string.IsNullOrWhiteSpace(entry.Win32Path))
        {
            entries.Add($"Win32: {entry.Win32Path}");
        }
        if(!string.IsNullOrWhiteSpace(entry.Win64Path))
        {
            entries.Add($"Win64: {entry.Win64Path}");
        }
        node.ToolTipText = string.Join("\r\n", entries);

        return node;
    }

    private static IEnumerable<TreeNode> LoadTypeLibs(COMRegistry registry)
    {
        List<TreeNode> typeLibNodes = new();
        foreach (COMTypeLibEntry ent in registry.Typelibs.Values)
        {
            var root = CreateNode(ent.Name, FolderKey, ent);
            root.Nodes.AddRange(ent.Versions.Select(v => CreateTypelibVersionNode(v)).ToArray());
            typeLibNodes.Add(root);
        }
        return typeLibNodes.OrderBy(n => n.Text);
    }

    private void AddInterfaceNodes(TreeNode node, IEnumerable<COMInterfaceInstance> intfs)
    {
        node.Nodes.AddRange(intfs.Select(i => CreateInterfaceNameNode(m_registry, m_registry.MapIidToInterface(i.Iid), i)).OrderBy(n => n.Text).ToArray());
    }

    private async Task SetupCLSIDNodeTree(ICOMClassEntry clsid, TreeNode node, bool bRefresh)
    {
        node.Nodes.Clear();
        TreeNode wait_node = CreateNode("Please Wait, Populating Interfaces", InterfaceKey, null);
        node.Nodes.Add(wait_node);
        try
        {
            await clsid.LoadSupportedInterfacesAsync(bRefresh, null);
            int interface_count = clsid.Interfaces.Count();
            int factory_count = clsid.FactoryInterfaces.Count();
            if (interface_count == 0 && factory_count == 0)
            {
                wait_node.Text = "Error querying COM interfaces - Timeout";
            }
            else
            {
                treeComRegistry.SuspendLayout();
                if (interface_count > 0)
                {
                    node.Nodes.Remove(wait_node);
                    AddInterfaceNodes(node, clsid.Interfaces);
                }
                else
                {
                    wait_node.Text = "Error querying COM interfaces - No Instance Interfaces";
                }

                if (factory_count > 0)
                {
                    TreeNode factory = CreateNode("Factory Interfaces", FolderKey, null);
                    AddInterfaceNodes(factory, clsid.FactoryInterfaces);
                    node.Nodes.Add(factory);

                    if (clsid is COMCLSIDEntry clsid_entry && 
                        (clsid.FactoryInterfaces.Any(i => i.Iid == COMInterfaceEntry.IID_IPSFactoryBuffer)
                        || m_registry.ProxiesByClsid.ContainsKey(clsid_entry.Clsid)))
                    {
                        TreeNode proxy = CreateNode("Proxy", ProcessKey, new COMProxyFormatter(clsid_entry));
                        node.Nodes.Add(proxy);
                    }
                }

                var typelib = m_registry.MapClsidToEntry(clsid.Clsid)?.TypeLibEntry;
                if (typelib != null)
                {
                    TreeNode typelib_node = CreateNode("Typelib", ProcessKey, typelib);
                    typelib_node.Nodes.AddRange(typelib.Versions.Select(v => CreateTypelibVersionNode(v)).ToArray());
                    node.Nodes.Add(typelib_node);
                }

                treeComRegistry.ResumeLayout();
            }
        }
        catch (Win32Exception ex)
        {
            wait_node.Text = $"Error querying COM interfaces - {ex.Message}";
        }
    }

    private void AddTypeLibNodes(TreeNode node, IEnumerable<COMTypeLibTypeInfo> types, string category, string image_key)
    {
        if (types.Any())
        {
            var sub_node = CreateNode(category, FolderKey, types);
            node.Nodes.Add(sub_node);
            sub_node.Nodes.AddRange(types.Select(type => CreateNode(type.Name, image_key, type)).ToArray());
        }
    }

    private async Task SetupTypeLibNodeTree(COMTypeLibVersionEntry typelib, TreeNode node)
    {
        treeComRegistry.SuspendLayout();
        node.Nodes.Clear();
        TreeNode wait_node = CreateNode("Please Wait, Parsing type library", FolderKey, null);
        node.Nodes.Add(wait_node);
        treeComRegistry.ResumeLayout();
        try
        {
            var parsed_typelib = await Task.Run(() => typelib.Parse());
            treeComRegistry.SuspendLayout();
            node.Nodes.Remove(wait_node);
            AddTypeLibNodes(node, parsed_typelib.Interfaces, "Interfaces", InterfaceKey);
            AddTypeLibNodes(node, parsed_typelib.Dispatch, "Dispatch Interfaces", InterfaceKey);
            AddTypeLibNodes(node, parsed_typelib.Classes, "Classes", ClassKey);
            AddTypeLibNodes(node, parsed_typelib.Records, "Records", ClassKey);
            AddTypeLibNodes(node, parsed_typelib.Unions, "Unions", ClassKey);
            AddTypeLibNodes(node, parsed_typelib.Enums, "Enums", ClassKey);
            AddTypeLibNodes(node, parsed_typelib.Modules, "Modules", ProcessKey);
            AddTypeLibNodes(node, parsed_typelib.Aliases, "Aliases", ClassKey);
            treeComRegistry.ResumeLayout();
            sourceCodeViewerControl.SelectedObject = typelib;
        }
        catch (Exception ex)
        {
            wait_node.Text = $"Error parsing type library - {ex.Message}";
        }
    }

    private void AddProxyNodes(TreeNode node, IEnumerable<COMProxyTypeInfo> types, string category, string image_key)
    {
        if (types.Any())
        {
            var sub_node = CreateNode(category, FolderKey, types);
            node.Nodes.Add(sub_node);
            sub_node.Nodes.AddRange(types.Select(type => CreateNode(type.Name, image_key, type)).ToArray());
        }
    }

    private async Task SetupCOMProxyNodeTree(COMProxyFormatter proxy, TreeNode node)
    {
        treeComRegistry.SuspendLayout();
        node.Nodes.Clear();
        TreeNode wait_node = CreateNode("Please Wait, Parsing proxy file.", FolderKey, null);
        node.Nodes.Add(wait_node);
        treeComRegistry.ResumeLayout();
        try
        {
            await Task.Run(() => proxy.ParseSourceCode());
            treeComRegistry.SuspendLayout();
            node.Nodes.Remove(wait_node);
            AddProxyNodes(node, proxy.ProxyFile.Entries, "Interfaces", InterfaceKey);
            AddProxyNodes(node, proxy.ProxyFile.ComplexTypes.Where(t => !t.IsUnion), "Structs", ClassKey);
            AddProxyNodes(node, proxy.ProxyFile.ComplexTypes.Where(t => t.IsUnion), "Unions", ClassKey);
            treeComRegistry.ResumeLayout();
            sourceCodeViewerControl.SelectedObject = proxy;
        }
        catch (Exception ex)
        {
            wait_node.Text = $"Error parsing type library - {ex.Message}";
        }
    }

    private async void treeComRegistry_BeforeExpand(object sender, TreeViewCancelEventArgs e)
    {
        Cursor currCursor = Cursor.Current;
        Cursor.Current = Cursors.WaitCursor;

        if (e.Node is DynamicTreeNode node && !node.IsGenerated)
        {
            node.IsGenerated = true;
            if (e.Node.Tag is ICOMClassEntry class_entry)
            {
                await SetupCLSIDNodeTree(class_entry, e.Node, false);
            }
            else if (e.Node.Tag is COMTypeLibVersionEntry typelib_entry)
            {
                await SetupTypeLibNodeTree(typelib_entry, e.Node);
            }
            else if (e.Node.Tag is COMTypeLibCoClass typelib_class)
            {
                await SetupCLSIDNodeTree(m_registry.MapClsidToEntry(typelib_class.Uuid), e.Node, false);
            }
            else if (e.Node.Tag is COMProxyFormatter proxy)
            {
                await SetupCOMProxyNodeTree(proxy, e.Node);
            }
        }

        Cursor.Current = currCursor;
    }

    private static bool CanGetGuid(TreeNode node)
    {
        object tag = node?.Tag;
        return tag is ICOMGuid || tag is Guid;
    }

    private static Guid GetGuidFromType(TreeNode node)
    {
        object tag = node?.Tag;
        if (tag is ICOMGuid com_guid)
        {
            return com_guid.ComGuid;
        }

        if (tag is Guid guid)
        {
            return guid;
        }

        return Guid.Empty;
    }

    private void copyGUIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Guid guid = GetGuidFromType(treeComRegistry.SelectedNode);
        if (guid != Guid.Empty)
        {
            COMUtilities.CopyGuidToClipboard(guid, GuidFormat.String);
        }
    }

    private void copyGUIDCStructureToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Guid guid = GetGuidFromType(treeComRegistry.SelectedNode);

        if (guid != Guid.Empty)
        {
            COMUtilities.CopyGuidToClipboard(guid, GuidFormat.Structure);
        }
    }

    private void copyGUIDHexStringToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Guid guid = GetGuidFromType(treeComRegistry.SelectedNode);

        if (guid != Guid.Empty)
        {
            COMUtilities.CopyGuidToClipboard(guid, GuidFormat.HexString);
        }
    }

    private void copyObjectTagToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        Guid guid = Guid.Empty;

        if (node?.Tag is COMCLSIDEntry clsid)
        {
            guid = clsid.Clsid;
        }
        else if (node?.Tag is COMProgIDEntry prog_id)
        {
            guid = prog_id.Clsid;
        }

        if (guid != Guid.Empty)
        {
            COMUtilities.CopyGuidToClipboard(guid, GuidFormat.Object);
        }
    }

    private async Task SetupObjectView(ICOMClassEntry ent, object obj, bool factory)
    {
        await EntryPoint.GetMainForm(m_registry).HostObject(ent, obj, factory);
    }

    private ICOMClassEntry GetSelectedClassEntry()
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node?.Tag is ICOMClassEntry clsid)
        {
            return clsid;
        }
        else if (node?.Tag is COMProgIDEntry prog_id)
        {
            return m_registry.MapClsidToEntry(prog_id.Clsid);
        }
        else if (node?.Tag is COMTypeLibCoClass typelib_class)
        {
            return m_registry.MapClsidToEntry(typelib_class.Uuid);
        }
        return null;
    }

    private async Task CreateInstance(CLSCTX clsctx, string server)
    {
        ICOMClassEntry ent = GetSelectedClassEntry();
        if (ent == null)
            return;
        try
        {
            object comObj = ent.CreateInstanceAsObject(clsctx, server);
            if (comObj != null)
            {
                await SetupObjectView(ent, comObj, false);
            }
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private async Task CreateClassFactory(string server)
    {
        ICOMClassEntry ent = GetSelectedClassEntry();
        if (ent == null)
            return;
        try
        {
            object comObj = ent.CreateClassFactory(CLSCTX.ALL, server);
            if (comObj != null)
            {
                await SetupObjectView(ent, comObj, true);
            }
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private async void createInstanceToolStripMenuItem_Click(object sender, EventArgs e)
    {
        await CreateInstance(CLSCTX.ALL, null);
    }

    private void EnableViewPermissions(COMAppIDEntry appid)
    {
        if (appid.HasAccessPermission)
        {
            contextMenuStrip.Items.Add(viewAccessPermissionsToolStripMenuItem);
        }
        if (appid.HasLaunchPermission)
        {
            contextMenuStrip.Items.Add(viewLaunchPermissionsToolStripMenuItem);
        }
    }

    private void SetupCreateSpecialSessions()
    {
        createInSessionToolStripMenuItem.DropDownItems.Clear();
        createInSessionToolStripMenuItem.DropDownItems.Add(consoleToolStripMenuItem);
        foreach (int session_id in COMSecurity.GetSessionIds())
        {
            ToolStripMenuItem item = new(session_id.ToString());
            item.Tag = session_id.ToString();
            item.Click += consoleToolStripMenuItem_Click;
            createInSessionToolStripMenuItem.DropDownItems.Add(item);
        }
        createSpecialToolStripMenuItem.DropDownItems.Add(createInSessionToolStripMenuItem);
    }

    private static bool HasServerType(COMCLSIDEntry clsid, COMServerType type)
    {
        if (clsid == null)
        {
            return false;
        }

        if (clsid.DefaultServerType == COMServerType.UnknownServer)
        {
            // If we have no servers we assume anything is possible.
            return true;
        }

        return clsid.Servers.ContainsKey(type);
    }

    private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;

        if ((node != null) && (node.Tag != null))
        {
            contextMenuStrip.Items.Clear();
            contextMenuStrip.Items.Add(copyToolStripMenuItem);
            if (CanGetGuid(node))
            {
                contextMenuStrip.Items.Add(copyGUIDToolStripMenuItem);
                contextMenuStrip.Items.Add(copyGUIDHexStringToolStripMenuItem);
                contextMenuStrip.Items.Add(copyGUIDCStructureToolStripMenuItem);
            }

            if ((node.Tag is ICOMClassEntry) || (node.Tag is COMProgIDEntry) || (node.Tag is COMTypeLibCoClass))
            {
                contextMenuStrip.Items.Add(copyObjectTagToolStripMenuItem);
                contextMenuStrip.Items.Add(createInstanceToolStripMenuItem);

                COMCLSIDEntry clsid = node.Tag as COMCLSIDEntry;
                COMRuntimeClassEntry runtime_class = node.Tag as COMRuntimeClassEntry;
                ICOMClassEntry entry = node.Tag as ICOMClassEntry;
                if (node.Tag is COMProgIDEntry progid)
                {
                    clsid = m_registry.MapClsidToEntry(progid.Clsid);
                    entry = clsid;
                }
                else if (node.Tag is COMTypeLibCoClass typelib_class)
                {
                    clsid = m_registry.MapClsidToEntry(typelib_class.Uuid);
                    entry = clsid;
                }

                createSpecialToolStripMenuItem.DropDownItems.Clear();

                if (HasServerType(clsid, COMServerType.InProcServer32))
                {
                    createSpecialToolStripMenuItem.DropDownItems.Add(createInProcServerToolStripMenuItem);
                }

                if (HasServerType(clsid, COMServerType.InProcHandler32))
                {
                    createSpecialToolStripMenuItem.DropDownItems.Add(createInProcHandlerToolStripMenuItem);
                }

                if (HasServerType(clsid, COMServerType.LocalServer32))
                {
                    createSpecialToolStripMenuItem.DropDownItems.Add(createLocalServerToolStripMenuItem);
                    SetupCreateSpecialSessions();
                    if (clsid.CanElevate)
                    {
                        createSpecialToolStripMenuItem.DropDownItems.Add(createElevatedToolStripMenuItem);
                    }
                    createSpecialToolStripMenuItem.DropDownItems.Add(createRemoteToolStripMenuItem);
                }

                createSpecialToolStripMenuItem.DropDownItems.Add(createClassFactoryToolStripMenuItem);
                if (entry != null && entry.SupportsRemoteActivation)
                {
                    createSpecialToolStripMenuItem.DropDownItems.Add(createClassFactoryRemoteToolStripMenuItem);
                }

                if (runtime_class != null && runtime_class.TrustLevel == TrustLevel.PartialTrust)
                {
                    createSpecialToolStripMenuItem.DropDownItems.Add(createInRuntimeBrokerToolStripMenuItem);
                    createSpecialToolStripMenuItem.DropDownItems.Add(createInPerUserRuntimeBrokerToolStripMenuItem);
                    createSpecialToolStripMenuItem.DropDownItems.Add(createFactoryInRuntimeBrokerToolStripMenuItem);
                    createSpecialToolStripMenuItem.DropDownItems.Add(createFactoryInPerUserRuntimeBrokerToolStripMenuItem);
                }

                contextMenuStrip.Items.Add(createSpecialToolStripMenuItem);
                contextMenuStrip.Items.Add(refreshInterfacesToolStripMenuItem);

                if (clsid != null)
                {
                    if (m_registry.Typelibs.ContainsKey(clsid.TypeLib))
                    {
                        contextMenuStrip.Items.Add(viewTypeLibraryToolStripMenuItem);
                    }

                    if (!clsid.IsAutomationProxy && m_registry.GetProxiesForClsid(clsid).Length > 0)
                    {
                        contextMenuStrip.Items.Add(viewProxyLibraryToolStripMenuItem);
                    }

                    if (m_registry.AppIDs.ContainsKey(clsid.AppID))
                    {
                        EnableViewPermissions(m_registry.AppIDs[clsid.AppID]);
                    }
                }

                if (runtime_class != null)
                {
                    COMRuntimeServerEntry server = 
                        runtime_class.HasServer 
                            ? m_registry.MapServerNameToEntry(runtime_class.Server) : null;
                    if (runtime_class.HasPermission || (server != null && server.HasPermission))
                    {
                        contextMenuStrip.Items.Add(viewAccessPermissionsToolStripMenuItem);
                    }
                }
            }
            else if (node.Tag is COMTypeLibVersionEntry)
            {
                contextMenuStrip.Items.Add(viewTypeLibraryToolStripMenuItem);
            }
            else if (node.Tag is COMAppIDEntry appid_entry)
            {
                EnableViewPermissions(appid_entry);
            }
            else if (node.Tag is COMInterfaceEntry intf)
            {
                if (intf.HasTypeLib)
                {
                    contextMenuStrip.Items.Add(viewTypeLibraryToolStripMenuItem);
                }

                if (intf.HasProxy && intf.ProxyClassEntry?.IsAutomationProxy == false)
                {
                    contextMenuStrip.Items.Add(viewProxyLibraryToolStripMenuItem);
                }

                if (COMUtilities.RuntimeInterfaceMetadata.ContainsKey(intf.Iid))
                {
                    contextMenuStrip.Items.Add(viewRuntimeInterfaceToolStripMenuItem);
                }
            }
            else if (node.Tag is COMProcessEntry)
            {
                contextMenuStrip.Items.Add(refreshProcessToolStripMenuItem);
                contextMenuStrip.Items.Add(viewAccessPermissionsToolStripMenuItem);
            }
            else if (node.Tag is COMIPIDEntry ipid)
            {
                intf = m_registry.MapIidToInterface(ipid.Iid);

                if (intf.HasTypeLib)
                {
                    contextMenuStrip.Items.Add(viewTypeLibraryToolStripMenuItem);
                }

                if (intf.HasProxy && m_registry.Clsids.ContainsKey(intf.ProxyClsid))
                {
                    contextMenuStrip.Items.Add(viewProxyLibraryToolStripMenuItem);
                }

                contextMenuStrip.Items.Add(unmarshalToolStripMenuItem);
            }
            else if (node.Tag is COMRuntimeClassEntry runtime_class)
            {
                if (runtime_class.HasPermission)
                {
                    contextMenuStrip.Items.Add(viewAccessPermissionsToolStripMenuItem);
                }
            }
            else if (node.Tag is COMRuntimeServerEntry runtime_server)
            {
                if (runtime_server.HasPermission)
                {
                    contextMenuStrip.Items.Add(viewAccessPermissionsToolStripMenuItem);
                }
            }

            if (m_filter_types.Contains(FilterType.CLSID))
            {
                contextMenuStrip.Items.Add(queryAllInterfacesToolStripMenuItem);
            }

            if (treeComRegistry.Nodes.Count > 0)
            {
                contextMenuStrip.Items.Add(cloneTreeToolStripMenuItem);
                selectedToolStripMenuItem.Enabled = treeComRegistry.SelectedNode != null;
            }

            if (PropertiesControl.SupportsProperties(node.Tag))
            {
                contextMenuStrip.Items.Add(propertiesToolStripMenuItem);
            }

            contextMenuStrip.Items.Add(showSourceCodeToolStripMenuItem);
        }
        else
        {
            e.Cancel = true;
        }
    }

    private async void refreshInterfacesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node != null)
        {
            ICOMClassEntry class_entry = node.Tag as ICOMClassEntry;

            if (node.Tag is COMTypeLibCoClass typelib_class)
            {
                class_entry = m_registry.MapClsidToEntry(typelib_class.Uuid);
            }

            if (class_entry != null)
            {
                await SetupCLSIDNodeTree(class_entry, node, true);
            }
        }
    }

    /// <summary>
    /// Convert a basic Glob to a regular expression
    /// </summary>
    /// <param name="glob">The glob string</param>
    /// <param name="ignoreCase">Indicates that match should ignore case</param>
    /// <returns>The regular expression</returns>
    private static Regex GlobToRegex(string glob, bool ignoreCase)
    {
        StringBuilder builder = new();

        builder.Append("^");

        foreach (char ch in glob)
        {
            if (ch == '*')
            {
                builder.Append(".*");
            }
            else if (ch == '?')
            {
                builder.Append(".");
            }
            else
            {
                builder.Append(Regex.Escape(new string(ch, 1)));
            }
        }

        builder.Append("$");

        return new Regex(builder.ToString(), ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
    }

    private FilterResult RunComplexFilter(TreeNode node, RegistryViewerFilter filter)
    {
        try
        {
            FilterResult result = filter.Filter(node.Tag);
            if (result == FilterResult.None && node.Tag is COMCLSIDEntry clsid && clsid.InterfacesLoaded)
            {
                foreach (COMInterfaceEntry intf in clsid.Interfaces.Concat(clsid.FactoryInterfaces).Select(i => m_registry.MapIidToInterface(i.Iid)))
                {
                    result = filter.Filter(intf);
                    if (result != FilterResult.None)
                    {
                        break;
                    }
                }
            }
            return result;
        }
        catch
        {
            return FilterResult.None;
        }
    }

    private FilterResult RunAccessibleFilter(TreeNode node, COMAccessCheck access_check)
    {
        if (node.Tag is not ICOMAccessSecurity obj)
        {
            return FilterResult.Exclude;
        }

        return access_check.AccessCheck(obj) ? FilterResult.Include : FilterResult.Exclude;
    }

    private enum FilterMode
    {
        Contains,
        BeginsWith,
        EndsWith,
        Equals,
        Glob,
        Regex,
        Accessible,
        NotAccessible,
        Complex,
    }

    private static Func<TreeNode, bool> CreateFilter(string filter, FilterMode mode, bool caseSensitive)
    {
        StringComparison comp;

        filter = filter.Trim();
        if (string.IsNullOrEmpty(filter))
        {
            return null;
        }

        if(caseSensitive)
        {
            comp = StringComparison.CurrentCulture;
        }
        else
        {
            comp = StringComparison.CurrentCultureIgnoreCase;
        }

        switch (mode)
        {
            case FilterMode.Contains:
                if (caseSensitive)
                {
                    return n => n.Text.Contains(filter);
                }
                else
                {
                    filter = filter.ToLower();
                    return n => n.Text.ToLower().Contains(filter.ToLower());
                }
            case FilterMode.BeginsWith:
                return n => n.Text.StartsWith(filter, comp);
            case FilterMode.EndsWith:
                return n => n.Text.EndsWith(filter, comp);
            case FilterMode.Equals:
                return n => n.Text.Equals(filter, comp);
            case FilterMode.Glob:
                {
                    Regex r = GlobToRegex(filter, caseSensitive);

                    return n => r.IsMatch(n.Text);
                }
            case FilterMode.Regex:
                {
                    Regex r = new(filter, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);

                    return n => r.IsMatch(n.Text);
                }
            default:
                throw new ArgumentException("Invalid mode value");
        }
    }

    // Check if top node or one of its subnodes matches the filter
    private static FilterResult FilterNode(TreeNode n, Func<TreeNode, FilterResult> filterFunc)
    {
        FilterResult result = filterFunc(n);

        if (result == FilterResult.None)
        {
            foreach (TreeNode node in n.Nodes)
            {
                result = FilterNode(node, filterFunc);
                if (result == FilterResult.Include)
                {
                    break;
                }
            }
        }

        return result;
    }

    private async void btnApply_Click(object sender, EventArgs e)
    {
        NtToken token = null;
        try
        {
            TreeNode[] nodes = null;
            Func<TreeNode, FilterResult> filterFunc = null;
            IDisposable filter_object = null;
            FilterMode mode = (FilterMode)comboBoxMode.SelectedItem;

            if (mode == FilterMode.Complex)
            {
                using ViewFilterForm form = new(m_filter, m_filter_types);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    m_filter = form.Filter;
                    if (m_filter.Filters.Count > 0)
                    {
                        filterFunc = n => RunComplexFilter(n, m_filter);
                    }
                }
                else
                {
                    return;
                }
            }
            else if (mode == FilterMode.Accessible || mode == FilterMode.NotAccessible)
            {
                using SelectSecurityCheckForm form = new(m_mode == DisplayMode.Processes);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    COMAccessCheck access_check = new(form.Token, form.Principal, form.AccessRights, form.LaunchRights, form.IgnoreDefault);
                    filter_object = access_check;
                    filterFunc = n => RunAccessibleFilter(n, access_check);
                    if (mode == FilterMode.NotAccessible)
                    {
                        Func<TreeNode, FilterResult> last_filter = filterFunc;
                        filterFunc = n => last_filter(n) == FilterResult.Exclude ? FilterResult.Include : FilterResult.Exclude;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                Func<TreeNode, bool> filter = CreateFilter(textBoxFilter.Text, mode, false);
                if (filter != null)
                {
                    filterFunc = n => filter(n) ? FilterResult.Include : FilterResult.None;
                }
            }

            if (filterFunc != null)
            {
                using (filter_object)
                {
                    nodes = await Task.Run(() => m_originalNodes.Where(n => FilterNode(n, filterFunc) == FilterResult.Include).ToArray());
                }
            }
            else
            {
                nodes = m_originalNodes;
            }

            treeComRegistry.SuspendLayout();
            treeComRegistry.Nodes.Clear();
            treeComRegistry.Nodes.AddRange(nodes);
            treeComRegistry.ResumeLayout();
            UpdateStatusLabel();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            token?.Close();
        }
    }

    private void textBoxFilter_KeyDown(object sender, KeyEventArgs e)
    {
        if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
        {
            btnApply.PerformClick();
        }
    }

    private void treeComRegistry_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            TreeNode node = treeComRegistry.GetNodeAt(e.X, e.Y);

            if (node != null)
            {
                treeComRegistry.SelectedNode = node;
            }
        }
    }

    private void viewTypeLibraryToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node == null)
            return;

        COMTypeLibVersionEntry ent = node.Tag as COMTypeLibVersionEntry;
        Guid selected_guid = Guid.Empty;

        if (ent == null)
        {
            COMCLSIDEntry clsid = node.Tag as COMCLSIDEntry;
            if (node.Tag is COMProgIDEntry progid)
            {
                clsid = m_registry.MapClsidToEntry(progid.Clsid);
            }

            if (clsid != null && m_registry.Typelibs.ContainsKey(clsid.TypeLib))
            {
                ent = m_registry.Typelibs[clsid.TypeLib].Versions.First();
                selected_guid = clsid.Clsid;
            }

            if (node.Tag is COMInterfaceEntry intf && m_registry.Typelibs.ContainsKey(intf.TypeLib))
            {
                ent = m_registry.GetTypeLibVersionEntry(intf.TypeLib, intf.TypeLibVersion);
                selected_guid = intf.Iid;
            }
        }
            
        if(ent != null)
        {
            try
            {
                EntryPoint.GetMainForm(m_registry).HostControl(
                    new TypeLibControl(m_registry, ent.Name,
                    COMTypeLib.FromFile(ent.NativePath), selected_guid));
            }
            catch (Exception ex)
            {
                EntryPoint.ShowError(this, ex);
            }
        }
    }

    private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node != null)
        {
            EntryPoint.GetMainForm(m_registry).HostControl(new PropertiesControl(m_registry, node.Text, node.Tag));
        }
    }

    private void ViewPermissions(bool access)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node != null)
        {
            if (node.Tag is COMProcessEntry proc)
            {
                COMSecurity.ViewSecurity(m_registry, $"{proc.Name} Access", proc.AccessPermissions, true);
            }
            else if (node.Tag is COMRuntimeClassEntry || node.Tag is COMRuntimeServerEntry)
            {
                COMRuntimeServerEntry runtime_server = node.Tag as COMRuntimeServerEntry;
                COMRuntimeClassEntry runtime_class = node.Tag as COMRuntimeClassEntry;
                string name = runtime_class != null ? runtime_class.Name : runtime_server.Name;
                if (runtime_class != null && runtime_class.HasServer)
                {
                    runtime_server = m_registry.MapServerNameToEntry(runtime_class.Server);
                }

                COMSecurityDescriptor perms = runtime_server?.Permissions ?? runtime_class.Permissions;

                COMSecurity.ViewSecurity(m_registry, $"{name} Access", perms, false);
            }
            else
            {
                COMAppIDEntry appid = node.Tag as COMAppIDEntry;
                if (appid == null)
                {
                    COMCLSIDEntry clsid = node.Tag as COMCLSIDEntry;
                    if (clsid == null && node.Tag is COMProgIDEntry prog_id)
                    {
                        clsid = m_registry.MapClsidToEntry(prog_id.Clsid);
                    }

                    if (clsid != null && m_registry.AppIDs.ContainsKey(clsid.AppID))
                    {
                        appid = m_registry.AppIDs[clsid.AppID];
                    }
                }

                if (appid != null)
                {
                    COMSecurity.ViewSecurity(m_registry, appid, access);
                }
            }
        }
    }

    private void viewLaunchPermissionsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ViewPermissions(false);
    }

    private void viewAccessPermissionsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ViewPermissions(true);
    }

    private async void createLocalServerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        await CreateInstance(CLSCTX.LOCAL_SERVER, null);
    }

    private async void createInProcServerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        await CreateInstance(CLSCTX.INPROC_SERVER, null);
    }

    private async Task CreateFromMoniker(COMCLSIDEntry ent, string moniker)
    {
        try
        {
            object obj = COMUtilities.CreateFromMoniker(moniker, CLSCTX.LOCAL_SERVER);
            await SetupObjectView(ent, obj, obj is IClassFactory);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CreateInSession(COMCLSIDEntry ent, string session_id)
    {
        await CreateFromMoniker(ent, $"session:{session_id}!new:{ent.Clsid}");
    }

    private async Task CreateElevated(COMCLSIDEntry ent, bool factory)
    {
        await CreateFromMoniker(ent, $"Elevation:Administrator!{(factory ? "clsid" : "new")}:{ent.Clsid}");
    }

    private async void consoleToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (GetSelectedClassEntry() is COMCLSIDEntry ent && sender is ToolStripMenuItem item && item.Tag is string)
        {
            await CreateInSession(ent, (string)item.Tag);
        }
    }

    private void copyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node != null)
        {
            COMUtilities.CopyTextToClipboard(node.Text);
        }
    }

    private void viewProxyLibraryToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node != null)
        {
            COMCLSIDEntry clsid = node.Tag as COMCLSIDEntry;
            Guid selected_iid = Guid.Empty;
            if (clsid == null && (node.Tag is COMInterfaceEntry || node.Tag is COMIPIDEntry))
            {
                COMInterfaceEntry intf = node.Tag as COMInterfaceEntry;
                intf ??= m_registry.MapIidToInterface(((COMIPIDEntry)node.Tag).Iid);

                selected_iid = intf.Iid;
                clsid = m_registry.Clsids[intf.ProxyClsid];
            }

            if (clsid != null)
            {
                try
                {
                    string comClassIdName = null;
                    Guid? comClassId = null;
                    if(Text == "Local Services")
                    {
                        comClassIdName = node.Parent.Text;
                        COMCLSIDEntry comClassClsId = node.Parent.Tag as COMCLSIDEntry;
                        comClassId = comClassClsId?.Clsid;
                    }

                    using var resolver = EntryPoint.GetProxyParserSymbolResolver();
                    EntryPoint.GetMainForm(m_registry).HostControl(
                        new TypeLibControl(
                            m_registry,
                            COMUtilities.GetFileName(clsid.DefaultServer),
                            COMProxyFile.GetFromCLSID(clsid, resolver),
                            selected_iid,
                            comClassIdName,
                            comClassId
                        )
                    );
                }
                catch (Exception ex)
                {
                    EntryPoint.ShowError(this, ex);
                }
            }
        }
    }

    private async void createClassFactoryToolStripMenuItem_Click(object sender, EventArgs e)
    {
        await CreateClassFactory(null);
    }

    private void GetClsidsFromNodes(List<COMCLSIDEntry> clsids, TreeNodeCollection nodes)
    {
        foreach (TreeNode node in nodes)
        {
            if (node.Tag is COMCLSIDEntry clsid)
            {
                clsids.Add(clsid);
            }

            if (node.Nodes.Count > 0)
            {
                GetClsidsFromNodes(clsids, node.Nodes);
            }
        }
    }

    private void queryAllInterfacesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using QueryInterfacesOptionsForm options = new();
        if (options.ShowDialog(this) == DialogResult.OK)
        {
            List<COMCLSIDEntry> clsids = new();
            GetClsidsFromNodes(clsids, treeComRegistry.Nodes);
            if (clsids.Count > 0)
            {
                COMUtilities.QueryAllInterfaces(this, clsids,
                    options.ServerTypes, options.ConcurrentQueries,
                    options.RefreshInterfaces);
            }
        }
    }

    private async void createInProcHandlerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        await CreateInstance(CLSCTX.INPROC_HANDLER, null);
    }

    private async void instanceToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (GetSelectedClassEntry() is COMCLSIDEntry clsid)
        {
            await CreateElevated(clsid, false);
        }
    }

    private async void classFactoryToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (GetSelectedClassEntry() is COMCLSIDEntry clsid)
        {
            await CreateElevated(clsid, true);
        }
    }

    private void comboBoxMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboBoxMode.SelectedItem != null)
        {
            FilterMode mode = (FilterMode)comboBoxMode.SelectedItem;
            textBoxFilter.Enabled = mode != FilterMode.Complex && mode != FilterMode.Accessible && mode != FilterMode.NotAccessible;
        }
    }

    private async void createRemoteToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using GetTextForm frm = new("localhost");
        frm.Text = "Enter Remote Host";
        if (frm.ShowDialog(this) == DialogResult.OK)
        {
            await CreateInstance(CLSCTX.REMOTE_SERVER, frm.Data);
        }
    }

    private async void createClassFactoryRemoteToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using GetTextForm frm = new("localhost");
        frm.Text = "Enter Remote Host";
        if (frm.ShowDialog(this) == DialogResult.OK)
        {
            await CreateClassFactory(frm.Data);
        }
    }

    private void treeComRegistry_AfterExpand(object sender, TreeViewEventArgs e)
    {
        TreeNode node = e.Node;
        if (node.ImageKey == FolderKey)
        {
            node.ImageKey = FolderOpenKey;
            node.SelectedImageKey = FolderOpenKey;
        }
    }

    private void treeComRegistry_AfterCollapse(object sender, TreeViewEventArgs e)
    {
        TreeNode node = e.Node;
        if (node.ImageKey == FolderOpenKey)
        {
            node.ImageKey = FolderKey;
            node.SelectedImageKey = FolderKey;
        }
    }

    private void refreshProcessToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node != null && node.Tag is COMProcessEntry)
        {
            COMProcessEntry process = (COMProcessEntry)node.Tag;
            process = COMProcessParser.ParseProcess(process.ProcessId, COMUtilities.GetProcessParserConfig(), m_registry);
            if (process == null)
            {
                treeComRegistry.Nodes.Remove(treeComRegistry.SelectedNode);
                m_originalNodes = m_originalNodes.Where(n => n != node).ToArray();
            }
            else
            {
                node.Tag = process;
                node.Nodes.Clear();
                PopulatorIpids(m_registry, node, process);
            }
        }
    }

    private COMIPIDEntry GetSelectedIpid()
    {
        if (treeComRegistry.SelectedNode != null)
        {
            return treeComRegistry.SelectedNode.Tag as COMIPIDEntry;
        }
        return null;
    }

    private void toHexEditorToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid != null)
        {
            EntryPoint.GetMainForm(m_registry).HostControl(new ObjectHexEditor(m_registry, 
                ipid.Ipid.ToString(),
                ipid.ToObjref()));
        }
    }

    private void toFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid != null)
        {
            using SaveFileDialog dlg = new();
            dlg.Filter = "All Files (*.*)|*.*";
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    File.WriteAllBytes(dlg.FileName, ipid.ToObjref());
                }
                catch (Exception ex)
                {
                    EntryPoint.ShowError(this, ex);
                }
            }
        }
    }

    private async void toObjectToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid != null)
        {
            try
            {
                await EntryPoint.GetMainForm(m_registry).OpenObjectInformation(
                    COMUtilities.UnmarshalObject(ipid.ToObjref()),
                    $"IPID {ipid.Ipid}");
            }
            catch (Exception ex)
            {
                EntryPoint.ShowError(this, ex);
            }
        }
    }

    private void CreateClonedTree(IEnumerable<TreeNode> nodes)
    {
        string text = Text;
        if (!text.StartsWith("Clone of "))
        {
            text = "Clone of " + text;
        }
        COMRegistryViewer viewer = new(m_registry, m_mode, m_processes, nodes.Select(n => (TreeNode)n.Clone()), m_filter_types, text);
        EntryPoint.GetMainForm(m_registry).HostControl(viewer);
    }

    private void allVisibleToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CreateClonedTree(treeComRegistry.Nodes.OfType<TreeNode>());
    }

    private void selectedToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode selected = treeComRegistry.SelectedNode;
        if (selected != null)
        {
            CreateClonedTree(new TreeNode[] { selected });
        }
    }

    private async void filteredToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using ViewFilterForm form = new(new RegistryViewerFilter(), m_filter_types);
        if (form.ShowDialog(this) == DialogResult.OK && form.Filter.Filters.Count > 0)
        {
            IEnumerable<TreeNode> nodes =
                await Task.Run(() => m_originalNodes.Where(n =>
                FilterNode(n, x => RunComplexFilter(x, form.Filter)) == FilterResult.Include).ToArray());
            CreateClonedTree(nodes);
        }
    }

    private void treeComRegistry_AfterSelect(object sender, TreeViewEventArgs e)
    {
        var main_form = EntryPoint.GetMainForm(m_registry);
        object obj = treeComRegistry.SelectedNode?.Tag;
        main_form.UpdatePropertyGrid(obj);
        sourceCodeViewerControl.SelectedObject = obj;
    }

    private void allChildrenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode selected = treeComRegistry.SelectedNode;
        if (selected != null && selected.Nodes.Count > 0)
        {
            CreateClonedTree(selected.Nodes.OfType<TreeNode>());
        }
    }

    private void viewRuntimeInterfaceToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node != null)
        {
            if (node.Tag is COMInterfaceEntry ent && COMUtilities.RuntimeInterfaceMetadata.ContainsKey(ent.Iid))
            {
                Assembly asm = COMUtilities.RuntimeInterfaceMetadata[ent.Iid].Assembly;
                EntryPoint.GetMainForm(m_registry).HostControl(new TypeLibControl(asm.GetName().Name,
                    COMUtilities.RuntimeInterfaceMetadata[ent.Iid].Assembly, ent.Iid, false));
            }
        }
    }

    [Guid("D63B10C5-BB46-4990-A94F-E40B9D520160")]
    [ComImport]
    private class RuntimeBroker
    {
    }

    [Guid("2593F8B9-4EAF-457C-B68A-50F6B8EA6B54")]
    [ComImport]
    private class PerUserRuntimeBroker
    {
    }

    private IRuntimeBroker CreateBroker(bool per_user)
    {
        if (per_user)
        {
            return (IRuntimeBroker)new PerUserRuntimeBroker();
        }
        else
        {
            return (IRuntimeBroker)new RuntimeBroker();
        }
    }

    private async void CreateInRuntimeBroker(bool per_user, bool factory)
    {
        try
        {
            if (GetSelectedClassEntry() is COMRuntimeClassEntry runtime_class)
            {
                IRuntimeBroker broker = CreateBroker(per_user);
                object comObj;
                if (factory)
                {
                    comObj = broker.GetActivationFactory(runtime_class.Name, COMInterfaceEntry.IID_IUnknown);
                }
                else
                {
                    comObj = broker.ActivateInstance(runtime_class.Name);
                }

                await SetupObjectView(runtime_class, comObj, factory);
            }
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private void createInRuntimeBrokerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CreateInRuntimeBroker(false, false);
    }

    private void createInPerUserRuntimeBrokerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CreateInRuntimeBroker(true, false);
    }

    private void createFactoryInRuntimeBrokerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CreateInRuntimeBroker(false, true);
    }

    private void createFactoryInPerUserRuntimeBrokerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CreateInRuntimeBroker(false, true);
    }

    private void showSourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        splitContainer.Panel2Collapsed = !splitContainer.Panel2Collapsed;
        showSourceCodeToolStripMenuItem.Checked = !splitContainer.Panel2Collapsed;
    }

    private void COMRegistryViewer_Load(object sender, EventArgs e)
    {
        treeComRegistry.SuspendLayout();
        treeComRegistry.Nodes.AddRange(m_originalNodes);
        treeComRegistry.ResumeLayout();
    }
}
