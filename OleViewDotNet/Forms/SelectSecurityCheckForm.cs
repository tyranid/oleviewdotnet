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
using OleViewDotNet.Security;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class SelectSecurityCheckForm : Form
{
    private readonly bool _process_security;

    public SelectSecurityCheckForm(bool process_security)
    {
        InitializeComponent();
        _process_security = process_security;
        Disposed += SelectSecurityCheckForm_Disposed;
        string username = $@"{Environment.UserDomainName}\{Environment.UserName}";
        textBoxPrincipal.Text = username;
        selectProcessControl.UpdateProcessList(ProcessAccessRights.None, true, false);
        foreach (object value in Enum.GetValues(typeof(TokenIntegrityLevel)))
        {
            comboBoxIL.Items.Add(value);
        }
        comboBoxIL.SelectedItem = TokenIntegrityLevel.Low;
        if (process_security)
        {
            textBoxPrincipal.Text = string.Empty;
            checkBoxLocalLaunch.Enabled = false;
            checkBoxRemoteLaunch.Enabled = false;
            checkBoxLocalActivate.Enabled = false;
            checkBoxRemoteActivate.Enabled = false;
            checkBoxIgnoreDefault.Enabled = false;
        }
    }

    private void SelectSecurityCheckForm_Disposed(object sender, EventArgs e)
    {
        selectProcessControl.Dispose();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    internal COMAccessToken Token { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    internal COMSid Principal { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    internal COMAccessRights AccessRights { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    internal COMAccessRights LaunchRights { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    internal bool IgnoreDefault { get; private set; }

    private void radioSpecificProcess_CheckedChanged(object sender, EventArgs e)
    {
        selectProcessControl.Enabled = radioSpecificProcess.Checked;
    }

    private NtToken OpenImpersonationToken()
    {
        using NtToken token = NtToken.OpenProcessToken();
        return token.DuplicateToken(SecurityImpersonationLevel.Identification);
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        try
        {
            if (radioCurrentProcess.Checked)
            {
                Token = new COMAccessToken();
            }
            else if (radioSpecificProcess.Checked)
            {
                NtProcess process = selectProcessControl.SelectedProcess ?? throw new InvalidOperationException("Please select a process from the list");
                using var token = NtToken.OpenProcessToken(process, false, TokenAccessRights.Duplicate);
                Token = COMAccessToken.FromToken(token);
            }
            else if (radioAnonymous.Checked)
            {
                Token = COMAccessToken.GetAnonymous();
            }

            if (checkBoxSetIL.Checked)
            {
                Token.Token.SetIntegrityLevel((TokenIntegrityLevel)comboBoxIL.SelectedItem);
            }

            if (checkBoxLocalAccess.Checked)
            {
                AccessRights |= COMAccessRights.ExecuteLocal;
            }
            if (checkBoxRemoteAccess.Checked)
            {
                AccessRights |= COMAccessRights.ExecuteRemote;
            }
            if (!string.IsNullOrWhiteSpace(textBoxPrincipal.Text))
            {
                Principal = COMSid.Parse(textBoxPrincipal.Text);
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
                IgnoreDefault = checkBoxIgnoreDefault.Checked;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private void checkBoxSetIL_CheckedChanged(object sender, EventArgs e)
    {
        comboBoxIL.Enabled = checkBoxSetIL.Checked;
    }

    private void listViewProcesses_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        ListItemComparer.UpdateListComparer(sender as ListView, e.Column);
    }
}
