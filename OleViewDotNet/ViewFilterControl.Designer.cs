namespace OleViewDotNet
{
    partial class ViewFilterControl
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
            System.Windows.Forms.Label lblType;
            System.Windows.Forms.Label label1;
            this.comboBoxFilterType = new System.Windows.Forms.ComboBox();
            this.comboBoxFilterOperation = new System.Windows.Forms.ComboBox();
            lblType = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Location = new System.Drawing.Point(12, 14);
            lblType.Name = "lblType";
            lblType.Size = new System.Drawing.Size(34, 13);
            lblType.TabIndex = 0;
            lblType.Text = "Type:";
            // 
            // comboBoxFilterType
            // 
            this.comboBoxFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFilterType.FormattingEnabled = true;
            this.comboBoxFilterType.Location = new System.Drawing.Point(74, 11);
            this.comboBoxFilterType.Name = "comboBoxFilterType";
            this.comboBoxFilterType.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFilterType.TabIndex = 1;
            // 
            // comboBoxFilterOperation
            // 
            this.comboBoxFilterOperation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFilterOperation.FormattingEnabled = true;
            this.comboBoxFilterOperation.Location = new System.Drawing.Point(74, 38);
            this.comboBoxFilterOperation.Name = "comboBoxFilterOperation";
            this.comboBoxFilterOperation.Size = new System.Drawing.Size(121, 21);
            this.comboBoxFilterOperation.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 41);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(56, 13);
            label1.TabIndex = 2;
            label1.Text = "Operation:";
            // 
            // ViewFilterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.comboBoxFilterOperation);
            this.Controls.Add(label1);
            this.Controls.Add(this.comboBoxFilterType);
            this.Controls.Add(lblType);
            this.Name = "ViewFilterControl";
            this.Size = new System.Drawing.Size(646, 222);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox comboBoxFilterType;
        private System.Windows.Forms.ComboBox comboBoxFilterOperation;
    }
}
