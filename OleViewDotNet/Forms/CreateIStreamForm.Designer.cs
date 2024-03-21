namespace OleViewDotNet.Forms;

partial class CreateIStreamForm
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
        this.btnCreateWrite = new System.Windows.Forms.Button();
        this.btnCreateRead = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // btnCreateWrite
        // 
        this.btnCreateWrite.Location = new System.Drawing.Point(111, 12);
        this.btnCreateWrite.Name = "btnCreateWrite";
        this.btnCreateWrite.Size = new System.Drawing.Size(75, 43);
        this.btnCreateWrite.TabIndex = 0;
        this.btnCreateWrite.Text = "Write Stream";
        this.btnCreateWrite.UseVisualStyleBackColor = true;
        this.btnCreateWrite.Click += new System.EventHandler(this.btnCreateWrite_Click);
        // 
        // btnCreateRead
        // 
        this.btnCreateRead.Location = new System.Drawing.Point(12, 12);
        this.btnCreateRead.Name = "btnCreateRead";
        this.btnCreateRead.Size = new System.Drawing.Size(75, 43);
        this.btnCreateRead.TabIndex = 1;
        this.btnCreateRead.Text = "Read Stream";
        this.btnCreateRead.UseVisualStyleBackColor = true;
        this.btnCreateRead.Click += new System.EventHandler(this.btnCreateRead_Click);
        // 
        // CreateIStreamForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(198, 67);
        this.Controls.Add(this.btnCreateRead);
        this.Controls.Add(this.btnCreateWrite);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "CreateIStreamForm";
        this.ShowIcon = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Create IStream";
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnCreateWrite;
    private System.Windows.Forms.Button btnCreateRead;


}