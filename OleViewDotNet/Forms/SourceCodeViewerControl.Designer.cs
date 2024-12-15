namespace OleViewDotNet.Forms
{
    partial class SourceCodeViewerControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.textEditor = new ICSharpCode.TextEditor.TextEditorControl();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.parseSourceCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoParseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.formatOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemHideComments = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemInterfacesOnly = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemOutputType = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemIDLOutputType = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCppOutputType = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemGenericOutputType = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // textEditor
            // 
            this.textEditor.ContextMenuStrip = this.contextMenuStrip;
            this.textEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textEditor.IsReadOnly = false;
            this.textEditor.Location = new System.Drawing.Point(0, 0);
            this.textEditor.Margin = new System.Windows.Forms.Padding(4);
            this.textEditor.Name = "textEditor";
            this.textEditor.Size = new System.Drawing.Size(1156, 611);
            this.textEditor.TabIndex = 0;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.parseSourceCodeToolStripMenuItem,
            this.autoParseToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.editNameToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.formatOptionsToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(301, 276);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // parseSourceCodeToolStripMenuItem
            // 
            this.parseSourceCodeToolStripMenuItem.Name = "parseSourceCodeToolStripMenuItem";
            this.parseSourceCodeToolStripMenuItem.Size = new System.Drawing.Size(300, 38);
            this.parseSourceCodeToolStripMenuItem.Text = "Parse Source Code";
            this.parseSourceCodeToolStripMenuItem.Click += new System.EventHandler(this.parseSourceCodeToolStripMenuItem_Click);
            // 
            // autoParseToolStripMenuItem
            // 
            this.autoParseToolStripMenuItem.CheckOnClick = true;
            this.autoParseToolStripMenuItem.Name = "autoParseToolStripMenuItem";
            this.autoParseToolStripMenuItem.Size = new System.Drawing.Size(300, 38);
            this.autoParseToolStripMenuItem.Text = "Auto Parse";
            this.autoParseToolStripMenuItem.Click += new System.EventHandler(this.autoParseToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(300, 38);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // editNameToolStripMenuItem
            // 
            this.editNameToolStripMenuItem.Name = "editNameToolStripMenuItem";
            this.editNameToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.editNameToolStripMenuItem.Size = new System.Drawing.Size(300, 38);
            this.editNameToolStripMenuItem.Text = "Edit Name";
            this.editNameToolStripMenuItem.Click += new System.EventHandler(this.editNameToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(300, 38);
            this.refreshToolStripMenuItem.Text = "Refresh";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // formatOptionsToolStripMenuItem
            // 
            this.formatOptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemHideComments,
            this.toolStripMenuItemInterfacesOnly,
            this.toolStripMenuItemOutputType});
            this.formatOptionsToolStripMenuItem.Name = "formatOptionsToolStripMenuItem";
            this.formatOptionsToolStripMenuItem.Size = new System.Drawing.Size(300, 38);
            this.formatOptionsToolStripMenuItem.Text = "Format Options";
            // 
            // toolStripMenuItemHideComments
            // 
            this.toolStripMenuItemHideComments.Name = "toolStripMenuItemHideComments";
            this.toolStripMenuItemHideComments.Size = new System.Drawing.Size(320, 44);
            this.toolStripMenuItemHideComments.Text = "Hide Comments";
            this.toolStripMenuItemHideComments.Click += new System.EventHandler(this.toolStripMenuItemHideComments_Click);
            // 
            // toolStripMenuItemInterfacesOnly
            // 
            this.toolStripMenuItemInterfacesOnly.Name = "toolStripMenuItemInterfacesOnly";
            this.toolStripMenuItemInterfacesOnly.Size = new System.Drawing.Size(320, 44);
            this.toolStripMenuItemInterfacesOnly.Text = "Interfaces Only";
            this.toolStripMenuItemInterfacesOnly.Click += new System.EventHandler(this.toolStripMenuItemInterfacesOnly_Click);
            // 
            // toolStripMenuItemOutputType
            // 
            this.toolStripMenuItemOutputType.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemIDLOutputType,
            this.toolStripMenuItemCppOutputType,
            this.toolStripMenuItemGenericOutputType});
            this.toolStripMenuItemOutputType.Name = "toolStripMenuItemOutputType";
            this.toolStripMenuItemOutputType.Size = new System.Drawing.Size(320, 44);
            this.toolStripMenuItemOutputType.Text = "Output Type";
            // 
            // toolStripMenuItemIDLOutputType
            // 
            this.toolStripMenuItemIDLOutputType.Name = "toolStripMenuItemIDLOutputType";
            this.toolStripMenuItemIDLOutputType.Size = new System.Drawing.Size(228, 44);
            this.toolStripMenuItemIDLOutputType.Text = "IDL";
            this.toolStripMenuItemIDLOutputType.Click += new System.EventHandler(this.toolStripMenuItemIDLOutputType_Click);
            // 
            // toolStripMenuItemCppOutputType
            // 
            this.toolStripMenuItemCppOutputType.Name = "toolStripMenuItemCppOutputType";
            this.toolStripMenuItemCppOutputType.Size = new System.Drawing.Size(228, 44);
            this.toolStripMenuItemCppOutputType.Text = "C++";
            this.toolStripMenuItemCppOutputType.Click += new System.EventHandler(this.toolStripMenuItemCppOutputType_Click);
            // 
            // toolStripMenuItemGenericOutputType
            // 
            this.toolStripMenuItemGenericOutputType.Name = "toolStripMenuItemGenericOutputType";
            this.toolStripMenuItemGenericOutputType.Size = new System.Drawing.Size(228, 44);
            this.toolStripMenuItemGenericOutputType.Text = "Generic";
            this.toolStripMenuItemGenericOutputType.Click += new System.EventHandler(this.toolStripMenuItemGenericOutputType_Click);
            // 
            // SourceCodeViewerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textEditor);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SourceCodeViewerControl";
            this.Size = new System.Drawing.Size(1156, 611);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        /* Added */
        public void ChangeToCpp()
        {
            this.toolStripMenuItemIDLOutputType.Checked = false;
            this.toolStripMenuItemCppOutputType.Checked = true;
            this.toolStripMenuItemGenericOutputType.Checked = false;
        }
        /* Added */

        #endregion

        private ICSharpCode.TextEditor.TextEditorControl textEditor;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem parseSourceCodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem formatOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHideComments;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInterfacesOnly;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOutputType;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemIDLOutputType;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCppOutputType;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemGenericOutputType;
        private System.Windows.Forms.ToolStripMenuItem autoParseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
    }
}
