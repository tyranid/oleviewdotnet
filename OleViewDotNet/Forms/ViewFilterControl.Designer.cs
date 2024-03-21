namespace OleViewDotNet.Forms;

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
        this.components = new System.ComponentModel.Container();
        System.Windows.Forms.Label label1;
        System.Windows.Forms.Label label2;
        System.Windows.Forms.Label label3;
        System.Windows.Forms.Label label4;
        System.Windows.Forms.Label label5;
        System.Windows.Forms.ColumnHeader columnHeaderType;
        System.Windows.Forms.ColumnHeader columnHeaderField;
        System.Windows.Forms.ColumnHeader columnHeaderComparison;
        System.Windows.Forms.ColumnHeader columnHeaderValue;
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewFilterControl));
        this.comboBoxFilterType = new System.Windows.Forms.ComboBox();
        this.comboBoxFilterComparison = new System.Windows.Forms.ComboBox();
        this.comboBoxField = new System.Windows.Forms.ComboBox();
        this.comboBoxValue = new System.Windows.Forms.ComboBox();
        this.listViewFilters = new System.Windows.Forms.ListView();
        this.imageList = new System.Windows.Forms.ImageList(this.components);
        this.btnReset = new System.Windows.Forms.Button();
        this.btnAdd = new System.Windows.Forms.Button();
        this.btnRemove = new System.Windows.Forms.Button();
        this.comboBoxDecision = new System.Windows.Forms.ComboBox();
        label1 = new System.Windows.Forms.Label();
        label2 = new System.Windows.Forms.Label();
        label3 = new System.Windows.Forms.Label();
        label4 = new System.Windows.Forms.Label();
        label5 = new System.Windows.Forms.Label();
        columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        columnHeaderField = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        columnHeaderComparison = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        columnHeaderValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.SuspendLayout();
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(25, 1);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(31, 13);
        label1.TabIndex = 5;
        label1.Text = "Type";
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(142, 0);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(29, 13);
        label2.TabIndex = 6;
        label2.Text = "Field";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new System.Drawing.Point(242, 0);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(62, 13);
        label3.TabIndex = 7;
        label3.Text = "Comparison";
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new System.Drawing.Point(412, 1);
        label4.Name = "label4";
        label4.Size = new System.Drawing.Size(34, 13);
        label4.TabIndex = 9;
        label4.Text = "Value";
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Location = new System.Drawing.Point(562, 1);
        label5.Name = "label5";
        label5.Size = new System.Drawing.Size(48, 13);
        label5.TabIndex = 15;
        label5.Text = "Decision";
        // 
        // columnHeaderType
        // 
        columnHeaderType.Text = "Type";
        // 
        // columnHeaderField
        // 
        columnHeaderField.Text = "Field";
        // 
        // columnHeaderComparison
        // 
        columnHeaderComparison.Text = "Comparison";
        // 
        // columnHeaderValue
        // 
        columnHeaderValue.Text = "Value";
        // 
        // comboBoxFilterType
        // 
        this.comboBoxFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboBoxFilterType.FormattingEnabled = true;
        this.comboBoxFilterType.Location = new System.Drawing.Point(0, 17);
        this.comboBoxFilterType.Name = "comboBoxFilterType";
        this.comboBoxFilterType.Size = new System.Drawing.Size(89, 21);
        this.comboBoxFilterType.TabIndex = 1;
        this.comboBoxFilterType.SelectedIndexChanged += new System.EventHandler(this.comboBoxFilterType_SelectedIndexChanged);
        // 
        // comboBoxFilterComparison
        // 
        this.comboBoxFilterComparison.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboBoxFilterComparison.FormattingEnabled = true;
        this.comboBoxFilterComparison.Location = new System.Drawing.Point(228, 17);
        this.comboBoxFilterComparison.Name = "comboBoxFilterComparison";
        this.comboBoxFilterComparison.Size = new System.Drawing.Size(89, 21);
        this.comboBoxFilterComparison.TabIndex = 3;
        // 
        // comboBoxField
        // 
        this.comboBoxField.DisplayMember = "Name";
        this.comboBoxField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboBoxField.FormattingEnabled = true;
        this.comboBoxField.Location = new System.Drawing.Point(95, 17);
        this.comboBoxField.Name = "comboBoxField";
        this.comboBoxField.Size = new System.Drawing.Size(127, 21);
        this.comboBoxField.TabIndex = 4;
        this.comboBoxField.ValueMember = "Name";
        this.comboBoxField.SelectedIndexChanged += new System.EventHandler(this.comboBoxField_SelectedIndexChanged);
        // 
        // comboBoxValue
        // 
        this.comboBoxValue.FormattingEnabled = true;
        this.comboBoxValue.Location = new System.Drawing.Point(323, 17);
        this.comboBoxValue.Name = "comboBoxValue";
        this.comboBoxValue.Size = new System.Drawing.Size(218, 21);
        this.comboBoxValue.TabIndex = 8;
        // 
        // listViewFilters
        // 
        this.listViewFilters.CheckBoxes = true;
        this.listViewFilters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        columnHeaderType,
        columnHeaderField,
        columnHeaderComparison,
        columnHeaderValue});
        this.listViewFilters.FullRowSelect = true;
        this.listViewFilters.Location = new System.Drawing.Point(0, 73);
        this.listViewFilters.MultiSelect = false;
        this.listViewFilters.Name = "listViewFilters";
        this.listViewFilters.Size = new System.Drawing.Size(628, 140);
        this.listViewFilters.SmallImageList = this.imageList;
        this.listViewFilters.TabIndex = 10;
        this.listViewFilters.UseCompatibleStateImageBehavior = false;
        this.listViewFilters.View = System.Windows.Forms.View.Details;
        this.listViewFilters.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewFilters_ItemChecked);
        this.listViewFilters.SelectedIndexChanged += new System.EventHandler(this.listViewFilters_SelectedIndexChanged);
        // 
        // imageList
        // 
        this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
        this.imageList.TransparentColor = System.Drawing.Color.Transparent;
        this.imageList.Images.SetKeyName(0, "exclude_filter.png");
        this.imageList.Images.SetKeyName(1, "include_filter.png");
        // 
        // btnReset
        // 
        this.btnReset.Location = new System.Drawing.Point(0, 44);
        this.btnReset.Name = "btnReset";
        this.btnReset.Size = new System.Drawing.Size(75, 23);
        this.btnReset.TabIndex = 11;
        this.btnReset.Text = "Reset";
        this.btnReset.UseVisualStyleBackColor = true;
        this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
        // 
        // btnAdd
        // 
        this.btnAdd.Location = new System.Drawing.Point(472, 44);
        this.btnAdd.Name = "btnAdd";
        this.btnAdd.Size = new System.Drawing.Size(75, 23);
        this.btnAdd.TabIndex = 12;
        this.btnAdd.Text = "Add";
        this.btnAdd.UseVisualStyleBackColor = true;
        this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
        // 
        // btnRemove
        // 
        this.btnRemove.Location = new System.Drawing.Point(553, 44);
        this.btnRemove.Name = "btnRemove";
        this.btnRemove.Size = new System.Drawing.Size(75, 23);
        this.btnRemove.TabIndex = 13;
        this.btnRemove.Text = "Remove";
        this.btnRemove.UseVisualStyleBackColor = true;
        this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
        // 
        // comboBoxDecision
        // 
        this.comboBoxDecision.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.comboBoxDecision.FormattingEnabled = true;
        this.comboBoxDecision.Location = new System.Drawing.Point(549, 17);
        this.comboBoxDecision.Name = "comboBoxDecision";
        this.comboBoxDecision.Size = new System.Drawing.Size(79, 21);
        this.comboBoxDecision.TabIndex = 14;
        // 
        // ViewFilterControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(label5);
        this.Controls.Add(this.comboBoxDecision);
        this.Controls.Add(this.btnRemove);
        this.Controls.Add(this.btnAdd);
        this.Controls.Add(this.btnReset);
        this.Controls.Add(this.listViewFilters);
        this.Controls.Add(label4);
        this.Controls.Add(this.comboBoxValue);
        this.Controls.Add(label3);
        this.Controls.Add(label2);
        this.Controls.Add(label1);
        this.Controls.Add(this.comboBoxField);
        this.Controls.Add(this.comboBoxFilterComparison);
        this.Controls.Add(this.comboBoxFilterType);
        this.Name = "ViewFilterControl";
        this.Size = new System.Drawing.Size(631, 215);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.ComboBox comboBoxFilterType;
    private System.Windows.Forms.ComboBox comboBoxFilterComparison;
    private System.Windows.Forms.ComboBox comboBoxField;
    private System.Windows.Forms.ComboBox comboBoxValue;
    private System.Windows.Forms.ListView listViewFilters;
    private System.Windows.Forms.Button btnReset;
    private System.Windows.Forms.Button btnAdd;
    private System.Windows.Forms.Button btnRemove;
    private System.Windows.Forms.ComboBox comboBoxDecision;
    private System.Windows.Forms.ImageList imageList;
}
