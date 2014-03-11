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
            this.menuFileNewWindow = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuFileExit = new System.Windows.Forms.MenuItem();
            this.menuView = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDs = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDsByName = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDsByServer = new System.Windows.Forms.MenuItem();
            this.menuViewCLSIDsByLocalServer = new System.Windows.Forms.MenuItem();
            this.menuViewProgIDs = new System.Windows.Forms.MenuItem();
            this.menuViewPreApproved = new System.Windows.Forms.MenuItem();
            this.menuViewImplementedCategories = new System.Windows.Forms.MenuItem();
            this.menuViewIELowRights = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuViewInterfaces = new System.Windows.Forms.MenuItem();
            this.menuViewInterfacesByName = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuObjectROT = new System.Windows.Forms.MenuItem();
            this.menuObjectCreateInstanceFromCLSID = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuFile,
            this.menuView,
            this.menuItem4});
            // 
            // menuFile
            // 
            this.menuFile.Index = 0;
            this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuFileNewWindow,
            this.menuItem2,
            this.menuFileExit});
            this.menuFile.Text = "&File";
            // 
            // menuFileNewWindow
            // 
            this.menuFileNewWindow.Index = 0;
            this.menuFileNewWindow.Text = "New Window";
            this.menuFileNewWindow.Click += new System.EventHandler(this.menuViewNewWindow_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.Text = "-";
            // 
            // menuFileExit
            // 
            this.menuFileExit.Index = 2;
            this.menuFileExit.Text = "E&xit";
            this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
            // 
            // menuView
            // 
            this.menuView.Index = 1;
            this.menuView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuViewCLSIDs,
            this.menuViewCLSIDsByName,
            this.menuViewCLSIDsByServer,
            this.menuViewCLSIDsByLocalServer,
            this.menuViewProgIDs,
            this.menuViewPreApproved,
            this.menuViewImplementedCategories,
            this.menuViewIELowRights,
            this.menuItem1,
            this.menuViewInterfaces,
            this.menuViewInterfacesByName});
            this.menuView.Text = "&View";
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
            // menuViewProgIDs
            // 
            this.menuViewProgIDs.Index = 4;
            this.menuViewProgIDs.Text = "ProgIDs";
            this.menuViewProgIDs.Click += new System.EventHandler(this.menuViewProgIDs_Click);
            // 
            // menuViewPreApproved
            // 
            this.menuViewPreApproved.Index = 5;
            this.menuViewPreApproved.Text = "Explorer PreApproved";
            this.menuViewPreApproved.Click += new System.EventHandler(this.menuViewPreApproved_Click);
            // 
            // menuViewImplementedCategories
            // 
            this.menuViewImplementedCategories.Index = 6;
            this.menuViewImplementedCategories.Text = "Implemented Categories";
            this.menuViewImplementedCategories.Click += new System.EventHandler(this.menuViewImplementedCategories_Click);
            // 
            // menuViewIELowRights
            // 
            this.menuViewIELowRights.Index = 7;
            this.menuViewIELowRights.Text = "IE Low RIghts Elevation Policy";
            this.menuViewIELowRights.Click += new System.EventHandler(this.menuViewIELowRights_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 8;
            this.menuItem1.Text = "-";
            // 
            // menuViewInterfaces
            // 
            this.menuViewInterfaces.Index = 9;
            this.menuViewInterfaces.Text = "Interfaces";
            this.menuViewInterfaces.Click += new System.EventHandler(this.menuViewInterfaces_Click);
            // 
            // menuViewInterfacesByName
            // 
            this.menuViewInterfacesByName.Index = 10;
            this.menuViewInterfacesByName.Text = "Interfaces By Name";
            this.menuViewInterfacesByName.Click += new System.EventHandler(this.menuViewInterfacesByName_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 2;
            this.menuItem4.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuObjectROT,
            this.menuObjectCreateInstanceFromCLSID});
            this.menuItem4.Text = "&Object";
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
        private System.Windows.Forms.MenuItem menuView;
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
        private System.Windows.Forms.MenuItem menuFileNewWindow;
        private System.Windows.Forms.MenuItem menuObjectCreateInstanceFromCLSID;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem menuViewCLSIDsByLocalServer;
        private System.Windows.Forms.MenuItem menuViewIELowRights;
    }
}

