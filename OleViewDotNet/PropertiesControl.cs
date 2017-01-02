//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
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
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class PropertiesControl : UserControl
    {
        private COMRegistry m_registry;
        private COMAppIDEntry m_appid;
        private COMCLSIDEntry m_clsid;
        private COMInterfaceEntry m_interface;
        private COMTypeLibVersionEntry m_typelib;

        private void LoadInterfaceList(IEnumerable<COMInterfaceInstance> entries, ListView view)
        {
            view.Items.Clear();
            foreach (Tuple<COMInterfaceInstance, COMInterfaceEntry> entry in 
                entries.Select(e => new Tuple<COMInterfaceInstance, COMInterfaceEntry>(e, m_registry.MapIidToInterface(e.Iid))).OrderBy(e => e.Item2.Name))
            {
                ListViewItem item = view.Items.Add(entry.Item2.Name);
                item.SubItems.Add(entry.Item1.Iid.ToString("B"));
                item.SubItems.Add(entry.Item2.NumMethods.ToString());
                if (!String.IsNullOrWhiteSpace(entry.Item1.Module))
                {
                    item.SubItems.Add(String.Format("{0}+0x{1:X}", 
                        entry.Item1.Module, entry.Item1.VTableOffset));
                }
                item.Tag = entry;
            }
            view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private static string GetStringValue(string value)
        {
            return String.IsNullOrWhiteSpace(value) ? "N/A" : value;
        }

        private static string GetGuidValue(Guid guid)
        {
            return guid == Guid.Empty ? "N/A" : guid.ToString("B");
        }

        private void SetupAppIdEntry(COMAppIDEntry entry)
        {
            textBoxAppIdName.Text = entry.Name;
            textBoxAppIdGuid.Text = entry.AppId.ToString("B");
            textBoxLaunchPermission.Text = entry.LaunchPermission;
            textBoxAccessPermission.Text = entry.AccessPermission;
            textBoxAppIDRunAs.Text = GetStringValue(entry.RunAs);
            textBoxAppIDService.Text = GetStringValue(entry.IsService ? entry.LocalService.Name : null);
            textBoxAppIDFlags.Text = entry.Flags.ToString();
            textBoxDllSurrogate.Text = GetStringValue(entry.DllSurrogate);
            btnViewAccessPermissions.Enabled = entry.HasAccessPermission;
            btnViewLaunchPermissions.Enabled = entry.HasLaunchPermission;
            tabControlProperties.TabPages.Add(tabPageAppID);

            if (entry.IsService)
            {
                textBoxServiceName.Text = entry.LocalService.Name;
                textBoxServiceDisplayName.Text = GetStringValue(entry.LocalService.DisplayName);
                textBoxServiceType.Text = entry.LocalService.ServiceType.ToString();
                textBoxServiceImagePath.Text = entry.LocalService.ImagePath;
                textBoxServiceDll.Text = GetStringValue(entry.LocalService.ServiceDll);
                textBoxServiceUserName.Text = GetStringValue(entry.LocalService.UserName);
                tabControlProperties.TabPages.Add(tabPageService);
            }

            m_appid = entry;
        }

        private void SetupClsidEntry(COMCLSIDEntry entry)
        {
            textBoxClsidName.Text = entry.Name;
            textBoxClsid.Text = entry.Clsid.ToString("B");
            textBoxServerType.Text = entry.DefaultServerType.ToString();
            textBoxThreadingModel.Text = entry.DefaultThreadingModel.ToString();
            textBoxServer.Text = entry.DefaultServer;
            textBoxCmdLine.Text = GetStringValue(entry.DefaultCmdLine);
            textBoxTreatAs.Text = GetGuidValue(entry.TreatAs);
            btnTreatAsProps.Enabled = m_registry.Clsids.ContainsKey(entry.TreatAs);
            var progids = m_registry.Progids;

            foreach (string progid in m_registry.GetProgIdsForClsid(entry.Clsid).Select(p => p.ProgID))
            {
                ListViewItem item = listViewProgIDs.Items.Add(progid);
                if (progids.ContainsKey(progid))
                {
                    item.Tag = progids[progid];
                }
            }
            listViewProgIDs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            foreach (Guid catid in entry.Categories)
            {
                ListViewItem item = listViewCategories.Items.Add(COMUtilities.GetCategoryName(catid));
                item.Tag = catid;
            }
            listViewCategories.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            LoadInterfaceList(entry.Interfaces, listViewInterfaces);
            LoadInterfaceList(entry.FactoryInterfaces, listViewFactoryInterfaces);
            tabPageSupportedInterfaces.Tag = entry;

            tabControlProperties.TabPages.Add(tabPageClsid);
            tabControlProperties.TabPages.Add(tabPageSupportedInterfaces);
            if (m_registry.AppIDs.ContainsKey(entry.AppID))
            {
                SetupAppIdEntry(m_registry.AppIDs[entry.AppID]);
            }

            IEnumerable<COMInterfaceEntry> proxies = m_registry.GetProxiesForClsid(entry);
            if (proxies.Count() > 0)
            {
                foreach (COMInterfaceEntry intf in proxies.OrderBy(i => i.Name))
                {
                    ListViewItem item = listViewProxies.Items.Add(intf.Name);
                    item.SubItems.Add(intf.Iid.ToString("B"));
                }
                listViewProxies.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listViewProxies.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                tabControlProperties.TabPages.Add(tabPageProxies);
            }

            if (entry.Servers.Count > 1)
            {
                foreach (COMCLSIDServerEntry server in entry.Servers.Values)
                {
                    ListViewItem item = listViewCLSIDServers.Items.Add(server.ServerType.ToString());
                    item.SubItems.Add(server.Server);
                    item.SubItems.Add(server.CommandLine);
                    item.SubItems.Add(server.ThreadingModel.ToString());
                }

                listViewCLSIDServers.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listViewCLSIDServers.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                tabControlProperties.TabPages.Add(tabPageServers);
            }

            m_clsid = entry;
        }

        private void SetupInterfaceEntry(COMInterfaceEntry entry)
        {
            textBoxInterfaceName.Text = entry.Name;
            textBoxIID.Text = GetGuidValue(entry.Iid);
            textBoxInterfaceBase.Text = GetStringValue(entry.Base);
            textBoxInterfaceProxy.Text = GetGuidValue(entry.ProxyClsid);
            btnProxyProperties.Enabled = m_registry.Clsids.ContainsKey(entry.ProxyClsid);
            textBoxTypeLib.Text = GetGuidValue(entry.TypeLib);
            m_typelib = m_registry.GetTypeLibVersionEntry(entry.TypeLib, entry.TypeLibVersion);
            btnOpenTypeLib.Enabled = m_typelib != null;            
            tabControlProperties.TabPages.Add(tabPageInterface);
            m_interface = entry;
        }

        private void SetupProperties(object obj)
        {
            if (obj is COMCLSIDEntry)
            {
                SetupClsidEntry((COMCLSIDEntry)obj);
            }

            if (obj is COMProgIDEntry)
            {
                COMProgIDEntry entry = (COMProgIDEntry)obj;
                COMCLSIDEntry clsid_entry = m_registry.MapClsidToEntry(entry.Clsid);
                SetupClsidEntry(clsid_entry);
            }

            if (obj is COMAppIDEntry)
            {
                SetupAppIdEntry((COMAppIDEntry)obj);
            }

            if (obj is COMInterfaceEntry)
            {
                SetupInterfaceEntry((COMInterfaceEntry)obj);
            }
        }

        public static bool SupportsProperties(object obj)
        {
            return obj is COMCLSIDEntry || obj is COMProgIDEntry || obj is COMAppIDEntry || obj is COMInterfaceEntry;
        }

        public PropertiesControl(COMRegistry registry, string name, object obj)
        {
            m_registry = registry;
            InitializeComponent();
            listViewCategories.Columns.Add("Name", 100);
            listViewProgIDs.Columns.Add("Name", 100);
            listViewInterfaces.Columns.Add("Name", 100);
            listViewInterfaces.Columns.Add("IID", 100);
            listViewInterfaces.Columns.Add("Methods", 100);
            listViewInterfaces.Columns.Add("VTable Offset", 100);
            listViewFactoryInterfaces.Columns.Add("Name", 100);
            listViewFactoryInterfaces.Columns.Add("IID", 100);
            listViewFactoryInterfaces.Columns.Add("Methods", 100);
            listViewFactoryInterfaces.Columns.Add("VTable Offset", 100);
            tabControlProperties.TabPages.Clear();
            SetupProperties(obj);
            if (tabControlProperties.TabCount == 0)
            {
                tabControlProperties.TabPages.Add(tabPageNoProperties);
            }
            this.Text = String.Format("{0} Properties", name.Replace("&", "&&"));
        }

        private async void btnRefreshInterfaces_Click(object sender, EventArgs e)
        {
            try
            {
                COMCLSIDEntry entry = (COMCLSIDEntry)tabPageSupportedInterfaces.Tag;
                await entry.LoadSupportedInterfacesAsync(true);
                LoadInterfaceList(entry.Interfaces, listViewInterfaces);                
                LoadInterfaceList(entry.FactoryInterfaces, listViewFactoryInterfaces);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnViewLaunchPermissions_Click(object sender, EventArgs e)
        {
            COMSecurity.ViewSecurity(this, m_appid, false);
        }

        private void btnViewAccessPermissions_Click(object sender, EventArgs e)
        {
            COMSecurity.ViewSecurity(this, m_appid, true);
        }

        private void copyProgIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewProgIDs.SelectedItems.Count > 0)
            {
                ListViewItem item = listViewProgIDs.SelectedItems[0];
                COMRegistryViewer.CopyTextToClipboard(item.Text);
            }
        }

        private void btnTreatAsProps_Click(object sender, EventArgs e)
        {
            if (m_registry.Clsids.ContainsKey(m_clsid.TreatAs))
            {
                Program.GetMainForm(m_registry).HostControl(new PropertiesControl(m_registry, 
                    m_clsid.Name, m_registry.Clsids[m_clsid.TreatAs]));
            }
        }

        private async void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                object comObj = m_clsid.CreateInstanceAsObject(m_clsid.CreateContext, null);
                if (comObj != null)
                {
                    await Program.GetMainForm(m_registry).HostObject(m_clsid, comObj, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnProxyProperties_Click(object sender, EventArgs e)
        {
            if (m_registry.Clsids.ContainsKey(m_interface.ProxyClsid))
            {
                COMCLSIDEntry entry = m_registry.Clsids[m_interface.ProxyClsid];
                Program.GetMainForm(m_registry).HostControl(new PropertiesControl(m_registry,
                    entry.Name, entry));
            }
        }

        private void btnOpenTypeLib_Click(object sender, EventArgs e)
        {
            if (m_typelib != null)
            {
                Assembly typelib = COMUtilities.LoadTypeLib(this, m_typelib.NativePath);
                if (typelib != null)
                {
                    Program.GetMainForm(m_registry).HostControl(new TypeLibControl(m_typelib.Name, typelib, m_interface.Iid));
                }
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView view = sender as ListView;
            if (view != null && view.SelectedIndices.Count > 0)
            {
                ListViewItem item = view.SelectedItems[0];
                COMRegistryViewer.CopyTextToClipboard(item.Text);
            }
        }

        private void CopyIID(ListView view, COMRegistryViewer.CopyGuidType type)
        {
            if (view != null && view.SelectedIndices.Count > 0)
            {
                ListViewItem item = view.SelectedItems[0];
                Tuple<COMInterfaceInstance, COMInterfaceEntry> intf = item.Tag as Tuple<COMInterfaceInstance, COMInterfaceEntry>;
                COMRegistryViewer.CopyGuidToClipboard(intf.Item1.Iid, type);
            }
        }

        private void asStringToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            CopyIID(GetListViewForMenu(sender), COMRegistryViewer.CopyGuidType.CopyAsString);
        }

        private void asCStructureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyIID(GetListViewForMenu(sender), COMRegistryViewer.CopyGuidType.CopyAsStructure);
        }

        private void asHexStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyIID(GetListViewForMenu(sender), COMRegistryViewer.CopyGuidType.CopyAsHexString);
        }

        private ListView GetListViewForMenu(object sender)
        {
            ContextMenuStrip menu = sender as ContextMenuStrip;
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            if (item != null)
            {
                menu = item.Owner as ContextMenuStrip; 
            }

            if (menu != null)
            {
                return menu.SourceControl as ListView;
            }
            return null;
        }

        private void contextMenuStripInterfaces_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ListView view = GetListViewForMenu(sender);
            if (view != null && view.SelectedIndices.Count > 0)
            {
                ListViewItem item = view.SelectedItems[0];
                Tuple<COMInterfaceInstance, COMInterfaceEntry> intf = 
                    item.Tag as Tuple<COMInterfaceInstance, COMInterfaceEntry>;
                viewProxyDefinitionToolStripMenuItem.Enabled = m_registry.Clsids.ContainsKey(intf.Item2.ProxyClsid);
            }
            else
            {
                viewProxyDefinitionToolStripMenuItem.Enabled = false;
            }
        }

        private void viewProxyDefinitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ListView view = GetListViewForMenu(sender);
                if (view != null && view.SelectedIndices.Count > 0)
                {
                    ListViewItem item = view.SelectedItems[0];
                    Tuple<COMInterfaceInstance, COMInterfaceEntry> intf =
                        item.Tag as Tuple<COMInterfaceInstance, COMInterfaceEntry>;

                    if (m_registry.Clsids.ContainsKey(intf.Item2.ProxyClsid))
                    {
                        COMCLSIDEntry clsid = m_registry.Clsids[intf.Item2.ProxyClsid];
                        Program.GetMainForm(m_registry).HostControl(new TypeLibControl(m_registry,
                            clsid.Name, COMProxyInstance.GetFromCLSID(clsid), intf.Item1.Iid));
                    }
                }
            }
            catch (Exception ex)
            {
                Program.ShowError(this, ex);
            }
        }
    }
}
