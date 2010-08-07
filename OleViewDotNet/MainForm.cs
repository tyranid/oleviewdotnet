using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Runtime.InteropServices;

namespace OleViewDotNet
{
    public partial class MainForm : Form
    {
        private DockPanel   m_dockPanel;
        private COMRegistry m_comRegistry;        

        public MainForm(COMRegistry comRegistry)
        {            
            m_comRegistry = comRegistry;
            InitializeComponent();
            m_dockPanel = new DockPanel();
            m_dockPanel.ActiveAutoHideContent = null;
            m_dockPanel.Dock = DockStyle.Fill;
            m_dockPanel.Name = "dockPanel";
            Controls.Add(m_dockPanel);
            m_dockPanel.BringToFront();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {            
            if (IntPtr.Size == 4)
            {
                Text += " 32bit";
            }
            else
            {
                Text += " 64bit";
            }
        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenView(COMRegistryViewer.DisplayMode mode)
        {            
            COMRegistryViewer view = new COMRegistryViewer(m_comRegistry, mode);
            view.ShowHint = DockState.Document;
            view.Show(m_dockPanel);            
        }

        private void menuViewCLSIDs_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.CLSIDs);
        }

        private void menuViewCLSIDsByName_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.CLSIDsByName);
        }

        private void menuViewProgIDs_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.ProgIDs);
        }

        private void menuViewCLSIDsByServer_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.CLSIDsByServer);
        }

        private void menuViewInterfaces_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.Interfaces);
        }

        private void menuViewInterfacesByName_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.InterfacesByName);
        }

        private void menuViewROT_Click(object sender, EventArgs e)
        {
            ROTViewer view = new ROTViewer(m_comRegistry);
            view.ShowHint = DockState.Document;
            view.Show(m_dockPanel);
        }

        private void menuViewImplementedCategories_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.ImplementedCategories);
        }

        private void menuViewPreApproved_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.PreApproved);
        }

        private void menuViewNewWindow_Click(object sender, EventArgs e)
        {
            Program.CreateNewMainForm();            
        }

        private void menuViewCreateInstanceFromCLSID_Click(object sender, EventArgs e)
        {
            GetTextForm frm = new GetTextForm("");
            if (frm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Guid g = new Guid(frm.Data);
                    Dictionary<string, string> props = new Dictionary<string,string>();
                    object comObj = null;
                    string strObjName = "";
                    COMInterfaceEntry[] ints = null;

                    if (m_comRegistry.Clsids.ContainsKey(g))
                    {
                        COMCLSIDEntry ent = m_comRegistry.Clsids[g];
                        strObjName = ent.Name;
                        props.Add("CLSID", ent.Clsid.ToString("B"));
                        props.Add("Name", ent.Name);
                        props.Add("Server", ent.Server);

                        comObj = ent.CreateInstanceAsObject();
                        ints = m_comRegistry.GetSupportedInterfaces(ent, false);
                    }
                    else
                    {
                        Guid unk = COMInterfaceEntry.IID_IUnknown;
                        IntPtr pObj;

                        if (COMUtilities.CoCreateInstance(ref g, IntPtr.Zero, COMUtilities.CLSCTX.CLSCTX_SERVER,
                            ref unk, out pObj) == 0)
                        {
                            ints = m_comRegistry.GetInterfacesForIUnknown(pObj);
                            comObj = Marshal.GetObjectForIUnknown(pObj);                            
                            strObjName = g.ToString("B");
                            props.Add("CLSID", g.ToString("B"));
                            Marshal.Release(pObj);
                        }
                    }

                    if (comObj != null)
                    {
                        /* Need to implement a type library reader */
                        Type dispType = COMUtilities.GetDispatchTypeInfo(comObj);

                        ObjectInformation view = new ObjectInformation(strObjName, comObj, props, ints);
                        view.ShowHint = DockState.Document;
                        view.Show(m_dockPanel);
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuFileOpenPSWindow_Click(object sender, EventArgs e)
        {
            try
            {
                PowerShellInstance.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
