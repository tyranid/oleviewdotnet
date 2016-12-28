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
using System.ComponentModel;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class ViewFilterControl : UserControl
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

        private FilterSorter m_sorter;

        private void UpdateFilter(RegistryViewerFilter filter)
        {
            listViewFilters.Items.Clear();
            foreach (var entry in filter.Filters)
            {
                ListViewItem item = new ListViewItem(entry.Type.ToString(), entry.Decision == FilterDecision.Include ? 0 : 1);
                item.SubItems.Add(entry.Field.ToString());
                item.SubItems.Add(entry.Comparison.ToString());
                item.SubItems.Add(entry.Value);
                item.Checked = entry.Enabled;
                item.Tag = entry.Clone();
            }
            listViewFilters.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewFilters.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewFilters.Sort();
        }

        private RegistryViewerFilter BuildFilter()
        {
            RegistryViewerFilter filter = new RegistryViewerFilter();
            foreach (ListViewItem item in listViewFilters.Items)
            {
                filter.Filters.Add((RegistryViewerFilterEntry)item.Tag);
            }
            return filter;
        }

        public ViewFilterControl()
        {
            InitializeComponent();
            m_sorter = new FilterSorter(0);
            listViewFilters.ListViewItemSorter = m_sorter;
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RegistryViewerFilter Filter
        {
            get
            {
                return BuildFilter();
            }

            set
            {
                UpdateFilter(value);
            }
        }

        public event EventHandler FilterChanged;

        private void OnFilterChanged()
        {
            FilterChanged(this, new EventArgs());
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
    }
}
