namespace OleViewDotNet.Forms;

partial class WaitingDialog
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaitingDialog));
        this.lblProgress = new System.Windows.Forms.Label();
        this.progressBar = new System.Windows.Forms.ProgressBar();
        this.btnCancel = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // lblProgress
        // 
        this.lblProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.lblProgress.Location = new System.Drawing.Point(16, 9);
        this.lblProgress.Name = "lblProgress";
        this.lblProgress.Size = new System.Drawing.Size(345, 20);
        this.lblProgress.TabIndex = 0;
        this.lblProgress.Text = "Please Wait.";
        this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // progressBar
        // 
        this.progressBar.Location = new System.Drawing.Point(16, 42);
        this.progressBar.MarqueeAnimationSpeed = 10;
        this.progressBar.Name = "progressBar";
        this.progressBar.Size = new System.Drawing.Size(345, 23);
        this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
        this.progressBar.TabIndex = 1;
        // 
        // btnCancel
        // 
        this.btnCancel.Location = new System.Drawing.Point(143, 71);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(75, 23);
        this.btnCancel.TabIndex = 3;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        // 
        // WaitingDialog
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(373, 99);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.progressBar);
        this.Controls.Add(this.lblProgress);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "WaitingDialog";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Loading...";
        this.Load += new System.EventHandler(this.LoadingDialog_Load);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lblProgress;
    private System.Windows.Forms.ProgressBar progressBar;
    private System.Windows.Forms.Button btnCancel;
}