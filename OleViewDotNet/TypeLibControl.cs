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
                if (mi is FieldInfo)
                {
                    builder.AppendFormat("{0};", name).AppendLine();
                }
                else
                {
                    builder.AppendLine(name);
                }
            }
        }

        static Dictionary<MethodInfo, string> MapMethodNamesToCOM(IEnumerable<MethodInfo> mis)
        {
            HashSet<string> matched_names = new HashSet<string>();
            Dictionary<MethodInfo, string> ret = new Dictionary<MethodInfo, string>();
            foreach (MethodInfo mi in mis.Reverse())
            {
                int count = 2;
                string name = mi.Name;
                while (!matched_names.Add(name))
                {
                    name = String.Format("{0}_{1}", mi.Name, count++);
                }
                ret.Add(mi, name);
            }
            return ret;
        }

        static string TypeToText(Type t)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                if (t.IsInterface)
                {
                    builder.AppendFormat("[Guid(\"{0}\")]", t.GUID).AppendLine();
                    builder.AppendFormat("interface {0}", t.Name).AppendLine();
                }
                else if (t.IsEnum)
                {
                    builder.AppendFormat("enum {0}", t.Name).AppendLine();
                }
                else if (t.IsClass)
                {
                    builder.AppendFormat("[Guid(\"{0}\")]", t.GUID).AppendLine();
                    ClassInterfaceAttribute class_attr = t.GetCustomAttribute<ClassInterfaceAttribute>();
                    if (class_attr != null)
                    {
                        builder.AppendFormat("[ClassInterface(ClassInterfaceType.{0})]", class_attr.Value).AppendLine();
                    }
                    builder.AppendFormat("class {0}", t.Name).AppendLine();
                }
                else
                {
                    builder.AppendFormat("struct {0}", t.Name).AppendLine();
                }
                builder.AppendLine("{");

                if (t.IsInterface || t.IsClass)
                {
                    MethodInfo[] methods = t.GetMethods().Where(m => !m.IsStatic && (m.Attributes & MethodAttributes.SpecialName) == 0).ToArray();
                    if (methods.Length > 0)
                    {
                        builder.AppendLine("   /* Methods */");

                        Dictionary<MethodInfo, string> name_mapping = new Dictionary<MethodInfo, string>();
                        if (t.IsClass)
                        {
                            name_mapping = MapMethodNamesToCOM(methods);
                        }

                        foreach (MethodInfo mi in methods)
                        {
                            if (name_mapping.ContainsKey(mi) && name_mapping[mi] != mi.Name)
                            {
                                builder.AppendFormat("    /* Exposed as {0} */", name_mapping[mi]).AppendLine();
                            }

                            EmitMember(builder, mi);
                        }
                    }

                    var props = t.GetProperties().Where(p => !p.GetMethod.IsStatic);
                    if (props.Count() > 0)
                    {
                        builder.AppendLine("   /* Properties */");
                        foreach (PropertyInfo pi in props)
                        {
                            EmitMember(builder, pi);
                        }
                    }
                }
                else if (t.IsEnum)
                {
                    foreach (var value in Enum.GetValues(t))
                    {
                        builder.Append("   ");
                        try
                        {
                            builder.AppendFormat("{0} = {1};", value, Convert.ToInt64(value));
                        }
                        catch
                        {
                            builder.AppendFormat("{0};");
                        }
                        builder.AppendLine();
                    }
                }
                else
                {
                    FieldInfo[] fields = t.GetFields();
                    if (fields.Length > 0)
                    {
                        builder.AppendLine("   /* Fields */");
                        foreach (FieldInfo fi in fields)
                        {
                            EmitMember(builder, fi);
                        }
                    }
                }

                builder.AppendLine("}");

                return builder.ToString();
            }
            catch (InvalidOperationException ex)
            {
                return ex.ToString();
            }
        }

        private class ListViewItemWithGuid : ListViewItem
        {
            public Guid Guid { get; private set; }
            public ListViewItemWithGuid(string name, Guid iid) : base(name)
            {
                Guid = iid;
            }
        }

        private static IEnumerable<ListViewItemWithGuid> FormatTypes(IEnumerable<Type> types, bool com_visible)
        {
            if (com_visible)
            {
                types = types.Where(t => Marshal.IsTypeVisibleFromCom(t));
            }
            else
            {
                types = types.Where(t => Attribute.IsDefined(t, typeof(ComImportAttribute)));
            }

            foreach (Type t in types.OrderBy(t => t.Name))
            {
                ListViewItemWithGuid item = new ListViewItemWithGuid(t.Name, t.GUID);
                item.SubItems.Add(t.GUID.FormatGuid());
                item.Tag = t;
                yield return item;
            }
        }

        private static IEnumerable<ListViewItemWithGuid> FormatInterfaces(Assembly typelib, bool com_visible)
        {
            return FormatTypes(typelib.GetTypes().Where(t => t.IsInterface), com_visible);
        }

        private static IEnumerable<ListViewItemWithGuid> FormatClasses(Assembly typelib, bool com_visible)
        {
            return FormatTypes(typelib.GetTypes().Where(t => t.IsClass), com_visible);
        }
        
        private static IEnumerable<ListViewItemWithGuid> FormatProxyInstance(COMProxyInstance proxy)
        {
            foreach (COMProxyInstanceEntry t in proxy.Entries.OrderBy(t => t.Name))
            {
                ListViewItemWithGuid item = new ListViewItemWithGuid(t.Name, t.Iid);
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
                item.Tag = type;
                yield return item;
            }
        }

        private static IEnumerable<ListViewItem> FormatAssemblyStructs(Assembly typelib, bool com_visible)
        {
            var types = typelib.GetTypes().Where(t => t.IsValueType && !t.IsEnum);
            if (com_visible)
            {
                types = types.Where(t => Marshal.IsTypeVisibleFromCom(t));
            }

            foreach (Type t in types.OrderBy(t => t.Name))
            {
                ListViewItem item = new ListViewItem(t.Name);
                item.Tag = t;
                yield return item;
            }
        }

        private static IEnumerable<ListViewItem> FormatAssemblyEnums(Assembly typelib, bool com_visible)
        {
            if (typelib.ReflectionOnly)
            {
                yield break;
            }
            var types = typelib.GetTypes().Where(t => t.IsEnum);
            if (com_visible)
            {
                types = types.Where(t => Marshal.IsTypeVisibleFromCom(t));
            }

            foreach (Type t in typelib.GetTypes().Where(t => t.IsEnum).OrderBy(t => t.Name))
            {
                ListViewItem item = new ListViewItem(t.Name);
                item.Tag = t;
                yield return item;
            }
        }

        private void AddGuidItems(ListView list, 
            IEnumerable<ListViewItemWithGuid> items, 
            TabPage tab_page, Guid guid_to_view)
        {
            if (items.Count() > 0)
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

            AddGuidItems(listViewInterfaces, interfaces, tabPageInterfaces, guid_to_view);
            AddGuidItems(listViewClasses, classes, tabPageClasses, guid_to_view);

            if (structs.Count() > 0)
            {
                listViewStructures.Items.AddRange(structs.ToArray());
                listViewStructures.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            else
            {
                tabControl.TabPages.Remove(tabPageStructures);
            }

            if (enums.Count() > 0)
            {
                listViewEnums.Items.AddRange(enums.ToArray());
                listViewEnums.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            else
            {
                tabControl.TabPages.Remove(tabPageEnums);
            }

            textEditor.SetHighlighting("C#");
            textEditor.IsReadOnly = true;
            Text = name;
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
            Type type = tag as Type;
            COMProxyInstanceEntry proxy = tag as COMProxyInstanceEntry;
            NdrComplexTypeReference str = tag as NdrComplexTypeReference;

            if (type != null)
            {
                return TypeToText(type);
            }
            else if (proxy != null)
            {
                return proxy.Format(m_iids_to_names);
            }
            else if (str != null)
            {
                INdrFormatter formatter = DefaultNdrFormatter.Create(m_iids_to_names);
                return formatter.FormatComplexType(str);
            }

            return String.Empty;
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = String.Empty;

            ListView list = sender as ListView;

            if (list.SelectedItems.Count > 0)
            {
                ListViewItem item = list.SelectedItems[0];
                text = GetTextFromTag(item.Tag);
            }

            textEditor.Text = text;
            textEditor.Refresh();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView list = sender as ListView;
            if (list != null && list.SelectedItems.Count > 0)
            {
                ListViewItem item = listViewInterfaces.SelectedItems[0];
                COMRegistryViewer.CopyTextToClipboard(item.Text);
            }
        }

        private void CopyGuid(object sender, COMRegistryViewer.CopyGuidType copy_type)
        {
            ListView list = sender as ListView;
            if (list != null && list.SelectedItems.Count > 0)
            {
                ListViewItemWithGuid item = list.SelectedItems[0] as ListViewItemWithGuid;
                if (item != null)
                {
                    COMRegistryViewer.CopyGuidToClipboard(item.Guid, copy_type);
                }
            }
        }

        private void copyGUIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyGuid(sender, COMRegistryViewer.CopyGuidType.CopyAsString);
        }

        private void copyGUIDCStructureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyGuid(sender, COMRegistryViewer.CopyGuidType.CopyAsStructure);
        }

        private void copyGIUDHexStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyGuid(sender, COMRegistryViewer.CopyGuidType.CopyAsHexString);
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
    }
}
