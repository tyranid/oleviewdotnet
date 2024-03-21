namespace OleViewDotNet.Forms;

partial class DiffRegistryForm
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
        System.Windows.Forms.Label lblLeft;
        System.Windows.Forms.Label lblRight;
        System.Windows.Forms.GroupBox groupBoxDiffMode;
        this.radioRightOnly = new System.Windows.Forms.RadioButton();
        this.radioLeftOnly = new System.Windows.Forms.RadioButton();
        this.comboBoxLeft = new System.Windows.Forms.ComboBox();
        this.btnAddLeft = new System.Windows.Forms.Button();
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnAddRight = new System.Windows.Forms.Button();
        this.comboBoxRight = new System.Windows.Forms.ComboBox();
        lblLeft = new System.Windows.Forms.Label();
        lblRight = new System.Windows.Forms.Label();
        groupBoxDiffMode = new System.Windows.Forms.GroupBox();
        groupBoxDiffMode.SuspendLayout();
        this.SuspendLayout();
        // 
        // lblLeft
        // 
        lblLeft.AutoSize = true;
        lblLeft.Location = new System.Drawing.Point(12, 9);
        lblLeft.Name = "lblLeft";
        lblLeft.Size = new System.Drawing.Size(28, 13);
        lblLeft.TabIndex = 0;
        lblLeft.Text = "Left:";
        // 
        // lblRight
        // 
        lblRight.AutoSize = true;
        lblRight.Location = new System.Drawing.Point(12, 36);
        lblRight.Name = "lblRight";
        lblRight.Size = new System.Drawing.Size(35, 13);
        lblRight.TabIndex = 5;
        lblRight.Text = "Right:";
        // 
        // groupBoxDiffMode
        // 
        groupBoxDiffMode.Controls.Add(this.radioRightOnly);
        groupBoxDiffMode.Controls.Add(this.radioLeftOnly);
        groupBoxDiffMode.Location = new System.Drawing.Point(15, 66);
        groupBoxDiffMode.Name = "groupBoxDiffMode";
        groupBoxDiffMode.Size = new System.Drawing.Size(447, 52);
        groupBoxDiffMode.TabIndex = 9;
        groupBoxDiffMode.TabStop = false;
        groupBoxDiffMode.Text = "Difference Mode";
        // 
        // radioRightOnly
        // 
        this.radioRightOnly.AutoSize = true;
        this.radioRightOnly.Location = new System.Drawing.Point(245, 19);
        this.radioRightOnly.Name = "radioRightOnly";
        this.radioRightOnly.Size = new System.Drawing.Size(154, 17);
        this.radioRightOnly.TabIndex = 2;
        this.radioRightOnly.Text = "Keep Right Difference Only";
        this.radioRightOnly.UseVisualStyleBackColor = true;
        // 
        // radioLeftOnly
        // 
        this.radioLeftOnly.AutoSize = true;
        this.radioLeftOnly.Checked = true;
        this.radioLeftOnly.Cursor = System.Windows.Forms.Cursors.Default;
        this.radioLeftOnly.Location = new System.Drawing.Point(65, 19);
        this.radioLeftOnly.Name = "radioLeftOnly";
        this.radioLeftOnly.Size = new System.Drawing.Size(147, 17);
        this.radioLeftOnly.TabIndex = 1;
        this.radioLeftOnly.TabStop = true;
        this.radioLeftOnly.Text = "Keep Left Difference Only";
        this.radioLeftOnly.UseVisualStyleBackColor = true;
        // 
        // comboBoxLeft
        // 
        this.comboBoxLeft.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
        this.comboBoxLeft.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboBoxLeft.FormattingEnabled = true;
        this.comboBoxLeft.Location = new System.Drawing.Point(46, 6);
        this.comboBoxLeft.Name = "comboBoxLeft";
        this.comboBoxLeft.Size = new System.Drawing.Size(335, 21);
        this.comboBoxLeft.TabIndex = 1;
        // 
        // btnAddLeft
        // 
        this.btnAddLeft.Location = new System.Drawing.Point(387, 4);
        this.btnAddLeft.Name = "btnAddLeft";
        this.btnAddLeft.Size = new System.Drawing.Size(75, 23);
        this.btnAddLeft.TabIndex = 2;
        this.btnAddLeft.Text = "Add";
        this.btnAddLeft.UseVisualStyleBackColor = true;
        this.btnAddLeft.Click += new System.EventHandler(this.btnAddLeft_Click);
        // 
        // btnOK
        // 
        this.btnOK.Location = new System.Drawing.Point(130, 124);
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
        this.btnCancel.Location = new System.Drawing.Point(273, 124);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(75, 23);
        this.btnCancel.TabIndex = 4;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        // 
        // btnAddRight
        // 
        this.btnAddRight.Location = new System.Drawing.Point(387, 31);
        this.btnAddRight.Name = "btnAddRight";
        this.btnAddRight.Size = new System.Drawing.Size(75, 23);
        this.btnAddRight.TabIndex = 7;
        this.btnAddRight.Text = "Add";
        this.btnAddRight.UseVisualStyleBackColor = true;
        this.btnAddRight.Click += new System.EventHandler(this.btnAddRight_Click);
        // 
        // comboBoxRight
        // 
        this.comboBoxRight.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
        this.comboBoxRight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboBoxRight.FormattingEnabled = true;
        this.comboBoxRight.Location = new System.Drawing.Point(46, 33);
        this.comboBoxRight.Name = "comboBoxRight";
        this.comboBoxRight.Size = new System.Drawing.Size(335, 21);
        this.comboBoxRight.TabIndex = 6;
        // 
        // DiffRegistryForm
        // 
        this.AcceptButton = this.btnOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(478, 152);
        this.Controls.Add(this.btnAddRight);
        this.Controls.Add(this.comboBoxRight);
        this.Controls.Add(lblRight);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.btnAddLeft);
        this.Controls.Add(this.comboBoxLeft);
        this.Controls.Add(lblLeft);
        this.Controls.Add(groupBoxDiffMode);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "DiffRegistryForm";
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Difference Registry";
        groupBoxDiffMode.ResumeLayout(false);
        groupBoxDiffMode.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox comboBoxLeft;
    private System.Windows.Forms.Button btnAddLeft;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnAddRight;
    private System.Windows.Forms.ComboBox comboBoxRight;
    private System.Windows.Forms.RadioButton radioLeftOnly;
    private System.Windows.Forms.RadioButton radioRightOnly;
}