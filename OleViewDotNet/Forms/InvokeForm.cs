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

using NtApiDotNet.Win32.Rpc;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.TypeManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class InvokeForm : Form
{
    private readonly MethodBase m_mi;
    private readonly object m_pObject;
    private object m_ret;
    private readonly string m_objName;
    private readonly COMRegistry m_registry;

    private class ParamData
    {
        public ParameterInfo pi;
        public object data;
    }

    private List<ParamData> m_paramdata;

    private static bool IsComObject(object obj)
    {
        return Marshal.IsComObject(obj) || obj is ICOMObjectWrapper;
    }

    public InvokeForm(COMRegistry registry, MethodBase mi, object pObject, string objName)
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
        m_paramdata = new();

        for (int i = 0; i < pis.Length; i++)
        {
            ParameterInfo pi = pis[i];
            ParamData data = new();
            m_paramdata.Add(data);

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
            if (t.IsByRef)
            {
                t = t.GetElementType();
            }

            if (t == typeof(string))
            {
                ret = string.Empty;
            }
            else if (t == typeof(byte[]))
            {
                ret = Array.Empty<byte>();
            }
            else if (t == typeof(IBindCtx))
            {
                ret = NativeMethods.CreateBindCtx(0);
            }
            else if (!t.IsAbstract)
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
            item.SubItems.Add(FormUtils.FormatObject(data.data, false));

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
        if (m_mi is MethodInfo mi)
        {
            lblReturn.Text = "Return: " + mi.ReturnType.ToString();
        }
    }

    private ParamData GetSelectedParam()
    {
        if (listViewParameters.SelectedItems.Count > 0)
        {
            ListViewItem item = listViewParameters.SelectedItems[0];
            return item.Tag as ParamData;
        }
        return null;
    }

    private void SetValue_Handler(object sender, EventArgs e)
    {
        ParamData data = GetSelectedParam();
        if (data is null)
        {
            return;
        }
        Type baseType = data.pi.ParameterType;

        if (baseType.IsByRef)
        {
            baseType = baseType.GetElementType();
        }

        if (baseType == typeof(IStream))
        {
            using CreateIStreamForm frm = new();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                data.data = frm.Stream;
            }
        }
        else if (baseType == typeof(byte[]))
        {
            using HexEditorForm frm = new();
            frm.Bytes = data.data as byte[];
            if (frm.ShowDialog() == DialogResult.OK)
            {
                data.data = frm.Bytes;
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

    private void btnInvoke_Click(object sender, EventArgs e)
    {
        object[] p = new object[m_paramdata.Count];
        int i = 0;

        foreach (ParamData data in m_paramdata)
        {
            p[i++] = data.data;
        }

        try
        {
            if (m_mi is ConstructorInfo ci)
            {
                m_ret = ci.Invoke(p);
            }
            else
            {
                m_ret = m_mi.Invoke(m_pObject, p);
            }
            if (m_ret is not null)
            {
                lblReturn.Text = "Return: " + m_ret.GetType().ToString();
                textBoxReturn.Text = FormUtils.FormatObject(m_ret, true);

                if (IsComObject(m_ret))
                {
                    btnOpenObject.Enabled = true;
                }
            }

            for (i = 0; i < m_paramdata.Count; i++)
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

            EntryPoint.ShowError(this, ex);
        }
    }

    private void InvokeForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        /* Try and clean up any referenced Com Objects */
        m_paramdata.Clear();
        GC.Collect();
    }

    private void OpenObject(object obj, Type type, bool info, bool close)
    {
        if (!IsComObject(obj))
        {
            return;
        }

        Control c;
        if (info)
        {
            obj = COMTypeManager.Unwrap(obj);
            c = new ObjectInformation(m_registry, null, m_objName, obj,
                new(), m_registry.GetInterfacesForObject(obj).ToArray());
        }
        else
        {
            if (obj is COMObjectWrapper wrapper)
            {
                type = wrapper.Type;
                obj = wrapper.Unwrap();
            }

            c = new TypedObjectViewer(m_registry, m_objName, obj, type);
        }
        EntryPoint.GetMainForm(m_registry).HostControl(c);
        if (close)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private void btnOpenObject_Click(object sender, EventArgs e)
    {
        Type type;
        if (m_mi is MethodInfo mi)
        {
            type = mi.ReturnType;
        }
        else
        {
            type = m_mi.DeclaringType;
        }
        OpenObject(m_ret, type, false, true);
    }

    private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
    {
        ParamData data = GetSelectedParam();
        if (data is null)
        {
            return;
        }
        bool is_com_obj = IsComObject(data.data);
        openObjectToolStripMenuItem.Enabled = is_com_obj;
        openObjectInformationToolStripMenuItem.Enabled = is_com_obj;
    }

    private void openObjectToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ParamData data = GetSelectedParam();
        if (data is null)
        {
            return;
        }

        try
        {
            Type type = data.pi.ParameterType;
            if (data.data is ICOMObjectWrapper)
            {
                type = data.data.GetType();
            }

            OpenObject(data.data, type, false, false);
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private void openObjectInformationToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ParamData data = GetSelectedParam();
        if (data is null)
        {
            return;
        }

        try
        {
            Type type = data.pi.ParameterType;
            if (data.data is RpcClientBase)
            {
                type = data.data.GetType();
            }

            OpenObject(data.data, type, true, false);
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }
}
