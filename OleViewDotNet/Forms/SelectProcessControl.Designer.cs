namespace OleViewDotNet.Forms;

partial class SelectProcessControl
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
        System.Windows.Forms.ColumnHeader columnHeaderPID;
        System.Windows.Forms.ColumnHeader columnHeaderName;
        System.Windows.Forms.ColumnHeader columnHeaderUser;
        System.Windows.Forms.ColumnHeader columnHeaderIL;
        System.Windows.Forms.ColumnHeader columnHeaderBitness;
        this.listViewProcesses = new System.Windows.Forms.ListView();
        columnHeaderPID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        columnHeaderUser = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        columnHeaderIL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        columnHeaderBitness = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.SuspendLayout();
        // 
        // columnHeaderPID
        // 
        columnHeaderPID.Text = "PID";
        // 
        // columnHeaderName
        // 
        columnHeaderName.Text = "Name";
        // 
        // columnHeaderUser
        // 
        columnHeaderUser.Text = "User";
        // 
        // columnHeaderIL
        // 
        columnHeaderIL.Text = "Integrity";
        // 
        // listViewProcesses
        // 
        this.listViewProcesses.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        columnHeaderPID,
        columnHeaderName,
        columnHeaderBitness,
        columnHeaderUser,
        columnHeaderIL});
        this.listViewProcesses.Dock = System.Windows.Forms.DockStyle.Fill;
        this.listViewProcesses.FullRowSelect = true;
        this.listViewProcesses.Location = new System.Drawing.Point(0, 0);
        this.listViewProcesses.Margin = new System.Windows.Forms.Padding(4);
        this.listViewProcesses.MultiSelect = false;
        this.listViewProcesses.Name = "listViewProcesses";
        this.listViewProcesses.Size = new System.Drawing.Size(493, 469);
        this.listViewProcesses.TabIndex = 1;
        this.listViewProcesses.UseCompatibleStateImageBehavior = false;
        this.listViewProcesses.View = System.Windows.Forms.View.Details;
        this.listViewProcesses.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewProcesses_ColumnClick);
        this.listViewProcesses.DoubleClick += new System.EventHandler(this.listViewProcesses_DoubleClick);
        // 
        // columnHeaderBitness
        // 
        columnHeaderBitness.Text = "Bitness";
        // 
        // SelectProcessControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.listViewProcesses);
        this.Name = "SelectProcessControl";
        this.Size = new System.Drawing.Size(493, 469);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ListView listViewProcesses;
}
