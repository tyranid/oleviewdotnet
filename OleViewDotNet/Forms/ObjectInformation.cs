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

using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

/// <summary>
/// Form to display basic information about an object
/// </summary>
internal partial class ObjectInformation : UserControl
{
    private readonly ObjectEntry m_pEntry;
    private readonly object m_pObject;
    private readonly Dictionary<string, string> m_properties;
    private readonly COMInterfaceEntry[] m_interfaces;
    private readonly string m_objName;
    private readonly COMRegistry m_registry;
    private readonly ICOMClassEntry m_entry;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="objName">Descriptive name of the object</param>
    /// <param name="pObject">Managed wrapper to the object</param>
    /// <param name="properties">List of textual properties to display</param>
    /// <param name="interfaces">List of available interfaces</param>
    public ObjectInformation(COMRegistry registry, ICOMClassEntry entry, string objName, object pObject, Dictionary<string, string> properties, COMInterfaceEntry[] interfaces)
    {
        m_entry = entry;
        if (m_entry is null)
        {
            Guid clsid = COMUtilities.GetObjectClass(pObject);
            if (registry.Clsids.ContainsKey(clsid))
            {
                m_entry = registry.MapClsidToEntry(clsid);
            }
        }

        m_registry = registry;
        m_pEntry = ObjectCache.Add(objName, pObject, interfaces);
        m_pObject = pObject;
        m_properties = properties;
        m_interfaces = interfaces.OrderBy(i => i.Name).ToArray();
        m_objName = objName;

        InitializeComponent();

        LoadProperties();
        LoadInterfaces();
        Text = m_objName;
        listViewInterfaces.ListViewItemSorter = new ListItemComparer(0);
    }

    /// <summary>
    /// Load the textual properties into a list box
    /// </summary>
    private void LoadProperties()
    {
        listViewProperties.Columns.Add("Key");
        listViewProperties.Columns.Add("Value");

        foreach (KeyValuePair<string, string> pair in m_properties)
        {
            ListViewItem item = listViewProperties.Items.Add(pair.Key);
            item.SubItems.Add(pair.Value);
        }

        try
        {
            /* Also add IObjectSafety information if available */
            if (m_pObject is IObjectSafety objSafety)
            {
                Guid iid = COMKnownGuids.IID_IDispatch;

                objSafety.GetInterfaceSafetyOptions(iid, out uint supportedOptions, out uint enabledOptions);
                for (int i = 0; i < 4; i++)
                {
                    int val = 1 << i;
                    if ((val & supportedOptions) != 0)
                    {
                        ListViewItem item = listViewProperties.Items.Add(Enum.GetName(typeof(ObjectSafetyFlags), val));
                    }
                }
            }
        }
        catch
        {
        }

        ServerInformation? info = COMUtilities.GetServerInformation(m_pObject, m_interfaces);
        if (info.HasValue)
        {
            listViewProperties.Items.Add("Server PID").SubItems.Add(info.Value.dwServerPid.ToString());
            listViewProperties.Items.Add("Server TID").SubItems.Add(info.Value.dwServerTid.ToString());
            listViewProperties.Items.Add("Server Address").SubItems.Add($"0x{info.Value.ui64ServerAddress:X}");
        }

        listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
    }

    /// <summary>
    /// Load interface list into list box
    /// </summary>
    private void LoadInterfaces()
    {
        listViewInterfaces.Columns.Add("Name");
        listViewInterfaces.Columns.Add("IID");
        listViewInterfaces.Columns.Add("Viewer");

        bool has_dispatch = false;
        bool has_olecontrol = false;
        bool has_persiststream = false;
        bool has_classfactory = false;

        foreach (COMInterfaceEntry ent in m_interfaces)
        {
            ListViewItem item = listViewInterfaces.Items.Add(ent.Name);
            item.Tag = ent;
            item.SubItems.Add(ent.Iid.FormatGuid());

            InterfaceViewers.ITypeViewerFactory factory = InterfaceViewers.InterfaceViewers.GetInterfaceViewer(ent.Iid);
            if (factory is not null)
            {
                item.SubItems.Add("Yes");
            }
            else
            {
                item.SubItems.Add("No");
            }

            if (ent.IsDispatch)
            {
                has_dispatch = true;
            }
            else if (ent.IsOleControl)
            {
                has_olecontrol = true;
            }
            else if (ent.IsPersistStream)
            {
                has_persiststream = true;
            }
            else if (ent.IsClassFactory)
            {
                has_classfactory = true;
            }
        }

        openDispatchToolStripMenuItem.Visible = has_dispatch;
        openOLEToolStripMenuItem.Visible = has_olecontrol;
        saveStreamToolStripMenuItem.Visible = has_persiststream;
        createToolStripMenuItem.Visible = has_classfactory;

        listViewInterfaces.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewInterfaces.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void listViewInterfaces_DoubleClick(object sender, EventArgs e)
    {
        if (listViewInterfaces.SelectedItems.Count > 0)
        {
            COMInterfaceEntry ent = (COMInterfaceEntry)listViewInterfaces.SelectedItems[0].Tag;
            InterfaceViewers.ITypeViewerFactory factory = InterfaceViewers.InterfaceViewers.GetInterfaceViewer(ent.Iid);

            try
            {
                if (factory is not null)
                {
                    Control frm = factory.CreateInstance(m_registry, m_entry, m_objName, m_pEntry);
                    if ((frm is not null) && !frm.IsDisposed)
                    {
                        EntryPoint.GetMainForm(m_registry).HostControl(frm);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void btnOleContainer_Click(object sender, EventArgs e)
    {
        Control frm = new ObjectContainer(m_objName, m_pObject);
        if ((frm is not null) && !frm.IsDisposed)
        {
            EntryPoint.GetMainForm(m_registry).HostControl(frm);
        }
    }

    private void btnDispatch_Click(object sender, EventArgs e)
    {
        Type disp_type = COMUtilities.GetDispatchTypeInfo(this, m_pObject);
        if (disp_type is not null)
        {
            Control frm = new TypedObjectViewer(m_registry, m_objName, m_pEntry, disp_type);
            if ((frm is not null) && !frm.IsDisposed)
            {
                EntryPoint.GetMainForm(m_registry).HostControl(frm);
            }
        }
    }

    private void btnSaveStream_Click(object sender, EventArgs e)
    {
        try
        {
            using MemoryStream stm = new();
            COMUtilities.OleSaveToStream(m_pObject, stm);
            EntryPoint.GetMainForm(m_registry).HostControl(new ObjectHexEditor(m_registry, "Stream Editor", stm.ToArray()));
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private Guid GetSelectedIID()
    {
        if (listViewInterfaces.SelectedItems.Count > 0)
        {
            if (listViewInterfaces.SelectedItems[0].Tag is COMInterfaceEntry ent)
            {
                return ent.Iid;
            }
        }
        return COMKnownGuids.IID_IUnknown;
    }

    private void btnMarshal_Click(object sender, EventArgs e)
    {
        try
        {
            EntryPoint.GetMainForm(m_registry).HostControl(new ObjectHexEditor(m_registry,
                "Marshal Editor", COMUtilities.MarshalObject(m_pObject, GetSelectedIID(),
                MSHCTX.DIFFERENTMACHINE, MSHLFLAGS.NORMAL)));
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private void listViewInterfaces_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        ListItemComparer.UpdateListComparer(sender as ListView, e.Column);
    }

    private void btnCreate_Click(object sender, EventArgs e)
    {
        try
        {
            IClassFactory factory = (IClassFactory)m_pObject;
            Dictionary<string, string> props = new();
            props.Add("Name", m_objName);
            factory.CreateInstance(null, COMKnownGuids.IID_IUnknown, out object new_object);
            ObjectInformation view = new(m_registry,
                m_entry, m_objName, new_object,
                props, m_registry.GetInterfacesForObject(new_object).ToArray());
            EntryPoint.GetMainForm(m_registry).HostControl(view);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void viewPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            EntryPoint.GetMainForm(m_registry).HostControl(new MarshalEditorControl(m_registry,
                COMUtilities.MarshalObjectToObjRef(m_pObject, GetSelectedIID(),
                MSHCTX.DIFFERENTMACHINE, MSHLFLAGS.NORMAL)));
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private void viewInterfaceToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            if (COMUtilities.MarshalObjectToObjRef(m_pObject,
                GetSelectedIID(), MSHCTX.DIFFERENTMACHINE, MSHLFLAGS.NORMAL) is not COMObjRefStandard objref)
            {
                throw new Exception("Object must be standard marshaled to view the interface");
            }

            EntryPoint.GetMainForm(m_registry).LoadIPid(objref.Ipid);
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private void viewServerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            if (COMUtilities.MarshalObjectToObjRef(m_pObject,
                    GetSelectedIID(), MSHCTX.DIFFERENTMACHINE, MSHLFLAGS.NORMAL) is not COMObjRefStandard objref)
            {
                throw new Exception("Object must be standard marshaled to view the interface");
            }

            EntryPoint.GetMainForm(m_registry).LoadProcessByProcessId(objref.ProcessId);
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }
}
