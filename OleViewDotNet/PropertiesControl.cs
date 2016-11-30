using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace OleViewDotNet
{
    public partial class PropertiesControl : UserControl
    {
        private void SetupClsidEntry(COMCLSIDEntry entry)
        {
            textBoxClsidName.Text = entry.Name;
            textBoxClsid.Text = entry.Clsid.ToString("B");
            lblServerType.Text = "Server Type: " + entry.ServerType;
            lblThreadingModel.Text = "Threading Model: " + entry.ThreadingModel;
            var progids = COMRegistry.Instance.Progids;

            foreach (string progid in entry.ProgIDs)
            {
                ListViewItem item = listViewProgIDs.Items.Add(progid);
                if (progids.ContainsKey(progid))
                {
                    item.Tag = progids[progid];
                }
            }

            tabControlProperties.TabPages.Add(tabPageClsid);
        }

        private void SetupProperties(object obj)
        {
            if (obj is COMCLSIDEntry)
            {
                SetupClsidEntry((COMCLSIDEntry)obj);
            }
        }

        public PropertiesControl(object obj)
        {
            InitializeComponent();
            tabControlProperties.TabPages.Clear();
            SetupProperties(obj);
            if (tabControlProperties.TabCount == 0)
            {
                tabControlProperties.TabPages.Add(tabPageNoProperties);
            }
        }
    }
}
