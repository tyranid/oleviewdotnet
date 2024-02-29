//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017, 2018
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
using OleViewDotNet.Utilities;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class SelectProcessControl : UserControl
{
    private DisposableList<NtProcess> _processes;

    public SelectProcessControl()
    {
        InitializeComponent();
        Disposed += SelectProcessControl_Disposed;
    }

    private void SelectProcessControl_Disposed(object sender, EventArgs e)
    {
        ClearListView();
    }

    private void listViewProcesses_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        ListItemComparer.UpdateListComparer(sender as ListView, e.Column);
    }

    private void ClearListView()
    {
        _processes?.Dispose();
        _processes = new DisposableList<NtProcess>();
        listViewProcesses.Items.Clear();
    }

    public void UpdateProcessList(ProcessAccessRights desired_access, bool require_token, bool filter_native)
    {
        NtToken.EnableDebugPrivilege();
        ClearListView();
        using (var ps = NtProcess.GetProcesses(ProcessAccessRights.QueryLimitedInformation | desired_access, true).ToDisposableList())
        {
            foreach (var p in ps.OrderBy(p => p.ProcessId))
            {
                if (p.Is64Bit && !Environment.Is64BitProcess && filter_native)
                {
                    continue;
                }

                using var result = NtToken.OpenProcessToken(p, TokenAccessRights.Query, false);
                if (!result.IsSuccess && require_token)
                {
                    continue;
                }

                ListViewItem item = listViewProcesses.Items.Add(p.ProcessId.ToString());
                item.SubItems.Add(p.Name);
                item.SubItems.Add(COMUtilities.FormatBitness(p.Is64Bit));
                if (result.IsSuccess)
                {
                    NtToken token = result.Result;
                    item.SubItems.Add(p.User.Name);
                    item.SubItems.Add(token.IntegrityLevel.ToString());
                }
                item.Tag = _processes.AddResource(p.Duplicate());
            }
        }
        listViewProcesses.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewProcesses.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        listViewProcesses.ListViewItemSorter = new ListItemComparer(0);
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public NtProcess SelectedProcess
    {
        get
        {
            if (listViewProcesses.SelectedItems.Count > 0)
            {
                return listViewProcesses.SelectedItems[0].Tag as NtProcess;
            }
            return null;
        }
    }

    public event EventHandler ProcessSelected;

    private void listViewProcesses_DoubleClick(object sender, EventArgs e)
    {
        if (listViewProcesses.SelectedItems.Count > 0)
        {
            ProcessSelected?.Invoke(this, new EventArgs());
        }
    }
}
