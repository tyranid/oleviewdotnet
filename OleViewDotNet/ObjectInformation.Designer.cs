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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
            this.listViewProperties = new System.Windows.Forms.ListView();
            this.listViewInterfaces = new System.Windows.Forms.ListView();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnOleContainer = new System.Windows.Forms.Button();
            this.btnMarshal = new System.Windows.Forms.Button();
            this.btnDispatch = new System.Windows.Forms.Button();
            this.btnSaveStream = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(57, 13);
            label1.TabIndex = 0;
            label1.Text = "Properties:";
            // 
            // label2
            // 
            label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 233);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(57, 13);
            label2.TabIndex = 2;
            label2.Text = "Interfaces:";
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.AutoSize = true;
            tableLayoutPanel.ColumnCount = 5;
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel.Controls.Add(label2, 0, 2);
            tableLayoutPanel.Controls.Add(this.listViewProperties, 0, 1);
            tableLayoutPanel.Controls.Add(this.listViewInterfaces, 0, 3);
            tableLayoutPanel.Controls.Add(label1, 0, 0);
            tableLayoutPanel.Controls.Add(this.btnCreate, 4, 4);
            tableLayoutPanel.Controls.Add(this.btnOleContainer, 0, 4);
            tableLayoutPanel.Controls.Add(this.btnMarshal, 3, 4);
            tableLayoutPanel.Controls.Add(this.btnDispatch, 1, 4);
            tableLayoutPanel.Controls.Add(this.btnSaveStream, 2, 4);
            tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 5;
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel.Size = new System.Drawing.Size(728, 495);
            tableLayoutPanel.TabIndex = 5;
            // 
            // listViewProperties
            // 
            tableLayoutPanel.SetColumnSpan(this.listViewProperties, 5);
            this.listViewProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewProperties.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewProperties.FullRowSelect = true;
            this.listViewProperties.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewProperties.Location = new System.Drawing.Point(3, 16);
            this.listViewProperties.Name = "listViewProperties";
            this.listViewProperties.Size = new System.Drawing.Size(722, 214);
            this.listViewProperties.TabIndex = 1;
            this.listViewProperties.UseCompatibleStateImageBehavior = false;
            this.listViewProperties.View = System.Windows.Forms.View.Details;
            // 
            // listViewInterfaces
            // 
            tableLayoutPanel.SetColumnSpan(this.listViewInterfaces, 5);
            this.listViewInterfaces.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewInterfaces.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewInterfaces.FullRowSelect = true;
            this.listViewInterfaces.Location = new System.Drawing.Point(3, 249);
            this.listViewInterfaces.Name = "listViewInterfaces";
            this.listViewInterfaces.Size = new System.Drawing.Size(722, 214);
            this.listViewInterfaces.TabIndex = 3;
            this.listViewInterfaces.UseCompatibleStateImageBehavior = false;
            this.listViewInterfaces.View = System.Windows.Forms.View.Details;
            this.listViewInterfaces.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewInterfaces_ColumnClick);
            this.listViewInterfaces.DoubleClick += new System.EventHandler(this.listViewInterfaces_DoubleClick);
            // 
            // btnCreate
            // 
            this.btnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCreate.AutoSize = true;
            this.btnCreate.Enabled = false;
            this.btnCreate.Location = new System.Drawing.Point(349, 469);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(66, 23);
            this.btnCreate.TabIndex = 8;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnOleContainer
            // 
            this.btnOleContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOleContainer.AutoSize = true;
            this.btnOleContainer.Enabled = false;
            this.btnOleContainer.Location = new System.Drawing.Point(3, 469);
            this.btnOleContainer.Name = "btnOleContainer";
            this.btnOleContainer.Size = new System.Drawing.Size(81, 23);
            this.btnOleContainer.TabIndex = 4;
            this.btnOleContainer.Text = "Open OLE";
            this.btnOleContainer.UseVisualStyleBackColor = true;
            this.btnOleContainer.Click += new System.EventHandler(this.btnOleContainer_Click);
            // 
            // btnMarshal
            // 
            this.btnMarshal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMarshal.AutoSize = true;
            this.btnMarshal.Location = new System.Drawing.Point(276, 469);
            this.btnMarshal.Name = "btnMarshal";
            this.btnMarshal.Size = new System.Drawing.Size(67, 23);
            this.btnMarshal.TabIndex = 7;
            this.btnMarshal.Text = "Marshal";
            this.btnMarshal.UseVisualStyleBackColor = true;
            this.btnMarshal.Click += new System.EventHandler(this.btnMarshal_Click);
            // 
            // btnDispatch
            // 
            this.btnDispatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDispatch.AutoSize = true;
            this.btnDispatch.Enabled = false;
            this.btnDispatch.Location = new System.Drawing.Point(90, 469);
            this.btnDispatch.Name = "btnDispatch";
            this.btnDispatch.Size = new System.Drawing.Size(81, 23);
            this.btnDispatch.TabIndex = 5;
            this.btnDispatch.Text = "Open Disp";
            this.btnDispatch.UseVisualStyleBackColor = true;
            this.btnDispatch.Click += new System.EventHandler(this.btnDispatch_Click);
            // 
            // btnSaveStream
            // 
            this.btnSaveStream.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSaveStream.AutoSize = true;
            this.btnSaveStream.Enabled = false;
            this.btnSaveStream.Location = new System.Drawing.Point(177, 469);
            this.btnSaveStream.Name = "btnSaveStream";
            this.btnSaveStream.Size = new System.Drawing.Size(93, 23);
            this.btnSaveStream.TabIndex = 6;
            this.btnSaveStream.Text = "Save Stream";
            this.btnSaveStream.UseVisualStyleBackColor = true;
            this.btnSaveStream.Click += new System.EventHandler(this.btnSaveStream_Click);
            // 
            // ObjectInformation
            // 
            this.Controls.Add(tableLayoutPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ObjectInformation";
            this.Size = new System.Drawing.Size(728, 495);
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView listViewProperties;
        private System.Windows.Forms.ListView listViewInterfaces;
        private System.Windows.Forms.Button btnOleContainer;
        private System.Windows.Forms.Button btnDispatch;
        private System.Windows.Forms.Button btnSaveStream;
        private System.Windows.Forms.Button btnMarshal;
        private System.Windows.Forms.Button btnCreate;
    }
}