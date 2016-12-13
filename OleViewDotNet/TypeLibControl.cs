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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class TypeLibControl : UserControl
    {
        static void EmitMember(StringBuilder builder, MemberInfo mi)
        {
            String name = COMUtilities.MemberInfoToString(mi);
            if (!String.IsNullOrWhiteSpace(name))
            {
                builder.Append("   ");
                builder.AppendLine(name);
            }
        }

        static string TypeToText(Type t)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("[Guid(\"{0}\")]", t.GUID).AppendLine();
            builder.AppendFormat("interface {0}", t.Name).AppendLine();
            builder.AppendLine("{");

            IEnumerable<MethodInfo> methods = t.GetMethods().Where(m => (m.Attributes & MethodAttributes.SpecialName) == 0);
            if (methods.Count() > 0)
            {
                builder.AppendLine("   /* Methods */");
                foreach (MethodInfo mi in methods)
                {
                    EmitMember(builder, mi);
                }
            }

            PropertyInfo[] props = t.GetProperties();
            if (props.Length > 0)
            {
                builder.AppendLine("   /* Properties */");
                foreach (PropertyInfo pi in props)
                {
                    EmitMember(builder, pi);
                }
            }
            
            builder.AppendLine("}");

            return builder.ToString();
        }

        public TypeLibControl(string name, Assembly typeLib)
        {
            InitializeComponent();
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (Type t in typeLib.GetTypes().Where(t => Attribute.IsDefined(t, typeof(ComImportAttribute)) && t.IsInterface).OrderBy(t => t.Name))
            {
                ListViewItem item = new ListViewItem(t.Name);
                item.SubItems.Add(t.GUID.ToString("B"));
                item.Tag = t;
                items.Add(item);
            }
            
            listViewTypes.Items.AddRange(items.ToArray());
            listViewTypes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            textEditor.SetHighlighting("C#");
            textEditor.IsReadOnly = true;
            Text = name;
        }

        private void listViewTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = String.Empty;

            if (listViewTypes.SelectedItems.Count > 0)
            {
                ListViewItem item = listViewTypes.SelectedItems[0];

                text = TypeToText((Type)item.Tag);
            }

            textEditor.Text = text;
            textEditor.Refresh();
        }
    }
}
