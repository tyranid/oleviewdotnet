namespace OleViewDotNet.Forms;

partial class MarshalEditorControl
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
        System.Windows.Forms.Label label3;
        this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        this.textBoxObjRefType = new System.Windows.Forms.TextBox();
        this.textBoxIid = new System.Windows.Forms.TextBox();
        this.textBoxIIdName = new System.Windows.Forms.TextBox();
        label1 = new System.Windows.Forms.Label();
        label2 = new System.Windows.Forms.Label();
        label3 = new System.Windows.Forms.Label();
        this.tableLayoutPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // label1
        // 
        label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(3, 6);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(78, 13);
        label1.TabIndex = 0;
        label1.Text = "OBJREF Type:";
        // 
        // label2
        // 
        label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(171, 6);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(24, 13);
        label2.TabIndex = 2;
        label2.Text = "IID:";
        // 
        // label3
        // 
        label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label3.AutoSize = true;
        label3.Location = new System.Drawing.Point(474, 6);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(55, 13);
        label3.TabIndex = 4;
        label3.Text = "IID Name:";
        // 
        // tableLayoutPanel
        // 
        this.tableLayoutPanel.ColumnCount = 6;
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13.70537F));
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44.43553F));
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.8591F));
        this.tableLayoutPanel.Controls.Add(label1, 0, 0);
        this.tableLayoutPanel.Controls.Add(this.textBoxObjRefType, 1, 0);
        this.tableLayoutPanel.Controls.Add(label2, 2, 0);
        this.tableLayoutPanel.Controls.Add(this.textBoxIid, 3, 0);
        this.tableLayoutPanel.Controls.Add(label3, 4, 0);
        this.tableLayoutPanel.Controls.Add(this.textBoxIIdName, 5, 0);
        this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel.Name = "tableLayoutPanel";
        this.tableLayoutPanel.RowCount = 2;
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.tableLayoutPanel.Size = new System.Drawing.Size(791, 484);
        this.tableLayoutPanel.TabIndex = 0;
        // 
        // textBoxObjRefType
        // 
        this.textBoxObjRefType.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxObjRefType.Location = new System.Drawing.Point(87, 3);
        this.textBoxObjRefType.Name = "textBoxObjRefType";
        this.textBoxObjRefType.ReadOnly = true;
        this.textBoxObjRefType.Size = new System.Drawing.Size(78, 20);
        this.textBoxObjRefType.TabIndex = 1;
        this.textBoxObjRefType.Text = "ABC";
        // 
        // textBoxIid
        // 
        this.textBoxIid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxIid.Location = new System.Drawing.Point(201, 3);
        this.textBoxIid.Name = "textBoxIid";
        this.textBoxIid.ReadOnly = true;
        this.textBoxIid.Size = new System.Drawing.Size(267, 20);
        this.textBoxIid.TabIndex = 3;
        // 
        // textBoxIIdName
        // 
        this.textBoxIIdName.Dock = System.Windows.Forms.DockStyle.Fill;
        this.textBoxIIdName.Location = new System.Drawing.Point(535, 3);
        this.textBoxIIdName.Name = "textBoxIIdName";
        this.textBoxIIdName.ReadOnly = true;
        this.textBoxIIdName.Size = new System.Drawing.Size(253, 20);
        this.textBoxIIdName.TabIndex = 5;
        // 
        // MarshalEditorControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.tableLayoutPanel);
        this.Name = "MarshalEditorControl";
        this.Size = new System.Drawing.Size(791, 484);
        this.tableLayoutPanel.ResumeLayout(false);
        this.tableLayoutPanel.PerformLayout();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.TextBox textBoxObjRefType;
    private System.Windows.Forms.TextBox textBoxIid;
    private System.Windows.Forms.TextBox textBoxIIdName;
}
