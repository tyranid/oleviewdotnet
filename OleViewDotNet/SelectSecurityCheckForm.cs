//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017
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
using NtApiDotNet.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet
{
    internal partial class SelectSecurityCheckForm : Form
    {
        private bool _process_security;
        private List<NtToken> _tokens;

        public SelectSecurityCheckForm(bool process_security)
        {
            InitializeComponent();
            _process_security = process_security;
            _tokens = new List<NtToken>();
            Disposed += SelectSecurityCheckForm_Disposed;
            string username = String.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName);
            textBoxPrincipal.Text = username;
            COMProcessParser.EnableDebugPrivilege();
            
            foreach (Process p in Process.GetProcesses().OrderBy(p => p.Id))
            {
                try
                {
                    using (var process = NtProcess.Open(p.Id, ProcessAccessRights.QueryLimitedInformation))
                    {
                        var token = process.OpenToken();
                        _tokens.Add(token);
                        ListViewItem item = listViewProcesses.Items.Add(p.Id.ToString());
                        item.SubItems.Add(p.ProcessName);
                        item.SubItems.Add(process.User.Name);
                        item.SubItems.Add(token.IntegrityLevel.ToString());
                        item.Tag = token;
                    }
                }
                catch
                {
                }
            }
            listViewProcesses.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewProcesses.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewProcesses.ListViewItemSorter = new ListItemComparer(0);
            
            foreach (object value in Enum.GetValues(typeof(TokenIntegrityLevel)))
            {
                comboBoxIL.Items.Add(value);
            }
            comboBoxIL.SelectedItem = TokenIntegrityLevel.Low;
            if (process_security)
            {
                textBoxPrincipal.Enabled = false;
                checkBoxLocalLaunch.Enabled = false;
                checkBoxRemoteLaunch.Enabled = false;
                checkBoxLocalActivate.Enabled = false;
                checkBoxRemoteActivate.Enabled = false;
            }
        }

        private void SelectSecurityCheckForm_Disposed(object sender, EventArgs e)
        {
            foreach (var token in _tokens)
            {
                token.Close();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        internal NtToken Token { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        internal string Principal { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        internal COMAccessRights AccessRights { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        internal COMAccessRights LaunchRights { get; private set; }

        private void radioSpecificProcess_CheckedChanged(object sender, EventArgs e)
        {
            listViewProcesses.Enabled = radioSpecificProcess.Checked;
        }

        private NtToken OpenImpersonationToken()
        {
            using (NtToken token = NtToken.OpenProcessToken())
            {
                return token.DuplicateToken(SecurityImpersonationLevel.Identification);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioCurrentProcess.Checked)
                {
                    Token = OpenImpersonationToken();
                }
                else if (radioSpecificProcess.Checked)
                {
                    if (listViewProcesses.SelectedItems.Count < 1)
                    {
                        throw new InvalidOperationException("Please select a process from the list");
                    }

                    Token = ((NtToken)listViewProcesses.SelectedItems[0].Tag).DuplicateToken(SecurityImpersonationLevel.Identification);
                }
                else if (radioAnonymous.Checked)
                {
                    Token = TokenUtils.GetAnonymousToken();
                }

                if (checkBoxSetIL.Checked)
                {
                    Token.SetIntegrityLevel((TokenIntegrityLevel)comboBoxIL.SelectedItem);
                }

                if (checkBoxLocalAccess.Checked)
                {
                    AccessRights |= COMAccessRights.ExecuteLocal;
                }
                if (checkBoxRemoteAccess.Checked)
                {
                    AccessRights |= COMAccessRights.ExecuteRemote;
                }

                if (!_process_security)
                {
                    if (checkBoxLocalLaunch.Checked)
                    {
                        LaunchRights |= COMAccessRights.ExecuteLocal;
                    }
                    if (checkBoxRemoteLaunch.Checked)
                    {
                        LaunchRights |= COMAccessRights.ExecuteRemote;
                    }
                    if (checkBoxLocalActivate.Checked)
                    {
                        LaunchRights |= COMAccessRights.ActivateLocal;
                    }
                    if (checkBoxRemoteActivate.Checked)
                    {
                        LaunchRights |= COMAccessRights.ActivateRemote;
                    }
                    if (!_process_security)
                    {
                        Principal = COMSecurity.UserToSid(textBoxPrincipal.Text);
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Program.ShowError(this, ex);
            }
        }

        private void checkBoxSetIL_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxIL.Enabled = checkBoxSetIL.Checked;
        }

        private void listViewProcesses_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView view = sender as ListView;
            ListItemComparer.UpdateListComparer(sender as ListView, e.Column);
        }
    }
}
