namespace OleViewDotNet.Forms;

partial class BuildMonikerForm
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
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.textBoxMoniker = new System.Windows.Forms.TextBox();
        this.label1 = new System.Windows.Forms.Label();
        this.checkBoxParseComposite = new System.Windows.Forms.CheckBox();
        this.SuspendLayout();
        // 
        // btnOK
        // 
        this.btnOK.Location = new System.Drawing.Point(148, 51);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(75, 23);
        this.btnOK.TabIndex = 1;
        this.btnOK.Text = "OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        // 
        // btnCancel
        // 
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(275, 51);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(75, 23);
        this.btnCancel.TabIndex = 2;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        // 
        // textBoxMoniker
        // 
        this.textBoxMoniker.Location = new System.Drawing.Point(54, 2);
        this.textBoxMoniker.Name = "textBoxMoniker";
        this.textBoxMoniker.Size = new System.Drawing.Size(433, 20);
        this.textBoxMoniker.TabIndex = 3;
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(3, 5);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(48, 13);
        this.label1.TabIndex = 4;
        this.label1.Text = "Moniker:";
        // 
        // checkBoxParseComposite
        // 
        this.checkBoxParseComposite.AutoSize = true;
        this.checkBoxParseComposite.Location = new System.Drawing.Point(6, 28);
        this.checkBoxParseComposite.Name = "checkBoxParseComposite";
        this.checkBoxParseComposite.Size = new System.Drawing.Size(212, 17);
        this.checkBoxParseComposite.TabIndex = 5;
        this.checkBoxParseComposite.Text = "Parse as Composite (use ! as separator)";
        this.checkBoxParseComposite.UseVisualStyleBackColor = true;
        // 
        // BuildMonikerForm
        // 
        this.AcceptButton = this.btnOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(499, 79);
        this.Controls.Add(this.checkBoxParseComposite);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.textBoxMoniker);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnOK);
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "BuildMonikerForm";
        this.ShowIcon = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Build Moniker Form";
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.TextBox textBoxMoniker;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckBox checkBoxParseComposite;
}