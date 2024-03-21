namespace OleViewDotNet.Forms;

partial class GetTypeForm
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
        this.comboBoxTypes = new System.Windows.Forms.ComboBox();
        this.label1 = new System.Windows.Forms.Label();
        this.label2 = new System.Windows.Forms.Label();
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.checkBoxSetNULL = new System.Windows.Forms.CheckBox();
        this.comboBoxValue = new System.Windows.Forms.ComboBox();
        this.SuspendLayout();
        // 
        // comboBoxTypes
        // 
        this.comboBoxTypes.FormattingEnabled = true;
        this.comboBoxTypes.Location = new System.Drawing.Point(61, 12);
        this.comboBoxTypes.Name = "comboBoxTypes";
        this.comboBoxTypes.Size = new System.Drawing.Size(262, 21);
        this.comboBoxTypes.TabIndex = 0;
        this.comboBoxTypes.SelectedIndexChanged += new System.EventHandler(this.comboBoxTypes_SelectedIndexChanged);
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(12, 15);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(34, 13);
        this.label1.TabIndex = 5;
        this.label1.Text = "Type:";
        // 
        // label2
        // 
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(12, 48);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(37, 13);
        this.label2.TabIndex = 6;
        this.label2.Text = "Value:";
        // 
        // btnOK
        // 
        this.btnOK.Location = new System.Drawing.Point(86, 102);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(75, 23);
        this.btnOK.TabIndex = 3;
        this.btnOK.Text = "OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        // 
        // btnCancel
        // 
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(187, 102);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(75, 23);
        this.btnCancel.TabIndex = 4;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        // 
        // checkBoxSetNULL
        // 
        this.checkBoxSetNULL.AutoSize = true;
        this.checkBoxSetNULL.Location = new System.Drawing.Point(15, 74);
        this.checkBoxSetNULL.Name = "checkBoxSetNULL";
        this.checkBoxSetNULL.Size = new System.Drawing.Size(73, 17);
        this.checkBoxSetNULL.TabIndex = 2;
        this.checkBoxSetNULL.Text = "Set to null";
        this.checkBoxSetNULL.UseVisualStyleBackColor = true;
        // 
        // comboBoxValue
        // 
        this.comboBoxValue.FormattingEnabled = true;
        this.comboBoxValue.Location = new System.Drawing.Point(61, 45);
        this.comboBoxValue.Name = "comboBoxValue";
        this.comboBoxValue.Size = new System.Drawing.Size(262, 21);
        this.comboBoxValue.TabIndex = 1;
        // 
        // GetTypeForm
        // 
        this.AcceptButton = this.btnOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(348, 137);
        this.Controls.Add(this.comboBoxValue);
        this.Controls.Add(this.checkBoxSetNULL);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.label2);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.comboBoxTypes);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "GetTypeForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "GetTypeForm";
        this.Load += new System.EventHandler(this.GetTypeForm_Load);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox comboBoxTypes;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.CheckBox checkBoxSetNULL;
    private System.Windows.Forms.ComboBox comboBoxValue;
}