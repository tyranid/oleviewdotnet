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

using NtApiDotNet.Ndr;
using OleViewDotNet.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace OleViewDotNet.Forms
{
    public partial class TypeLibControl : UserControl
    {
        private IDictionary<Guid, string> m_iids_to_names;
        private IEnumerable<ListViewItemWithGuid> interfaces;
        private Guid guid_to_view;

        private const string filterDefaultString = "filter interfaces";

        private class ListViewItemWithGuid : ListViewItem
        {
            public Guid Guid { get; private set; }
            public ListViewItemWithGuid(string name, Guid iid) : base(name)
            {
                Guid = iid;
            }
        }

        private static ListViewItemWithGuid MapTypeToItem(Type type)
        {
            ListViewItemWithGuid item = new ListViewItemWithGuid(type.Name, type.GUID);
            item.SubItems.Add(type.GUID.FormatGuid());
            item.Tag = type;
            return item;
        }

        private static ListViewItemWithGuid MapTypeToItemNoSubItem(Type type)
        {
            ListViewItemWithGuid item = new ListViewItemWithGuid(type.Name, type.GUID);
            item.Tag = type;
            return item;
        }

        private static IEnumerable<ListViewItemWithGuid> FormatInterfaces(Assembly typelib, bool com_visible)
        {
            return COMUtilities.GetComInterfaces(typelib, com_visible).OrderBy(t => t.Name).Select(MapTypeToItem);
        }

        private static IEnumerable<ListViewItemWithGuid> FormatClasses(Assembly typelib, bool com_visible)
        {
            return COMUtilities.GetComClasses(typelib, com_visible).OrderBy(t => t.Name).Select(MapTypeToItem);
        }

        private static IEnumerable<ListViewItemWithGuid> FormatProxyInstance(COMProxyInstance proxy)
        {
            foreach (var t in proxy.Entries.OrderBy(t => COMUtilities.DemangleWinRTName(t.Name)))
            {
                ListViewItemWithGuid item = new ListViewItemWithGuid(COMUtilities.DemangleWinRTName(t.Name), t.Iid);
                item.SubItems.Add(t.Iid.FormatGuid());
                item.Tag = t;
                yield return item;
            }
        }

        private static IEnumerable<ListViewItem> FormatProxyInstanceComplexTypes(COMProxyInstance proxy)
        {
            foreach (var type in proxy.ComplexTypes.OrderBy(p => p.Name))
            {
                ListViewItem item = new ListViewItem(type.Name);
                item.SubItems.Add(type.GetSize().ToString());
                item.Tag = type;
                yield return item;
            }
        }

        private static IEnumerable<ListViewItem> FormatAssemblyStructs(Assembly typelib, bool com_visible)
        {
            return COMUtilities.GetComStructs(typelib, com_visible).OrderBy(t => t.Name).Select(MapTypeToItemNoSubItem);
        }

        private static IEnumerable<ListViewItem> FormatAssemblyEnums(Assembly typelib, bool com_visible)
        {
            return COMUtilities.GetComEnums(typelib, com_visible).OrderBy(t => t.Name).Select(MapTypeToItemNoSubItem);
        }

        private void AddGuidItems(ListView list,
            IEnumerable<ListViewItemWithGuid> items,
            TabPage tab_page, Guid guid_to_view, bool removeIfEmpty = true)
        {
            if (items.Any())
            {
                list.Items.Clear();
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

        private TypeLibControl(IDictionary<Guid, string> iids_to_names,
            string name,
            Guid guid_to_view,
            IEnumerable<ListViewItemWithGuid> interfaces,
            IEnumerable<ListViewItemWithGuid> classes,
            IEnumerable<ListViewItem> structs,
            IEnumerable<ListViewItem> enums)
        {
            m_iids_to_names = iids_to_names;
            InitializeComponent();

            textEditor.SetHighlighting("C#");
            textEditor.IsReadOnly = true;
            Text = name;

            this.inputFilter.Text = filterDefaultString;
            this.guid_to_view = guid_to_view;
            this.interfaces = interfaces;

            AddGuidItems(listViewClasses, classes, tabPageClasses, guid_to_view);

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

            RefreshInterfaces();
        }

        private void RefreshInterfaces()
        {
            bool showAll = inputFilter.Text.Length <= 0 || inputFilter.Text.Equals(filterDefaultString);
            var filteredInterfaces = interfaces;
            if(!showAll)
            {
                filteredInterfaces = interfaces.Where(item =>
                {
                    if (item.Text.IndexOf(inputFilter.Text, 0, StringComparison.OrdinalIgnoreCase) > -1)
                        return true;

                    string interfaceDescription = GetTextFromTag(item.Tag);
                    return interfaceDescription.IndexOf(inputFilter.Text, 0, StringComparison.OrdinalIgnoreCase) > -1;
                });
            }
            AddGuidItems(listViewInterfaces, filteredInterfaces, tabPageInterfaces, guid_to_view, false);
        }

        public TypeLibControl(string name, Assembly typelib, Guid guid_to_view, bool dotnet_assembly)
            : this(null, name, guid_to_view, FormatInterfaces(typelib, dotnet_assembly),
                  dotnet_assembly ? FormatClasses(typelib, dotnet_assembly) : new ListViewItemWithGuid[0],
                  FormatAssemblyStructs(typelib, dotnet_assembly),
                  FormatAssemblyEnums(typelib, dotnet_assembly))
        {
        }

        public TypeLibControl(COMRegistry registry, string name, COMProxyInstance proxy, Guid guid_to_view)
            : this(registry.InterfacesToNames, name, guid_to_view,
                  FormatProxyInstance(proxy),
                  new ListViewItemWithGuid[0], FormatProxyInstanceComplexTypes(proxy), new ListViewItem[0])
        {
        }

        private string GetTextFromTag(object tag)
        {
            var type = tag as Type;
            var proxy = tag as NdrComProxyDefinition;
            NdrComplexTypeReference str = tag as NdrComplexTypeReference;

            if (type != null)
            {
                return COMUtilities.FormatComType(type);
            }
            else if (proxy != null)
            {
                INdrFormatter formatter = DefaultNdrFormatter.Create(m_iids_to_names, COMUtilities.DemangleWinRTName);
                return formatter.FormatComProxy(proxy);
            }
            else if (str != null)
            {
                INdrFormatter formatter = DefaultNdrFormatter.Create(m_iids_to_names);
                return formatter.FormatComplexType(str);
            }

            return String.Empty;
        }

        private void UpdateFromListView(ListView list)
        {
            string text = String.Empty;

            if (list.SelectedItems.Count > 0)
            {
                ListViewItem item = list.SelectedItems[0];
                text = GetTextFromTag(item.Tag);
            }

            textEditor.Text = text;
            textEditor.Refresh();
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFromListView(sender as ListView);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView list = sender as ListView;
            if (list != null && list.SelectedItems.Count > 0)
            {
                ListViewItem item = listViewInterfaces.SelectedItems[0];
                COMUtilities.CopyTextToClipboard(item.Text);
            }
        }

        private void CopyGuid(object sender, GuidFormat copy_type)
        {
            ListView list = sender as ListView;
            if (list != null && list.SelectedItems.Count > 0)
            {
                ListViewItemWithGuid item = list.SelectedItems[0] as ListViewItemWithGuid;
                if (item != null)
                {
                    COMUtilities.CopyGuidToClipboard(item.Guid, copy_type);
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
        
        private void inputFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.Handled = true;
            RefreshInterfaces();
        }

        private void btnExportInterfaces_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach(ListViewItem item in this.listViewInterfaces.Items)
            {
                sb.Append( GetTextFromTag(item.Tag) );
                sb.AppendLine();
            }

            Clipboard.SetText(sb.ToString());
        }
    }
}
