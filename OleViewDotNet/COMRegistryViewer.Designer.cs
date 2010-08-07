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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(COMRegistryViewer));
            this.treeComRegistry = new System.Windows.Forms.TreeView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyGUIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGUIDCStructureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGUIDHexStringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyObjectTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createInstanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeImageList = new System.Windows.Forms.ImageList(this.components);
            this.refreshInterfacesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeComRegistry
            // 
            this.treeComRegistry.ContextMenuStrip = this.contextMenuStrip;
            this.treeComRegistry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeComRegistry.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeComRegistry.ImageIndex = 0;
            this.treeComRegistry.ImageList = this.treeImageList;
            this.treeComRegistry.Location = new System.Drawing.Point(0, 0);
            this.treeComRegistry.Name = "treeComRegistry";
            this.treeComRegistry.SelectedImageIndex = 0;
            this.treeComRegistry.ShowNodeToolTips = true;
            this.treeComRegistry.Size = new System.Drawing.Size(688, 473);
            this.treeComRegistry.TabIndex = 0;
            this.treeComRegistry.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeComRegistry_BeforeExpand);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyGUIDToolStripMenuItem,
            this.copyGUIDCStructureToolStripMenuItem,
            this.copyGUIDHexStringToolStripMenuItem,
            this.copyObjectTagToolStripMenuItem,
            this.createInstanceToolStripMenuItem,
            this.refreshInterfacesToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(195, 158);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // copyGUIDToolStripMenuItem
            // 
            this.copyGUIDToolStripMenuItem.Name = "copyGUIDToolStripMenuItem";
            this.copyGUIDToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.copyGUIDToolStripMenuItem.Text = "Copy GUID";
            this.copyGUIDToolStripMenuItem.Click += new System.EventHandler(this.copyGUIDToolStripMenuItem_Click);
            // 
            // copyGUIDCStructureToolStripMenuItem
            // 
            this.copyGUIDCStructureToolStripMenuItem.Name = "copyGUIDCStructureToolStripMenuItem";
            this.copyGUIDCStructureToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.copyGUIDCStructureToolStripMenuItem.Text = "Copy GUID C Structure";
            this.copyGUIDCStructureToolStripMenuItem.Click += new System.EventHandler(this.copyGUIDCStructureToolStripMenuItem_Click);
            // 
            // copyGUIDHexStringToolStripMenuItem
            // 
            this.copyGUIDHexStringToolStripMenuItem.Name = "copyGUIDHexStringToolStripMenuItem";
            this.copyGUIDHexStringToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.copyGUIDHexStringToolStripMenuItem.Text = "Copy GUID Hex String";
            this.copyGUIDHexStringToolStripMenuItem.Click += new System.EventHandler(this.copyGUIDHexStringToolStripMenuItem_Click);
            // 
            // copyObjectTagToolStripMenuItem
            // 
            this.copyObjectTagToolStripMenuItem.Name = "copyObjectTagToolStripMenuItem";
            this.copyObjectTagToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.copyObjectTagToolStripMenuItem.Text = "Copy Object Tag";
            this.copyObjectTagToolStripMenuItem.Click += new System.EventHandler(this.copyObjectTagToolStripMenuItem_Click);
            // 
            // createInstanceToolStripMenuItem
            // 
            this.createInstanceToolStripMenuItem.Name = "createInstanceToolStripMenuItem";
            this.createInstanceToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.createInstanceToolStripMenuItem.Text = "Create Instance";
            this.createInstanceToolStripMenuItem.Click += new System.EventHandler(this.createInstanceToolStripMenuItem_Click);
            // 
            // treeImageList
            // 
            this.treeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeImageList.ImageStream")));
            this.treeImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.treeImageList.Images.SetKeyName(0, "folder.ico");
            this.treeImageList.Images.SetKeyName(1, "interface.ico");
            this.treeImageList.Images.SetKeyName(2, "class.ico");
            // 
            // refreshInterfacesToolStripMenuItem
            // 
            this.refreshInterfacesToolStripMenuItem.Name = "refreshInterfacesToolStripMenuItem";
            this.refreshInterfacesToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.refreshInterfacesToolStripMenuItem.Text = "Refresh Interfaces";
            this.refreshInterfacesToolStripMenuItem.Click += new System.EventHandler(this.refreshInterfacesToolStripMenuItem_Click);
            // 
            // COMRegistryViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(688, 473);
            this.Controls.Add(this.treeComRegistry);
            this.Name = "COMRegistryViewer";
            this.TabText = "COMRegisterViewer";
            this.Text = "COMRegisterViewer";
            this.Load += new System.EventHandler(this.COMRegisterViewer_Load);
            this.contextMenuStrip.ResumeLayout(false);
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
    }
}