//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;
using WeifenLuo.WinFormsUI.Docking;

namespace OleViewDotNet
{
    public partial class MainForm : Form
    {
        private DockPanel   m_dockPanel;
        private COMRegistry m_registry;  

        public MainForm(COMRegistry registry)
        {
            m_registry = registry;
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
            if (!Environment.Is64BitOperatingSystem)
            {
                menuFileOpenViewer.Visible = false;
            }
            else
            {
                if (!Environment.Is64BitProcess)
                {
                    Text += " 32bit";
                    menuFileOpenViewer.Text = "Open 64bit Viewer";
                }
                else
                {
                    Text += " 64bit";
                    menuFileOpenViewer.Text = "Open 32bit Viewer";
                }
            }
        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void HostControl(Control c)
        {
            DocumentForm frm = new DocumentForm(c);

            frm.ShowHint = DockState.Document;
            frm.Show(m_dockPanel);
        }

        private void OpenView(COMRegistryViewer.DisplayMode mode)
        {
            HostControl(new COMRegistryViewer(m_registry, mode));                
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
            HostControl(new ROTViewer(m_registry));
        }

        private void menuViewImplementedCategories_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.ImplementedCategories);
        }

        private void menuViewPreApproved_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.PreApproved);
        }

        private async void menuViewCreateInstanceFromCLSID_Click(object sender, EventArgs e)
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
                        IEnumerable<COMInterfaceEntry> ints = null;

                        if (m_registry.Clsids.ContainsKey(g))
                        {
                            COMCLSIDEntry ent = m_registry.Clsids[g];
                            strObjName = ent.Name;
                            props.Add("CLSID", ent.Clsid.ToString("B"));
                            props.Add("Name", ent.Name);
                            props.Add("Server", ent.Server);

                            comObj = ent.CreateInstanceAsObject(frm.ClsCtx);
                            ints = await ent.LoadSupportedInterfaces(false);
                        }
                        else
                        {
                            Guid unk = COMInterfaceEntry.IID_IUnknown;
                            IntPtr pObj;

                            if (COMUtilities.CoCreateInstance(ref g, IntPtr.Zero, frm.ClsCtx,
                                ref unk, out pObj) == 0)
                            {
                                ints = m_registry.GetInterfacesForIUnknown(pObj);
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

                            HostControl(new ObjectInformation(m_registry, strObjName, comObj, props, ints.ToArray()));                            
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
            HostControl(new PythonConsole());
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

                        IStreamImpl stm = new IStreamImpl(new MemoryStream(data));
                        Guid iid = COMInterfaceEntry.IID_IUnknown;
                        IntPtr pv;

                        int hr = COMUtilities.CoUnmarshalInterface(stm, ref iid, out pv);
                        if (hr == 0)
                        {
                            object comObj = Marshal.GetObjectForIUnknown(pv);
                            Marshal.Release(pv);

                            OpenObjectInformation(comObj, "Marshalled Object");
                        }
                        else
                        {
                            Marshal.ThrowExceptionForHR(hr);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void OpenObjectInformation(object comObj, string defaultName)
        {
            if (comObj != null)
            {
                Dictionary<string, string> props = new Dictionary<string, string>();
                string strObjName = "";
                IEnumerable<COMInterfaceEntry> ints = null;
                Guid clsid = COMUtilities.GetObjectClass(comObj);

                if (m_registry.Clsids.ContainsKey(clsid))
                {
                    COMCLSIDEntry ent = m_registry.Clsids[clsid];
                    strObjName = ent.Name;
                    props.Add("CLSID", ent.Clsid.ToString("B"));
                    props.Add("Name", ent.Name);
                    props.Add("Server", ent.Server);
                    ints = await ent.LoadSupportedInterfaces(false);
                }
                else
                {
                    ints = m_registry.GetInterfacesForObject(comObj);
                    strObjName = defaultName != null ? defaultName : clsid.ToString("B");
                    props.Add("CLSID", clsid.ToString("B"));
                }

                Type dispType = COMUtilities.GetDispatchTypeInfo(comObj);
                HostControl(new ObjectInformation(m_registry, strObjName, comObj, props, ints.ToArray()));
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

        private void menuHelpAbout_Click(object sender, EventArgs e)
        {
            using (AboutForm frm = new AboutForm())
            {
                frm.ShowDialog(this);
            }
        }

        private void menuRegistryTypeLibs_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.Typelibs);
        }

        private void menuRegistryAppIDsIL_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.AppIDsWithIL);
        }

        private void menuViewCLSIDsWithSurrogate_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.CLSIDsWithSurrogate);
        }

        private void menuFileOpenViewer_Click(object sender, EventArgs e)
        {
            try
            {
                if (Environment.Is64BitProcess)
                {
                    Process.Start(COMUtilities.Get32bitExePath()).Close();
                }
                else
                {
                    Process.Start(COMUtilities.GetExePath()).Close();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void menuObjectBindMoniker_Click(object sender, EventArgs e)
        {
            using (GetTextForm frm = new GetTextForm("Specify Monker"))
            {
                frm.Text = "Specify Moniker";
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        object comObj = Marshal.BindToMoniker(frm.Data);

                        if (comObj != null)
                        {
                            OpenObjectInformation(comObj, frm.Data);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void menuFileSaveDatabase_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "OleViewDotNet DB File (*.ovdb)|*.ovdb|All Files (*.*)|*.*";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        m_registry.Save(dlg.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void menuFileOpenDatabase_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "OleViewDotNet DB File (*.ovdb)|*.ovdb|All Files (*.*)|*.*";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    using (LoadingDialog loader = new LoadingDialog(false, dlg.FileName))
                    {
                        if ((loader.ShowDialog() != DialogResult.OK) && (loader.Error != null))
                        {
                            MessageBox.Show(loader.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }                    
                }
            }
        }

        private void menuRegistryMimeTypes_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.MimeTypes);
        }
    }
}
