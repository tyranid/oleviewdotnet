namespace OleViewDotNet
{
    partial class PythonConsole
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
            this.consoleControl = new OleViewDotNet.ConsoleControl();
            this.SuspendLayout();
            // 
            // consoleControl
            // 
            this.consoleControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consoleControl.Location = new System.Drawing.Point(0, 0);
            this.consoleControl.Name = "consoleControl";
            this.consoleControl.Size = new System.Drawing.Size(747, 418);
            this.consoleControl.TabIndex = 0;
            // 
            // PythonConsole
            //             
            this.ClientSize = new System.Drawing.Size(747, 418);
            this.Controls.Add(this.consoleControl);
            this.Name = "PythonConsole";
            this.Text = "PythonConsole";
            this.ResumeLayout(false);

        }

        #endregion

        private ConsoleControl consoleControl;
    }
}