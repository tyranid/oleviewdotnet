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

using System;
using System.IO;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class ObjectHexEditor : UserControl
    {
        private COMRegistry m_registry;

        public ObjectHexEditor(COMRegistry registry, byte[] bytes)
        {
            InitializeComponent();
            hexEditor.Bytes = bytes;
            Text = "Hex Editor";
            m_registry = registry;
        }

        private async void btnLoadFromStream_Click(object sender, System.EventArgs e)
        {
            try
            {
                MemoryStream stm = new MemoryStream(hexEditor.Bytes);
                Guid clsid;
                object obj = COMUtilities.OleLoadFromStream(new MemoryStream(hexEditor.Bytes), out clsid);
                await Program.GetMainForm(m_registry).HostObject(m_registry.MapClsidToEntry(clsid), obj);
            }
            catch (Exception ex)
            {
                Program.ShowError(this, ex);
            }
        }

        private async void btnUnmarshal_Click(object sender, EventArgs e)
        {
            try
            {
                MemoryStream stm = new MemoryStream(hexEditor.Bytes);
                Guid clsid;
                object obj = COMUtilities.UnmarshalObject(new MemoryStream(hexEditor.Bytes), out clsid);
                await Program.GetMainForm(m_registry).HostObject(m_registry.MapClsidToEntry(clsid), obj);
            }
            catch (Exception ex)
            {
                Program.ShowError(this, ex);
            }
        }
    }
}
