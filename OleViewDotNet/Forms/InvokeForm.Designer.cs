namespace OleViewDotNet.Forms;

partial class InvokeForm
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
        this.listViewParameters = new System.Windows.Forms.ListView();
        this.label1 = new System.Windows.Forms.Label();
        this.lblReturn = new System.Windows.Forms.Label();
        this.textBoxReturn = new System.Windows.Forms.TextBox();
        this.btnInvoke = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnOpenObject = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // listViewParameters
        // 
        this.listViewParameters.FullRowSelect = true;
        this.listViewParameters.Location = new System.Drawing.Point(15, 25);
        this.listViewParameters.Name = "listViewParameters";
        this.listViewParameters.Size = new System.Drawing.Size(757, 154);
        this.listViewParameters.TabIndex = 1;
        this.listViewParameters.UseCompatibleStateImageBehavior = false;
        this.listViewParameters.View = System.Windows.Forms.View.Details;
        this.listViewParameters.DoubleClick += new System.EventHandler(this.listViewParameters_DoubleClick);
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(12, 9);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(63, 13);
        this.label1.TabIndex = 2;
        this.label1.Text = "Parameters:";
        // 
        // lblReturn
        // 
        this.lblReturn.AutoSize = true;
        this.lblReturn.Location = new System.Drawing.Point(12, 192);
        this.lblReturn.Name = "lblReturn";
        this.lblReturn.Size = new System.Drawing.Size(42, 13);
        this.lblReturn.TabIndex = 3;
        this.lblReturn.Text = "Return:";
        // 
        // textBoxReturn
        // 
        this.textBoxReturn.Location = new System.Drawing.Point(15, 208);
        this.textBoxReturn.Name = "textBoxReturn";
        this.textBoxReturn.ReadOnly = true;
        this.textBoxReturn.Size = new System.Drawing.Size(757, 20);
        this.textBoxReturn.TabIndex = 4;
        // 
        // btnInvoke
        // 
        this.btnInvoke.Location = new System.Drawing.Point(305, 239);
        this.btnInvoke.Name = "btnInvoke";
        this.btnInvoke.Size = new System.Drawing.Size(75, 23);
        this.btnInvoke.TabIndex = 5;
        this.btnInvoke.Text = "Invoke";
        this.btnInvoke.UseVisualStyleBackColor = true;
        this.btnInvoke.Click += new System.EventHandler(this.btnInvoke_Click);
        // 
        // btnCancel
        // 
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(405, 239);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(75, 23);
        this.btnCancel.TabIndex = 6;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        // 
        // btnOpenObject
        // 
        this.btnOpenObject.Enabled = false;
        this.btnOpenObject.Location = new System.Drawing.Point(697, 239);
        this.btnOpenObject.Name = "btnOpenObject";
        this.btnOpenObject.Size = new System.Drawing.Size(75, 23);
        this.btnOpenObject.TabIndex = 7;
        this.btnOpenObject.Text = "Open Object";
        this.btnOpenObject.UseVisualStyleBackColor = true;
        this.btnOpenObject.Click += new System.EventHandler(this.btnOpenObject_Click);
        // 
        // InvokeForm
        // 
        this.AcceptButton = this.btnInvoke;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(784, 270);
        this.Controls.Add(this.btnOpenObject);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnInvoke);
        this.Controls.Add(this.textBoxReturn);
        this.Controls.Add(this.lblReturn);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.listViewParameters);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "InvokeForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "InvokeForm";
        this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InvokeForm_FormClosed);
        this.Load += new System.EventHandler(this.InvokeForm_Load);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListView listViewParameters;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label lblReturn;
    private System.Windows.Forms.TextBox textBoxReturn;
    private System.Windows.Forms.Button btnInvoke;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOpenObject;
}