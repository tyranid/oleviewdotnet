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
        private IDictionary<Guid, string> m_iids_to_names;

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

        private class ListViewItemWithIid : ListViewItem
        {
            public Guid Iid { get; private set; }
            public ListViewItemWithIid(string name, Guid iid) : base(name)
            {
                Iid = iid;
            }
        }

        private static IEnumerable<ListViewItemWithIid> FormatAssembly(Assembly typelib)
        {
            foreach (Type t in typelib.GetTypes().Where(t => Attribute.IsDefined(t, typeof(ComImportAttribute)) && t.IsInterface).OrderBy(t => t.Name))
            {
                ListViewItemWithIid item = new ListViewItemWithIid(t.Name, t.GUID);
                item.SubItems.Add(t.GUID.ToString("B"));
                item.Tag = t;
                yield return item;
            }
        }

        private static IEnumerable<ListViewItemWithIid> FormatProxyInstance(COMProxyInstance proxy)
        {
            foreach (COMProxyInstanceEntry t in proxy.Entries.OrderBy(t => t.Name))
            {
                ListViewItemWithIid item = new ListViewItemWithIid(t.Name, t.Iid);
                item.SubItems.Add(t.Iid.ToString("B"));
                item.Tag = t;
                yield return item;
            }
        }

        private TypeLibControl(IDictionary<Guid, string> iids_to_names, string name, Guid iid_to_view, IEnumerable<ListViewItemWithIid> items)
        {
            m_iids_to_names = iids_to_names;
            InitializeComponent();
            listViewTypes.Items.AddRange(items.ToArray());
            listViewTypes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            foreach (ListViewItemWithIid item in listViewTypes.Items)
            {
                if (item.Iid == iid_to_view)
                {
                    item.Selected = true;
                }
            }
            
            textEditor.SetHighlighting("C#");
            textEditor.IsReadOnly = true;
            Text = name;
        }

        public TypeLibControl(string name, Assembly typelib, Guid iid_to_view) 
            : this(null, name, iid_to_view, FormatAssembly(typelib))
        {
        }

        public TypeLibControl(COMRegistry registry, string name, COMProxyInstance proxy, Guid iid_to_view) 
            : this(registry.InterfacesToNames, name, iid_to_view, FormatProxyInstance(proxy))
        {
        }

        private void listViewTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = String.Empty;

            if (listViewTypes.SelectedItems.Count > 0)
            {
                ListViewItem item = listViewTypes.SelectedItems[0];
                Type type = item.Tag as Type;
                COMProxyInstanceEntry proxy = item.Tag as COMProxyInstanceEntry;

                if (type != null)
                {
                    text = TypeToText(type);
                }
                else if (proxy != null)
                {
                    text = proxy.Format(m_iids_to_names);
                }
            }

            textEditor.Text = text;
            textEditor.Refresh();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewTypes.SelectedItems.Count > 0)
            {
                ListViewItem item = listViewTypes.SelectedItems[0];
                COMRegistryViewer.CopyTextToClipboard(item.Text);
            }
        }

        private void CopyGuid(COMRegistryViewer.CopyGuidType copy_type)
        {
            if (listViewTypes.SelectedItems.Count > 0)
            {
                ListViewItemWithIid item = (ListViewItemWithIid) listViewTypes.SelectedItems[0];
                COMRegistryViewer.CopyGuidToClipboard(item.Iid, copy_type);
            }
        }

        private void copyGUIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyGuid(COMRegistryViewer.CopyGuidType.CopyAsString);
        }

        private void copyGUIDCStructureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyGuid(COMRegistryViewer.CopyGuidType.CopyAsStructure);
        }

        private void copyGIUDHexStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyGuid(COMRegistryViewer.CopyGuidType.CopyAsHexString);
        }
    }
}
