namespace OleViewDotNet.Forms;

partial class ElevatedFactoryServerTypeViewer
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
        this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        this.comboBoxClass = new System.Windows.Forms.ComboBox();
        this.btnCreate = new System.Windows.Forms.Button();
        label1 = new System.Windows.Forms.Label();
        this.tableLayoutPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // label1
        // 
        label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(3, 7);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(35, 13);
        label1.TabIndex = 0;
        label1.Text = "Class:";
        // 
        // tableLayoutPanel
        // 
        this.tableLayoutPanel.ColumnCount = 2;
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel.Controls.Add(label1, 0, 0);
        this.tableLayoutPanel.Controls.Add(this.comboBoxClass, 1, 0);
        this.tableLayoutPanel.Controls.Add(this.btnCreate, 1, 1);
        this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel.Name = "tableLayoutPanel";
        this.tableLayoutPanel.RowCount = 3;
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel.Size = new System.Drawing.Size(664, 357);
        this.tableLayoutPanel.TabIndex = 0;
        // 
        // comboBoxClass
        // 
        this.comboBoxClass.DisplayMember = "Name";
        this.comboBoxClass.Dock = System.Windows.Forms.DockStyle.Fill;
        this.comboBoxClass.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboBoxClass.FormattingEnabled = true;
        this.comboBoxClass.Location = new System.Drawing.Point(44, 3);
        this.comboBoxClass.Name = "comboBoxClass";
        this.comboBoxClass.Size = new System.Drawing.Size(617, 21);
        this.comboBoxClass.TabIndex = 1;
        this.comboBoxClass.ValueMember = "Name";
        // 
        // btnCreate
        // 
        this.btnCreate.Location = new System.Drawing.Point(44, 30);
        this.btnCreate.Name = "btnCreate";
        this.btnCreate.Size = new System.Drawing.Size(117, 70);
        this.btnCreate.TabIndex = 2;
        this.btnCreate.Text = "Create";
        this.btnCreate.UseVisualStyleBackColor = true;
        this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
        // 
        // ElevatedFactoryServerTypeViewer
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.tableLayoutPanel);
        this.Name = "ElevatedFactoryServerTypeViewer";
        this.Size = new System.Drawing.Size(664, 357);
        this.tableLayoutPanel.ResumeLayout(false);
        this.tableLayoutPanel.PerformLayout();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.ComboBox comboBoxClass;
    private System.Windows.Forms.Button btnCreate;
}
