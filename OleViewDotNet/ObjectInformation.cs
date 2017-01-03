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
using System.IO;
using System.Windows.Forms;

namespace OleViewDotNet
{
    /// <summary>
    /// Form to display basic information about an object
    /// </summary>
    public partial class ObjectInformation : UserControl
    {
        private ObjectEntry m_pEntry;
        private Object m_pObject;
        private Dictionary<string, string> m_properties;
        private COMInterfaceEntry[] m_interfaces;        
        private string m_objName;
        private COMRegistry m_registry;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objName">Descriptive name of the object</param>
        /// <param name="pObject">Managed wrapper to the object</param>
        /// <param name="properties">List of textual properties to display</param>
        /// <param name="interfaces">List of available interfaces</param>
        public ObjectInformation(COMRegistry registry, string objName, Object pObject, Dictionary<string, string> properties, COMInterfaceEntry[] interfaces)
        {
            m_registry = registry;
            m_pEntry = ObjectCache.Add(registry, objName, pObject, interfaces);
            m_pObject = pObject;
            m_properties = properties;
            m_interfaces = interfaces;            
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
                IObjectSafety objSafety = m_pObject as IObjectSafety;
                if (objSafety != null)
                {
                    uint supportedOptions;
                    uint enabledOptions;
                    Guid iid = COMInterfaceEntry.IID_IDispatch;

                    objSafety.GetInterfaceSafetyOptions(ref iid, out supportedOptions, out enabledOptions);
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

            foreach (COMInterfaceEntry ent in m_interfaces)
            {
                ListViewItem item = listViewInterfaces.Items.Add(ent.Name);
                item.Tag = ent;
                item.SubItems.Add(ent.Iid.ToString("B"));

                InterfaceViewers.ITypeViewerFactory factory = InterfaceViewers.InterfaceViewers.GetInterfaceViewer(ent.Iid);
                if (factory != null)
                {
                    item.SubItems.Add("Yes");
                }
                else
                {
                    item.SubItems.Add("No");
                }

                if (ent.IsDispatch)
                {
                    btnDispatch.Enabled = true;
                }
                else if (ent.IsOleControl)
                {
                    btnOleContainer.Enabled = true;
                }
                else if (ent.IsPersistStream)
                {
                    btnSaveStream.Enabled = true;
                }
                else if (ent.IsMarshal)
                {
                    btnMarshal.Enabled = true;
                }
                else if (ent.IsClassFactory)
                {
                    btnCreate.Enabled = true;
                }
            }

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
                    if (factory != null)
                    {
                        Control frm = factory.CreateInstance(m_registry, m_objName, m_pEntry);
                        if ((frm != null) && !frm.IsDisposed)
                        {
                            Program.GetMainForm(m_registry).HostControl(frm);                            
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
            if ((frm != null) && !frm.IsDisposed)
            {
                Program.GetMainForm(m_registry).HostControl(frm);
            }
        }

        private void btnDispatch_Click(object sender, EventArgs e)
        {
            Type disp_type = COMUtilities.GetDispatchTypeInfo(m_pObject);
            if (disp_type != null)
            {
                Control frm = new TypedObjectViewer(m_registry, m_objName, m_pEntry, disp_type);
                if ((frm != null) && !frm.IsDisposed)
                {
                    Program.GetMainForm(m_registry).HostControl(frm);
                }
            }
        }

        private void btnSaveStream_Click(object sender, EventArgs e)
        {
            try
            {
                using (MemoryStream stm = new MemoryStream())
                {
                    COMUtilities.OleSaveToStream(m_pObject, stm);
                    Program.GetMainForm(m_registry).HostControl(new ObjectHexEditor(m_registry, stm.ToArray()));
                }
            }
            catch (Exception ex)
            {
                Program.ShowError(this, ex);
            }
        }

        private void btnMarshal_Click(object sender, EventArgs e)
        {
            try
            {
                Program.GetMainForm(m_registry).HostControl(new ObjectHexEditor(m_registry, COMUtilities.MarshalObject(m_pObject)));
            }
            catch (Exception ex)
            {
                Program.ShowError(this, ex);
            }   
        }

        private void listViewInterfaces_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView view = sender as ListView;

            if (view != null)
            {
                ListItemComparer comparer = view.ListViewItemSorter as ListItemComparer;

                if (comparer != null)
                {
                    if (e.Column != comparer.Column)
                    {
                        comparer.Column = e.Column;
                        comparer.Ascending = true;
                    }
                    else
                    {
                        comparer.Ascending = !comparer.Ascending;
                    }

                    view.Sort();
                }
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                IClassFactory factory = (IClassFactory)m_pObject;
                object new_object;
                Guid IID_IUnknown = COMInterfaceEntry.IID_IUnknown;
                Dictionary<string, string> props = new Dictionary<string, string>();
                props.Add("Name", m_objName);
                factory.CreateInstance(null, ref IID_IUnknown, out new_object);
                ObjectInformation view = new ObjectInformation(m_registry, m_objName, new_object,
                    props, m_registry.GetInterfacesForObject(new_object));
                Program.GetMainForm(m_registry).HostControl(view);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
