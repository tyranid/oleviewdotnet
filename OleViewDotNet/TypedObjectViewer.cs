using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using WeifenLuo.WinFormsUI.Docking;

namespace OleViewDotNet
{
    /// <summary>
    /// Generic interface viewer from a type
    /// </summary>
    public partial class TypedObjectViewer : DockContent
    {
        private string m_objName;
        private object m_pObject;
        private Type m_dispType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strObjName">Descriptive name of the object</param>
        /// <param name="pObject">Instance of the object</param>
        /// <param name="dispType">Reflected type</param>
        public TypedObjectViewer(string strObjName, object pObject, Type dispType)
        {
            m_pObject = pObject;
            m_objName = strObjName;
            m_dispType = dispType;
            InitializeComponent();
        }

        private void LoadDispatch()
        {            
            listViewMethods.Columns.Add("Name");
            listViewMethods.Columns.Add("Return");
            listViewMethods.Columns.Add("Params");
            listViewProperties.Columns.Add("Name");
            listViewProperties.Columns.Add("Type");
            listViewProperties.Columns.Add("Value");

            lblName.Text = "Name: " + m_dispType.Name;
            MemberInfo[] members = m_dispType.GetMembers();
            foreach (MemberInfo info in members)
            {
                if (info.MemberType == MemberTypes.Method)
                {
                    MethodInfo mi = (MethodInfo)info;
                    ListViewItem item = listViewMethods.Items.Add(mi.Name);
                    item.Tag = mi;
                    StringBuilder pars = new StringBuilder();
                    item.SubItems.Add(mi.ReturnType.ToString());
                    ParameterInfo[] pis = mi.GetParameters();
                    
                    foreach (ParameterInfo pi in pis)
                    {
                        string strDir = "";

                        if (pi.IsIn)
                        {
                            strDir += "in";
                        }

                        if (pi.IsOut)
                        {
                            strDir += "out";
                        }

                        if (strDir.Length == 0)
                        {
                            strDir = "in";
                        }

                        if (pi.IsRetval)
                        {
                            strDir += " retval";
                        }

                        if (pi.IsOptional)
                        {
                            strDir += " optional";
                        }

                        pars.AppendFormat("[{0}] {1} {2}, ", strDir, pi.ParameterType.Name, pi.Name);
                    }

                    item.SubItems.Add(pars.ToString());
                }
                else if (info.MemberType == MemberTypes.Property)
                {
                    PropertyInfo pi = (PropertyInfo)info;
                    ListViewItem item = listViewProperties.Items.Add(pi.Name);
                    item.Tag = pi;
                    item.SubItems.Add(pi.PropertyType.ToString());
                    object val = null;

                    try
                    {
                        if (pi.CanRead)
                        {
                            val = pi.GetValue(m_pObject, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        val = null;
                    }

                    if (val != null)
                    {
                        item.SubItems.Add(val.ToString());
                    }
                    else
                    {
                        item.SubItems.Add("<null>");
                    }
                }
            }

            listViewMethods.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void UpdateProperties()
        {
            foreach (ListViewItem item in listViewProperties.Items)
            {
                PropertyInfo pi = (PropertyInfo)item.Tag;
                object val = null;

                try
                {
                    if (pi.CanRead)
                    {
                        val = pi.GetValue(m_pObject, null);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    val = null;
                }

                if (val != null)
                {                    
                    item.SubItems[2].Text = val.ToString();
                }
                else
                {
                    item.SubItems[2].Text = "<null>";
                }

            }
            listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }


        private void listViewMethods_DoubleClick(object sender, EventArgs e)
        {
            if (listViewMethods.SelectedItems.Count > 0)
            {
                InvokeForm frm = new InvokeForm((MethodInfo)listViewMethods.SelectedItems[0].Tag, m_pObject);
                frm.ShowDialog();
                UpdateProperties();
            }
        }

        private void listViewProperties_DoubleClick(object sender, EventArgs e)
        {
            if (listViewProperties.SelectedItems.Count > 0)
            {
                PropertyInfo pi = (PropertyInfo)listViewProperties.SelectedItems[0].Tag;

                if (pi.CanWrite)
                {
                    object val = null;

                    try
                    {
                        if (pi.CanRead)
                        {
                            val = pi.GetValue(m_pObject, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        val = null;
                    }

                    GetTypeForm frm = new GetTypeForm(pi.PropertyType, val);
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            pi.SetValue(m_pObject, frm.Data, null);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                        }
                        UpdateProperties();
                    }
                }
            }
        }

        private void TypedObjectViewer_Load(object sender, EventArgs e)
        {
            if (m_dispType != null)
            {
                LoadDispatch();
                TabText = String.Format("{0} {1}", m_objName, m_dispType.Name);
            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateProperties();
        }

        private void OpenObject(ListView lv)
        {
            if (lv.SelectedItems.Count > 0)
            {
                ListViewItem item = lv.SelectedItems[0];
                if (item.Tag is PropertyInfo)
                {
                    PropertyInfo pi = (PropertyInfo)item.Tag;
                    object val = null;

                    try
                    {
                        if (pi.CanRead)
                        {
                            val = pi.GetValue(m_pObject, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        val = null;
                    }

                    if (val != null)
                    {
                        TypedObjectViewer view = new TypedObjectViewer(m_objName, val, pi.PropertyType);
                        view.ShowHint = DockState.Document;
                        view.Show(this.DockPanel);                
                    }
                }
            }
        }

        private void openObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenObject(listViewProperties);            
        }

        private void openObjectToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenObject(listViewMethods);
        }
    }
}
