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
using OleViewDotNet.Utilities.Format;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

public partial class FormattedObjectControl : UserControl
{
    private COMRegistry m_registry;
    private object m_selected_obj;

    public FormattedObjectControl()
    {
        InitializeComponent();
        textEditor.SetHighlighting("C#");
        textEditor.IsReadOnly = true;
        SetText(string.Empty);
    }

    private void SetText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = m_selected_obj == null ? 
                "<No formattable object selected>" : 
                $"<'{m_selected_obj}' is not formattable.>";
        }

        textEditor.Text = text;
        textEditor.Refresh();
    }

    internal void SetRegistry(COMRegistry registry)
    {
        m_registry = registry;
    }

    internal object SelectedObject
    {
        get => m_selected_obj;
        set
        {
            COMSourceCodeBuilder builder = new(m_registry);
            m_selected_obj = value;
            if (m_selected_obj is ICOMSourceCodeFormattable formattable)
            {
                formattable.Format(builder);
            }
            else if (m_selected_obj is IEnumerable<ICOMSourceCodeFormattable> list && list.Any())
            {
                foreach (var entry in list)
                {
                    entry.Format(builder);
                }
            }
            SetText(builder.ToString());
        }
    }
}
