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

using OleViewDotNet.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace OleViewDotNet.Forms
{
    public partial class ROTViewer : UserControl
    {
        private COMRegistry m_registry;

        struct MonikerInfo
        {
            public string strDisplayName;
            public Guid clsid;
            public IMoniker moniker;

            public MonikerInfo(string name, Guid guid, IMoniker mon)
            {
                strDisplayName = name;
                clsid = guid;
                moniker = mon;
            }
        }

        public ROTViewer(COMRegistry reg)
        {
            m_registry = reg;
            InitializeComponent();
        }

        void LoadROT(bool trusted_only)
        {
            IBindCtx bindCtx;

            listViewROT.Items.Clear();
            try
            {
                bindCtx = COMUtilities.CreateBindCtx(trusted_only ? 1U : 0U);
                IRunningObjectTable rot;
                IEnumMoniker enumMoniker;
                IMoniker[] moniker = new IMoniker[1];

                bindCtx.GetRunningObjectTable(out rot);
                rot.EnumRunning(out enumMoniker);
                while (enumMoniker.Next(1, moniker, IntPtr.Zero) == 0)
                {
                    string strDisplayName;

                    moniker[0].GetDisplayName(bindCtx, null, out strDisplayName);
                    Guid clsid = COMUtilities.GetObjectClass(moniker[0]);
                    ListViewItem item = listViewROT.Items.Add(strDisplayName);
                    item.Tag = new MonikerInfo(strDisplayName, clsid, moniker[0]);
                    
                    if (m_registry.Clsids.ContainsKey(clsid))
                    {
                        item.SubItems.Add(m_registry.Clsids[clsid].Name);
                    }
                    else
                    {
                        item.SubItems.Add(clsid.FormatGuid());
                    }
                }
            }
            catch (Exception e)
            {
                EntryPoint.ShowError(this, e);
            }

            listViewROT.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void ROTViewer_Load(object sender, EventArgs e)
        {
            listViewROT.Columns.Add("Display Name");
            listViewROT.Columns.Add("CLSID");
            LoadROT(false);
            Text = "ROT";
        }

        private void menuROTRefresh_Click(object sender, EventArgs e)
        {
            LoadROT(checkBoxTrustedOnly.Checked);
        }

        private void menuROTBindToObject_Click(object sender, EventArgs e)
        {
            if (listViewROT.SelectedItems.Count != 0)
            {
                MonikerInfo info = (MonikerInfo)(listViewROT.SelectedItems[0].Tag);

                Dictionary<string, string> props = new Dictionary<string, string>();
                props.Add("Display Name", info.strDisplayName);
                props.Add("CLSID", info.clsid.FormatGuid());

                try
                {
                    IBindCtx bindCtx = COMUtilities.CreateBindCtx(0);
                    Guid unk = COMInterfaceEntry.IID_IUnknown;
                    object comObj;
                    Type dispType;

                    info.moniker.BindToObject(bindCtx, null, ref unk, out comObj);
                    dispType = COMUtilities.GetDispatchTypeInfo(this, comObj);
                    ObjectInformation view = new ObjectInformation(m_registry, null, info.strDisplayName, 
                        comObj, props, m_registry.GetInterfacesForObject(comObj).ToArray());
                    EntryPoint.GetMainForm(m_registry).HostControl(view);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
