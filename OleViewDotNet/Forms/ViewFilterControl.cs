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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class ViewFilterControl : UserControl
{
    private class FilterSorter : IComparer
    {
        private int Column { get; set; }

        public FilterSorter(int column)
        {
            Column = column;
        }

        public int Compare(object x, object y)
        {
            ListViewItem item_x = (ListViewItem)x;
            ListViewItem item_y = (ListViewItem)y;
            return item_x.SubItems[Column].Text.CompareTo(item_y.SubItems[Column].Text);
        }
    }

    private readonly FilterSorter m_sorter;

    private static ListViewItem CreateItem(RegistryViewerFilterEntry entry)
    {
        ListViewItem item = new(entry.Type.ToString(), entry.Decision == FilterDecision.Include ? 1 : 0);
        item.SubItems.Add(entry.Field.ToString());
        item.SubItems.Add(entry.Comparison.ToString());
        item.SubItems.Add(entry.Value);
        item.Checked = entry.Enabled;
        item.Tag = entry.Clone();
        return item;
    }

    private void UpdateFilter(RegistryViewerFilter filter)
    {
        listViewFilters.Items.Clear();
        foreach (var entry in filter.Filters)
        {
            listViewFilters.Items.Add(CreateItem(entry));
        }
        listViewFilters.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewFilters.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        listViewFilters.Sort();
    }

    private RegistryViewerFilter BuildFilter()
    {
        RegistryViewerFilter filter = new();
        foreach (ListViewItem item in listViewFilters.Items)
        {
            filter.Filters.Add((RegistryViewerFilterEntry)item.Tag);
        }
        return filter;
    }

    private static void PopulateComboBox(ComboBox comboBox, IEnumerable<object> values)
    {
        comboBox.Items.Clear();
        comboBox.Items.AddRange(values.ToArray());
        if (comboBox.Items.Count > 0)
        {
            comboBox.SelectedIndex = 0;
        }
    }

    private static void PopulateComboBox(ComboBox comboBox, Type enum_type)
    {
        PopulateComboBox(comboBox, Enum.GetValues(enum_type).OfType<object>());
    }

    public ViewFilterControl()
    {
        InitializeComponent();
        m_sorter = new FilterSorter(0);
        listViewFilters.ListViewItemSorter = m_sorter;
        PopulateComboBox(comboBoxDecision, typeof(FilterDecision));
        PopulateComboBox(comboBoxFilterComparison, typeof(FilterComparison));
    }

    public void SetTypes(IEnumerable<FilterType> types)
    {
        PopulateComboBox(comboBoxFilterType, types.OfType<object>());
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RegistryViewerFilter Filter
    {
        get => BuildFilter();

        set => UpdateFilter(value);
    }

    public event EventHandler FilterChanged;

    private void OnFilterChanged()
    {
        FilterChanged?.Invoke(this, new EventArgs());
    }

    private void listViewFilters_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
        RegistryViewerFilterEntry entry = (RegistryViewerFilterEntry)e.Item.Tag;
        entry.Enabled = e.Item.Checked;
        OnFilterChanged();
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
        listViewFilters.Items.Clear();
        OnFilterChanged();
    }

    private void comboBoxFilterType_SelectedIndexChanged(object sender, EventArgs e)
    {
        comboBoxField.Items.Clear();
        PopulateComboBox(comboBoxField, RegistryViewerFilter.GetFieldsForType((FilterType)comboBoxFilterType.SelectedItem));
    }

    private void comboBoxField_SelectedIndexChanged(object sender, EventArgs e)
    {
        comboBoxValue.Items.Clear();
        comboBoxValue.Text = string.Empty;

        PropertyInfo pi = (PropertyInfo)comboBoxField.SelectedItem;
        Type type = pi.PropertyType;

        if (type == typeof(bool))
        {
            PopulateComboBox(comboBoxValue, new object[] { "True", "False" });
            comboBoxValue.DropDownStyle = ComboBoxStyle.DropDownList;
        }
        else if (type.IsEnum)
        {
            PopulateComboBox(comboBoxValue, type);
            comboBoxValue.DropDownStyle = ComboBoxStyle.DropDownList;
        }
        else if (type == typeof(Guid))
        {
            comboBoxValue.DropDownStyle = ComboBoxStyle.DropDown;
            comboBoxValue.Text = Guid.Empty.ToString();
        }
        else
        {
            comboBoxValue.DropDownStyle = ComboBoxStyle.DropDown;
        }
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
        RegistryViewerFilterEntry entry = new();
        entry.Type = (FilterType)comboBoxFilterType.SelectedItem;
        entry.Field = ((PropertyInfo)comboBoxField.SelectedItem).Name;
        entry.Enabled = true;
        entry.Decision = (FilterDecision)comboBoxDecision.SelectedItem;
        entry.Comparison = (FilterComparison)comboBoxFilterComparison.SelectedItem;
        entry.Value = comboBoxValue.Text;
        listViewFilters.Items.Add(CreateItem(entry));
        listViewFilters.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewFilters.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        listViewFilters.Sort();
        OnFilterChanged();
    }

    private void UpdateInputForEntry(RegistryViewerFilterEntry entry)
    {
        bool in_list = false;
        foreach (FilterType type in comboBoxFilterType.Items)
        {
            if (entry.Type == type)
            {
                in_list = true;
                break;
            }
        }

        if (!in_list)
        {
            return;
        }

        in_list = false;
        comboBoxFilterType.SelectedItem = entry.Type;
        foreach (PropertyInfo pi in comboBoxField.Items)
        {
            if (pi.Name == entry.Field)
            {
                comboBoxField.SelectedItem = pi;
                in_list = true;
                break;
            }
        }

        if (!in_list)
        {
            return;
        }

        comboBoxFilterComparison.SelectedItem = entry.Comparison;
        comboBoxValue.Text = entry.Value;
        comboBoxDecision.SelectedItem = entry.Decision;
    }

    private void btnRemove_Click(object sender, EventArgs e)
    {
        if (listViewFilters.SelectedIndices.Count > 0)
        {
            RegistryViewerFilterEntry entry = (RegistryViewerFilterEntry)listViewFilters.SelectedItems[0].Tag;
            listViewFilters.Items.RemoveAt(listViewFilters.SelectedIndices[0]);
            UpdateInputForEntry(entry);

            OnFilterChanged();
        }
    }

    private void listViewFilters_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (listViewFilters.SelectedIndices.Count > 0)
        {
            btnRemove.Enabled = true;
        }
        else
        {
            btnRemove.Enabled = false;
        }
    }
}
