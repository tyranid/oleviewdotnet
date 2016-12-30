namespace OleViewDotNet
{
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
            groupBoxServerType.Location = new System.Drawing.Point(22, 22);
            groupBoxServerType.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            groupBoxServerType.Name = "groupBoxServerType";
            groupBoxServerType.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            groupBoxServerType.Size = new System.Drawing.Size(614, 120);
            groupBoxServerType.TabIndex = 0;
            groupBoxServerType.TabStop = false;
            groupBoxServerType.Text = "COM Server Types";
            // 
            // checkBoxInProcHandler
            // 
            this.checkBoxInProcHandler.AutoSize = true;
            this.checkBoxInProcHandler.Checked = true;
            this.checkBoxInProcHandler.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxInProcHandler.Location = new System.Drawing.Point(394, 55);
            this.checkBoxInProcHandler.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.checkBoxInProcHandler.Name = "checkBoxInProcHandler";
            this.checkBoxInProcHandler.Size = new System.Drawing.Size(172, 29);
            this.checkBoxInProcHandler.TabIndex = 2;
            this.checkBoxInProcHandler.Text = "In Proc Handler";
            this.checkBoxInProcHandler.UseVisualStyleBackColor = true;
            // 
            // checkBoxLocalServer
            // 
            this.checkBoxLocalServer.AutoSize = true;
            this.checkBoxLocalServer.Checked = true;
            this.checkBoxLocalServer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLocalServer.Location = new System.Drawing.Point(226, 55);
            this.checkBoxLocalServer.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.checkBoxLocalServer.Name = "checkBoxLocalServer";
            this.checkBoxLocalServer.Size = new System.Drawing.Size(148, 29);
            this.checkBoxLocalServer.TabIndex = 1;
            this.checkBoxLocalServer.Text = "Local Server";
            this.checkBoxLocalServer.UseVisualStyleBackColor = true;
            // 
            // checkBoxInProcServer
            // 
            this.checkBoxInProcServer.AutoSize = true;
            this.checkBoxInProcServer.Checked = true;
            this.checkBoxInProcServer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxInProcServer.Location = new System.Drawing.Point(31, 55);
            this.checkBoxInProcServer.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.checkBoxInProcServer.Name = "checkBoxInProcServer";
            this.checkBoxInProcServer.Size = new System.Drawing.Size(162, 29);
            this.checkBoxInProcServer.TabIndex = 0;
            this.checkBoxInProcServer.Text = "In Proc Server";
            this.checkBoxInProcServer.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(22, 148);
            label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(189, 25);
            label1.TabIndex = 1;
            label1.Text = "Concurrent Queries:";
            // 
            // numericUpDownConcurrentQueries
            // 
            this.numericUpDownConcurrentQueries.Location = new System.Drawing.Point(218, 144);
            this.numericUpDownConcurrentQueries.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.numericUpDownConcurrentQueries.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownConcurrentQueries.Name = "numericUpDownConcurrentQueries";
            this.numericUpDownConcurrentQueries.Size = new System.Drawing.Size(112, 29);
            this.numericUpDownConcurrentQueries.TabIndex = 2;
            this.numericUpDownConcurrentQueries.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(149, 192);
            this.btnOK.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(138, 42);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(359, 192);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(138, 42);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // checkBoxRefreshInterfaces
            // 
            this.checkBoxRefreshInterfaces.AutoSize = true;
            this.checkBoxRefreshInterfaces.Location = new System.Drawing.Point(378, 150);
            this.checkBoxRefreshInterfaces.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.checkBoxRefreshInterfaces.Name = "checkBoxRefreshInterfaces";
            this.checkBoxRefreshInterfaces.Size = new System.Drawing.Size(195, 29);
            this.checkBoxRefreshInterfaces.TabIndex = 5;
            this.checkBoxRefreshInterfaces.Text = "Refresh Interfaces";
            this.checkBoxRefreshInterfaces.UseVisualStyleBackColor = true;
            // 
            // QueryInterfacesOptionsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(658, 247);
            this.ControlBox = false;
            this.Controls.Add(this.checkBoxRefreshInterfaces);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.numericUpDownConcurrentQueries);
            this.Controls.Add(label1);
            this.Controls.Add(groupBoxServerType);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximumSize = new System.Drawing.Size(682, 311);
            this.MinimumSize = new System.Drawing.Size(682, 311);
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
}