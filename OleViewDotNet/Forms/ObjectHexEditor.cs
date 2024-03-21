//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
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
using System;
using System.IO;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class ObjectHexEditor : UserControl
{
    private readonly COMRegistry m_registry;

    public ObjectHexEditor(COMRegistry registry, string name, byte[] bytes)
    {
        InitializeComponent();
        hexEditor.Bytes = bytes;
        if (name is not null)
        {
            Text = name;
        }
        else
        {
            Text = "Hex Editor";
        }
        m_registry = registry;
    }

    private async void btnLoadFromStream_Click(object sender, System.EventArgs e)
    {
        try
        {
            MemoryStream stm = new(hexEditor.Bytes);
            object obj = COMUtilities.OleLoadFromStream(new MemoryStream(hexEditor.Bytes), out Guid clsid);
            await EntryPoint.GetMainForm(m_registry).HostObject(m_registry.MapClsidToEntry(clsid), obj, false);
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private async void btnUnmarshal_Click(object sender, EventArgs e)
    {
        try
        {
            MemoryStream stm = new(hexEditor.Bytes);
            object obj = COMUtilities.UnmarshalObject(hexEditor.Bytes);
            await EntryPoint.GetMainForm(m_registry).OpenObjectInformation(obj, "Unmarshaled Object");
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private void btnMarshalProps_Click(object sender, EventArgs e)
    {
        try
        {
            COMObjRef objref = COMObjRef.FromArray(hexEditor.Bytes);
            EntryPoint.GetMainForm(m_registry).HostControl(new MarshalEditorControl(m_registry, objref));
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }
}
