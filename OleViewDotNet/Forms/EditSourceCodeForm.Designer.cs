namespace OleViewDotNet.Forms
{
    partial class EditSourceCodeForm
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
            if (disposing && (components != null))
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditSourceCodeForm));
            this.sourceCodeViewerControl = new OleViewDotNet.Forms.SourceCodeViewerControl();
            this.SuspendLayout();
            // 
            // sourceCodeViewerControl
            // 
            this.sourceCodeViewerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceCodeViewerControl.Location = new System.Drawing.Point(0, 0);
            this.sourceCodeViewerControl.Margin = new System.Windows.Forms.Padding(4);
            this.sourceCodeViewerControl.Name = "sourceCodeViewerControl";
            this.sourceCodeViewerControl.Size = new System.Drawing.Size(1473, 826);
            this.sourceCodeViewerControl.TabIndex = 0;
            // 
            // EditSourceCodeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1473, 826);
            this.Controls.Add(this.sourceCodeViewerControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EditSourceCodeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Source Code";
            this.ResumeLayout(false);

        }

        #endregion

        private SourceCodeViewerControl sourceCodeViewerControl;
    }
}