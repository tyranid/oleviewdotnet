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
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace OleViewDotNet
{
    public partial class PropertiesControl : UserControl
    {
        private COMRegistry m_registry;
        private COMAppIDEntry m_appid;
        private COMCLSIDEntry m_clsid;

        private void LoadInterfaceList(IEnumerable<COMInterfaceInstance> entries, ListView view)
        {
            view.Items.Clear();
            foreach (COMInterfaceInstance entry in entries)
            {
                COMInterfaceEntry int_entry = m_registry.MapIidToInterface(entry.Iid);
                ListViewItem item = view.Items.Add(int_entry.Name);
                item.SubItems.Add(int_entry.NumMethods.ToString());
                if (!String.IsNullOrWhiteSpace(entry.Module))
                {
                    item.SubItems.Add(String.Format("{0}+0x{1:X}", entry.Module, entry.VTableOffset));
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

        private void SetupAppIdEntry(COMAppIDEntry entry)
        {
            textBoxAppIdName.Text = entry.Name;
            textBoxAppIdGuid.Text = entry.AppId.ToString("B");
            textBoxLaunchPermission.Text = entry.LaunchPermission;
            textBoxAccessPermission.Text = entry.AccessPermission;
            lblAppIdRunAs.Text = String.Format("Run As: {0}", GetStringValue(entry.RunAs));
            lblService.Text = String.Format("Service: {0}", GetStringValue(entry.LocalService));
            lblAppIDFlags.Text = String.Format("Flags: {0}", entry.Flags);
            textBoxDllSurrogate.Text = GetStringValue(entry.DllSurrogate);
            btnViewAccessPermissions.Enabled = entry.HasAccessPermission;
            btnViewLaunchPermissions.Enabled = entry.HasLaunchPermission;
            tabControlProperties.TabPages.Add(tabPageAppID);
            m_appid = entry;
        }

        private void SetupClsidEntry(COMCLSIDEntry entry)
        {
            textBoxClsidName.Text = entry.Name;
            textBoxClsid.Text = entry.Clsid.ToString("B");
            lblServerType.Text = "Server Type: " + entry.ServerType;
            lblThreadingModel.Text = "Threading Model: " + entry.ThreadingModel;
            textBoxServer.Text = entry.Server;
            textBoxCmdLine.Text = GetStringValue(entry.CmdLine);
            if (entry.TreatAs != Guid.Empty)
            {
                textBoxTreatAs.Text = entry.TreatAs.ToString("B");
                if (m_registry.Clsids.ContainsKey(entry.TreatAs))
                {
                    btnTreatAsProps.Enabled = true;
                }
            }
            else
            {
                textBoxTreatAs.Text = "N/A";
                btnTreatAsProps.Enabled = false;
            }
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
            m_clsid = entry;
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
        }

        public static bool SupportsProperties(object obj)
        {
            return obj is COMCLSIDEntry || obj is COMProgIDEntry || obj is COMAppIDEntry;
        }

        public PropertiesControl(COMRegistry registry, string name, object obj)
        {
            m_registry = registry;
            InitializeComponent();
            listViewCategories.Columns.Add("Name", 100);
            listViewProgIDs.Columns.Add("Name", 100);
            listViewInterfaces.Columns.Add("Name", 100);
            listViewInterfaces.Columns.Add("Methods", 100);
            listViewInterfaces.Columns.Add("VTable Offset", 100);
            listViewFactoryInterfaces.Columns.Add("Name", 100);
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
                object comObj = m_clsid.CreateInstanceAsObject(m_clsid.CreateContext);
                if (comObj != null)
                {
                    await Program.GetMainForm(m_registry).HostObject(m_clsid, comObj);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
