namespace OleViewDotNet
{
    partial class StorageViewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StorageViewer));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeViewStorage = new System.Windows.Forms.TreeView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.tabControlProperties = new System.Windows.Forms.TabControl();
            this.tabPageProperties = new System.Windows.Forms.TabPage();
            this.propertyGridStat = new System.Windows.Forms.PropertyGrid();
            this.tabPageStream = new System.Windows.Forms.TabPage();
            this.hexEditorStream = new OleViewDotNet.HexEditor();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControlProperties.SuspendLayout();
            this.tabPageProperties.SuspendLayout();
            this.tabPageStream.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeViewStorage);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControlProperties);
            this.splitContainer1.Size = new System.Drawing.Size(798, 459);
            this.splitContainer1.SplitterDistance = 351;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 0;
            // 
            // treeViewStorage
            // 
            this.treeViewStorage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewStorage.ImageIndex = 0;
            this.treeViewStorage.ImageList = this.imageList;
            this.treeViewStorage.Location = new System.Drawing.Point(0, 0);
            this.treeViewStorage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.treeViewStorage.Name = "treeViewStorage";
            this.treeViewStorage.SelectedImageIndex = 0;
            this.treeViewStorage.Size = new System.Drawing.Size(351, 459);
            this.treeViewStorage.TabIndex = 0;
            this.treeViewStorage.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeViewStorage_AfterCollapse);
            this.treeViewStorage.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeViewStorage_AfterExpand);
            this.treeViewStorage.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewStorage_AfterSelect);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "folder.ico");
            this.imageList.Images.SetKeyName(1, "FolderOpen.ico");
            this.imageList.Images.SetKeyName(2, "interface.ico");
            // 
            // tabControlProperties
            // 
            this.tabControlProperties.Controls.Add(this.tabPageProperties);
            this.tabControlProperties.Controls.Add(this.tabPageStream);
            this.tabControlProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlProperties.Location = new System.Drawing.Point(0, 0);
            this.tabControlProperties.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControlProperties.Name = "tabControlProperties";
            this.tabControlProperties.SelectedIndex = 0;
            this.tabControlProperties.Size = new System.Drawing.Size(445, 459);
            this.tabControlProperties.TabIndex = 0;
            // 
            // tabPageProperties
            // 
            this.tabPageProperties.Controls.Add(this.propertyGridStat);
            this.tabPageProperties.Location = new System.Drawing.Point(4, 22);
            this.tabPageProperties.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageProperties.Name = "tabPageProperties";
            this.tabPageProperties.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageProperties.Size = new System.Drawing.Size(437, 433);
            this.tabPageProperties.TabIndex = 0;
            this.tabPageProperties.Text = "Properties";
            this.tabPageProperties.UseVisualStyleBackColor = true;
            // 
            // propertyGridStat
            // 
            this.propertyGridStat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridStat.HelpVisible = false;
            this.propertyGridStat.Location = new System.Drawing.Point(2, 2);
            this.propertyGridStat.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.propertyGridStat.Name = "propertyGridStat";
            this.propertyGridStat.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridStat.Size = new System.Drawing.Size(433, 429);
            this.propertyGridStat.TabIndex = 0;
            this.propertyGridStat.ToolbarVisible = false;
            // 
            // tabPageStream
            // 
            this.tabPageStream.Controls.Add(this.hexEditorStream);
            this.tabPageStream.Location = new System.Drawing.Point(4, 22);
            this.tabPageStream.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageStream.Name = "tabPageStream";
            this.tabPageStream.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageStream.Size = new System.Drawing.Size(437, 433);
            this.tabPageStream.TabIndex = 1;
            this.tabPageStream.Text = "Stream";
            this.tabPageStream.UseVisualStyleBackColor = true;
            // 
            // hexEditorStream
            // 
            this.hexEditorStream.Bytes = new byte[0];
            this.hexEditorStream.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hexEditorStream.Location = new System.Drawing.Point(2, 2);
            this.hexEditorStream.Name = "hexEditorStream";
            this.hexEditorStream.ReadOnly = true;
            this.hexEditorStream.Size = new System.Drawing.Size(433, 429);
            this.hexEditorStream.TabIndex = 0;
            // 
            // StorageViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "StorageViewer";
            this.Size = new System.Drawing.Size(798, 459);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControlProperties.ResumeLayout(false);
            this.tabPageProperties.ResumeLayout(false);
            this.tabPageStream.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeViewStorage;
        private System.Windows.Forms.TabControl tabControlProperties;
        private System.Windows.Forms.TabPage tabPageProperties;
        private System.Windows.Forms.PropertyGrid propertyGridStat;
        private System.Windows.Forms.TabPage tabPageStream;
        private HexEditor hexEditorStream;
        private System.Windows.Forms.ImageList imageList;
    }
}
