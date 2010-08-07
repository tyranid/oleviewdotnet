namespace OleViewDotNet
{
    partial class TypedObjectViewer
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
            this.lblName = new System.Windows.Forms.Label();
            this.listViewMethods = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.listViewProperties = new System.Windows.Forms.ListView();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.contextMenuProperties = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuMethods = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openObjectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.contextMenuProperties.SuspendLayout();
            this.contextMenuMethods.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(9, 9);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(38, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Name:";
            // 
            // listViewMethods
            // 
            this.listViewMethods.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewMethods.FullRowSelect = true;
            this.listViewMethods.Location = new System.Drawing.Point(12, 44);
            this.listViewMethods.Name = "listViewMethods";
            this.listViewMethods.Size = new System.Drawing.Size(1026, 194);
            this.listViewMethods.TabIndex = 1;
            this.listViewMethods.UseCompatibleStateImageBehavior = false;
            this.listViewMethods.View = System.Windows.Forms.View.Details;
            this.listViewMethods.DoubleClick += new System.EventHandler(this.listViewMethods_DoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Methods:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Properties:";
            // 
            // listViewProperties
            // 
            this.listViewProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewProperties.ContextMenuStrip = this.contextMenuProperties;
            this.listViewProperties.FullRowSelect = true;
            this.listViewProperties.Location = new System.Drawing.Point(12, 26);
            this.listViewProperties.Name = "listViewProperties";
            this.listViewProperties.Size = new System.Drawing.Size(1026, 212);
            this.listViewProperties.TabIndex = 4;
            this.listViewProperties.UseCompatibleStateImageBehavior = false;
            this.listViewProperties.View = System.Windows.Forms.View.Details;
            this.listViewProperties.DoubleClick += new System.EventHandler(this.listViewProperties_DoubleClick);
            // 
            // splitContainer
            // 
            this.splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.listViewMethods);
            this.splitContainer.Panel1.Controls.Add(this.lblName);
            this.splitContainer.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.label2);
            this.splitContainer.Panel2.Controls.Add(this.listViewProperties);
            this.splitContainer.Size = new System.Drawing.Size(1052, 505);
            this.splitContainer.SplitterDistance = 252;
            this.splitContainer.TabIndex = 5;
            // 
            // contextMenuProperties
            // 
            this.contextMenuProperties.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshToolStripMenuItem,
            this.openObjectToolStripMenuItem});
            this.contextMenuProperties.Name = "contextMenuProperties";
            this.contextMenuProperties.Size = new System.Drawing.Size(142, 48);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // openObjectToolStripMenuItem
            // 
            this.openObjectToolStripMenuItem.Name = "openObjectToolStripMenuItem";
            this.openObjectToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.openObjectToolStripMenuItem.Text = "Open Object";
            this.openObjectToolStripMenuItem.Click += new System.EventHandler(this.openObjectToolStripMenuItem_Click);
            // 
            // contextMenuMethods
            // 
            this.contextMenuMethods.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openObjectToolStripMenuItem1});
            this.contextMenuMethods.Name = "contextMenuMethods";
            this.contextMenuMethods.Size = new System.Drawing.Size(153, 48);
            // 
            // openObjectToolStripMenuItem1
            // 
            this.openObjectToolStripMenuItem1.Name = "openObjectToolStripMenuItem1";
            this.openObjectToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.openObjectToolStripMenuItem1.Text = "Open Object";
            this.openObjectToolStripMenuItem1.Click += new System.EventHandler(this.openObjectToolStripMenuItem1_Click);
            // 
            // TypedObjectViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1052, 505);
            this.Controls.Add(this.splitContainer);
            this.Name = "TypedObjectViewer";
            this.TabText = "ObjectDispatch";
            this.Text = "ObjectDispatch";
            this.Load += new System.EventHandler(this.TypedObjectViewer_Load);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            this.splitContainer.ResumeLayout(false);
            this.contextMenuProperties.ResumeLayout(false);
            this.contextMenuMethods.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.ListView listViewMethods;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listViewProperties;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ContextMenuStrip contextMenuProperties;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openObjectToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuMethods;
        private System.Windows.Forms.ToolStripMenuItem openObjectToolStripMenuItem1;
    }
}