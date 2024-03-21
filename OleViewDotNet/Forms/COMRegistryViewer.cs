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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

/// <summary>
/// Form to view the COM registration information
/// </summary>
internal partial class COMRegistryViewer : UserControl
{
    #region Private Members
    private readonly COMRegistry m_registry;
    private readonly HashSet<FilterType> m_filter_types;
    private readonly COMRegistryDisplayMode m_mode;
    private readonly IEnumerable<COMProcessEntry> m_processes;
    private readonly TreeNode m_visible_node;
    private RegistryViewerFilter m_filter;
    private TreeNode[] m_original_nodes;

    private const string FolderKey = "folder.ico";
    private const string InterfaceKey = "interface.ico";
    private const string ClassKey = "class.ico";
    private const string FolderOpenKey = "folderopen.ico";
    private const string ProcessKey = "process.ico";
    private const string ApplicationKey = "application.ico";

    private sealed class DynamicPlaceholderTreeNode : TreeNode
    {
        public DynamicPlaceholderTreeNode() : base("DUMMY")
        {
        }
    }

    private sealed class RuntimeNodeComparer : Comparer<TreeNode>
    {
        public override int Compare(TreeNode x, TreeNode y)
        {
            if ((x.Tag is null && y.Tag is null) || (x.Tag is not null && y.Tag is not null))
            {
                return string.Compare(x.Text, y.Text);
            }
            if (x.Tag is null)
                return -1;
            return 1;
        }
    }

    private static bool IsDynamicNode(TreeNode node)
    {
        return node.Nodes.Count == 1 && node.Nodes[0] is DynamicPlaceholderTreeNode;
    }

    private static bool IsLeafNode(TreeNode node)
    {
        return node.Tag is ICOMClassEntry;
    }

    private static string GetDisplayName(COMRegistryDisplayMode mode)
    {
        return mode switch
        {
            COMRegistryDisplayMode.CLSIDsByName => "CLSIDs by Name",
            COMRegistryDisplayMode.CLSIDs => "CLSIDs",
            COMRegistryDisplayMode.ProgIDs => "ProgIDs",
            COMRegistryDisplayMode.CLSIDsByServer => "CLSIDs by Server",
            COMRegistryDisplayMode.CLSIDsByLocalServer => "CLSIDs by Local Server",
            COMRegistryDisplayMode.CLSIDsWithSurrogate => "CLSIDs with Surrogate",
            COMRegistryDisplayMode.Interfaces => "Interfaces",
            COMRegistryDisplayMode.InterfacesByName => "Interfaces by Name",
            COMRegistryDisplayMode.ImplementedCategories => "Implemented Categories",
            COMRegistryDisplayMode.PreApproved => "Explorer Pre-Approved",
            COMRegistryDisplayMode.IELowRights => "IE Low Rights Policy",
            COMRegistryDisplayMode.LocalServices => "Local Services",
            COMRegistryDisplayMode.AppIDs => "AppIDs",
            COMRegistryDisplayMode.AppIDsWithIL => "AppIDs with IL",
            COMRegistryDisplayMode.AppIDsWithAC => "AppIDs with AC",
            COMRegistryDisplayMode.Typelibs => "TypeLibs",
            COMRegistryDisplayMode.MimeTypes => "MIME Types",
            COMRegistryDisplayMode.ProxyCLSIDs => "Proxy CLSIDs",
            COMRegistryDisplayMode.Processes => "COM Processes",
            COMRegistryDisplayMode.RuntimeClasses => "Runtime Classes",
            COMRegistryDisplayMode.RuntimeServers => "Runtime Servers",
            COMRegistryDisplayMode.RuntimeInterfaces or COMRegistryDisplayMode.RuntimeInterfacesTree => "Runtime Interfaces",
            _ => throw new ArgumentException("Invalid mode value"),
        };
    }

    private static IEnumerable<FilterType> GetFilterTypes(COMRegistryDisplayMode mode)
    {
        HashSet<FilterType> filter_types = new();
        switch (mode)
        {
            case COMRegistryDisplayMode.CLSIDsByName:
            case COMRegistryDisplayMode.CLSIDs:
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Interface);
                break;
            case COMRegistryDisplayMode.ProgIDs:
                filter_types.Add(FilterType.ProgID);
                filter_types.Add(FilterType.Interface);
                break;
            case COMRegistryDisplayMode.CLSIDsByServer:
            case COMRegistryDisplayMode.CLSIDsByLocalServer:
            case COMRegistryDisplayMode.CLSIDsWithSurrogate:
            case COMRegistryDisplayMode.ProxyCLSIDs:
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Server);
                filter_types.Add(FilterType.Interface);
                break;
            case COMRegistryDisplayMode.Interfaces:
            case COMRegistryDisplayMode.InterfacesByName:
            case COMRegistryDisplayMode.RuntimeInterfaces:
            case COMRegistryDisplayMode.RuntimeInterfacesTree:
                filter_types.Add(FilterType.Interface);
                break;
            case COMRegistryDisplayMode.ImplementedCategories:
                filter_types.Add(FilterType.Category);
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Interface);
                break;
            case COMRegistryDisplayMode.PreApproved:
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Interface);
                break;
            case COMRegistryDisplayMode.IELowRights:
                filter_types.Add(FilterType.LowRights);
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Interface);
                break;
            case COMRegistryDisplayMode.AppIDs:
            case COMRegistryDisplayMode.AppIDsWithIL:
            case COMRegistryDisplayMode.AppIDsWithAC:
            case COMRegistryDisplayMode.LocalServices:
                filter_types.Add(FilterType.AppID);
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Interface);
                break;
            case COMRegistryDisplayMode.Typelibs:
                filter_types.Add(FilterType.TypeLib);
                break;
            case COMRegistryDisplayMode.MimeTypes:
                filter_types.Add(FilterType.MimeType);
                filter_types.Add(FilterType.CLSID);
                filter_types.Add(FilterType.Interface);
                break;
            case COMRegistryDisplayMode.Processes:
                filter_types.Add(FilterType.Process);
                filter_types.Add(FilterType.Ipid);
                filter_types.Add(FilterType.AppID);
                break;
            case COMRegistryDisplayMode.RuntimeClasses:
                filter_types.Add(FilterType.RuntimeClass);
                filter_types.Add(FilterType.Interface);
                break;
            case COMRegistryDisplayMode.RuntimeServers:
                filter_types.Add(FilterType.RuntimeServer);
                filter_types.Add(FilterType.RuntimeClass);
                filter_types.Add(FilterType.Interface);
                break;
            default:
                throw new ArgumentException("Invalid mode value");
        }
        return filter_types;
    }

    private void UpdateStatusLabel()
    {
        toolStripStatusLabelCount.Text = $"Showing {treeComRegistry.Nodes.Count} of {m_original_nodes.Length} entries";
    }

    private static TreeNode CreateNode(string text, string image_key, object tag, string tooltip = null)
    {
        bool dynamic = false;
        if (tag is ICOMClassEntry || tag is COMTypeLibVersionEntry || tag is COMTypeLibCoClass || tag is COMProxyFormatter)
        {
            dynamic = true;
        }

        TreeNode node = new(text)
        {
            ImageKey = image_key,
            SelectedImageKey = image_key,
            Tag = tag ?? new object()
        };
        if (tooltip is not null)
        {
            node.ToolTipText = tooltip;
        }
        if (dynamic)
        {
            node.Nodes.Add(new DynamicPlaceholderTreeNode());
        }
        return node;
    }

    private static IEnumerable<TreeNode> SetupTree(COMRegistry registry, COMRegistryDisplayMode mode, IEnumerable<COMProcessEntry> processes)
    {
        try
        {
            switch (mode)
            {
                case COMRegistryDisplayMode.CLSIDsByName:
                    return LoadCLSIDsByNames(registry);
                case COMRegistryDisplayMode.CLSIDs:
                    return LoadCLSIDs(registry);
                case COMRegistryDisplayMode.ProgIDs:
                    return LoadProgIDs(registry);
                case COMRegistryDisplayMode.CLSIDsByServer:
                    return LoadCLSIDByServer(registry, ServerType.None);
                case COMRegistryDisplayMode.CLSIDsByLocalServer:
                    return LoadCLSIDByServer(registry, ServerType.Local);
                case COMRegistryDisplayMode.CLSIDsWithSurrogate:
                    return LoadCLSIDByServer(registry, ServerType.Surrogate);
                case COMRegistryDisplayMode.Interfaces:
                    return LoadInterfaces(registry, false, false);
                case COMRegistryDisplayMode.InterfacesByName:
                    return LoadInterfaces(registry, true, false);
                case COMRegistryDisplayMode.RuntimeInterfaces:
                    return LoadInterfaces(registry, true, true);
                case COMRegistryDisplayMode.RuntimeInterfacesTree:
                    return LoadRuntimeInterfacesTree(registry);
                case COMRegistryDisplayMode.ImplementedCategories:
                    return LoadImplementedCategories(registry);
                case COMRegistryDisplayMode.PreApproved:
                    return LoadPreApproved(registry);
                case COMRegistryDisplayMode.IELowRights:
                    return LoadIELowRights(registry);
                case COMRegistryDisplayMode.LocalServices:
                    return LoadLocalServices(registry);
                case COMRegistryDisplayMode.AppIDs:
                    return LoadAppIDs(registry, false, false);
                case COMRegistryDisplayMode.AppIDsWithIL:
                    return LoadAppIDs(registry, true, false);
                case COMRegistryDisplayMode.AppIDsWithAC:
                    return LoadAppIDs(registry, false, true);
                case COMRegistryDisplayMode.Typelibs:
                    return LoadTypeLibs(registry);
                case COMRegistryDisplayMode.MimeTypes:
                    return LoadMimeTypes(registry);
                case COMRegistryDisplayMode.ProxyCLSIDs:
                    return LoadCLSIDByServer(registry, ServerType.Proxies);
                case COMRegistryDisplayMode.Processes:
                    return LoadProcesses(registry, processes);
                case COMRegistryDisplayMode.RuntimeClasses:
                    return LoadRuntimeClasses(registry);
                case COMRegistryDisplayMode.RuntimeServers:
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
        if (entry is not null)
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
        if (instance is not null && instance.Module is not null)
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
        return CreateNode($"{ent.Clsid.FormatGuid()} - {ent.Name}", ClassKey, ent, BuildCLSIDToolTip(registry, ent));
    }

    private static TreeNode CreateInterfaceNode(COMRegistry registry, COMInterfaceEntry ent)
    {
        return CreateNode($"{ent.Iid.FormatGuid()} - {ent.Name}", 
            InterfaceKey, ent, BuildInterfaceToolTip(ent, null));
    }

    private static TreeNode CreateInterfaceNameNode(COMRegistry registry, COMInterfaceEntry ent, COMInterfaceInstance instance)
    {
        return CreateNode(ent.Name, InterfaceKey, ent, BuildInterfaceToolTip(ent, instance));
    }

    private static IEnumerable<TreeNode> LoadCLSIDs(COMRegistry registry)
    {
        return registry.Clsids.Values.Select(ent => CreateCLSIDNode(registry, ent));
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
            if (server is null)
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
        return registry.Progids.Values.Select(p => 
        CreateNode(p.ProgID, ClassKey, p, BuildProgIDToolTip(registry, p)));
    }

    private static IEnumerable<TreeNode> LoadCLSIDsByNames(COMRegistry registry)
    {
        return registry.Clsids.Values.Select(ent 
            => CreateNode(ent.Name, ClassKey, 
            ent, BuildCLSIDToolTip(registry, ent))).OrderBy(n => n.Text);
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
                    var curr_server = server;
                    if (curr_server.AppIdHosted)
                    {
                        var local_service = entry.AppIDEntry?.LocalService;
                        if (local_service is null)
                            continue;
                        if (!string.IsNullOrEmpty(local_service.ServiceDll))
                        {
                            curr_server = new COMCLSIDServerEntry(COMServerType.LocalServer32, local_service.ServiceDll);
                        }
                        else
                        {
                            curr_server = new COMCLSIDServerEntry(COMServerType.LocalServer32, local_service.ImagePath);
                        }
                    }

                    if (!dict.ContainsKey(curr_server))
                    {
                        dict[curr_server] = new List<COMCLSIDEntry>();
                    }
                    dict[curr_server].Add(entry);
                }
            }

            servers = dict;

            if (serverType == ServerType.Local)
            {
                servers = servers.Where(p => p.Key.ServerType == COMServerType.LocalServer32);
            }
            else if (serverType == ServerType.Proxies)
            {
                servers = servers.Where(p => p.Key.ServerType == COMServerType.InProcServer32);
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

    private static IEnumerable<TreeNode> LoadInterfaces(COMRegistry registry, bool by_name, bool runtime_interfaces)
    {
        var total_intfs = registry.Interfaces.Values.AsEnumerable();
        if (runtime_interfaces)
            total_intfs = total_intfs.Where(i => i.RuntimeInterface);

        if (by_name)
        {
            return total_intfs.OrderBy(i => i.Name).Select(i => CreateInterfaceNameNode(registry, i, null));
        }
        
        return total_intfs.Select(i => CreateInterfaceNode(registry, i));
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
            if (appids.ContainsKey(pair.Key) && appids[pair.Key].IsService)
            {
                serverNodes.Add(CreateLocalServiceNode(registry, appids[pair.Key], pair.Value));
            }
        }

        return serverNodes.OrderBy(n => n.Text);
    }

    private static string LimitString(string s, int max)
    {
        if (s is null)
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

        return sortedNodes.Values;
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
            if (entry is not null)
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
        if (!string.IsNullOrWhiteSpace(entry.Win32Path))
        {
            entries.Add($"Win32: {entry.Win32Path}");
        }
        if (!string.IsNullOrWhiteSpace(entry.Win64Path))
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

    private static TreeNode CreateNodes(List<TreeNode> base_nodes, Dictionary<string, TreeNode> tree, string name)
    {
        string base_name = string.Empty;
        string node_name = name;
        int index = name.LastIndexOf('.');

        if (index >= 0)
        {
            base_name = name.Substring(0, index);
            node_name = name.Substring(index + 1);
        }

        TreeNode node = new(node_name);
        node.ImageKey = FolderKey;
        node.SelectedImageKey = FolderOpenKey;

        if (base_name == string.Empty)
        {
            if (!tree.ContainsKey(node_name))
            {
                tree[node_name] = node;
                base_nodes.Add(node);
            }
        }
        else
        {
            if (!tree.TryGetValue(base_name, out TreeNode root_node))
            {
                root_node = CreateNodes(base_nodes, tree, base_name);
                tree[base_name] = root_node;
            }
            root_node.Nodes.Add(node);
        }

        return node;
    }

    private static IEnumerable<TreeNode> LoadRuntimeInterfacesTree(COMRegistry registry)
    {
        Dictionary<string, TreeNode> tree = new();
        List<TreeNode> base_nodes = new();
        foreach (var intf in registry.Interfaces.Values.Where(i => i.RuntimeInterface).OrderBy(i => i.Name))
        {
            var node = CreateNodes(base_nodes, tree, intf.Name);
            node.ImageKey = InterfaceKey;
            node.SelectedImageKey = InterfaceKey;
            node.Tag = intf;
        }

        return base_nodes;
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
                        (clsid.FactoryInterfaces.Any(i => i.Iid == COMKnownGuids.IID_IPSFactoryBuffer)
                        || m_registry.ProxiesByClsid.ContainsKey(clsid_entry.Clsid)))
                    {
                        TreeNode proxy = CreateNode("Proxy", ProcessKey, new COMProxyFormatter(clsid_entry));
                        node.Nodes.Add(proxy);
                    }
                }

                var typelib = m_registry.MapClsidToEntry(clsid.Clsid)?.TypeLibEntry;
                if (typelib is not null)
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

    private static void AddTypeLibNodes(TreeNode node, IEnumerable<COMTypeLibTypeInfo> types, string category, string image_key)
    {
        if (types.Any())
        {
            var sub_node = CreateNode(category, FolderKey, types);
            node.Nodes.Add(sub_node);
            sub_node.Nodes.AddRange(types.Select(type => CreateNode(type.Name, image_key, type)).ToArray());
        }
    }

    private static void SetupTypeLibNodeTree(COMTypeLib typelib, TreeNode node)
    {
        AddTypeLibNodes(node, typelib.Interfaces, "Interfaces", InterfaceKey);
        AddTypeLibNodes(node, typelib.Dispatch, "Dispatch Interfaces", InterfaceKey);
        AddTypeLibNodes(node, typelib.Classes, "Classes", ClassKey);
        AddTypeLibNodes(node, typelib.Records, "Records", ClassKey);
        AddTypeLibNodes(node, typelib.Unions, "Unions", ClassKey);
        AddTypeLibNodes(node, typelib.Enums, "Enums", ClassKey);
        AddTypeLibNodes(node, typelib.Modules, "Modules", ProcessKey);
        AddTypeLibNodes(node, typelib.Aliases, "Aliases", ClassKey);
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
            SetupTypeLibNodeTree(parsed_typelib, node);
            treeComRegistry.ResumeLayout();
            sourceCodeViewerControl.SelectedObject = typelib;
        }
        catch (Exception ex)
        {
            wait_node.Text = $"Error parsing type library - {ex.Message}";
        }
    }

    private static void AddProxyNodes(TreeNode node, IEnumerable<COMProxyTypeInfo> types, string category, string image_key)
    {
        if (types.Any())
        {
            var sub_node = CreateNode(category, FolderKey, types);
            node.Nodes.Add(sub_node);
            sub_node.Nodes.AddRange(types.Select(type => CreateNode(type.Name, image_key, type)).ToArray());
        }
    }

    private static void SetupProxyNodeTree(COMProxyFile proxy_file, TreeNode node)
    {
        AddProxyNodes(node, proxy_file.Entries, "Interfaces", InterfaceKey);
        AddProxyNodes(node, proxy_file.ComplexTypes.Where(t => !t.IsUnion), "Structs", ClassKey);
        AddProxyNodes(node, proxy_file.ComplexTypes.Where(t => t.IsUnion), "Unions", ClassKey);
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
            SetupProxyNodeTree(proxy.ProxyFile, node);
            treeComRegistry.ResumeLayout();
            sourceCodeViewerControl.SelectedObject = proxy;
        }
        catch (Exception ex)
        {
            wait_node.Text = $"Error parsing proxy file - {ex.Message}";
        }
    }

    private static TreeNode CreateProxyNodes(COMProxyFile proxy)
    {
        TreeNode root_node = CreateNode(proxy.Path, ClassKey, proxy);
        SetupProxyNodeTree(proxy, root_node);
        return root_node;
    }

    private async void treeComRegistry_BeforeExpand(object sender, TreeViewCancelEventArgs e)
    {
        Cursor currCursor = Cursor.Current;
        Cursor.Current = Cursors.WaitCursor;

        if (IsDynamicNode(e.Node))
        {
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
            else
            {
                e.Node.Nodes.Clear();
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
            MiscUtilities.CopyGuidToClipboard(guid, GuidFormat.String);
        }
    }

    private void copyGUIDCStructureToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Guid guid = GetGuidFromType(treeComRegistry.SelectedNode);

        if (guid != Guid.Empty)
        {
            MiscUtilities.CopyGuidToClipboard(guid, GuidFormat.Structure);
        }
    }

    private void copyGUIDHexStringToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Guid guid = GetGuidFromType(treeComRegistry.SelectedNode);

        if (guid != Guid.Empty)
        {
            MiscUtilities.CopyGuidToClipboard(guid, GuidFormat.HexString);
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
            MiscUtilities.CopyGuidToClipboard(guid, GuidFormat.Object);
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
        if (ent is null)
            return;
        try
        {
            object comObj = ent.CreateInstanceAsObject(clsctx, server);
            if (comObj is not null)
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
        if (ent is null)
            return;
        try
        {
            object comObj = ent.CreateClassFactory(CLSCTX.ALL, server);
            if (comObj is not null)
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
        if (clsid is null)
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

        if ((node is not null) && (node.Tag is not null))
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
                if (entry is not null && entry.SupportsRemoteActivation)
                {
                    createSpecialToolStripMenuItem.DropDownItems.Add(createClassFactoryRemoteToolStripMenuItem);
                }

                if (runtime_class is not null && runtime_class.TrustLevel == TrustLevel.PartialTrust)
                {
                    createSpecialToolStripMenuItem.DropDownItems.Add(createInRuntimeBrokerToolStripMenuItem);
                    createSpecialToolStripMenuItem.DropDownItems.Add(createInPerUserRuntimeBrokerToolStripMenuItem);
                    createSpecialToolStripMenuItem.DropDownItems.Add(createFactoryInRuntimeBrokerToolStripMenuItem);
                    createSpecialToolStripMenuItem.DropDownItems.Add(createFactoryInPerUserRuntimeBrokerToolStripMenuItem);
                }

                contextMenuStrip.Items.Add(createSpecialToolStripMenuItem);
                contextMenuStrip.Items.Add(refreshInterfacesToolStripMenuItem);

                if (clsid is not null)
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

                if (runtime_class is not null)
                {
                    COMRuntimeServerEntry server =
                        runtime_class.HasServer
                            ? m_registry.MapServerNameToEntry(runtime_class.Server) : null;
                    if (runtime_class.HasPermission || (server is not null && server.HasPermission))
                    {
                        contextMenuStrip.Items.Add(viewAccessPermissionsToolStripMenuItem);
                    }
                }
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

                if (RuntimeMetadata.Interfaces.ContainsKey(intf.Iid))
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

            if (m_filter_types.Contains(FilterType.CLSID) || 
                m_filter_types.Contains(FilterType.RuntimeClass) || 
                m_filter_types.Contains(FilterType.ProgID))
            {
                contextMenuStrip.Items.Add(queryAllInterfacesToolStripMenuItem);
            }

            if (treeComRegistry.Nodes.Count > 0)
            {
                contextMenuStrip.Items.Add(cloneTreeToolStripMenuItem);
                selectedToolStripMenuItem.Enabled = treeComRegistry.SelectedNode is not null;
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
        if (node is not null)
        {
            ICOMClassEntry class_entry = node.Tag as ICOMClassEntry;

            if (node.Tag is COMTypeLibCoClass typelib_class)
            {
                class_entry = m_registry.MapClsidToEntry(typelib_class.Uuid);
            }

            if (class_entry is not null)
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

    private static Func<string, bool> CreateFilter(string filter, FilterMode mode, bool caseSensitive)
    {
        filter = filter.Trim();
        if (string.IsNullOrEmpty(filter))
        {
            return null;
        }

        StringComparison comp = caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

        switch (mode)
        {
            case FilterMode.Contains:
                return caseSensitive ? (n => n.Contains(filter)) : (n => n.ToLower().Contains(filter.ToLower()));
            case FilterMode.BeginsWith:
                return n => n.StartsWith(filter, comp);
            case FilterMode.EndsWith:
                return n => n.EndsWith(filter, comp);
            case FilterMode.Equals:
                return n => n.Equals(filter, comp);
            case FilterMode.Glob:
                {
                    Regex r = GlobToRegex(filter, caseSensitive);

                    return n => r.IsMatch(n);
                }
            case FilterMode.Regex:
                {
                    Regex r = new(filter, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);

                    return n => r.IsMatch(n);
                }
            default:
                throw new ArgumentException("Invalid mode value");
        }
    }

    private FilterResult FilterLeafNode(TreeNode n, IRegistryViewerFilter filter)
    {
        FilterResult result = FilterResult.None;
        if (n.Tag is ICOMClassEntry class_entry && class_entry.InterfacesLoaded)
        {
            foreach (COMInterfaceEntry intf in class_entry.Interfaces.Concat(class_entry.FactoryInterfaces)
                .Select(i => m_registry.MapIidToInterface(i.Iid)))
            {
                var curr_result = filter.Filter(intf.Name, intf);
                if (curr_result == FilterResult.Exclude)
                {
                    return FilterResult.Exclude;
                }
                else if (curr_result != FilterResult.None)
                {
                    result = FilterResult.Include;
                }
            }
        }
        return result;
    }

    // Check if top node or one of its subnodes matches the filter
    private FilterResult FilterNode(TreeNode n, IRegistryViewerFilter filter)
    {
        FilterResult result = filter.Filter(n.Text, n.Tag);

        if (result == FilterResult.None)
        {
            if (IsLeafNode(n))
            {
                result = FilterLeafNode(n, filter);
            }
            else if (!IsDynamicNode(n) && n.Nodes.Count > 0)
            {
                var nodes = n.Nodes.OfType<TreeNode>().ToArray();
                n.Nodes.Clear();
                bool has_children = false;
                foreach (TreeNode node in nodes)
                {
                    result = FilterNode(node, filter);
                    if (result == FilterResult.Include)
                    {
                        n.Nodes.Add(node);
                        has_children = true;
                    }
                }
                if (has_children)
                {
                    result = FilterResult.Include;
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
            IRegistryViewerFilter filter = null;
            FilterMode mode = (FilterMode)comboBoxMode.SelectedItem;

            if (mode == FilterMode.Complex)
            {
                using ViewFilterForm form = new(m_filter, m_filter_types);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    m_filter = form.Filter;
                    if (m_filter.Filters.Count > 0)
                    {
                        filter = m_filter;
                    }
                }
                else
                {
                    return;
                }
            }
            else if (mode == FilterMode.Accessible || mode == FilterMode.NotAccessible)
            {
                using SelectSecurityCheckForm form = new(m_mode == COMRegistryDisplayMode.Processes);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    COMAccessCheck access_check = new(form.Token, form.Principal, form.AccessRights, form.LaunchRights, form.IgnoreDefault);
                    filter = new RegistryViewerAccessibleFilter(access_check, mode == FilterMode.NotAccessible);
                }
                else
                {
                    return;
                }
            }
            else
            {
                Func<string, bool> filterFunc = CreateFilter(textBoxFilter.Text, mode, false);
                if (filterFunc is not null)
                {
                    filter = new RegistryViewerDisplayFilter(filterFunc);
                }
            }

            if (filter is not null)
            {
                using (filter)
                {
                    nodes = await Task.Run(() => m_original_nodes.Select(n => (TreeNode)n.Clone()).
                        Where(n => FilterNode(n, filter) == FilterResult.Include).ToArray());
                }
            }
            else
            {
                nodes = m_original_nodes;
            }

            treeComRegistry.SuspendLayout();
            bool node_expanded = treeComRegistry.SelectedNode?.IsExpanded ?? false;
            TreeNode visible_node = FindVisibleNode(nodes, treeComRegistry.SelectedNode?.Tag);
            treeComRegistry.Nodes.Clear();
            treeComRegistry.Nodes.AddRange(nodes);
            visible_node?.EnsureVisible();
            if (node_expanded)
            {
                visible_node?.Expand();
            }
            treeComRegistry.SelectedNode = visible_node;
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

            if (node is not null)
            {
                treeComRegistry.SelectedNode = node;
            }
        }
    }

    private static TreeNode CreateTypeLibNodes(COMTypeLib typelib)
    {
        TreeNode root_node = CreateNode($"{typelib.Name} : Version {typelib.Version}",
                ClassKey, typelib);
        SetupTypeLibNodeTree(typelib, root_node);
        return root_node;
    }

    private async void viewTypeLibraryToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node?.Tag is null)
            return;

        try
        {
            COMTypeLibVersionEntry typelib = null;
            ICOMClassEntry class_entry = node.Tag as ICOMClassEntry;
            COMInterfaceEntry intf_entry = node.Tag as COMInterfaceEntry;

            if (class_entry is not null)
            {
                typelib = m_registry.MapClsidToEntry(class_entry.Clsid)?.TypeLibEntry?.
                    Versions.First() ?? throw new ArgumentException("No type library registered for class.");
            }
            else if (intf_entry is not null)
            {
                typelib = intf_entry.TypeLibVersionEntry ?? throw new ArgumentException("No type library registered for interface.");
            }
            else
            {
                throw new ArgumentException("No type library registered.");
            }

            var parsed_typelib = await Task.Run(() => typelib.Parse());
            COMTypeLibTypeInfo visible_type = null;
            if (class_entry is not null)
            {
                visible_type = parsed_typelib.Classes.FirstOrDefault(c => c.Uuid == class_entry.Clsid);
            }
            else
            {
                visible_type = parsed_typelib.Interfaces.FirstOrDefault(i => i.Uuid == intf_entry.Iid);
                if (visible_type is null)
                {
                    visible_type = parsed_typelib.Dispatch.FirstOrDefault(i => i.Uuid == intf_entry.Iid);
                }
            }

            COMRegistryViewer viewer = new(m_registry, parsed_typelib, visible_type);
            EntryPoint.GetMainForm(m_registry).HostControl(viewer);
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node is not null)
        {
            EntryPoint.GetMainForm(m_registry).HostControl(new PropertiesControl(m_registry, node.Text, node.Tag));
        }
    }

    private void ViewPermissions(bool access)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node is not null)
        {
            if (node.Tag is COMProcessEntry proc)
            {
                COMSecurity.ViewSecurity(m_registry, $"{proc.Name} Access", proc.AccessPermissions, true);
            }
            else if (node.Tag is COMRuntimeClassEntry || node.Tag is COMRuntimeServerEntry)
            {
                COMRuntimeServerEntry runtime_server = node.Tag as COMRuntimeServerEntry;
                COMRuntimeClassEntry runtime_class = node.Tag as COMRuntimeClassEntry;
                string name = runtime_class is not null ? runtime_class.Name : runtime_server.Name;
                if (runtime_class is not null && runtime_class.HasServer)
                {
                    runtime_server = m_registry.MapServerNameToEntry(runtime_class.Server);
                }

                COMSecurityDescriptor perms = runtime_server?.Permissions ?? runtime_class.Permissions;

                COMSecurity.ViewSecurity(m_registry, $"{name} Access", perms, false);
            }
            else
            {
                COMAppIDEntry appid = node.Tag as COMAppIDEntry;
                if (appid is null)
                {
                    COMCLSIDEntry clsid = node.Tag as COMCLSIDEntry;
                    if (clsid is null && node.Tag is COMProgIDEntry prog_id)
                    {
                        clsid = m_registry.MapClsidToEntry(prog_id.Clsid);
                    }

                    if (clsid is not null && m_registry.AppIDs.ContainsKey(clsid.AppID))
                    {
                        appid = m_registry.AppIDs[clsid.AppID];
                    }
                }

                if (appid is not null)
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
        if (node is not null)
        {
            MiscUtilities.CopyTextToClipboard(node.Text);
        }
    }

    private void viewProxyLibraryToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node is not null)
        {
            COMCLSIDEntry clsid = node.Tag as COMCLSIDEntry;
            Guid selected_iid = Guid.Empty;
            if (clsid is null && (node.Tag is COMInterfaceEntry || node.Tag is COMIPIDEntry))
            {
                COMInterfaceEntry intf = node.Tag as COMInterfaceEntry;
                intf ??= m_registry.MapIidToInterface(((COMIPIDEntry)node.Tag).Iid);

                selected_iid = intf.Iid;
                clsid = m_registry.Clsids[intf.ProxyClsid];
            }

            if (clsid is not null)
            {
                try
                {
                    COMProxyFile proxy_file = COMProxyFile.GetFromCLSID(clsid);
                    COMProxyTypeInfo visible_interface = proxy_file.Entries.Where(e => e.Iid == selected_iid).FirstOrDefault();

                    EntryPoint.GetMainForm(m_registry).HostControl(
                        new COMRegistryViewer(
                            m_registry,
                            proxy_file,
                            visible_interface
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

    private void GetClsidsFromNodes(List<ICOMClassEntry> clsids, TreeNodeCollection nodes)
    {
        foreach (TreeNode node in nodes)
        {
            if (node.Tag is ICOMClassEntry clsid)
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
            List<ICOMClassEntry> clsids = new();
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
        if (comboBoxMode.SelectedItem is not null)
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
        if (node is not null && node.Tag is COMProcessEntry)
        {
            COMProcessEntry process = (COMProcessEntry)node.Tag;
            process = COMProcessParser.ParseProcess(process.ProcessId, COMProcessParserConfig.Default, m_registry);
            if (process is null)
            {
                treeComRegistry.Nodes.Remove(treeComRegistry.SelectedNode);
                m_original_nodes = m_original_nodes.Where(n => n != node).ToArray();
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
        if (treeComRegistry.SelectedNode is not null)
        {
            return treeComRegistry.SelectedNode.Tag as COMIPIDEntry;
        }
        return null;
    }

    private void toHexEditorToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid is not null)
        {
            EntryPoint.GetMainForm(m_registry).HostControl(new ObjectHexEditor(m_registry,
                ipid.Ipid.ToString(),
                ipid.ToObjref()));
        }
    }

    private void toFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid is not null)
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
        if (ipid is not null)
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

    private void CreateClonedTree(IEnumerable<TreeNode> nodes, bool cloned = false)
    {
        string text = Text;
        if (!text.StartsWith("Clone of "))
        {
            text = "Clone of " + text;
        }
        if (!cloned)
        {
            nodes = nodes.Select(n => (TreeNode)n.Clone());
        }
        COMRegistryViewer viewer = new(m_registry, m_mode, m_processes, nodes, m_filter_types, text);
        EntryPoint.GetMainForm(m_registry).HostControl(viewer);
    }

    private void allVisibleToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CreateClonedTree(treeComRegistry.Nodes.OfType<TreeNode>());
    }

    private void selectedToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode selected = treeComRegistry.SelectedNode;
        if (selected is not null)
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
                await Task.Run(() => m_original_nodes.Select(n => (TreeNode)n.Clone()).Where(n =>
                FilterNode(n, form.Filter) == FilterResult.Include).ToArray());
            CreateClonedTree(nodes, true);
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
        if (selected is not null && selected.Nodes.Count > 0)
        {
            CreateClonedTree(selected.Nodes.OfType<TreeNode>());
        }
    }

    private void viewRuntimeInterfaceToolStripMenuItem_Click(object sender, EventArgs e)
    {
        TreeNode node = treeComRegistry.SelectedNode;
        if (node is not null)
        {
            if (node.Tag is COMInterfaceEntry ent && RuntimeMetadata.Interfaces.ContainsKey(ent.Iid))
            {
                Assembly asm = RuntimeMetadata.Interfaces[ent.Iid].Assembly;
                EntryPoint.GetMainForm(m_registry).HostControl(new TypeLibControl(asm.GetName().Name,
                    asm, ent.Iid, false));
            }
        }
    }

    private async void CreateInRuntimeBroker(bool per_user, bool factory)
    {
        try
        {
            if (GetSelectedClassEntry() is COMRuntimeClassEntry runtime_class)
            {
                IRuntimeBroker broker = COMUtilities.CreateBroker(per_user);
                object comObj;
                if (factory)
                {
                    comObj = broker.GetActivationFactory(runtime_class.Name, COMKnownGuids.IID_IUnknown);
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
        treeComRegistry.Nodes.AddRange(m_original_nodes);
        treeComRegistry.ResumeLayout();
        UpdateStatusLabel();
        m_visible_node?.EnsureVisible();
        treeComRegistry.SelectedNode = m_visible_node;
    }

    private static TreeNode FindVisibleNode(IEnumerable<TreeNode> nodes, object tag)
    {
        if (tag is null)
            return null;
        TreeNode curr_node = null;
        foreach (var node in nodes)
        {
            if (ReferenceEquals(node.Tag, tag))
            {
                curr_node = node;
                break;
            }
            curr_node = FindVisibleNode(node.Nodes.OfType<TreeNode>(), tag);
            if (curr_node is not null)
            {
                break;
            }
        }
        return curr_node;
    }

    private COMRegistryViewer(COMRegistry reg, COMRegistryDisplayMode mode,
        TreeNode root_node, IEnumerable<FilterType> filter_types, 
        string text, object visible_obj) 
        : this(reg, mode, Array.Empty<COMProcessEntry>(), new[] { root_node }, filter_types, text)
    {
        m_visible_node = FindVisibleNode(m_original_nodes, visible_obj);
        splitContainer.Panel2Collapsed = false;
        showSourceCodeToolStripMenuItem.Visible = false;
        root_node.Expand();
    }

    private COMRegistryViewer(COMRegistry reg, COMRegistryDisplayMode mode,
        IEnumerable<COMProcessEntry> processes, IEnumerable<TreeNode> nodes, 
        IEnumerable<FilterType> filter_types, string text)
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
        splitContainer.Panel2Collapsed = !ProgramSettings.AlwaysShowSourceCode;
        showSourceCodeToolStripMenuItem.Checked = ProgramSettings.AlwaysShowSourceCode;
        sourceCodeViewerControl.AutoParse = ProgramSettings.EnableAutoParsing;
        m_original_nodes = nodes.ToArray();
    }
    #endregion

    #region Constructors
    public COMRegistryViewer(COMRegistry reg, COMRegistryDisplayMode mode, IEnumerable<COMProcessEntry> processes)
        : this(reg, mode, processes, SetupTree(reg, mode, processes), GetFilterTypes(mode), GetDisplayName(mode))
    {
        if (m_mode == COMRegistryDisplayMode.RuntimeInterfacesTree)
        {
            treeComRegistry.TreeViewNodeSorter = new RuntimeNodeComparer();
        }
    }

    public COMRegistryViewer(COMRegistry reg, COMTypeLib typelib, COMTypeLibTypeInfo visible_type)
        : this(reg, COMRegistryDisplayMode.Typelibs, CreateTypeLibNodes(typelib), 
              new[] { FilterType.TypeLibTypeInfo }, typelib.ToString(), visible_type)
    {
    }

    public COMRegistryViewer(COMRegistry reg, COMProxyFile proxy, COMProxyTypeInfo visible_type)
    : this(reg, COMRegistryDisplayMode.ProxyCLSIDs, CreateProxyNodes(proxy),
          new[] { FilterType.TypeLibTypeInfo }, proxy.Path, visible_type)
    {
    }
    #endregion
}
