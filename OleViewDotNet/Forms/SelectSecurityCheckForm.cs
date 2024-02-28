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
        textBoxUsername.Text = username;
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
        Token?.Dispose();
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
            else if (radioLogonUser.Checked)
            {
                if (string.IsNullOrEmpty(textBoxUsername.Text))
                    throw new ArgumentException("No username specified.");

                using COMCredentials creds = new();
                creds.UserName = textBoxUsername.Text;
                int slash_index = creds.UserName.IndexOf('\\');
                if (slash_index >= 0)
                {
                    creds.Domain = creds.UserName.Substring(0, slash_index);
                    creds.UserName = creds.UserName.Substring(slash_index + 1);
                }
                if (checkBoxS4U.Checked)
                {
                    Token = COMAccessToken.LogonS4U(creds, TokenLogonType.Network);
                }
                else
                {
                    creds.SetPassword(textBoxPassword.Text);
                    Token = COMAccessToken.Logon(creds, TokenLogonType.Network);
                }
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

    private void radioLogonUser_CheckedChanged(object sender, EventArgs e)
    {
        bool enabled = radioLogonUser.Checked;
        checkBoxS4U.Enabled = enabled;
        lblUserName.Enabled = enabled;
        textBoxUsername.Enabled = enabled;

        enabled = enabled && !checkBoxS4U.Checked;
        lblPassword.Enabled = enabled;
        textBoxPassword.Enabled = enabled;
    }

    private void checkBoxS4U_CheckedChanged(object sender, EventArgs e)
    {
        lblPassword.Enabled = !checkBoxS4U.Checked;
        textBoxPassword.Enabled = !checkBoxS4U.Checked;
    }
}
