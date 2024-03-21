namespace OleViewDotNet.Forms;

partial class ROTViewer
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
        this.listViewROT = new System.Windows.Forms.ListView();
        this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.bindToObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        this.checkBoxTrustedOnly = new System.Windows.Forms.CheckBox();
        this.contextMenuStrip.SuspendLayout();
        this.tableLayoutPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // listViewROT
        // 
        this.listViewROT.ContextMenuStrip = this.contextMenuStrip;
        this.listViewROT.Dock = System.Windows.Forms.DockStyle.Fill;
        this.listViewROT.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.listViewROT.Location = new System.Drawing.Point(4, 31);
        this.listViewROT.Margin = new System.Windows.Forms.Padding(4);
        this.listViewROT.Name = "listViewROT";
        this.listViewROT.Size = new System.Drawing.Size(1148, 563);
        this.listViewROT.TabIndex = 0;
        this.listViewROT.UseCompatibleStateImageBehavior = false;
        this.listViewROT.View = System.Windows.Forms.View.Details;
        // 
        // contextMenuStrip
        // 
        this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
        this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.refreshToolStripMenuItem,
        this.bindToObjectToolStripMenuItem});
        this.contextMenuStrip.Name = "contextMenuStrip";
        this.contextMenuStrip.Size = new System.Drawing.Size(169, 52);
        // 
        // refreshToolStripMenuItem
        // 
        this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
        this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
        this.refreshToolStripMenuItem.Size = new System.Drawing.Size(168, 24);
        this.refreshToolStripMenuItem.Text = "Refresh";
        this.refreshToolStripMenuItem.Click += new System.EventHandler(this.menuROTRefresh_Click);
        // 
        // bindToObjectToolStripMenuItem
        // 
        this.bindToObjectToolStripMenuItem.Name = "bindToObjectToolStripMenuItem";
        this.bindToObjectToolStripMenuItem.Size = new System.Drawing.Size(168, 24);
        this.bindToObjectToolStripMenuItem.Text = "BindToObject";
        this.bindToObjectToolStripMenuItem.Click += new System.EventHandler(this.menuROTBindToObject_Click);
        // 
        // tableLayoutPanel
        // 
        this.tableLayoutPanel.ColumnCount = 1;
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel.Controls.Add(this.listViewROT, 0, 1);
        this.tableLayoutPanel.Controls.Add(this.checkBoxTrustedOnly, 0, 0);
        this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel.Name = "tableLayoutPanel";
        this.tableLayoutPanel.RowCount = 2;
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel.Size = new System.Drawing.Size(1156, 598);
        this.tableLayoutPanel.TabIndex = 1;
        // 
        // checkBoxTrustedOnly
        // 
        this.checkBoxTrustedOnly.AutoSize = true;
        this.checkBoxTrustedOnly.Location = new System.Drawing.Point(3, 3);
        this.checkBoxTrustedOnly.Name = "checkBoxTrustedOnly";
        this.checkBoxTrustedOnly.Size = new System.Drawing.Size(112, 21);
        this.checkBoxTrustedOnly.TabIndex = 1;
        this.checkBoxTrustedOnly.Text = "Trusted Only";
        this.checkBoxTrustedOnly.UseVisualStyleBackColor = true;
        this.checkBoxTrustedOnly.Click += new System.EventHandler(this.menuROTRefresh_Click);
        // 
        // ROTViewer
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.tableLayoutPanel);
        this.Margin = new System.Windows.Forms.Padding(4);
        this.Name = "ROTViewer";
        this.Size = new System.Drawing.Size(1156, 598);
        this.Load += new System.EventHandler(this.ROTViewer_Load);
        this.contextMenuStrip.ResumeLayout(false);
        this.tableLayoutPanel.ResumeLayout(false);
        this.tableLayoutPanel.PerformLayout();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ListView listViewROT;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
    private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem bindToObjectToolStripMenuItem;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.CheckBox checkBoxTrustedOnly;
}