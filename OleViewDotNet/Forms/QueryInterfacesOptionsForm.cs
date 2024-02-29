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

using OleViewDotNet.Database;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class QueryInterfacesOptionsForm : Form
{
    public QueryInterfacesOptionsForm()
    {
        InitializeComponent();
        numericUpDownConcurrentQueries.Maximum = Environment.ProcessorCount * 2;
        numericUpDownConcurrentQueries.Value = Environment.ProcessorCount;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        if (!checkBoxInProcHandler.Checked && !checkBoxLocalServer.Checked && !checkBoxInProcServer.Checked)
        {
            MessageBox.Show(this, "Must check at least one server type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            List<COMServerType> server_types = new();
            if (checkBoxInProcHandler.Checked)
            {
                server_types.Add(COMServerType.InProcHandler32);
            }

            if (checkBoxInProcServer.Checked)
            {
                server_types.Add(COMServerType.InProcServer32);
            }

            if (checkBoxLocalServer.Checked)
            {
                server_types.Add(COMServerType.LocalServer32);
            }

            ServerTypes = server_types.AsReadOnly();
            ConcurrentQueries = (int)numericUpDownConcurrentQueries.Value;
            RefreshInterfaces = checkBoxRefreshInterfaces.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public IEnumerable<COMServerType> ServerTypes
    {
        get; private set;
    }

    public int ConcurrentQueries
    {
        get; private set; 
    }

    public bool RefreshInterfaces
    {
        get; private set;
    }
        
}
