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
            this.components = new System.ComponentModel.Container();
            this.textEditor = new ICSharpCode.TextEditor.TextEditorControl();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeCommentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeComplexTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectOutputTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iDLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.genericToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // textEditor
            // 
            this.textEditor.ContextMenuStrip = this.contextMenuStrip;
            this.textEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textEditor.IsReadOnly = false;
            this.textEditor.Location = new System.Drawing.Point(0, 0);
            this.textEditor.Name = "textEditor";
            this.textEditor.Size = new System.Drawing.Size(867, 489);
            this.textEditor.TabIndex = 0;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeCommentsToolStripMenuItem,
            this.removeComplexTypesToolStripMenuItem,
            this.selectOutputTypeToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(274, 165);
            // 
            // removeCommentsToolStripMenuItem
            // 
            this.removeCommentsToolStripMenuItem.Name = "removeCommentsToolStripMenuItem";
            this.removeCommentsToolStripMenuItem.Size = new System.Drawing.Size(273, 32);
            this.removeCommentsToolStripMenuItem.Text = "Remove Comments";
            this.removeCommentsToolStripMenuItem.Click += new System.EventHandler(this.removeCommentsToolStripMenuItem_Click);
            // 
            // removeComplexTypesToolStripMenuItem
            // 
            this.removeComplexTypesToolStripMenuItem.Name = "removeComplexTypesToolStripMenuItem";
            this.removeComplexTypesToolStripMenuItem.Size = new System.Drawing.Size(273, 32);
            this.removeComplexTypesToolStripMenuItem.Text = "Remove Complex Types";
            this.removeComplexTypesToolStripMenuItem.Click += new System.EventHandler(this.removeComplexTypesToolStripMenuItem_Click);
            // 
            // selectOutputTypeToolStripMenuItem
            // 
            this.selectOutputTypeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.iDLToolStripMenuItem,
            this.cToolStripMenuItem,
            this.genericToolStripMenuItem});
            this.selectOutputTypeToolStripMenuItem.Name = "selectOutputTypeToolStripMenuItem";
            this.selectOutputTypeToolStripMenuItem.Size = new System.Drawing.Size(273, 32);
            this.selectOutputTypeToolStripMenuItem.Text = "Select Output Type";
            // 
            // iDLToolStripMenuItem
            // 
            this.iDLToolStripMenuItem.Name = "iDLToolStripMenuItem";
            this.iDLToolStripMenuItem.Size = new System.Drawing.Size(172, 34);
            this.iDLToolStripMenuItem.Text = "IDL";
            this.iDLToolStripMenuItem.Click += new System.EventHandler(this.iDLToolStripMenuItem_Click);
            // 
            // cToolStripMenuItem
            // 
            this.cToolStripMenuItem.Name = "cToolStripMenuItem";
            this.cToolStripMenuItem.Size = new System.Drawing.Size(172, 34);
            this.cToolStripMenuItem.Text = "C++";
            this.cToolStripMenuItem.Click += new System.EventHandler(this.cToolStripMenuItem_Click);
            // 
            // genericToolStripMenuItem
            // 
            this.genericToolStripMenuItem.Name = "genericToolStripMenuItem";
            this.genericToolStripMenuItem.Size = new System.Drawing.Size(172, 34);
            this.genericToolStripMenuItem.Text = "Generic";
            this.genericToolStripMenuItem.Click += new System.EventHandler(this.genericToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(273, 32);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // FormattedObjectControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textEditor);
            this.Name = "FormattedObjectControl";
            this.Size = new System.Drawing.Size(867, 489);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ICSharpCode.TextEditor.TextEditorControl textEditor;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem removeCommentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeComplexTypesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectOutputTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iDLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem genericToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
    }
}
