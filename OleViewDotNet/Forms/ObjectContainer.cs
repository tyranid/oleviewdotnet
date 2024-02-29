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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class ObjectContainer : UserControl
{
    private sealed class GenericAxHost : AxHost
    {
        private readonly object m_pObject;

        public GenericAxHost(object pObject) : base(Guid.Empty.ToString())
        {
            m_pObject = pObject;
        }

        protected override object CreateInstanceCore(Guid clsid)
        {
            return m_pObject;
        }
    }

    private readonly string m_objName;
    private readonly GenericAxHost m_axControl;

    public ObjectContainer(string strObjName, object pObject)
    {
        m_objName = strObjName;
        InitializeComponent();

        try
        {
            ComponentResourceManager resources = new(typeof(ObjectContainer));
            m_axControl = new GenericAxHost(pObject);
            m_axControl.BeginInit();
            SuspendLayout();

            m_axControl.Enabled = true;
            m_axControl.Location = new Point(50, 39);
            m_axControl.Name = "axControl";
            m_axControl.OcxState = (AxHost.State)resources.GetObject("axControl.OcxState");
            m_axControl.Dock = DockStyle.Fill;
            m_axControl.TabIndex = 0;
            Controls.Add(m_axControl);
            m_axControl.EndInit();
            ResumeLayout(false);
            Text = $"{m_objName} Container";
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }
}
