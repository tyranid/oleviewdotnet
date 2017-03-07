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


using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet
{
    internal partial class SelectSecurityCheckForm : Form
    {
        public SelectSecurityCheckForm()
        {
            InitializeComponent();
            Disposed += SelectSecurityCheckForm_Disposed;
            string username = String.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName);
            textBoxPrincipal.Text = username;
            COMProcessParser.EnableDebugPrivilege();
            foreach (Process p in Process.GetProcesses().OrderBy(p => p.Id))
            {
                try
                {
                    using (SafeProcessHandle process = SafeProcessHandle.Open(p.Id, ProcessAccessRights.QueryInformation))
                    {
                        SafeTokenHandle token = process.OpenToken();
                        ListViewItem item = listViewProcesses.Items.Add(p.Id.ToString());
                        item.SubItems.Add(p.ProcessName);
                        item.SubItems.Add(process.GetProcessUser());
                        item.SubItems.Add(token.GetIntegrityLevel().ToString());
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
            
            foreach (object value in Enum.GetValues(typeof(SecurityIntegrityLevel)))
            {
                comboBoxIL.Items.Add(value);
            }
            comboBoxIL.SelectedItem = SecurityIntegrityLevel.Low;
        }

        private void SelectSecurityCheckForm_Disposed(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewProcesses.Items)
            {
                SafeTokenHandle token = (SafeTokenHandle)item.Tag;
                token.Close();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        internal SafeTokenHandle Token { get; private set; }

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

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioCurrentProcess.Checked)
                {
                    Token = SafeProcessHandle.Current.OpenTokenAsImpersonation(SecurityImpersonationLevel.SecurityIdentification);
                }
                else if (radioSpecificProcess.Checked)
                {
                    if (listViewProcesses.SelectedItems.Count < 1)
                    {
                        throw new InvalidOperationException("Please select a process from the list");
                    }

                    Token = ((SafeTokenHandle)listViewProcesses.SelectedItems[0].Tag).DuplicateImpersonation(SecurityImpersonationLevel.SecurityIdentification);
                }

                if (checkBoxSetIL.Checked)
                {
                    Token.SetIntegrityLevel((SecurityIntegrityLevel)comboBoxIL.SelectedItem);
                }

                Principal = COMSecurity.UserToSid(textBoxPrincipal.Text);
                if (checkBoxLocalAccess.Checked)
                {
                    AccessRights |= COMAccessRights.ExecuteLocal;
                }
                if (checkBoxRemoteAccess.Checked)
                {
                    AccessRights |= COMAccessRights.ExecuteRemote;
                }
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
