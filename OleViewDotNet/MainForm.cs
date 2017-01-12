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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace OleViewDotNet
{
    public partial class MainForm : Form
    {
        private DockPanel   m_dockPanel;
        private COMRegistry m_registry;

        private void UpdateTitle()
        {
            Text = "OleViewDotNet";
            if (Environment.Is64BitOperatingSystem)
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

            if (m_registry.FilePath != null)
            {
                Text += String.Format(" - {0}", m_registry.FilePath);
            }
        }

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

            if (!Environment.Is64BitOperatingSystem)
            {
                menuFileOpenViewer.Visible = false;
            }
            else
            {
                if (!Environment.Is64BitProcess)
                {
                    menuFileOpenViewer.Text = "Open 64bit Viewer";
                }
                else
                {
                    menuFileOpenViewer.Text = "Open 32bit Viewer";
                }
            }

            UpdateTitle();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            HostControl(new RegistryPropertiesControl(m_registry));
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

        public async Task HostObject(COMCLSIDEntry ent, object obj, bool factory)
        {
            Dictionary<string, string> props = new Dictionary<string, string>();

            if (ent == null)
            {
                ent = new COMCLSIDEntry(Guid.Empty, COMServerType.UnknownServer);
            }
            props.Add("CLSID", ent.Clsid.FormatGuid());
            props.Add("Name", ent.Name);
            props.Add("Server", ent.DefaultServer);

            /* Need to implement a type library reader */
            Type dispType = COMUtilities.GetDispatchTypeInfo(this, obj);

            if (!ent.InterfacesLoaded)
            {
                await ent.LoadSupportedInterfacesAsync(false);
            }

            IEnumerable<COMInterfaceInstance> intfs = factory ? ent.FactoryInterfaces : ent.Interfaces;

            ObjectInformation view = new ObjectInformation(m_registry, ent.Name, obj,
                props, intfs.Select(i => m_registry.MapIidToInterface(i.Iid)).ToArray());
            HostControl(view);
        }

        private void OpenView(COMRegistryViewer.DisplayMode mode, IEnumerable<COMProcessEntry> processes)
        {
            HostControl(new COMRegistryViewer(m_registry, mode, processes));                
        }

        private void OpenView(COMRegistryViewer.DisplayMode mode)
        {
            OpenView(mode, null);
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
                            props.Add("CLSID", ent.Clsid.FormatGuid());
                            props.Add("Name", ent.Name);
                            props.Add("Server", ent.DefaultServer);
                            await ent.LoadSupportedInterfacesAsync(false);

                            if (frm.ClassFactory)
                            {
                                comObj = ent.CreateClassFactory();
                                ints = ent.FactoryInterfaces.Select(i => m_registry.MapIidToInterface(i.Iid));
                            }
                            else
                            {
                                comObj = ent.CreateInstanceAsObject(frm.ClsCtx, null);
                                ints = ent.Interfaces.Select(i => m_registry.MapIidToInterface(i.Iid));
                            }
                        }
                        else
                        {
                            Guid unk = COMInterfaceEntry.IID_IUnknown;
                            IntPtr pObj;
                            int hr;

                            if (frm.ClassFactory)
                            {
                                hr = COMUtilities.CoGetClassObject(ref g, frm.ClsCtx, null, ref unk, out pObj);
                            }
                            else
                            {
                                hr = COMUtilities.CoCreateInstance(ref g, IntPtr.Zero, frm.ClsCtx,
                                            ref unk, out pObj);
                            }

                            if (hr == 0)
                            {
                                try
                                {
                                    ints = m_registry.GetInterfacesForIUnknown(pObj);
                                    comObj = Marshal.GetObjectForIUnknown(pObj);
                                    strObjName = g.FormatGuid();
                                    props.Add("CLSID", g.FormatGuid());
                                }
                                finally
                                {
                                    Marshal.Release(pObj);
                                }
                            }
                        }

                        if (comObj != null)
                        {
                            /* Need to implement a type library reader */
                            Type dispType = COMUtilities.GetDispatchTypeInfo(this, comObj);

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

        private async void menuObjectFromMarshalledStream_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "All Files (*.*)|*.*";

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        byte[] data = File.ReadAllBytes(dlg.FileName);
                        object comObj = COMUtilities.UnmarshalObject(new MemoryStream(data));
                        await OpenObjectInformation(comObj, String.Format("Unmarshalled {0}", Path.GetFileName(dlg.FileName)));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        public async Task OpenObjectInformation(object comObj, string defaultName)
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
                    props.Add("CLSID", ent.Clsid.FormatGuid());
                    props.Add("Name", ent.Name);
                    props.Add("Server", ent.DefaultServer);
                    await ent.LoadSupportedInterfacesAsync(false);
                    ints = ent.Interfaces.Select(i => m_registry.MapIidToInterface(i.Iid));
                }
                else
                {
                    ints = m_registry.GetInterfacesForObject(comObj);
                    strObjName = defaultName != null ? defaultName : clsid.FormatGuid();
                    props.Add("CLSID", clsid.FormatGuid());
                }

                Type dispType = COMUtilities.GetDispatchTypeInfo(this, comObj);
                HostControl(new ObjectInformation(m_registry, strObjName, comObj, props, ints.ToArray()));
            }
        }

        private async void menuObjectFromSerializedStream_Click(object sender, EventArgs e)
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
                            Guid clsid;
                            object obj = COMUtilities.OleLoadFromStream(stm, out clsid);
                            await HostObject(m_registry.MapClsidToEntry(clsid), obj, obj is IClassFactory);
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

        private string _last_moniker = "Specify Moniker";

        private async Task ParseOrBindMoniker(bool bind)
        {
            using (BuildMonikerForm frm = new BuildMonikerForm(_last_moniker))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        _last_moniker = frm.MonikerString;
                        object comObj = frm.Moniker;
                        if (bind)
                        {
                            Guid iid = COMInterfaceEntry.IID_IUnknown;
                            frm.Moniker.BindToObject(frm.BindContext, null, ref iid, out comObj);
                        }

                        if (comObj != null)
                        {
                            await OpenObjectInformation(comObj, _last_moniker);
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.ShowError(this, ex);
                    }
                }
            }
        }
        
        private async void menuObjectBindMoniker_Click(object sender, EventArgs e)
        {
            await ParseOrBindMoniker(true);
        }

        private string GetSaveFileName(bool save)
        {
            if (save && !String.IsNullOrWhiteSpace(m_registry.FilePath))
            {
                return m_registry.FilePath;
            }

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                if (!String.IsNullOrWhiteSpace(m_registry.FilePath))
                {
                    dlg.FileName = m_registry.FilePath;
                }

                dlg.Filter = "OleViewDotNet DB File (*.ovdb)|*.ovdb|All Files (*.*)|*.*";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    return dlg.FileName;
                }
            }

            return String.Empty;
        }

        private void SaveDatabase(bool save)
        {
            string filename = GetSaveFileName(save);
            if (String.IsNullOrWhiteSpace(filename))
            {
                return;
            }

            try
            {
                m_registry.Save(filename);
                UpdateTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuFileSaveDatabase_Click(object sender, EventArgs e)
        {
            SaveDatabase(true);
        }

        private void menuFileOpenDatabase_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "OleViewDotNet DB File (*.ovdb)|*.ovdb|All Files (*.*)|*.*";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadRegistry(() => COMUtilities.LoadRegistry(this, dlg.FileName));
                }
            }
        }

        private void menuRegistryMimeTypes_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.MimeTypes);
        }

        private void menuRegistryAppIDsWithAC_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.AppIDsWithAC);
        }

        private void menuSecurityDefaultAccess_Click(object sender, EventArgs e)
        {
            COMSecurity.ViewSecurity(this, "Default Access", m_registry.DefaultAccessPermission, true);
        }

        private void menuSecurityDefaultAccessRestriction_Click(object sender, EventArgs e)
        {
            COMSecurity.ViewSecurity(this, "Default Access Restrictions", m_registry.DefaultAccessRestriction, true);
        }

        private void menuSecurityDefaultLaunch_Click(object sender, EventArgs e)
        {
            COMSecurity.ViewSecurity(this, "Default Launch", m_registry.DefaultLaunchPermission, false);
        }

        private void menuSecurityDefaultLaunchRestriction_Click(object sender, EventArgs e)
        {
            COMSecurity.ViewSecurity(this, "Default Launch Restrictions", m_registry.DefaultLaunchRestriction, false);
        }

        public COMRegistry Registry { get { return m_registry; } }

        private void menuRegistryProperties_Click(object sender, EventArgs e)
        {
            HostControl(new RegistryPropertiesControl(m_registry));
        }

        private void LoadRegistry(Func<COMRegistry> loader)
        {
            try
            {
                new MainForm(loader()).Show();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuFileOpenMachineOnly_Click(object sender, EventArgs e)
        {
            LoadRegistry(() => COMUtilities.LoadRegistry(this, COMRegistryMode.MachineOnly));
        }

        private void menuFileOpenUserOnly_Click(object sender, EventArgs e)
        {
            LoadRegistry(() => COMUtilities.LoadRegistry(this, COMRegistryMode.UserOnly));
        }

        private void menuFileDiff_Click(object sender, EventArgs e)
        {
            using (DiffRegistryForm frm = new DiffRegistryForm(m_registry))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    new MainForm(frm.DiffRegistry).Show();
                }
            }
        }

        private async void menuObjectFromFile_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Filter = "All Files (*.*)|*.*";
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        COMCLSIDEntry entry = m_registry.GetFileClass(dlg.FileName);
                        if (entry != null)
                        {
                            IPersistFile ps = (IPersistFile)entry.CreateInstanceAsObject(entry.CreateContext, null);
                            ps.Load(dlg.FileName, (int)STGM.READ);
                            await HostObject(entry, ps, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void menuObjectParseMoniker_Click(object sender, EventArgs e)
        {
            await ParseOrBindMoniker(false);
        }

        private void OpenHexEditor(string name, byte[] bytes)
        {
            ObjectHexEditor editor = new ObjectHexEditor(m_registry, name, bytes);
            HostControl(editor);
        }

        private void menuHexEditorFromFile_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Multiselect = true;
                    dlg.Filter = "All Files (*.*)|*.*";
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        foreach(string file in dlg.FileNames)
                        {
                            OpenHexEditor(Path.GetFileName(file), 
                                File.ReadAllBytes(file));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuHexEditorEmpty_Click(object sender, EventArgs e)
        {
            OpenHexEditor("Empty", new byte[0]);
        }

        private void menuFileOpenTypeLib_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "TLB Files (*.tlb)|*.tlb|Executable Files (*.exe;*.dll;*.ocx)|*.exe;*.dll;*.ocx|All Files (*.*)|*.*";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    Assembly typelib = COMUtilities.LoadTypeLib(this, dlg.FileName);
                    if (typelib != null)
                    {
                        HostControl(new TypeLibControl(Path.GetFileName(dlg.FileName), typelib, Guid.Empty));
                    }
                }
            }
        }

        private void menuRegistryInterfaceProxies_Click(object sender, EventArgs e)
        {
            OpenView(COMRegistryViewer.DisplayMode.ProxyCLSIDs);
        }

        private void menuFileOpenProxyDll_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Executable Files (*.dll;*.ocx)|*.dll;*.ocx|All Files (*.*)|*.*";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        COMProxyInstance proxy = COMProxyInstance.GetFromFile(dlg.FileName);
                        HostControl(new TypeLibControl(m_registry, Path.GetFileName(dlg.FileName), proxy, Guid.Empty));
                    }
                    catch (Exception ex)
                    {
                        Program.ShowError(this, ex);
                    }
                }
            }
        }

        private void menuFileQueryAllInterfaces_Click(object sender, EventArgs e)
        {
            using (QueryInterfacesOptionsForm options = new QueryInterfacesOptionsForm())
            {
                if (options.ShowDialog(this) == DialogResult.OK)
                {
                    COMUtilities.QueryAllInterfaces(this, m_registry.Clsids.Values, 
                        options.ServerTypes, options.ConcurrentQueries,
                        options.RefreshInterfaces);
                }
            }
        }

        private void menuFileSaveAsDatabase_Click(object sender, EventArgs e)
        {
            SaveDatabase(false);
        }

        private void menuFileConfigureSymbols_Click(object sender, EventArgs e)
        {
            using (ConfigureSymbolsForm frm = new ConfigureSymbolsForm())
            {
                frm.ShowDialog(this);
            }
        }

        private void LoadProcesses<TKey>(Func<COMProcessEntry, TKey> orderby_selector)
        {
            if (!Properties.Settings.Default.SymbolsConfigured)
            {
                if (MessageBox.Show(this, "Symbol support has not been configured, would you like to do that now?",
                    "Configure Symbols", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (ConfigureSymbolsForm frm = new ConfigureSymbolsForm())
                    {
                        frm.ShowDialog(this);
                    }
                }
            }

            IEnumerable<COMProcessEntry> processes = COMUtilities.LoadProcesses(this);
            if (processes != null && processes.Count() > 0)
            {
                OpenView(COMRegistryViewer.DisplayMode.Processes, processes.OrderBy(orderby_selector));
            }
        }

        private void menuObjectProcessesByPid_Click(object sender, EventArgs e)
        {
            LoadProcesses(p => p.Pid);
        }

        private void menuObjectProcessesByName_Click(object sender, EventArgs e)
        {
            LoadProcesses(p => p.Name);
        }

        private void menuObjectProcessByUser_Click(object sender, EventArgs e)
        {
            LoadProcesses(p => p.User);
        }

        private void menuItem11_Click(object sender, EventArgs e)
        {

        }
    }
}
