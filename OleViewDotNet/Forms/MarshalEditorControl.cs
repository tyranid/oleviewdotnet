//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017
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
using OleViewDotNet.Marshaling;
using OleViewDotNet.Utilities;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class MarshalEditorControl : UserControl
{
    private readonly COMRegistry m_registry;
    private readonly COMObjRef m_objref;

    public MarshalEditorControl(COMRegistry registry, COMObjRef objref)
    {
        m_objref = objref;
        m_registry = registry;
        InitializeComponent();
        textBoxObjRefType.Text = objref.Flags.ToString();
        textBoxIid.Text = objref.Iid.FormatGuid();
        textBoxIIdName.Text = registry.MapIidToInterface(objref.Iid).Name;
        Control ctl = null;

        if (objref is COMObjRefStandard)
        {
            ctl = new StandardMarshalEditorControl(registry, (COMObjRefStandard)objref);
        }
        else if (objref is COMObjRefCustom)
        {
            ctl = new CustomMarshalEditorControl(registry, (COMObjRefCustom)objref);
        }

        if (ctl is not null)
        {
            tableLayoutPanel.Controls.Add(ctl, 0, 1);
            tableLayoutPanel.SetColumnSpan(ctl, tableLayoutPanel.ColumnCount);
            ctl.Dock = DockStyle.Fill;
        }

        Text = $"Marshal Viewer - {objref.Flags}";
    }
}
