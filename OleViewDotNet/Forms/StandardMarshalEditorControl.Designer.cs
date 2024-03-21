namespace OleViewDotNet.Forms;

partial class StandardMarshalEditorControl
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.Windows.Forms.Label label1;
        System.Windows.Forms.Label label2;
        System.Windows.Forms.Label label3;
        System.Windows.Forms.Label label4;
        System.Windows.Forms.Label label5;
        System.Windows.Forms.Label label6;
        System.Windows.Forms.Label label7;
        System.Windows.Forms.Label label8;
        System.Windows.Forms.Label label11;
        System.Windows.Forms.Label label12;
        System.Windows.Forms.ColumnHeader columnHeaderTowerId;
        System.Windows.Forms.ColumnHeader columnHeaderAddress;
        this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        this.textBoxStandardFlags = new System.Windows.Forms.TextBox();
        this.textBoxPublicRefs = new System.Windows.Forms.TextBox();
        this.textBoxOxid = new System.Windows.Forms.TextBox();
        this.textBoxOid = new System.Windows.Forms.TextBox();
        this.textBoxIpid = new System.Windows.Forms.TextBox();
        this.textBoxApartmentId = new System.Windows.Forms.TextBox();
        this.textBoxProcessName = new System.Windows.Forms.TextBox();
        this.lblHandlerClsid = new System.Windows.Forms.Label();
        this.textBoxHandlerClsid = new System.Windows.Forms.TextBox();
        this.lblHandlerName = new System.Windows.Forms.Label();
        this.textBoxHandlerName = new System.Windows.Forms.TextBox();
        this.listViewStringBindings = new System.Windows.Forms.ListView();
        this.listViewSecurityBindings = new System.Windows.Forms.ListView();
        this.columnHeaderAuthnSvc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.columnHeaderPrincipalName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        this.textBoxProcessId = new System.Windows.Forms.TextBox();
        this.btnViewProcess = new System.Windows.Forms.Button();
        label1 = new System.Windows.Forms.Label();
        label2 = new System.Windows.Forms.Label();
        label3 = new System.Windows.Forms.Label();
        label4 = new System.Windows.Forms.Label();
        label5 = new System.Windows.Forms.Label();
        label6 = new System.Windows.Forms.Label();
        label7 = new System.Windows.Forms.Label();
        label8 = new System.Windows.Forms.Label();
        label11 = new System.Windows.Forms.Label();
        label12 = new System.Windows.Forms.Label();
        columnHeaderTowerId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        columnHeaderAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.tableLayoutPanel.SuspendLayout();
        this.tableLayoutPanel1.SuspendLayout();
        this.SuspendLayout();
        // 
        // label1
        // 
        label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(4, 6);
        label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(108, 17);
        label1.TabIndex = 0;
        label1.Text = "Standard Flags:";
        // 
        // label2
        // 
        label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(488, 6);
        label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(83, 17);
        label2.TabIndex = 2;
        label2.Text = "Public Refs:";
        // 
        // label3
        // 
        label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label3.AutoSize = true;
        label3.Location = new System.Drawing.Point(4, 36);
        label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(45, 17);
        label3.TabIndex = 4;
        label3.Text = "OXID:";
        // 
        // label4
        // 
        label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label4.AutoSize = true;
        label4.Location = new System.Drawing.Point(488, 36);
        label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        label4.Name = "label4";
        label4.Size = new System.Drawing.Size(36, 17);
        label4.TabIndex = 6;
        label4.Text = "OID:";
        // 
        // label5
        // 
        label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label5.AutoSize = true;
        label5.Location = new System.Drawing.Point(4, 66);
        label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        label5.Name = "label5";
        label5.Size = new System.Drawing.Size(37, 17);
        label5.TabIndex = 8;
        label5.Text = "IPID:";
        // 
        // label6
        // 
        label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label6.AutoSize = true;
        label6.Location = new System.Drawing.Point(488, 66);
        label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        label6.Name = "label6";
        label6.Size = new System.Drawing.Size(77, 17);
        label6.TabIndex = 10;
        label6.Text = "Apartment:";
        // 
        // label7
        // 
        label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label7.AutoSize = true;
        label7.Location = new System.Drawing.Point(4, 96);
        label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        label7.Name = "label7";
        label7.Size = new System.Drawing.Size(80, 17);
        label7.TabIndex = 12;
        label7.Text = "Process ID:";
        // 
        // label8
        // 
        label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label8.AutoSize = true;
        label8.Location = new System.Drawing.Point(488, 96);
        label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        label8.Name = "label8";
        label8.Size = new System.Drawing.Size(104, 17);
        label8.TabIndex = 14;
        label8.Text = "Process Name:";
        // 
        // label11
        // 
        label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        label11.AutoSize = true;
        label11.Location = new System.Drawing.Point(4, 150);
        label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        label11.Name = "label11";
        label11.Size = new System.Drawing.Size(107, 17);
        label11.TabIndex = 20;
        label11.Text = "String Bindings:";
        // 
        // label12
        // 
        label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        label12.AutoSize = true;
        label12.Location = new System.Drawing.Point(488, 150);
        label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        label12.Name = "label12";
        label12.Size = new System.Drawing.Size(121, 17);
        label12.TabIndex = 21;
        label12.Text = "Security Bindings:";
        // 
        // columnHeaderTowerId
        // 
        columnHeaderTowerId.Text = "Tower ID";
        // 
        // columnHeaderAddress
        // 
        columnHeaderAddress.Text = "Network Address";
        // 
        // tableLayoutPanel
        // 
        this.tableLayoutPanel.ColumnCount = 4;
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.tableLayoutPanel.Controls.Add(label1, 0, 0);
        this.tableLayoutPanel.Controls.Add(this.textBoxStandardFlags, 1, 0);
        this.tableLayoutPanel.Controls.Add(label2, 2, 0);
        this.tableLayoutPanel.Controls.Add(this.textBoxPublicRefs, 3, 0);
        this.tableLayoutPanel.Controls.Add(label3, 0, 1);
        this.tableLayoutPanel.Controls.Add(this.textBoxOxid, 1, 1);
        this.tableLayoutPanel.Controls.Add(label4, 2, 1);
        this.tableLayoutPanel.Controls.Add(this.textBoxOid, 3, 1);
        this.tableLayoutPanel.Controls.Add(label5, 0, 2);
        this.tableLayoutPanel.Controls.Add(this.textBoxIpid, 1, 2);
        this.tableLayoutPanel.Controls.Add(label6, 2, 2);
        this.tableLayoutPanel.Controls.Add(this.textBoxApartmentId, 3, 2);
        this.tableLayoutPanel.Controls.Add(label7, 0, 3);
        this.tableLayoutPanel.Controls.Add(label8, 2, 3);
        this.tableLayoutPanel.Controls.Add(this.textBoxProcessName, 3, 3);
        this.tableLayoutPanel.Controls.Add(this.lblHandlerClsid, 0, 4);
        this.tableLayoutPanel.Controls.Add(this.textBoxHandlerClsid, 1, 4);
        this.tableLayoutPanel.Controls.Add(this.lblHandlerName, 2, 4);
        this.tableLayoutPanel.Controls.Add(this.textBoxHandlerName, 3, 4);
        this.tableLayoutPanel.Controls.Add(label11, 0, 5);
        this.tableLayoutPanel.Controls.Add(label12, 2, 5);
        this.tableLayoutPanel.Controls.Add(this.listViewStringBindings, 0, 6);
        this.tableLayoutPanel.Controls.Add(this.listViewSecurityBindings, 2, 6);
        this.tableLayoutPanel.Controls.Add(this.tableLayoutPanel1, 1, 3);
        this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.tableLayoutPanel.Name = "tableLayoutPanel";
        this.tableLayoutPanel.RowCount = 7;
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel.Size = new System.Drawing.Size(981, 636);
        this.tableLayoutPanel.TabIndex = 0;
        // 
        // textBoxStandardFlags
        // 
        this.textBoxStandardFlags.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxStandardFlags.Location = new System.Drawing.Point(120, 4);
        this.textBoxStandardFlags.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.textBoxStandardFlags.Name = "textBoxStandardFlags";
        this.textBoxStandardFlags.ReadOnly = true;
        this.textBoxStandardFlags.Size = new System.Drawing.Size(360, 22);
        this.textBoxStandardFlags.TabIndex = 0;
        // 
        // textBoxPublicRefs
        // 
        this.textBoxPublicRefs.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxPublicRefs.Location = new System.Drawing.Point(617, 4);
        this.textBoxPublicRefs.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.textBoxPublicRefs.Name = "textBoxPublicRefs";
        this.textBoxPublicRefs.ReadOnly = true;
        this.textBoxPublicRefs.Size = new System.Drawing.Size(360, 22);
        this.textBoxPublicRefs.TabIndex = 1;
        // 
        // textBoxOxid
        // 
        this.textBoxOxid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxOxid.Location = new System.Drawing.Point(120, 34);
        this.textBoxOxid.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.textBoxOxid.Name = "textBoxOxid";
        this.textBoxOxid.ReadOnly = true;
        this.textBoxOxid.Size = new System.Drawing.Size(360, 22);
        this.textBoxOxid.TabIndex = 2;
        // 
        // textBoxOid
        // 
        this.textBoxOid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxOid.Location = new System.Drawing.Point(617, 34);
        this.textBoxOid.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.textBoxOid.Name = "textBoxOid";
        this.textBoxOid.ReadOnly = true;
        this.textBoxOid.Size = new System.Drawing.Size(360, 22);
        this.textBoxOid.TabIndex = 3;
        // 
        // textBoxIpid
        // 
        this.textBoxIpid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxIpid.Location = new System.Drawing.Point(120, 64);
        this.textBoxIpid.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.textBoxIpid.Name = "textBoxIpid";
        this.textBoxIpid.ReadOnly = true;
        this.textBoxIpid.Size = new System.Drawing.Size(360, 22);
        this.textBoxIpid.TabIndex = 4;
        // 
        // textBoxApartmentId
        // 
        this.textBoxApartmentId.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxApartmentId.Location = new System.Drawing.Point(617, 64);
        this.textBoxApartmentId.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.textBoxApartmentId.Name = "textBoxApartmentId";
        this.textBoxApartmentId.ReadOnly = true;
        this.textBoxApartmentId.Size = new System.Drawing.Size(360, 22);
        this.textBoxApartmentId.TabIndex = 5;
        // 
        // textBoxProcessName
        // 
        this.textBoxProcessName.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxProcessName.Location = new System.Drawing.Point(617, 94);
        this.textBoxProcessName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.textBoxProcessName.Name = "textBoxProcessName";
        this.textBoxProcessName.ReadOnly = true;
        this.textBoxProcessName.Size = new System.Drawing.Size(360, 22);
        this.textBoxProcessName.TabIndex = 6;
        // 
        // lblHandlerClsid
        // 
        this.lblHandlerClsid.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblHandlerClsid.AutoSize = true;
        this.lblHandlerClsid.Location = new System.Drawing.Point(4, 126);
        this.lblHandlerClsid.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        this.lblHandlerClsid.Name = "lblHandlerClsid";
        this.lblHandlerClsid.Size = new System.Drawing.Size(105, 17);
        this.lblHandlerClsid.TabIndex = 16;
        this.lblHandlerClsid.Text = "Handler CLSID:";
        // 
        // textBoxHandlerClsid
        // 
        this.textBoxHandlerClsid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxHandlerClsid.Location = new System.Drawing.Point(120, 124);
        this.textBoxHandlerClsid.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.textBoxHandlerClsid.Name = "textBoxHandlerClsid";
        this.textBoxHandlerClsid.ReadOnly = true;
        this.textBoxHandlerClsid.Size = new System.Drawing.Size(360, 22);
        this.textBoxHandlerClsid.TabIndex = 7;
        // 
        // lblHandlerName
        // 
        this.lblHandlerName.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblHandlerName.AutoSize = true;
        this.lblHandlerName.Location = new System.Drawing.Point(488, 126);
        this.lblHandlerName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
        this.lblHandlerName.Name = "lblHandlerName";
        this.lblHandlerName.Size = new System.Drawing.Size(103, 17);
        this.lblHandlerName.TabIndex = 18;
        this.lblHandlerName.Text = "Handler Name:";
        // 
        // textBoxHandlerName
        // 
        this.textBoxHandlerName.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxHandlerName.Location = new System.Drawing.Point(617, 124);
        this.textBoxHandlerName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.textBoxHandlerName.Name = "textBoxHandlerName";
        this.textBoxHandlerName.ReadOnly = true;
        this.textBoxHandlerName.Size = new System.Drawing.Size(360, 22);
        this.textBoxHandlerName.TabIndex = 8;
        // 
        // listViewStringBindings
        // 
        this.listViewStringBindings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        columnHeaderTowerId,
        columnHeaderAddress});
        this.tableLayoutPanel.SetColumnSpan(this.listViewStringBindings, 2);
        this.listViewStringBindings.Dock = System.Windows.Forms.DockStyle.Fill;
        this.listViewStringBindings.FullRowSelect = true;
        this.listViewStringBindings.Location = new System.Drawing.Point(4, 171);
        this.listViewStringBindings.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.listViewStringBindings.MultiSelect = false;
        this.listViewStringBindings.Name = "listViewStringBindings";
        this.listViewStringBindings.Size = new System.Drawing.Size(476, 461);
        this.listViewStringBindings.TabIndex = 9;
        this.listViewStringBindings.UseCompatibleStateImageBehavior = false;
        this.listViewStringBindings.View = System.Windows.Forms.View.Details;
        this.listViewStringBindings.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
        // 
        // listViewSecurityBindings
        // 
        this.listViewSecurityBindings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        this.columnHeaderAuthnSvc,
        this.columnHeaderPrincipalName});
        this.tableLayoutPanel.SetColumnSpan(this.listViewSecurityBindings, 2);
        this.listViewSecurityBindings.Dock = System.Windows.Forms.DockStyle.Fill;
        this.listViewSecurityBindings.FullRowSelect = true;
        this.listViewSecurityBindings.Location = new System.Drawing.Point(488, 171);
        this.listViewSecurityBindings.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.listViewSecurityBindings.MultiSelect = false;
        this.listViewSecurityBindings.Name = "listViewSecurityBindings";
        this.listViewSecurityBindings.Size = new System.Drawing.Size(489, 461);
        this.listViewSecurityBindings.TabIndex = 10;
        this.listViewSecurityBindings.UseCompatibleStateImageBehavior = false;
        this.listViewSecurityBindings.View = System.Windows.Forms.View.Details;
        // 
        // columnHeaderAuthnSvc
        // 
        this.columnHeaderAuthnSvc.Text = "Authentication Service";
        // 
        // columnHeaderPrincipalName
        // 
        this.columnHeaderPrincipalName.Text = "Principal Name";
        // 
        // tableLayoutPanel1
        // 
        this.tableLayoutPanel1.ColumnCount = 2;
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel1.Controls.Add(this.textBoxProcessId, 0, 0);
        this.tableLayoutPanel1.Controls.Add(this.btnViewProcess, 1, 0);
        this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel1.Location = new System.Drawing.Point(116, 90);
        this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
        this.tableLayoutPanel1.Name = "tableLayoutPanel1";
        this.tableLayoutPanel1.RowCount = 1;
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel1.Size = new System.Drawing.Size(368, 30);
        this.tableLayoutPanel1.TabIndex = 24;
        // 
        // textBoxProcessId
        // 
        this.textBoxProcessId.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxProcessId.Location = new System.Drawing.Point(4, 4);
        this.textBoxProcessId.Margin = new System.Windows.Forms.Padding(4);
        this.textBoxProcessId.Name = "textBoxProcessId";
        this.textBoxProcessId.ReadOnly = true;
        this.textBoxProcessId.Size = new System.Drawing.Size(267, 22);
        this.textBoxProcessId.TabIndex = 0;
        // 
        // btnViewProcess
        // 
        this.btnViewProcess.Dock = System.Windows.Forms.DockStyle.Fill;
        this.btnViewProcess.Location = new System.Drawing.Point(278, 3);
        this.btnViewProcess.Name = "btnViewProcess";
        this.btnViewProcess.Size = new System.Drawing.Size(87, 24);
        this.btnViewProcess.TabIndex = 1;
        this.btnViewProcess.Text = "View";
        this.btnViewProcess.UseVisualStyleBackColor = true;
        this.btnViewProcess.Click += new System.EventHandler(this.btnViewProcess_Click);
        // 
        // StandardMarshalEditorControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.tableLayoutPanel);
        this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.Name = "StandardMarshalEditorControl";
        this.Size = new System.Drawing.Size(981, 636);
        this.tableLayoutPanel.ResumeLayout(false);
        this.tableLayoutPanel.PerformLayout();
        this.tableLayoutPanel1.ResumeLayout(false);
        this.tableLayoutPanel1.PerformLayout();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.TextBox textBoxStandardFlags;
    private System.Windows.Forms.TextBox textBoxPublicRefs;
    private System.Windows.Forms.TextBox textBoxOxid;
    private System.Windows.Forms.TextBox textBoxOid;
    private System.Windows.Forms.TextBox textBoxIpid;
    private System.Windows.Forms.TextBox textBoxApartmentId;
    private System.Windows.Forms.TextBox textBoxProcessName;
    private System.Windows.Forms.TextBox textBoxHandlerClsid;
    private System.Windows.Forms.TextBox textBoxHandlerName;
    private System.Windows.Forms.ListView listViewStringBindings;
    private System.Windows.Forms.ListView listViewSecurityBindings;
    private System.Windows.Forms.ColumnHeader columnHeaderAuthnSvc;
    private System.Windows.Forms.ColumnHeader columnHeaderPrincipalName;
    private System.Windows.Forms.Label lblHandlerClsid;
    private System.Windows.Forms.Label lblHandlerName;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.TextBox textBoxProcessId;
    private System.Windows.Forms.Button btnViewProcess;
}
