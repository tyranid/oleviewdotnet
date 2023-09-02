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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet.Forms
{
    public partial class ElevatedFactoryServerTypeViewer : UserControl
    {
        private object _obj;
        private string _name;
        private COMRegistry _registry;
        private COMCLSIDEntry _entry;

        public ElevatedFactoryServerTypeViewer(COMRegistry registry, COMCLSIDEntry entry, string objName, object obj)
        {
            InitializeComponent();
            _obj = obj;
            _name = objName;
            _registry = registry;
            _entry = entry;
            if (_entry != null && _entry.Elevation != null)
            {
                foreach (COMCLSIDEntry vso in _entry.Elevation.VirtualServerObjects.Select(v => registry.MapClsidToEntry(v)))
                {
                    comboBoxClass.Items.Add(vso);
                }
                if (comboBoxClass.Items.Count > 0)
                {
                    comboBoxClass.SelectedIndex = 0;
                }
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                IElevatedFactoryServer factory = (IElevatedFactoryServer)_obj;
                COMCLSIDEntry vso = comboBoxClass.SelectedItem as COMCLSIDEntry;
                if (vso != null)
                {
                    object new_object;
                    Dictionary<string, string> props = new Dictionary<string, string>();
                    props.Add("Name", _name);
                    props.Add("CLSID", vso.Clsid.FormatGuid());
                    factory.ServerCreateElevatedObject(vso.Clsid, COMInterfaceEntry.IID_IUnknown, out new_object);
                    ObjectInformation view = new ObjectInformation(_registry, vso,
                        vso.Name, new_object,
                        props, _registry.GetInterfacesForObject(new_object).ToArray());
                    EntryPoint.GetMainForm(_registry).HostControl(view);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
