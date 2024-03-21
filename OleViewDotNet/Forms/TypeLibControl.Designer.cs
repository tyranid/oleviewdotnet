namespace OleViewDotNet.Forms;

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
            this.tabPageDispatch = new System.Windows.Forms.TabPage();
            this.listViewDispatch = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.textBoxFilter = new OleViewDotNet.Forms.InputTextBox();
            this.sourceCodeViewerControl = new OleViewDotNet.Forms.SourceCodeViewerControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageInterfaces.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.tabPageDispatch.SuspendLayout();
            this.tabPageClasses.SuspendLayout();
            this.tabPageStructures.SuspendLayout();
            this.tabPageEnums.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.panel2);
            this.splitContainer.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.sourceCodeViewerControl);
            this.splitContainer.Size = new System.Drawing.Size(1048, 643);
            this.splitContainer.SplitterDistance = 439;
            this.splitContainer.SplitterWidth = 6;
            this.splitContainer.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tabControl);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 71);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(439, 572);
            this.panel2.TabIndex = 1;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageInterfaces);
            this.tabControl.Controls.Add(this.tabPageDispatch);
            this.tabControl.Controls.Add(this.tabPageClasses);
            this.tabControl.Controls.Add(this.tabPageStructures);
            this.tabControl.Controls.Add(this.tabPageEnums);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(439, 572);
            this.tabControl.TabIndex = 3;
            // 
            // tabPageInterfaces
            // 
            this.tabPageInterfaces.Controls.Add(this.listViewInterfaces);
            this.tabPageInterfaces.Location = new System.Drawing.Point(4, 29);
            this.tabPageInterfaces.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageInterfaces.Name = "tabPageInterfaces";
            this.tabPageInterfaces.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageInterfaces.Size = new System.Drawing.Size(431, 539);
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
            this.listViewInterfaces.Location = new System.Drawing.Point(4, 5);
            this.listViewInterfaces.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.listViewInterfaces.Name = "listViewInterfaces";
            this.listViewInterfaces.Size = new System.Drawing.Size(423, 529);
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
            this.contextMenuStrip.Size = new System.Drawing.Size(265, 132);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(264, 32);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // copyGUIDToolStripMenuItem
            // 
            this.copyGUIDToolStripMenuItem.Name = "copyGUIDToolStripMenuItem";
            this.copyGUIDToolStripMenuItem.Size = new System.Drawing.Size(264, 32);
            this.copyGUIDToolStripMenuItem.Text = "Copy GUID";
            this.copyGUIDToolStripMenuItem.Click += new System.EventHandler(this.copyGUIDToolStripMenuItem_Click);
            // 
            // copyGUIDCStructureToolStripMenuItem
            // 
            this.copyGUIDCStructureToolStripMenuItem.Name = "copyGUIDCStructureToolStripMenuItem";
            this.copyGUIDCStructureToolStripMenuItem.Size = new System.Drawing.Size(264, 32);
            this.copyGUIDCStructureToolStripMenuItem.Text = "Copy GUID C Structure";
            this.copyGUIDCStructureToolStripMenuItem.Click += new System.EventHandler(this.copyGUIDCStructureToolStripMenuItem_Click);
            // 
            // copyGIUDHexStringToolStripMenuItem
            // 
            this.copyGIUDHexStringToolStripMenuItem.Name = "copyGIUDHexStringToolStripMenuItem";
            this.copyGIUDHexStringToolStripMenuItem.Size = new System.Drawing.Size(264, 32);
            this.copyGIUDHexStringToolStripMenuItem.Text = "Copy GIUD Hex String";
            this.copyGIUDHexStringToolStripMenuItem.Click += new System.EventHandler(this.copyGIUDHexStringToolStripMenuItem_Click);
            // 
            // tabPageDispatch
            // 
            this.tabPageDispatch.Controls.Add(this.listViewDispatch);
            this.tabPageDispatch.Location = new System.Drawing.Point(4, 29);
            this.tabPageDispatch.Name = "tabPageDispatch";
            this.tabPageDispatch.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDispatch.Size = new System.Drawing.Size(431, 539);
            this.tabPageDispatch.TabIndex = 4;
            this.tabPageDispatch.Text = "Dispatch";
            this.tabPageDispatch.UseVisualStyleBackColor = true;
            // 
            // listViewDispatch
            // 
            this.listViewDispatch.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader5});
            this.listViewDispatch.ContextMenuStrip = this.contextMenuStrip;
            this.listViewDispatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewDispatch.FullRowSelect = true;
            this.listViewDispatch.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewDispatch.HideSelection = false;
            this.listViewDispatch.Location = new System.Drawing.Point(3, 3);
            this.listViewDispatch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.listViewDispatch.Name = "listViewDispatch";
            this.listViewDispatch.Size = new System.Drawing.Size(425, 533);
            this.listViewDispatch.TabIndex = 1;
            this.listViewDispatch.UseCompatibleStateImageBehavior = false;
            this.listViewDispatch.View = System.Windows.Forms.View.Details;
            this.listViewDispatch.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 278;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "IID";
            // 
            // tabPageClasses
            // 
            this.tabPageClasses.Controls.Add(this.listViewClasses);
            this.tabPageClasses.Location = new System.Drawing.Point(4, 29);
            this.tabPageClasses.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageClasses.Name = "tabPageClasses";
            this.tabPageClasses.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageClasses.Size = new System.Drawing.Size(431, 539);
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
            this.listViewClasses.Location = new System.Drawing.Point(4, 5);
            this.listViewClasses.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.listViewClasses.Name = "listViewClasses";
            this.listViewClasses.Size = new System.Drawing.Size(423, 529);
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
            this.tabPageStructures.Location = new System.Drawing.Point(4, 29);
            this.tabPageStructures.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageStructures.Name = "tabPageStructures";
            this.tabPageStructures.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageStructures.Size = new System.Drawing.Size(431, 539);
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
            this.listViewStructures.Location = new System.Drawing.Point(4, 5);
            this.listViewStructures.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.listViewStructures.Name = "listViewStructures";
            this.listViewStructures.Size = new System.Drawing.Size(423, 529);
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
            this.tabPageEnums.Location = new System.Drawing.Point(4, 29);
            this.tabPageEnums.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageEnums.Name = "tabPageEnums";
            this.tabPageEnums.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPageEnums.Size = new System.Drawing.Size(431, 539);
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
            this.listViewEnums.Location = new System.Drawing.Point(4, 5);
            this.listViewEnums.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.listViewEnums.Name = "listViewEnums";
            this.listViewEnums.Size = new System.Drawing.Size(423, 529);
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
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(439, 71);
            this.panel1.TabIndex = 0;
            // 
            // btnExportInterfaces
            // 
            this.btnExportInterfaces.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportInterfaces.Location = new System.Drawing.Point(314, 17);
            this.btnExportInterfaces.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnExportInterfaces.Name = "btnExportInterfaces";
            this.btnExportInterfaces.Size = new System.Drawing.Size(110, 35);
            this.btnExportInterfaces.TabIndex = 7;
            this.btnExportInterfaces.Text = "Export";
            this.btnExportInterfaces.UseVisualStyleBackColor = true;
            this.btnExportInterfaces.Click += new System.EventHandler(this.btnExportInterfaces_Click);
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFilter.Location = new System.Drawing.Point(10, 18);
            this.textBoxFilter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(293, 26);
            this.textBoxFilter.TabIndex = 6;
            this.textBoxFilter.Enter += new System.EventHandler(this.textBoxFilter_Enter);
            this.textBoxFilter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxFilter_KeyUp);
            // 
            // sourceCodeViewerControl
            // 
            this.sourceCodeViewerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceCodeViewerControl.Location = new System.Drawing.Point(0, 0);
            this.sourceCodeViewerControl.Name = "sourceCodeViewerControl";
            this.sourceCodeViewerControl.Size = new System.Drawing.Size(603, 643);
            this.sourceCodeViewerControl.TabIndex = 0;
            // 
            // TypeLibControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "TypeLibControl";
            this.Size = new System.Drawing.Size(1048, 643);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageInterfaces.ResumeLayout(false);
            this.contextMenuStrip.ResumeLayout(false);
            this.tabPageDispatch.ResumeLayout(false);
            this.tabPageClasses.ResumeLayout(false);
            this.tabPageStructures.ResumeLayout(false);
            this.tabPageEnums.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer;
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
    private System.Windows.Forms.TabPage tabPageDispatch;
    private System.Windows.Forms.ListView listViewDispatch;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private SourceCodeViewerControl sourceCodeViewerControl;
}
