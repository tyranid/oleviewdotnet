using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet
{
    public partial class InvokeForm : Form
    {
        private MethodInfo m_mi;
        private object m_pObject;

        class ParamData
        {
            public ParameterInfo pi;
            public object data;
        }

        private ParamData[] m_paramdata;        

        public InvokeForm(MethodInfo mi, object pObject)
        {
            m_mi = mi;
            m_pObject = pObject;
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
                ParamData data = new ParamData();
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
                if (t.GetElementType() != null)
                {
                    t = t.GetElementType();
                }

                /* No idea why on earth why strings cannot be activated */
                if (t == typeof(string))
                {
                    ret = "";
                }
                else if (t == typeof(IBindCtx))
                {
                    ret = COMUtilities.CreateBindCtx(0);
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
                if (data.data == null)
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

                if (baseType.GetElementType() != null)
                {
                    baseType = baseType.GetElementType();
                }

                if(baseType == typeof(IStream))
                {
                    CreateIStreamForm frm = new CreateIStreamForm();
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        data.data = frm.Stream;
                    }
                }
                else
                {
                    GetTypeForm frm = new GetTypeForm(data.pi.ParameterType, data.data);                
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
                object ret = m_mi.Invoke(m_pObject, p);
                if (ret != null)
                {
                    lblReturn.Text = "Return: " + ret.GetType().ToString();
                    textBoxReturn.Text = ret.ToString();
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
                MessageBox.Show(ex.InnerException.Message, "Invoke Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InvokeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            /* Try and clean up any referenced Com Objects */
            foreach (ParamData data in m_paramdata)
            {
                if (data.data != null)
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
    }
}
