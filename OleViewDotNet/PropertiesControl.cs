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

        private void SetupClsidEntry(COMCLSIDEntry entry)
        {
            textBoxClsidName.Text = entry.Name;
            textBoxClsid.Text = entry.Clsid.ToString("B");
            lblServerType.Text = "Server Type: " + entry.ServerType;
            lblThreadingModel.Text = "Threading Model: " + entry.ThreadingModel;
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
        }

        private void SetupProperties(object obj)
        {
            if (obj is COMCLSIDEntry)
            {
                SetupClsidEntry((COMCLSIDEntry)obj);
            }
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
            COMCLSIDEntry entry = (COMCLSIDEntry)tabPageSupportedInterfaces.Tag;
            await entry.LoadSupportedInterfaces(true);
            LoadInterfaceList(entry.Interfaces, listViewInterfaces);
            await entry.LoadSupportedFactoryInterfaces(true);
            LoadInterfaceList(entry.FactoryInterfaces, listViewFactoryInterfaces);
        }
    }
}
