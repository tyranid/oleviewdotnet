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
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class RegistryPropertiesControl : UserControl
{
    private void AddListItem(string name, object value)
    {
        if (value is not null)
        {
            ListViewItem item = listViewProperties.Items.Add(name);
            item.SubItems.Add(value.ToString());
        }
    }

    public RegistryPropertiesControl(COMRegistry registry)
    {
        InitializeComponent();
        AddListItem("Created Date", registry.CreatedDate);
        AddListItem("Created Machine", registry.CreatedMachine);
        AddListItem("Created User", registry.CreatedUser);
        AddListItem("Loading Mode", registry.LoadingMode);
        AddListItem("64bit", registry.SixtyFourBit);
        AddListItem("Architecture", registry.Architecture);
        AddListItem("File Path", registry.FilePath);
        AddListItem("CLSID Count", registry.Clsids.Count);
        AddListItem("InProcServer CLSID Count", registry.Clsids.Values.Where(c => c.Servers.ContainsKey(COMServerType.InProcServer32)).Count());
        AddListItem("LocalServer CLSID Count", registry.Clsids.Values.Where(c => c.Servers.ContainsKey(COMServerType.LocalServer32)).Count());
        AddListItem("InProcHandler CLSID Count", registry.Clsids.Values.Where(c => c.Servers.ContainsKey(COMServerType.InProcHandler32)).Count());
        AddListItem("AppID Count", registry.AppIDs.Count);
        AddListItem("ProgID Count", registry.Progids.Count);
        AddListItem("Interfaces Count", registry.Interfaces.Count);
        AddListItem("Implemented Categories Count", registry.ImplementedCategories.Count);
        AddListItem("Low Rights Policy Count", registry.LowRights.Count());
        AddListItem("MIME Types Count", registry.MimeTypes.Count());
        AddListItem("Pre-Approved Count", registry.PreApproved.Count());
        AddListItem("Type Libs Count", registry.Typelibs.Count);
        AddListItem("Runtime Class Count", registry.RuntimeClasses.Count);
        AddListItem("Runtime Server Count", registry.RuntimeServers.Count);
        listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listViewProperties.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        Text = "Registry Properties";
    }
}
