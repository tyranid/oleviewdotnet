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
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

/// <summary>
/// Generic interface viewer from a type
/// </summary>
internal partial class TypedObjectViewer : UserControl
{
    private readonly string m_objName;
    private readonly ObjectEntry m_pEntry;
    private readonly object m_pObject;
    private readonly Type m_dispType;
    private readonly COMRegistry m_registry;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="strObjName">Descriptive name of the object</param>
    /// <param name="pEntry">Instance of the object</param>
    /// <param name="dispType">Reflected type</param>
    public TypedObjectViewer(COMRegistry registry, string strObjName, ObjectEntry pEntry, Type dispType)
    {
        m_pEntry = pEntry;
        m_pObject = pEntry.Instance;
        m_objName = strObjName;
        m_dispType = dispType;
        m_registry = registry;
        InitializeComponent();

        LoadDispatch();
        Text = $"{m_objName} {m_dispType.Name}";
    }

    public TypedObjectViewer(COMRegistry registry, string strObjName, object pObject, Type dispType)
        : this(registry, strObjName, new ObjectEntry(registry, strObjName, pObject), dispType)
    {
    }

    private void LoadDispatch()
    {
        listViewMethods.Columns.Add("Name");
        listViewMethods.Columns.Add("Return");
        listViewMethods.Columns.Add("Params");
        listViewProperties.Columns.Add("Name");
        listViewProperties.Columns.Add("Type");
        listViewProperties.Columns.Add("Value");
        listViewProperties.Columns.Add("Writeable");

        lblName.Text = m_dispType.Name;
        MemberInfo[] members = m_dispType.GetMembers();
        foreach (MemberInfo info in members)
        {
            if (info.MemberType == MemberTypes.Method)
            {
                MethodInfo mi = (MethodInfo)info;

                if (!mi.IsSpecialName)
                {
                    ListViewItem item = listViewMethods.Items.Add(mi.Name);
                    item.Tag = mi;
                    List<string> pars = new();
                    item.SubItems.Add(mi.ReturnType.ToString());
                    ParameterInfo[] pis = mi.GetParameters();

                    foreach (ParameterInfo pi in pis)
                    {
                        string strDir = "";

                        if (pi.IsIn)
                        {
                            strDir += "in";
                        }

                        if (pi.IsOut)
                        {
                            strDir += "out";
                        }

                        if (strDir.Length == 0)
                        {
                            strDir = "in";
                        }

                        if (pi.IsRetval)
                        {
                            strDir += " retval";
                        }

                        if (pi.IsOptional)
                        {
                            strDir += " optional";
                        }

                        pars.Add($"[{strDir}] {pi.ParameterType.Name} {pi.Name}");
                    }

                    item.SubItems.Add(string.Join(", ", pars));
                }
            }
            else if (info.MemberType == MemberTypes.Property)
            {
                PropertyInfo pi = (PropertyInfo)info;
                ListViewItem item = listViewProperties.Items.Add(pi.Name);
                item.Tag = pi;
                item.SubItems.Add(pi.PropertyType.ToString());

                object val = null;

                try
                {
                    if (pi.CanRead)
                    {
                        val = pi.GetValue(m_pObject, null);
                    }
                }
                catch (Exception)
                {
                    val = null;
                }

                if (val is not null)
                {
                    item.SubItems.Add(val.ToString());
                }
                else
                {
                    item.SubItems.Add("<null>");
                }

                item.SubItems.Add(pi.CanWrite.ToString());
            }
        }

        listViewMethods.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewMethods.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

        listViewMethods.ListViewItemSorter = new ListItemComparer(0);
        listViewMethods.Sort();
        listViewProperties.ListViewItemSorter = new ListItemComparer(0);
        listViewProperties.Sort();
    }

    private void UpdateProperties()
    {
        foreach (ListViewItem item in listViewProperties.Items)
        {
            PropertyInfo pi = (PropertyInfo)item.Tag;
            object val = null;

            try
            {
                if (pi.CanRead)
                {
                    val = pi.GetValue(m_pObject, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                val = null;
            }

            if (val is not null)
            {
                item.SubItems[2].Text = val.ToString();
            }
            else
            {
                item.SubItems[2].Text = "<null>";
            }
        }
        listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
    {
        UpdateProperties();
    }

    private void OpenObject(ListView lv)
    {
        if (lv.SelectedItems.Count > 0)
        {
            ListViewItem item = lv.SelectedItems[0];
            if (item.Tag is PropertyInfo)
            {
                PropertyInfo pi = (PropertyInfo)item.Tag;
                object val = null;

                try
                {
                    if (pi.CanRead)
                    {
                        val = pi.GetValue(m_pObject, null);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    val = null;
                }

                if (val is not null)
                {
                    EntryPoint.GetMainForm(m_registry).HostControl(new TypedObjectViewer(m_registry, m_objName, val, pi.PropertyType));
                }
            }
        }
    }

    private void openObjectToolStripMenuItem_Click(object sender, EventArgs e)
    {
        OpenObject(listViewProperties);
    }

    private void openObjectToolStripMenuItem1_Click(object sender, EventArgs e)
    {
        OpenObject(listViewMethods);
    }

    private void listViewMethods_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (listViewMethods.SelectedItems.Count > 0)
        {
            using (InvokeForm frm = new(m_registry, (MethodInfo)listViewMethods.SelectedItems[0].Tag, m_pObject, m_objName))
            {
                frm.ShowDialog();
            }

            UpdateProperties();
        }
    }

    private void listViewProperties_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (listViewProperties.SelectedItems.Count > 0)
        {
            PropertyInfo pi = (PropertyInfo)listViewProperties.SelectedItems[0].Tag;

            if (pi.CanWrite)
            {
                object val = null;

                try
                {
                    if (pi.CanRead)
                    {
                        val = pi.GetValue(m_pObject, null);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    val = null;
                }

                using GetTypeForm frm = new(pi.PropertyType, val);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pi.SetValue(m_pObject, frm.Data, null);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                    UpdateProperties();
                }
            }
        }
    }

    private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        ListItemComparer.UpdateListComparer(sender as ListView, e.Column);
    }
}
