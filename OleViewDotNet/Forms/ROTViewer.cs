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
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class ROTViewer : UserControl
{
    private readonly COMRegistry m_registry;

    public ROTViewer(COMRegistry reg)
    {
        m_registry = reg;
        InitializeComponent();
    }

    private void LoadROT(bool trusted_only)
    {
        listViewROT.Items.Clear();
        try
        {
            foreach (var entry in COMRunningObjectTable.EnumRunning(trusted_only))
            {
                ListViewItem item = listViewROT.Items.Add(entry.DisplayName);
                item.Tag = entry;

                if (m_registry.Clsids.ContainsKey(entry.Clsid))
                {
                    item.SubItems.Add(m_registry.Clsids[entry.Clsid].Name);
                }
                else
                {
                    item.SubItems.Add(entry.Clsid.FormatGuid());
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

    private void getObjectToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (listViewROT.SelectedItems.Count != 0)
        {
            COMRunningObjectTableEntry info = (COMRunningObjectTableEntry)listViewROT.SelectedItems[0].Tag;

            Dictionary<string, string> props = new()
            {
                { "Display Name", info.DisplayName },
                { "CLSID", info.Clsid.FormatGuid() }
            };

            try
            {
                object obj = info.GetObject();
                ObjectInformation view = new(m_registry, null, info.DisplayName,
                    obj, props, m_registry.GetInterfacesForObject(obj).ToArray());
                EntryPoint.GetMainForm(m_registry).HostControl(view);
            }
            catch (Exception ex)
            {
                EntryPoint.ShowError(this, ex);
            }
        }
    }
}
