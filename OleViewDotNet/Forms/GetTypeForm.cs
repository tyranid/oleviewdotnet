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
using System.Reflection;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class GetTypeForm : Form
{
    /// <summary>
    /// Dictionary of previous entries to the type field
    /// </summary>
    private static readonly Dictionary<Guid, string[]> m_history = new();
    private const int MAX_HISTORY_ENTRIES = 10;

    private readonly Type m_currType;
    private object m_data;

    public object Data => m_data;

    public GetTypeForm(Type currType, object data)
    {
        if (currType.GetElementType() is not null)
        {
            m_currType = currType.GetElementType();
        }
        else
        {
            m_currType = currType;
        }
        
        m_data = data;

        InitializeComponent();
    }

    private void GetTypeForm_Load(object sender, EventArgs e)
    {
        if (m_data is null)
        {
            checkBoxSetNULL.Checked = true;
        }
        
        if (m_currType == typeof(object))
        {
            comboBoxTypes.Items.Add(typeof(string));
            comboBoxTypes.Items.Add(typeof(byte));
            comboBoxTypes.Items.Add(typeof(sbyte));
            comboBoxTypes.Items.Add(typeof(ushort));
            comboBoxTypes.Items.Add(typeof(short));
            comboBoxTypes.Items.Add(typeof(uint));
            comboBoxTypes.Items.Add(typeof(int));
            comboBoxTypes.Items.Add(typeof(ulong));
            comboBoxTypes.Items.Add(typeof(long));
            comboBoxTypes.Items.Add(typeof(bool));
            comboBoxTypes.Items.Add(typeof(double));
            comboBoxTypes.Items.Add(typeof(float));
            comboBoxTypes.Items.Add(typeof(decimal));
            comboBoxTypes.Items.Add(typeof(DateTime));
            comboBoxTypes.Items.Add(typeof(Guid));
        }
        else
        {
            comboBoxTypes.Items.Add(m_currType);
            comboBoxTypes.Enabled = false;
        }

        if (m_data is not null)
        {
            comboBoxValue.Items.Add(m_data.ToString());
            comboBoxValue.SelectedIndex = 0;
        }

        comboBoxTypes.SelectedIndex = 0;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        try
        {
            if (checkBoxSetNULL.Checked == false)
            {
                Type t = (Type)comboBoxTypes.SelectedItem;

                if (t is not null)
                {
                    /* First check if this type has a constructor which takes a string */
                    ConstructorInfo ci = t.GetConstructor(new Type[] { typeof(string) });
                    if (ci is not null)
                    {
                        m_data = ci.Invoke(new object[] { comboBoxValue.Text });
                    }                        
                    else
                    {
                        /* Try default conversion */
                        m_data = Convert.ChangeType(comboBoxValue.Text, t);
                    }

                    if (!m_history.ContainsKey(t.GUID))
                    {
                        m_history[t.GUID] = new string[MAX_HISTORY_ENTRIES];                            
                    }
                    Array.Copy(m_history[t.GUID], 0, m_history[t.GUID], 1, MAX_HISTORY_ENTRIES - 1);
                    m_history[t.GUID][0] = comboBoxValue.Text;
                }
            }
            else
            {
                m_data = null;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Type Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }            
    }

    private void comboBoxTypes_SelectedIndexChanged(object sender, EventArgs e)
    {
        Type t = (Type)comboBoxTypes.SelectedItem;
        comboBoxValue.Items.Clear();
        if (t is not null)
        {
            if (m_history.ContainsKey(t.GUID))
            {
                foreach (string s in m_history[t.GUID])
                {
                    if (s is not null)
                    {
                        comboBoxValue.Items.Add(s);
                    }
                }
                comboBoxValue.Text = "";
            }
        }
    }
}
