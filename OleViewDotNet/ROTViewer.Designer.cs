namespace OleViewDotNet
{
    partial class ROTViewer
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
            this.components = new System.ComponentModel.Container();
            this.listViewROT = new System.Windows.Forms.ListView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bindToObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewROT
            // 
            this.listViewROT.ContextMenuStrip = this.contextMenuStrip;
            this.listViewROT.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewROT.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewROT.Location = new System.Drawing.Point(0, 0);
            this.listViewROT.Name = "listViewROT";
            this.listViewROT.Size = new System.Drawing.Size(867, 486);
            this.listViewROT.TabIndex = 0;
            this.listViewROT.UseCompatibleStateImageBehavior = false;
            this.listViewROT.View = System.Windows.Forms.View.Details;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshToolStripMenuItem,
            this.bindToObjectToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(148, 48);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.menuROTRefresh_Click);
            // 
            // bindToObjectToolStripMenuItem
            // 
            this.bindToObjectToolStripMenuItem.Name = "bindToObjectToolStripMenuItem";
            this.bindToObjectToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.bindToObjectToolStripMenuItem.Text = "BindToObject";
            this.bindToObjectToolStripMenuItem.Click += new System.EventHandler(this.menuROTBindToObject_Click);
            // 
            // ROTViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(867, 486);
            this.Controls.Add(this.listViewROT);
            this.Name = "ROTViewer";            
            this.Text = "ROTViewer";
            this.Load += new System.EventHandler(this.ROTViewer_Load);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewROT;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bindToObjectToolStripMenuItem;
    }
}