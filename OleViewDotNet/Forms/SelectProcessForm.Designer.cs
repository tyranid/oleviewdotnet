namespace OleViewDotNet.Forms;

partial class SelectProcessForm
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
        this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.selectProcessControl = new OleViewDotNet.Forms.SelectProcessControl();
        this.tableLayoutPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // tableLayoutPanel
        // 
        this.tableLayoutPanel.ColumnCount = 2;
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel.Controls.Add(this.btnOK, 0, 1);
        this.tableLayoutPanel.Controls.Add(this.btnCancel, 1, 1);
        this.tableLayoutPanel.Controls.Add(this.selectProcessControl, 0, 0);
        this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanel.Name = "tableLayoutPanel";
        this.tableLayoutPanel.RowCount = 2;
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanel.Size = new System.Drawing.Size(748, 358);
        this.tableLayoutPanel.TabIndex = 0;
        // 
        // btnOK
        // 
        this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.btnOK.Location = new System.Drawing.Point(282, 317);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(89, 38);
        this.btnOK.TabIndex = 0;
        this.btnOK.Text = "OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        // 
        // btnCancel
        // 
        this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(377, 317);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(89, 38);
        this.btnCancel.TabIndex = 1;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        // 
        // selectProcessControl
        // 
        this.tableLayoutPanel.SetColumnSpan(this.selectProcessControl, 2);
        this.selectProcessControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.selectProcessControl.Location = new System.Drawing.Point(3, 3);
        this.selectProcessControl.Name = "selectProcessControl";
        this.selectProcessControl.Size = new System.Drawing.Size(742, 308);
        this.selectProcessControl.TabIndex = 2;
        // 
        // SelectProcessForm
        // 
        this.AcceptButton = this.btnOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(748, 358);
        this.Controls.Add(this.tableLayoutPanel);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SelectProcessForm";
        this.ShowIcon = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Select Process";
        this.tableLayoutPanel.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private SelectProcessControl selectProcessControl;
}