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

internal partial class CustomMarshalEditorControl : UserControl
{
    private readonly COMRegistry m_registry;
    private readonly COMObjRefCustom m_objref;

    public CustomMarshalEditorControl(COMRegistry registry, COMObjRefCustom objref)
    {
        m_objref = objref;
        m_registry = registry;
        InitializeComponent();
        textBoxClsid.Text = objref.Clsid.FormatGuid();
        COMCLSIDEntry ent = registry.MapClsidToEntry(objref.Clsid);
        if (ent is not null)
        {
            textBoxName.Text = ent.Name;
        }

        hexEditor.Bytes = objref.ObjectData;
    }
}
