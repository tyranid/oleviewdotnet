namespace OleViewDotNet.Forms;

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
        if (disposing && (components is not null))
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
        System.Windows.Forms.TableLayoutPanel tableLayoutPanelTop;
        System.Windows.Forms.TableLayoutPanel tableLayoutPanelBottom;
        this.listViewMethods = new System.Windows.Forms.ListView();
        this.contextMenuMethods = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.openInvokeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.openObjectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
        this.label1 = new System.Windows.Forms.Label();
        this.lblName = new System.Windows.Forms.Label();
        this.label3 = new System.Windows.Forms.Label();
        this.label2 = new System.Windows.Forms.Label();
        this.listViewProperties = new System.Windows.Forms.ListView();
        this.contextMenuProperties = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.openObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.tabControl = new System.Windows.Forms.TabControl();
        this.tabPageInvoke = new System.Windows.Forms.TabPage();
        this.splitContainer = new System.Windows.Forms.SplitContainer();
        tableLayoutPanelTop = new System.Windows.Forms.TableLayoutPanel();
        tableLayoutPanelBottom = new System.Windows.Forms.TableLayoutPanel();
        tableLayoutPanelTop.SuspendLayout();
        this.contextMenuMethods.SuspendLayout();
        tableLayoutPanelBottom.SuspendLayout();
        this.contextMenuProperties.SuspendLayout();
        this.tabControl.SuspendLayout();
        this.tabPageInvoke.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
        this.splitContainer.Panel1.SuspendLayout();
        this.splitContainer.Panel2.SuspendLayout();
        this.splitContainer.SuspendLayout();
        this.SuspendLayout();
        // 
        // tableLayoutPanelTop
        // 
        tableLayoutPanelTop.ColumnCount = 2;
        tableLayoutPanelTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        tableLayoutPanelTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tableLayoutPanelTop.Controls.Add(this.listViewMethods, 0, 2);
        tableLayoutPanelTop.Controls.Add(this.label1, 0, 1);
        tableLayoutPanelTop.Controls.Add(this.lblName, 1, 0);
        tableLayoutPanelTop.Controls.Add(this.label3, 0, 0);
        tableLayoutPanelTop.Dock = System.Windows.Forms.DockStyle.Fill;
        tableLayoutPanelTop.Location = new System.Drawing.Point(0, 0);
        tableLayoutPanelTop.Name = "tableLayoutPanelTop";
        tableLayoutPanelTop.RowCount = 3;
        tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tableLayoutPanelTop.Size = new System.Drawing.Size(1672, 489);
        tableLayoutPanelTop.TabIndex = 3;
        // 
        // listViewMethods
        // 
        tableLayoutPanelTop.SetColumnSpan(this.listViewMethods, 2);
        this.listViewMethods.ContextMenuStrip = this.contextMenuMethods;
        this.listViewMethods.Dock = System.Windows.Forms.DockStyle.Fill;
        this.listViewMethods.FullRowSelect = true;
        this.listViewMethods.Location = new System.Drawing.Point(3, 29);
        this.listViewMethods.Name = "listViewMethods";
        this.listViewMethods.Size = new System.Drawing.Size(1666, 457);
        this.listViewMethods.TabIndex = 1;
        this.listViewMethods.UseCompatibleStateImageBehavior = false;
        this.listViewMethods.View = System.Windows.Forms.View.Details;
        this.listViewMethods.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_ColumnClick);
        this.listViewMethods.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewMethods_MouseDoubleClick);
        // 
        // contextMenuMethods
        // 
        this.contextMenuMethods.ImageScalingSize = new System.Drawing.Size(28, 28);
        this.contextMenuMethods.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.openInvokeToolStripMenuItem,
        this.openObjectToolStripMenuItem1});
        this.contextMenuMethods.Name = "contextMenuMethods";
        this.contextMenuMethods.Size = new System.Drawing.Size(142, 48);
        // 
        // openInvokeToolStripMenuItem
        // 
        this.openInvokeToolStripMenuItem.Name = "openInvokeToolStripMenuItem";
        this.openInvokeToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
        this.openInvokeToolStripMenuItem.Text = "Open Invoke";
        // 
        // openObjectToolStripMenuItem1
        // 
        this.openObjectToolStripMenuItem1.Name = "openObjectToolStripMenuItem1";
        this.openObjectToolStripMenuItem1.Size = new System.Drawing.Size(141, 22);
        this.openObjectToolStripMenuItem1.Text = "Open Object";
        this.openObjectToolStripMenuItem1.Click += new System.EventHandler(this.openObjectToolStripMenuItem1_Click);
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(3, 13);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(51, 13);
        this.label1.TabIndex = 2;
        this.label1.Text = "Methods:";
        // 
        // lblName
        // 
        this.lblName.AutoSize = true;
        this.lblName.Location = new System.Drawing.Point(60, 0);
        this.lblName.Name = "lblName";
        this.lblName.Size = new System.Drawing.Size(38, 13);
        this.lblName.TabIndex = 0;
        this.lblName.Text = "Name:";
        // 
        // label3
        // 
        this.label3.AutoSize = true;
        this.label3.Location = new System.Drawing.Point(3, 0);
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size(38, 13);
        this.label3.TabIndex = 3;
        this.label3.Text = "Name:";
        // 
        // tableLayoutPanelBottom
        // 
        tableLayoutPanelBottom.ColumnCount = 1;
        tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tableLayoutPanelBottom.Controls.Add(this.label2, 0, 0);
        tableLayoutPanelBottom.Controls.Add(this.listViewProperties, 0, 1);
        tableLayoutPanelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
        tableLayoutPanelBottom.Location = new System.Drawing.Point(0, 0);
        tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
        tableLayoutPanelBottom.RowCount = 2;
        tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tableLayoutPanelBottom.Size = new System.Drawing.Size(1672, 497);
        tableLayoutPanelBottom.TabIndex = 5;
        // 
        // label2
        // 
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(3, 0);
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
        this.listViewProperties.Location = new System.Drawing.Point(3, 16);
        this.listViewProperties.Name = "listViewProperties";
        this.listViewProperties.Size = new System.Drawing.Size(1666, 478);
        this.listViewProperties.TabIndex = 4;
        this.listViewProperties.UseCompatibleStateImageBehavior = false;
        this.listViewProperties.View = System.Windows.Forms.View.Details;
        this.listViewProperties.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_ColumnClick);
        this.listViewProperties.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewProperties_MouseDoubleClick);
        // 
        // contextMenuProperties
        // 
        this.contextMenuProperties.ImageScalingSize = new System.Drawing.Size(28, 28);
        this.contextMenuProperties.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.refreshToolStripMenuItem,
        this.openObjectToolStripMenuItem});
        this.contextMenuProperties.Name = "contextMenuProperties";
        this.contextMenuProperties.Size = new System.Drawing.Size(181, 70);
        // 
        // refreshToolStripMenuItem
        // 
        this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
        this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
        this.refreshToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.refreshToolStripMenuItem.Text = "Refresh";
        this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
        // 
        // openObjectToolStripMenuItem
        // 
        this.openObjectToolStripMenuItem.Name = "openObjectToolStripMenuItem";
        this.openObjectToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.openObjectToolStripMenuItem.Text = "Open Object";
        this.openObjectToolStripMenuItem.Click += new System.EventHandler(this.openObjectToolStripMenuItem_Click);
        // 
        // tabControl
        // 
        this.tabControl.Controls.Add(this.tabPageInvoke);
        this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabControl.Location = new System.Drawing.Point(0, 0);
        this.tabControl.Name = "tabControl";
        this.tabControl.SelectedIndex = 0;
        this.tabControl.Size = new System.Drawing.Size(1688, 1026);
        this.tabControl.TabIndex = 2;
        // 
        // tabPageInvoke
        // 
        this.tabPageInvoke.Controls.Add(this.splitContainer);
        this.tabPageInvoke.Location = new System.Drawing.Point(4, 22);
        this.tabPageInvoke.Name = "tabPageInvoke";
        this.tabPageInvoke.Padding = new System.Windows.Forms.Padding(3);
        this.tabPageInvoke.Size = new System.Drawing.Size(1680, 1000);
        this.tabPageInvoke.TabIndex = 0;
        this.tabPageInvoke.Text = "Invoke";
        this.tabPageInvoke.UseVisualStyleBackColor = true;
        // 
        // splitContainer
        // 
        this.splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitContainer.Location = new System.Drawing.Point(3, 3);
        this.splitContainer.Name = "splitContainer";
        this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
        // 
        // splitContainer.Panel1
        // 
        this.splitContainer.Panel1.Controls.Add(tableLayoutPanelTop);
        // 
        // splitContainer.Panel2
        // 
        this.splitContainer.Panel2.Controls.Add(tableLayoutPanelBottom);
        this.splitContainer.Size = new System.Drawing.Size(1674, 994);
        this.splitContainer.SplitterDistance = 491;
        this.splitContainer.TabIndex = 6;
        // 
        // TypedObjectViewer
        // 
        this.Controls.Add(this.tabControl);
        this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.Name = "TypedObjectViewer";
        this.Size = new System.Drawing.Size(1688, 1026);
        tableLayoutPanelTop.ResumeLayout(false);
        tableLayoutPanelTop.PerformLayout();
        this.contextMenuMethods.ResumeLayout(false);
        tableLayoutPanelBottom.ResumeLayout(false);
        tableLayoutPanelBottom.PerformLayout();
        this.contextMenuProperties.ResumeLayout(false);
        this.tabControl.ResumeLayout(false);
        this.tabPageInvoke.ResumeLayout(false);
        this.splitContainer.Panel1.ResumeLayout(false);
        this.splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
        this.splitContainer.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ContextMenuStrip contextMenuProperties;
    private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem openObjectToolStripMenuItem;
    private System.Windows.Forms.ContextMenuStrip contextMenuMethods;
    private System.Windows.Forms.ToolStripMenuItem openObjectToolStripMenuItem1;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TabPage tabPageInvoke;
    private System.Windows.Forms.SplitContainer splitContainer;
    private System.Windows.Forms.ListView listViewMethods;
    private System.Windows.Forms.Label lblName;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ListView listViewProperties;
    private System.Windows.Forms.ToolStripMenuItem openInvokeToolStripMenuItem;
    private System.Windows.Forms.Label label3;
}