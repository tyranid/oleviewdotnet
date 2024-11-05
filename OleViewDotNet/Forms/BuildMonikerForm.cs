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

using OleViewDotNet.Utilities;
using System;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class BuildMonikerForm : Form
{
    public BuildMonikerForm(string last_moniker)
    {
        InitializeComponent();
        textBoxMoniker.Text = last_moniker;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        try
        {
            if (BindMoniker)
            {
                Result = COMUtilities.ParseAndBindMoniker(textBoxMoniker.Text, checkBoxParseComposite.Checked);
            }
            else
            {
                Result = COMUtilities.ParseMoniker(textBoxMoniker.Text, checkBoxParseComposite.Checked);
            }
            MonikerString = textBoxMoniker.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    public bool BindMoniker { get; set; }
    public object Result { get; private set; }
    public string MonikerString { get; private set; }
}
