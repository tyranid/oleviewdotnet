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

using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Utilities;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class InvokeForm : Form
{
    private readonly MethodInfo m_mi;
    private readonly object m_pObject;
    private object m_ret;
    private readonly string m_objName;
    private readonly COMRegistry m_registry;

    private class ParamData
    {
        public ParameterInfo pi;
        public object data;
    }

    private ParamData[] m_paramdata;        

    public InvokeForm(COMRegistry registry, MethodInfo mi, object pObject, string objName)
    {
        m_mi = mi;
        m_pObject = pObject;
        m_objName = objName;
        m_registry = registry;
        LoadParameters();

        InitializeComponent(); 
    }

    private void LoadParameters()
    {
        ParameterInfo[] pis = m_mi.GetParameters();
        m_paramdata = new ParamData[pis.Length];

        for (int i = 0; i < pis.Length; i++)
        {
            ParameterInfo pi = pis[i];
            ParamData data = new();
            m_paramdata[i] = data;

            data.pi = pis[i];
            if (!pi.IsOptional)
            {
                data.data = CreateDefaultType(pi.ParameterType);
            }
            else
            {
                data.data = null;
            }
        }
    }

    private object CreateDefaultType(Type t)
    {
        object ret = null;

        try
        {
            if (t.GetElementType() is not null)
            {
                t = t.GetElementType();
            }
           
            if (t == typeof(string))
            {
                ret = string.Empty;
            }
            else if (t == typeof(IBindCtx))
            {
                ret = NativeMethods.CreateBindCtx(0);
            }
            else
            {
                /* Try the default activation route */
                ret = System.Activator.CreateInstance(t);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return ret;
    }

    private void RefreshParameters()
    {
        listViewParameters.SuspendLayout();
        listViewParameters.Clear();

        /* Setup columns */
        listViewParameters.Columns.Add("Name");
        listViewParameters.Columns.Add("Type");
        listViewParameters.Columns.Add("Value");
        listViewParameters.Columns.Add("Dir");
        listViewParameters.Columns.Add("Optional");

        foreach (ParamData data in m_paramdata)
        {
            ListViewItem item = listViewParameters.Items.Add(data.pi.Name);
            item.SubItems.Add(data.pi.ParameterType.ToString());
            if (data.data is null)
            {
                item.SubItems.Add("<null>");
            }
            else
            {
                item.SubItems.Add(data.data.ToString());
            }

            string strDir = "";

            if (data.pi.IsOut)
            {
                strDir += "out";
            }

            if (data.pi.IsIn || (strDir.Length == 0))
            {
                strDir = "in" + strDir;
            }

            item.SubItems.Add(strDir);

            if (data.pi.IsOptional)
            {
                item.SubItems.Add("Yes");
            }
            else
            {
                item.SubItems.Add("No");
            }

            item.Tag = data;
        }

        listViewParameters.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewParameters.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        listViewParameters.ResumeLayout();
    }

    private void InvokeForm_Load(object sender, EventArgs e)
    {            
        RefreshParameters();

        Text = "Invoke " + m_mi.Name;
        lblReturn.Text = "Return: " + m_mi.ReturnType.ToString();
    }

    private void listViewParameters_DoubleClick(object sender, EventArgs e)
    {
        if (listViewParameters.SelectedItems.Count > 0)
        {
            ListViewItem item = listViewParameters.SelectedItems[0];
            ParamData data = (ParamData)item.Tag;

            Type baseType = data.pi.ParameterType;

            if (baseType.GetElementType() is not null)
            {
                baseType = baseType.GetElementType();
            }

            if(baseType == typeof(IStream))
            {
                using CreateIStreamForm frm = new();
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    data.data = frm.Stream;
                }
            }
            else
            {
                using GetTypeForm frm = new(data.pi.ParameterType, data.data);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    data.data = frm.Data;
                }
            }

            RefreshParameters();
        }
    }

    private void btnInvoke_Click(object sender, EventArgs e)
    {            
        object[] p = new object[m_paramdata.Length];
        int i = 0;

        foreach (ParamData data in m_paramdata)
        {                
            p[i++] = data.data;
        }

        try
        {
            m_ret = m_mi.Invoke(m_pObject, p);
            if (m_ret is not null)
            {
                lblReturn.Text = "Return: " + m_ret.GetType().ToString();
                textBoxReturn.Text = m_ret.ToString();

                if (Marshal.IsComObject(m_ret))
                {
                    btnOpenObject.Enabled = true;
                }
            }

            for (i = 0; i < m_paramdata.Length; i++)
            {
                if (m_paramdata[i].pi.IsOut)
                {
                    m_paramdata[i].data = p[i];
                }
            }

            RefreshParameters();
        }
        catch (Exception ex)
        {
            Exception printEx = ex;
            if (printEx.InnerException is not null)
            {
                printEx = printEx.InnerException;
            }

            MessageBox.Show(printEx.Message, "Invoke Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InvokeForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        /* Try and clean up any referenced Com Objects */
        foreach (ParamData data in m_paramdata)
        {
            if (data.data is not null)
            {
                try
                {
                    if (data.data is IStreamImpl)
                    {
                        ((IStreamImpl)data.data).Dispose();
                    }
                    else
                    {
                        while (Marshal.ReleaseComObject(data.data) != 0)
                        {
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }

    private void btnOpenObject_Click(object sender, EventArgs e)
    {
        if (m_ret is not null)
        {
            EntryPoint.GetMainForm(m_registry).HostControl(new TypedObjectViewer(m_registry, m_objName, m_ret, m_mi.ReturnType));
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
