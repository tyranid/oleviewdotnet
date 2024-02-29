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
using System.Collections.Generic;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class ViewFilterForm : Form
{
    public ViewFilterForm(RegistryViewerFilter filter, IEnumerable<FilterType> types)
    {
        InitializeComponent();
        viewFilterControl.Filter = filter;
        viewFilterControl.SetTypes(types);
    }

    public RegistryViewerFilter Filter
    {
        get; private set;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        Filter = viewFilterControl.Filter;
        DialogResult = DialogResult.OK;
        Close();
    }
}
