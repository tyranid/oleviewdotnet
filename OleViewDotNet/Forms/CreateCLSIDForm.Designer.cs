namespace OleViewDotNet.Forms;

partial class CreateCLSIDForm
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
        System.Windows.Forms.Label lblCLSID;
        this.textBoxCLSID = new System.Windows.Forms.TextBox();
        this.label1 = new System.Windows.Forms.Label();
        this.comboBoxClsCtx = new System.Windows.Forms.ComboBox();
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.checkBoxClassFactory = new System.Windows.Forms.CheckBox();
        this.toolTip = new System.Windows.Forms.ToolTip(this.components);
        lblCLSID = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // lblCLSID
        // 
        lblCLSID.AutoSize = true;
        lblCLSID.Location = new System.Drawing.Point(22, 17);
        lblCLSID.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
        lblCLSID.Name = "lblCLSID";
        lblCLSID.Size = new System.Drawing.Size(77, 25);
        lblCLSID.TabIndex = 0;
        lblCLSID.Text = "CLSID:";
        // 
        // textBoxCLSID
        // 
        this.textBoxCLSID.Location = new System.Drawing.Point(127, 11);
        this.textBoxCLSID.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
        this.textBoxCLSID.Name = "textBoxCLSID";
        this.textBoxCLSID.Size = new System.Drawing.Size(583, 29);
        this.textBoxCLSID.TabIndex = 1;
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(22, 63);
        this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(100, 25);
        this.label1.TabIndex = 2;
        this.label1.Text = "CLSCTX:";
        // 
        // comboBoxClsCtx
        // 
        this.comboBoxClsCtx.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboBoxClsCtx.FormattingEnabled = true;
        this.comboBoxClsCtx.Location = new System.Drawing.Point(127, 63);
        this.comboBoxClsCtx.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
        this.comboBoxClsCtx.Name = "comboBoxClsCtx";
        this.comboBoxClsCtx.Size = new System.Drawing.Size(334, 32);
        this.comboBoxClsCtx.TabIndex = 3;
        // 
        // btnOK
        // 
        this.btnOK.Location = new System.Drawing.Point(169, 113);
        this.btnOK.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(138, 42);
        this.btnOK.TabIndex = 4;
        this.btnOK.Text = "OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        // 
        // btnCancel
        // 
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(425, 113);
        this.btnCancel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(138, 42);
        this.btnCancel.TabIndex = 5;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        // 
        // checkBoxClassFactory
        // 
        this.checkBoxClassFactory.AutoSize = true;
        this.checkBoxClassFactory.Location = new System.Drawing.Point(506, 66);
        this.checkBoxClassFactory.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
        this.checkBoxClassFactory.Name = "checkBoxClassFactory";
        this.checkBoxClassFactory.Size = new System.Drawing.Size(158, 29);
        this.checkBoxClassFactory.TabIndex = 6;
        this.checkBoxClassFactory.Text = "Class Factory";
        this.checkBoxClassFactory.UseVisualStyleBackColor = true;
        // 
        // CreateCLSIDForm
        // 
        this.AcceptButton = this.btnOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(728, 170);
        this.Controls.Add(this.checkBoxClassFactory);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.comboBoxClsCtx);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.textBoxCLSID);
        this.Controls.Add(lblCLSID);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "CreateCLSIDForm";
        this.ShowIcon = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Create From CLSID";
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox textBoxCLSID;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox comboBoxClsCtx;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.CheckBox checkBoxClassFactory;
    private System.Windows.Forms.ToolTip toolTip;
}