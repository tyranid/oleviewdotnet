namespace OleViewDotNet
{
    partial class PropertiesControl
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
            System.Windows.Forms.Label lblClsid;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            System.Windows.Forms.Label label7;
            System.Windows.Forms.Label label8;
            System.Windows.Forms.Label label9;
            System.Windows.Forms.Label label10;
            System.Windows.Forms.Label label11;
            System.Windows.Forms.Label label12;
            this.tabControlProperties = new System.Windows.Forms.TabControl();
            this.tabPageClsid = new System.Windows.Forms.TabPage();
            this.textBoxServer = new System.Windows.Forms.TextBox();
            this.listViewCategories = new System.Windows.Forms.ListView();
            this.listViewProgIDs = new System.Windows.Forms.ListView();
            this.textBoxClsidName = new System.Windows.Forms.TextBox();
            this.lblThreadingModel = new System.Windows.Forms.Label();
            this.lblServerType = new System.Windows.Forms.Label();
            this.textBoxClsid = new System.Windows.Forms.TextBox();
            this.tabPageNoProperties = new System.Windows.Forms.TabPage();
            this.tabPageSupportedInterfaces = new System.Windows.Forms.TabPage();
            this.btnRefreshInterfaces = new System.Windows.Forms.Button();
            this.listViewFactoryInterfaces = new System.Windows.Forms.ListView();
            this.listViewInterfaces = new System.Windows.Forms.ListView();
            this.tabPageAppID = new System.Windows.Forms.TabPage();
            this.textBoxDllSurrogate = new System.Windows.Forms.TextBox();
            this.lblService = new System.Windows.Forms.Label();
            this.lblAppIdRunAs = new System.Windows.Forms.Label();
            this.textBoxAccessPermission = new System.Windows.Forms.TextBox();
            this.textBoxLaunchPermission = new System.Windows.Forms.TextBox();
            this.textBoxAppIdName = new System.Windows.Forms.TextBox();
            this.textBoxAppIdGuid = new System.Windows.Forms.TextBox();
            this.splitContainerInterfaces = new System.Windows.Forms.SplitContainer();
            lblClsid = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            label12 = new System.Windows.Forms.Label();
            this.tabControlProperties.SuspendLayout();
            this.tabPageClsid.SuspendLayout();
            this.tabPageNoProperties.SuspendLayout();
            this.tabPageSupportedInterfaces.SuspendLayout();
            this.tabPageAppID.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerInterfaces)).BeginInit();
            this.splitContainerInterfaces.Panel1.SuspendLayout();
            this.splitContainerInterfaces.Panel2.SuspendLayout();
            this.splitContainerInterfaces.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblClsid
            // 
            lblClsid.AutoSize = true;
            lblClsid.Location = new System.Drawing.Point(11, 44);
            lblClsid.Name = "lblClsid";
            lblClsid.Size = new System.Drawing.Size(41, 13);
            lblClsid.TabIndex = 0;
            lblClsid.Text = "CLSID:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(11, 14);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(38, 13);
            label1.TabIndex = 4;
            label1.Text = "Name:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(18, 13);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(71, 13);
            label2.TabIndex = 0;
            label2.Text = "No Properties";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(11, 150);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(48, 13);
            label3.TabIndex = 7;
            label3.Text = "ProgIDs:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(11, 251);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(60, 13);
            label4.TabIndex = 9;
            label4.Text = "Categories:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(3, 11);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(57, 13);
            label5.TabIndex = 9;
            label5.Text = "Interfaces:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(3, 9);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(95, 13);
            label6.TabIndex = 11;
            label6.Text = "Factory Interfaces:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(11, 99);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(41, 13);
            label7.TabIndex = 10;
            label7.Text = "Server:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(7, 9);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(38, 13);
            label8.TabIndex = 8;
            label8.Text = "Name:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(7, 39);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(40, 13);
            label9.TabIndex = 6;
            label9.Text = "AppID:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(4, 125);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(99, 13);
            label10.TabIndex = 10;
            label10.Text = "Launch Permission:";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(4, 169);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(98, 13);
            label11.TabIndex = 12;
            label11.Text = "Access Permission:";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(4, 216);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(71, 13);
            label12.TabIndex = 16;
            label12.Text = "Dll Surrogate:";
            // 
            // tabControlProperties
            // 
            this.tabControlProperties.Controls.Add(this.tabPageClsid);
            this.tabControlProperties.Controls.Add(this.tabPageNoProperties);
            this.tabControlProperties.Controls.Add(this.tabPageSupportedInterfaces);
            this.tabControlProperties.Controls.Add(this.tabPageAppID);
            this.tabControlProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlProperties.Location = new System.Drawing.Point(0, 0);
            this.tabControlProperties.Name = "tabControlProperties";
            this.tabControlProperties.SelectedIndex = 0;
            this.tabControlProperties.Size = new System.Drawing.Size(693, 459);
            this.tabControlProperties.TabIndex = 0;
            // 
            // tabPageClsid
            // 
            this.tabPageClsid.Controls.Add(this.textBoxServer);
            this.tabPageClsid.Controls.Add(label7);
            this.tabPageClsid.Controls.Add(label4);
            this.tabPageClsid.Controls.Add(this.listViewCategories);
            this.tabPageClsid.Controls.Add(label3);
            this.tabPageClsid.Controls.Add(this.listViewProgIDs);
            this.tabPageClsid.Controls.Add(this.textBoxClsidName);
            this.tabPageClsid.Controls.Add(label1);
            this.tabPageClsid.Controls.Add(this.lblThreadingModel);
            this.tabPageClsid.Controls.Add(this.lblServerType);
            this.tabPageClsid.Controls.Add(this.textBoxClsid);
            this.tabPageClsid.Controls.Add(lblClsid);
            this.tabPageClsid.Location = new System.Drawing.Point(4, 22);
            this.tabPageClsid.Name = "tabPageClsid";
            this.tabPageClsid.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageClsid.Size = new System.Drawing.Size(685, 433);
            this.tabPageClsid.TabIndex = 0;
            this.tabPageClsid.Text = "CLSID";
            this.tabPageClsid.UseVisualStyleBackColor = true;
            // 
            // textBoxServer
            // 
            this.textBoxServer.Location = new System.Drawing.Point(55, 96);
            this.textBoxServer.Name = "textBoxServer";
            this.textBoxServer.ReadOnly = true;
            this.textBoxServer.Size = new System.Drawing.Size(335, 20);
            this.textBoxServer.TabIndex = 11;
            // 
            // listViewCategories
            // 
            this.listViewCategories.FullRowSelect = true;
            this.listViewCategories.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewCategories.Location = new System.Drawing.Point(11, 267);
            this.listViewCategories.MultiSelect = false;
            this.listViewCategories.Name = "listViewCategories";
            this.listViewCategories.Size = new System.Drawing.Size(379, 82);
            this.listViewCategories.TabIndex = 8;
            this.listViewCategories.UseCompatibleStateImageBehavior = false;
            this.listViewCategories.View = System.Windows.Forms.View.Details;
            // 
            // listViewProgIDs
            // 
            this.listViewProgIDs.FullRowSelect = true;
            this.listViewProgIDs.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewProgIDs.Location = new System.Drawing.Point(11, 166);
            this.listViewProgIDs.MultiSelect = false;
            this.listViewProgIDs.Name = "listViewProgIDs";
            this.listViewProgIDs.Size = new System.Drawing.Size(379, 82);
            this.listViewProgIDs.TabIndex = 6;
            this.listViewProgIDs.UseCompatibleStateImageBehavior = false;
            this.listViewProgIDs.View = System.Windows.Forms.View.Details;
            // 
            // textBoxClsidName
            // 
            this.textBoxClsidName.Location = new System.Drawing.Point(55, 11);
            this.textBoxClsidName.Name = "textBoxClsidName";
            this.textBoxClsidName.ReadOnly = true;
            this.textBoxClsidName.Size = new System.Drawing.Size(335, 20);
            this.textBoxClsidName.TabIndex = 5;
            // 
            // lblThreadingModel
            // 
            this.lblThreadingModel.AutoSize = true;
            this.lblThreadingModel.Location = new System.Drawing.Point(11, 125);
            this.lblThreadingModel.Name = "lblThreadingModel";
            this.lblThreadingModel.Size = new System.Drawing.Size(90, 13);
            this.lblThreadingModel.TabIndex = 3;
            this.lblThreadingModel.Text = "Threading Model:";
            // 
            // lblServerType
            // 
            this.lblServerType.AutoSize = true;
            this.lblServerType.Location = new System.Drawing.Point(11, 75);
            this.lblServerType.Name = "lblServerType";
            this.lblServerType.Size = new System.Drawing.Size(68, 13);
            this.lblServerType.TabIndex = 2;
            this.lblServerType.Text = "Server Type:";
            // 
            // textBoxClsid
            // 
            this.textBoxClsid.Location = new System.Drawing.Point(55, 41);
            this.textBoxClsid.Name = "textBoxClsid";
            this.textBoxClsid.ReadOnly = true;
            this.textBoxClsid.Size = new System.Drawing.Size(335, 20);
            this.textBoxClsid.TabIndex = 1;
            // 
            // tabPageNoProperties
            // 
            this.tabPageNoProperties.Controls.Add(label2);
            this.tabPageNoProperties.Location = new System.Drawing.Point(4, 22);
            this.tabPageNoProperties.Name = "tabPageNoProperties";
            this.tabPageNoProperties.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageNoProperties.Size = new System.Drawing.Size(685, 433);
            this.tabPageNoProperties.TabIndex = 1;
            this.tabPageNoProperties.Text = "No Properties";
            this.tabPageNoProperties.UseVisualStyleBackColor = true;
            // 
            // tabPageSupportedInterfaces
            // 
            this.tabPageSupportedInterfaces.Controls.Add(this.splitContainerInterfaces);
            this.tabPageSupportedInterfaces.Location = new System.Drawing.Point(4, 22);
            this.tabPageSupportedInterfaces.Name = "tabPageSupportedInterfaces";
            this.tabPageSupportedInterfaces.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSupportedInterfaces.Size = new System.Drawing.Size(685, 433);
            this.tabPageSupportedInterfaces.TabIndex = 2;
            this.tabPageSupportedInterfaces.Text = "Supported Interfaces";
            this.tabPageSupportedInterfaces.UseVisualStyleBackColor = true;
            // 
            // btnRefreshInterfaces
            // 
            this.btnRefreshInterfaces.Location = new System.Drawing.Point(66, 6);
            this.btnRefreshInterfaces.Name = "btnRefreshInterfaces";
            this.btnRefreshInterfaces.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshInterfaces.TabIndex = 12;
            this.btnRefreshInterfaces.Text = "Refresh";
            this.btnRefreshInterfaces.UseVisualStyleBackColor = true;
            this.btnRefreshInterfaces.Click += new System.EventHandler(this.btnRefreshInterfaces_Click);
            // 
            // listViewFactoryInterfaces
            // 
            this.listViewFactoryInterfaces.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFactoryInterfaces.FullRowSelect = true;
            this.listViewFactoryInterfaces.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewFactoryInterfaces.Location = new System.Drawing.Point(3, 34);
            this.listViewFactoryInterfaces.MultiSelect = false;
            this.listViewFactoryInterfaces.Name = "listViewFactoryInterfaces";
            this.listViewFactoryInterfaces.Size = new System.Drawing.Size(673, 179);
            this.listViewFactoryInterfaces.TabIndex = 10;
            this.listViewFactoryInterfaces.UseCompatibleStateImageBehavior = false;
            this.listViewFactoryInterfaces.View = System.Windows.Forms.View.Details;
            // 
            // listViewInterfaces
            // 
            this.listViewInterfaces.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewInterfaces.FullRowSelect = true;
            this.listViewInterfaces.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewInterfaces.Location = new System.Drawing.Point(3, 36);
            this.listViewInterfaces.MultiSelect = false;
            this.listViewInterfaces.Name = "listViewInterfaces";
            this.listViewInterfaces.Size = new System.Drawing.Size(673, 157);
            this.listViewInterfaces.TabIndex = 8;
            this.listViewInterfaces.UseCompatibleStateImageBehavior = false;
            this.listViewInterfaces.View = System.Windows.Forms.View.Details;
            // 
            // tabPageAppID
            // 
            this.tabPageAppID.Controls.Add(this.textBoxDllSurrogate);
            this.tabPageAppID.Controls.Add(label12);
            this.tabPageAppID.Controls.Add(this.lblService);
            this.tabPageAppID.Controls.Add(this.lblAppIdRunAs);
            this.tabPageAppID.Controls.Add(this.textBoxAccessPermission);
            this.tabPageAppID.Controls.Add(label11);
            this.tabPageAppID.Controls.Add(this.textBoxLaunchPermission);
            this.tabPageAppID.Controls.Add(label10);
            this.tabPageAppID.Controls.Add(this.textBoxAppIdName);
            this.tabPageAppID.Controls.Add(label8);
            this.tabPageAppID.Controls.Add(this.textBoxAppIdGuid);
            this.tabPageAppID.Controls.Add(label9);
            this.tabPageAppID.Location = new System.Drawing.Point(4, 22);
            this.tabPageAppID.Name = "tabPageAppID";
            this.tabPageAppID.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAppID.Size = new System.Drawing.Size(685, 433);
            this.tabPageAppID.TabIndex = 3;
            this.tabPageAppID.Text = "AppID";
            this.tabPageAppID.UseVisualStyleBackColor = true;
            // 
            // textBoxDllSurrogate
            // 
            this.textBoxDllSurrogate.Location = new System.Drawing.Point(5, 232);
            this.textBoxDllSurrogate.Name = "textBoxDllSurrogate";
            this.textBoxDllSurrogate.ReadOnly = true;
            this.textBoxDllSurrogate.Size = new System.Drawing.Size(376, 20);
            this.textBoxDllSurrogate.TabIndex = 17;
            // 
            // lblService
            // 
            this.lblService.AutoSize = true;
            this.lblService.Location = new System.Drawing.Point(6, 99);
            this.lblService.Name = "lblService";
            this.lblService.Size = new System.Drawing.Size(46, 13);
            this.lblService.TabIndex = 15;
            this.lblService.Text = "Service:";
            // 
            // lblAppIdRunAs
            // 
            this.lblAppIdRunAs.AutoSize = true;
            this.lblAppIdRunAs.Location = new System.Drawing.Point(7, 72);
            this.lblAppIdRunAs.Name = "lblAppIdRunAs";
            this.lblAppIdRunAs.Size = new System.Drawing.Size(45, 13);
            this.lblAppIdRunAs.TabIndex = 14;
            this.lblAppIdRunAs.Text = "Run As:";
            // 
            // textBoxAccessPermission
            // 
            this.textBoxAccessPermission.Location = new System.Drawing.Point(5, 185);
            this.textBoxAccessPermission.Name = "textBoxAccessPermission";
            this.textBoxAccessPermission.ReadOnly = true;
            this.textBoxAccessPermission.Size = new System.Drawing.Size(376, 20);
            this.textBoxAccessPermission.TabIndex = 13;
            // 
            // textBoxLaunchPermission
            // 
            this.textBoxLaunchPermission.Location = new System.Drawing.Point(5, 141);
            this.textBoxLaunchPermission.Name = "textBoxLaunchPermission";
            this.textBoxLaunchPermission.ReadOnly = true;
            this.textBoxLaunchPermission.Size = new System.Drawing.Size(376, 20);
            this.textBoxLaunchPermission.TabIndex = 11;
            // 
            // textBoxAppIdName
            // 
            this.textBoxAppIdName.Location = new System.Drawing.Point(51, 6);
            this.textBoxAppIdName.Name = "textBoxAppIdName";
            this.textBoxAppIdName.ReadOnly = true;
            this.textBoxAppIdName.Size = new System.Drawing.Size(335, 20);
            this.textBoxAppIdName.TabIndex = 9;
            // 
            // textBoxAppIdGuid
            // 
            this.textBoxAppIdGuid.Location = new System.Drawing.Point(51, 36);
            this.textBoxAppIdGuid.Name = "textBoxAppIdGuid";
            this.textBoxAppIdGuid.ReadOnly = true;
            this.textBoxAppIdGuid.Size = new System.Drawing.Size(335, 20);
            this.textBoxAppIdGuid.TabIndex = 7;
            // 
            // splitContainerInterfaces
            // 
            this.splitContainerInterfaces.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerInterfaces.Location = new System.Drawing.Point(3, 3);
            this.splitContainerInterfaces.Name = "splitContainerInterfaces";
            this.splitContainerInterfaces.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerInterfaces.Panel1
            // 
            this.splitContainerInterfaces.Panel1.Controls.Add(label5);
            this.splitContainerInterfaces.Panel1.Controls.Add(this.btnRefreshInterfaces);
            this.splitContainerInterfaces.Panel1.Controls.Add(this.listViewInterfaces);
            // 
            // splitContainerInterfaces.Panel2
            // 
            this.splitContainerInterfaces.Panel2.Controls.Add(label6);
            this.splitContainerInterfaces.Panel2.Controls.Add(this.listViewFactoryInterfaces);
            this.splitContainerInterfaces.Size = new System.Drawing.Size(679, 427);
            this.splitContainerInterfaces.SplitterDistance = 207;
            this.splitContainerInterfaces.TabIndex = 13;
            // 
            // PropertiesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControlProperties);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "PropertiesControl";
            this.Size = new System.Drawing.Size(693, 459);
            this.tabControlProperties.ResumeLayout(false);
            this.tabPageClsid.ResumeLayout(false);
            this.tabPageClsid.PerformLayout();
            this.tabPageNoProperties.ResumeLayout(false);
            this.tabPageNoProperties.PerformLayout();
            this.tabPageSupportedInterfaces.ResumeLayout(false);
            this.tabPageAppID.ResumeLayout(false);
            this.tabPageAppID.PerformLayout();
            this.splitContainerInterfaces.Panel1.ResumeLayout(false);
            this.splitContainerInterfaces.Panel1.PerformLayout();
            this.splitContainerInterfaces.Panel2.ResumeLayout(false);
            this.splitContainerInterfaces.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerInterfaces)).EndInit();
            this.splitContainerInterfaces.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlProperties;
        private System.Windows.Forms.TabPage tabPageClsid;
        private System.Windows.Forms.TabPage tabPageNoProperties;
        private System.Windows.Forms.TextBox textBoxClsid;
        private System.Windows.Forms.Label lblServerType;
        private System.Windows.Forms.Label lblThreadingModel;
        private System.Windows.Forms.TextBox textBoxClsidName;
        private System.Windows.Forms.ListView listViewProgIDs;
        private System.Windows.Forms.ListView listViewCategories;
        private System.Windows.Forms.TabPage tabPageSupportedInterfaces;
        private System.Windows.Forms.ListView listViewFactoryInterfaces;
        private System.Windows.Forms.ListView listViewInterfaces;
        private System.Windows.Forms.Button btnRefreshInterfaces;
        private System.Windows.Forms.TextBox textBoxServer;
        private System.Windows.Forms.TabPage tabPageAppID;
        private System.Windows.Forms.TextBox textBoxAppIdName;
        private System.Windows.Forms.TextBox textBoxAppIdGuid;
        private System.Windows.Forms.TextBox textBoxAccessPermission;
        private System.Windows.Forms.TextBox textBoxLaunchPermission;
        private System.Windows.Forms.Label lblAppIdRunAs;
        private System.Windows.Forms.Label lblService;
        private System.Windows.Forms.TextBox textBoxDllSurrogate;
        private System.Windows.Forms.SplitContainer splitContainerInterfaces;
    }
}