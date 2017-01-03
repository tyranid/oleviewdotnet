namespace OleViewDotNet
{
    partial class COMRegistryViewer
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label labelFilter;
            System.Windows.Forms.Label labelMode;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(COMRegistryViewer));
            this.treeComRegistry = new System.Windows.Forms.TreeView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGUIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGUIDCStructureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGUIDHexStringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyObjectTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createInstanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshInterfacesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createSpecialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createLocalServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createInProcServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createInProcHandlerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createRemoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createClassFactoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createClassFactoryRemoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createInSessionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.consoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createElevatedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.instanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.classFactoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewTypeLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewProxyDefinitionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewLaunchPermissionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewAccessPermissionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.queryAllInterfacesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeImageList = new System.Windows.Forms.ImageList(this.components);
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnApply = new System.Windows.Forms.Button();
            labelFilter = new System.Windows.Forms.Label();
            labelMode = new System.Windows.Forms.Label();
            this.contextMenuStrip.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelFilter
            // 
            labelFilter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelFilter.AutoSize = true;
            labelFilter.Location = new System.Drawing.Point(3, 8);
            labelFilter.Name = "labelFilter";
            labelFilter.Size = new System.Drawing.Size(32, 13);
            labelFilter.TabIndex = 1;
            labelFilter.Text = "Filter:";
            // 
            // labelMode
            // 
            labelMode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelMode.AutoSize = true;
            labelMode.Location = new System.Drawing.Point(759, 8);
            labelMode.Name = "labelMode";
            labelMode.Size = new System.Drawing.Size(37, 13);
            labelMode.TabIndex = 5;
            labelMode.Text = "Mode:";
            // 
            // treeComRegistry
            // 
            this.tableLayoutPanel.SetColumnSpan(this.treeComRegistry, 5);
            this.treeComRegistry.ContextMenuStrip = this.contextMenuStrip;
            this.treeComRegistry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeComRegistry.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeComRegistry.ImageIndex = 0;
            this.treeComRegistry.ImageList = this.treeImageList;
            this.treeComRegistry.Location = new System.Drawing.Point(3, 32);
            this.treeComRegistry.Name = "treeComRegistry";
            this.treeComRegistry.SelectedImageIndex = 0;
            this.treeComRegistry.ShowNodeToolTips = true;
            this.treeComRegistry.Size = new System.Drawing.Size(1046, 495);
            this.treeComRegistry.TabIndex = 0;
            this.treeComRegistry.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeComRegistry_AfterCollapse);
            this.treeComRegistry.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeComRegistry_BeforeExpand);
            this.treeComRegistry.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeComRegistry_AfterExpand);
            this.treeComRegistry.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeComRegistry_MouseDown);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.copyGUIDToolStripMenuItem,
            this.copyGUIDCStructureToolStripMenuItem,
            this.copyGUIDHexStringToolStripMenuItem,
            this.copyObjectTagToolStripMenuItem,
            this.createInstanceToolStripMenuItem,
            this.refreshInterfacesToolStripMenuItem,
            this.createSpecialToolStripMenuItem,
            this.viewTypeLibraryToolStripMenuItem,
            this.viewProxyDefinitionToolStripMenuItem,
            this.viewLaunchPermissionsToolStripMenuItem,
            this.viewAccessPermissionsToolStripMenuItem,
            this.queryAllInterfacesToolStripMenuItem,
            this.propertiesToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(208, 312);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // copyGUIDToolStripMenuItem
            // 
            this.copyGUIDToolStripMenuItem.Name = "copyGUIDToolStripMenuItem";
            this.copyGUIDToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.copyGUIDToolStripMenuItem.Text = "Copy GUID";
            this.copyGUIDToolStripMenuItem.Click += new System.EventHandler(this.copyGUIDToolStripMenuItem_Click);
            // 
            // copyGUIDCStructureToolStripMenuItem
            // 
            this.copyGUIDCStructureToolStripMenuItem.Name = "copyGUIDCStructureToolStripMenuItem";
            this.copyGUIDCStructureToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.copyGUIDCStructureToolStripMenuItem.Text = "Copy GUID C Structure";
            this.copyGUIDCStructureToolStripMenuItem.Click += new System.EventHandler(this.copyGUIDCStructureToolStripMenuItem_Click);
            // 
            // copyGUIDHexStringToolStripMenuItem
            // 
            this.copyGUIDHexStringToolStripMenuItem.Name = "copyGUIDHexStringToolStripMenuItem";
            this.copyGUIDHexStringToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.copyGUIDHexStringToolStripMenuItem.Text = "Copy GUID Hex String";
            this.copyGUIDHexStringToolStripMenuItem.Click += new System.EventHandler(this.copyGUIDHexStringToolStripMenuItem_Click);
            // 
            // copyObjectTagToolStripMenuItem
            // 
            this.copyObjectTagToolStripMenuItem.Name = "copyObjectTagToolStripMenuItem";
            this.copyObjectTagToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.copyObjectTagToolStripMenuItem.Text = "Copy Object Tag";
            this.copyObjectTagToolStripMenuItem.Click += new System.EventHandler(this.copyObjectTagToolStripMenuItem_Click);
            // 
            // createInstanceToolStripMenuItem
            // 
            this.createInstanceToolStripMenuItem.Name = "createInstanceToolStripMenuItem";
            this.createInstanceToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.createInstanceToolStripMenuItem.Text = "Create Instance";
            this.createInstanceToolStripMenuItem.Click += new System.EventHandler(this.createInstanceToolStripMenuItem_Click);
            // 
            // refreshInterfacesToolStripMenuItem
            // 
            this.refreshInterfacesToolStripMenuItem.Name = "refreshInterfacesToolStripMenuItem";
            this.refreshInterfacesToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.refreshInterfacesToolStripMenuItem.Text = "Refresh Interfaces";
            this.refreshInterfacesToolStripMenuItem.Click += new System.EventHandler(this.refreshInterfacesToolStripMenuItem_Click);
            // 
            // createSpecialToolStripMenuItem
            // 
            this.createSpecialToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createLocalServerToolStripMenuItem,
            this.createInProcServerToolStripMenuItem,
            this.createInProcHandlerToolStripMenuItem,
            this.createRemoteToolStripMenuItem,
            this.createClassFactoryToolStripMenuItem,
            this.createClassFactoryRemoteToolStripMenuItem,
            this.createInSessionToolStripMenuItem,
            this.createElevatedToolStripMenuItem});
            this.createSpecialToolStripMenuItem.Name = "createSpecialToolStripMenuItem";
            this.createSpecialToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.createSpecialToolStripMenuItem.Text = "Create Special";
            // 
            // createLocalServerToolStripMenuItem
            // 
            this.createLocalServerToolStripMenuItem.Name = "createLocalServerToolStripMenuItem";
            this.createLocalServerToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.createLocalServerToolStripMenuItem.Text = "Create Local Server";
            this.createLocalServerToolStripMenuItem.Click += new System.EventHandler(this.createLocalServerToolStripMenuItem_Click);
            // 
            // createInProcServerToolStripMenuItem
            // 
            this.createInProcServerToolStripMenuItem.Name = "createInProcServerToolStripMenuItem";
            this.createInProcServerToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.createInProcServerToolStripMenuItem.Text = "Create InProc Server";
            this.createInProcServerToolStripMenuItem.Click += new System.EventHandler(this.createInProcServerToolStripMenuItem_Click);
            // 
            // createInProcHandlerToolStripMenuItem
            // 
            this.createInProcHandlerToolStripMenuItem.Name = "createInProcHandlerToolStripMenuItem";
            this.createInProcHandlerToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.createInProcHandlerToolStripMenuItem.Text = "Create InProc Handler";
            this.createInProcHandlerToolStripMenuItem.Click += new System.EventHandler(this.createInProcHandlerToolStripMenuItem_Click);
            // 
            // createRemoteToolStripMenuItem
            // 
            this.createRemoteToolStripMenuItem.Name = "createRemoteToolStripMenuItem";
            this.createRemoteToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.createRemoteToolStripMenuItem.Text = "Create Remote";
            this.createRemoteToolStripMenuItem.Click += new System.EventHandler(this.createRemoteToolStripMenuItem_Click);
            // 
            // createClassFactoryToolStripMenuItem
            // 
            this.createClassFactoryToolStripMenuItem.Name = "createClassFactoryToolStripMenuItem";
            this.createClassFactoryToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.createClassFactoryToolStripMenuItem.Text = "Create Class Factory";
            this.createClassFactoryToolStripMenuItem.Click += new System.EventHandler(this.createClassFactoryToolStripMenuItem_Click);
            // 
            // createClassFactoryRemoteToolStripMenuItem
            // 
            this.createClassFactoryRemoteToolStripMenuItem.Name = "createClassFactoryRemoteToolStripMenuItem";
            this.createClassFactoryRemoteToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.createClassFactoryRemoteToolStripMenuItem.Text = "Create Class Factory Remote";
            this.createClassFactoryRemoteToolStripMenuItem.Click += new System.EventHandler(this.createClassFactoryRemoteToolStripMenuItem_Click);
            // 
            // createInSessionToolStripMenuItem
            // 
            this.createInSessionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.consoleToolStripMenuItem});
            this.createInSessionToolStripMenuItem.Name = "createInSessionToolStripMenuItem";
            this.createInSessionToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.createInSessionToolStripMenuItem.Text = "Create In Session";
            // 
            // consoleToolStripMenuItem
            // 
            this.consoleToolStripMenuItem.Name = "consoleToolStripMenuItem";
            this.consoleToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.consoleToolStripMenuItem.Tag = "Console";
            this.consoleToolStripMenuItem.Text = "Console";
            this.consoleToolStripMenuItem.Click += new System.EventHandler(this.consoleToolStripMenuItem_Click);
            // 
            // createElevatedToolStripMenuItem
            // 
            this.createElevatedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.instanceToolStripMenuItem,
            this.classFactoryToolStripMenuItem});
            this.createElevatedToolStripMenuItem.Name = "createElevatedToolStripMenuItem";
            this.createElevatedToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.createElevatedToolStripMenuItem.Text = "Create Elevated";
            // 
            // instanceToolStripMenuItem
            // 
            this.instanceToolStripMenuItem.Name = "instanceToolStripMenuItem";
            this.instanceToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.instanceToolStripMenuItem.Text = "Instance";
            this.instanceToolStripMenuItem.Click += new System.EventHandler(this.instanceToolStripMenuItem_Click);
            // 
            // classFactoryToolStripMenuItem
            // 
            this.classFactoryToolStripMenuItem.Name = "classFactoryToolStripMenuItem";
            this.classFactoryToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.classFactoryToolStripMenuItem.Text = "Class Factory";
            this.classFactoryToolStripMenuItem.Click += new System.EventHandler(this.classFactoryToolStripMenuItem_Click);
            // 
            // viewTypeLibraryToolStripMenuItem
            // 
            this.viewTypeLibraryToolStripMenuItem.Name = "viewTypeLibraryToolStripMenuItem";
            this.viewTypeLibraryToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.viewTypeLibraryToolStripMenuItem.Text = "View Type Library";
            this.viewTypeLibraryToolStripMenuItem.Click += new System.EventHandler(this.viewTypeLibraryToolStripMenuItem_Click);
            // 
            // viewProxyDefinitionToolStripMenuItem
            // 
            this.viewProxyDefinitionToolStripMenuItem.Name = "viewProxyDefinitionToolStripMenuItem";
            this.viewProxyDefinitionToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.viewProxyDefinitionToolStripMenuItem.Text = "View Proxy Definition";
            this.viewProxyDefinitionToolStripMenuItem.Click += new System.EventHandler(this.viewProxyDefinitionToolStripMenuItem_Click);
            // 
            // viewLaunchPermissionsToolStripMenuItem
            // 
            this.viewLaunchPermissionsToolStripMenuItem.Name = "viewLaunchPermissionsToolStripMenuItem";
            this.viewLaunchPermissionsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.viewLaunchPermissionsToolStripMenuItem.Text = "View Launch Permissions";
            this.viewLaunchPermissionsToolStripMenuItem.Click += new System.EventHandler(this.viewLaunchPermissionsToolStripMenuItem_Click);
            // 
            // viewAccessPermissionsToolStripMenuItem
            // 
            this.viewAccessPermissionsToolStripMenuItem.Name = "viewAccessPermissionsToolStripMenuItem";
            this.viewAccessPermissionsToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.viewAccessPermissionsToolStripMenuItem.Text = "View Access Permissions";
            this.viewAccessPermissionsToolStripMenuItem.Click += new System.EventHandler(this.viewAccessPermissionsToolStripMenuItem_Click);
            // 
            // queryAllInterfacesToolStripMenuItem
            // 
            this.queryAllInterfacesToolStripMenuItem.Name = "queryAllInterfacesToolStripMenuItem";
            this.queryAllInterfacesToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.queryAllInterfacesToolStripMenuItem.Text = "Query All Interfaces";
            this.queryAllInterfacesToolStripMenuItem.Click += new System.EventHandler(this.queryAllInterfacesToolStripMenuItem_Click);
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.propertiesToolStripMenuItem.Text = "Properties";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
            // 
            // treeImageList
            // 
            this.treeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeImageList.ImageStream")));
            this.treeImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.treeImageList.Images.SetKeyName(0, "folder.ico");
            this.treeImageList.Images.SetKeyName(1, "class.ico");
            this.treeImageList.Images.SetKeyName(2, "interface.ico");
            this.treeImageList.Images.SetKeyName(3, "folderopen.ico");
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Location = new System.Drawing.Point(802, 4);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(164, 21);
            this.comboBoxMode.TabIndex = 4;
            this.comboBoxMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxMode_SelectedIndexChanged);
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFilter.Location = new System.Drawing.Point(41, 4);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(712, 20);
            this.textBoxFilter.TabIndex = 0;
            this.textBoxFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxFilter_KeyDown);
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 5;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Controls.Add(labelFilter, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.textBoxFilter, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.treeComRegistry, 0, 1);
            this.tableLayoutPanel.Controls.Add(labelMode, 2, 0);
            this.tableLayoutPanel.Controls.Add(this.comboBoxMode, 3, 0);
            this.tableLayoutPanel.Controls.Add(this.btnApply, 4, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(1052, 530);
            this.tableLayoutPanel.TabIndex = 6;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnApply.AutoSize = true;
            this.btnApply.Location = new System.Drawing.Point(972, 3);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(77, 23);
            this.btnApply.TabIndex = 6;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // COMRegistryViewer
            // 
            this.Controls.Add(this.tableLayoutPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "COMRegistryViewer";
            this.Size = new System.Drawing.Size(1052, 530);
            this.contextMenuStrip.ResumeLayout(false);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeComRegistry;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copyGUIDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyGUIDCStructureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyGUIDHexStringToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyObjectTagToolStripMenuItem;
        private System.Windows.Forms.ImageList treeImageList;
        private System.Windows.Forms.ToolStripMenuItem createInstanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshInterfacesToolStripMenuItem;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.ComboBox comboBoxMode;
        private System.Windows.Forms.ToolStripMenuItem viewTypeLibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewLaunchPermissionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewAccessPermissionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createSpecialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createLocalServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createInProcServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createInSessionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem consoleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewProxyDefinitionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createClassFactoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem queryAllInterfacesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createInProcHandlerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createElevatedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem instanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem classFactoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createRemoteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createClassFactoryRemoteToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Button btnApply;
    }
}