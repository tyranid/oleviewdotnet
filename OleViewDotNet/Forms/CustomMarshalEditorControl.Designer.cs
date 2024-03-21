namespace OleViewDotNet.Forms;

partial class CustomMarshalEditorControl
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
        System.Windows.Forms.Label label1;
        System.Windows.Forms.Label label2;
        this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        this.textBoxClsid = new System.Windows.Forms.TextBox();
        this.textBoxName = new System.Windows.Forms.TextBox();
        this.hexEditor = new OleViewDotNet.Forms.HexEditorControl();
        label1 = new System.Windows.Forms.Label();
        label2 = new System.Windows.Forms.Label();
        this.tableLayoutPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // label1
        // 
        label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(3, 6);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(41, 13);
        label1.TabIndex = 0;
        label1.Text = "CLSID:";
        // 
        // label2
        // 
        label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(339, 6);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(38, 13);
        label2.TabIndex = 2;
        label2.Text = "Name:";
        // 
        // tableLayoutPanel
        // 
        this.tableLayoutPanel.ColumnCount = 4;
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel.Controls.Add(label1, 0, 0);
        this.tableLayoutPanel.Controls.Add(this.textBoxClsid, 1, 0);
        this.tableLayoutPanel.Controls.Add(label2, 2, 0);
        this.tableLayoutPanel.Controls.Add(this.textBoxName, 3, 0);
        this.tableLayoutPanel.Controls.Add(this.hexEditor, 0, 1);
        this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel.Name = "tableLayoutPanel";
        this.tableLayoutPanel.RowCount = 2;
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel.Size = new System.Drawing.Size(669, 474);
        this.tableLayoutPanel.TabIndex = 0;
        // 
        // textBoxClsid
        // 
        this.textBoxClsid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxClsid.Location = new System.Drawing.Point(50, 3);
        this.textBoxClsid.Name = "textBoxClsid";
        this.textBoxClsid.ReadOnly = true;
        this.textBoxClsid.Size = new System.Drawing.Size(283, 20);
        this.textBoxClsid.TabIndex = 1;
        // 
        // textBoxName
        // 
        this.textBoxName.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxName.Location = new System.Drawing.Point(383, 3);
        this.textBoxName.Name = "textBoxName";
        this.textBoxName.ReadOnly = true;
        this.textBoxName.Size = new System.Drawing.Size(283, 20);
        this.textBoxName.TabIndex = 3;
        // 
        // hexEditor
        // 
        this.hexEditor.Bytes = new byte[0];
        this.tableLayoutPanel.SetColumnSpan(this.hexEditor, 4);
        this.hexEditor.Dock = System.Windows.Forms.DockStyle.Fill;
        this.hexEditor.Location = new System.Drawing.Point(3, 29);
        this.hexEditor.Name = "hexEditor";
        this.hexEditor.ReadOnly = true;
        this.hexEditor.Size = new System.Drawing.Size(663, 442);
        this.hexEditor.TabIndex = 4;
        // 
        // CustomMarshalEditorControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.tableLayoutPanel);
        this.Name = "CustomMarshalEditorControl";
        this.Size = new System.Drawing.Size(669, 474);
        this.tableLayoutPanel.ResumeLayout(false);
        this.tableLayoutPanel.PerformLayout();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.TextBox textBoxClsid;
    private System.Windows.Forms.TextBox textBoxName;
    private HexEditorControl hexEditor;
}
