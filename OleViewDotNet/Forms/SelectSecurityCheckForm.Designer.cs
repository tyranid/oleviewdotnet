namespace OleViewDotNet.Forms;

partial class SelectSecurityCheckForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components is not null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            System.Windows.Forms.GroupBox groupBoxAccessToken;
            System.Windows.Forms.GroupBox groupBoxOptions;
            System.Windows.Forms.Label label1;
            this.checkBoxS4U = new System.Windows.Forms.CheckBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.radioLogonUser = new System.Windows.Forms.RadioButton();
            this.selectProcessControl = new OleViewDotNet.Forms.SelectProcessControl();
            this.radioAnonymous = new System.Windows.Forms.RadioButton();
            this.radioSpecificProcess = new System.Windows.Forms.RadioButton();
            this.radioCurrentProcess = new System.Windows.Forms.RadioButton();
            this.checkBoxIgnoreDefault = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoteActivate = new System.Windows.Forms.CheckBox();
            this.checkBoxLocalActivate = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoteLaunch = new System.Windows.Forms.CheckBox();
            this.checkBoxLocalLaunch = new System.Windows.Forms.CheckBox();
            this.textBoxPrincipal = new System.Windows.Forms.TextBox();
            this.checkBoxRemoteAccess = new System.Windows.Forms.CheckBox();
            this.checkBoxLocalAccess = new System.Windows.Forms.CheckBox();
            this.comboBoxIL = new System.Windows.Forms.ComboBox();
            this.checkBoxSetIL = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            groupBoxAccessToken = new System.Windows.Forms.GroupBox();
            groupBoxOptions = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            groupBoxAccessToken.SuspendLayout();
            groupBoxOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxAccessToken
            // 
            groupBoxAccessToken.Controls.Add(this.checkBoxS4U);
            groupBoxAccessToken.Controls.Add(this.textBoxPassword);
            groupBoxAccessToken.Controls.Add(this.lblPassword);
            groupBoxAccessToken.Controls.Add(this.textBoxUsername);
            groupBoxAccessToken.Controls.Add(this.lblUserName);
            groupBoxAccessToken.Controls.Add(this.radioLogonUser);
            groupBoxAccessToken.Controls.Add(this.selectProcessControl);
            groupBoxAccessToken.Controls.Add(this.radioAnonymous);
            groupBoxAccessToken.Controls.Add(this.radioSpecificProcess);
            groupBoxAccessToken.Controls.Add(this.radioCurrentProcess);
            groupBoxAccessToken.Location = new System.Drawing.Point(14, 19);
            groupBoxAccessToken.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBoxAccessToken.Name = "groupBoxAccessToken";
            groupBoxAccessToken.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBoxAccessToken.Size = new System.Drawing.Size(951, 367);
            groupBoxAccessToken.TabIndex = 4;
            groupBoxAccessToken.TabStop = false;
            groupBoxAccessToken.Text = "Access Token";
            // 
            // checkBoxS4U
            // 
            this.checkBoxS4U.AutoSize = true;
            this.checkBoxS4U.Enabled = false;
            this.checkBoxS4U.Location = new System.Drawing.Point(857, 297);
            this.checkBoxS4U.Name = "checkBoxS4U";
            this.checkBoxS4U.Size = new System.Drawing.Size(67, 24);
            this.checkBoxS4U.TabIndex = 12;
            this.checkBoxS4U.Text = "S4U";
            this.checkBoxS4U.UseVisualStyleBackColor = true;
            this.checkBoxS4U.CheckedChanged += new System.EventHandler(this.checkBoxS4U_CheckedChanged);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Enabled = false;
            this.textBoxPassword.Location = new System.Drawing.Point(599, 296);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.Size = new System.Drawing.Size(243, 26);
            this.textBoxPassword.TabIndex = 11;
            this.textBoxPassword.UseSystemPasswordChar = true;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Enabled = false;
            this.lblPassword.Location = new System.Drawing.Point(511, 298);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(82, 20);
            this.lblPassword.TabIndex = 10;
            this.lblPassword.Text = "Password:";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Enabled = false;
            this.textBoxUsername.Location = new System.Drawing.Point(262, 296);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(243, 26);
            this.textBoxUsername.TabIndex = 9;
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Enabled = false;
            this.lblUserName.Location = new System.Drawing.Point(173, 298);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(87, 20);
            this.lblUserName.TabIndex = 8;
            this.lblUserName.Text = "Username:";
            // 
            // radioLogonUser
            // 
            this.radioLogonUser.AutoSize = true;
            this.radioLogonUser.Location = new System.Drawing.Point(9, 296);
            this.radioLogonUser.Name = "radioLogonUser";
            this.radioLogonUser.Size = new System.Drawing.Size(117, 24);
            this.radioLogonUser.TabIndex = 7;
            this.radioLogonUser.TabStop = true;
            this.radioLogonUser.Text = "Logon User";
            this.radioLogonUser.UseVisualStyleBackColor = true;
            this.radioLogonUser.CheckedChanged += new System.EventHandler(this.radioLogonUser_CheckedChanged);
            // 
            // selectProcessControl
            // 
            this.selectProcessControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectProcessControl.Enabled = false;
            this.selectProcessControl.Location = new System.Drawing.Point(8, 100);
            this.selectProcessControl.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.selectProcessControl.Name = "selectProcessControl";
            this.selectProcessControl.Size = new System.Drawing.Size(935, 188);
            this.selectProcessControl.TabIndex = 6;
            // 
            // radioAnonymous
            // 
            this.radioAnonymous.AutoSize = true;
            this.radioAnonymous.Location = new System.Drawing.Point(9, 330);
            this.radioAnonymous.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioAnonymous.Name = "radioAnonymous";
            this.radioAnonymous.Size = new System.Drawing.Size(166, 24);
            this.radioAnonymous.TabIndex = 5;
            this.radioAnonymous.TabStop = true;
            this.radioAnonymous.Text = "Anonymous Token";
            this.radioAnonymous.UseVisualStyleBackColor = true;
            // 
            // radioSpecificProcess
            // 
            this.radioSpecificProcess.AutoSize = true;
            this.radioSpecificProcess.Location = new System.Drawing.Point(9, 65);
            this.radioSpecificProcess.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioSpecificProcess.Name = "radioSpecificProcess";
            this.radioSpecificProcess.Size = new System.Drawing.Size(151, 24);
            this.radioSpecificProcess.TabIndex = 4;
            this.radioSpecificProcess.Text = "Specific Process";
            this.radioSpecificProcess.UseVisualStyleBackColor = true;
            this.radioSpecificProcess.CheckedChanged += new System.EventHandler(this.radioSpecificProcess_CheckedChanged);
            // 
            // radioCurrentProcess
            // 
            this.radioCurrentProcess.AutoSize = true;
            this.radioCurrentProcess.Checked = true;
            this.radioCurrentProcess.Location = new System.Drawing.Point(9, 29);
            this.radioCurrentProcess.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioCurrentProcess.Name = "radioCurrentProcess";
            this.radioCurrentProcess.Size = new System.Drawing.Size(148, 24);
            this.radioCurrentProcess.TabIndex = 3;
            this.radioCurrentProcess.TabStop = true;
            this.radioCurrentProcess.Text = "Current Process";
            this.radioCurrentProcess.UseVisualStyleBackColor = true;
            // 
            // groupBoxOptions
            // 
            groupBoxOptions.Controls.Add(this.checkBoxIgnoreDefault);
            groupBoxOptions.Controls.Add(this.checkBoxRemoteActivate);
            groupBoxOptions.Controls.Add(this.checkBoxLocalActivate);
            groupBoxOptions.Controls.Add(this.checkBoxRemoteLaunch);
            groupBoxOptions.Controls.Add(this.checkBoxLocalLaunch);
            groupBoxOptions.Controls.Add(this.textBoxPrincipal);
            groupBoxOptions.Controls.Add(label1);
            groupBoxOptions.Controls.Add(this.checkBoxRemoteAccess);
            groupBoxOptions.Controls.Add(this.checkBoxLocalAccess);
            groupBoxOptions.Controls.Add(this.comboBoxIL);
            groupBoxOptions.Controls.Add(this.checkBoxSetIL);
            groupBoxOptions.Location = new System.Drawing.Point(14, 396);
            groupBoxOptions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBoxOptions.Name = "groupBoxOptions";
            groupBoxOptions.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            groupBoxOptions.Size = new System.Drawing.Size(945, 121);
            groupBoxOptions.TabIndex = 5;
            groupBoxOptions.TabStop = false;
            groupBoxOptions.Text = "Options";
            // 
            // checkBoxIgnoreDefault
            // 
            this.checkBoxIgnoreDefault.AutoSize = true;
            this.checkBoxIgnoreDefault.Location = new System.Drawing.Point(771, 29);
            this.checkBoxIgnoreDefault.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxIgnoreDefault.Name = "checkBoxIgnoreDefault";
            this.checkBoxIgnoreDefault.Size = new System.Drawing.Size(137, 24);
            this.checkBoxIgnoreDefault.TabIndex = 10;
            this.checkBoxIgnoreDefault.Text = "Ignore Default";
            this.checkBoxIgnoreDefault.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoteActivate
            // 
            this.checkBoxRemoteActivate.AutoSize = true;
            this.checkBoxRemoteActivate.Location = new System.Drawing.Point(771, 65);
            this.checkBoxRemoteActivate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxRemoteActivate.Name = "checkBoxRemoteActivate";
            this.checkBoxRemoteActivate.Size = new System.Drawing.Size(153, 24);
            this.checkBoxRemoteActivate.TabIndex = 9;
            this.checkBoxRemoteActivate.Text = "Remote Activate";
            this.checkBoxRemoteActivate.UseVisualStyleBackColor = true;
            // 
            // checkBoxLocalActivate
            // 
            this.checkBoxLocalActivate.AutoSize = true;
            this.checkBoxLocalActivate.Checked = true;
            this.checkBoxLocalActivate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLocalActivate.Location = new System.Drawing.Point(621, 65);
            this.checkBoxLocalActivate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxLocalActivate.Name = "checkBoxLocalActivate";
            this.checkBoxLocalActivate.Size = new System.Drawing.Size(134, 24);
            this.checkBoxLocalActivate.TabIndex = 8;
            this.checkBoxLocalActivate.Text = "Local Activate";
            this.checkBoxLocalActivate.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoteLaunch
            // 
            this.checkBoxRemoteLaunch.AutoSize = true;
            this.checkBoxRemoteLaunch.Location = new System.Drawing.Point(459, 65);
            this.checkBoxRemoteLaunch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxRemoteLaunch.Name = "checkBoxRemoteLaunch";
            this.checkBoxRemoteLaunch.Size = new System.Drawing.Size(149, 24);
            this.checkBoxRemoteLaunch.TabIndex = 7;
            this.checkBoxRemoteLaunch.Text = "Remote Launch";
            this.checkBoxRemoteLaunch.UseVisualStyleBackColor = true;
            // 
            // checkBoxLocalLaunch
            // 
            this.checkBoxLocalLaunch.AutoSize = true;
            this.checkBoxLocalLaunch.Checked = true;
            this.checkBoxLocalLaunch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLocalLaunch.Location = new System.Drawing.Point(314, 65);
            this.checkBoxLocalLaunch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxLocalLaunch.Name = "checkBoxLocalLaunch";
            this.checkBoxLocalLaunch.Size = new System.Drawing.Size(130, 24);
            this.checkBoxLocalLaunch.TabIndex = 6;
            this.checkBoxLocalLaunch.Text = "Local Launch";
            this.checkBoxLocalLaunch.UseVisualStyleBackColor = true;
            // 
            // textBoxPrincipal
            // 
            this.textBoxPrincipal.Location = new System.Drawing.Point(477, 26);
            this.textBoxPrincipal.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxPrincipal.Name = "textBoxPrincipal";
            this.textBoxPrincipal.Size = new System.Drawing.Size(278, 26);
            this.textBoxPrincipal.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(393, 29);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(72, 20);
            label1.TabIndex = 4;
            label1.Text = "Principal:";
            // 
            // checkBoxRemoteAccess
            // 
            this.checkBoxRemoteAccess.AutoSize = true;
            this.checkBoxRemoteAccess.Location = new System.Drawing.Point(153, 65);
            this.checkBoxRemoteAccess.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxRemoteAccess.Name = "checkBoxRemoteAccess";
            this.checkBoxRemoteAccess.Size = new System.Drawing.Size(148, 24);
            this.checkBoxRemoteAccess.TabIndex = 3;
            this.checkBoxRemoteAccess.Text = "Remote Access";
            this.checkBoxRemoteAccess.UseVisualStyleBackColor = true;
            // 
            // checkBoxLocalAccess
            // 
            this.checkBoxLocalAccess.AutoSize = true;
            this.checkBoxLocalAccess.Checked = true;
            this.checkBoxLocalAccess.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLocalAccess.Location = new System.Drawing.Point(9, 65);
            this.checkBoxLocalAccess.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxLocalAccess.Name = "checkBoxLocalAccess";
            this.checkBoxLocalAccess.Size = new System.Drawing.Size(129, 24);
            this.checkBoxLocalAccess.TabIndex = 2;
            this.checkBoxLocalAccess.Text = "Local Access";
            this.checkBoxLocalAccess.UseVisualStyleBackColor = true;
            // 
            // comboBoxIL
            // 
            this.comboBoxIL.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIL.Enabled = false;
            this.comboBoxIL.FormattingEnabled = true;
            this.comboBoxIL.Location = new System.Drawing.Point(189, 22);
            this.comboBoxIL.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBoxIL.Name = "comboBoxIL";
            this.comboBoxIL.Size = new System.Drawing.Size(180, 28);
            this.comboBoxIL.TabIndex = 1;
            // 
            // checkBoxSetIL
            // 
            this.checkBoxSetIL.AutoSize = true;
            this.checkBoxSetIL.Location = new System.Drawing.Point(9, 29);
            this.checkBoxSetIL.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxSetIL.Name = "checkBoxSetIL";
            this.checkBoxSetIL.Size = new System.Drawing.Size(166, 24);
            this.checkBoxSetIL.TabIndex = 0;
            this.checkBoxSetIL.Text = "Set Integrity Level:";
            this.checkBoxSetIL.UseVisualStyleBackColor = true;
            this.checkBoxSetIL.CheckedChanged += new System.EventHandler(this.checkBoxSetIL_CheckedChanged);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(346, 527);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(112, 35);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(513, 527);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 35);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // SelectSecurityCheckForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(972, 572);
            this.Controls.Add(groupBoxOptions);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(groupBoxAccessToken);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectSecurityCheckForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Security Options";
            groupBoxAccessToken.ResumeLayout(false);
            groupBoxAccessToken.PerformLayout();
            groupBoxOptions.ResumeLayout(false);
            groupBoxOptions.PerformLayout();
            this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.RadioButton radioCurrentProcess;
    private System.Windows.Forms.RadioButton radioSpecificProcess;
    private System.Windows.Forms.ComboBox comboBoxIL;
    private System.Windows.Forms.CheckBox checkBoxSetIL;
    private System.Windows.Forms.CheckBox checkBoxRemoteActivate;
    private System.Windows.Forms.CheckBox checkBoxLocalActivate;
    private System.Windows.Forms.CheckBox checkBoxRemoteLaunch;
    private System.Windows.Forms.CheckBox checkBoxLocalLaunch;
    private System.Windows.Forms.TextBox textBoxPrincipal;
    private System.Windows.Forms.CheckBox checkBoxRemoteAccess;
    private System.Windows.Forms.CheckBox checkBoxLocalAccess;
    private System.Windows.Forms.RadioButton radioAnonymous;
    private SelectProcessControl selectProcessControl;
    private System.Windows.Forms.CheckBox checkBoxIgnoreDefault;
    private System.Windows.Forms.RadioButton radioLogonUser;
    private System.Windows.Forms.CheckBox checkBoxS4U;
    private System.Windows.Forms.TextBox textBoxPassword;
    private System.Windows.Forms.TextBox textBoxUsername;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.Label lblUserName;
}