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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet.Forms
{
    public partial class ClassFactoryTypeViewer : UserControl
    {
        private object _obj;
        private string _name;
        private COMRegistry _registry;
        private ICOMClassEntry _entry;

        public ClassFactoryTypeViewer(COMRegistry registry, ICOMClassEntry entry, string objName, object obj)
        {
            InitializeComponent();
            _obj = obj;
            _name = objName;
            _registry = registry;
            _entry = entry;
            Text = objName + " ClassFactory";
        }

        private void btnCreateInstance_Click(object sender, EventArgs e)
        {
            try
            {
                IClassFactory factory = (IClassFactory)_obj;
                object new_object;
                Guid IID_IUnknown = COMInterfaceEntry.IID_IUnknown;
                Dictionary<string, string> props = new Dictionary<string, string>();
                props.Add("Name", _name);
                factory.CreateInstance(null, ref IID_IUnknown, out new_object);
                ObjectInformation view = new ObjectInformation(_registry, _entry, _name, new_object,
                    props, _registry.GetInterfacesForObject(new_object).ToArray());
                EntryPoint.GetMainForm(_registry).HostControl(view);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
