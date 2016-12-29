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

using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OleViewDotNet
{
    /// <summary>
    /// Form to view the COM registration information
    /// </summary>
    public partial class COMRegistryViewer : UserControl
    {
        /// <summary>
        /// Current registry
        /// </summary>
        COMRegistry m_registry;
        TreeNode[] m_originalNodes;
        HashSet<FilterType> m_filter_types;
        RegistryViewerFilter m_filter;

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
        }

        /// <summary>
        /// Current display mode
        /// </summary>
        private DisplayMode m_mode;

        /// <summary>
        /// Constants for the ImageList icons
        /// </summary>
        private const int FolderIcon = 0;
        private const int InterfaceIcon = 1;
        private const int ClassIcon = 2;        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reg">The COM registry</param>
        /// <param name="mode">The display mode</param>
        public COMRegistryViewer(COMRegistry reg, DisplayMode mode)
        {
            InitializeComponent();
            m_registry = reg;
            m_filter_types = new HashSet<FilterType>();
            m_filter = new RegistryViewerFilter();
            m_mode = mode;
            foreach (FilterMode filter in Enum.GetValues(typeof(FilterMode)))
            {
                comboBoxMode.Items.Add(filter);
            }
            comboBoxMode.SelectedIndex = 0;
            SetupTree();
        }

        private void SetupTree()
        {
            Cursor currCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                switch (m_mode)
                {
                    case DisplayMode.CLSIDsByName:
                        LoadCLSIDsByNames();
                        break;
                    case DisplayMode.CLSIDs:
                        LoadCLSIDs();
                        break;
                    case DisplayMode.ProgIDs:
                        LoadProgIDs();
                        break;
                    case DisplayMode.CLSIDsByServer:
                        LoadCLSIDByServer(ServerType.None);
                        break;
                    case DisplayMode.CLSIDsByLocalServer:
                        LoadCLSIDByServer(ServerType.Local);
                        break;
                    case DisplayMode.CLSIDsWithSurrogate:
                        LoadCLSIDByServer(ServerType.Surrogate);
                        break;
                    case DisplayMode.Interfaces:
                        LoadInterfaces(false);
                        break;
                    case DisplayMode.InterfacesByName:
                        LoadInterfaces(true);
                        break;
                    case DisplayMode.ImplementedCategories:
                        LoadImplementedCategories();
                        break;
                    case DisplayMode.PreApproved:
                        LoadPreApproved();
                        break;
                    case DisplayMode.IELowRights:
                        LoadIELowRights();
                        break;
                    case DisplayMode.LocalServices:
                        LoadLocalServices();
                        break;
                    case DisplayMode.AppIDs:
                        LoadAppIDs(false, false);
                        break;
                    case DisplayMode.AppIDsWithIL:
                        LoadAppIDs(true, false);
                        break;
                    case DisplayMode.AppIDsWithAC:
                        LoadAppIDs(false, true);
                        break;
                    case DisplayMode.Typelibs:
                        LoadTypeLibs();
                        break;
                    case DisplayMode.MimeTypes:
                        LoadMimeTypes();
                        break;
                    case DisplayMode.ProxyCLSIDs:
                        LoadCLSIDByServer(ServerType.Proxies);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Cursor.Current = currCursor;
            m_originalNodes = treeComRegistry.Nodes.OfType<TreeNode>().ToArray();
        }

        /// <summary>
        /// Build a tooltip for a CLSID entry
        /// </summary>
        /// <param name="ent">The CLSID entry to build the tool tip from</param>
        /// <returns>A string tooltip</returns>
        private string BuildCLSIDToolTip(COMCLSIDEntry ent)
        {
            StringBuilder strRet = new StringBuilder();

            AppendFormatLine(strRet, "CLSID: {0}", ent.Clsid.ToString("B"));
            AppendFormatLine(strRet, "Name: {0}", ent.Name);
            AppendFormatLine(strRet, "{0}: {1}", ent.DefaultServerType.ToString(), ent.DefaultServer);
            IEnumerable<string> progids = m_registry.GetProgIdsForClsid(ent.Clsid).Select(p => p.ProgID);
            if (progids.Count() > 0)
            {
                strRet.AppendLine("ProgIDs:");
                foreach (string progid in progids)
                {
                    AppendFormatLine(strRet, "{0}", progid);
                }
            }
            if (ent.AppID != Guid.Empty)
            {
                AppendFormatLine(strRet, "AppID: {0}", ent.AppID.ToString("B"));            
            }
            if (ent.TypeLib != Guid.Empty)
            {
                AppendFormatLine(strRet, "TypeLib: {0}", ent.TypeLib.ToString("B"));
            }

            COMInterfaceEntry[] proxies = m_registry.GetProxiesForClsid(ent);
            if (proxies.Length > 0)
            {
                AppendFormatLine(strRet, "Interface Proxies: {0}", proxies.Length);
            }

            if (ent.InterfacesLoaded)
            {
                AppendFormatLine(strRet, "Instance Interfaces: {0}", ent.Interfaces.Count());
                AppendFormatLine(strRet, "Factory Interfaces: {0}", ent.FactoryInterfaces.Count());
            }

            return strRet.ToString();
        }

        /// <summary>
        /// Build a ProgID entry tooltip
        /// </summary>
        /// <param name="ent">The ProgID entry</param>
        /// <returns>The ProgID tooltip</returns>
        private string BuildProgIDToolTip(COMProgIDEntry ent)
        {
            string strRet;
            COMCLSIDEntry entry = m_registry.MapClsidToEntry(ent.Clsid);
            if (entry != null)
            {
                strRet = BuildCLSIDToolTip(entry);
            }
            else
            {
                strRet = String.Format("CLSID: {0}\n", ent.Clsid.ToString("B"));
            }

            return strRet;
        }

        private string BuildInterfaceToolTip(COMInterfaceEntry ent, COMInterfaceInstance instance)
        {            
            StringBuilder builder = new StringBuilder();

            AppendFormatLine(builder, "Name: {0}", ent.Name);
            AppendFormatLine(builder, "IID: {0}", ent.Iid.ToString("B"));
            if (ent.ProxyClsid != Guid.Empty)
            {
                AppendFormatLine(builder, "ProxyCLSID: {0}", ent.ProxyClsid.ToString("B"));
            }
            if (instance != null && instance.Module != null)
            {
                AppendFormatLine(builder, "VTable Address: {0}+0x{1:X}", instance.Module, instance.VTableOffset);
            }
            if (ent.HasTypeLib)
            {
                AppendFormatLine(builder, "TypeLib: {0}", ent.TypeLib.ToString("B"));
            }

            return builder.ToString();
        }

        private TreeNode CreateCLSIDNode(COMCLSIDEntry ent)
        {            
            TreeNode nodeRet = new TreeNode(String.Format("{0} - {1}", ent.Clsid.ToString(), ent.Name), ClassIcon, ClassIcon);
            nodeRet.ToolTipText = BuildCLSIDToolTip(ent);
            nodeRet.Tag = ent;
            nodeRet.Nodes.Add("IUnknown");

            return nodeRet;
        }

        private TreeNode CreateInterfaceNode(COMInterfaceEntry ent)
        {
            TreeNode nodeRet = new TreeNode(String.Format("{0} - {1}", ent.Iid.ToString(), ent.Name), InterfaceIcon, InterfaceIcon);
            nodeRet.ToolTipText = BuildInterfaceToolTip(ent, null);
            nodeRet.Tag = ent;

            return nodeRet;
        }

        private TreeNode CreateInterfaceNameNode(COMInterfaceEntry ent, COMInterfaceInstance instance)
        {
            TreeNode nodeRet = new TreeNode(ent.Name, InterfaceIcon, InterfaceIcon);
            nodeRet.ToolTipText = BuildInterfaceToolTip(ent, instance);
            nodeRet.Tag = ent;

            return nodeRet;
        }

        private void LoadCLSIDs()
        {
            int i = 0;
            TreeNode[] clsidNodes = new TreeNode[m_registry.Clsids.Count];
            foreach (COMCLSIDEntry ent in m_registry.Clsids.Values)
            {
                clsidNodes[i] = CreateCLSIDNode(ent);                
                i++;
            }
            m_filter_types.Add(FilterType.CLSID);
            m_filter_types.Add(FilterType.Interface);
            treeComRegistry.Nodes.AddRange(clsidNodes);
            Text = "CLSIDs";            
        }
        
        private void LoadProgIDs()
        {
            int i = 0;
            TreeNode[] progidNodes = new TreeNode[m_registry.Progids.Count];
            foreach (COMProgIDEntry ent in m_registry.Progids.Values)
            {
                progidNodes[i] = new TreeNode(ent.ProgID, ClassIcon, ClassIcon);
                progidNodes[i].ToolTipText = BuildProgIDToolTip(ent);
                progidNodes[i].Tag = ent;
                if (m_registry.MapClsidToEntry(ent.Clsid) != null)
                {
                    progidNodes[i].Nodes.Add("IUnknown");
                }
                i++;
            }
            m_filter_types.Add(FilterType.ProgID);
            treeComRegistry.Nodes.AddRange(progidNodes);
            Text = "ProgIDs";
        }

        private void LoadCLSIDsByNames()
        {
            List<TreeNode> nodes = new List<TreeNode>(m_registry.Clsids.Count);
            foreach (COMCLSIDEntry ent in m_registry.Clsids.Values)
            {
                TreeNode node = new TreeNode(ent.Name, ClassIcon, ClassIcon);
                node.ToolTipText = BuildCLSIDToolTip(ent);
                node.Tag = ent;
                node.Nodes.Add("IUnknown");
                nodes.Add(node);
            }

            m_filter_types.Add(FilterType.CLSID);
            m_filter_types.Add(FilterType.Interface);
            treeComRegistry.Nodes.AddRange(nodes.OrderBy(n => n.Text).ToArray());
            Text = "CLSIDs by Name";
        }

        enum ServerType
        {
            None,
            Local,
            Surrogate,
            Proxies,
        }

        private bool IsProxyClsid(COMCLSIDEntry ent)
        {
            return ent.DefaultServerType == COMServerType.InProcServer32 && m_registry.GetProxiesForClsid(ent).Count() > 0;
        }

        private bool HasSurrogate(COMCLSIDEntry ent)
        {
            return m_registry.AppIDs.ContainsKey(ent.AppID) && !String.IsNullOrWhiteSpace(m_registry.AppIDs[ent.AppID].DllSurrogate);
        }

        private class COMCLSIDServerEqualityComparer : IEqualityComparer<COMCLSIDServerEntry>
        {
            public bool Equals(COMCLSIDServerEntry x, COMCLSIDServerEntry y)
            {
                return x.Server.Equals(y.Server, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(COMCLSIDServerEntry obj)
            {
                return obj.Server.GetHashCode();
            }
        }

        private void LoadCLSIDByServer(ServerType serverType)
        {
            IEnumerable<KeyValuePair<COMCLSIDServerEntry, List<COMCLSIDEntry>>> servers = null;

            if (serverType == ServerType.Surrogate)
            {
                servers = m_registry.Clsids.Values.Where(c => HasSurrogate(c))
                    .GroupBy(c => m_registry.AppIDs[c.AppID].DllSurrogate, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => new COMCLSIDServerEntry(COMServerType.LocalServer32, g.Key), g => g.AsEnumerable().ToList());
            }
            else
            {
                Dictionary<COMCLSIDServerEntry, List<COMCLSIDEntry>> dict = 
                    new Dictionary<COMCLSIDServerEntry, List<COMCLSIDEntry>>(new COMCLSIDServerEqualityComparer());
                IEnumerable<COMCLSIDEntry> clsids = m_registry.Clsids.Values.Where(e => e.Servers.Count > 0);
                if (serverType == ServerType.Proxies)
                {
                    clsids = clsids.Where(c => IsProxyClsid(c));
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

            switch (serverType)
            {
                case ServerType.Local:
                    Text = "CLSIDs by Local Server";
                    break;
                case ServerType.Surrogate:
                    Text = "CLSIDs With Surrogate";
                    break;
                case ServerType.None:
                    Text = "CLSIDs by Server";
                    break;
                case ServerType.Proxies:
                    Text = "Proxy CLSIDs By Server";
                    break;
            }

            List<TreeNode> serverNodes = new List<TreeNode>(m_registry.Clsids.Count);
            foreach (var pair in servers)
            {
                TreeNode node = new TreeNode(pair.Key.Server);
                node.ToolTipText = pair.Key.Server;
                node.Tag = pair.Key;
                node.Nodes.AddRange(pair.Value.OrderBy(c => c.Name).Select(c => CreateClsidNode(c)).ToArray());
                serverNodes.Add(node);
            }

            m_filter_types.Add(FilterType.CLSID);
            m_filter_types.Add(FilterType.Server);
            m_filter_types.Add(FilterType.Interface);
            treeComRegistry.Nodes.AddRange(serverNodes.OrderBy(n => n.Text).ToArray());
        }

        private void LoadInterfaces(bool by_name)
        {
            IEnumerable<TreeNode> intfs = null;
            if (by_name)
            {
                intfs = m_registry.Interfaces.Values.OrderBy(i => i.Name).Select(i => CreateInterfaceNameNode(i, null));
                Text = "Interfaces by Name";
            }
            else
            {
                intfs = m_registry.Interfaces.Values.Select(i => CreateInterfaceNode(i));
                Text = "Interfaces";
            }

            m_filter_types.Add(FilterType.Interface);
            treeComRegistry.BeginUpdate();
            treeComRegistry.Nodes.AddRange(intfs.ToArray());
            treeComRegistry.EndUpdate();
        }
        
        private static StringBuilder AppendFormatLine(StringBuilder builder, string format, params object[] ps)
        {
            return builder.AppendFormat(format, ps).AppendLine();
        }

        private TreeNode CreateClsidNode(COMCLSIDEntry ent)
        {
            TreeNode currNode = new TreeNode(ent.Name, ClassIcon, ClassIcon);
            currNode.ToolTipText = BuildCLSIDToolTip(ent);
            currNode.Tag = ent;
            currNode.Nodes.Add("IUnknown");

            return currNode;
        }

        private void LoadLocalServices()
        {
            List<IGrouping<Guid, COMCLSIDEntry>> clsidsByAppId = m_registry.ClsidsByAppId.ToList();
            IDictionary<Guid, COMAppIDEntry> appids = m_registry.AppIDs;

            List<TreeNode> serverNodes = new List<TreeNode>();
            foreach (IGrouping<Guid, COMCLSIDEntry> pair in clsidsByAppId)
            {   
                if(appids.ContainsKey(pair.Key) && appids[pair.Key].IsService)
                {
                    COMAppIDEntry appidEnt = appids[pair.Key];

                    string name = appidEnt.LocalService.DisplayName;
                    if (String.IsNullOrWhiteSpace(name))
                    {
                        name = appidEnt.LocalService.Name;
                    }
                    
                    TreeNode node = new TreeNode(name);
                    node.ToolTipText = BuildAppIdTooltip(appidEnt);
                    node.Tag = appidEnt;
                    
                    int count = pair.Count();

                    TreeNode[] clsidNodes = new TreeNode[count];
                    string[] nodeNames = new string[count];
                    int j = 0;

                    foreach(COMCLSIDEntry ent in pair)
                    {                        
                        clsidNodes[j] = CreateClsidNode(ent);
                        nodeNames[j] = ent.Name;
                        j++;
                    }

                    Array.Sort(nodeNames, clsidNodes);
                    node.Nodes.AddRange(clsidNodes);

                    serverNodes.Add(node);
                }
            }

            m_filter_types.Add(FilterType.AppID);
            m_filter_types.Add(FilterType.CLSID);
            m_filter_types.Add(FilterType.Interface);
            treeComRegistry.Nodes.AddRange(serverNodes.OrderBy(n => n.Text).ToArray());
            Text = "Local Services";
        }

        static string LimitString(string s, int max)
        {
            if (s.Length > max)
            {
                return s.Substring(0, max) + "...";
            }
            return s;
        }

        private static string BuildAppIdTooltip(COMAppIDEntry appidEnt)
        {
            StringBuilder builder = new StringBuilder();

            AppendFormatLine(builder, "AppID: {0}", appidEnt.AppId);
            if (!String.IsNullOrWhiteSpace(appidEnt.RunAs))
            {
                AppendFormatLine(builder, "RunAs: {0}", appidEnt.RunAs);
            }

            if (appidEnt.IsService)
            {
                COMAppIDServiceEntry service = appidEnt.LocalService;
                AppendFormatLine(builder, "Service Name: {0}", service.Name);
                if (!String.IsNullOrWhiteSpace(service.DisplayName))
                {
                    AppendFormatLine(builder, "Display Name: {0}", service.DisplayName);
                }
                if (!String.IsNullOrWhiteSpace(service.UserName))
                {
                    AppendFormatLine(builder, "Service User: {0}", service.UserName);
                }
                AppendFormatLine(builder, "Service Path: {0}", 
                    service.ServiceType == ServiceType.Win32ShareProcess ? service.ServiceDll : service.ImagePath);
            }

            if (appidEnt.HasLaunchPermission)
            {
                AppendFormatLine(builder, "Launch: {0}", LimitString(appidEnt.LaunchPermission, 64));
            }

            if (appidEnt.HasAccessPermission)
            {
                AppendFormatLine(builder, "Access: {0}", LimitString(appidEnt.AccessPermission, 64));
            }

            if (appidEnt.RotFlags != COMAppIDRotFlags.None)
            {
                AppendFormatLine(builder, "RotFlags: {0}", appidEnt.RotFlags);
            }

            if (!String.IsNullOrWhiteSpace(appidEnt.DllSurrogate))
            {
                AppendFormatLine(builder, "DLL Surrogate: {0}", appidEnt.DllSurrogate);
            }

            return builder.ToString();
        }

        private void LoadAppIDs(bool filterIL, bool filterAC)
        {
            List<IGrouping<Guid, COMCLSIDEntry>> clsidsByAppId = m_registry.ClsidsByAppId.ToList();
            IDictionary<Guid, COMAppIDEntry> appids = m_registry.AppIDs;            

            List<TreeNode> serverNodes = new List<TreeNode>();
            foreach (IGrouping<Guid, COMCLSIDEntry> pair in clsidsByAppId)
            {
                if (appids.ContainsKey(pair.Key))
                {
                    COMAppIDEntry appidEnt = appids[pair.Key];
                    
                    if (filterIL && COMSecurity.GetILForSD(appidEnt.AccessPermission) == SecurityIntegrityLevel.Medium &&
                        COMSecurity.GetILForSD(appidEnt.LaunchPermission) == SecurityIntegrityLevel.Medium)
                    {
                        continue;
                    }

                    if (filterAC && !COMSecurity.SDHasAC(appidEnt.AccessPermission) && !COMSecurity.SDHasAC(appidEnt.LaunchPermission))
                    {
                        continue;
                    }

                    TreeNode node = new TreeNode(appidEnt.Name);
                    node.Tag = appidEnt;
                    
                    node.ToolTipText = BuildAppIdTooltip(appidEnt);

                    int count = pair.Count();

                    TreeNode[] clsidNodes = new TreeNode[count];
                    string[] nodeNames = new string[count];
                    int j = 0;

                    foreach (COMCLSIDEntry ent in pair)
                    {
                        clsidNodes[j] = CreateClsidNode(ent);
                        nodeNames[j] = ent.Name;
                        j++;
                    }

                    Array.Sort(nodeNames, clsidNodes);
                    node.Nodes.AddRange(clsidNodes);

                    serverNodes.Add(node);
                }
            }

            m_filter_types.Add(FilterType.AppID);
            m_filter_types.Add(FilterType.CLSID);
            m_filter_types.Add(FilterType.Interface);
            treeComRegistry.Nodes.AddRange(serverNodes.OrderBy(n => n.Text).ToArray());
            string text = "AppIDs";
            if (filterIL)
            {
                text += " with Low IL";
            }
            if (filterAC)
            {
                text += " with AC";
            }
            Text = text;
        }

        private void LoadImplementedCategories()
        {
            int i = 0;
            SortedDictionary<string, TreeNode> sortedNodes = new SortedDictionary<string, TreeNode>();

            foreach (var pair in m_registry.ImplementedCategories.Values)
            {
                TreeNode currNode = new TreeNode(pair.Name);
                currNode.Tag = pair;
                currNode.ToolTipText = String.Format("CATID: {0}", pair.CategoryID.ToString("B"));
                sortedNodes.Add(currNode.Text, currNode);

                IEnumerable<COMCLSIDEntry> clsids = pair.Clsids.Select(c => m_registry.MapClsidToEntry(c)).Where(c => c != null).OrderBy(c => c.Name);

                IEnumerable<TreeNode> clsidNodes = clsids.Select(n => CreateClsidNode(n));
                currNode.Nodes.AddRange(clsidNodes.ToArray());
            }

            TreeNode[] catNodes = new TreeNode[sortedNodes.Count];
            i = 0;
            foreach (KeyValuePair<string, TreeNode> pair in sortedNodes)
            {
                catNodes[i++] = pair.Value;
            }

            m_filter_types.Add(FilterType.Category);
            m_filter_types.Add(FilterType.CLSID);
            treeComRegistry.Nodes.AddRange(catNodes);
            Text = "Implemented Categories";
        }

        private void LoadPreApproved()
        {
            List<TreeNode> nodes = new List<TreeNode>();
            foreach (COMCLSIDEntry ent in m_registry.PreApproved)
            {
                nodes.Add(CreateCLSIDNode(ent));
            }

            m_filter_types.Add(FilterType.CLSID);
            m_filter_types.Add(FilterType.Interface);
            treeComRegistry.Nodes.AddRange(nodes.ToArray());
            Text = "Explorer PreApproved";   
        }

        private void LoadIELowRights()
        {
            List<TreeNode> clsidNodes = new List<TreeNode>();
            foreach (COMIELowRightsElevationPolicy ent in m_registry.LowRights)
            {
                StringBuilder tooltip = new StringBuilder();
                List<COMCLSIDEntry> clsids = new List<COMCLSIDEntry>();
                if (ent.Clsid != Guid.Empty)
                {
                    clsids.Add(m_registry.MapClsidToEntry(ent.Clsid));
                }

                if (!String.IsNullOrWhiteSpace(ent.AppPath) && m_registry.ClsidsByServer.ContainsKey(ent.AppPath))
                {
                    clsids.AddRange(m_registry.ClsidsByServer[ent.AppPath]);
                    tooltip.AppendFormat("{0}", ent.AppPath);
                    tooltip.AppendLine();
                }

                if (clsids.Count == 0)
                {
                    continue;
                }

                TreeNode currNode = new TreeNode(ent.Name);
                clsidNodes.Add(currNode);

                foreach (COMCLSIDEntry cls in clsids)
                {
                    currNode.Nodes.Add(CreateCLSIDNode(cls));
                }

                tooltip.AppendFormat("Policy: {0}", ent.Policy);
                tooltip.AppendLine();
                currNode.ToolTipText = tooltip.ToString();
            }

            m_filter_types.Add(FilterType.LowRights);
            m_filter_types.Add(FilterType.CLSID);
            m_filter_types.Add(FilterType.Interface);
            treeComRegistry.Nodes.AddRange(clsidNodes.ToArray());
            
            Text = "IE Low Rights Elevation Policy"; 
        }

        private void LoadMimeTypes()
        {
            List<TreeNode> nodes = new List<TreeNode>(m_registry.MimeTypes.Count());
            foreach (COMMimeType ent in m_registry.MimeTypes)
            {
                TreeNode node = new TreeNode(ent.MimeType);
                if (m_registry.Clsids.ContainsKey(ent.Clsid))
                {
                    node.Nodes.Add(CreateCLSIDNode(m_registry.Clsids[ent.Clsid]));
                }

                if (!String.IsNullOrWhiteSpace(ent.Extension))
                {
                    node.ToolTipText = String.Format("Extension {0}", ent.Extension);
                }
                node.Tag = ent;
                nodes.Add(node);
            }

            m_filter_types.Add(FilterType.MimeType);
            m_filter_types.Add(FilterType.CLSID);
            m_filter_types.Add(FilterType.Interface);
            treeComRegistry.Nodes.AddRange(nodes.ToArray());
            Text = "MIME Types";
        }

        private TreeNode CreateTypelibVersionNode(COMTypeLibVersionEntry entry)
        {
            TreeNode node = new TreeNode(String.Format("{0} : Version {1}", entry.Name, entry.Version), 
                ClassIcon, ClassIcon);

            node.Tag = entry;
            List<string> entries = new List<string>();
            if(!String.IsNullOrWhiteSpace(entry.Win32Path))
            {
                entries.Add(String.Format("Win32: {0}", entry.Win32Path));
            }
            if(!String.IsNullOrWhiteSpace(entry.Win64Path))
            {
                entries.Add(String.Format("Win64: {0}", entry.Win64Path));
            }
            node.ToolTipText = String.Join("\r\n", entries);

            return node;
        }

        private void LoadTypeLibs()
        {
            int i = 0;
            TreeNode[] typelibNodes = new TreeNode[m_registry.Typelibs.Values.Count];
            foreach (COMTypeLibEntry ent in m_registry.Typelibs.Values)
            {
                typelibNodes[i] = new TreeNode(ent.TypelibId.ToString());
                foreach (COMTypeLibVersionEntry ver in ent.Versions)
                {
                    typelibNodes[i].Nodes.Add(CreateTypelibVersionNode(ver));
                }
                i++;
            }

            m_filter_types.Add(FilterType.TypeLib);
            treeComRegistry.Nodes.AddRange(typelibNodes);
            Text = "Type Libraries"; 
        }

        private async Task SetupCLSIDNodeTree(TreeNode node, bool bRefresh)
        {
            COMCLSIDEntry clsid = null;

            if (node.Tag is COMCLSIDEntry)
            {
                clsid = (COMCLSIDEntry)node.Tag;

            }
            else if (node.Tag is COMProgIDEntry)
            {
                clsid = m_registry.MapClsidToEntry(((COMProgIDEntry)node.Tag).Clsid);
            }

            if (clsid != null)
            {
                node.Nodes.Clear();
                TreeNode wait_node = new TreeNode("Please Wait, Populating Interfaces", InterfaceIcon, InterfaceIcon);
                node.Nodes.Add(wait_node);
                try
                {
                    await clsid.LoadSupportedInterfacesAsync(bRefresh);
                    if (clsid.Interfaces.Count() > 0)
                    {
                        node.Nodes.Remove(wait_node);
                        foreach (COMInterfaceInstance ent in clsid.Interfaces)
                        {
                            node.Nodes.Add(CreateInterfaceNameNode(m_registry.MapIidToInterface(ent.Iid), ent));
                        }
                    }
                    else
                    {
                        if (clsid.FactoryInterfaces.Count() > 0)
                        {
                            wait_node.Text = "Error querying COM interfaces - No Instance Interfaces";
                        }
                        else
                        {
                            wait_node.Text = "Error querying COM interfaces - Timeout";
                        }
                    }
                }
                catch (Win32Exception ex)
                {
                    wait_node.Text = String.Format("Error querying COM interfaces - {0}", ex.Message);
                }
            }
        }

        private async void treeComRegistry_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {            
            Cursor currCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            await SetupCLSIDNodeTree(e.Node, false);

            Cursor.Current = currCursor;
        }

        public enum CopyGuidType
        {
            CopyAsString,
            CopyAsStructure,
            CopyAsObject,
            CopyAsHexString,            
        }

        public static void CopyTextToClipboard(string text)
        {
            int tries = 10;
            while (tries > 0)
            {
                try
                {
                    Clipboard.SetText(text);
                    break;
                }
                catch (ExternalException)
                {
                }
                System.Threading.Thread.Sleep(100);
                tries--;
            }
        }

        public static void CopyGuidToClipboard(Guid guid, CopyGuidType copyType)
        {
            string strCopy = null;

            switch (copyType)
            {
                case CopyGuidType.CopyAsObject:
                    strCopy = String.Format("<object id=\"obj\" classid=\"clsid:{0}\">NO OBJECT</object>",
                        guid.ToString());
                    break;
                case CopyGuidType.CopyAsString:
                    strCopy = guid.ToString("B");
                    break;
                case CopyGuidType.CopyAsStructure:
                    {
                        MemoryStream ms = new MemoryStream(guid.ToByteArray());
                        BinaryReader reader = new BinaryReader(ms);
                        strCopy = "struct GUID guidObject = { ";
                        strCopy += String.Format("0x{0:X08}, 0x{1:X04}, 0x{2:X04}, {{", reader.ReadUInt32(),
                            reader.ReadUInt16(), reader.ReadUInt16());
                        for (int i = 0; i < 8; i++)
                        {
                            strCopy += String.Format("0x{0:X02}, ", reader.ReadByte());
                        }
                        strCopy += "}};";
                    }
                    break;
                case CopyGuidType.CopyAsHexString:
                    {
                        byte[] data = guid.ToByteArray();
                        strCopy = String.Join(" ", data.Select(b => String.Format("{0:X02}", b)));                        
                    }
                    break;                
            }

            if (strCopy != null)
            {
                CopyTextToClipboard(strCopy);
            }
        }

        private Guid GetGuidFromType(TreeNode node)
        {
            Guid guid = Guid.Empty;
            if (node != null)
            {
                object tag = node.Tag;
                if (tag is COMCLSIDEntry)
                {
                    guid = ((COMCLSIDEntry)tag).Clsid;
                }
                else if (tag is COMInterfaceEntry)
                {
                    guid = ((COMInterfaceEntry)tag).Iid;
                }
                else if (tag is COMProgIDEntry)
                {
                    COMProgIDEntry ent = (COMProgIDEntry)tag;                    
                    guid = ent.Clsid;
                }
                else if (tag is COMTypeLibEntry)
                {
                    guid = ((COMTypeLibEntry)tag).TypelibId;
                }
                else if (tag is Guid)
                {
                    guid = (Guid)tag;
                }
                else if (tag is COMAppIDEntry)
                {
                    guid = ((COMAppIDEntry)tag).AppId;
                }
            }

            return guid;
        }

        private void copyGUIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Guid guid = GetGuidFromType(treeComRegistry.SelectedNode);

            if (guid != Guid.Empty)
            {
                CopyGuidToClipboard(guid, CopyGuidType.CopyAsString);
            }
        }

        private void copyGUIDCStructureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Guid guid = GetGuidFromType(treeComRegistry.SelectedNode);

            if (guid != Guid.Empty)
            {
                CopyGuidToClipboard(guid, CopyGuidType.CopyAsStructure);
            }
        }

        private void copyGUIDHexStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Guid guid = GetGuidFromType(treeComRegistry.SelectedNode);

            if (guid != Guid.Empty)
            {
                CopyGuidToClipboard(guid, CopyGuidType.CopyAsHexString);
            }
        }

        private void copyObjectTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeComRegistry.SelectedNode;
            Guid guid = Guid.Empty;

            if (node != null)
            {
                if (node.Tag is COMCLSIDEntry)
                {
                    guid = ((COMCLSIDEntry)node.Tag).Clsid;
                }
                else if (node.Tag is COMProgIDEntry)
                {
                    COMProgIDEntry ent = (COMProgIDEntry)node.Tag;
                    guid = ent.Clsid;
                }

                if (guid != Guid.Empty)
                {
                    CopyGuidToClipboard(guid, CopyGuidType.CopyAsObject);
                }
            }
        }

        private async Task SetupObjectView(COMCLSIDEntry ent, object obj)
        {
            await Program.GetMainForm(m_registry).HostObject(ent, obj);
        }

        private COMCLSIDEntry GetSelectedClsidEntry()
        {
            COMCLSIDEntry ent = null;
            TreeNode node = treeComRegistry.SelectedNode;
            if (node != null)
            {
                if (node.Tag is COMCLSIDEntry)
                {
                    ent = (COMCLSIDEntry)node.Tag;
                }
                else if (node.Tag is COMProgIDEntry)
                {
                    ent = m_registry.MapClsidToEntry(((COMProgIDEntry)node.Tag).Clsid);
                }
            }
            return ent;
        }

        private async Task CreateInstance(CLSCTX clsctx)
        {
            COMCLSIDEntry ent = GetSelectedClsidEntry();
            if (ent != null)
            {
                try
                {
                    object comObj = ent.CreateInstanceAsObject(clsctx);
                    if (comObj != null)
                    {
                        await SetupObjectView(ent, comObj);
                    }
                }
                catch (Exception ex)
                {
                    Program.ShowError(this, ex);
                }
            }
        }

        private async Task CreateClassFactory()
        {
            COMCLSIDEntry ent = GetSelectedClsidEntry();
            if (ent != null)
            {
                try
                {
                    object comObj = ent.CreateClassFactory();
                    if (comObj != null)
                    {
                        await SetupObjectView(ent, comObj);
                    }
                }
                catch (Exception ex)
                {
                    Program.ShowError(this, ex);
                }
            }
        }

        private async void createInstanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await CreateInstance(CLSCTX.CLSCTX_ALL);
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
                ToolStripMenuItem item = new ToolStripMenuItem(session_id.ToString());
                item.Tag = session_id.ToString();
                item.Click += consoleToolStripMenuItem_Click;
                createInSessionToolStripMenuItem.DropDownItems.Add(item);
            }
            createSpecialToolStripMenuItem.DropDownItems.Add(createInSessionToolStripMenuItem);
        }

        private static bool HasServerType(COMCLSIDEntry clsid, COMServerType type)
        {
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
                contextMenuStrip.Items.Add(copyGUIDToolStripMenuItem);
                contextMenuStrip.Items.Add(copyGUIDHexStringToolStripMenuItem);
                contextMenuStrip.Items.Add(copyGUIDCStructureToolStripMenuItem);
                if ((node.Tag is COMCLSIDEntry) || (node.Tag is COMProgIDEntry))
                {
                    contextMenuStrip.Items.Add(copyObjectTagToolStripMenuItem);
                    contextMenuStrip.Items.Add(createInstanceToolStripMenuItem);

                    COMProgIDEntry progid = node.Tag as COMProgIDEntry;
                    COMCLSIDEntry clsid = node.Tag as COMCLSIDEntry;
                    if (progid != null && m_registry.Clsids.ContainsKey(progid.Clsid))
                    {
                        clsid = m_registry.MapClsidToEntry(progid.Clsid);
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
                        createSpecialToolStripMenuItem.DropDownItems.Add(createElevatedToolStripMenuItem);
                    }

                    createSpecialToolStripMenuItem.DropDownItems.Add(createClassFactoryToolStripMenuItem);

                    contextMenuStrip.Items.Add(createSpecialToolStripMenuItem);
                    
                    contextMenuStrip.Items.Add(refreshInterfacesToolStripMenuItem);

                    if (clsid != null && m_registry.Typelibs.ContainsKey(clsid.TypeLib))
                    {
                        contextMenuStrip.Items.Add(viewTypeLibraryToolStripMenuItem);
                    }

                    if (clsid != null && m_registry.GetProxiesForClsid(clsid).Length > 0)
                    {
                        contextMenuStrip.Items.Add(viewProxyDefinitionToolStripMenuItem);
                    }

                    if (clsid != null && m_registry.AppIDs.ContainsKey(clsid.AppID))
                    {
                        EnableViewPermissions(m_registry.AppIDs[clsid.AppID]);
                    }
                }
                else if (node.Tag is COMTypeLibVersionEntry)
                {
                    contextMenuStrip.Items.Add(viewTypeLibraryToolStripMenuItem);
                }
                else if (node.Tag is COMAppIDEntry)
                {
                    EnableViewPermissions((COMAppIDEntry)node.Tag);
                }
                else if (node.Tag is COMInterfaceEntry)
                {
                    COMInterfaceEntry intf = (COMInterfaceEntry)node.Tag;
                    if (intf.HasTypeLib)
                    {
                        contextMenuStrip.Items.Add(viewTypeLibraryToolStripMenuItem);
                    }

                    if (intf.HasProxy && m_registry.Clsids.ContainsKey(intf.ProxyClsid))
                    {
                        contextMenuStrip.Items.Add(viewProxyDefinitionToolStripMenuItem);
                    }
                }

                if (m_filter_types.Contains(FilterType.CLSID))
                {
                    contextMenuStrip.Items.Add(queryAllInterfacesToolStripMenuItem);
                }

                if (PropertiesControl.SupportsProperties(node.Tag))
                {
                    contextMenuStrip.Items.Add(propertiesToolStripMenuItem);
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private async void refreshInterfacesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeComRegistry.SelectedNode;
            if ((node != null) && (node.Tag != null))
            {
                await SetupCLSIDNodeTree(node, true);
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
            StringBuilder builder = new StringBuilder();

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
                    builder.Append(Regex.Escape(new String(ch, 1)));
                }
            }

            builder.Append("$");

            return new Regex(builder.ToString(), ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        private static Func<object, bool> CreatePythonFilter(string filter)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("from OleViewDotNet import *");
            builder.AppendLine("def run_filter(entry):");
            builder.AppendFormat("  return {0}", filter);
            builder.AppendLine();

            ScriptEngine engine = Python.CreateEngine();
            ScriptSource source = engine.CreateScriptSourceFromString(builder.ToString(), SourceCodeKind.File);
            ScriptScope scope = engine.CreateScope();
            scope.Engine.Runtime.LoadAssembly(typeof(COMCLSIDEntry).Assembly);
            source.Execute(scope);
            return scope.GetVariable<Func<object, bool>>("run_filter");            
        }

        private static bool RunPythonFilter(TreeNode node, Func<object, bool> python_filter)
        {
            try
            {
                return python_filter(node.Tag);
            }
            catch 
            {
                return false;
            }
        }

        private static FilterResult RunComplexFilter(TreeNode node, RegistryViewerFilter filter)
        {
            try
            {
                return filter.Filter(node.Tag);
            }
            catch
            {
                return FilterResult.None;
            }
        }

        private enum FilterMode
        {
            Contains,
            BeginsWith,
            EndsWith,
            Equals,
            Glob,
            Regex,
            Python,
            Complex,
        }

        private static Func<TreeNode, bool> CreateFilter(string filter, FilterMode mode, bool caseSensitive)
        {                        
            StringComparison comp;

            filter = filter.Trim();
            if (String.IsNullOrEmpty(filter))
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
                        Regex r = new Regex(filter, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);

                        return n => r.IsMatch(n.Text);
                    }
                case FilterMode.Python:
                    {
                        Func<object, bool> python_filter = CreatePythonFilter(filter);

                        return n => RunPythonFilter(n, python_filter);
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
            try
            {
                TreeNode[] nodes = null;
                Func<TreeNode, FilterResult> filterFunc = null;
                FilterMode mode = (FilterMode)comboBoxMode.SelectedItem;
                if (mode == FilterMode.Complex)
                {
                    using (ViewFilterForm form = new ViewFilterForm(m_filter, m_filter_types))
                    {
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            m_filter = form.Filter;
                            if (m_filter.Filters.Count > 0)
                            {
                                filterFunc = n => RunComplexFilter(n, m_filter);
                            }
                        }
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
                    nodes = await Task.Run(() => m_originalNodes.Where(n => FilterNode(n, filterFunc) == FilterResult.Include).ToArray());
                }
                else
                {
                    nodes = m_originalNodes;
                }

                treeComRegistry.SuspendLayout();
                treeComRegistry.Nodes.Clear();
                treeComRegistry.Nodes.AddRange(nodes);
                treeComRegistry.ResumeLayout();
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
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

            if (node != null)
            {
                COMTypeLibVersionEntry ent = node.Tag as COMTypeLibVersionEntry;
                Guid selected_guid = Guid.Empty;

                if (ent == null)
                {
                    COMCLSIDEntry clsid = node.Tag as COMCLSIDEntry;
                    COMProgIDEntry progid = node.Tag as COMProgIDEntry;
                    COMInterfaceEntry intf = node.Tag as COMInterfaceEntry;
                    if(progid != null)
                    {
                        clsid = m_registry.MapClsidToEntry(progid.Clsid);
                    }

                    if(clsid != null && m_registry.Typelibs.ContainsKey(clsid.TypeLib))
                    {
                        ent = m_registry.Typelibs[clsid.TypeLib].Versions.First();
                        selected_guid = clsid.Clsid;
                    }

                    if (intf != null && m_registry.Typelibs.ContainsKey(intf.TypeLib))
                    {
                        ent = m_registry.GetTypeLibVersionEntry(intf.TypeLib, intf.TypeLibVersion);
                        selected_guid = intf.Iid;
                    }
                }
                
                if(ent != null)
                {
                    Assembly typelib = COMUtilities.LoadTypeLib(this, ent.NativePath);
                    if (typelib != null)
                    {
                        Program.GetMainForm(m_registry).HostControl(new TypeLibControl(ent.Name, typelib, selected_guid));
                    }
                }
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeComRegistry.SelectedNode;
            if (node != null)
            {
                Program.GetMainForm(m_registry).HostControl(new PropertiesControl(m_registry, node.Text, node.Tag));
            }
        }

        private void ViewPermissions(bool access)
        {
            TreeNode node = treeComRegistry.SelectedNode;
            if (node != null)
            {
                COMAppIDEntry appid = node.Tag as COMAppIDEntry;
                if (appid == null)
                {
                    COMCLSIDEntry clsid = node.Tag as COMCLSIDEntry;
                    if (clsid != null && m_registry.AppIDs.ContainsKey(clsid.AppID))
                    {
                        appid = m_registry.AppIDs[clsid.AppID];
                    }
                }

                if (appid != null)
                {
                    COMSecurity.ViewSecurity(this, appid, access);
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
            await CreateInstance(CLSCTX.CLSCTX_LOCAL_SERVER);
        }

        private async void createInProcServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await CreateInstance(CLSCTX.CLSCTX_INPROC_SERVER);
        }

        private async Task CreateFromMoniker(COMCLSIDEntry ent, string moniker)
        {
            try
            {
                object obj = Marshal.BindToMoniker(moniker);
                await SetupObjectView(ent, obj);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CreateInSession(COMCLSIDEntry ent, string session_id)
        {
            await CreateFromMoniker(ent, String.Format("session:{0}!new:{1}", session_id, ent.Clsid));
        }

        private async Task CreateElevated(COMCLSIDEntry ent, bool factory)
        {
            await CreateFromMoniker(ent, String.Format("Elevation:Administrator!{0}:{1}", 
                factory ? "clsid" : "new", ent.Clsid));
        }

        private async void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            COMCLSIDEntry ent = GetSelectedClsidEntry();
            if (ent != null && item != null && item.Tag is string)
            {
                await CreateInSession(ent, (string)item.Tag);
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeComRegistry.SelectedNode;
            if (node != null)
            {
                CopyTextToClipboard(node.Text);
            }
        }

        private void viewProxyDefinitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeComRegistry.SelectedNode;
            if (node != null)
            {
                COMCLSIDEntry clsid = node.Tag as COMCLSIDEntry;
                Guid selected_iid = Guid.Empty;
                if (clsid == null && node.Tag is COMInterfaceEntry)
                {
                    COMInterfaceEntry intf = (COMInterfaceEntry)node.Tag;
                    selected_iid = intf.Iid;
                    clsid = m_registry.Clsids[intf.ProxyClsid];
                }

                if (clsid != null)
                {
                    try
                    {
                        Program.GetMainForm(m_registry).HostControl(new TypeLibControl(m_registry, 
                            clsid.Name, COMProxyInstance.GetFromCLSID(clsid), selected_iid));
                    }
                    catch (Exception ex)
                    {
                        Program.ShowError(this, ex);
                    }
                }
            }
        }

        private async void createClassFactoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await CreateClassFactory();
        }

        private void GetClsidsFromNodes(List<COMCLSIDEntry> clsids, TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is COMCLSIDEntry)
                {
                    clsids.Add((COMCLSIDEntry)node.Tag);
                }

                if (node.Nodes.Count > 0)
                {
                    GetClsidsFromNodes(clsids, node.Nodes);
                }
            }
        }

        private void queryAllInterfacesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (QueryInterfacesOptionsForm options = new QueryInterfacesOptionsForm())
            {
                if (options.ShowDialog(this) == DialogResult.OK)
                {
                    List<COMCLSIDEntry> clsids = new List<COMCLSIDEntry>();
                    GetClsidsFromNodes(clsids, treeComRegistry.Nodes);
                    if (clsids.Count > 0)
                    {
                        COMUtilities.QueryAllInterfaces(this, clsids,
                            options.ServerTypes, options.ConcurrentQueries,
                            options.RefreshInterfaces);
                    }
                }
            }
        }

        private async void createInProcHandlerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await CreateInstance(CLSCTX.CLSCTX_INPROC_HANDLER);
        }

        private async void instanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            COMCLSIDEntry clsid = GetSelectedClsidEntry();
            if (clsid != null)
            {
                await CreateElevated(clsid, false);
            }
        }

        private async void classFactoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            COMCLSIDEntry clsid = GetSelectedClsidEntry();
            if (clsid != null)
            {
                await CreateElevated(clsid, true);
            }
        }
    }
}
