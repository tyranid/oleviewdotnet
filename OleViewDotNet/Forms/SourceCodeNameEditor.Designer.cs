namespace OleViewDotNet.Forms
{
    partial class SourceCodeNameEditor
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
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.treeViewEditor = new System.Windows.Forms.TreeView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteWinDBGDqsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 6;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 164F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 165F));
            this.tableLayoutPanel.Controls.Add(this.btnCancel, 4, 1);
            this.tableLayoutPanel.Controls.Add(this.btnOK, 2, 1);
            this.tableLayoutPanel.Controls.Add(this.treeViewEditor, 0, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(800, 450);
            this.tableLayoutPanel.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCancel.Location = new System.Drawing.Point(488, 404);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(144, 43);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOK.Location = new System.Drawing.Point(174, 404);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(144, 43);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // treeViewEditor
            // 
            this.tableLayoutPanel.SetColumnSpan(this.treeViewEditor, 6);
            this.treeViewEditor.ContextMenuStrip = this.contextMenuStrip;
            this.treeViewEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewEditor.FullRowSelect = true;
            this.treeViewEditor.LabelEdit = true;
            this.treeViewEditor.Location = new System.Drawing.Point(3, 3);
            this.treeViewEditor.Name = "treeViewEditor";
            this.treeViewEditor.Size = new System.Drawing.Size(794, 395);
            this.treeViewEditor.TabIndex = 5;
            this.treeViewEditor.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeViewEditor_AfterLabelEdit);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameToolStripMenuItem,
            this.pasteNamesToolStripMenuItem,
            this.resetNamesToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(231, 100);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(230, 32);
            this.renameToolStripMenuItem.Text = "&Rename";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // pasteNamesToolStripMenuItem
            // 
            this.pasteNamesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pasteListToolStripMenuItem,
            this.pasteWinDBGDqsToolStripMenuItem});
            this.pasteNamesToolStripMenuItem.Name = "pasteNamesToolStripMenuItem";
            this.pasteNamesToolStripMenuItem.Size = new System.Drawing.Size(230, 32);
            this.pasteNamesToolStripMenuItem.Text = "&Paste Child Names";
            // 
            // pasteListToolStripMenuItem
            // 
            this.pasteListToolStripMenuItem.Name = "pasteListToolStripMenuItem";
            this.pasteListToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.pasteListToolStripMenuItem.Text = "&List";
            this.pasteListToolStripMenuItem.Click += new System.EventHandler(this.pasteListToolStripMenuItem_Click);
            // 
            // pasteWinDBGDqsToolStripMenuItem
            // 
            this.pasteWinDBGDqsToolStripMenuItem.Name = "pasteWinDBGDqsToolStripMenuItem";
            this.pasteWinDBGDqsToolStripMenuItem.Size = new System.Drawing.Size(270, 34);
            this.pasteWinDBGDqsToolStripMenuItem.Text = "WinDBG d&qs";
            this.pasteWinDBGDqsToolStripMenuItem.Click += new System.EventHandler(this.pasteWinDBGDqsToolStripMenuItem_Click);
            // 
            // resetNamesToolStripMenuItem
            // 
            this.resetNamesToolStripMenuItem.Name = "resetNamesToolStripMenuItem";
            this.resetNamesToolStripMenuItem.Size = new System.Drawing.Size(230, 32);
            this.resetNamesToolStripMenuItem.Text = "Reset Names";
            this.resetNamesToolStripMenuItem.Click += new System.EventHandler(this.resetNamesToolStripMenuItem_Click);
            // 
            // SourceCodeNameEditor
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SourceCodeNameEditor";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SourceCodeNameEditor";
            this.tableLayoutPanel.ResumeLayout(false);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TreeView treeViewEditor;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteNamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteWinDBGDqsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetNamesToolStripMenuItem;
    }
}