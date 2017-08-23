namespace OleViewDotNet
{
    partial class TypeLibControl
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
            System.Windows.Forms.ColumnHeader columnHeaderName;
            System.Windows.Forms.ColumnHeader columnHeaderGuid;
            System.Windows.Forms.ColumnHeader columnHeader1;
            System.Windows.Forms.ColumnHeader columnHeader2;
            System.Windows.Forms.ColumnHeader columnHeader3;
            System.Windows.Forms.ColumnHeader columnHeader4;
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageInterfaces = new System.Windows.Forms.TabPage();
            this.listViewInterfaces = new System.Windows.Forms.ListView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGUIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGUIDCStructureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGIUDHexStringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPageStructures = new System.Windows.Forms.TabPage();
            this.listViewStructures = new System.Windows.Forms.ListView();
            this.tabPageEnums = new System.Windows.Forms.TabPage();
            this.listViewEnums = new System.Windows.Forms.ListView();
            this.textEditor = new ICSharpCode.TextEditor.TextEditorControl();
            this.tabPageClasses = new System.Windows.Forms.TabPage();
            this.listViewClasses = new System.Windows.Forms.ListView();
            columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeaderGuid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageInterfaces.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.tabPageStructures.SuspendLayout();
            this.tabPageEnums.SuspendLayout();
            this.tabPageClasses.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnHeaderName
            // 
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 278;
            // 
            // columnHeaderGuid
            // 
            columnHeaderGuid.Text = "IID";
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Name";
            columnHeader1.Width = 278;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Name";
            columnHeader2.Width = 278;
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.tabControl);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.textEditor);
            this.splitContainer.Size = new System.Drawing.Size(699, 418);
            this.splitContainer.SplitterDistance = 294;
            this.splitContainer.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageInterfaces);
            this.tabControl.Controls.Add(this.tabPageClasses);
            this.tabControl.Controls.Add(this.tabPageStructures);
            this.tabControl.Controls.Add(this.tabPageEnums);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(294, 418);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageInterfaces
            // 
            this.tabPageInterfaces.Controls.Add(this.listViewInterfaces);
            this.tabPageInterfaces.Location = new System.Drawing.Point(4, 22);
            this.tabPageInterfaces.Name = "tabPageInterfaces";
            this.tabPageInterfaces.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInterfaces.Size = new System.Drawing.Size(286, 392);
            this.tabPageInterfaces.TabIndex = 0;
            this.tabPageInterfaces.Text = "Interfaces";
            this.tabPageInterfaces.UseVisualStyleBackColor = true;
            // 
            // listViewInterfaces
            // 
            this.listViewInterfaces.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeaderName,
            columnHeaderGuid});
            this.listViewInterfaces.ContextMenuStrip = this.contextMenuStrip;
            this.listViewInterfaces.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewInterfaces.FullRowSelect = true;
            this.listViewInterfaces.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewInterfaces.HideSelection = false;
            this.listViewInterfaces.Location = new System.Drawing.Point(3, 3);
            this.listViewInterfaces.Name = "listViewInterfaces";
            this.listViewInterfaces.Size = new System.Drawing.Size(280, 386);
            this.listViewInterfaces.TabIndex = 0;
            this.listViewInterfaces.UseCompatibleStateImageBehavior = false;
            this.listViewInterfaces.View = System.Windows.Forms.View.Details;
            this.listViewInterfaces.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.copyGUIDToolStripMenuItem,
            this.copyGUIDCStructureToolStripMenuItem,
            this.copyGIUDHexStringToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(195, 92);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
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
            // copyGIUDHexStringToolStripMenuItem
            // 
            this.copyGIUDHexStringToolStripMenuItem.Name = "copyGIUDHexStringToolStripMenuItem";
            this.copyGIUDHexStringToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.copyGIUDHexStringToolStripMenuItem.Text = "Copy GIUD Hex String";
            this.copyGIUDHexStringToolStripMenuItem.Click += new System.EventHandler(this.copyGIUDHexStringToolStripMenuItem_Click);
            // 
            // tabPageStructures
            // 
            this.tabPageStructures.Controls.Add(this.listViewStructures);
            this.tabPageStructures.Location = new System.Drawing.Point(4, 22);
            this.tabPageStructures.Name = "tabPageStructures";
            this.tabPageStructures.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageStructures.Size = new System.Drawing.Size(286, 392);
            this.tabPageStructures.TabIndex = 1;
            this.tabPageStructures.Text = "Structures";
            this.tabPageStructures.UseVisualStyleBackColor = true;
            // 
            // listViewStructures
            // 
            this.listViewStructures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1});
            this.listViewStructures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewStructures.FullRowSelect = true;
            this.listViewStructures.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewStructures.HideSelection = false;
            this.listViewStructures.Location = new System.Drawing.Point(3, 3);
            this.listViewStructures.Name = "listViewStructures";
            this.listViewStructures.Size = new System.Drawing.Size(280, 386);
            this.listViewStructures.TabIndex = 1;
            this.listViewStructures.UseCompatibleStateImageBehavior = false;
            this.listViewStructures.View = System.Windows.Forms.View.Details;
            this.listViewStructures.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // tabPageEnums
            // 
            this.tabPageEnums.Controls.Add(this.listViewEnums);
            this.tabPageEnums.Location = new System.Drawing.Point(4, 22);
            this.tabPageEnums.Name = "tabPageEnums";
            this.tabPageEnums.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageEnums.Size = new System.Drawing.Size(286, 392);
            this.tabPageEnums.TabIndex = 2;
            this.tabPageEnums.Text = "Enums";
            this.tabPageEnums.UseVisualStyleBackColor = true;
            // 
            // listViewEnums
            // 
            this.listViewEnums.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader2});
            this.listViewEnums.ContextMenuStrip = this.contextMenuStrip;
            this.listViewEnums.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewEnums.FullRowSelect = true;
            this.listViewEnums.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewEnums.HideSelection = false;
            this.listViewEnums.Location = new System.Drawing.Point(3, 3);
            this.listViewEnums.Name = "listViewEnums";
            this.listViewEnums.Size = new System.Drawing.Size(280, 386);
            this.listViewEnums.TabIndex = 1;
            this.listViewEnums.UseCompatibleStateImageBehavior = false;
            this.listViewEnums.View = System.Windows.Forms.View.Details;
            this.listViewEnums.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // textEditor
            // 
            this.textEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textEditor.IsReadOnly = false;
            this.textEditor.Location = new System.Drawing.Point(0, 0);
            this.textEditor.Name = "textEditor";
            this.textEditor.Size = new System.Drawing.Size(401, 418);
            this.textEditor.TabIndex = 0;
            // 
            // tabPageClasses
            // 
            this.tabPageClasses.Controls.Add(this.listViewClasses);
            this.tabPageClasses.Location = new System.Drawing.Point(4, 22);
            this.tabPageClasses.Name = "tabPageClasses";
            this.tabPageClasses.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageClasses.Size = new System.Drawing.Size(286, 392);
            this.tabPageClasses.TabIndex = 3;
            this.tabPageClasses.Text = "Classes";
            this.tabPageClasses.UseVisualStyleBackColor = true;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Name";
            columnHeader3.Width = 278;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "GUID";
            // 
            // listViewClasses
            // 
            this.listViewClasses.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader3,
            columnHeader4});
            this.listViewClasses.ContextMenuStrip = this.contextMenuStrip;
            this.listViewClasses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewClasses.FullRowSelect = true;
            this.listViewClasses.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewClasses.HideSelection = false;
            this.listViewClasses.Location = new System.Drawing.Point(3, 3);
            this.listViewClasses.Name = "listViewClasses";
            this.listViewClasses.Size = new System.Drawing.Size(280, 386);
            this.listViewClasses.TabIndex = 1;
            this.listViewClasses.UseCompatibleStateImageBehavior = false;
            this.listViewClasses.View = System.Windows.Forms.View.Details;
            this.listViewClasses.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // TypeLibControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "TypeLibControl";
            this.Size = new System.Drawing.Size(699, 418);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageInterfaces.ResumeLayout(false);
            this.contextMenuStrip.ResumeLayout(false);
            this.tabPageStructures.ResumeLayout(false);
            this.tabPageEnums.ResumeLayout(false);
            this.tabPageClasses.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ListView listViewInterfaces;
        private ICSharpCode.TextEditor.TextEditorControl textEditor;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyGUIDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyGUIDCStructureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyGIUDHexStringToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageInterfaces;
        private System.Windows.Forms.TabPage tabPageStructures;
        private System.Windows.Forms.ListView listViewStructures;
        private System.Windows.Forms.TabPage tabPageEnums;
        private System.Windows.Forms.ListView listViewEnums;
        private System.Windows.Forms.TabPage tabPageClasses;
        private System.Windows.Forms.ListView listViewClasses;
    }
}
