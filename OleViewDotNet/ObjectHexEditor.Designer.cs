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
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
            this.btnLoadFromStream = new System.Windows.Forms.Button();
            this.btnUnmarshal = new System.Windows.Forms.Button();
            this.hexEditor = new OleViewDotNet.HexEditor();
            tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLoadFromStream
            // 
            this.btnLoadFromStream.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnLoadFromStream.Location = new System.Drawing.Point(3, 460);
            this.btnLoadFromStream.Name = "btnLoadFromStream";
            this.btnLoadFromStream.Size = new System.Drawing.Size(75, 23);
            this.btnLoadFromStream.TabIndex = 1;
            this.btnLoadFromStream.Text = "Load Stream";
            this.btnLoadFromStream.UseVisualStyleBackColor = true;
            this.btnLoadFromStream.Click += new System.EventHandler(this.btnLoadFromStream_Click);
            // 
            // btnUnmarshal
            // 
            this.btnUnmarshal.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnUnmarshal.Location = new System.Drawing.Point(84, 460);
            this.btnUnmarshal.Name = "btnUnmarshal";
            this.btnUnmarshal.Size = new System.Drawing.Size(75, 23);
            this.btnUnmarshal.TabIndex = 2;
            this.btnUnmarshal.Text = "Unmarshal";
            this.btnUnmarshal.UseVisualStyleBackColor = true;
            this.btnUnmarshal.Click += new System.EventHandler(this.btnUnmarshal_Click);
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 3;
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(this.btnLoadFromStream, 0, 1);
            tableLayoutPanel.Controls.Add(this.hexEditor, 0, 0);
            tableLayoutPanel.Controls.Add(this.btnUnmarshal, 1, 1);
            tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 2;
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel.Size = new System.Drawing.Size(861, 486);
            tableLayoutPanel.TabIndex = 3;
            // 
            // hexEditor
            // 
            this.hexEditor.Bytes = new byte[0];
            tableLayoutPanel.SetColumnSpan(this.hexEditor, 3);
            this.hexEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hexEditor.Location = new System.Drawing.Point(3, 3);
            this.hexEditor.Name = "hexEditor";
            this.hexEditor.Size = new System.Drawing.Size(855, 451);
            this.hexEditor.TabIndex = 0;
            // 
            // ObjectHexEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(tableLayoutPanel);
            this.Name = "ObjectHexEditor";
            this.Size = new System.Drawing.Size(861, 486);
            tableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HexEditor hexEditor;
        private System.Windows.Forms.Button btnLoadFromStream;
        private System.Windows.Forms.Button btnUnmarshal;
    }
}
