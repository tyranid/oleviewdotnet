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
using OleViewDotNet.Interop;
using OleViewDotNet.Proxy;
using OleViewDotNet.TypeLib;
using OleViewDotNet.Utilities;
using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

public partial class TypeLibControl : UserControl
{
    private readonly IEnumerable<ListViewItemWithGuid> m_interfaces;
    private readonly COMSourceCodeBuilder m_builder;
    private Guid m_guid_to_view;
    private readonly string m_com_class_id_name;
    private Guid? m_com_class_id;

    private const string filterDefaultString = "filter interfaces";

    private class ListViewItemWithGuid : ListViewItem
    {
        public Guid Guid { get; }
        public ListViewItemWithGuid(string name, Guid iid) : base(name)
        {
            Guid = iid;
        }
    }

    private sealed class ProxyFormattable : ICOMSourceCodeFormattable
    {
        private readonly Func<INdrFormatter, string> m_format;

        public ProxyFormattable(NdrComProxyDefinition proxy)
        {
            m_format = f => f.FormatComProxy(proxy);
        }

        public ProxyFormattable(NdrComplexTypeReference type)
        {
            m_format = f => f.FormatComplexType(type);
        }

        void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
        {
            builder.AppendLine(m_format(builder.GetNdrFormatter()));
        }
    }

    private static ListViewItemWithGuid MapTypeInfoToItem(COMTypeLibTypeInfo type)
    {
        ListViewItemWithGuid item = new(type.Name, type.Uuid);
        item.SubItems.Add(type.Uuid.FormatGuid());
        item.Tag = type;
        return item;
    }

    private static ListViewItemWithGuid MapTypeInfoToItemNoSubItem(COMTypeLibTypeInfo type)
    {
        return new(type.Name, type.Uuid)
        {
            Tag = type
        };
    }

    private static IEnumerable<ListViewItemWithGuid> FormatInterfaces(IProxyFormatter formatter, IDictionary<Guid, string> iids_to_names)
    {
        if (formatter is COMTypeLib typelib)
            return typelib.Interfaces.OrderBy(t => t.Name).Select(MapTypeInfoToItem);
        if (formatter is COMProxyInstance proxy)
            return FormatProxyInstance(proxy, iids_to_names);
        return Array.Empty<ListViewItemWithGuid>();
    }

    private static IEnumerable<ListViewItemWithGuid> FormatDispatch(IProxyFormatter formatter)
    {
        if (formatter is COMTypeLib typelib)
            return typelib.Dispatch.OrderBy(t => t.Name).Select(MapTypeInfoToItem);
        return Array.Empty<ListViewItemWithGuid>();
    }

    private static IEnumerable<ListViewItemWithGuid> FormatClasses(IProxyFormatter formatter)
    {
        if(formatter is COMTypeLib typelib)
            return typelib.Classes.OrderBy(t => t.Name).Select(MapTypeInfoToItem);
        return Array.Empty<ListViewItemWithGuid>();
    }

    private static IEnumerable<ListViewItem> FormatStructs(IProxyFormatter formatter)
    {
        if (formatter is COMTypeLib typelib)
            return typelib.ComplexTypes.OrderBy(t => t.Name).Select(MapTypeInfoToItemNoSubItem);
        if (formatter is COMProxyInstance proxy)
            return FormatProxyInstanceComplexTypes(proxy);
        return Array.Empty<ListViewItemWithGuid>();
    }

    private static IEnumerable<ListViewItem> FormatEnums(IProxyFormatter formatter)
    {
        if (formatter is COMTypeLib typelib)
            return typelib.Enums.OrderBy(t => t.Name).Select(MapTypeInfoToItemNoSubItem);
        return Array.Empty<ListViewItemWithGuid>();
    }

    private static ListViewItemWithGuid MapTypeToItem(Type type)
    {
        ListViewItemWithGuid item = new(type.Name, type.GUID);
        item.SubItems.Add(type.GUID.FormatGuid());
        item.Tag = new SourceCodeFormattableType(type);
        return item;
    }

    private static ListViewItemWithGuid MapTypeToItemNoSubItem(Type type)
    {
        return new(type.Name, type.GUID)
        {
            Tag = new SourceCodeFormattableType(type)
        };
    }

    private static IEnumerable<ListViewItemWithGuid> FormatInterfaces(Assembly typelib, bool com_visible)
    {
        return COMUtilities.GetComInterfaces(typelib, com_visible).OrderBy(t => t.Name).Select(MapTypeToItem);
    }

    private static IEnumerable<ListViewItemWithGuid> FormatClasses(Assembly typelib, bool com_visible)
    {
        return COMUtilities.GetComClasses(typelib, com_visible).OrderBy(t => t.Name).Select(MapTypeToItem);
    }

    private static string GetComProxyName(NdrComProxyDefinition proxy, IDictionary<Guid, string> iids_to_names)
    {
        if (!string.IsNullOrWhiteSpace(proxy.Name))
        {
            return COMUtilities.DemangleWinRTName(proxy.Name, proxy.Iid);
        }
        if (iids_to_names != null && iids_to_names.TryGetValue(proxy.Iid, out string name))
        {
            return name;
        }
        return $"intf_{proxy.Iid.ToString().Replace('-', '_')}";
    }

    private static IEnumerable<ListViewItemWithGuid> FormatProxyInstance(COMProxyInstance proxy, IDictionary<Guid, string> iids_to_names)
    {
        foreach (var entry in proxy.Entries.Select(t => Tuple.Create(GetComProxyName(t, iids_to_names), t)).OrderBy(t => t.Item1))
        {
            ListViewItemWithGuid item = new(entry.Item1, entry.Item2.Iid);
            item.SubItems.Add(entry.Item2.Iid.FormatGuid());
            item.Tag = new ProxyFormattable(entry.Item2);
            yield return item;
        }
    }

    private static IEnumerable<ListViewItem> FormatProxyInstanceComplexTypes(COMProxyInstance proxy)
    {
        foreach (var type in proxy.ComplexTypes.OrderBy(p => p.Name))
        {
            ListViewItem item = new(type.Name);
            item.SubItems.Add(type.GetSize().ToString());
            item.Tag = new ProxyFormattable(type);
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
        m_builder = new(registry);
        m_builder.RemoveComments = true;
        InitializeComponent();

        cbProxyRenderStyle.SelectedIndex = 0;

        textEditor.SetHighlighting("C#");
        textEditor.IsReadOnly = true;
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

    public TypeLibControl(string name, Assembly typelib, Guid guid_to_view, bool dotnet_assembly)
        : this(null, name, guid_to_view, FormatInterfaces(typelib, dotnet_assembly), Array.Empty<ListViewItemWithGuid>(),
              dotnet_assembly ? FormatClasses(typelib, dotnet_assembly) : Array.Empty<ListViewItemWithGuid>(),
              FormatAssemblyStructs(typelib, dotnet_assembly),
              FormatAssemblyEnums(typelib, dotnet_assembly))
    {
        btnDqs.Visible = false;
        cbProxyRenderStyle.Visible = false;
        checkBoxHideComments.Visible = false;
        lblRendering.Visible = false;
    }

    public TypeLibControl(COMRegistry registry, string name, IProxyFormatter formatter,
        Guid guid_to_view, string com_class_id_name = null, Guid? com_class_id = null)
        : this(registry, name, guid_to_view,
              FormatInterfaces(formatter, registry?.InterfacesToNames), FormatDispatch(formatter),
              FormatClasses(formatter), FormatStructs(formatter), FormatEnums(formatter))
    {
        bool is_proxy = formatter is COMProxyInstance;
        btnDqs.Visible = is_proxy;
        cbProxyRenderStyle.Visible = is_proxy;
        checkBoxHideComments.Visible = is_proxy;
        lblRendering.Visible = is_proxy;
        m_com_class_id = com_class_id;
        m_com_class_id_name = com_class_id_name;
    }

    private string GetTextFromTag(object tag)
    {
        m_builder.Reset();
        if (tag is ICOMSourceCodeFormattable formattable)
        {
            formattable.Format(m_builder);
            return m_builder.ToString();
        }
        return string.Empty;
    }

    private void UpdateFromListView(ListView list)
    {
        string text = string.Empty;
        if (list.SelectedItems.Count > 0)
        {
            text = GetTextFromTag(list.SelectedItems[0].Tag);
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
        if (sender is ListView list && list.SelectedItems.Count > 0)
        {
            ListViewItem item = listViewInterfaces.SelectedItems[0];
            COMUtilities.CopyTextToClipboard(item.Text);
        }
    }

    private void CopyGuid(object sender, GuidFormat copy_type)
    {
        if (sender is ListView list && list.SelectedItems.Count > 0)
        {
            if (list.SelectedItems[0] is ListViewItemWithGuid item)
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

    private void btnExportInterfaces_Click(object sender, EventArgs e)
    {
        StringBuilder sb = new();
        if((cbProxyRenderStyle.SelectedIndex > 1) && (m_com_class_id_name != null) && (m_com_class_id != null))
        {
            // C++ style is requsted, let's add a line about the CLSID being rendered

            sb.AppendLine(COMUtilities.FormatGuidAsCStruct(m_com_class_id_name, m_com_class_id.Value));
            sb.AppendLine();
        }
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

    private void btnDqs_Click(object sender, EventArgs e)
    {
        using var formInput = new TextAreaInputForm();
        formInput.Text = "Paste the DQS lines obtained from windbg";
        if (DialogResult.OK != formInput.ShowDialog()) return;

        CombineOVDNIdlAndDqs(textEditor.Text, formInput.textEditor.Text);
    }

    private static void CombineOVDNIdlAndDqs(string ovdnIdl, string dqs)
    {
        MessageBox.Show(CombineOVDNIdlAndDqsInt(ovdnIdl, dqs));
    }

    private static string CombineOVDNIdlAndDqsInt(string ovdnIdl, string dqs)
    {
        MatchCollection unknownMethodNames = Regex.Matches(ovdnIdl, @"\s+Proc\d+\(");
        MatchCollection dqsSymbolNames = Regex.Matches(dqs, @"\s*[0-9a-f`]+\s+[0-9a-f`]+\s+\S+!(?:[^:]+::)?(\S+)");

        if (unknownMethodNames.Count != dqsSymbolNames.Count)
            return $"Different number of methods found in OVDN IDL ({unknownMethodNames.Count}) and in windbg dqs ({dqsSymbolNames.Count}). Don't forget to remove the methods of the parent interface from the DQS lines! (E.g. strip QueryInterface/AddRef/Release lines if the target is IUnknown based)";
        for (var i = 0; i < unknownMethodNames.Count; i++)
        {
            ovdnIdl = ovdnIdl.Replace(unknownMethodNames[i].Groups[0].Value, " " + dqsSymbolNames[i].Groups[1].Value + "(");
        }

        // and saving the result to the clipboard
        Clipboard.SetText(ovdnIdl);
        return "Alrighty, the combined lines are now on your clipboard.";
    }

    private void cbProxyRenderStyle_SelectedIndexChanged(object sender, EventArgs e)
    {
        if ((tabControl.SelectedTab.Controls.Count > 0) && (tabControl.SelectedTab.Controls[0] is ListView view))
        {
            m_builder.OutputType = cbProxyRenderStyle.SelectedIndex switch
            {
                1 => COMSourceCodeBuilderType.Generic,
                2 => COMSourceCodeBuilderType.Cpp,
                _ => COMSourceCodeBuilderType.Idl
            };

            UpdateFromListView(view);
        }
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

    private void checkBoxHideComments_CheckedChanged(object sender, EventArgs e)
    {
        if ((tabControl.SelectedTab.Controls.Count > 0) && (tabControl.SelectedTab.Controls[0] is ListView view))
        {
            m_builder.RemoveComments = checkBoxHideComments.Checked;
            UpdateFromListView(view);
        }
    }
}
