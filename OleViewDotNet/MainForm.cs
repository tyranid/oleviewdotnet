//    This file is part of OleViewDotNet.
//
//    OleViewDotNet is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    OleViewDotNet is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with OleViewDotNet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

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
            if (!Environment.Is64BitProcess)
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
            using (CreateCLSIDForm frm = new CreateCLSIDForm())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Guid g = frm.Clsid;
                        Dictionary<string, string> props = new Dictionary<string, string>();
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

                            comObj = ent.CreateInstanceAsObject(frm.ClsCtx);
                            ints = m_comRegistry.GetSupportedInterfaces(ent, false);
                        }
                        else
                        {
                            Guid unk = COMInterfaceEntry.IID_IUnknown;
                            IntPtr pObj;

                            if (COMUtilities.CoCreateInstance(ref g, IntPtr.Zero, frm.ClsCtx,
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
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void menuViewCLSIDsByLocalServer_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.CLSIDsByLocalServer);
        }

        private void menuViewIELowRights_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.IELowRights);
        }

    }
}
