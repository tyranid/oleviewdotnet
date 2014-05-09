namespace OleViewDotNet
{
    partial class ObjectInformation
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
            this.label1 = new System.Windows.Forms.Label();
            this.listViewProperties = new System.Windows.Forms.ListView();
            this.label2 = new System.Windows.Forms.Label();
            this.listViewInterfaces = new System.Windows.Forms.ListView();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.btnMarshal = new System.Windows.Forms.Button();
            this.btnSaveStream = new System.Windows.Forms.Button();
            this.btnDispatch = new System.Windows.Forms.Button();
            this.btnOleContainer = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Properties:";
            // 
            // listViewProperties
            // 
            this.listViewProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewProperties.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewProperties.FullRowSelect = true;
            this.listViewProperties.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewProperties.Location = new System.Drawing.Point(14, 24);
            this.listViewProperties.Name = "listViewProperties";
            this.listViewProperties.Size = new System.Drawing.Size(1009, 180);
            this.listViewProperties.TabIndex = 1;
            this.listViewProperties.UseCompatibleStateImageBehavior = false;
            this.listViewProperties.View = System.Windows.Forms.View.Details;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Interfaces:";
            // 
            // listViewInterfaces
            // 
            this.listViewInterfaces.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewInterfaces.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewInterfaces.FullRowSelect = true;
            this.listViewInterfaces.Location = new System.Drawing.Point(14, 27);
            this.listViewInterfaces.Name = "listViewInterfaces";
            this.listViewInterfaces.Size = new System.Drawing.Size(1009, 198);
            this.listViewInterfaces.TabIndex = 3;
            this.listViewInterfaces.UseCompatibleStateImageBehavior = false;
            this.listViewInterfaces.View = System.Windows.Forms.View.Details;
            this.listViewInterfaces.DoubleClick += new System.EventHandler(this.listViewInterfaces_DoubleClick);
            // 
            // splitContainer
            // 
            this.splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.label1);
            this.splitContainer.Panel1.Controls.Add(this.listViewProperties);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.btnMarshal);
            this.splitContainer.Panel2.Controls.Add(this.btnSaveStream);
            this.splitContainer.Panel2.Controls.Add(this.btnDispatch);
            this.splitContainer.Panel2.Controls.Add(this.btnOleContainer);
            this.splitContainer.Panel2.Controls.Add(this.label2);
            this.splitContainer.Panel2.Controls.Add(this.listViewInterfaces);
            this.splitContainer.Size = new System.Drawing.Size(1036, 495);
            this.splitContainer.SplitterDistance = 220;
            this.splitContainer.TabIndex = 4;
            // 
            // btnMarshal
            // 
            this.btnMarshal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMarshal.Location = new System.Drawing.Point(257, 235);
            this.btnMarshal.Name = "btnMarshal";
            this.btnMarshal.Size = new System.Drawing.Size(75, 23);
            this.btnMarshal.TabIndex = 7;
            this.btnMarshal.Text = "Marshal";
            this.btnMarshal.UseVisualStyleBackColor = true;
            this.btnMarshal.Click += new System.EventHandler(this.btnMarshal_Click);
            // 
            // btnSaveStream
            // 
            this.btnSaveStream.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSaveStream.Enabled = false;
            this.btnSaveStream.Location = new System.Drawing.Point(176, 235);
            this.btnSaveStream.Name = "btnSaveStream";
            this.btnSaveStream.Size = new System.Drawing.Size(75, 23);
            this.btnSaveStream.TabIndex = 6;
            this.btnSaveStream.Text = "Save Strm";
            this.btnSaveStream.UseVisualStyleBackColor = true;
            this.btnSaveStream.Click += new System.EventHandler(this.btnSaveStream_Click);
            // 
            // btnDispatch
            // 
            this.btnDispatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDispatch.Enabled = false;
            this.btnDispatch.Location = new System.Drawing.Point(95, 235);
            this.btnDispatch.Name = "btnDispatch";
            this.btnDispatch.Size = new System.Drawing.Size(75, 23);
            this.btnDispatch.TabIndex = 5;
            this.btnDispatch.Text = "Open Disp";
            this.btnDispatch.UseVisualStyleBackColor = true;
            this.btnDispatch.Click += new System.EventHandler(this.btnDispatch_Click);
            // 
            // btnOleContainer
            // 
            this.btnOleContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOleContainer.Enabled = false;
            this.btnOleContainer.Location = new System.Drawing.Point(14, 235);
            this.btnOleContainer.Name = "btnOleContainer";
            this.btnOleContainer.Size = new System.Drawing.Size(75, 23);
            this.btnOleContainer.TabIndex = 4;
            this.btnOleContainer.Text = "Open OLE";
            this.btnOleContainer.UseVisualStyleBackColor = true;
            this.btnOleContainer.Click += new System.EventHandler(this.btnOleContainer_Click);
            // 
            // ObjectInformation
            //             
            this.ClientSize = new System.Drawing.Size(1036, 495);
            this.Controls.Add(this.splitContainer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ObjectInformation";            
            this.Text = "ObjectInformation";            
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listViewProperties;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listViewInterfaces;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Button btnOleContainer;
        private System.Windows.Forms.Button btnDispatch;
        private System.Windows.Forms.Button btnSaveStream;
        private System.Windows.Forms.Button btnMarshal;
    }
}