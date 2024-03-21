namespace OleViewDotNet.Forms;

partial class HexEditorControl
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
        this.hexBox = new Be.Windows.Forms.HexBox();
        this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.loadFromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.saveToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.copyHexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.copyGuidToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.pasteHexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.contextMenuStrip.SuspendLayout();
        this.SuspendLayout();
        // 
        // hexBox
        // 
        this.hexBox.ContextMenuStrip = this.contextMenuStrip;
        this.hexBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.hexBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.hexBox.LineInfoForeColor = System.Drawing.Color.Empty;
        this.hexBox.LineInfoVisible = true;
        this.hexBox.Location = new System.Drawing.Point(0, 0);
        this.hexBox.Name = "hexBox";
        this.hexBox.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
        this.hexBox.Size = new System.Drawing.Size(790, 424);
        this.hexBox.StringViewVisible = true;
        this.hexBox.TabIndex = 0;
        this.hexBox.UseFixedBytesPerLine = true;
        this.hexBox.VScrollBarVisible = true;
        // 
        // contextMenuStrip
        // 
        this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.loadFromFileToolStripMenuItem,
        this.saveToFileToolStripMenuItem,
        this.copyToolStripMenuItem,
        this.copyHexToolStripMenuItem,
        this.copyGuidToolStripMenuItem,
        this.cutToolStripMenuItem,
        this.pasteToolStripMenuItem,
        this.pasteHexToolStripMenuItem,
        this.selectAllToolStripMenuItem});
        this.contextMenuStrip.Name = "contextMenuStrip";
        this.contextMenuStrip.Size = new System.Drawing.Size(165, 224);
        this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
        // 
        // loadFromFileToolStripMenuItem
        // 
        this.loadFromFileToolStripMenuItem.Name = "loadFromFileToolStripMenuItem";
        this.loadFromFileToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
        this.loadFromFileToolStripMenuItem.Text = "Load from File";
        this.loadFromFileToolStripMenuItem.Click += new System.EventHandler(this.loadFromFileToolStripMenuItem_Click);
        // 
        // saveToFileToolStripMenuItem
        // 
        this.saveToFileToolStripMenuItem.Name = "saveToFileToolStripMenuItem";
        this.saveToFileToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
        this.saveToFileToolStripMenuItem.Text = "Save to File";
        this.saveToFileToolStripMenuItem.Click += new System.EventHandler(this.saveToFileToolStripMenuItem_Click);
        // 
        // copyToolStripMenuItem
        // 
        this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
        this.copyToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
        this.copyToolStripMenuItem.Text = "Copy";
        this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
        // 
        // copyHexToolStripMenuItem
        // 
        this.copyHexToolStripMenuItem.Name = "copyHexToolStripMenuItem";
        this.copyHexToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
        this.copyHexToolStripMenuItem.Text = "Copy Hex";
        this.copyHexToolStripMenuItem.Click += new System.EventHandler(this.copyHexToolStripMenuItem_Click);
        // 
        // copyGuidToolStripMenuItem
        // 
        this.copyGuidToolStripMenuItem.Name = "copyGuidToolStripMenuItem";
        this.copyGuidToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
        this.copyGuidToolStripMenuItem.Text = "Copy Guid";
        this.copyGuidToolStripMenuItem.Click += new System.EventHandler(this.copyGuidToolStripMenuItem_Click);
        // 
        // cutToolStripMenuItem
        // 
        this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
        this.cutToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
        this.cutToolStripMenuItem.Text = "Cut";
        this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
        // 
        // pasteToolStripMenuItem
        // 
        this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
        this.pasteToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
        this.pasteToolStripMenuItem.Text = "Paste";
        this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
        // 
        // pasteHexToolStripMenuItem
        // 
        this.pasteHexToolStripMenuItem.Name = "pasteHexToolStripMenuItem";
        this.pasteHexToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
        this.pasteHexToolStripMenuItem.Text = "Paste Hex";
        this.pasteHexToolStripMenuItem.Click += new System.EventHandler(this.pasteHexToolStripMenuItem_Click);
        // 
        // selectAllToolStripMenuItem
        // 
        this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
        this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
        this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
        this.selectAllToolStripMenuItem.Text = "Select All";
        this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
        // 
        // HexEditor
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.hexBox);
        this.Name = "HexEditor";
        this.Size = new System.Drawing.Size(790, 424);
        this.contextMenuStrip.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private Be.Windows.Forms.HexBox hexBox;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
    private System.Windows.Forms.ToolStripMenuItem loadFromFileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveToFileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem pasteHexToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem copyHexToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem copyGuidToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
}
