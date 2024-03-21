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
using System.Collections;
using System.Globalization;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal class ListItemComparer : IComparer
{
    public ListItemComparer(int column)
    {
        Column = column;
        Ascending = true;
    }

    private static IComparable GetComparableItem(string value)
    {
        if (long.TryParse(value, out long l))
        {
            return l;
        }
        else if (value.StartsWith("0x") && long.TryParse(value.Substring(2), NumberStyles.HexNumber, null, out l))
        {
            return l;
        }
        if (Guid.TryParse(value, out Guid g))
        {
            return g;
        }
        return value;
    }

    public int Compare(object x, object y)
    {
        ListViewItem xi = (ListViewItem)x;
        ListViewItem yi = (ListViewItem)y;

        if (xi.SubItems.Count <= Column)
        {
            throw new ArgumentException("Invalid item for comparer", "x");
        }

        if (yi.SubItems.Count <= Column)
        {
            throw new ArgumentException("Invalid item for comparer", "y");
        }

        IComparable left = GetComparableItem(xi.SubItems[Column].Text);
        IComparable right = GetComparableItem(yi.SubItems[Column].Text);

        if (left.GetType() != right.GetType())
        {
            left = left.ToString();
            right = right.ToString();
        }

        if (Ascending)
        {
            return left.CompareTo(right);
        }
        else
        {
            return right.CompareTo(left);
        }
    }

    public int Column
    {
        get;
        set;
    }

    public bool Ascending
    {
        get;
        set;
    }

    public static void UpdateListComparer(ListView view, int selected_column)
    {
        if (view is not null)
        {
            if (view.ListViewItemSorter is ListItemComparer comparer)
            {
                if (selected_column != comparer.Column)
                {
                    comparer.Column = selected_column;
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

}
