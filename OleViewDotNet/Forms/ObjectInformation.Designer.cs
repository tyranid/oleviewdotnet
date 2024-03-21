namespace OleViewDotNet.Forms;

partial class ObjectInformation
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectInformation));
            this.listViewProperties = new System.Windows.Forms.ListView();
            this.listViewInterfaces = new System.Windows.Forms.ListView();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripDropDownButtonOperations = new System.Windows.Forms.ToolStripDropDownButton();
            this.openDispatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openOLEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.marshalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toHexEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewInterfaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewServerProcessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveStreamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(91, 20);
            label1.TabIndex = 0;
            label1.Text = "Properties:";
            // 
            // label2
            // 
            label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 231);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(88, 20);
            label2.TabIndex = 2;
            label2.Text = "Interfaces:";
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.AutoSize = true;
            tableLayoutPanel.ColumnCount = 5;
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(label2, 0, 2);
            tableLayoutPanel.Controls.Add(this.listViewProperties, 0, 1);
            tableLayoutPanel.Controls.Add(this.listViewInterfaces, 0, 3);
            tableLayoutPanel.Controls.Add(label1, 0, 0);
            tableLayoutPanel.Controls.Add(this.statusStrip, 0, 4);
            tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 5;
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel.Size = new System.Drawing.Size(728, 495);
            tableLayoutPanel.TabIndex = 5;
            // 
            // listViewProperties
            // 
            tableLayoutPanel.SetColumnSpan(this.listViewProperties, 5);
            this.listViewProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewProperties.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewProperties.FullRowSelect = true;
            this.listViewProperties.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewProperties.HideSelection = false;
            this.listViewProperties.Location = new System.Drawing.Point(3, 23);
            this.listViewProperties.Name = "listViewProperties";
            this.listViewProperties.Size = new System.Drawing.Size(722, 205);
            this.listViewProperties.TabIndex = 1;
            this.listViewProperties.UseCompatibleStateImageBehavior = false;
            this.listViewProperties.View = System.Windows.Forms.View.Details;
            // 
            // listViewInterfaces
            // 
            tableLayoutPanel.SetColumnSpan(this.listViewInterfaces, 5);
            this.listViewInterfaces.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewInterfaces.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewInterfaces.FullRowSelect = true;
            this.listViewInterfaces.HideSelection = false;
            this.listViewInterfaces.Location = new System.Drawing.Point(3, 254);
            this.listViewInterfaces.Name = "listViewInterfaces";
            this.listViewInterfaces.Size = new System.Drawing.Size(722, 205);
            this.listViewInterfaces.TabIndex = 3;
            this.listViewInterfaces.UseCompatibleStateImageBehavior = false;
            this.listViewInterfaces.View = System.Windows.Forms.View.Details;
            this.listViewInterfaces.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewInterfaces_ColumnClick);
            this.listViewInterfaces.DoubleClick += new System.EventHandler(this.listViewInterfaces_DoubleClick);
            // 
            // statusStrip
            // 
            tableLayoutPanel.SetColumnSpan(this.statusStrip, 5);
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButtonOperations});
            this.statusStrip.Location = new System.Drawing.Point(0, 463);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(728, 32);
            this.statusStrip.TabIndex = 9;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripDropDownButtonOperations
            // 
            this.toolStripDropDownButtonOperations.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButtonOperations.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openDispatchToolStripMenuItem,
            this.openOLEToolStripMenuItem,
            this.createToolStripMenuItem,
            this.marshalToolStripMenuItem,
            this.viewInterfaceToolStripMenuItem,
            this.viewServerProcessToolStripMenuItem,
            this.saveStreamToolStripMenuItem});
            this.toolStripDropDownButtonOperations.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButtonOperations.Image")));
            this.toolStripDropDownButtonOperations.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButtonOperations.Name = "toolStripDropDownButtonOperations";
            this.toolStripDropDownButtonOperations.Size = new System.Drawing.Size(118, 29);
            this.toolStripDropDownButtonOperations.Text = "Operations";
            // 
            // openDispatchToolStripMenuItem
            // 
            this.openDispatchToolStripMenuItem.Name = "openDispatchToolStripMenuItem";
            this.openDispatchToolStripMenuItem.Size = new System.Drawing.Size(275, 34);
            this.openDispatchToolStripMenuItem.Text = "Open Dispatch";
            this.openDispatchToolStripMenuItem.Click += new System.EventHandler(this.btnDispatch_Click);
            // 
            // openOLEToolStripMenuItem
            // 
            this.openOLEToolStripMenuItem.Name = "openOLEToolStripMenuItem";
            this.openOLEToolStripMenuItem.Size = new System.Drawing.Size(275, 34);
            this.openOLEToolStripMenuItem.Text = "Open OLE Container";
            this.openOLEToolStripMenuItem.Click += new System.EventHandler(this.btnOleContainer_Click);
            // 
            // createToolStripMenuItem
            // 
            this.createToolStripMenuItem.Name = "createToolStripMenuItem";
            this.createToolStripMenuItem.Size = new System.Drawing.Size(275, 34);
            this.createToolStripMenuItem.Text = "Create Instance";
            this.createToolStripMenuItem.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // marshalToolStripMenuItem
            // 
            this.marshalToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewPropertiesToolStripMenuItem,
            this.toHexEditorToolStripMenuItem});
            this.marshalToolStripMenuItem.Name = "marshalToolStripMenuItem";
            this.marshalToolStripMenuItem.Size = new System.Drawing.Size(275, 34);
            this.marshalToolStripMenuItem.Text = "Marshal";
            // 
            // viewPropertiesToolStripMenuItem
            // 
            this.viewPropertiesToolStripMenuItem.Name = "viewPropertiesToolStripMenuItem";
            this.viewPropertiesToolStripMenuItem.Size = new System.Drawing.Size(236, 34);
            this.viewPropertiesToolStripMenuItem.Text = "View Properties";
            this.viewPropertiesToolStripMenuItem.Click += new System.EventHandler(this.viewPropertiesToolStripMenuItem_Click);
            // 
            // toHexEditorToolStripMenuItem
            // 
            this.toHexEditorToolStripMenuItem.Name = "toHexEditorToolStripMenuItem";
            this.toHexEditorToolStripMenuItem.Size = new System.Drawing.Size(236, 34);
            this.toHexEditorToolStripMenuItem.Text = "To Hex Editor";
            this.toHexEditorToolStripMenuItem.Click += new System.EventHandler(this.btnMarshal_Click);
            // 
            // viewInterfaceToolStripMenuItem
            // 
            this.viewInterfaceToolStripMenuItem.Name = "viewInterfaceToolStripMenuItem";
            this.viewInterfaceToolStripMenuItem.Size = new System.Drawing.Size(275, 34);
            this.viewInterfaceToolStripMenuItem.Text = "View Interface";
            this.viewInterfaceToolStripMenuItem.Click += new System.EventHandler(this.viewInterfaceToolStripMenuItem_Click);
            // 
            // viewServerProcessToolStripMenuItem
            // 
            this.viewServerProcessToolStripMenuItem.Name = "viewServerProcessToolStripMenuItem";
            this.viewServerProcessToolStripMenuItem.Size = new System.Drawing.Size(275, 34);
            this.viewServerProcessToolStripMenuItem.Text = "View Server Process";
            this.viewServerProcessToolStripMenuItem.Click += new System.EventHandler(this.viewServerToolStripMenuItem_Click);
            // 
            // saveStreamToolStripMenuItem
            // 
            this.saveStreamToolStripMenuItem.Name = "saveStreamToolStripMenuItem";
            this.saveStreamToolStripMenuItem.Size = new System.Drawing.Size(275, 34);
            this.saveStreamToolStripMenuItem.Text = "Save Stream";
            this.saveStreamToolStripMenuItem.Click += new System.EventHandler(this.btnSaveStream_Click);
            // 
            // ObjectInformation
            // 
            this.Controls.Add(tableLayoutPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ObjectInformation";
            this.Size = new System.Drawing.Size(728, 495);
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.ListView listViewProperties;
    private System.Windows.Forms.ListView listViewInterfaces;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonOperations;
    private System.Windows.Forms.ToolStripMenuItem openDispatchToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem openOLEToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem marshalToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveStreamToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem toHexEditorToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem viewPropertiesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem viewInterfaceToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem viewServerProcessToolStripMenuItem;
}