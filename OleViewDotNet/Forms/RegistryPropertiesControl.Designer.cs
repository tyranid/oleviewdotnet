namespace OleViewDotNet.Forms;

partial class RegistryPropertiesControl
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
        System.Windows.Forms.ColumnHeader columnProperty;
        this.listViewProperties = new System.Windows.Forms.ListView();
        this.columnValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        columnProperty = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.SuspendLayout();
        // 
        // columnProperty
        // 
        columnProperty.Text = "Property";
        // 
        // listViewProperties
        // 
        this.listViewProperties.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        columnProperty,
        this.columnValue});
        this.listViewProperties.Dock = System.Windows.Forms.DockStyle.Fill;
        this.listViewProperties.FullRowSelect = true;
        this.listViewProperties.Location = new System.Drawing.Point(0, 0);
        this.listViewProperties.MultiSelect = false;
        this.listViewProperties.Name = "listViewProperties";
        this.listViewProperties.Size = new System.Drawing.Size(437, 252);
        this.listViewProperties.TabIndex = 1;
        this.listViewProperties.UseCompatibleStateImageBehavior = false;
        this.listViewProperties.View = System.Windows.Forms.View.Details;
        // 
        // columnValue
        // 
        this.columnValue.Text = "Value";
        // 
        // RegistryPropertiesControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.listViewProperties);
        this.Name = "RegistryPropertiesControl";
        this.Size = new System.Drawing.Size(437, 252);
        this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.ListView listViewProperties;
    private System.Windows.Forms.ColumnHeader columnValue;
}