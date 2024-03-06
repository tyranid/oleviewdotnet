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
using OleViewDotNet.Utilities;
using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class TypeLibControl : UserControl
{
    private readonly IEnumerable<ListViewItemWithGuid> m_interfaces;
    private readonly COMSourceCodeBuilder m_builder;
    private Guid m_guid_to_view;

    private const string filterDefaultString = "filter interfaces";

    private class ListViewItemWithGuid : ListViewItem
    {
        public Guid Guid { get; }
        public ListViewItemWithGuid(string name, Guid iid) : base(name)
        {
            Guid = iid;
        }
    }

    private static ListViewItemWithGuid MapTypeToItem(SourceCodeFormattableType type)
    {
        ListViewItemWithGuid item = new(type.Name, type.GUID);
        item.SubItems.Add(type.GUID.FormatGuid());
        item.Tag = type;
        return item;
    }

    private static ListViewItemWithGuid MapTypeToItemNoSubItem(SourceCodeFormattableType type)
    {
        return new(type.Name, type.GUID)
        {
            Tag = type
        };
    }

    private static IEnumerable<ListViewItemWithGuid> FormatInterfaces(SourceCodeFormattableAssembly typelib)
    {
        return typelib.GetComInterfaces().OrderBy(t => t.Name).Select(MapTypeToItem);
    }

    private static IEnumerable<ListViewItemWithGuid> FormatClasses(SourceCodeFormattableAssembly typelib)
    {
        return typelib.GetComClasses().OrderBy(t => t.Name).Select(MapTypeToItem);
    }

    private static IEnumerable<ListViewItem> FormatAssemblyStructs(SourceCodeFormattableAssembly typelib)
    {
        return typelib.GetComStructs().OrderBy(t => t.Name).Select(MapTypeToItemNoSubItem);
    }

    private static IEnumerable<ListViewItem> FormatAssemblyEnums(SourceCodeFormattableAssembly typelib)
    {
        return typelib.GetComEnums().OrderBy(t => t.Name).Select(MapTypeToItemNoSubItem);
    }

    private void AddGuidItems(ListView list,
        IEnumerable<ListViewItemWithGuid> items,
        TabPage tab_page, Guid guid_to_view, bool removeIfEmpty = true)
    {
        // clear is done outside of .Any() check so if the quick filter has no hits,
        // the previous ones will be cleared out of the listview 
        list.Items.Clear();
        if (items.Any())
        {
            list.Items.AddRange(items.ToArray());
            list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            foreach (ListViewItemWithGuid item in list.Items)
            {
                if (item.Guid == guid_to_view)
                {
                    item.Selected = true;
                    item.EnsureVisible();
                    tabControl.SelectedTab = tab_page;
                }
            }
        }
        else
        {
            if(removeIfEmpty)
               tabControl.TabPages.Remove(tab_page);
        }
    }

    private TypeLibControl(
        COMRegistry registry,
        string name,
        Guid guid_to_view,
        IEnumerable<ListViewItemWithGuid> interfaces,
        IEnumerable<ListViewItemWithGuid> dispatch,
        IEnumerable<ListViewItemWithGuid> classes,
        IEnumerable<ListViewItem> structs,
        IEnumerable<ListViewItem> enums)
    {
        m_builder = new(registry)
        {
            HideComments = true,
            InterfacesOnly = true
        };
        InitializeComponent();

        Text = name;

        textBoxFilter.Text = filterDefaultString;
        m_guid_to_view = guid_to_view;
        m_interfaces = interfaces;

        AddGuidItems(listViewClasses, classes, tabPageClasses, guid_to_view);
        AddGuidItems(listViewDispatch, dispatch, tabPageDispatch, guid_to_view);

        if (structs.Any())
        {
            listViewStructures.Items.AddRange(structs.ToArray());
            listViewStructures.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }
        else
        {
            tabControl.TabPages.Remove(tabPageStructures);
        }

        if (enums.Any())
        {
            listViewEnums.Items.AddRange(enums.ToArray());
            listViewEnums.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }
        else
        {
            tabControl.TabPages.Remove(tabPageEnums);
        }

        sourceCodeViewerControl.HideParsingOptions = true;
        RefreshInterfaces();
    }

    private void RefreshInterfaces()
    {
        bool showAll = textBoxFilter.Text.Length <= 0 || textBoxFilter.Text.Equals(filterDefaultString);
        var filteredInterfaces = m_interfaces;
        if(!showAll)
        {
            filteredInterfaces = m_interfaces.Where(item =>
            {
                if (item.Text.IndexOf(textBoxFilter.Text, 0, StringComparison.OrdinalIgnoreCase) > -1)
                    return true;

                string interfaceDescription = GetTextFromTag(item.Tag);
                return interfaceDescription.IndexOf(textBoxFilter.Text, 0, StringComparison.OrdinalIgnoreCase) > -1;
            });
        }
        AddGuidItems(listViewInterfaces, filteredInterfaces, tabPageInterfaces, m_guid_to_view, false);
    }

    private TypeLibControl(string name, SourceCodeFormattableAssembly typelib, Guid guid_to_view)
        : this(null, name, guid_to_view, FormatInterfaces(typelib), Array.Empty<ListViewItemWithGuid>(),
              typelib.ComVisible ? FormatClasses(typelib) : Array.Empty<ListViewItemWithGuid>(),
              FormatAssemblyStructs(typelib),
              FormatAssemblyEnums(typelib))
    {
    }

    public TypeLibControl(string name, Assembly typelib, Guid guid_to_view, bool dotnet_assembly)
        : this(name, new SourceCodeFormattableAssembly(typelib, dotnet_assembly), guid_to_view)
    {
    }

    private string GetTextFromTag(object tag)
    {
        m_builder.Reset();
        if (tag is ICOMSourceCodeFormattable formattable)
        {
            formattable.Format(m_builder);
            return m_builder.ToString().TrimEnd();
        }
        return string.Empty;
    }

    private void UpdateFromListView(ListView list)
    {
        string text = string.Empty;
        if (list.SelectedItems.Count > 0)
        {
            sourceCodeViewerControl.SelectedObject = list.SelectedItems[0].Tag;
        }
    }

    private void listView_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateFromListView(sender as ListView);
    }

    private void copyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (sender is ListView list && list.SelectedItems.Count > 0)
        {
            ListViewItem item = listViewInterfaces.SelectedItems[0];
            MiscUtilities.CopyTextToClipboard(item.Text);
        }
    }

    private void CopyGuid(object sender, GuidFormat copy_type)
    {
        if (sender is ListView list && list.SelectedItems.Count > 0)
        {
            if (list.SelectedItems[0] is ListViewItemWithGuid item)
            {
                MiscUtilities.CopyGuidToClipboard(item.Guid, copy_type);
            }
        }
    }

    private void copyGUIDToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CopyGuid(sender, GuidFormat.String);
    }

    private void copyGUIDCStructureToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CopyGuid(sender, GuidFormat.Structure);
    }

    private void copyGIUDHexStringToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CopyGuid(sender, GuidFormat.HexString);
    }

    public void SelectInterface(Guid iid)
    {
        foreach (ListViewItemWithGuid item in listViewInterfaces.Items)
        {
            if (item.Guid == iid)
            {
                item.Selected = true;
                item.EnsureVisible();
            }
        }
    }

    private void tabControl_Selected(object sender, TabControlEventArgs e)
    {
        if (e.TabPage.Controls.Count > 0 && e.TabPage.Controls[0] is ListView)
        {
            UpdateFromListView((ListView)e.TabPage.Controls[0]);
        }
    }

    private void btnExportInterfaces_Click(object sender, EventArgs e)
    {
        StringBuilder sb = new();
        foreach(var listView in new ListView[] { listViewEnums, listViewStructures, listViewClasses, listViewInterfaces })
        {
            foreach (ListViewItem item in listView.Items)
            {
                sb.Append(GetTextFromTag(item.Tag));
                sb.AppendLine();
            }

            sb.AppendLine();
        }

        Clipboard.SetText(sb.ToString());
        MessageBox.Show("View has been exported to the clipboard as text.");
    }

    private void textBoxFilter_Enter(object sender, EventArgs e)
    {
        textBoxFilter.SelectAll();
    }

    private void textBoxFilter_KeyUp(object sender, KeyEventArgs e)
    {
        if (IsDisposed)
            return;
        RefreshInterfaces();
    }
}
