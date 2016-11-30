namespace OleViewDotNet
{
    partial class PropertiesControl
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
            System.Windows.Forms.Label lblClsid;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            this.tabControlProperties = new System.Windows.Forms.TabControl();
            this.tabPageClsid = new System.Windows.Forms.TabPage();
            this.textBoxClsidName = new System.Windows.Forms.TextBox();
            this.lblThreadingModel = new System.Windows.Forms.Label();
            this.lblServerType = new System.Windows.Forms.Label();
            this.textBoxClsid = new System.Windows.Forms.TextBox();
            this.tabPageNoProperties = new System.Windows.Forms.TabPage();
            this.listViewProgIDs = new System.Windows.Forms.ListView();
            lblClsid = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            this.tabControlProperties.SuspendLayout();
            this.tabPageClsid.SuspendLayout();
            this.tabPageNoProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblClsid
            // 
            lblClsid.AutoSize = true;
            lblClsid.Location = new System.Drawing.Point(8, 44);
            lblClsid.Name = "lblClsid";
            lblClsid.Size = new System.Drawing.Size(41, 13);
            lblClsid.TabIndex = 0;
            lblClsid.Text = "CLSID:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 14);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(38, 13);
            label1.TabIndex = 4;
            label1.Text = "Name:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(18, 13);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(71, 13);
            label2.TabIndex = 0;
            label2.Text = "No Properties";
            // 
            // tabControlProperties
            // 
            this.tabControlProperties.Controls.Add(this.tabPageClsid);
            this.tabControlProperties.Controls.Add(this.tabPageNoProperties);
            this.tabControlProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlProperties.Location = new System.Drawing.Point(0, 0);
            this.tabControlProperties.Name = "tabControlProperties";
            this.tabControlProperties.SelectedIndex = 0;
            this.tabControlProperties.Size = new System.Drawing.Size(693, 459);
            this.tabControlProperties.TabIndex = 0;
            // 
            // tabPageClsid
            // 
            this.tabPageClsid.Controls.Add(label3);
            this.tabPageClsid.Controls.Add(this.listViewProgIDs);
            this.tabPageClsid.Controls.Add(this.textBoxClsidName);
            this.tabPageClsid.Controls.Add(label1);
            this.tabPageClsid.Controls.Add(this.lblThreadingModel);
            this.tabPageClsid.Controls.Add(this.lblServerType);
            this.tabPageClsid.Controls.Add(this.textBoxClsid);
            this.tabPageClsid.Controls.Add(lblClsid);
            this.tabPageClsid.Location = new System.Drawing.Point(4, 22);
            this.tabPageClsid.Name = "tabPageClsid";
            this.tabPageClsid.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageClsid.Size = new System.Drawing.Size(685, 433);
            this.tabPageClsid.TabIndex = 0;
            this.tabPageClsid.Text = "CLSID";
            this.tabPageClsid.UseVisualStyleBackColor = true;
            // 
            // textBoxClsidName
            // 
            this.textBoxClsidName.Location = new System.Drawing.Point(55, 11);
            this.textBoxClsidName.Name = "textBoxClsidName";
            this.textBoxClsidName.ReadOnly = true;
            this.textBoxClsidName.Size = new System.Drawing.Size(335, 20);
            this.textBoxClsidName.TabIndex = 5;
            // 
            // lblThreadingModel
            // 
            this.lblThreadingModel.AutoSize = true;
            this.lblThreadingModel.Location = new System.Drawing.Point(8, 103);
            this.lblThreadingModel.Name = "lblThreadingModel";
            this.lblThreadingModel.Size = new System.Drawing.Size(90, 13);
            this.lblThreadingModel.TabIndex = 3;
            this.lblThreadingModel.Text = "Threading Model:";
            // 
            // lblServerType
            // 
            this.lblServerType.AutoSize = true;
            this.lblServerType.Location = new System.Drawing.Point(8, 75);
            this.lblServerType.Name = "lblServerType";
            this.lblServerType.Size = new System.Drawing.Size(68, 13);
            this.lblServerType.TabIndex = 2;
            this.lblServerType.Text = "Server Type:";
            // 
            // textBoxClsid
            // 
            this.textBoxClsid.Location = new System.Drawing.Point(55, 41);
            this.textBoxClsid.Name = "textBoxClsid";
            this.textBoxClsid.ReadOnly = true;
            this.textBoxClsid.Size = new System.Drawing.Size(335, 20);
            this.textBoxClsid.TabIndex = 1;
            // 
            // tabPageNoProperties
            // 
            this.tabPageNoProperties.Controls.Add(label2);
            this.tabPageNoProperties.Location = new System.Drawing.Point(4, 22);
            this.tabPageNoProperties.Name = "tabPageNoProperties";
            this.tabPageNoProperties.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageNoProperties.Size = new System.Drawing.Size(685, 433);
            this.tabPageNoProperties.TabIndex = 1;
            this.tabPageNoProperties.Text = "No Properties";
            this.tabPageNoProperties.UseVisualStyleBackColor = true;
            // 
            // listViewProgIDs
            // 
            this.listViewProgIDs.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewProgIDs.Location = new System.Drawing.Point(11, 144);
            this.listViewProgIDs.MultiSelect = false;
            this.listViewProgIDs.Name = "listViewProgIDs";
            this.listViewProgIDs.Size = new System.Drawing.Size(354, 123);
            this.listViewProgIDs.TabIndex = 6;
            this.listViewProgIDs.UseCompatibleStateImageBehavior = false;
            this.listViewProgIDs.View = System.Windows.Forms.View.List;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(11, 128);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(48, 13);
            label3.TabIndex = 7;
            label3.Text = "ProgIDs:";
            // 
            // PropertiesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControlProperties);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "PropertiesControl";
            this.Size = new System.Drawing.Size(693, 459);
            this.tabControlProperties.ResumeLayout(false);
            this.tabPageClsid.ResumeLayout(false);
            this.tabPageClsid.PerformLayout();
            this.tabPageNoProperties.ResumeLayout(false);
            this.tabPageNoProperties.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlProperties;
        private System.Windows.Forms.TabPage tabPageClsid;
        private System.Windows.Forms.TabPage tabPageNoProperties;
        private System.Windows.Forms.TextBox textBoxClsid;
        private System.Windows.Forms.Label lblServerType;
        private System.Windows.Forms.Label lblThreadingModel;
        private System.Windows.Forms.TextBox textBoxClsidName;
        private System.Windows.Forms.ListView listViewProgIDs;
    }
}