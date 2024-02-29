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

using NtApiDotNet.Win32;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class SettingsForm : Form
{
    public SettingsForm()
    {
        InitializeComponent();
        textBoxDbgHelp.Text = ProgramSettings.DbgHelpPath;
        textBoxSymbolPath.Text = ProgramSettings.SymbolPath;
        checkBoxProxyParserResolveSymbols.Checked = ProgramSettings.ProxyParserResolveSymbols;
    }

    private void btnBrowseDbgHelpPath_Click(object sender, EventArgs e)
    {
        using OpenFileDialog dlg = new();
        dlg.Filter = "DBGHELP DLL|dbghelp.dll";
        dlg.Multiselect = false;
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            textBoxDbgHelp.Text = dlg.FileName;
        }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        bool valid_dll = false;
        try
        {
            using SafeLoadLibraryHandle lib = SafeLoadLibraryHandle.LoadLibrary(textBoxDbgHelp.Text);
            if (lib.GetProcAddress("SymInitializeW") != IntPtr.Zero)
            {
                valid_dll = true;
            }
        }
        catch(Win32Exception)
        {
        }

        if (!valid_dll)
        {
            MessageBox.Show(this, "Invalid DBGHELP.DLL file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ProgramSettings.DbgHelpPath = textBoxDbgHelp.Text;
        ProgramSettings.SymbolPath = textBoxSymbolPath.Text;
        ProgramSettings.SymbolsConfigured = true;
        ProgramSettings.ProxyParserResolveSymbols = checkBoxProxyParserResolveSymbols.Checked;
        DialogResult = DialogResult.OK;
        Close();
    }
}
