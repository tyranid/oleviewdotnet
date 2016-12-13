namespace OleViewDotNet
{
    partial class TypeLibControl
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
            System.Windows.Forms.ColumnHeader columnHeaderName;
            System.Windows.Forms.ColumnHeader columnHeaderGuid;
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listViewTypes = new System.Windows.Forms.ListView();
            this.textEditor = new ICSharpCode.TextEditor.TextEditorControl();
            columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeaderGuid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnHeaderName
            // 
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 278;
            // 
            // columnHeaderGuid
            // 
            columnHeaderGuid.Text = "IID";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listViewTypes);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textEditor);
            this.splitContainer1.Size = new System.Drawing.Size(699, 418);
            this.splitContainer1.SplitterDistance = 294;
            this.splitContainer1.TabIndex = 0;
            // 
            // listViewTypes
            // 
            this.listViewTypes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeaderName,
            columnHeaderGuid});
            this.listViewTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewTypes.FullRowSelect = true;
            this.listViewTypes.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewTypes.Location = new System.Drawing.Point(0, 0);
            this.listViewTypes.Name = "listViewTypes";
            this.listViewTypes.Size = new System.Drawing.Size(294, 418);
            this.listViewTypes.TabIndex = 0;
            this.listViewTypes.UseCompatibleStateImageBehavior = false;
            this.listViewTypes.View = System.Windows.Forms.View.Details;
            this.listViewTypes.SelectedIndexChanged += new System.EventHandler(this.listViewTypes_SelectedIndexChanged);
            // 
            // textEditor
            // 
            this.textEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textEditor.IsReadOnly = false;
            this.textEditor.Location = new System.Drawing.Point(0, 0);
            this.textEditor.Name = "textEditor";
            this.textEditor.Size = new System.Drawing.Size(401, 418);
            this.textEditor.TabIndex = 0;
            // 
            // TypeLibControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "TypeLibControl";
            this.Size = new System.Drawing.Size(699, 418);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView listViewTypes;
        private ICSharpCode.TextEditor.TextEditorControl textEditor;
    }
}
