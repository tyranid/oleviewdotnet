namespace OleViewDotNet
{
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
            if (disposing && (components != null))
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
            System.Windows.Forms.ColumnHeader columnHeaderPID;
            System.Windows.Forms.ColumnHeader columnHeaderName;
            System.Windows.Forms.ColumnHeader columnHeaderUser;
            System.Windows.Forms.ColumnHeader columnHeaderIL;
            System.Windows.Forms.GroupBox groupBoxAccessToken;
            System.Windows.Forms.GroupBox groupBoxOptions;
            System.Windows.Forms.Label label1;
            this.radioSpecificProcess = new System.Windows.Forms.RadioButton();
            this.radioCurrentProcess = new System.Windows.Forms.RadioButton();
            this.listViewProcesses = new System.Windows.Forms.ListView();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.checkBoxSetIL = new System.Windows.Forms.CheckBox();
            this.comboBoxIL = new System.Windows.Forms.ComboBox();
            this.checkBoxLocalAccess = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoteAccess = new System.Windows.Forms.CheckBox();
            this.textBoxPrincipal = new System.Windows.Forms.TextBox();
            this.checkBoxLocalLaunch = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoteLaunch = new System.Windows.Forms.CheckBox();
            this.checkBoxLocalActivate = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoteActivate = new System.Windows.Forms.CheckBox();
            columnHeaderPID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeaderUser = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeaderIL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            groupBoxAccessToken = new System.Windows.Forms.GroupBox();
            groupBoxOptions = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            groupBoxAccessToken.SuspendLayout();
            groupBoxOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnHeaderPID
            // 
            columnHeaderPID.Text = "PID";
            // 
            // columnHeaderName
            // 
            columnHeaderName.Text = "Name";
            // 
            // columnHeaderUser
            // 
            columnHeaderUser.Text = "User";
            // 
            // columnHeaderIL
            // 
            columnHeaderIL.Text = "Integrity";
            // 
            // groupBoxAccessToken
            // 
            groupBoxAccessToken.Controls.Add(this.radioSpecificProcess);
            groupBoxAccessToken.Controls.Add(this.radioCurrentProcess);
            groupBoxAccessToken.Controls.Add(this.listViewProcesses);
            groupBoxAccessToken.Location = new System.Drawing.Point(9, 12);
            groupBoxAccessToken.Name = "groupBoxAccessToken";
            groupBoxAccessToken.Size = new System.Drawing.Size(634, 217);
            groupBoxAccessToken.TabIndex = 4;
            groupBoxAccessToken.TabStop = false;
            groupBoxAccessToken.Text = "Access Token";
            // 
            // radioSpecificProcess
            // 
            this.radioSpecificProcess.AutoSize = true;
            this.radioSpecificProcess.Location = new System.Drawing.Point(6, 42);
            this.radioSpecificProcess.Name = "radioSpecificProcess";
            this.radioSpecificProcess.Size = new System.Drawing.Size(104, 17);
            this.radioSpecificProcess.TabIndex = 4;
            this.radioSpecificProcess.Text = "Specific Process";
            this.radioSpecificProcess.UseVisualStyleBackColor = true;
            this.radioSpecificProcess.CheckedChanged += new System.EventHandler(this.radioSpecificProcess_CheckedChanged);
            // 
            // radioCurrentProcess
            // 
            this.radioCurrentProcess.AutoSize = true;
            this.radioCurrentProcess.Checked = true;
            this.radioCurrentProcess.Location = new System.Drawing.Point(6, 19);
            this.radioCurrentProcess.Name = "radioCurrentProcess";
            this.radioCurrentProcess.Size = new System.Drawing.Size(100, 17);
            this.radioCurrentProcess.TabIndex = 3;
            this.radioCurrentProcess.TabStop = true;
            this.radioCurrentProcess.Text = "Current Process";
            this.radioCurrentProcess.UseVisualStyleBackColor = true;
            // 
            // listViewProcesses
            // 
            this.listViewProcesses.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeaderPID,
            columnHeaderName,
            columnHeaderUser,
            columnHeaderIL});
            this.listViewProcesses.Enabled = false;
            this.listViewProcesses.FullRowSelect = true;
            this.listViewProcesses.Location = new System.Drawing.Point(3, 62);
            this.listViewProcesses.MultiSelect = false;
            this.listViewProcesses.Name = "listViewProcesses";
            this.listViewProcesses.Size = new System.Drawing.Size(627, 149);
            this.listViewProcesses.TabIndex = 0;
            this.listViewProcesses.UseCompatibleStateImageBehavior = false;
            this.listViewProcesses.View = System.Windows.Forms.View.Details;
            this.listViewProcesses.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewProcesses_ColumnClick);
            // 
            // groupBoxOptions
            // 
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
            groupBoxOptions.Location = new System.Drawing.Point(9, 235);
            groupBoxOptions.Name = "groupBoxOptions";
            groupBoxOptions.Size = new System.Drawing.Size(630, 79);
            groupBoxOptions.TabIndex = 5;
            groupBoxOptions.TabStop = false;
            groupBoxOptions.Text = "Options";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(231, 320);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(342, 320);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // checkBoxSetIL
            // 
            this.checkBoxSetIL.AutoSize = true;
            this.checkBoxSetIL.Location = new System.Drawing.Point(6, 19);
            this.checkBoxSetIL.Name = "checkBoxSetIL";
            this.checkBoxSetIL.Size = new System.Drawing.Size(114, 17);
            this.checkBoxSetIL.TabIndex = 0;
            this.checkBoxSetIL.Text = "Set Integrity Level:";
            this.checkBoxSetIL.UseVisualStyleBackColor = true;
            this.checkBoxSetIL.CheckedChanged += new System.EventHandler(this.checkBoxSetIL_CheckedChanged);
            // 
            // comboBoxIL
            // 
            this.comboBoxIL.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIL.Enabled = false;
            this.comboBoxIL.FormattingEnabled = true;
            this.comboBoxIL.Location = new System.Drawing.Point(126, 15);
            this.comboBoxIL.Name = "comboBoxIL";
            this.comboBoxIL.Size = new System.Drawing.Size(121, 21);
            this.comboBoxIL.TabIndex = 1;
            // 
            // checkBoxLocalAccess
            // 
            this.checkBoxLocalAccess.AutoSize = true;
            this.checkBoxLocalAccess.Checked = true;
            this.checkBoxLocalAccess.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLocalAccess.Location = new System.Drawing.Point(6, 42);
            this.checkBoxLocalAccess.Name = "checkBoxLocalAccess";
            this.checkBoxLocalAccess.Size = new System.Drawing.Size(90, 17);
            this.checkBoxLocalAccess.TabIndex = 2;
            this.checkBoxLocalAccess.Text = "Local Access";
            this.checkBoxLocalAccess.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoteAccess
            // 
            this.checkBoxRemoteAccess.AutoSize = true;
            this.checkBoxRemoteAccess.Location = new System.Drawing.Point(102, 42);
            this.checkBoxRemoteAccess.Name = "checkBoxRemoteAccess";
            this.checkBoxRemoteAccess.Size = new System.Drawing.Size(101, 17);
            this.checkBoxRemoteAccess.TabIndex = 3;
            this.checkBoxRemoteAccess.Text = "Remote Access";
            this.checkBoxRemoteAccess.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(262, 19);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(50, 13);
            label1.TabIndex = 4;
            label1.Text = "Principal:";
            // 
            // textBoxPrincipal
            // 
            this.textBoxPrincipal.Location = new System.Drawing.Point(318, 17);
            this.textBoxPrincipal.Name = "textBoxPrincipal";
            this.textBoxPrincipal.Size = new System.Drawing.Size(306, 20);
            this.textBoxPrincipal.TabIndex = 5;
            // 
            // checkBoxLocalLaunch
            // 
            this.checkBoxLocalLaunch.AutoSize = true;
            this.checkBoxLocalLaunch.Checked = true;
            this.checkBoxLocalLaunch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLocalLaunch.Location = new System.Drawing.Point(209, 42);
            this.checkBoxLocalLaunch.Name = "checkBoxLocalLaunch";
            this.checkBoxLocalLaunch.Size = new System.Drawing.Size(91, 17);
            this.checkBoxLocalLaunch.TabIndex = 6;
            this.checkBoxLocalLaunch.Text = "Local Launch";
            this.checkBoxLocalLaunch.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoteLaunch
            // 
            this.checkBoxRemoteLaunch.AutoSize = true;
            this.checkBoxRemoteLaunch.Location = new System.Drawing.Point(306, 42);
            this.checkBoxRemoteLaunch.Name = "checkBoxRemoteLaunch";
            this.checkBoxRemoteLaunch.Size = new System.Drawing.Size(102, 17);
            this.checkBoxRemoteLaunch.TabIndex = 7;
            this.checkBoxRemoteLaunch.Text = "Remote Launch";
            this.checkBoxRemoteLaunch.UseVisualStyleBackColor = true;
            // 
            // checkBoxLocalActivate
            // 
            this.checkBoxLocalActivate.AutoSize = true;
            this.checkBoxLocalActivate.Checked = true;
            this.checkBoxLocalActivate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLocalActivate.Location = new System.Drawing.Point(414, 42);
            this.checkBoxLocalActivate.Name = "checkBoxLocalActivate";
            this.checkBoxLocalActivate.Size = new System.Drawing.Size(94, 17);
            this.checkBoxLocalActivate.TabIndex = 8;
            this.checkBoxLocalActivate.Text = "Local Activate";
            this.checkBoxLocalActivate.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoteActivate
            // 
            this.checkBoxRemoteActivate.AutoSize = true;
            this.checkBoxRemoteActivate.Location = new System.Drawing.Point(514, 42);
            this.checkBoxRemoteActivate.Name = "checkBoxRemoteActivate";
            this.checkBoxRemoteActivate.Size = new System.Drawing.Size(105, 17);
            this.checkBoxRemoteActivate.TabIndex = 9;
            this.checkBoxRemoteActivate.Text = "Remote Activate";
            this.checkBoxRemoteActivate.UseVisualStyleBackColor = true;
            // 
            // SelectSecurityCheckForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(648, 355);
            this.Controls.Add(groupBoxOptions);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(groupBoxAccessToken);
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

        private System.Windows.Forms.ListView listViewProcesses;
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
    }
}