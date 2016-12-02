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
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            this.tabControlProperties = new System.Windows.Forms.TabControl();
            this.tabPageClsid = new System.Windows.Forms.TabPage();
            this.listViewProgIDs = new System.Windows.Forms.ListView();
            this.textBoxClsidName = new System.Windows.Forms.TextBox();
            this.lblThreadingModel = new System.Windows.Forms.Label();
            this.lblServerType = new System.Windows.Forms.Label();
            this.textBoxClsid = new System.Windows.Forms.TextBox();
            this.tabPageNoProperties = new System.Windows.Forms.TabPage();
            this.listViewCategories = new System.Windows.Forms.ListView();
            this.tabPageSupportedInterfaces = new System.Windows.Forms.TabPage();
            this.listViewInterfaces = new System.Windows.Forms.ListView();
            this.listViewFactoryInterfaces = new System.Windows.Forms.ListView();
            this.btnRefreshInterfaces = new System.Windows.Forms.Button();
            lblClsid = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            this.tabControlProperties.SuspendLayout();
            this.tabPageClsid.SuspendLayout();
            this.tabPageNoProperties.SuspendLayout();
            this.tabPageSupportedInterfaces.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblClsid
            // 
            lblClsid.AutoSize = true;
            lblClsid.Location = new System.Drawing.Point(11, 44);
            lblClsid.Name = "lblClsid";
            lblClsid.Size = new System.Drawing.Size(41, 13);
            lblClsid.TabIndex = 0;
            lblClsid.Text = "CLSID:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(11, 14);
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
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(11, 128);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(48, 13);
            label3.TabIndex = 7;
            label3.Text = "ProgIDs:";
            // 
            // tabControlProperties
            // 
            this.tabControlProperties.Controls.Add(this.tabPageClsid);
            this.tabControlProperties.Controls.Add(this.tabPageNoProperties);
            this.tabControlProperties.Controls.Add(this.tabPageSupportedInterfaces);
            this.tabControlProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlProperties.Location = new System.Drawing.Point(0, 0);
            this.tabControlProperties.Name = "tabControlProperties";
            this.tabControlProperties.SelectedIndex = 0;
            this.tabControlProperties.Size = new System.Drawing.Size(693, 459);
            this.tabControlProperties.TabIndex = 0;
            // 
            // tabPageClsid
            // 
            this.tabPageClsid.Controls.Add(label4);
            this.tabPageClsid.Controls.Add(this.listViewCategories);
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
            // listViewProgIDs
            // 
            this.listViewProgIDs.FullRowSelect = true;
            this.listViewProgIDs.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewProgIDs.Location = new System.Drawing.Point(11, 144);
            this.listViewProgIDs.MultiSelect = false;
            this.listViewProgIDs.Name = "listViewProgIDs";
            this.listViewProgIDs.Size = new System.Drawing.Size(379, 82);
            this.listViewProgIDs.TabIndex = 6;
            this.listViewProgIDs.UseCompatibleStateImageBehavior = false;
            this.listViewProgIDs.View = System.Windows.Forms.View.List;
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
            this.lblThreadingModel.Location = new System.Drawing.Point(11, 103);
            this.lblThreadingModel.Name = "lblThreadingModel";
            this.lblThreadingModel.Size = new System.Drawing.Size(90, 13);
            this.lblThreadingModel.TabIndex = 3;
            this.lblThreadingModel.Text = "Threading Model:";
            // 
            // lblServerType
            // 
            this.lblServerType.AutoSize = true;
            this.lblServerType.Location = new System.Drawing.Point(11, 75);
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
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(11, 229);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(60, 13);
            label4.TabIndex = 9;
            label4.Text = "Categories:";
            // 
            // listViewCategories
            // 
            this.listViewCategories.FullRowSelect = true;
            this.listViewCategories.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewCategories.Location = new System.Drawing.Point(11, 245);
            this.listViewCategories.MultiSelect = false;
            this.listViewCategories.Name = "listViewCategories";
            this.listViewCategories.Size = new System.Drawing.Size(379, 82);
            this.listViewCategories.TabIndex = 8;
            this.listViewCategories.UseCompatibleStateImageBehavior = false;
            this.listViewCategories.View = System.Windows.Forms.View.List;
            // 
            // tabPageSupportedInterfaces
            // 
            this.tabPageSupportedInterfaces.Controls.Add(this.btnRefreshInterfaces);
            this.tabPageSupportedInterfaces.Controls.Add(label6);
            this.tabPageSupportedInterfaces.Controls.Add(this.listViewFactoryInterfaces);
            this.tabPageSupportedInterfaces.Controls.Add(label5);
            this.tabPageSupportedInterfaces.Controls.Add(this.listViewInterfaces);
            this.tabPageSupportedInterfaces.Location = new System.Drawing.Point(4, 22);
            this.tabPageSupportedInterfaces.Name = "tabPageSupportedInterfaces";
            this.tabPageSupportedInterfaces.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSupportedInterfaces.Size = new System.Drawing.Size(685, 433);
            this.tabPageSupportedInterfaces.TabIndex = 2;
            this.tabPageSupportedInterfaces.Text = "Supported Interfaces";
            this.tabPageSupportedInterfaces.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(6, 13);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(57, 13);
            label5.TabIndex = 9;
            label5.Text = "Interfaces:";
            // 
            // listViewInterfaces
            // 
            this.listViewInterfaces.FullRowSelect = true;
            this.listViewInterfaces.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewInterfaces.Location = new System.Drawing.Point(6, 38);
            this.listViewInterfaces.MultiSelect = false;
            this.listViewInterfaces.Name = "listViewInterfaces";
            this.listViewInterfaces.Size = new System.Drawing.Size(379, 124);
            this.listViewInterfaces.TabIndex = 8;
            this.listViewInterfaces.UseCompatibleStateImageBehavior = false;
            this.listViewInterfaces.View = System.Windows.Forms.View.List;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(6, 175);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(95, 13);
            label6.TabIndex = 11;
            label6.Text = "Factory Interfaces:";
            // 
            // listViewFactoryInterfaces
            // 
            this.listViewFactoryInterfaces.FullRowSelect = true;
            this.listViewFactoryInterfaces.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewFactoryInterfaces.Location = new System.Drawing.Point(6, 200);
            this.listViewFactoryInterfaces.MultiSelect = false;
            this.listViewFactoryInterfaces.Name = "listViewFactoryInterfaces";
            this.listViewFactoryInterfaces.Size = new System.Drawing.Size(379, 124);
            this.listViewFactoryInterfaces.TabIndex = 10;
            this.listViewFactoryInterfaces.UseCompatibleStateImageBehavior = false;
            this.listViewFactoryInterfaces.View = System.Windows.Forms.View.List;
            // 
            // btnRefreshInterfaces
            // 
            this.btnRefreshInterfaces.Location = new System.Drawing.Point(310, 9);
            this.btnRefreshInterfaces.Name = "btnRefreshInterfaces";
            this.btnRefreshInterfaces.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshInterfaces.TabIndex = 12;
            this.btnRefreshInterfaces.Text = "Refresh";
            this.btnRefreshInterfaces.UseVisualStyleBackColor = true;
            this.btnRefreshInterfaces.Click += new System.EventHandler(this.btnRefreshInterfaces_Click);
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
            this.tabPageSupportedInterfaces.ResumeLayout(false);
            this.tabPageSupportedInterfaces.PerformLayout();
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
        private System.Windows.Forms.ListView listViewCategories;
        private System.Windows.Forms.TabPage tabPageSupportedInterfaces;
        private System.Windows.Forms.ListView listViewFactoryInterfaces;
        private System.Windows.Forms.ListView listViewInterfaces;
        private System.Windows.Forms.Button btnRefreshInterfaces;
    }
}