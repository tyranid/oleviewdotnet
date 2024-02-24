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

using OleViewDotNet.Utilities.Format;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

public partial class FormattedObjectControl : UserControl
{
    private object m_selected_obj;

    public FormattedObjectControl()
    {
        InitializeComponent();
        textEditor.SetHighlighting("C#");
        textEditor.IsReadOnly = true;
    }

    private void SetText(string text)
    {
        textEditor.Text = text;
        textEditor.Refresh();
    }

    private void SetObjects(IEnumerable<ICOMFormattable> fs)
    {
        SourceCodeBuilder builder = new();
        foreach (var obj in fs)
        {
            obj.Format(builder);
        }
        SetText(builder.ToString());
    }

    internal object SelectedObject
    {
        get => m_selected_obj;
        set
        {
            m_selected_obj = value;
            if (m_selected_obj is ICOMFormattable formattable)
            {
                SetObjects(new[] { formattable });
            }
            else if (m_selected_obj is IEnumerable<ICOMFormattable> list && list.Any())
            {
                SetObjects(list);
            }
            else
            {
                SetText("<No Formattable Object Selected>");
            }
        }
    }
}
