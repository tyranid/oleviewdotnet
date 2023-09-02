namespace OleViewDotNet.Forms
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageInterfaces = new System.Windows.Forms.TabPage();
            this.listViewInterfaces = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderGuid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGUIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGUIDCStructureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGIUDHexStringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPageClasses = new System.Windows.Forms.TabPage();
            this.listViewClasses = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPageStructures = new System.Windows.Forms.TabPage();
            this.listViewStructures = new System.Windows.Forms.ListView();
            this.columnHeaderStructuresName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStructuresSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPageEnums = new System.Windows.Forms.TabPage();
            this.listViewEnums = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnExportInterfaces = new System.Windows.Forms.Button();
            this.textBoxFilter = new OleViewDotNet.InputTextBox();
            this.panelProxy = new System.Windows.Forms.Panel();
            this.lblRendering = new System.Windows.Forms.Label();
            this.cbProxyRenderStyle = new System.Windows.Forms.ComboBox();
            this.btnDqs = new System.Windows.Forms.Button();
            this.textEditor = new ICSharpCode.TextEditor.TextEditorControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageInterfaces.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.tabPageClasses.SuspendLayout();
            this.tabPageStructures.SuspendLayout();
            this.tabPageEnums.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panelProxy.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.panel2);
            this.splitContainer.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.panelProxy);
            this.splitContainer.Panel2.Controls.Add(this.textEditor);
            this.splitContainer.Size = new System.Drawing.Size(699, 418);
            this.splitContainer.SplitterDistance = 293;
            this.splitContainer.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tabControl);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 46);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(293, 372);
            this.panel2.TabIndex = 1;
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
            this.tabControl.Size = new System.Drawing.Size(293, 372);
            this.tabControl.TabIndex = 3;
            // 
            // tabPageInterfaces
            // 
            this.tabPageInterfaces.Controls.Add(this.listViewInterfaces);
            this.tabPageInterfaces.Location = new System.Drawing.Point(4, 22);
            this.tabPageInterfaces.Name = "tabPageInterfaces";
            this.tabPageInterfaces.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInterfaces.Size = new System.Drawing.Size(285, 346);
            this.tabPageInterfaces.TabIndex = 0;
            this.tabPageInterfaces.Text = "Interfaces";
            this.tabPageInterfaces.UseVisualStyleBackColor = true;
            // 
            // listViewInterfaces
            // 
            this.listViewInterfaces.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderGuid});
            this.listViewInterfaces.ContextMenuStrip = this.contextMenuStrip;
            this.listViewInterfaces.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewInterfaces.FullRowSelect = true;
            this.listViewInterfaces.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewInterfaces.HideSelection = false;
            this.listViewInterfaces.Location = new System.Drawing.Point(3, 3);
            this.listViewInterfaces.Name = "listViewInterfaces";
            this.listViewInterfaces.Size = new System.Drawing.Size(279, 340);
            this.listViewInterfaces.TabIndex = 0;
            this.listViewInterfaces.UseCompatibleStateImageBehavior = false;
            this.listViewInterfaces.View = System.Windows.Forms.View.Details;
            this.listViewInterfaces.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 278;
            // 
            // columnHeaderGuid
            // 
            this.columnHeaderGuid.Text = "IID";
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
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
            // tabPageClasses
            // 
            this.tabPageClasses.Controls.Add(this.listViewClasses);
            this.tabPageClasses.Location = new System.Drawing.Point(4, 22);
            this.tabPageClasses.Name = "tabPageClasses";
            this.tabPageClasses.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageClasses.Size = new System.Drawing.Size(285, 346);
            this.tabPageClasses.TabIndex = 3;
            this.tabPageClasses.Text = "Classes";
            this.tabPageClasses.UseVisualStyleBackColor = true;
            // 
            // listViewClasses
            // 
            this.listViewClasses.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
            this.listViewClasses.ContextMenuStrip = this.contextMenuStrip;
            this.listViewClasses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewClasses.FullRowSelect = true;
            this.listViewClasses.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewClasses.HideSelection = false;
            this.listViewClasses.Location = new System.Drawing.Point(3, 3);
            this.listViewClasses.Name = "listViewClasses";
            this.listViewClasses.Size = new System.Drawing.Size(279, 340);
            this.listViewClasses.TabIndex = 1;
            this.listViewClasses.UseCompatibleStateImageBehavior = false;
            this.listViewClasses.View = System.Windows.Forms.View.Details;
            this.listViewClasses.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Name";
            this.columnHeader3.Width = 278;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "GUID";
            // 
            // tabPageStructures
            // 
            this.tabPageStructures.Controls.Add(this.listViewStructures);
            this.tabPageStructures.Location = new System.Drawing.Point(4, 22);
            this.tabPageStructures.Name = "tabPageStructures";
            this.tabPageStructures.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageStructures.Size = new System.Drawing.Size(285, 346);
            this.tabPageStructures.TabIndex = 1;
            this.tabPageStructures.Text = "Structures";
            this.tabPageStructures.UseVisualStyleBackColor = true;
            // 
            // listViewStructures
            // 
            this.listViewStructures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderStructuresName,
            this.columnHeaderStructuresSize});
            this.listViewStructures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewStructures.FullRowSelect = true;
            this.listViewStructures.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewStructures.HideSelection = false;
            this.listViewStructures.Location = new System.Drawing.Point(3, 3);
            this.listViewStructures.Name = "listViewStructures";
            this.listViewStructures.Size = new System.Drawing.Size(279, 340);
            this.listViewStructures.TabIndex = 1;
            this.listViewStructures.UseCompatibleStateImageBehavior = false;
            this.listViewStructures.View = System.Windows.Forms.View.Details;
            this.listViewStructures.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // columnHeaderStructuresName
            // 
            this.columnHeaderStructuresName.Text = "Name";
            this.columnHeaderStructuresName.Width = 278;
            // 
            // columnHeaderStructuresSize
            // 
            this.columnHeaderStructuresSize.Text = "Size";
            // 
            // tabPageEnums
            // 
            this.tabPageEnums.Controls.Add(this.listViewEnums);
            this.tabPageEnums.Location = new System.Drawing.Point(4, 22);
            this.tabPageEnums.Name = "tabPageEnums";
            this.tabPageEnums.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageEnums.Size = new System.Drawing.Size(285, 346);
            this.tabPageEnums.TabIndex = 2;
            this.tabPageEnums.Text = "Enums";
            this.tabPageEnums.UseVisualStyleBackColor = true;
            // 
            // listViewEnums
            // 
            this.listViewEnums.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.listViewEnums.ContextMenuStrip = this.contextMenuStrip;
            this.listViewEnums.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewEnums.FullRowSelect = true;
            this.listViewEnums.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewEnums.HideSelection = false;
            this.listViewEnums.Location = new System.Drawing.Point(3, 3);
            this.listViewEnums.Name = "listViewEnums";
            this.listViewEnums.Size = new System.Drawing.Size(279, 340);
            this.listViewEnums.TabIndex = 1;
            this.listViewEnums.UseCompatibleStateImageBehavior = false;
            this.listViewEnums.View = System.Windows.Forms.View.Details;
            this.listViewEnums.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 278;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnExportInterfaces);
            this.panel1.Controls.Add(this.textBoxFilter);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(293, 46);
            this.panel1.TabIndex = 0;
            // 
            // btnExportInterfaces
            // 
            this.btnExportInterfaces.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportInterfaces.Location = new System.Drawing.Point(210, 11);
            this.btnExportInterfaces.Name = "btnExportInterfaces";
            this.btnExportInterfaces.Size = new System.Drawing.Size(73, 23);
            this.btnExportInterfaces.TabIndex = 7;
            this.btnExportInterfaces.Text = "Export";
            this.btnExportInterfaces.UseVisualStyleBackColor = true;
            this.btnExportInterfaces.Click += new System.EventHandler(this.btnExportInterfaces_Click);
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFilter.Location = new System.Drawing.Point(7, 12);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(197, 20);
            this.textBoxFilter.TabIndex = 6;
            this.textBoxFilter.Enter += new System.EventHandler(this.textBoxFilter_Enter);
            this.textBoxFilter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxFilter_KeyUp);
            // 
            // panelProxy
            // 
            this.panelProxy.Controls.Add(this.lblRendering);
            this.panelProxy.Controls.Add(this.cbProxyRenderStyle);
            this.panelProxy.Controls.Add(this.btnDqs);
            this.panelProxy.Location = new System.Drawing.Point(3, 3);
            this.panelProxy.Name = "panelProxy";
            this.panelProxy.Size = new System.Drawing.Size(396, 43);
            this.panelProxy.TabIndex = 2;
            // 
            // lblRendering
            // 
            this.lblRendering.AutoSize = true;
            this.lblRendering.Location = new System.Drawing.Point(129, 14);
            this.lblRendering.Name = "lblRendering";
            this.lblRendering.Size = new System.Drawing.Size(83, 13);
            this.lblRendering.TabIndex = 3;
            this.lblRendering.Text = "Rendering style:";
            // 
            // cbProxyRenderStyle
            // 
            this.cbProxyRenderStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProxyRenderStyle.Enabled = false;
            this.cbProxyRenderStyle.FormattingEnabled = true;
            this.cbProxyRenderStyle.Items.AddRange(new object[] {
            "Original",
            "Original, no comments",
            "C++ style",
            "C++ style, no comments"});
            this.cbProxyRenderStyle.Location = new System.Drawing.Point(218, 9);
            this.cbProxyRenderStyle.Name = "cbProxyRenderStyle";
            this.cbProxyRenderStyle.Size = new System.Drawing.Size(175, 21);
            this.cbProxyRenderStyle.TabIndex = 2;
            this.cbProxyRenderStyle.SelectedIndexChanged += new System.EventHandler(this.cbProxyRenderStyle_SelectedIndexChanged);
            // 
            // btnDqs
            // 
            this.btnDqs.Enabled = false;
            this.btnDqs.Location = new System.Drawing.Point(3, 9);
            this.btnDqs.Name = "btnDqs";
            this.btnDqs.Size = new System.Drawing.Size(119, 23);
            this.btnDqs.TabIndex = 1;
            this.btnDqs.Text = "Combine with dqs";
            this.btnDqs.UseVisualStyleBackColor = true;
            this.btnDqs.Click += new System.EventHandler(this.btnDqs_Click);
            // 
            // textEditor
            // 
            this.textEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textEditor.IsReadOnly = false;
            this.textEditor.Location = new System.Drawing.Point(0, 68);
            this.textEditor.Name = "textEditor";
            this.textEditor.Size = new System.Drawing.Size(402, 350);
            this.textEditor.TabIndex = 0;
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
            this.panel2.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageInterfaces.ResumeLayout(false);
            this.contextMenuStrip.ResumeLayout(false);
            this.tabPageClasses.ResumeLayout(false);
            this.tabPageStructures.ResumeLayout(false);
            this.tabPageEnums.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panelProxy.ResumeLayout(false);
            this.panelProxy.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private ICSharpCode.TextEditor.TextEditorControl textEditor;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyGUIDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyGUIDCStructureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyGIUDHexStringToolStripMenuItem;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageInterfaces;
        private System.Windows.Forms.ListView listViewInterfaces;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderGuid;
        private System.Windows.Forms.TabPage tabPageClasses;
        private System.Windows.Forms.ListView listViewClasses;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.TabPage tabPageStructures;
        private System.Windows.Forms.ListView listViewStructures;
        private System.Windows.Forms.ColumnHeader columnHeaderStructuresName;
        private System.Windows.Forms.ColumnHeader columnHeaderStructuresSize;
        private System.Windows.Forms.TabPage tabPageEnums;
        private System.Windows.Forms.ListView listViewEnums;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Panel panel1;
        private InputTextBox textBoxFilter;
        private System.Windows.Forms.Button btnExportInterfaces;
        private System.Windows.Forms.Button btnDqs;
        private System.Windows.Forms.Panel panelProxy;
        private System.Windows.Forms.Label lblRendering;
        private System.Windows.Forms.ComboBox cbProxyRenderStyle;
    }
}
