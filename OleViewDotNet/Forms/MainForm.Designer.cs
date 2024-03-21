namespace OleViewDotNet.Forms;

partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.menuFile = new System.Windows.Forms.MenuItem();
            this.menuFileOpen32BitViewer = new System.Windows.Forms.MenuItem();
            this.menuFileOpen64BitViewer = new System.Windows.Forms.MenuItem();
            this.menuFileOpenARM64Viewer = new System.Windows.Forms.MenuItem();
            this.menuFileOpenAsAdmin = new System.Windows.Forms.MenuItem();
            this.menuFileOpenPowershell = new System.Windows.Forms.MenuItem();
            this.menuItem10 = new System.Windows.Forms.MenuItem();
            this.menuFileOpenDatabase = new System.Windows.Forms.MenuItem();
            this.menuFileSaveDatabase = new System.Windows.Forms.MenuItem();
            this.menuFileSaveAsDatabase = new System.Windows.Forms.MenuItem();
            this.menuItem14 = new System.Windows.Forms.MenuItem();
            this.menuFileSaveDefaultDatabase = new System.Windows.Forms.MenuItem();
            this.menuFileDeleteDefaultDatabase = new System.Windows.Forms.MenuItem();
            this.menuFileSaveDatabaseOnExit = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuFileOpenMachineOnly = new System.Windows.Forms.MenuItem();
            this.menuFileOpenUserOnly = new System.Windows.Forms.MenuItem();
            this.menuFileDiff = new System.Windows.Forms.MenuItem();
            this.menuFileQueryAllInterfaces = new System.Windows.Forms.MenuItem();
            this.menuFileExportInterfaceList = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.menuFileOpenTypeLib = new System.Windows.Forms.MenuItem();
            this.menuFileOpenProxyDll = new System.Windows.Forms.MenuItem();
            this.menuItem12 = new System.Windows.Forms.MenuItem();
            this.menuFileSettings = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuFileExit = new System.Windows.Forms.MenuItem();
            this.menuRegistry = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDs = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDsByName = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDsByServer = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDsByLocalServer = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDsWithSurrogate = new System.Windows.Forms.MenuItem();
            this.menuViewProgIDs = new System.Windows.Forms.MenuItem();
            this.menuRegistryMimeTypes = new System.Windows.Forms.MenuItem();
            this.menuRegistryRuntimeClasses = new System.Windows.Forms.MenuItem();
            this.menuRegistryRuntimeServers = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuViewPreApproved = new System.Windows.Forms.MenuItem();
            this.menuViewImplementedCategories = new System.Windows.Forms.MenuItem();
            this.menuViewIELowRights = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuViewAppIDs = new System.Windows.Forms.MenuItem();
            this.menuRegistryAppIDsIL = new System.Windows.Forms.MenuItem();
            this.menuRegistryAppIDsWithAC = new System.Windows.Forms.MenuItem();
            this.menuViewLocalServices = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuViewInterfaces = new System.Windows.Forms.MenuItem();
            this.menuViewInterfacesByName = new System.Windows.Forms.MenuItem();
            this.menuViewRuntimeInterfaces = new System.Windows.Forms.MenuItem();
            this.menuViewRuntimeInterfacesTree = new System.Windows.Forms.MenuItem();
            this.menuRegistryTypeLibraries = new System.Windows.Forms.MenuItem();
            this.menuRegistryInterfaceProxies = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuRegistryProperties = new System.Windows.Forms.MenuItem();
            this.menuObject = new System.Windows.Forms.MenuItem();
            this.menuObjectROT = new System.Windows.Forms.MenuItem();
            this.menuItem13 = new System.Windows.Forms.MenuItem();
            this.menuObjectCreateInstanceFromCLSID = new System.Windows.Forms.MenuItem();
            this.menuObjectFromMarshalledStream = new System.Windows.Forms.MenuItem();
            this.menuObjectFromSerializedStream = new System.Windows.Forms.MenuItem();
            this.menuObjectFromFile = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuObjectParseMoniker = new System.Windows.Forms.MenuItem();
            this.menuObjectBindMoniker = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.menuHexEditor = new System.Windows.Forms.MenuItem();
            this.menuHexEditorFromFile = new System.Windows.Forms.MenuItem();
            this.menuHexEditorEmpty = new System.Windows.Forms.MenuItem();
            this.menuSecurity = new System.Windows.Forms.MenuItem();
            this.menuSecurityDefaultAccess = new System.Windows.Forms.MenuItem();
            this.menuSecurityDefaultAccessRestriction = new System.Windows.Forms.MenuItem();
            this.menuSecurityDefaultLaunch = new System.Windows.Forms.MenuItem();
            this.menuSecurityDefaultLaunchRestriction = new System.Windows.Forms.MenuItem();
            this.menuProcesses = new System.Windows.Forms.MenuItem();
            this.menuProcessesSelectProcess = new System.Windows.Forms.MenuItem();
            this.menuItemProcessesAllProcesses = new System.Windows.Forms.MenuItem();
            this.menuProcessesAllProcessesByPid = new System.Windows.Forms.MenuItem();
            this.menuProcessesAllProcessesByName = new System.Windows.Forms.MenuItem();
            this.menuProcessesAllProcessesByUser = new System.Windows.Forms.MenuItem();
            this.menuItem16 = new System.Windows.Forms.MenuItem();
            this.menuProcessesOptions = new System.Windows.Forms.MenuItem();
            this.menuProcessesOptionsResolveMethodNames = new System.Windows.Forms.MenuItem();
            this.menuProcessesOptionsParseStubs = new System.Windows.Forms.MenuItem();
            this.menuProcessesOptionsParseRegisteredClasses = new System.Windows.Forms.MenuItem();
            this.menuProcessesOptionsParseActCtx = new System.Windows.Forms.MenuItem();
            this.menuItemStorage = new System.Windows.Forms.MenuItem();
            this.menuStorageNewStorage = new System.Windows.Forms.MenuItem();
            this.menuStorageOpenStorage = new System.Windows.Forms.MenuItem();
            this.menuItemView = new System.Windows.Forms.MenuItem();
            this.menuViewOpenPropertiesViewer = new System.Windows.Forms.MenuItem();
            this.menuViewRegistryViewOptions = new System.Windows.Forms.MenuItem();
            this.menuViewAlwaysShowSourceCode = new System.Windows.Forms.MenuItem();
            this.menuViewEnableAutoParsing = new System.Windows.Forms.MenuItem();
            this.menuHelp = new System.Windows.Forms.MenuItem();
            this.menuHelpAbout = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuFile,
            this.menuRegistry,
            this.menuObject,
            this.menuSecurity,
            this.menuProcesses,
            this.menuItemStorage,
            this.menuItemView,
            this.menuHelp});
            // 
            // menuFile
            // 
            this.menuFile.Index = 0;
            this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuFileOpen32BitViewer,
            this.menuFileOpen64BitViewer,
            this.menuFileOpenARM64Viewer,
            this.menuFileOpenAsAdmin,
            this.menuFileOpenPowershell,
            this.menuItem10,
            this.menuFileOpenDatabase,
            this.menuFileSaveDatabase,
            this.menuFileSaveAsDatabase,
            this.menuItem14,
            this.menuFileSaveDefaultDatabase,
            this.menuFileDeleteDefaultDatabase,
            this.menuFileSaveDatabaseOnExit,
            this.menuItem7,
            this.menuFileOpenMachineOnly,
            this.menuFileOpenUserOnly,
            this.menuFileDiff,
            this.menuFileQueryAllInterfaces,
            this.menuFileExportInterfaceList,
            this.menuItem8,
            this.menuFileOpenTypeLib,
            this.menuFileOpenProxyDll,
            this.menuItem12,
            this.menuFileSettings,
            this.menuItem2,
            this.menuFileExit});
            this.menuFile.Text = "&File";
            this.menuFile.Popup += new System.EventHandler(this.menuFile_Popup);
            // 
            // menuFileOpen32BitViewer
            // 
            this.menuFileOpen32BitViewer.Index = 0;
            this.menuFileOpen32BitViewer.Text = "Open &32 Bit Viewer";
            this.menuFileOpen32BitViewer.Click += new System.EventHandler(this.menuFileOpen32BitViewer_Click);
            // 
            // menuFileOpen64BitViewer
            // 
            this.menuFileOpen64BitViewer.Index = 1;
            this.menuFileOpen64BitViewer.Text = "Open &64 Bit Viewer";
            this.menuFileOpen64BitViewer.Click += new System.EventHandler(this.menuFileOpen64BitViewer_Click);
            // 
            // menuFileOpenARM64Viewer
            // 
            this.menuFileOpenARM64Viewer.Index = 2;
            this.menuFileOpenARM64Viewer.Text = "Open &ARM64 Viewer";
            this.menuFileOpenARM64Viewer.Click += new System.EventHandler(this.menuFileOpenARM64Viewer_Click);
            // 
            // menuFileOpenAsAdmin
            // 
            this.menuFileOpenAsAdmin.Index = 3;
            this.menuFileOpenAsAdmin.Text = "Open as Administrator";
            this.menuFileOpenAsAdmin.Click += new System.EventHandler(this.menuFileOpenAsAdmin_Click);
            // 
            // menuFileOpenPowershell
            // 
            this.menuFileOpenPowershell.Index = 4;
            this.menuFileOpenPowershell.Text = "Open Powershell";
            this.menuFileOpenPowershell.Click += new System.EventHandler(this.menuFileOpenPowershell_Click);
            // 
            // menuItem10
            // 
            this.menuItem10.Index = 5;
            this.menuItem10.Text = "-";
            // 
            // menuFileOpenDatabase
            // 
            this.menuFileOpenDatabase.Index = 6;
            this.menuFileOpenDatabase.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.menuFileOpenDatabase.Text = "&Open Database";
            this.menuFileOpenDatabase.Click += new System.EventHandler(this.menuFileOpenDatabase_Click);
            // 
            // menuFileSaveDatabase
            // 
            this.menuFileSaveDatabase.Index = 7;
            this.menuFileSaveDatabase.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.menuFileSaveDatabase.Text = "&Save Database";
            this.menuFileSaveDatabase.Click += new System.EventHandler(this.menuFileSaveDatabase_Click);
            // 
            // menuFileSaveAsDatabase
            // 
            this.menuFileSaveAsDatabase.Index = 8;
            this.menuFileSaveAsDatabase.Text = "Save &as... Database";
            this.menuFileSaveAsDatabase.Click += new System.EventHandler(this.menuFileSaveAsDatabase_Click);
            // 
            // menuItem14
            // 
            this.menuItem14.Index = 9;
            this.menuItem14.Text = "-";
            // 
            // menuFileSaveDefaultDatabase
            // 
            this.menuFileSaveDefaultDatabase.Index = 10;
            this.menuFileSaveDefaultDatabase.Text = "Save De&fault Database";
            this.menuFileSaveDefaultDatabase.Click += new System.EventHandler(this.menuFileSaveDefaultDatabase_Click);
            // 
            // menuFileDeleteDefaultDatabase
            // 
            this.menuFileDeleteDefaultDatabase.Index = 11;
            this.menuFileDeleteDefaultDatabase.Text = "Delete Default Database";
            this.menuFileDeleteDefaultDatabase.Click += new System.EventHandler(this.menuFileDeleteDefaultDatabase_Click);
            // 
            // menuFileSaveDatabaseOnExit
            // 
            this.menuFileSaveDatabaseOnExit.Index = 12;
            this.menuFileSaveDatabaseOnExit.Text = "Save Default Database on Exit";
            this.menuFileSaveDatabaseOnExit.Click += new System.EventHandler(this.menuFileSaveDatabaseOnExit_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 13;
            this.menuItem7.Text = "-";
            // 
            // menuFileOpenMachineOnly
            // 
            this.menuFileOpenMachineOnly.Index = 14;
            this.menuFileOpenMachineOnly.Text = "Open &Machine Only";
            this.menuFileOpenMachineOnly.Click += new System.EventHandler(this.menuFileOpenMachineOnly_Click);
            // 
            // menuFileOpenUserOnly
            // 
            this.menuFileOpenUserOnly.Index = 15;
            this.menuFileOpenUserOnly.Text = "Open &User Only";
            this.menuFileOpenUserOnly.Click += new System.EventHandler(this.menuFileOpenUserOnly_Click);
            // 
            // menuFileDiff
            // 
            this.menuFileDiff.Index = 16;
            this.menuFileDiff.Text = "&Diff Registries";
            this.menuFileDiff.Click += new System.EventHandler(this.menuFileDiff_Click);
            // 
            // menuFileQueryAllInterfaces
            // 
            this.menuFileQueryAllInterfaces.Index = 17;
            this.menuFileQueryAllInterfaces.Text = "&Query All Interfaces";
            this.menuFileQueryAllInterfaces.Click += new System.EventHandler(this.menuFileQueryAllInterfaces_Click);
            // 
            // menuFileExportInterfaceList
            // 
            this.menuFileExportInterfaceList.Index = 18;
            this.menuFileExportInterfaceList.Text = "Export Interface List";
            this.menuFileExportInterfaceList.Click += new System.EventHandler(this.menuFileExportInterfaceList_Click);
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 19;
            this.menuItem8.Text = "-";
            // 
            // menuFileOpenTypeLib
            // 
            this.menuFileOpenTypeLib.Index = 20;
            this.menuFileOpenTypeLib.Text = "Open &TypeLib";
            this.menuFileOpenTypeLib.Click += new System.EventHandler(this.menuFileOpenTypeLib_Click);
            // 
            // menuFileOpenProxyDll
            // 
            this.menuFileOpenProxyDll.Index = 21;
            this.menuFileOpenProxyDll.Text = "Open Proxy D&LL";
            this.menuFileOpenProxyDll.Click += new System.EventHandler(this.menuFileOpenProxyDll_Click);
            // 
            // menuItem12
            // 
            this.menuItem12.Index = 22;
            this.menuItem12.Text = "-";
            // 
            // menuFileSettings
            // 
            this.menuFileSettings.Index = 23;
            this.menuFileSettings.Text = "Settin&gs";
            this.menuFileSettings.Click += new System.EventHandler(this.menuFileSettings_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 24;
            this.menuItem2.Text = "-";
            // 
            // menuFileExit
            // 
            this.menuFileExit.Index = 25;
            this.menuFileExit.Shortcut = System.Windows.Forms.Shortcut.AltF4;
            this.menuFileExit.Text = "E&xit";
            this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
            // 
            // menuRegistry
            // 
            this.menuRegistry.Index = 1;
            this.menuRegistry.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuViewCLSIDs,
            this.menuViewCLSIDsByName,
            this.menuViewCLSIDsByServer,
            this.menuViewCLSIDsByLocalServer,
            this.menuViewCLSIDsWithSurrogate,
            this.menuViewProgIDs,
            this.menuRegistryMimeTypes,
            this.menuRegistryRuntimeClasses,
            this.menuRegistryRuntimeServers,
            this.menuItem4,
            this.menuViewPreApproved,
            this.menuViewImplementedCategories,
            this.menuViewIELowRights,
            this.menuItem5,
            this.menuViewAppIDs,
            this.menuRegistryAppIDsIL,
            this.menuRegistryAppIDsWithAC,
            this.menuViewLocalServices,
            this.menuItem1,
            this.menuViewInterfaces,
            this.menuViewInterfacesByName,
            this.menuViewRuntimeInterfaces,
            this.menuViewRuntimeInterfacesTree,
            this.menuRegistryTypeLibraries,
            this.menuRegistryInterfaceProxies,
            this.menuItem6,
            this.menuRegistryProperties});
            this.menuRegistry.Text = "&Registry";
            // 
            // menuViewCLSIDs
            // 
            this.menuViewCLSIDs.Index = 0;
            this.menuViewCLSIDs.Text = "&CLSIDs";
            this.menuViewCLSIDs.Click += new System.EventHandler(this.menuViewCLSIDs_Click);
            // 
            // menuViewCLSIDsByName
            // 
            this.menuViewCLSIDsByName.Index = 1;
            this.menuViewCLSIDsByName.Text = "CLSIDs By &Name";
            this.menuViewCLSIDsByName.Click += new System.EventHandler(this.menuViewCLSIDsByName_Click);
            // 
            // menuViewCLSIDsByServer
            // 
            this.menuViewCLSIDsByServer.Index = 2;
            this.menuViewCLSIDsByServer.Text = "CLSIDs By &Server";
            this.menuViewCLSIDsByServer.Click += new System.EventHandler(this.menuViewCLSIDsByServer_Click);
            // 
            // menuViewCLSIDsByLocalServer
            // 
            this.menuViewCLSIDsByLocalServer.Index = 3;
            this.menuViewCLSIDsByLocalServer.Text = "CLSIDs By &Local Server";
            this.menuViewCLSIDsByLocalServer.Click += new System.EventHandler(this.menuViewCLSIDsByLocalServer_Click);
            // 
            // menuViewCLSIDsWithSurrogate
            // 
            this.menuViewCLSIDsWithSurrogate.Index = 4;
            this.menuViewCLSIDsWithSurrogate.Text = "CLSIDs with &DLL Surrogate";
            this.menuViewCLSIDsWithSurrogate.Click += new System.EventHandler(this.menuViewCLSIDsWithSurrogate_Click);
            // 
            // menuViewProgIDs
            // 
            this.menuViewProgIDs.Index = 5;
            this.menuViewProgIDs.Text = "&Prog IDs";
            this.menuViewProgIDs.Click += new System.EventHandler(this.menuViewProgIDs_Click);
            // 
            // menuRegistryMimeTypes
            // 
            this.menuRegistryMimeTypes.Index = 6;
            this.menuRegistryMimeTypes.Text = "&MIME Types";
            this.menuRegistryMimeTypes.Click += new System.EventHandler(this.menuRegistryMimeTypes_Click);
            // 
            // menuRegistryRuntimeClasses
            // 
            this.menuRegistryRuntimeClasses.Index = 7;
            this.menuRegistryRuntimeClasses.Text = "Runtime Classes";
            this.menuRegistryRuntimeClasses.Click += new System.EventHandler(this.menuRegistryRuntimeClasses_Click);
            // 
            // menuRegistryRuntimeServers
            // 
            this.menuRegistryRuntimeServers.Index = 8;
            this.menuRegistryRuntimeServers.Text = "Runtime Servers";
            this.menuRegistryRuntimeServers.Click += new System.EventHandler(this.menuRegistryRuntimeServers_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 9;
            this.menuItem4.Text = "-";
            // 
            // menuViewPreApproved
            // 
            this.menuViewPreApproved.Index = 10;
            this.menuViewPreApproved.Text = "&Explorer PreApproved";
            this.menuViewPreApproved.Click += new System.EventHandler(this.menuViewPreApproved_Click);
            // 
            // menuViewImplementedCategories
            // 
            this.menuViewImplementedCategories.Index = 11;
            this.menuViewImplementedCategories.Text = "&Implemented Categories";
            this.menuViewImplementedCategories.Click += new System.EventHandler(this.menuViewImplementedCategories_Click);
            // 
            // menuViewIELowRights
            // 
            this.menuViewIELowRights.Index = 12;
            this.menuViewIELowRights.Text = "IE Low &Rights Elevation Policy";
            this.menuViewIELowRights.Click += new System.EventHandler(this.menuViewIELowRights_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 13;
            this.menuItem5.Text = "-";
            // 
            // menuViewAppIDs
            // 
            this.menuViewAppIDs.Index = 14;
            this.menuViewAppIDs.Text = "&App IDs";
            this.menuViewAppIDs.Click += new System.EventHandler(this.menuViewAppIDs_Click);
            // 
            // menuRegistryAppIDsIL
            // 
            this.menuRegistryAppIDsIL.Index = 15;
            this.menuRegistryAppIDsIL.Text = "App IDs &With IL";
            this.menuRegistryAppIDsIL.Click += new System.EventHandler(this.menuRegistryAppIDsIL_Click);
            // 
            // menuRegistryAppIDsWithAC
            // 
            this.menuRegistryAppIDsWithAC.Index = 16;
            this.menuRegistryAppIDsWithAC.Text = "App IDs with AppContai&ner";
            this.menuRegistryAppIDsWithAC.Click += new System.EventHandler(this.menuRegistryAppIDsWithAC_Click);
            // 
            // menuViewLocalServices
            // 
            this.menuViewLocalServices.Index = 17;
            this.menuViewLocalServices.Text = "L&ocal Services";
            this.menuViewLocalServices.Click += new System.EventHandler(this.menuViewLocalServices_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 18;
            this.menuItem1.Text = "-";
            // 
            // menuViewInterfaces
            // 
            this.menuViewInterfaces.Index = 19;
            this.menuViewInterfaces.Text = "Inter&faces";
            this.menuViewInterfaces.Click += new System.EventHandler(this.menuViewInterfaces_Click);
            // 
            // menuViewInterfacesByName
            // 
            this.menuViewInterfacesByName.Index = 20;
            this.menuViewInterfacesByName.Text = "Interfaces By Name";
            this.menuViewInterfacesByName.Click += new System.EventHandler(this.menuViewInterfacesByName_Click);
            // 
            // menuViewRuntimeInterfaces
            // 
            this.menuViewRuntimeInterfaces.Index = 21;
            this.menuViewRuntimeInterfaces.Text = "Runtime Interfaces";
            this.menuViewRuntimeInterfaces.Click += new System.EventHandler(this.menuViewRuntimeInterfaces_Click);
            // 
            // menuViewRuntimeInterfacesTree
            // 
            this.menuViewRuntimeInterfacesTree.Index = 22;
            this.menuViewRuntimeInterfacesTree.Text = "Runtime Interfaces Tree";
            this.menuViewRuntimeInterfacesTree.Click += new System.EventHandler(this.menuViewRuntimeInterfacesTree_Click);
            // 
            // menuRegistryTypeLibraries
            // 
            this.menuRegistryTypeLibraries.Index = 23;
            this.menuRegistryTypeLibraries.Text = "&Type Libraries";
            this.menuRegistryTypeLibraries.Click += new System.EventHandler(this.menuRegistryTypeLibs_Click);
            // 
            // menuRegistryInterfaceProxies
            // 
            this.menuRegistryInterfaceProxies.Index = 24;
            this.menuRegistryInterfaceProxies.Text = "Interface Pro&xies";
            this.menuRegistryInterfaceProxies.Click += new System.EventHandler(this.menuRegistryInterfaceProxies_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 25;
            this.menuItem6.Text = "-";
            // 
            // menuRegistryProperties
            // 
            this.menuRegistryProperties.Index = 26;
            this.menuRegistryProperties.Text = "Registry Properties";
            this.menuRegistryProperties.Click += new System.EventHandler(this.menuRegistryProperties_Click);
            // 
            // menuObject
            // 
            this.menuObject.Index = 2;
            this.menuObject.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuObjectROT,
            this.menuItem13,
            this.menuObjectCreateInstanceFromCLSID,
            this.menuObjectFromMarshalledStream,
            this.menuObjectFromSerializedStream,
            this.menuObjectFromFile,
            this.menuItem3,
            this.menuObjectParseMoniker,
            this.menuObjectBindMoniker,
            this.menuItem9,
            this.menuHexEditor});
            this.menuObject.Text = "&Object";
            // 
            // menuObjectROT
            // 
            this.menuObjectROT.Index = 0;
            this.menuObjectROT.Text = "&Running Object Table";
            this.menuObjectROT.Click += new System.EventHandler(this.menuViewROT_Click);
            // 
            // menuItem13
            // 
            this.menuItem13.Index = 1;
            this.menuItem13.Text = "-";
            // 
            // menuObjectCreateInstanceFromCLSID
            // 
            this.menuObjectCreateInstanceFromCLSID.Index = 2;
            this.menuObjectCreateInstanceFromCLSID.Text = "&Create Instance from CLSID";
            this.menuObjectCreateInstanceFromCLSID.Click += new System.EventHandler(this.menuViewCreateInstanceFromCLSID_Click);
            // 
            // menuObjectFromMarshalledStream
            // 
            this.menuObjectFromMarshalledStream.Index = 3;
            this.menuObjectFromMarshalledStream.Text = "From &Marshalled Stream";
            this.menuObjectFromMarshalledStream.Click += new System.EventHandler(this.menuObjectFromMarshalledStream_Click);
            // 
            // menuObjectFromSerializedStream
            // 
            this.menuObjectFromSerializedStream.Index = 4;
            this.menuObjectFromSerializedStream.Text = "From &Serialized Stream";
            this.menuObjectFromSerializedStream.Click += new System.EventHandler(this.menuObjectFromSerializedStream_Click);
            // 
            // menuObjectFromFile
            // 
            this.menuObjectFromFile.Index = 5;
            this.menuObjectFromFile.Text = "From &File";
            this.menuObjectFromFile.Click += new System.EventHandler(this.menuObjectFromFile_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 6;
            this.menuItem3.Text = "-";
            // 
            // menuObjectParseMoniker
            // 
            this.menuObjectParseMoniker.Index = 7;
            this.menuObjectParseMoniker.Text = "&Parse Moniker";
            this.menuObjectParseMoniker.Click += new System.EventHandler(this.menuObjectParseMoniker_Click);
            // 
            // menuObjectBindMoniker
            // 
            this.menuObjectBindMoniker.Index = 8;
            this.menuObjectBindMoniker.Text = "&Bind Moniker";
            this.menuObjectBindMoniker.Click += new System.EventHandler(this.menuObjectBindMoniker_Click);
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 9;
            this.menuItem9.Text = "-";
            // 
            // menuHexEditor
            // 
            this.menuHexEditor.Index = 10;
            this.menuHexEditor.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuHexEditorFromFile,
            this.menuHexEditorEmpty});
            this.menuHexEditor.Text = "He&x Editor";
            // 
            // menuHexEditorFromFile
            // 
            this.menuHexEditorFromFile.Index = 0;
            this.menuHexEditorFromFile.Text = "From File";
            this.menuHexEditorFromFile.Click += new System.EventHandler(this.menuHexEditorFromFile_Click);
            // 
            // menuHexEditorEmpty
            // 
            this.menuHexEditorEmpty.Index = 1;
            this.menuHexEditorEmpty.Text = "Empty";
            this.menuHexEditorEmpty.Click += new System.EventHandler(this.menuHexEditorEmpty_Click);
            // 
            // menuSecurity
            // 
            this.menuSecurity.Index = 3;
            this.menuSecurity.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuSecurityDefaultAccess,
            this.menuSecurityDefaultAccessRestriction,
            this.menuSecurityDefaultLaunch,
            this.menuSecurityDefaultLaunchRestriction});
            this.menuSecurity.Text = "&Security";
            // 
            // menuSecurityDefaultAccess
            // 
            this.menuSecurityDefaultAccess.Index = 0;
            this.menuSecurityDefaultAccess.Text = "&View Default Access";
            this.menuSecurityDefaultAccess.Click += new System.EventHandler(this.menuSecurityDefaultAccess_Click);
            // 
            // menuSecurityDefaultAccessRestriction
            // 
            this.menuSecurityDefaultAccessRestriction.Index = 1;
            this.menuSecurityDefaultAccessRestriction.Text = "View Default &Access Restriction";
            this.menuSecurityDefaultAccessRestriction.Click += new System.EventHandler(this.menuSecurityDefaultAccessRestriction_Click);
            // 
            // menuSecurityDefaultLaunch
            // 
            this.menuSecurityDefaultLaunch.Index = 2;
            this.menuSecurityDefaultLaunch.Text = "View Default &Launch";
            this.menuSecurityDefaultLaunch.Click += new System.EventHandler(this.menuSecurityDefaultLaunch_Click);
            // 
            // menuSecurityDefaultLaunchRestriction
            // 
            this.menuSecurityDefaultLaunchRestriction.Index = 3;
            this.menuSecurityDefaultLaunchRestriction.Text = "View Default Launch &Restriction";
            this.menuSecurityDefaultLaunchRestriction.Click += new System.EventHandler(this.menuSecurityDefaultLaunchRestriction_Click);
            // 
            // menuProcesses
            // 
            this.menuProcesses.Index = 4;
            this.menuProcesses.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuProcessesSelectProcess,
            this.menuItemProcessesAllProcesses,
            this.menuItem16,
            this.menuProcessesOptions});
            this.menuProcesses.Text = "&Processes";
            // 
            // menuProcessesSelectProcess
            // 
            this.menuProcessesSelectProcess.Index = 0;
            this.menuProcessesSelectProcess.Text = "&Select Process";
            this.menuProcessesSelectProcess.Click += new System.EventHandler(this.menuProcessesSelectProcess_Click);
            // 
            // menuItemProcessesAllProcesses
            // 
            this.menuItemProcessesAllProcesses.Index = 1;
            this.menuItemProcessesAllProcesses.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuProcessesAllProcessesByPid,
            this.menuProcessesAllProcessesByName,
            this.menuProcessesAllProcessesByUser});
            this.menuItemProcessesAllProcesses.Text = "&All Processes";
            // 
            // menuProcessesAllProcessesByPid
            // 
            this.menuProcessesAllProcessesByPid.Index = 0;
            this.menuProcessesAllProcessesByPid.Text = "By PID";
            this.menuProcessesAllProcessesByPid.Click += new System.EventHandler(this.menuProcessesAllProcessesByPid_Click);
            // 
            // menuProcessesAllProcessesByName
            // 
            this.menuProcessesAllProcessesByName.Index = 1;
            this.menuProcessesAllProcessesByName.Text = "By Name";
            this.menuProcessesAllProcessesByName.Click += new System.EventHandler(this.menuProcessesAllProcessesByName_Click);
            // 
            // menuProcessesAllProcessesByUser
            // 
            this.menuProcessesAllProcessesByUser.Index = 2;
            this.menuProcessesAllProcessesByUser.Text = "By User";
            this.menuProcessesAllProcessesByUser.Click += new System.EventHandler(this.menuProcessesAllProcessesByUser_Click);
            // 
            // menuItem16
            // 
            this.menuItem16.Index = 2;
            this.menuItem16.Text = "-";
            // 
            // menuProcessesOptions
            // 
            this.menuProcessesOptions.Index = 3;
            this.menuProcessesOptions.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuProcessesOptionsResolveMethodNames,
            this.menuProcessesOptionsParseStubs,
            this.menuProcessesOptionsParseRegisteredClasses,
            this.menuProcessesOptionsParseActCtx});
            this.menuProcessesOptions.Text = "&Options";
            this.menuProcessesOptions.Popup += new System.EventHandler(this.menuProcessesOptions_Popup);
            // 
            // menuProcessesOptionsResolveMethodNames
            // 
            this.menuProcessesOptionsResolveMethodNames.Index = 0;
            this.menuProcessesOptionsResolveMethodNames.Text = "&Resolve Method Names";
            this.menuProcessesOptionsResolveMethodNames.Click += new System.EventHandler(this.menuProcessesOptionsResolveMethodNames_Click);
            // 
            // menuProcessesOptionsParseStubs
            // 
            this.menuProcessesOptionsParseStubs.Index = 1;
            this.menuProcessesOptionsParseStubs.Text = "&Parse Stubs";
            this.menuProcessesOptionsParseStubs.Click += new System.EventHandler(this.menuProcessesOptionsParseStubs_Click);
            // 
            // menuProcessesOptionsParseRegisteredClasses
            // 
            this.menuProcessesOptionsParseRegisteredClasses.Index = 2;
            this.menuProcessesOptionsParseRegisteredClasses.Text = "Parse Registered &Classes";
            this.menuProcessesOptionsParseRegisteredClasses.Click += new System.EventHandler(this.menuProcessesOptionsParseRegisteredClasses_Click);
            // 
            // menuProcessesOptionsParseActCtx
            // 
            this.menuProcessesOptionsParseActCtx.Index = 3;
            this.menuProcessesOptionsParseActCtx.Text = "Parse &Activation Context";
            this.menuProcessesOptionsParseActCtx.Click += new System.EventHandler(this.menuProcessesOptionsParseActCtx_Click);
            // 
            // menuItemStorage
            // 
            this.menuItemStorage.Index = 5;
            this.menuItemStorage.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuStorageNewStorage,
            this.menuStorageOpenStorage});
            this.menuItemStorage.Text = "S&torage";
            // 
            // menuStorageNewStorage
            // 
            this.menuStorageNewStorage.Index = 0;
            this.menuStorageNewStorage.Text = "New";
            this.menuStorageNewStorage.Click += new System.EventHandler(this.menuStorageNewStorage_Click);
            // 
            // menuStorageOpenStorage
            // 
            this.menuStorageOpenStorage.Index = 1;
            this.menuStorageOpenStorage.Text = "Open";
            this.menuStorageOpenStorage.Click += new System.EventHandler(this.menuStorageOpenStorage_Click);
            // 
            // menuItemView
            // 
            this.menuItemView.Index = 6;
            this.menuItemView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuViewOpenPropertiesViewer,
            this.menuViewRegistryViewOptions});
            this.menuItemView.Text = "&View";
            this.menuItemView.Popup += new System.EventHandler(this.menuItemView_Popup);
            // 
            // menuViewOpenPropertiesViewer
            // 
            this.menuViewOpenPropertiesViewer.Index = 0;
            this.menuViewOpenPropertiesViewer.Text = "&Open Properties Viewer";
            this.menuViewOpenPropertiesViewer.Click += new System.EventHandler(this.menuViewOpenPropertiesViewer_Click);
            // 
            // menuViewRegistryViewOptions
            // 
            this.menuViewRegistryViewOptions.Index = 1;
            this.menuViewRegistryViewOptions.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuViewAlwaysShowSourceCode,
            this.menuViewEnableAutoParsing});
            this.menuViewRegistryViewOptions.Text = "&Registry View Options";
            this.menuViewRegistryViewOptions.Popup += new System.EventHandler(this.menuViewRegistryViewOptions_Popup);
            // 
            // menuViewAlwaysShowSourceCode
            // 
            this.menuViewAlwaysShowSourceCode.Index = 0;
            this.menuViewAlwaysShowSourceCode.Text = "&Always Show Source Code";
            this.menuViewAlwaysShowSourceCode.Click += new System.EventHandler(this.menuViewAlwaysShowSourceCode_Click);
            // 
            // menuViewEnableAutoParsing
            // 
            this.menuViewEnableAutoParsing.Index = 1;
            this.menuViewEnableAutoParsing.Text = "&Enable Auto Parsing";
            this.menuViewEnableAutoParsing.Click += new System.EventHandler(this.menuViewEnableAutoParsing_Click);
            // 
            // menuHelp
            // 
            this.menuHelp.Index = 7;
            this.menuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuHelpAbout});
            this.menuHelp.Text = "&Help";
            // 
            // menuHelpAbout
            // 
            this.menuHelpAbout.Index = 0;
            this.menuHelpAbout.Text = "&About";
            this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1234, 690);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Menu = this.mainMenu;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "OleView .NET";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.MainMenu mainMenu;
    private System.Windows.Forms.MenuItem menuFile;
    private System.Windows.Forms.MenuItem menuFileExit;
    private System.Windows.Forms.MenuItem menuRegistry;
    private System.Windows.Forms.MenuItem menuViewCLSIDs;
    private System.Windows.Forms.MenuItem menuViewCLSIDsByName;
    private System.Windows.Forms.MenuItem menuViewProgIDs;
    private System.Windows.Forms.MenuItem menuViewCLSIDsByServer;
    private System.Windows.Forms.MenuItem menuViewInterfaces;
    private System.Windows.Forms.MenuItem menuViewInterfacesByName;
    private System.Windows.Forms.MenuItem menuObjectROT;
    private System.Windows.Forms.MenuItem menuViewImplementedCategories;
    private System.Windows.Forms.MenuItem menuViewPreApproved;
    private System.Windows.Forms.MenuItem menuItem1;
    private System.Windows.Forms.MenuItem menuObjectCreateInstanceFromCLSID;
    private System.Windows.Forms.MenuItem menuObject;
    private System.Windows.Forms.MenuItem menuViewCLSIDsByLocalServer;
    private System.Windows.Forms.MenuItem menuViewIELowRights;
    private System.Windows.Forms.MenuItem menuViewLocalServices;
    private System.Windows.Forms.MenuItem menuViewAppIDs;
    private System.Windows.Forms.MenuItem menuObjectFromMarshalledStream;
    private System.Windows.Forms.MenuItem menuObjectFromSerializedStream;
    private System.Windows.Forms.MenuItem menuHelp;
    private System.Windows.Forms.MenuItem menuHelpAbout;
    private System.Windows.Forms.MenuItem menuRegistryTypeLibraries;
    private System.Windows.Forms.MenuItem menuRegistryAppIDsIL;
    private System.Windows.Forms.MenuItem menuViewCLSIDsWithSurrogate;
    private System.Windows.Forms.MenuItem menuFileOpen32BitViewer;
    private System.Windows.Forms.MenuItem menuObjectBindMoniker;
    private System.Windows.Forms.MenuItem menuFileOpenDatabase;
    private System.Windows.Forms.MenuItem menuFileSaveDatabase;
    private System.Windows.Forms.MenuItem menuRegistryMimeTypes;
    private System.Windows.Forms.MenuItem menuRegistryAppIDsWithAC;
    private System.Windows.Forms.MenuItem menuSecurity;
    private System.Windows.Forms.MenuItem menuSecurityDefaultAccess;
    private System.Windows.Forms.MenuItem menuSecurityDefaultAccessRestriction;
    private System.Windows.Forms.MenuItem menuSecurityDefaultLaunch;
    private System.Windows.Forms.MenuItem menuSecurityDefaultLaunchRestriction;
    private System.Windows.Forms.MenuItem menuItem4;
    private System.Windows.Forms.MenuItem menuItem5;
    private System.Windows.Forms.MenuItem menuItem6;
    private System.Windows.Forms.MenuItem menuRegistryProperties;
    private System.Windows.Forms.MenuItem menuFileOpenMachineOnly;
    private System.Windows.Forms.MenuItem menuFileOpenUserOnly;
    private System.Windows.Forms.MenuItem menuFileDiff;
    private System.Windows.Forms.MenuItem menuObjectFromFile;
    private System.Windows.Forms.MenuItem menuObjectParseMoniker;
    private System.Windows.Forms.MenuItem menuHexEditor;
    private System.Windows.Forms.MenuItem menuHexEditorFromFile;
    private System.Windows.Forms.MenuItem menuHexEditorEmpty;
    private System.Windows.Forms.MenuItem menuItem7;
    private System.Windows.Forms.MenuItem menuItem8;
    private System.Windows.Forms.MenuItem menuFileOpenTypeLib;
    private System.Windows.Forms.MenuItem menuItem3;
    private System.Windows.Forms.MenuItem menuItem9;
    private System.Windows.Forms.MenuItem menuRegistryInterfaceProxies;
    private System.Windows.Forms.MenuItem menuFileOpenProxyDll;
    private System.Windows.Forms.MenuItem menuFileQueryAllInterfaces;
    private System.Windows.Forms.MenuItem menuItem10;
    private System.Windows.Forms.MenuItem menuFileSaveAsDatabase;
    private System.Windows.Forms.MenuItem menuFileSettings;
    private System.Windows.Forms.MenuItem menuItem12;
    private System.Windows.Forms.MenuItem menuProcesses;
    private System.Windows.Forms.MenuItem menuItem13;
    private System.Windows.Forms.MenuItem menuProcessesAllProcessesByPid;
    private System.Windows.Forms.MenuItem menuProcessesAllProcessesByName;
    private System.Windows.Forms.MenuItem menuProcessesAllProcessesByUser;
    private System.Windows.Forms.MenuItem menuFileOpenAsAdmin;
    private System.Windows.Forms.MenuItem menuViewOpenPropertiesViewer;
    private System.Windows.Forms.MenuItem menuStorageOpenStorage;
    private System.Windows.Forms.MenuItem menuRegistryRuntimeClasses;
    private System.Windows.Forms.MenuItem menuProcessesSelectProcess;
    private System.Windows.Forms.MenuItem menuRegistryRuntimeServers;
    private System.Windows.Forms.MenuItem menuItemStorage;
    private System.Windows.Forms.MenuItem menuStorageNewStorage;
    private System.Windows.Forms.MenuItem menuItemProcessesAllProcesses;
    private System.Windows.Forms.MenuItem menuItem2;
    private System.Windows.Forms.MenuItem menuFileOpenPowershell;
    private System.Windows.Forms.MenuItem menuFileOpen64BitViewer;
    private System.Windows.Forms.MenuItem menuFileOpenARM64Viewer;
    private System.Windows.Forms.MenuItem menuFileExportInterfaceList;
    private System.Windows.Forms.MenuItem menuFileSaveDefaultDatabase;
    private System.Windows.Forms.MenuItem menuFileDeleteDefaultDatabase;
    private System.Windows.Forms.MenuItem menuFileSaveDatabaseOnExit;
    private System.Windows.Forms.MenuItem menuItem14;
    private System.Windows.Forms.MenuItem menuItem16;
    private System.Windows.Forms.MenuItem menuProcessesOptions;
    private System.Windows.Forms.MenuItem menuProcessesOptionsResolveMethodNames;
    private System.Windows.Forms.MenuItem menuProcessesOptionsParseStubs;
    private System.Windows.Forms.MenuItem menuProcessesOptionsParseRegisteredClasses;
    private System.Windows.Forms.MenuItem menuProcessesOptionsParseActCtx;
    private System.Windows.Forms.MenuItem menuViewRuntimeInterfaces;
    private System.Windows.Forms.MenuItem menuViewRuntimeInterfacesTree;
    private System.Windows.Forms.MenuItem menuItemView;
    private System.Windows.Forms.MenuItem menuViewRegistryViewOptions;
    private System.Windows.Forms.MenuItem menuViewAlwaysShowSourceCode;
    private System.Windows.Forms.MenuItem menuViewEnableAutoParsing;
}

