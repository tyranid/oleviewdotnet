namespace OleViewDotNet.Forms;

partial class PersistStreamTypeViewer
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
        System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        this.btnLoad = new System.Windows.Forms.Button();
        this.btnSave = new System.Windows.Forms.Button();
        this.btnInit = new System.Windows.Forms.Button();
        this.hexEditor = new OleViewDotNet.Forms.HexEditorControl();
        tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        tableLayoutPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // btnLoad
        // 
        this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.btnLoad.Location = new System.Drawing.Point(3, 596);
        this.btnLoad.Name = "btnLoad";
        this.btnLoad.Size = new System.Drawing.Size(75, 23);
        this.btnLoad.TabIndex = 1;
        this.btnLoad.Text = "Load";
        this.btnLoad.UseVisualStyleBackColor = true;
        this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
        // 
        // btnSave
        // 
        this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.btnSave.Location = new System.Drawing.Point(84, 596);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(75, 23);
        this.btnSave.TabIndex = 2;
        this.btnSave.Text = "Save";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
        // 
        // btnInit
        // 
        this.btnInit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.btnInit.Location = new System.Drawing.Point(165, 596);
        this.btnInit.Name = "btnInit";
        this.btnInit.Size = new System.Drawing.Size(75, 23);
        this.btnInit.TabIndex = 3;
        this.btnInit.Text = "Init";
        this.btnInit.UseVisualStyleBackColor = true;
        this.btnInit.Click += new System.EventHandler(this.btnInit_Click);
        // 
        // hexEditor
        // 
        this.hexEditor.Bytes = new byte[0];
        tableLayoutPanel.SetColumnSpan(this.hexEditor, 4);
        this.hexEditor.Dock = System.Windows.Forms.DockStyle.Fill;
        this.hexEditor.Location = new System.Drawing.Point(3, 3);
        this.hexEditor.Name = "hexEditor";
        this.hexEditor.Size = new System.Drawing.Size(1027, 587);
        this.hexEditor.TabIndex = 4;
        // 
        // tableLayoutPanel
        // 
        tableLayoutPanel.ColumnCount = 4;
        tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tableLayoutPanel.Controls.Add(this.btnLoad, 0, 1);
        tableLayoutPanel.Controls.Add(this.hexEditor, 0, 0);
        tableLayoutPanel.Controls.Add(this.btnSave, 1, 1);
        tableLayoutPanel.Controls.Add(this.btnInit, 2, 1);
        tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
        tableLayoutPanel.Name = "tableLayoutPanel";
        tableLayoutPanel.RowCount = 2;
        tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayoutPanel.Size = new System.Drawing.Size(1033, 622);
        tableLayoutPanel.TabIndex = 5;
        // 
        // PersistStreamTypeViewer
        // 
        this.Controls.Add(tableLayoutPanel);
        this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.Name = "PersistStreamTypeViewer";
        this.Size = new System.Drawing.Size(1033, 622);
        tableLayoutPanel.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnLoad;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Button btnInit;
    private HexEditorControl hexEditor;
}