using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class PropertiesControl : UserControl
    {
        private COMRegistry _registry;

        private void LoadInterfaceList(IEnumerable<COMInterfaceEntry> entries, ListView view)
        {
            view.Items.Clear();
            foreach (COMInterfaceEntry entry in entries)
            {
                ListViewItem item = view.Items.Add(entry.Name);
                item.Tag = entry;
            }
        }

        private void SetupAppIdEntry(COMAppIDEntry entry)
        {
            textBoxAppIdName.Text = entry.Name;
            textBoxAppIdGuid.Text = entry.AppId.ToString("B");
            textBoxLaunchPermission.Text = entry.LaunchPermissionString ?? String.Empty;
            textBoxAccessPermission.Text = entry.AccessPermissionString ?? String.Empty;
            lblAppIdRunAs.Text = String.Format("Run As: {0}", entry.RunAs ?? "N/A");
            lblService.Text = String.Format("Service: {0}", entry.LocalService ?? "N/A");
            textBoxDllSurrogate.Text = entry.DllSurrogate ?? "N/A";
            tabControlProperties.TabPages.Add(tabPageAppID);
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

            foreach (Guid catid in entry.Categories)
            {
                ListViewItem item = listViewCategories.Items.Add(COMUtilities.GetCategoryName(catid));
                item.Tag = catid;
            }

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
                if (entry.Entry != null)
                {
                    SetupClsidEntry(entry.Entry);
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
            tabControlProperties.TabPages.Clear();
            SetupProperties(obj);
            if (tabControlProperties.TabCount == 0)
            {
                tabControlProperties.TabPages.Add(tabPageNoProperties);
            }
            this.Text = String.Format("{0} Properties", name);
        }

        private async void btnRefreshInterfaces_Click(object sender, EventArgs e)
        {
            try
            {
                COMCLSIDEntry entry = (COMCLSIDEntry)tabPageSupportedInterfaces.Tag;
                await entry.LoadSupportedInterfaces(true);
                LoadInterfaceList(entry.Interfaces, listViewInterfaces);                
                LoadInterfaceList(entry.FactoryInterfaces, listViewFactoryInterfaces);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
