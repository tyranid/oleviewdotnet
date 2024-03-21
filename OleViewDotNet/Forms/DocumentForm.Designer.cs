namespace OleViewDotNet.Forms;

partial class DocumentForm
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocumentForm));
        this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.closeAllButThisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.closeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.contextMenuStrip.SuspendLayout();
        this.SuspendLayout();
        // 
        // contextMenuStrip
        // 
        this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
        this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.closeToolStripMenuItem,
        this.closeAllButThisToolStripMenuItem,
        this.closeAllToolStripMenuItem,
        this.renameToolStripMenuItem});
        this.contextMenuStrip.Name = "contextMenuStrip";
        this.contextMenuStrip.Size = new System.Drawing.Size(220, 124);
        // 
        // closeToolStripMenuItem
        // 
        this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
        this.closeToolStripMenuItem.Size = new System.Drawing.Size(219, 30);
        this.closeToolStripMenuItem.Text = "Close";
        this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
        // 
        // closeAllButThisToolStripMenuItem
        // 
        this.closeAllButThisToolStripMenuItem.Name = "closeAllButThisToolStripMenuItem";
        this.closeAllButThisToolStripMenuItem.Size = new System.Drawing.Size(219, 30);
        this.closeAllButThisToolStripMenuItem.Text = "Close All But This";
        this.closeAllButThisToolStripMenuItem.Click += new System.EventHandler(this.closeAllButThisToolStripMenuItem_Click);
        // 
        // closeAllToolStripMenuItem
        // 
        this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
        this.closeAllToolStripMenuItem.Size = new System.Drawing.Size(219, 30);
        this.closeAllToolStripMenuItem.Text = "Close All";
        this.closeAllToolStripMenuItem.Click += new System.EventHandler(this.closeAllToolStripMenuItem_Click);
        // 
        // renameToolStripMenuItem
        // 
        this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
        this.renameToolStripMenuItem.Size = new System.Drawing.Size(219, 30);
        this.renameToolStripMenuItem.Text = "Rename";
        this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
        // 
        // DocumentForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1155, 620);
        this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "DocumentForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.TabPageContextMenuStrip = this.contextMenuStrip;
        this.Text = "DocumentForm";
        this.contextMenuStrip.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
    private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem closeAllButThisToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem closeAllToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
}