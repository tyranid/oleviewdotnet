namespace OleViewDotNet.Forms;

partial class QueryInterfacesOptionsForm
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
        System.Windows.Forms.GroupBox groupBoxServerType;
        System.Windows.Forms.Label label1;
        this.checkBoxInProcHandler = new System.Windows.Forms.CheckBox();
        this.checkBoxLocalServer = new System.Windows.Forms.CheckBox();
        this.checkBoxInProcServer = new System.Windows.Forms.CheckBox();
        this.numericUpDownConcurrentQueries = new System.Windows.Forms.NumericUpDown();
        this.btnOK = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.checkBoxRefreshInterfaces = new System.Windows.Forms.CheckBox();
        groupBoxServerType = new System.Windows.Forms.GroupBox();
        label1 = new System.Windows.Forms.Label();
        groupBoxServerType.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numericUpDownConcurrentQueries)).BeginInit();
        this.SuspendLayout();
        // 
        // groupBoxServerType
        // 
        groupBoxServerType.Controls.Add(this.checkBoxInProcHandler);
        groupBoxServerType.Controls.Add(this.checkBoxLocalServer);
        groupBoxServerType.Controls.Add(this.checkBoxInProcServer);
        groupBoxServerType.Location = new System.Drawing.Point(12, 12);
        groupBoxServerType.Name = "groupBoxServerType";
        groupBoxServerType.Size = new System.Drawing.Size(335, 65);
        groupBoxServerType.TabIndex = 0;
        groupBoxServerType.TabStop = false;
        groupBoxServerType.Text = "COM Server Types";
        // 
        // checkBoxInProcHandler
        // 
        this.checkBoxInProcHandler.AutoSize = true;
        this.checkBoxInProcHandler.Checked = true;
        this.checkBoxInProcHandler.CheckState = System.Windows.Forms.CheckState.Checked;
        this.checkBoxInProcHandler.Location = new System.Drawing.Point(215, 30);
        this.checkBoxInProcHandler.Name = "checkBoxInProcHandler";
        this.checkBoxInProcHandler.Size = new System.Drawing.Size(100, 17);
        this.checkBoxInProcHandler.TabIndex = 2;
        this.checkBoxInProcHandler.Text = "In Proc Handler";
        this.checkBoxInProcHandler.UseVisualStyleBackColor = true;
        // 
        // checkBoxLocalServer
        // 
        this.checkBoxLocalServer.AutoSize = true;
        this.checkBoxLocalServer.Checked = true;
        this.checkBoxLocalServer.CheckState = System.Windows.Forms.CheckState.Checked;
        this.checkBoxLocalServer.Location = new System.Drawing.Point(123, 30);
        this.checkBoxLocalServer.Name = "checkBoxLocalServer";
        this.checkBoxLocalServer.Size = new System.Drawing.Size(86, 17);
        this.checkBoxLocalServer.TabIndex = 1;
        this.checkBoxLocalServer.Text = "Local Server";
        this.checkBoxLocalServer.UseVisualStyleBackColor = true;
        // 
        // checkBoxInProcServer
        // 
        this.checkBoxInProcServer.AutoSize = true;
        this.checkBoxInProcServer.Checked = true;
        this.checkBoxInProcServer.CheckState = System.Windows.Forms.CheckState.Checked;
        this.checkBoxInProcServer.Location = new System.Drawing.Point(17, 30);
        this.checkBoxInProcServer.Name = "checkBoxInProcServer";
        this.checkBoxInProcServer.Size = new System.Drawing.Size(94, 17);
        this.checkBoxInProcServer.TabIndex = 0;
        this.checkBoxInProcServer.Text = "In Proc Server";
        this.checkBoxInProcServer.UseVisualStyleBackColor = true;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(12, 80);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(101, 13);
        label1.TabIndex = 1;
        label1.Text = "Concurrent Queries:";
        // 
        // numericUpDownConcurrentQueries
        // 
        this.numericUpDownConcurrentQueries.Location = new System.Drawing.Point(119, 78);
        this.numericUpDownConcurrentQueries.Minimum = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.numericUpDownConcurrentQueries.Name = "numericUpDownConcurrentQueries";
        this.numericUpDownConcurrentQueries.Size = new System.Drawing.Size(61, 20);
        this.numericUpDownConcurrentQueries.TabIndex = 2;
        this.numericUpDownConcurrentQueries.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        // 
        // btnOK
        // 
        this.btnOK.Location = new System.Drawing.Point(81, 104);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(75, 23);
        this.btnOK.TabIndex = 3;
        this.btnOK.Text = "OK";
        this.btnOK.UseVisualStyleBackColor = true;
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        // 
        // btnCancel
        // 
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(196, 104);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(75, 23);
        this.btnCancel.TabIndex = 4;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        // 
        // checkBoxRefreshInterfaces
        // 
        this.checkBoxRefreshInterfaces.AutoSize = true;
        this.checkBoxRefreshInterfaces.Location = new System.Drawing.Point(206, 81);
        this.checkBoxRefreshInterfaces.Name = "checkBoxRefreshInterfaces";
        this.checkBoxRefreshInterfaces.Size = new System.Drawing.Size(113, 17);
        this.checkBoxRefreshInterfaces.TabIndex = 5;
        this.checkBoxRefreshInterfaces.Text = "Refresh Interfaces";
        this.checkBoxRefreshInterfaces.UseVisualStyleBackColor = true;
        // 
        // QueryInterfacesOptionsForm
        // 
        this.AcceptButton = this.btnOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(363, 134);
        this.Controls.Add(this.checkBoxRefreshInterfaces);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.numericUpDownConcurrentQueries);
        this.Controls.Add(label1);
        this.Controls.Add(groupBoxServerType);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "QueryInterfacesOptionsForm";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Query Interfaces Options";
        groupBoxServerType.ResumeLayout(false);
        groupBoxServerType.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numericUpDownConcurrentQueries)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.CheckBox checkBoxInProcServer;
    private System.Windows.Forms.CheckBox checkBoxLocalServer;
    private System.Windows.Forms.CheckBox checkBoxInProcHandler;
    private System.Windows.Forms.NumericUpDown numericUpDownConcurrentQueries;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.CheckBox checkBoxRefreshInterfaces;
}