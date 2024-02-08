namespace OleViewDotNet.Forms;

partial class SettingsForm
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
            System.Windows.Forms.GroupBox groupBoxProcessParserConfig;
            this.textBoxDbgHelp = new System.Windows.Forms.TextBox();
            this.btnBrowseDbgHelpPath = new System.Windows.Forms.Button();
            this.textBoxSymbolPath = new System.Windows.Forms.TextBox();
            this.checkBoxParseActCtx = new System.Windows.Forms.CheckBox();
            this.checkBoxParseRegisteredClasses = new System.Windows.Forms.CheckBox();
            this.checkBoxResolveMethodNames = new System.Windows.Forms.CheckBox();
            this.checkBoxParseStubMethods = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.checkBoxProxyParserResolveSymbols = new System.Windows.Forms.CheckBox();
            this.groupBoxProxyParserConfig = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            groupBoxSymbols = new System.Windows.Forms.GroupBox();
            groupBoxProcessParserConfig = new System.Windows.Forms.GroupBox();
            groupBoxSymbols.SuspendLayout();
            groupBoxProcessParserConfig.SuspendLayout();
            this.groupBoxProxyParserConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 82);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(110, 20);
            label1.TabIndex = 0;
            label1.Text = "Dbghelp Path:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(8, 122);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(102, 20);
            label2.TabIndex = 3;
            label2.Text = "Symbol Path:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(8, 31);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(715, 40);
            label3.TabIndex = 7;
            label3.Text = "Specify path to dbghelp.dll. \r\nIdeally use the one which comes with Debugging Too" +
    "ls for Windows as that supports symbol servers.";
            // 
            // groupBoxSymbols
            // 
            groupBoxSymbols.Controls.Add(this.textBoxDbgHelp);
            groupBoxSymbols.Controls.Add(label1);
            groupBoxSymbols.Controls.Add(label3);
            groupBoxSymbols.Controls.Add(this.btnBrowseDbgHelpPath);
            groupBoxSymbols.Controls.Add(label2);
            groupBoxSymbols.Controls.Add(this.textBoxSymbolPath);
            groupBoxSymbols.Location = new System.Drawing.Point(14, 15);
            groupBoxSymbols.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            groupBoxSymbols.Name = "groupBoxSymbols";
            groupBoxSymbols.Padding = new System.Windows.Forms.Padding(3, 5, 3, 5);
            groupBoxSymbols.Size = new System.Drawing.Size(740, 168);
            groupBoxSymbols.TabIndex = 9;
            groupBoxSymbols.TabStop = false;
            groupBoxSymbols.Text = "Symbols";
            // 
            // textBoxDbgHelp
            // 
            this.textBoxDbgHelp.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxDbgHelp.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.textBoxDbgHelp.Location = new System.Drawing.Point(129, 78);
            this.textBoxDbgHelp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxDbgHelp.Name = "textBoxDbgHelp";
            this.textBoxDbgHelp.Size = new System.Drawing.Size(474, 26);
            this.textBoxDbgHelp.TabIndex = 1;
            // 
            // btnBrowseDbgHelpPath
            // 
            this.btnBrowseDbgHelpPath.Location = new System.Drawing.Point(615, 75);
            this.btnBrowseDbgHelpPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnBrowseDbgHelpPath.Name = "btnBrowseDbgHelpPath";
            this.btnBrowseDbgHelpPath.Size = new System.Drawing.Size(112, 35);
            this.btnBrowseDbgHelpPath.TabIndex = 2;
            this.btnBrowseDbgHelpPath.Text = "Browse";
            this.btnBrowseDbgHelpPath.UseVisualStyleBackColor = true;
            this.btnBrowseDbgHelpPath.Click += new System.EventHandler(this.btnBrowseDbgHelpPath_Click);
            // 
            // textBoxSymbolPath
            // 
            this.textBoxSymbolPath.Location = new System.Drawing.Point(129, 118);
            this.textBoxSymbolPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxSymbolPath.Name = "textBoxSymbolPath";
            this.textBoxSymbolPath.Size = new System.Drawing.Size(596, 26);
            this.textBoxSymbolPath.TabIndex = 4;
            // 
            // groupBoxProcessParserConfig
            // 
            groupBoxProcessParserConfig.Controls.Add(this.checkBoxParseActCtx);
            groupBoxProcessParserConfig.Controls.Add(this.checkBoxParseRegisteredClasses);
            groupBoxProcessParserConfig.Controls.Add(this.checkBoxResolveMethodNames);
            groupBoxProcessParserConfig.Controls.Add(this.checkBoxParseStubMethods);
            groupBoxProcessParserConfig.Location = new System.Drawing.Point(12, 182);
            groupBoxProcessParserConfig.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            groupBoxProcessParserConfig.Name = "groupBoxProcessParserConfig";
            groupBoxProcessParserConfig.Padding = new System.Windows.Forms.Padding(3, 5, 3, 5);
            groupBoxProcessParserConfig.Size = new System.Drawing.Size(742, 60);
            groupBoxProcessParserConfig.TabIndex = 10;
            groupBoxProcessParserConfig.TabStop = false;
            groupBoxProcessParserConfig.Text = "Process Parser Configuration";
            // 
            // checkBoxParseActCtx
            // 
            this.checkBoxParseActCtx.AutoSize = true;
            this.checkBoxParseActCtx.Location = new System.Drawing.Point(579, 26);
            this.checkBoxParseActCtx.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBoxParseActCtx.Name = "checkBoxParseActCtx";
            this.checkBoxParseActCtx.Size = new System.Drawing.Size(127, 24);
            this.checkBoxParseActCtx.TabIndex = 11;
            this.checkBoxParseActCtx.Text = "Parse ActCtx";
            this.toolTip.SetToolTip(this.checkBoxParseActCtx, "Check to parse activation context in each process.\r\n*WARNING* can make process pa" +
        "rsing slow.");
            this.checkBoxParseActCtx.UseVisualStyleBackColor = true;
            // 
            // checkBoxParseRegisteredClasses
            // 
            this.checkBoxParseRegisteredClasses.AutoSize = true;
            this.checkBoxParseRegisteredClasses.Location = new System.Drawing.Point(352, 26);
            this.checkBoxParseRegisteredClasses.Name = "checkBoxParseRegisteredClasses";
            this.checkBoxParseRegisteredClasses.Size = new System.Drawing.Size(218, 24);
            this.checkBoxParseRegisteredClasses.TabIndex = 10;
            this.checkBoxParseRegisteredClasses.Text = "Parse Registered Classes";
            this.toolTip.SetToolTip(this.checkBoxParseRegisteredClasses, "Check to parse registered classes in each process.\r\n*WARNING* can make process pa" +
        "rsing slow.\r\n");
            this.checkBoxParseRegisteredClasses.UseVisualStyleBackColor = true;
            // 
            // checkBoxResolveMethodNames
            // 
            this.checkBoxResolveMethodNames.AutoSize = true;
            this.checkBoxResolveMethodNames.Location = new System.Drawing.Point(12, 26);
            this.checkBoxResolveMethodNames.Name = "checkBoxResolveMethodNames";
            this.checkBoxResolveMethodNames.Size = new System.Drawing.Size(204, 24);
            this.checkBoxResolveMethodNames.TabIndex = 9;
            this.checkBoxResolveMethodNames.Text = "Resolve Method Names";
            this.toolTip.SetToolTip(this.checkBoxResolveMethodNames, "Check to resolve the symbols for interface methods.\r\nWARNING* can make process pa" +
        "rsing slow.\r\n");
            this.checkBoxResolveMethodNames.UseVisualStyleBackColor = true;
            // 
            // checkBoxParseStubMethods
            // 
            this.checkBoxParseStubMethods.AutoSize = true;
            this.checkBoxParseStubMethods.Location = new System.Drawing.Point(222, 26);
            this.checkBoxParseStubMethods.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.checkBoxParseStubMethods.Name = "checkBoxParseStubMethods";
            this.checkBoxParseStubMethods.Size = new System.Drawing.Size(122, 24);
            this.checkBoxParseStubMethods.TabIndex = 8;
            this.checkBoxParseStubMethods.Text = "Parse Stubs";
            this.toolTip.SetToolTip(this.checkBoxParseStubMethods, "Check to parse stub methods in each IPID entry.\r\n*WARNING* can make process parsi" +
        "ng slow.");
            this.checkBoxParseStubMethods.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(235, 306);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(112, 35);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(428, 306);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 35);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // checkBoxProxyParserResolveSymbols
            // 
            this.checkBoxProxyParserResolveSymbols.AutoSize = true;
            this.checkBoxProxyParserResolveSymbols.Location = new System.Drawing.Point(12, 26);
            this.checkBoxProxyParserResolveSymbols.Name = "checkBoxProxyParserResolveSymbols";
            this.checkBoxProxyParserResolveSymbols.Size = new System.Drawing.Size(156, 24);
            this.checkBoxProxyParserResolveSymbols.TabIndex = 0;
            this.checkBoxProxyParserResolveSymbols.Text = "Resolve Symbols";
            this.toolTip.SetToolTip(this.checkBoxProxyParserResolveSymbols, "Check to resolve symbols during proxy parsing which is useful for things like unk" +
        "nown user marshaller.\r\n*WARNING* can make proxy parsing slow.");
            this.checkBoxProxyParserResolveSymbols.UseVisualStyleBackColor = true;
            // 
            // groupBoxProxyParserConfig
            // 
            this.groupBoxProxyParserConfig.Controls.Add(this.checkBoxProxyParserResolveSymbols);
            this.groupBoxProxyParserConfig.Location = new System.Drawing.Point(12, 241);
            this.groupBoxProxyParserConfig.Name = "groupBoxProxyParserConfig";
            this.groupBoxProxyParserConfig.Size = new System.Drawing.Size(742, 57);
            this.groupBoxProxyParserConfig.TabIndex = 11;
            this.groupBoxProxyParserConfig.TabStop = false;
            this.groupBoxProxyParserConfig.Text = "Proxy Parser Configuration";
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(768, 357);
            this.Controls.Add(this.groupBoxProxyParserConfig);
            this.Controls.Add(groupBoxSymbols);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(groupBoxProcessParserConfig);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Settings";
            groupBoxSymbols.ResumeLayout(false);
            groupBoxSymbols.PerformLayout();
            groupBoxProcessParserConfig.ResumeLayout(false);
            groupBoxProcessParserConfig.PerformLayout();
            this.groupBoxProxyParserConfig.ResumeLayout(false);
            this.groupBoxProxyParserConfig.PerformLayout();
            this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.TextBox textBoxDbgHelp;
    private System.Windows.Forms.Button btnBrowseDbgHelpPath;
    private System.Windows.Forms.TextBox textBoxSymbolPath;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.CheckBox checkBoxParseStubMethods;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.CheckBox checkBoxResolveMethodNames;
    private System.Windows.Forms.GroupBox groupBoxProxyParserConfig;
    private System.Windows.Forms.CheckBox checkBoxProxyParserResolveSymbols;
    private System.Windows.Forms.CheckBox checkBoxParseRegisteredClasses;
    private System.Windows.Forms.CheckBox checkBoxParseActCtx;
}