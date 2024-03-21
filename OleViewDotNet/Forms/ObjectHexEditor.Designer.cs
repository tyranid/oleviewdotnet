namespace OleViewDotNet.Forms;

partial class ObjectHexEditor
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
        System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        this.btnLoadFromStream = new System.Windows.Forms.Button();
        this.hexEditor = new OleViewDotNet.Forms.HexEditorControl();
        this.btnUnmarshal = new System.Windows.Forms.Button();
        this.btnMarshalProps = new System.Windows.Forms.Button();
        tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        tableLayoutPanel.SuspendLayout();
        this.SuspendLayout();
        // 
        // tableLayoutPanel
        // 
        tableLayoutPanel.ColumnCount = 4;
        tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tableLayoutPanel.Controls.Add(this.btnLoadFromStream, 0, 1);
        tableLayoutPanel.Controls.Add(this.hexEditor, 0, 0);
        tableLayoutPanel.Controls.Add(this.btnUnmarshal, 1, 1);
        tableLayoutPanel.Controls.Add(this.btnMarshalProps, 2, 1);
        tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
        tableLayoutPanel.Name = "tableLayoutPanel";
        tableLayoutPanel.RowCount = 2;
        tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayoutPanel.Size = new System.Drawing.Size(861, 486);
        tableLayoutPanel.TabIndex = 3;
        // 
        // btnLoadFromStream
        // 
        this.btnLoadFromStream.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.btnLoadFromStream.Location = new System.Drawing.Point(3, 460);
        this.btnLoadFromStream.Name = "btnLoadFromStream";
        this.btnLoadFromStream.Size = new System.Drawing.Size(75, 23);
        this.btnLoadFromStream.TabIndex = 1;
        this.btnLoadFromStream.Text = "Load Stream";
        this.btnLoadFromStream.UseVisualStyleBackColor = true;
        this.btnLoadFromStream.Click += new System.EventHandler(this.btnLoadFromStream_Click);
        // 
        // hexEditor
        // 
        this.hexEditor.Bytes = new byte[0];
        tableLayoutPanel.SetColumnSpan(this.hexEditor, 4);
        this.hexEditor.Dock = System.Windows.Forms.DockStyle.Fill;
        this.hexEditor.Location = new System.Drawing.Point(3, 3);
        this.hexEditor.Name = "hexEditor";
        this.hexEditor.ReadOnly = false;
        this.hexEditor.Size = new System.Drawing.Size(855, 451);
        this.hexEditor.TabIndex = 0;
        // 
        // btnUnmarshal
        // 
        this.btnUnmarshal.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.btnUnmarshal.Location = new System.Drawing.Point(84, 460);
        this.btnUnmarshal.Name = "btnUnmarshal";
        this.btnUnmarshal.Size = new System.Drawing.Size(75, 23);
        this.btnUnmarshal.TabIndex = 2;
        this.btnUnmarshal.Text = "Unmarshal";
        this.btnUnmarshal.UseVisualStyleBackColor = true;
        this.btnUnmarshal.Click += new System.EventHandler(this.btnUnmarshal_Click);
        // 
        // btnMarshalProps
        // 
        this.btnMarshalProps.Location = new System.Drawing.Point(165, 460);
        this.btnMarshalProps.Name = "btnMarshalProps";
        this.btnMarshalProps.Size = new System.Drawing.Size(96, 23);
        this.btnMarshalProps.TabIndex = 3;
        this.btnMarshalProps.Text = "Marshal Props";
        this.btnMarshalProps.UseVisualStyleBackColor = true;
        this.btnMarshalProps.Click += new System.EventHandler(this.btnMarshalProps_Click);
        // 
        // ObjectHexEditor
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(tableLayoutPanel);
        this.Name = "ObjectHexEditor";
        this.Size = new System.Drawing.Size(861, 486);
        tableLayoutPanel.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private HexEditorControl hexEditor;
    private System.Windows.Forms.Button btnLoadFromStream;
    private System.Windows.Forms.Button btnUnmarshal;
    private System.Windows.Forms.Button btnMarshalProps;
}
