//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
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

using NtApiDotNet.Forms;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Processes;
using OleViewDotNet.Proxy;
using OleViewDotNet.Security;
using OleViewDotNet.TypeLib;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class PropertiesControl : UserControl
{
    private readonly COMRegistry m_registry;
    private COMAppIDEntry m_appid;
    private COMCLSIDEntry m_clsid;
    private COMInterfaceEntry m_interface;
    private COMTypeLibVersionEntry m_typelib;
    private COMProcessEntry m_process;
    private COMRuntimeClassEntry m_runtime_class;
    private COMRuntimeServerEntry m_runtime_server;
    private readonly object m_obj;
    private COMIPIDEntry m_ipid;

    private void LoadInterfaceList(IEnumerable<COMInterfaceInstance> entries, ListView view)
    {
        view.Items.Clear();
        foreach (Tuple<COMInterfaceInstance, COMInterfaceEntry> entry in
            entries.Select(e => new Tuple<COMInterfaceInstance, COMInterfaceEntry>(e, m_registry.MapIidToInterface(e.Iid))).OrderBy(e => e.Item2.Name))
        {
            ListViewItem item = view.Items.Add(entry.Item2.Name);
            item.SubItems.Add(entry.Item1.Iid.FormatGuid());
            item.SubItems.Add(entry.Item2.NumMethods.ToString());
            if (!string.IsNullOrWhiteSpace(entry.Item1.Module))
            {
                item.SubItems.Add($"{entry.Item1.Module}+0x{entry.Item1.VTableOffset:X}");
            }
            item.Tag = entry;
        }
        view.ListViewItemSorter = new ListItemComparer(0);
        view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private static string GetStringValue(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "N/A" : value;
    }

    private static string GetGuidValue(Guid guid)
    {
        return guid == Guid.Empty ? "N/A" : guid.FormatGuid();
    }

    private void SetupAppIdEntry(COMAppIDEntry entry, bool setup_security)
    {
        textBoxAppIdName.Text = entry.Name;
        textBoxAppIdGuid.Text = entry.AppId.FormatGuid();
        textBoxLaunchPermission.Text = entry.LaunchPermission?.ToSddl() ?? string.Empty;
        textBoxAccessPermission.Text = entry.AccessPermission?.ToSddl() ?? string.Empty;
        textBoxAppIDRunAs.Text = GetStringValue(entry.RunAs);
        textBoxAppIDService.Text = GetStringValue(entry.IsService ? entry.LocalService.Name : null);
        textBoxAppIDFlags.Text = entry.Flags.ToString();
        textBoxDllSurrogate.Text = GetStringValue(entry.DllSurrogate);
        tabControlProperties.TabPages.Add(tabPageAppID);

        if (entry.IsService)
        {
            textBoxServiceName.Text = entry.LocalService.Name;
            textBoxServiceDisplayName.Text = GetStringValue(entry.LocalService.DisplayName);
            textBoxServiceType.Text = entry.LocalService.ServiceType.ToString();
            textBoxServiceImagePath.Text = entry.LocalService.ImagePath;
            textBoxServiceDll.Text = GetStringValue(entry.LocalService.ServiceDll);
            textBoxServiceUserName.Text = GetStringValue(entry.LocalService.UserName);
            textBoxServiceProtectionLevel.Text = entry.LocalService.ProtectionLevel.ToString();
            tabControlProperties.TabPages.Add(tabPageService);
        }

        if (entry.HasAccessPermission && setup_security)
        {
            tabControlProperties.TabPages.Add(tabPageAccessSecurity);
            COMSecurity.SetupSecurityDescriptorControl(securityDescriptorViewerAccessSecurity, entry.AccessPermission, true);
        }

        if (entry.HasLaunchPermission)
        {
            tabControlProperties.TabPages.Add(tabPageLaunchSecurity);
            COMSecurity.SetupSecurityDescriptorControl(securityDescriptorViewerLaunchSecurity, entry.LaunchPermission, false);
        }

        m_appid = entry;
    }

    private void SetupClsidEntry(COMCLSIDEntry entry)
    {
        textBoxClsidName.Text = entry.Name;
        textBoxClsid.Text = entry.Clsid.FormatGuid();
        textBoxServerType.Text = entry.DefaultServerType.ToString();
        textBoxThreadingModel.Text = entry.DefaultThreadingModel.ToString();
        textBoxServer.Text = entry.DefaultServer;
        textBoxCmdLine.Text = GetStringValue(entry.DefaultCmdLine);
        textBoxTreatAs.Text = GetGuidValue(entry.TreatAs);
        btnTreatAsProps.Enabled = m_registry.Clsids.ContainsKey(entry.TreatAs);
        var progids = m_registry.Progids;

        foreach (string progid in m_registry.GetProgIdsForClsid(entry.Clsid).Select(p => p.ProgID))
        {
            ListViewItem item = listViewProgIDs.Items.Add(progid);
            if (progids.ContainsKey(progid))
            {
                item.Tag = progids[progid];
            }
        }
        listViewProgIDs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

        foreach (Guid catid in entry.Categories)
        {
            ListViewItem item = listViewCategories.Items.Add(COMUtilities.GetCategoryName(catid));
            item.Tag = catid;
        }
        listViewCategories.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

        LoadInterfaceList(entry.Interfaces, listViewInterfaces);
        LoadInterfaceList(entry.FactoryInterfaces, listViewFactoryInterfaces);
        tabPageSupportedInterfaces.Tag = entry;

        tabControlProperties.TabPages.Add(tabPageClsid);
        tabControlProperties.TabPages.Add(tabPageSupportedInterfaces);
        if (m_registry.AppIDs.ContainsKey(entry.AppID))
        {
            SetupAppIdEntry(m_registry.AppIDs[entry.AppID], true);
        }

        IEnumerable<COMInterfaceEntry> proxies = m_registry.GetProxiesForClsid(entry);
        if (proxies.Any())
        {
            foreach (COMInterfaceEntry intf in proxies.OrderBy(i => i.Name))
            {
                ListViewItem item = listViewProxies.Items.Add(intf.Name);
                item.SubItems.Add(intf.Iid.FormatGuid());
                item.Tag = intf;
            }
            listViewProxies.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewProxies.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewProxies.ListViewItemSorter = new ListItemComparer(0);
            tabControlProperties.TabPages.Add(tabPageProxies);
        }

        if (entry.Servers.Count > 1)
        {
            foreach (COMCLSIDServerEntry server in entry.Servers.Values)
            {
                ListViewItem item = listViewCLSIDServers.Items.Add(server.ServerType.ToString());
                item.SubItems.Add(server.Server);
                item.SubItems.Add(server.CommandLine);
                item.SubItems.Add(server.ThreadingModel.ToString());
                item.Tag = server;
            }

            listViewCLSIDServers.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewCLSIDServers.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewCLSIDServers.ListViewItemSorter = new ListItemComparer(0);
            tabControlProperties.TabPages.Add(tabPageServers);
        }

        SetupTypeLibVersionEntry(m_registry.GetTypeLibVersionEntry(entry.TypeLib, null));

        if (entry.Elevation is not null)
        {
            textBoxElevationEnabled.Text = entry.Elevation.Enabled.ToString();
            textBoxElevationAutoApproval.Text = entry.Elevation.AutoApproval.ToString();
            textBoxElevationIconReference.Text = GetStringValue(entry.Elevation.IconReference);
            foreach (COMCLSIDEntry vso in entry.Elevation.VirtualServerObjects.Select(v => m_registry.MapClsidToEntry(v)))
            {
                ListViewItem item = listViewElevationVSOs.Items.Add(vso.Name);
                item.SubItems.Add(vso.Clsid.ToString());
                item.SubItems.Add(vso.CanElevate.ToString());
                item.SubItems.Add(vso.AutoElevation.ToString());
                item.Tag = vso;
            }
            listViewElevationVSOs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewElevationVSOs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewElevationVSOs.ListViewItemSorter = new ListItemComparer(0);
            tabControlProperties.TabPages.Add(tabPageElevation);
        }

        if (entry.Servers.ContainsKey(COMServerType.InProcServer32) && entry.Servers[COMServerType.InProcServer32].HasDotNet)
        {
            COMCLSIDServerDotNetEntry dotnet = entry.Servers[COMServerType.InProcServer32].DotNet;
            textBoxDotNetAssemblyName.Text = dotnet.AssemblyName;
            textBoxDotNetClassName.Text = dotnet.ClassName;
            textBoxDotNetCodeBase.Text = dotnet.CodeBase;
            textBoxDotNetRuntimeVersion.Text = dotnet.RuntimeVersion;
            tabControlProperties.TabPages.Add(tabPageDotNet);
        }

        m_clsid = entry;
    }

    private void SetupInterfaceEntry(COMInterfaceEntry entry)
    {
        textBoxInterfaceName.Text = entry.Name;
        textBoxIID.Text = GetGuidValue(entry.Iid);
        textBoxInterfaceBase.Text = GetStringValue(entry.Base);
        textBoxInterfaceProxy.Text = GetGuidValue(entry.ProxyClsid);
        txtMethods.Text = entry.NumMethods.ToString();
        btnProxyProperties.Enabled = m_registry.Clsids.ContainsKey(entry.ProxyClsid);
        tabControlProperties.TabPages.Add(tabPageInterface);
        SetupTypeLibVersionEntry(m_registry.GetTypeLibVersionEntry(entry.TypeLib, entry.TypeLibVersion));
        m_interface = entry;
    }

    private void SetupRuntimeServerEntry(COMRuntimeServerEntry entry, bool setup_security)
    {
        textBoxRuntimeServerName.Text = entry.Name;
        textBoxRuntimeServerExePath.Text = GetStringValue(entry.ExePath);
        textBoxRuntimeServerPermissions.Text = GetStringValue(entry.Permissions?.ToSddl());
        textBoxRuntimeServerServiceName.Text = GetStringValue(entry.ServiceName);
        textBoxRuntimeServerType.Text = entry.ServerType.ToString();
        textBoxRuntimeServerIdentity.Text = GetStringValue(entry.Identity);
        textBoxRuntimeServerIdentityType.Text = entry.IdentityType.ToString();
        textBoxRuntimeServerInstancing.Text = entry.InstancingType.ToString();
        m_runtime_server = entry;
        tabControlProperties.TabPages.Add(tabPageRuntimeServer);
        if (entry.HasPermission && setup_security)
        {
            tabControlProperties.TabPages.Add(tabPageLaunchSecurity);
            COMSecurity.SetupSecurityDescriptorControl(securityDescriptorViewerLaunchSecurity, entry.Permissions, false);
        }
    }

    private void SetupRuntimeClassEntry(COMRuntimeClassEntry entry)
    {
        textBoxRuntimeClassName.Text = entry.Name;
        textBoxRuntimeClassCLSID.Text = GetGuidValue(entry.Clsid);
        textBoxRuntimeClassServer.Text = GetStringValue(entry.Server);
        textBoxRuntimeClassPermissions.Text = GetStringValue(entry.Permissions?.ToSddl());
        textBoxRuntimeClassDllPath.Text = GetStringValue(entry.DllPath);
        textBoxRuntimeClassActivationType.Text = entry.ActivationType.ToString();
        textBoxRuntimeClassTrustLevel.Text = entry.TrustLevel.ToString();
        textBoxRuntimeClassThreading.Text = entry.Threading.ToString();
        LoadInterfaceList(entry.Interfaces, listViewInterfaces);
        LoadInterfaceList(entry.FactoryInterfaces, listViewFactoryInterfaces);
        tabPageSupportedInterfaces.Tag = entry;
        m_runtime_class = entry;
        tabControlProperties.TabPages.Add(tabPageRuntimeClass);
        tabControlProperties.TabPages.Add(tabPageSupportedInterfaces);

        if (entry.HasPermission)
        {
            tabControlProperties.TabPages.Add(tabPageLaunchSecurity);
            COMSecurity.SetupSecurityDescriptorControl(securityDescriptorViewerLaunchSecurity, entry.Permissions, false);
        }

        COMRuntimeServerEntry server = m_registry.MapRuntimeClassToServerEntry(entry);
        if (server is not null)
        {
            SetupRuntimeServerEntry(server, !entry.HasPermission);
        }
    }

    private void SetupProperties(object obj)
    {
        if (obj is COMCLSIDEntry clsid)
        {
            SetupClsidEntry(clsid);
        }

        if (obj is COMProgIDEntry prog_id)
        {
            COMCLSIDEntry clsid_entry = m_registry.MapClsidToEntry(prog_id.Clsid);
            SetupClsidEntry(clsid_entry);
        }

        if (obj is COMAppIDEntry appid)
        {
            SetupAppIdEntry(appid, true);
        }

        if (obj is COMInterfaceEntry intf)
        {
            SetupInterfaceEntry(intf);
        }

        if (obj is COMTypeLibVersionEntry lib)
        {
            SetupTypeLibVersionEntry(lib);
        }

        if (obj is COMProcessEntry proc)
        {
            SetupProcessEntry(proc);
        }

        if (obj is COMIPIDEntry ipid)
        {
            SetupIPIDEntry(ipid);
        }

        if (obj is COMRuntimeClassEntry rt_class)
        {
            SetupRuntimeClassEntry(rt_class);
        }

        if (obj is COMRuntimeServerEntry rt_server)
        {
            SetupRuntimeServerEntry(rt_server, true);
        }
    }

    private void SetupIpidEntries(IEnumerable<COMIPIDEntry> ipids, bool show_disconnected)
    {
        listViewProcessIPids.Items.Clear();
        listViewProcessIPids.Items.AddRange(ipids.Where(ipid => ipid.IsRunning || show_disconnected).Select(ipid =>
        {
            ListViewItem item = new(ipid.Ipid.ToString());
            item.SubItems.Add(m_registry.MapIidToInterface(ipid.Iid).Name);
            item.SubItems.Add(ipid.Flags.ToString());
            item.Tag = ipid;
            return item;
        }).ToArray());
        listViewProcessIPids.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewProcessIPids.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void SetupProcessEntry(COMProcessEntry obj)
    {
        m_process = obj;
        textBoxProcessExecutablePath.Text = obj.ExecutablePath;
        textBoxProcessProcessId.Text = obj.ProcessId.ToString();
        textBoxProcessAppId.Text = GetGuidValue(obj.AppId);
        textBoxProcessAccessPermissions.Text = GetStringValue(obj.AccessPermissions?.ToSddl());
        textBoxProcessLrpcPermissions.Text = GetStringValue(obj.LRpcPermissions?.ToSddl());
        textBoxProcessUser.Text = GetStringValue(obj.User);
        textBoxProcessSecurity.Text = $"Capabilities: {obj.Capabilities}, Authn Level: {obj.AuthnLevel}, Imp Level: {obj.ImpLevel}, Unmarshal Policy: {obj.UnmarshalPolicy}";
        textBoxProcessStaHwnd.Text = $"0x{obj.STAMainHWnd.ToInt64():X}";
        SetupIpidEntries(obj.Ipids, false);
        listViewProcessIPids.ListViewItemSorter = new ListItemComparer(0);
        lblProcess64bit.Text = COMUtilities.FormatBitness(obj.Is64Bit);
        tabControlProperties.TabPages.Add(tabPageProcess);
        if (m_registry.AppIDs.ContainsKey(obj.AppId))
        {
            SetupAppIdEntry(m_registry.AppIDs[obj.AppId], obj.AccessPermissions is null);
        }
        if (obj.Classes.Any())
        {
            tabControlProperties.TabPages.Add(tabPageRegisteredClasses);
            foreach (var c in obj.Classes)
            {
                COMCLSIDEntry clsid = m_registry.MapClsidToEntry(c.Clsid);
                ListViewItem item = listViewRegisteredClasses.Items.Add(c.Clsid.FormatGuid());
                item.SubItems.Add(clsid.Name);
                item.SubItems.Add(c.VTable);
                item.SubItems.Add(c.RegFlags.ToString());
                item.SubItems.Add(c.Apartment.ToString());
                item.SubItems.Add(c.Context.ToString());
                item.Tag = c;
            }
            listViewRegisteredClasses.ListViewItemSorter = new ListItemComparer(0);
            listViewRegisteredClasses.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewRegisteredClasses.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
        if (obj.AccessPermissions is not null)
        {
            tabControlProperties.TabPages.Add(tabPageAccessSecurity);
            COMSecurity.SetupSecurityDescriptorControl(securityDescriptorViewerAccessSecurity, obj.AccessPermissions, true);
        }
    }

    private void SetupIPIDEntry(COMIPIDEntry obj)
    {
        textBoxIPID.Text = obj.Ipid.FormatGuid();
        textBoxIPIDIID.Text = obj.Iid.FormatGuid();
        textBoxIPIDIIDName.Text = m_registry.MapIidToInterface(obj.Iid).Name;
        textBoxIPIDFlags.Text = obj.Flags.ToString();
        textBoxIPIDInterface.Text = $"0x{obj.Interface.ToInt64():X}";
        textBoxIPIDInterfaceVTable.Text = GetStringValue(obj.InterfaceVTable);
        textBoxIPIDStub.Text = $"0x{obj.Stub.ToInt64():X}";
        textBoxIPIDStubVTable.Text = GetStringValue(obj.StubVTable);
        textBoxIPIDOXID.Text = obj.Oxid.FormatGuid();
        textBoxIPIDReferences.Text = $"Strong: {obj.StrongRefs}, Weak: {obj.WeakRefs}, Private: {obj.PrivateRefs}";

        textBoxIPIDProcessId.Text = COMUtilities.GetProcessIdFromIPid(obj.Ipid).ToString();
        textBoxIPIDApartment.Text = COMUtilities.GetApartmentIdStringFromIPid(obj.Ipid);
        textBoxIPIDStaHwnd.Text = $"0x{obj.ServerSTAHwnd.ToInt64():X}";
        listViewIpidMethods.Items.AddRange(obj.Methods.Select((method, i) =>
        {
            ListViewItem item = new(i.ToString());
            item.SubItems.Add(method.Name);
            item.SubItems.Add(method.Address);
            item.SubItems.Add(method.Symbol);
            int count = method.Procedure is not null ? method.Procedure.Params.Count : 0;
            if (i == 0)
            {
                count = 3;
            }
            item.SubItems.Add(count.ToString());
            return item;
        }
        ).ToArray());
        listViewIpidMethods.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewIpidMethods.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        m_ipid = obj;
        tabControlProperties.TabPages.Add(tabPageIPID);
    }

    private void SetupTypeLibVersionEntry(COMTypeLibVersionEntry entry)
    {
        if (entry is null)
        {
            return;
        }
        textBoxTypeLibName.Text = entry.Name;
        textBoxTypeLibId.Text = GetGuidValue(entry.TypelibId);
        textBoxTypeLibVersion.Text = entry.Version;
        textBoxTypeLibWin32.Text = GetStringValue(entry.Win32Path);
        textBoxTypeLibWin64.Text = GetStringValue(entry.Win64Path);
        m_typelib = entry;
        tabControlProperties.TabPages.Add(tabPageTypeLib);
    }

    public static bool SupportsProperties(object obj)
    {
        return obj is COMCLSIDEntry || obj is COMProgIDEntry || obj is COMAppIDEntry
            || obj is COMInterfaceEntry || obj is COMTypeLibVersionEntry || obj is COMProcessEntry
            || obj is COMIPIDEntry || obj is COMRuntimeClassEntry || obj is COMRuntimeServerEntry;
    }

    public PropertiesControl(COMRegistry registry, string name, object obj)
    {
        m_registry = registry;
        InitializeComponent();
        listViewCategories.Columns.Add("Name", 100);
        listViewProgIDs.Columns.Add("Name", 100);
        listViewInterfaces.Columns.Add("Name", 100);
        listViewInterfaces.Columns.Add("IID", 100);
        listViewInterfaces.Columns.Add("Methods", 100);
        listViewInterfaces.Columns.Add("VTable Offset", 100);
        listViewFactoryInterfaces.Columns.Add("Name", 100);
        listViewFactoryInterfaces.Columns.Add("IID", 100);
        listViewFactoryInterfaces.Columns.Add("Methods", 100);
        listViewFactoryInterfaces.Columns.Add("VTable Offset", 100);
        m_obj = obj;
        this.Text = $"{name.Replace("&", "&&")} Properties";
    }

    private async void btnRefreshInterfaces_Click(object sender, EventArgs e)
    {
        try
        {
            ICOMClassEntry entry = (ICOMClassEntry)tabPageSupportedInterfaces.Tag;
            await entry.LoadSupportedInterfacesAsync(true, null);
            LoadInterfaceList(entry.Interfaces, listViewInterfaces);
            LoadInterfaceList(entry.FactoryInterfaces, listViewFactoryInterfaces);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void copyProgIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (listViewProgIDs.SelectedItems.Count > 0)
        {
            ListViewItem item = listViewProgIDs.SelectedItems[0];
            MiscUtilities.CopyTextToClipboard(item.Text);
        }
    }

    private void btnTreatAsProps_Click(object sender, EventArgs e)
    {
        if (m_registry.Clsids.ContainsKey(m_clsid.TreatAs))
        {
            EntryPoint.GetMainForm(m_registry).HostControl(new PropertiesControl(m_registry,
                m_clsid.Name, m_registry.Clsids[m_clsid.TreatAs]));
        }
    }

    private async void btnCreate_Click(object sender, EventArgs e)
    {
        try
        {
            object comObj = m_clsid.CreateInstanceAsObject(m_clsid.CreateContext, null);
            if (comObj is not null)
            {
                await EntryPoint.GetMainForm(m_registry).HostObject(m_clsid, comObj, false);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnProxyProperties_Click(object sender, EventArgs e)
    {
        if (m_registry.Clsids.ContainsKey(m_interface.ProxyClsid))
        {
            COMCLSIDEntry entry = m_registry.Clsids[m_interface.ProxyClsid];
            EntryPoint.GetMainForm(m_registry).HostControl(new PropertiesControl(m_registry,
                entry.Name, entry));
        }
    }

    private async void btnOpenTypeLib_Click(object sender, EventArgs e)
    {
        if (m_typelib is not null)
        {
            try
            {
                var parsed_typelib = await Task.Run(() => m_typelib.Parse());
                COMTypeLibTypeInfo visible_type = parsed_typelib.Interfaces.FirstOrDefault(i => i.Uuid == m_interface?.Iid);
                visible_type ??= parsed_typelib.Dispatch.FirstOrDefault(i => i.Uuid == m_interface?.Iid);

                EntryPoint.GetMainForm(m_registry).HostControl(new COMRegistryViewer(m_registry, parsed_typelib,
                    visible_type));
            }
            catch (Exception ex)
            {
                EntryPoint.ShowError(this, ex);
            }
        }
    }

    private void copyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (sender is ListView view && view.SelectedIndices.Count > 0)
        {
            ListViewItem item = view.SelectedItems[0];
            MiscUtilities.CopyTextToClipboard(item.Text);
        }
    }

    private void CopyIID(ListView view, GuidFormat type)
    {
        if (view is not null && view.SelectedIndices.Count > 0)
        {
            ListViewItem item = view.SelectedItems[0];
            Tuple<COMInterfaceInstance, COMInterfaceEntry> intf = item.Tag as Tuple<COMInterfaceInstance, COMInterfaceEntry>;
            MiscUtilities.CopyGuidToClipboard(intf.Item1.Iid, type);
        }
    }

    private void asStringToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CopyIID(GetListViewForMenu(sender), GuidFormat.String);
    }

    private void asCStructureToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CopyIID(GetListViewForMenu(sender), GuidFormat.Structure);
    }

    private void asHexStringToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CopyIID(GetListViewForMenu(sender), GuidFormat.HexString);
    }

    private ListView GetListViewForMenu(object sender)
    {
        ContextMenuStrip menu = sender as ContextMenuStrip;

        if (sender is ToolStripMenuItem item)
        {
            menu = item.Owner as ContextMenuStrip;
        }

        if (menu is not null)
        {
            return menu.SourceControl as ListView;
        }
        return null;
    }

    private void contextMenuStripInterfaces_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        ListView view = GetListViewForMenu(sender);
        if (view is not null && view.SelectedIndices.Count > 0)
        {
            ListViewItem item = view.SelectedItems[0];
            Tuple<COMInterfaceInstance, COMInterfaceEntry> intf =
                item.Tag as Tuple<COMInterfaceInstance, COMInterfaceEntry>;
            viewProxyDefinitionToolStripMenuItem.Enabled = m_registry.Clsids.ContainsKey(intf.Item2.ProxyClsid);
        }
        else
        {
            viewProxyDefinitionToolStripMenuItem.Enabled = false;
        }
    }

    private void viewProxyDefinitionToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            ListView view = GetListViewForMenu(sender);
            if (view is not null && view.SelectedIndices.Count > 0)
            {
                ListViewItem item = view.SelectedItems[0];
                Tuple<COMInterfaceInstance, COMInterfaceEntry> intf =
                    item.Tag as Tuple<COMInterfaceInstance, COMInterfaceEntry>;

                if (m_registry.Clsids.TryGetValue(intf.Item2.ProxyClsid, out COMCLSIDEntry clsid))
                {
                    var proxy_file = COMProxyFile.GetFromCLSID(clsid);
                    var visible_type = proxy_file.Entries.FirstOrDefault(e => e.Iid == intf.Item1.Iid);
                    EntryPoint.GetMainForm(m_registry).HostControl(new COMRegistryViewer(m_registry,
                        proxy_file, visible_type));
                }
            }
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private COMIPIDEntry GetSelectedIpid()
    {
        if (listViewProcessIPids.SelectedItems.Count > 0)
        {
            return (COMIPIDEntry)listViewProcessIPids.SelectedItems[0].Tag;
        }
        return null;
    }

    private void copyInterfacePointerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid is not null)
        {
            MiscUtilities.CopyTextToClipboard($"0x{ipid.Interface.ToInt64():X}");
        }
    }

    private void copyStubPointerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid is not null)
        {
            MiscUtilities.CopyTextToClipboard($"0x{ipid.Stub.ToInt64():X}");
        }
    }

    private void toHexEditorToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid is not null)
        {
            EntryPoint.GetMainForm(m_registry).HostControl(new ObjectHexEditor(m_registry, ipid.Ipid.ToString(), ipid.ToObjref()));
        }
    }

    private void toFileToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid is not null)
        {
            using SaveFileDialog dlg = new();
            dlg.Filter = "All Files (*.*)|*.*";
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    File.WriteAllBytes(dlg.FileName, ipid.ToObjref());
                }
                catch (Exception ex)
                {
                    EntryPoint.ShowError(this, ex);
                }
            }
        }
    }

    private async void toObjectToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid is not null)
        {
            try
            {
                await EntryPoint.GetMainForm(m_registry).OpenObjectInformation(
                    COMUtilities.UnmarshalObject(ipid.ToObjref()),
                    $"IPID {ipid.Ipid}");
            }
            catch (Exception ex)
            {
                EntryPoint.ShowError(this, ex);
            }
        }
    }

    private void listViewElevationVSOs_DoubleClick(object sender, EventArgs e)
    {
        if (listViewElevationVSOs.SelectedItems.Count < 1)
        {
            return;
        }

        if (listViewElevationVSOs.SelectedItems[0].Tag is COMCLSIDEntry clsid)
        {
            EntryPoint.GetMainForm(m_registry).HostControl(new PropertiesControl(m_registry,
                    clsid.Name, clsid));
        }
    }

    private void listView_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is ListView list_view && list_view.SelectedItems.Count > 0 && list_view.SelectedItems[0].Tag is not null)
        {
            EntryPoint.GetMainForm(m_registry).UpdatePropertyGrid(list_view.SelectedItems[0].Tag);
        }
    }

    private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        ListItemComparer.UpdateListComparer(sender as ListView, e.Column);
    }

    private void btnViewAssembly_Click(object sender, EventArgs e)
    {
        try
        {
            Assembly asm = null;

            if (!string.IsNullOrWhiteSpace(textBoxDotNetCodeBase.Text))
            {
                asm = Assembly.LoadFrom(textBoxDotNetCodeBase.Text);
            }
            else
            {
                asm = Assembly.Load(textBoxDotNetAssemblyName.Text);
            }

            EntryPoint.GetMainForm(m_registry).HostControl(new TypeLibControl(asm.GetName().Name,
                    asm, m_clsid is not null ? m_clsid.Clsid : Guid.Empty, true));
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private void checkBoxShowDisconnected_CheckedChanged(object sender, EventArgs e)
    {
        SetupIpidEntries(m_process.Ipids, checkBoxShowDisconnected.Checked);
    }

    private void copyIPIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid is not null)
        {
            MiscUtilities.CopyGuidToClipboard(ipid.Ipid, GuidFormat.String);
        }
    }

    private void copyIPIDIIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid is not null)
        {
            MiscUtilities.CopyGuidToClipboard(ipid.Iid, GuidFormat.String);
        }
    }

    private void listViewProcessIPids_DoubleClick(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid is not null)
        {
            EntryPoint.GetMainForm(m_registry).HostControl(new PropertiesControl(m_registry,
                    $"IPID: {ipid.Ipid.FormatGuid()}", ipid));
        }
    }

    private void PropertiesControl_Load(object sender, EventArgs e)
    {
        tabControlProperties.TabPages.Clear();
        SetupProperties(m_obj);
        if (tabControlProperties.TabCount == 0)
        {
            tabControlProperties.TabPages.Add(tabPageNoProperties);
        }
    }

    private void listViewIpidMethods_DoubleClick(object sender, EventArgs e)
    {
        bool has_ndr = false;
        foreach (var method in m_ipid.Methods)
        {
            if (method.Procedure is not null)
            {
                has_ndr = true;
                break;
            }
        }

        if (has_ndr)
        {
            var proxy = m_ipid.ToProxyInstance();
            var visible_type = proxy.Entries.FirstOrDefault(e => e.Iid == m_ipid.Iid);

            EntryPoint.GetMainForm(m_registry).HostControl(new COMRegistryViewer(m_registry, proxy, visible_type));
        }
    }

    private COMProcessClassRegistration GetRegisteredClass()
    {
        if (listViewRegisteredClasses.SelectedItems.Count == 0)
        {
            return null;
        }

        return listViewRegisteredClasses.SelectedItems[0].Tag as COMProcessClassRegistration;
    }

    private void copyCLSIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMProcessClassRegistration c = GetRegisteredClass();
        if (c is not null)
        {
            MiscUtilities.CopyGuidToClipboard(c.Clsid, GuidFormat.String);
        }
    }

    private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMProcessClassRegistration c = GetRegisteredClass();
        if (c is not null && m_registry.Clsids.ContainsKey(c.Clsid))
        {
            COMCLSIDEntry clsid = m_registry.MapClsidToEntry(c.Clsid);
            EntryPoint.GetMainForm(m_registry).HostControl(new PropertiesControl(m_registry, clsid.Name, clsid));
        }
    }

    private void contextMenuStripRegisteredClasses_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        COMProcessClassRegistration c = GetRegisteredClass();
        propertiesToolStripMenuItem.Visible = c is not null && m_registry.Clsids.ContainsKey(c.Clsid);
    }

    private void toClipboardToolStripMenuItem_Click(object sender, EventArgs e)
    {
        COMIPIDEntry ipid = GetSelectedIpid();
        if (ipid is not null)
        {
            var objref = $"objref:{Convert.ToBase64String(ipid.ToObjref())}:";
            MiscUtilities.CopyTextToClipboard(objref);
        }
    }

    private async Task CreateInstance(bool class_factory)
    {
        COMProcessClassRegistration c = GetRegisteredClass();
        if (c is not null)
        {
            await EntryPoint.GetMainForm(m_registry).CreateInstanceFromCLSID(c.Clsid, CLSCTX.LOCAL_SERVER, class_factory);
        }
    }

    private async void createInstanceToolStripMenuItem_Click(object sender, EventArgs e)
    {
        await CreateInstance(false);
    }

    private async void createClassFactoryToolStripMenuItem_Click(object sender, EventArgs e)
    {
        await CreateInstance(true);
    }
}
