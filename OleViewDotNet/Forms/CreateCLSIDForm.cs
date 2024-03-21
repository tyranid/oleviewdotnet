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

using System;
using System.Windows.Forms;
using OleViewDotNet.Interop;

namespace OleViewDotNet.Forms;

internal partial class CreateCLSIDForm : Form
{
    public Guid Clsid { get; private set; }
    public CLSCTX ClsCtx { get; private set; }
    public bool ClassFactory { get; private set; }

    public CreateCLSIDForm()
    {
        InitializeComponent();
        ClsCtx = CLSCTX.SERVER;
        comboBoxClsCtx.Items.Add(CLSCTX.SERVER);
        comboBoxClsCtx.Items.Add(CLSCTX.INPROC_SERVER);
        comboBoxClsCtx.Items.Add(CLSCTX.LOCAL_SERVER);
        comboBoxClsCtx.Items.Add(CLSCTX.ACTIVATE_32_BIT_SERVER | CLSCTX.LOCAL_SERVER);
        comboBoxClsCtx.Items.Add(CLSCTX.ACTIVATE_64_BIT_SERVER | CLSCTX.LOCAL_SERVER);
        comboBoxClsCtx.SelectedIndex = 0;
        textBoxCLSID.Text = "Specify CLSID or ProgID";
    }

    private bool GetClsid(string name, out Guid clsid)
    {
        if (!Guid.TryParse(name, out clsid))
        {
            if (NativeMethods.CLSIDFromProgID(name, out clsid) == 0)
            {
                return true;
            }
            return false;
        }
        return true;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {

        if (GetClsid(textBoxCLSID.Text.Trim(), out Guid clsid) && (comboBoxClsCtx.SelectedItem is not null))
        {
            Clsid = clsid;
            ClsCtx = (CLSCTX)comboBoxClsCtx.SelectedItem;
            ClassFactory = checkBoxClassFactory.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            MessageBox.Show(this, "Invalid CLSID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
