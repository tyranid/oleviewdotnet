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
using OleViewDotNet.Utilities;

namespace OleViewDotNet.Forms;

internal partial class CreateIStreamForm : Form
{
    public CreateIStreamForm()
    {
        InitializeComponent();
    }

    public IStreamImpl Stream { get; private set; }

    private void btnCreateRead_Click(object sender, EventArgs e)
    {
        using (OpenFileDialog dlg = new())
        {
            dlg.Filter = "All Files (*.*)|*.*";
            dlg.ShowReadOnly = false;
            this.DialogResult = dlg.ShowDialog();
            if (this.DialogResult == DialogResult.OK)
            {
                try
                {
                    Stream = new IStreamImpl(dlg.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        Close();
    }

    private void btnCreateWrite_Click(object sender, EventArgs e)
    {
        using (SaveFileDialog dlg = new())
        {
            dlg.Filter = "All Files (*.*)|*.*";

            this.DialogResult = dlg.ShowDialog();

            if (this.DialogResult == DialogResult.OK)
            {
                try
                {
                    Stream = new IStreamImpl(dlg.FileName, System.IO.FileMode.CreateNew, System.IO.FileAccess.ReadWrite, System.IO.FileShare.Read);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        Close();
    }
}
