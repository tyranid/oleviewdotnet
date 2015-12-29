namespace OleViewDotNet
{
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.menuFile = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuFileExit = new System.Windows.Forms.MenuItem();
            this.menuRegistry = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDs = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDsByName = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDsByServer = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDsByLocalServer = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDsWithSurrogate = new System.Windows.Forms.MenuItem();
            this.menuViewProgIDs = new System.Windows.Forms.MenuItem();
            this.menuViewPreApproved = new System.Windows.Forms.MenuItem();
            this.menuViewImplementedCategories = new System.Windows.Forms.MenuItem();
            this.menuViewIELowRights = new System.Windows.Forms.MenuItem();
            this.menuViewAppIDs = new System.Windows.Forms.MenuItem();
            this.menuRegistryAppIDsIL = new System.Windows.Forms.MenuItem();
            this.menuViewLocalServices = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuViewInterfaces = new System.Windows.Forms.MenuItem();
            this.menuViewInterfacesByName = new System.Windows.Forms.MenuItem();
            this.menuRegistryTypeLibraries = new System.Windows.Forms.MenuItem();
            this.menuObject = new System.Windows.Forms.MenuItem();
            this.menuObjectROT = new System.Windows.Forms.MenuItem();
            this.menuObjectCreateInstanceFromCLSID = new System.Windows.Forms.MenuItem();
            this.menuObjectFromMarshalledStream = new System.Windows.Forms.MenuItem();
            this.menuObjectFromSerializedStream = new System.Windows.Forms.MenuItem();
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
            this.menuHelp});
            // 
            // menuFile
            // 
            this.menuFile.Index = 0;
            this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem3,
            this.menuItem2,
            this.menuFileExit});
            this.menuFile.Text = "&File";
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 0;
            this.menuItem3.Text = "Python Console";
            this.menuItem3.Click += new System.EventHandler(this.menuFilePythonConsole_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.Text = "-";
            // 
            // menuFileExit
            // 
            this.menuFileExit.Index = 2;
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
            this.menuViewPreApproved,
            this.menuViewImplementedCategories,
            this.menuViewIELowRights,
            this.menuViewAppIDs,
            this.menuRegistryAppIDsIL,
            this.menuViewLocalServices,
            this.menuItem1,
            this.menuViewInterfaces,
            this.menuViewInterfacesByName,
            this.menuRegistryTypeLibraries});
            this.menuRegistry.Text = "&Registry";
            // 
            // menuViewCLSIDs
            // 
            this.menuViewCLSIDs.Index = 0;
            this.menuViewCLSIDs.Text = "CLSIDs";
            this.menuViewCLSIDs.Click += new System.EventHandler(this.menuViewCLSIDs_Click);
            // 
            // menuViewCLSIDsByName
            // 
            this.menuViewCLSIDsByName.Index = 1;
            this.menuViewCLSIDsByName.Text = "CLSIDs By Name";
            this.menuViewCLSIDsByName.Click += new System.EventHandler(this.menuViewCLSIDsByName_Click);
            // 
            // menuViewCLSIDsByServer
            // 
            this.menuViewCLSIDsByServer.Index = 2;
            this.menuViewCLSIDsByServer.Text = "CLSIDs By Server";
            this.menuViewCLSIDsByServer.Click += new System.EventHandler(this.menuViewCLSIDsByServer_Click);
            // 
            // menuViewCLSIDsByLocalServer
            // 
            this.menuViewCLSIDsByLocalServer.Index = 3;
            this.menuViewCLSIDsByLocalServer.Text = "CLSIDs By Local Server";
            this.menuViewCLSIDsByLocalServer.Click += new System.EventHandler(this.menuViewCLSIDsByLocalServer_Click);
            // 
            // menuViewCLSIDsWithSurrogate
            // 
            this.menuViewCLSIDsWithSurrogate.Index = 4;
            this.menuViewCLSIDsWithSurrogate.Text = "CLSIDs with DLL Surrogate";
            this.menuViewCLSIDsWithSurrogate.Click += new System.EventHandler(this.menuViewCLSIDsWithSurrogate_Click);
            // 
            // menuViewProgIDs
            // 
            this.menuViewProgIDs.Index = 5;
            this.menuViewProgIDs.Text = "ProgIDs";
            this.menuViewProgIDs.Click += new System.EventHandler(this.menuViewProgIDs_Click);
            // 
            // menuViewPreApproved
            // 
            this.menuViewPreApproved.Index = 6;
            this.menuViewPreApproved.Text = "Explorer PreApproved";
            this.menuViewPreApproved.Click += new System.EventHandler(this.menuViewPreApproved_Click);
            // 
            // menuViewImplementedCategories
            // 
            this.menuViewImplementedCategories.Index = 7;
            this.menuViewImplementedCategories.Text = "Implemented Categories";
            this.menuViewImplementedCategories.Click += new System.EventHandler(this.menuViewImplementedCategories_Click);
            // 
            // menuViewIELowRights
            // 
            this.menuViewIELowRights.Index = 8;
            this.menuViewIELowRights.Text = "IE Low RIghts Elevation Policy";
            this.menuViewIELowRights.Click += new System.EventHandler(this.menuViewIELowRights_Click);
            // 
            // menuViewAppIDs
            // 
            this.menuViewAppIDs.Index = 9;
            this.menuViewAppIDs.Text = "App IDs";
            this.menuViewAppIDs.Click += new System.EventHandler(this.menuViewAppIDs_Click);
            // 
            // menuRegistryAppIDsIL
            // 
            this.menuRegistryAppIDsIL.Index = 10;
            this.menuRegistryAppIDsIL.Text = "App IDs With IL";
            this.menuRegistryAppIDsIL.Click += new System.EventHandler(this.menuRegistryAppIDsIL_Click);
            // 
            // menuViewLocalServices
            // 
            this.menuViewLocalServices.Index = 11;
            this.menuViewLocalServices.Text = "Local Services";
            this.menuViewLocalServices.Click += new System.EventHandler(this.menuViewLocalServices_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 12;
            this.menuItem1.Text = "-";
            // 
            // menuViewInterfaces
            // 
            this.menuViewInterfaces.Index = 13;
            this.menuViewInterfaces.Text = "Interfaces";
            this.menuViewInterfaces.Click += new System.EventHandler(this.menuViewInterfaces_Click);
            // 
            // menuViewInterfacesByName
            // 
            this.menuViewInterfacesByName.Index = 14;
            this.menuViewInterfacesByName.Text = "Interfaces By Name";
            this.menuViewInterfacesByName.Click += new System.EventHandler(this.menuViewInterfacesByName_Click);
            // 
            // menuRegistryTypeLibraries
            // 
            this.menuRegistryTypeLibraries.Index = 15;
            this.menuRegistryTypeLibraries.Text = "Type Libraries";
            this.menuRegistryTypeLibraries.Click += new System.EventHandler(this.menuRegistryTypeLibs_Click);
            // 
            // menuObject
            // 
            this.menuObject.Index = 2;
            this.menuObject.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuObjectROT,
            this.menuObjectCreateInstanceFromCLSID,
            this.menuObjectFromMarshalledStream,
            this.menuObjectFromSerializedStream});
            this.menuObject.Text = "&Object";
            // 
            // menuObjectROT
            // 
            this.menuObjectROT.Index = 0;
            this.menuObjectROT.Text = "Running Object Table";
            this.menuObjectROT.Click += new System.EventHandler(this.menuViewROT_Click);
            // 
            // menuObjectCreateInstanceFromCLSID
            // 
            this.menuObjectCreateInstanceFromCLSID.Index = 1;
            this.menuObjectCreateInstanceFromCLSID.Text = "Create Instance from CLSID";
            this.menuObjectCreateInstanceFromCLSID.Click += new System.EventHandler(this.menuViewCreateInstanceFromCLSID_Click);
            // 
            // menuObjectFromMarshalledStream
            // 
            this.menuObjectFromMarshalledStream.Index = 2;
            this.menuObjectFromMarshalledStream.Text = "From Marshalled Stream";
            this.menuObjectFromMarshalledStream.Click += new System.EventHandler(this.menuObjectFromMarshalledStream_Click);
            // 
            // menuObjectFromSerializedStream
            // 
            this.menuObjectFromSerializedStream.Index = 3;
            this.menuObjectFromSerializedStream.Text = "From Serialized Stream";
            this.menuObjectFromSerializedStream.Click += new System.EventHandler(this.menuObjectFromSerializedStream_Click);
            // 
            // menuHelp
            // 
            this.menuHelp.Index = 3;
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 448);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Menu = this.mainMenu;
            this.Name = "MainForm";
            this.Text = "OleView .NET";
            this.Load += new System.EventHandler(this.MainForm_Load);
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
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuObject;
        private System.Windows.Forms.MenuItem menuViewCLSIDsByLocalServer;
        private System.Windows.Forms.MenuItem menuViewIELowRights;
        private System.Windows.Forms.MenuItem menuViewLocalServices;
        private System.Windows.Forms.MenuItem menuViewAppIDs;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuObjectFromMarshalledStream;
        private System.Windows.Forms.MenuItem menuObjectFromSerializedStream;
        private System.Windows.Forms.MenuItem menuHelp;
        private System.Windows.Forms.MenuItem menuHelpAbout;
        private System.Windows.Forms.MenuItem menuRegistryTypeLibraries;
        private System.Windows.Forms.MenuItem menuRegistryAppIDsIL;
        private System.Windows.Forms.MenuItem menuViewCLSIDsWithSurrogate;
    }
}

