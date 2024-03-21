namespace OleViewDotNet.Forms;

partial class ClassFactoryTypeViewer
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
        this.btnCreateInstance = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // btnCreateInstance
        // 
        this.btnCreateInstance.Location = new System.Drawing.Point(3, 0);
        this.btnCreateInstance.Name = "btnCreateInstance";
        this.btnCreateInstance.Size = new System.Drawing.Size(199, 123);
        this.btnCreateInstance.TabIndex = 0;
        this.btnCreateInstance.Text = "Create Instance";
        this.btnCreateInstance.UseVisualStyleBackColor = true;
        this.btnCreateInstance.Click += new System.EventHandler(this.btnCreateInstance_Click);
        // 
        // ClassFactoryTypeViewer
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.btnCreateInstance);
        this.Name = "ClassFactoryTypeViewer";
        this.Size = new System.Drawing.Size(435, 266);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnCreateInstance;
}
