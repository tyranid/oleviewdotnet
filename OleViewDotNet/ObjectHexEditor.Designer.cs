namespace OleViewDotNet
{
    partial class ObjectHexEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLoadFromStream = new System.Windows.Forms.Button();
            this.btnUnmarshal = new System.Windows.Forms.Button();
            this.hexEditor = new OleViewDotNet.HexEditor();
            this.SuspendLayout();
            // 
            // btnLoadFromStream
            // 
            this.btnLoadFromStream.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLoadFromStream.Location = new System.Drawing.Point(3, 458);
            this.btnLoadFromStream.Name = "btnLoadFromStream";
            this.btnLoadFromStream.Size = new System.Drawing.Size(75, 23);
            this.btnLoadFromStream.TabIndex = 1;
            this.btnLoadFromStream.Text = "Load Stream";
            this.btnLoadFromStream.UseVisualStyleBackColor = true;
            this.btnLoadFromStream.Click += new System.EventHandler(this.btnLoadFromStream_Click);
            // 
            // btnUnmarshal
            // 
            this.btnUnmarshal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUnmarshal.Location = new System.Drawing.Point(84, 458);
            this.btnUnmarshal.Name = "btnUnmarshal";
            this.btnUnmarshal.Size = new System.Drawing.Size(75, 23);
            this.btnUnmarshal.TabIndex = 2;
            this.btnUnmarshal.Text = "Unmarshal";
            this.btnUnmarshal.UseVisualStyleBackColor = true;
            this.btnUnmarshal.Click += new System.EventHandler(this.btnUnmarshal_Click);
            // 
            // hexEditor
            // 
            this.hexEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hexEditor.Bytes = new byte[0];
            this.hexEditor.Location = new System.Drawing.Point(0, 0);
            this.hexEditor.Name = "hexEditor";
            this.hexEditor.Size = new System.Drawing.Size(858, 452);
            this.hexEditor.TabIndex = 0;
            // 
            // ObjectHexEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnUnmarshal);
            this.Controls.Add(this.btnLoadFromStream);
            this.Controls.Add(this.hexEditor);
            this.Name = "ObjectHexEditor";
            this.Size = new System.Drawing.Size(861, 486);
            this.ResumeLayout(false);

        }

        #endregion

        private HexEditor hexEditor;
        private System.Windows.Forms.Button btnLoadFromStream;
        private System.Windows.Forms.Button btnUnmarshal;
    }
}
