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
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class SelectProcessForm : Form
    {
        public SelectProcessForm(ProcessAccessRights desired_access, bool require_token)
        {
            InitializeComponent();
            selectProcessControl.UpdateProcessList(desired_access, false);
            selectProcessControl.ProcessSelected += btnOK_Click;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NtProcess SelectedProcess
        {
            get
            {
                return selectProcessControl.SelectedProcess;
            }
        }
    }
}
