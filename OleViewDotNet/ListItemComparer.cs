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
using System.Windows.Forms;

namespace OleViewDotNet
{
    public class ListItemComparer : IComparer
    {
        public ListItemComparer(int column)
        {
            Column = column;
            Ascending = true;
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

            if (Ascending)
            {
                return String.Compare(xi.SubItems[Column].Text, yi.SubItems[Column].Text);
            }
            else
            {
                return String.Compare(yi.SubItems[Column].Text, xi.SubItems[Column].Text);
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

    }
}
