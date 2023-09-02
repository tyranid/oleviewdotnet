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

namespace OleViewDotNet.Forms
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            if (Environment.Is64BitProcess)
            {
                textBoxDbgHelp.Text = Properties.Settings.Default.DbgHelpPath64;
                textBoxDatabasePath.Text = Properties.Settings.Default.DatabasePath64;
                checkBoxEnableSaveOnExit.Checked = Properties.Settings.Default.EnableSaveOnExit64;
                checkBoxEnableLoadOnStart.Checked = Properties.Settings.Default.EnableLoadOnStart64;
            }
            else
            {
                textBoxDbgHelp.Text = Properties.Settings.Default.DbgHelpPath32;
                textBoxDatabasePath.Text = Properties.Settings.Default.DatabasePath32;
                checkBoxEnableSaveOnExit.Checked = Properties.Settings.Default.EnableSaveOnExit32;
                checkBoxEnableLoadOnStart.Checked = Properties.Settings.Default.EnableLoadOnStart32;
            }
            textBoxSymbolPath.Text = Properties.Settings.Default.SymbolPath;
            checkBoxParseStubMethods.Checked = Properties.Settings.Default.ParseStubMethods;
            checkBoxResolveMethodNames.Checked = Properties.Settings.Default.ResolveMethodNames;
            checkBoxProxyParserResolveSymbols.Checked = Properties.Settings.Default.ProxyParserResolveSymbols;
            checkBoxParseRegisteredClasses.Checked = Properties.Settings.Default.ParseRegisteredClasses;
            checkBoxParseActCtx.Checked = Properties.Settings.Default.ParseActivationContext;
        }

        private void btnBrowseDbgHelpPath_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "DBGHELP DLL|dbghelp.dll";
                dlg.Multiselect = false;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    textBoxDbgHelp.Text = dlg.FileName;
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            bool valid_dll = false;
            try
            {
                using (SafeLoadLibraryHandle lib = SafeLoadLibraryHandle.LoadLibrary(textBoxDbgHelp.Text))
                {
                    if (lib.GetProcAddress("SymInitializeW") != IntPtr.Zero)
                    {
                        valid_dll = true;
                    }
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

            if (Environment.Is64BitProcess)
            {
                Properties.Settings.Default.DbgHelpPath64 = textBoxDbgHelp.Text;
                Properties.Settings.Default.DatabasePath64 = textBoxDatabasePath.Text;
                Properties.Settings.Default.EnableLoadOnStart64 = checkBoxEnableLoadOnStart.Checked;
                Properties.Settings.Default.EnableSaveOnExit64 = checkBoxEnableSaveOnExit.Checked;
            }
            else
            {
                Properties.Settings.Default.DbgHelpPath32 = textBoxDbgHelp.Text;
                Properties.Settings.Default.DatabasePath32 = textBoxDatabasePath.Text;
                Properties.Settings.Default.EnableLoadOnStart32 = checkBoxEnableLoadOnStart.Checked;
                Properties.Settings.Default.EnableSaveOnExit32 = checkBoxEnableSaveOnExit.Checked;
            }
            Properties.Settings.Default.SymbolPath = textBoxSymbolPath.Text;
            Properties.Settings.Default.SymbolsConfigured = true;
            Properties.Settings.Default.ParseStubMethods = checkBoxParseStubMethods.Checked;
            Properties.Settings.Default.ResolveMethodNames = checkBoxResolveMethodNames.Checked;
            Properties.Settings.Default.ProxyParserResolveSymbols = checkBoxProxyParserResolveSymbols.Checked;
            Properties.Settings.Default.ParseRegisteredClasses = checkBoxParseRegisteredClasses.Checked;
            Properties.Settings.Default.ParseActivationContext = checkBoxParseActCtx.Checked;
            try
            {
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                EntryPoint.ShowError(this, ex);
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
