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

using OleViewDotNet.InterfaceViewers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace OleViewDotNet
{
    public partial class MainForm : Form
    {
        private DockPanel   m_dockPanel;      

        public MainForm()
        {                        
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
            COMRegistryViewer view = new COMRegistryViewer(COMRegistry.Instance, mode);
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
            ROTViewer view = new ROTViewer(COMRegistry.Instance);
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

                        if (COMRegistry.Instance.Clsids.ContainsKey(g))
                        {
                            COMCLSIDEntry ent = COMRegistry.Instance.Clsids[g];
                            strObjName = ent.Name;
                            props.Add("CLSID", ent.Clsid.ToString("B"));
                            props.Add("Name", ent.Name);
                            props.Add("Server", ent.Server);

                            comObj = ent.CreateInstanceAsObject(frm.ClsCtx);
                            ints = COMRegistry.Instance.GetSupportedInterfaces(ent, false);
                        }
                        else
                        {
                            Guid unk = COMInterfaceEntry.IID_IUnknown;
                            IntPtr pObj;

                            if (COMUtilities.CoCreateInstance(ref g, IntPtr.Zero, frm.ClsCtx,
                                ref unk, out pObj) == 0)
                            {
                                ints = COMRegistry.Instance.GetInterfacesForIUnknown(pObj);
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

        private void menuViewLocalServices_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.LocalServices);
        }

        private void menuViewAppIDs_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.AppIDs);
        }

        private void menuFilePythonConsole_Click(object sender, EventArgs e)
        {
            PythonConsole view = new PythonConsole();
            view.ShowHint = DockState.Document;
            view.Show(m_dockPanel);
        }

        private void menuObjectFromMarshalledStream_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "All Files (*.*)|*.*";

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        byte[] data = File.ReadAllBytes(dlg.FileName);

                        string objref = String.Format("objref:{0}:", Convert.ToBase64String(data));
                        object comObj = Marshal.BindToMoniker(objref);

                        OpenObjectInformation(comObj, "Marshalled Object");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void OpenObjectInformation(object comObj, string defaultName)
        {
            if (comObj != null)
            {
                Dictionary<string, string> props = new Dictionary<string, string>();
                string strObjName = "";
                COMInterfaceEntry[] ints = null;
                Guid clsid = COMUtilities.GetObjectClass(comObj);

                if (COMRegistry.Instance.Clsids.ContainsKey(clsid))
                {
                    COMCLSIDEntry ent = COMRegistry.Instance.Clsids[clsid];
                    strObjName = ent.Name;
                    props.Add("CLSID", ent.Clsid.ToString("B"));
                    props.Add("Name", ent.Name);
                    props.Add("Server", ent.Server);
                    ints = COMRegistry.Instance.GetSupportedInterfaces(ent, false);
                }
                else
                {
                    ints = COMRegistry.Instance.GetInterfacesForObject(comObj);
                    strObjName = defaultName != null ? defaultName : clsid.ToString("B");
                    props.Add("CLSID", clsid.ToString("B"));
                }

                Type dispType = COMUtilities.GetDispatchTypeInfo(comObj);
                ObjectInformation view = new ObjectInformation(strObjName, comObj, props, ints);
                view.ShowHint = DockState.Document;
                view.Show(m_dockPanel);
            }
        }

        private void menuObjectFromSerializedStream_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "All Files (*.*)|*.*";

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        using (Stream stm = dlg.OpenFile())
                        {
                            OpenObjectInformation(COMUtilities.OleLoadFromStream(stm), null);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
