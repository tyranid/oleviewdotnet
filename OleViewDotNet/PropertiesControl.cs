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
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class PropertiesControl : UserControl
    {
        private COMRegistry _registry;
        private COMAppIDEntry _appid;

        private void LoadInterfaceList(IEnumerable<COMInterfaceInstance> entries, ListView view)
        {
            view.Items.Clear();
            foreach (COMInterfaceInstance entry in entries)
            {
                COMInterfaceEntry int_entry = _registry.MapIidToInterface(entry.Iid);
                ListViewItem item = view.Items.Add(int_entry.Name);
                item.SubItems.Add(int_entry.NumMethods.ToString());
                if (!String.IsNullOrWhiteSpace(entry.ModulePath))
                {
                    item.SubItems.Add(String.Format("{0}+0x{1:X}", entry.ModulePath, entry.VTableOffset));
                }
                item.Tag = entry;
            }
            view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void SetupAppIdEntry(COMAppIDEntry entry)
        {
            textBoxAppIdName.Text = entry.Name;
            textBoxAppIdGuid.Text = entry.AppId.ToString("B");
            textBoxLaunchPermission.Text = entry.LaunchPermissionString ?? String.Empty;
            textBoxAccessPermission.Text = entry.AccessPermissionString ?? String.Empty;
            lblAppIdRunAs.Text = String.Format("Run As: {0}", entry.RunAs ?? "N/A");
            lblService.Text = String.Format("Service: {0}", entry.LocalService ?? "N/A");
            lblAppIDFlags.Text = String.Format("Flags: {0}", entry.Flags);
            textBoxDllSurrogate.Text = entry.DllSurrogate ?? "N/A";
            btnViewAccessPermissions.Enabled = entry.AccessPermission != null;
            btnViewLaunchPermissions.Enabled = entry.LaunchPermission != null;
            tabControlProperties.TabPages.Add(tabPageAppID);
            _appid = entry;
        }

        private void SetupClsidEntry(COMCLSIDEntry entry)
        {
            textBoxClsidName.Text = entry.Name;
            textBoxClsid.Text = entry.Clsid.ToString("B");
            lblServerType.Text = "Server Type: " + entry.ServerType;
            lblThreadingModel.Text = "Threading Model: " + entry.ThreadingModel;
            textBoxServer.Text = entry.Server;
            var progids = _registry.Progids;

            foreach (string progid in entry.ProgIDs)
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
            if (_registry.AppIDs.ContainsKey(entry.AppID))
            {
                SetupAppIdEntry(_registry.AppIDs[entry.AppID]);
            }
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
                COMCLSIDEntry clsid_entry = _registry.MapClsidToEntry(entry.Clsid);
                if (clsid_entry != null)
                {
                    SetupClsidEntry(clsid_entry);
                }
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
            _registry = registry;
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
            COMSecurity.ViewSecurity(this, _appid, false);
        }

        private void btnViewAccessPermissions_Click(object sender, EventArgs e)
        {
            COMSecurity.ViewSecurity(this, _appid, true);
        }
    }
}
