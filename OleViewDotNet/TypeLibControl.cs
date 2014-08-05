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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OleViewDotNet
{
    public partial class TypeLibControl : UserControl
    {
        static string TypeToText(Type t)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("Name: {0}", t.Name).AppendLine();
            builder.AppendFormat("IID: {0}", t.GUID).AppendLine();
            builder.AppendLine("{");
            foreach (MemberInfo mi in t.GetMembers())
            {
                String name = COMUtilities.MemberInfoToString(mi);
                if (!String.IsNullOrWhiteSpace(name))
                {
                    builder.Append("   ");
                    builder.AppendLine(name);
                }
            }
            builder.AppendLine("}");

            return builder.ToString();
        }

        public TypeLibControl(COMTypeLibVersionEntry entry, Assembly typeLib)
        {
            InitializeComponent();
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (Type t in typeLib.GetTypes().Where(t => Attribute.IsDefined(t, typeof(ComImportAttribute)) && t.IsInterface))
            {
                ListViewItem item = new ListViewItem(String.Format("{0} - {1}", t.Name, t.GUID));
                item.Tag = t;
                items.Add(item);
            }

            listViewTypes.Items.AddRange(items.ToArray());
            this.Text = entry.Name;
        }

        private void listViewTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = String.Empty;

            if (listViewTypes.SelectedItems.Count > 0)
            {
                ListViewItem item = listViewTypes.SelectedItems[0];

                text = TypeToText((Type)item.Tag);
            }

            richTextBoxDump.Text = text;
        }
    }
}
