namespace OleViewDotNet.Forms;

partial class COMRegistryViewer
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label labelFilter;
            System.Windows.Forms.Label labelMode;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(COMRegistryViewer));
            this.treeComRegistry = new System.Windows.Forms.TreeView();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGUIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGUIDCStructureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGUIDHexStringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyObjectTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createInstanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshInterfacesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createSpecialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createLocalServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createInProcServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createInProcHandlerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createRemoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createClassFactoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createClassFactoryRemoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createInSessionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.consoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createElevatedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.instanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.classFactoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createInRuntimeBrokerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createInPerUserRuntimeBrokerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createFactoryInRuntimeBrokerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createFactoryInPerUserRuntimeBrokerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewTypeLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewProxyLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewRuntimeInterfaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewLaunchPermissionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewAccessPermissionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.queryAllInterfacesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshProcessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unmarshalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toHexEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cloneTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allVisibleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filteredToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allChildrenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSourceCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeImageList = new System.Windows.Forms.ImageList(this.components);
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnApply = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.sourceCodeViewerControl = new OleViewDotNet.Forms.SourceCodeViewerControl();
            labelFilter = new System.Windows.Forms.Label();
            labelMode = new System.Windows.Forms.Label();
            this.contextMenuStrip.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelFilter
            // 
            labelFilter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelFilter.AutoSize = true;
            labelFilter.Location = new System.Drawing.Point(3, 8);
            labelFilter.Name = "labelFilter";
            labelFilter.Size = new System.Drawing.Size(52, 20);
            labelFilter.TabIndex = 1;
            labelFilter.Text = "Filter:";
            // 
            // labelMode
            // 
            labelMode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelMode.AutoSize = true;
            labelMode.Location = new System.Drawing.Point(1089, 8);
            labelMode.Name = "labelMode";
            labelMode.Size = new System.Drawing.Size(55, 20);
            labelMode.TabIndex = 5;
            labelMode.Text = "Mode:";
            // 
            // treeComRegistry
            // 
            this.treeComRegistry.ContextMenuStrip = this.contextMenuStrip;
            this.treeComRegistry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeComRegistry.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeComRegistry.HideSelection = false;
            this.treeComRegistry.ImageIndex = 0;
            this.treeComRegistry.ImageList = this.treeImageList;
            this.treeComRegistry.Location = new System.Drawing.Point(0, 0);
            this.treeComRegistry.Name = "treeComRegistry";
            this.treeComRegistry.SelectedImageIndex = 0;
            this.treeComRegistry.ShowNodeToolTips = true;
            this.treeComRegistry.Size = new System.Drawing.Size(692, 456);
            this.treeComRegistry.TabIndex = 0;
            this.treeComRegistry.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeComRegistry_AfterCollapse);
            this.treeComRegistry.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeComRegistry_BeforeExpand);
            this.treeComRegistry.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeComRegistry_AfterExpand);
            this.treeComRegistry.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeComRegistry_AfterSelect);
            this.treeComRegistry.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeComRegistry_MouseDown);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.copyGUIDToolStripMenuItem,
            this.copyGUIDCStructureToolStripMenuItem,
            this.copyGUIDHexStringToolStripMenuItem,
            this.copyObjectTagToolStripMenuItem,
            this.createInstanceToolStripMenuItem,
            this.refreshInterfacesToolStripMenuItem,
            this.createSpecialToolStripMenuItem,
            this.viewTypeLibraryToolStripMenuItem,
            this.viewProxyLibraryToolStripMenuItem,
            this.viewRuntimeInterfaceToolStripMenuItem,
            this.viewLaunchPermissionsToolStripMenuItem,
            this.viewAccessPermissionsToolStripMenuItem,
            this.queryAllInterfacesToolStripMenuItem,
            this.refreshProcessToolStripMenuItem,
            this.propertiesToolStripMenuItem,
            this.unmarshalToolStripMenuItem,
            this.cloneTreeToolStripMenuItem,
            this.showSourceCodeToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(280, 612);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // copyGUIDToolStripMenuItem
            // 
            this.copyGUIDToolStripMenuItem.Name = "copyGUIDToolStripMenuItem";
            this.copyGUIDToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.copyGUIDToolStripMenuItem.Text = "Copy GUID";
            this.copyGUIDToolStripMenuItem.Click += new System.EventHandler(this.copyGUIDToolStripMenuItem_Click);
            // 
            // copyGUIDCStructureToolStripMenuItem
            // 
            this.copyGUIDCStructureToolStripMenuItem.Name = "copyGUIDCStructureToolStripMenuItem";
            this.copyGUIDCStructureToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.copyGUIDCStructureToolStripMenuItem.Text = "Copy GUID C Structure";
            this.copyGUIDCStructureToolStripMenuItem.Click += new System.EventHandler(this.copyGUIDCStructureToolStripMenuItem_Click);
            // 
            // copyGUIDHexStringToolStripMenuItem
            // 
            this.copyGUIDHexStringToolStripMenuItem.Name = "copyGUIDHexStringToolStripMenuItem";
            this.copyGUIDHexStringToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.copyGUIDHexStringToolStripMenuItem.Text = "Copy GUID Hex String";
            this.copyGUIDHexStringToolStripMenuItem.Click += new System.EventHandler(this.copyGUIDHexStringToolStripMenuItem_Click);
            // 
            // copyObjectTagToolStripMenuItem
            // 
            this.copyObjectTagToolStripMenuItem.Name = "copyObjectTagToolStripMenuItem";
            this.copyObjectTagToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.copyObjectTagToolStripMenuItem.Text = "Copy Object Tag";
            this.copyObjectTagToolStripMenuItem.Click += new System.EventHandler(this.copyObjectTagToolStripMenuItem_Click);
            // 
            // createInstanceToolStripMenuItem
            // 
            this.createInstanceToolStripMenuItem.Name = "createInstanceToolStripMenuItem";
            this.createInstanceToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.createInstanceToolStripMenuItem.Text = "Create Instance";
            this.createInstanceToolStripMenuItem.Click += new System.EventHandler(this.createInstanceToolStripMenuItem_Click);
            // 
            // refreshInterfacesToolStripMenuItem
            // 
            this.refreshInterfacesToolStripMenuItem.Name = "refreshInterfacesToolStripMenuItem";
            this.refreshInterfacesToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.refreshInterfacesToolStripMenuItem.Text = "Refresh Interfaces";
            this.refreshInterfacesToolStripMenuItem.Click += new System.EventHandler(this.refreshInterfacesToolStripMenuItem_Click);
            // 
            // createSpecialToolStripMenuItem
            // 
            this.createSpecialToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createLocalServerToolStripMenuItem,
            this.createInProcServerToolStripMenuItem,
            this.createInProcHandlerToolStripMenuItem,
            this.createRemoteToolStripMenuItem,
            this.createClassFactoryToolStripMenuItem,
            this.createClassFactoryRemoteToolStripMenuItem,
            this.createInSessionToolStripMenuItem,
            this.createElevatedToolStripMenuItem,
            this.createInRuntimeBrokerToolStripMenuItem,
            this.createInPerUserRuntimeBrokerToolStripMenuItem,
            this.createFactoryInRuntimeBrokerToolStripMenuItem,
            this.createFactoryInPerUserRuntimeBrokerToolStripMenuItem});
            this.createSpecialToolStripMenuItem.Name = "createSpecialToolStripMenuItem";
            this.createSpecialToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.createSpecialToolStripMenuItem.Text = "Create Special";
            // 
            // createLocalServerToolStripMenuItem
            // 
            this.createLocalServerToolStripMenuItem.Name = "createLocalServerToolStripMenuItem";
            this.createLocalServerToolStripMenuItem.Size = new System.Drawing.Size(443, 34);
            this.createLocalServerToolStripMenuItem.Text = "Create Local Server";
            this.createLocalServerToolStripMenuItem.Click += new System.EventHandler(this.createLocalServerToolStripMenuItem_Click);
            // 
            // createInProcServerToolStripMenuItem
            // 
            this.createInProcServerToolStripMenuItem.Name = "createInProcServerToolStripMenuItem";
            this.createInProcServerToolStripMenuItem.Size = new System.Drawing.Size(443, 34);
            this.createInProcServerToolStripMenuItem.Text = "Create InProc Server";
            this.createInProcServerToolStripMenuItem.Click += new System.EventHandler(this.createInProcServerToolStripMenuItem_Click);
            // 
            // createInProcHandlerToolStripMenuItem
            // 
            this.createInProcHandlerToolStripMenuItem.Name = "createInProcHandlerToolStripMenuItem";
            this.createInProcHandlerToolStripMenuItem.Size = new System.Drawing.Size(443, 34);
            this.createInProcHandlerToolStripMenuItem.Text = "Create InProc Handler";
            this.createInProcHandlerToolStripMenuItem.Click += new System.EventHandler(this.createInProcHandlerToolStripMenuItem_Click);
            // 
            // createRemoteToolStripMenuItem
            // 
            this.createRemoteToolStripMenuItem.Name = "createRemoteToolStripMenuItem";
            this.createRemoteToolStripMenuItem.Size = new System.Drawing.Size(443, 34);
            this.createRemoteToolStripMenuItem.Text = "Create Remote";
            this.createRemoteToolStripMenuItem.Click += new System.EventHandler(this.createRemoteToolStripMenuItem_Click);
            // 
            // createClassFactoryToolStripMenuItem
            // 
            this.createClassFactoryToolStripMenuItem.Name = "createClassFactoryToolStripMenuItem";
            this.createClassFactoryToolStripMenuItem.Size = new System.Drawing.Size(443, 34);
            this.createClassFactoryToolStripMenuItem.Text = "Create Class Factory";
            this.createClassFactoryToolStripMenuItem.Click += new System.EventHandler(this.createClassFactoryToolStripMenuItem_Click);
            // 
            // createClassFactoryRemoteToolStripMenuItem
            // 
            this.createClassFactoryRemoteToolStripMenuItem.Name = "createClassFactoryRemoteToolStripMenuItem";
            this.createClassFactoryRemoteToolStripMenuItem.Size = new System.Drawing.Size(443, 34);
            this.createClassFactoryRemoteToolStripMenuItem.Text = "Create Class Factory Remote";
            this.createClassFactoryRemoteToolStripMenuItem.Click += new System.EventHandler(this.createClassFactoryRemoteToolStripMenuItem_Click);
            // 
            // createInSessionToolStripMenuItem
            // 
            this.createInSessionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.consoleToolStripMenuItem});
            this.createInSessionToolStripMenuItem.Name = "createInSessionToolStripMenuItem";
            this.createInSessionToolStripMenuItem.Size = new System.Drawing.Size(443, 34);
            this.createInSessionToolStripMenuItem.Text = "Create In Session";
            // 
            // consoleToolStripMenuItem
            // 
            this.consoleToolStripMenuItem.Name = "consoleToolStripMenuItem";
            this.consoleToolStripMenuItem.Size = new System.Drawing.Size(178, 34);
            this.consoleToolStripMenuItem.Tag = "Console";
            this.consoleToolStripMenuItem.Text = "Console";
            this.consoleToolStripMenuItem.Click += new System.EventHandler(this.consoleToolStripMenuItem_Click);
            // 
            // createElevatedToolStripMenuItem
            // 
            this.createElevatedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.instanceToolStripMenuItem,
            this.classFactoryToolStripMenuItem});
            this.createElevatedToolStripMenuItem.Name = "createElevatedToolStripMenuItem";
            this.createElevatedToolStripMenuItem.Size = new System.Drawing.Size(443, 34);
            this.createElevatedToolStripMenuItem.Text = "Create Elevated";
            // 
            // instanceToolStripMenuItem
            // 
            this.instanceToolStripMenuItem.Name = "instanceToolStripMenuItem";
            this.instanceToolStripMenuItem.Size = new System.Drawing.Size(216, 34);
            this.instanceToolStripMenuItem.Text = "Instance";
            this.instanceToolStripMenuItem.Click += new System.EventHandler(this.instanceToolStripMenuItem_Click);
            // 
            // classFactoryToolStripMenuItem
            // 
            this.classFactoryToolStripMenuItem.Name = "classFactoryToolStripMenuItem";
            this.classFactoryToolStripMenuItem.Size = new System.Drawing.Size(216, 34);
            this.classFactoryToolStripMenuItem.Text = "Class Factory";
            this.classFactoryToolStripMenuItem.Click += new System.EventHandler(this.classFactoryToolStripMenuItem_Click);
            // 
            // createInRuntimeBrokerToolStripMenuItem
            // 
            this.createInRuntimeBrokerToolStripMenuItem.Name = "createInRuntimeBrokerToolStripMenuItem";
            this.createInRuntimeBrokerToolStripMenuItem.Size = new System.Drawing.Size(443, 34);
            this.createInRuntimeBrokerToolStripMenuItem.Text = "Create In Runtime Broker";
            this.createInRuntimeBrokerToolStripMenuItem.Click += new System.EventHandler(this.createInRuntimeBrokerToolStripMenuItem_Click);
            // 
            // createInPerUserRuntimeBrokerToolStripMenuItem
            // 
            this.createInPerUserRuntimeBrokerToolStripMenuItem.Name = "createInPerUserRuntimeBrokerToolStripMenuItem";
            this.createInPerUserRuntimeBrokerToolStripMenuItem.Size = new System.Drawing.Size(443, 34);
            this.createInPerUserRuntimeBrokerToolStripMenuItem.Text = "Create In Per-User Runtime Broker";
            this.createInPerUserRuntimeBrokerToolStripMenuItem.Click += new System.EventHandler(this.createInPerUserRuntimeBrokerToolStripMenuItem_Click);
            // 
            // createFactoryInRuntimeBrokerToolStripMenuItem
            // 
            this.createFactoryInRuntimeBrokerToolStripMenuItem.Name = "createFactoryInRuntimeBrokerToolStripMenuItem";
            this.createFactoryInRuntimeBrokerToolStripMenuItem.Size = new System.Drawing.Size(443, 34);
            this.createFactoryInRuntimeBrokerToolStripMenuItem.Text = "Create Factory In Runtime Broker";
            this.createFactoryInRuntimeBrokerToolStripMenuItem.Click += new System.EventHandler(this.createFactoryInRuntimeBrokerToolStripMenuItem_Click);
            // 
            // createFactoryInPerUserRuntimeBrokerToolStripMenuItem
            // 
            this.createFactoryInPerUserRuntimeBrokerToolStripMenuItem.Name = "createFactoryInPerUserRuntimeBrokerToolStripMenuItem";
            this.createFactoryInPerUserRuntimeBrokerToolStripMenuItem.Size = new System.Drawing.Size(443, 34);
            this.createFactoryInPerUserRuntimeBrokerToolStripMenuItem.Text = "Create Factory in Per-User Runtime Broker";
            this.createFactoryInPerUserRuntimeBrokerToolStripMenuItem.Click += new System.EventHandler(this.createFactoryInPerUserRuntimeBrokerToolStripMenuItem_Click);
            // 
            // viewTypeLibraryToolStripMenuItem
            // 
            this.viewTypeLibraryToolStripMenuItem.Name = "viewTypeLibraryToolStripMenuItem";
            this.viewTypeLibraryToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.viewTypeLibraryToolStripMenuItem.Text = "View Type Library";
            this.viewTypeLibraryToolStripMenuItem.Click += new System.EventHandler(this.viewTypeLibraryToolStripMenuItem_Click);
            // 
            // viewProxyLibraryToolStripMenuItem
            // 
            this.viewProxyLibraryToolStripMenuItem.Name = "viewProxyLibraryToolStripMenuItem";
            this.viewProxyLibraryToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.viewProxyLibraryToolStripMenuItem.Text = "View Proxy Library";
            this.viewProxyLibraryToolStripMenuItem.Click += new System.EventHandler(this.viewProxyLibraryToolStripMenuItem_Click);
            // 
            // viewRuntimeInterfaceToolStripMenuItem
            // 
            this.viewRuntimeInterfaceToolStripMenuItem.Name = "viewRuntimeInterfaceToolStripMenuItem";
            this.viewRuntimeInterfaceToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.viewRuntimeInterfaceToolStripMenuItem.Text = "View Runtime Interface";
            this.viewRuntimeInterfaceToolStripMenuItem.Click += new System.EventHandler(this.viewRuntimeInterfaceToolStripMenuItem_Click);
            // 
            // viewLaunchPermissionsToolStripMenuItem
            // 
            this.viewLaunchPermissionsToolStripMenuItem.Name = "viewLaunchPermissionsToolStripMenuItem";
            this.viewLaunchPermissionsToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.viewLaunchPermissionsToolStripMenuItem.Text = "View Launch Permissions";
            this.viewLaunchPermissionsToolStripMenuItem.Click += new System.EventHandler(this.viewLaunchPermissionsToolStripMenuItem_Click);
            // 
            // viewAccessPermissionsToolStripMenuItem
            // 
            this.viewAccessPermissionsToolStripMenuItem.Name = "viewAccessPermissionsToolStripMenuItem";
            this.viewAccessPermissionsToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.viewAccessPermissionsToolStripMenuItem.Text = "View Access Permissions";
            this.viewAccessPermissionsToolStripMenuItem.Click += new System.EventHandler(this.viewAccessPermissionsToolStripMenuItem_Click);
            // 
            // queryAllInterfacesToolStripMenuItem
            // 
            this.queryAllInterfacesToolStripMenuItem.Name = "queryAllInterfacesToolStripMenuItem";
            this.queryAllInterfacesToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.queryAllInterfacesToolStripMenuItem.Text = "Query All Interfaces";
            this.queryAllInterfacesToolStripMenuItem.Click += new System.EventHandler(this.queryAllInterfacesToolStripMenuItem_Click);
            // 
            // refreshProcessToolStripMenuItem
            // 
            this.refreshProcessToolStripMenuItem.Name = "refreshProcessToolStripMenuItem";
            this.refreshProcessToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.refreshProcessToolStripMenuItem.Text = "Refresh Process";
            this.refreshProcessToolStripMenuItem.Click += new System.EventHandler(this.refreshProcessToolStripMenuItem_Click);
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.propertiesToolStripMenuItem.Text = "Properties";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
            // 
            // unmarshalToolStripMenuItem
            // 
            this.unmarshalToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toHexEditorToolStripMenuItem,
            this.toFileToolStripMenuItem,
            this.toObjectToolStripMenuItem});
            this.unmarshalToolStripMenuItem.Name = "unmarshalToolStripMenuItem";
            this.unmarshalToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.unmarshalToolStripMenuItem.Text = "Unmarshal";
            // 
            // toHexEditorToolStripMenuItem
            // 
            this.toHexEditorToolStripMenuItem.Name = "toHexEditorToolStripMenuItem";
            this.toHexEditorToolStripMenuItem.Size = new System.Drawing.Size(219, 34);
            this.toHexEditorToolStripMenuItem.Text = "To Hex Editor";
            this.toHexEditorToolStripMenuItem.Click += new System.EventHandler(this.toHexEditorToolStripMenuItem_Click);
            // 
            // toFileToolStripMenuItem
            // 
            this.toFileToolStripMenuItem.Name = "toFileToolStripMenuItem";
            this.toFileToolStripMenuItem.Size = new System.Drawing.Size(219, 34);
            this.toFileToolStripMenuItem.Text = "To File";
            this.toFileToolStripMenuItem.Click += new System.EventHandler(this.toFileToolStripMenuItem_Click);
            // 
            // toObjectToolStripMenuItem
            // 
            this.toObjectToolStripMenuItem.Name = "toObjectToolStripMenuItem";
            this.toObjectToolStripMenuItem.Size = new System.Drawing.Size(219, 34);
            this.toObjectToolStripMenuItem.Text = "To Object";
            this.toObjectToolStripMenuItem.Click += new System.EventHandler(this.toObjectToolStripMenuItem_Click);
            // 
            // cloneTreeToolStripMenuItem
            // 
            this.cloneTreeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allVisibleToolStripMenuItem,
            this.selectedToolStripMenuItem,
            this.filteredToolStripMenuItem,
            this.allChildrenToolStripMenuItem});
            this.cloneTreeToolStripMenuItem.Name = "cloneTreeToolStripMenuItem";
            this.cloneTreeToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.cloneTreeToolStripMenuItem.Text = "Clone Tree";
            // 
            // allVisibleToolStripMenuItem
            // 
            this.allVisibleToolStripMenuItem.Name = "allVisibleToolStripMenuItem";
            this.allVisibleToolStripMenuItem.Size = new System.Drawing.Size(204, 34);
            this.allVisibleToolStripMenuItem.Text = "All Visible";
            this.allVisibleToolStripMenuItem.Click += new System.EventHandler(this.allVisibleToolStripMenuItem_Click);
            // 
            // selectedToolStripMenuItem
            // 
            this.selectedToolStripMenuItem.Name = "selectedToolStripMenuItem";
            this.selectedToolStripMenuItem.Size = new System.Drawing.Size(204, 34);
            this.selectedToolStripMenuItem.Text = "Selected";
            this.selectedToolStripMenuItem.Click += new System.EventHandler(this.selectedToolStripMenuItem_Click);
            // 
            // filteredToolStripMenuItem
            // 
            this.filteredToolStripMenuItem.Name = "filteredToolStripMenuItem";
            this.filteredToolStripMenuItem.Size = new System.Drawing.Size(204, 34);
            this.filteredToolStripMenuItem.Text = "Filtered";
            this.filteredToolStripMenuItem.Click += new System.EventHandler(this.filteredToolStripMenuItem_Click);
            // 
            // allChildrenToolStripMenuItem
            // 
            this.allChildrenToolStripMenuItem.Name = "allChildrenToolStripMenuItem";
            this.allChildrenToolStripMenuItem.Size = new System.Drawing.Size(204, 34);
            this.allChildrenToolStripMenuItem.Text = "All Children";
            this.allChildrenToolStripMenuItem.Click += new System.EventHandler(this.allChildrenToolStripMenuItem_Click);
            // 
            // showSourceCodeToolStripMenuItem
            // 
            this.showSourceCodeToolStripMenuItem.Name = "showSourceCodeToolStripMenuItem";
            this.showSourceCodeToolStripMenuItem.Size = new System.Drawing.Size(279, 32);
            this.showSourceCodeToolStripMenuItem.Text = "Show Source Code";
            this.showSourceCodeToolStripMenuItem.Click += new System.EventHandler(this.showSourceCodeToolStripMenuItem_Click);
            // 
            // treeImageList
            // 
            this.treeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeImageList.ImageStream")));
            this.treeImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.treeImageList.Images.SetKeyName(0, "folder.ico");
            this.treeImageList.Images.SetKeyName(1, "class.ico");
            this.treeImageList.Images.SetKeyName(2, "interface.ico");
            this.treeImageList.Images.SetKeyName(3, "folderopen.ico");
            this.treeImageList.Images.SetKeyName(4, "process.ico");
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Location = new System.Drawing.Point(1150, 4);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(164, 28);
            this.comboBoxMode.TabIndex = 4;
            this.comboBoxMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxMode_SelectedIndexChanged);
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFilter.Location = new System.Drawing.Point(61, 5);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(1022, 26);
            this.textBoxFilter.TabIndex = 0;
            this.textBoxFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxFilter_KeyDown);
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 5;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Controls.Add(labelFilter, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.textBoxFilter, 1, 0);
            this.tableLayoutPanel.Controls.Add(labelMode, 2, 0);
            this.tableLayoutPanel.Controls.Add(this.comboBoxMode, 3, 0);
            this.tableLayoutPanel.Controls.Add(this.btnApply, 4, 0);
            this.tableLayoutPanel.Controls.Add(this.statusStrip, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.splitContainer, 0, 1);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 3;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(1400, 530);
            this.tableLayoutPanel.TabIndex = 6;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnApply.AutoSize = true;
            this.btnApply.Location = new System.Drawing.Point(1320, 3);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(77, 30);
            this.btnApply.TabIndex = 6;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // statusStrip
            // 
            this.tableLayoutPanel.SetColumnSpan(this.statusStrip, 5);
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelCount});
            this.statusStrip.Location = new System.Drawing.Point(0, 498);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1400, 32);
            this.statusStrip.TabIndex = 7;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabelCount
            // 
            this.toolStripStatusLabelCount.Name = "toolStripStatusLabelCount";
            this.toolStripStatusLabelCount.Size = new System.Drawing.Size(199, 25);
            this.toolStripStatusLabelCount.Text = "Showing N of M Entries";
            // 
            // splitContainer
            // 
            this.tableLayoutPanel.SetColumnSpan(this.splitContainer, 5);
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(3, 39);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.treeComRegistry);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.sourceCodeViewerControl);
            this.splitContainer.Size = new System.Drawing.Size(1394, 456);
            this.splitContainer.SplitterDistance = 692;
            this.splitContainer.TabIndex = 7;
            // 
            // sourceCodeViewerControl
            // 
            this.sourceCodeViewerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceCodeViewerControl.Location = new System.Drawing.Point(0, 0);
            this.sourceCodeViewerControl.Name = "sourceCodeViewerControl";
            this.sourceCodeViewerControl.Size = new System.Drawing.Size(698, 456);
            this.sourceCodeViewerControl.TabIndex = 0;
            // 
            // COMRegistryViewer
            // 
            this.Controls.Add(this.tableLayoutPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "COMRegistryViewer";
            this.Size = new System.Drawing.Size(1400, 530);
            this.Load += new System.EventHandler(this.COMRegistryViewer_Load);
            this.contextMenuStrip.ResumeLayout(false);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TreeView treeComRegistry;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
    private System.Windows.Forms.ToolStripMenuItem copyGUIDToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem copyGUIDCStructureToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem copyGUIDHexStringToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem copyObjectTagToolStripMenuItem;
    private System.Windows.Forms.ImageList treeImageList;
    private System.Windows.Forms.ToolStripMenuItem createInstanceToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem refreshInterfacesToolStripMenuItem;
    private System.Windows.Forms.TextBox textBoxFilter;
    private System.Windows.Forms.ComboBox comboBoxMode;
    private System.Windows.Forms.ToolStripMenuItem viewTypeLibraryToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem viewLaunchPermissionsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem viewAccessPermissionsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createSpecialToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createLocalServerToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createInProcServerToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createInSessionToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem consoleToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem viewProxyLibraryToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createClassFactoryToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem queryAllInterfacesToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createInProcHandlerToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createElevatedToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem instanceToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem classFactoryToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createRemoteToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createClassFactoryRemoteToolStripMenuItem;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.Button btnApply;
    private System.Windows.Forms.ToolStripMenuItem refreshProcessToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem unmarshalToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem toHexEditorToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem toFileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem toObjectToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem cloneTreeToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem allVisibleToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem selectedToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem filteredToolStripMenuItem;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelCount;
    private System.Windows.Forms.ToolStripMenuItem allChildrenToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem viewRuntimeInterfaceToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createInRuntimeBrokerToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createInPerUserRuntimeBrokerToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createFactoryInRuntimeBrokerToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem createFactoryInPerUserRuntimeBrokerToolStripMenuItem;
    private System.Windows.Forms.SplitContainer splitContainer;
    private SourceCodeViewerControl sourceCodeViewerControl;
    private System.Windows.Forms.ToolStripMenuItem showSourceCodeToolStripMenuItem;
}