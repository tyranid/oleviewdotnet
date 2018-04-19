namespace OleViewDotNet
{
    partial class ConfigureProcessAnalysisForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.GroupBox groupBoxSymbols;
            System.Windows.Forms.GroupBox groupBoxParserConfig;
            this.textBoxDbgHelp = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.textBoxSymbolPath = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.checkBoxParseStubMethods = new System.Windows.Forms.CheckBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            groupBoxSymbols = new System.Windows.Forms.GroupBox();
            groupBoxParserConfig = new System.Windows.Forms.GroupBox();
            groupBoxSymbols.SuspendLayout();
            groupBoxParserConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 66);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(98, 17);
            label1.TabIndex = 0;
            label1.Text = "Dbghelp Path:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(7, 98);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(91, 17);
            label2.TabIndex = 3;
            label2.Text = "Symbol Path:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(7, 25);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(641, 34);
            label3.TabIndex = 7;
            label3.Text = "Specify path to dbghelp.dll. \r\nIdeally use the one which comes with Debugging Too" +
    "ls for Windows as that supports symbol servers.";
            // 
            // textBoxDbgHelp
            // 
            this.textBoxDbgHelp.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxDbgHelp.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.textBoxDbgHelp.Location = new System.Drawing.Point(115, 62);
            this.textBoxDbgHelp.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDbgHelp.Name = "textBoxDbgHelp";
            this.textBoxDbgHelp.Size = new System.Drawing.Size(421, 22);
            this.textBoxDbgHelp.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(547, 60);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(100, 28);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // textBoxSymbolPath
            // 
            this.textBoxSymbolPath.Location = new System.Drawing.Point(115, 94);
            this.textBoxSymbolPath.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxSymbolPath.Name = "textBoxSymbolPath";
            this.textBoxSymbolPath.Size = new System.Drawing.Size(531, 22);
            this.textBoxSymbolPath.TabIndex = 4;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(206, 226);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 28);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(376, 226);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // checkBoxParseStubMethods
            // 
            this.checkBoxParseStubMethods.AutoSize = true;
            this.checkBoxParseStubMethods.Location = new System.Drawing.Point(10, 21);
            this.checkBoxParseStubMethods.Name = "checkBoxParseStubMethods";
            this.checkBoxParseStubMethods.Size = new System.Drawing.Size(158, 21);
            this.checkBoxParseStubMethods.TabIndex = 8;
            this.checkBoxParseStubMethods.Text = "Parse Stub Methods";
            this.toolTip.SetToolTip(this.checkBoxParseStubMethods, "Check to parse stub methods in each IPID entry.\r\n*WARNING* can make process parsi" +
        "ng slow.");
            this.checkBoxParseStubMethods.UseVisualStyleBackColor = true;
            // 
            // groupBoxSymbols
            // 
            groupBoxSymbols.Controls.Add(this.textBoxDbgHelp);
            groupBoxSymbols.Controls.Add(label1);
            groupBoxSymbols.Controls.Add(label3);
            groupBoxSymbols.Controls.Add(this.btnBrowse);
            groupBoxSymbols.Controls.Add(label2);
            groupBoxSymbols.Controls.Add(this.textBoxSymbolPath);
            groupBoxSymbols.Location = new System.Drawing.Point(12, 12);
            groupBoxSymbols.Name = "groupBoxSymbols";
            groupBoxSymbols.Size = new System.Drawing.Size(658, 134);
            groupBoxSymbols.TabIndex = 9;
            groupBoxSymbols.TabStop = false;
            groupBoxSymbols.Text = "Symbols";
            // 
            // groupBoxParserConfig
            // 
            groupBoxParserConfig.Controls.Add(this.checkBoxParseStubMethods);
            groupBoxParserConfig.Location = new System.Drawing.Point(12, 152);
            groupBoxParserConfig.Name = "groupBoxParserConfig";
            groupBoxParserConfig.Size = new System.Drawing.Size(658, 57);
            groupBoxParserConfig.TabIndex = 10;
            groupBoxParserConfig.TabStop = false;
            groupBoxParserConfig.Text = "Parser Configuration";
            // 
            // ConfigureProcessAnalysisForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(683, 267);
            this.Controls.Add(groupBoxSymbols);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(groupBoxParserConfig);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigureProcessAnalysisForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Process Analysis";
            groupBoxSymbols.ResumeLayout(false);
            groupBoxSymbols.PerformLayout();
            groupBoxParserConfig.ResumeLayout(false);
            groupBoxParserConfig.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox textBoxDbgHelp;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox textBoxSymbolPath;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox checkBoxParseStubMethods;
        private System.Windows.Forms.ToolTip toolTip;
    }
}