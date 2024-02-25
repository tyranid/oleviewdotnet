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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

public partial class SourceCodeViewerControl : UserControl
{
    private COMRegistry m_registry;
    private object m_selected_obj;
    private COMSourceCodeBuilderType m_output_type;
    private bool m_remove_comments;
    private bool m_remove_complex_types;

    public SourceCodeViewerControl()
    {
        InitializeComponent();
        textEditor.SetHighlighting("C#");
        textEditor.IsReadOnly = true;
        m_remove_comments = true;
        removeCommentsToolStripMenuItem.Checked = true;
        m_remove_complex_types = true;
        removeComplexTypesToolStripMenuItem.Checked = true;
        iDLToolStripMenuItem.Checked = true;
        m_output_type = COMSourceCodeBuilderType.Idl;
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

    internal void Format()
    {
        COMSourceCodeBuilder builder = new(m_registry)
        {
            RemoveComplexTypes = m_remove_complex_types,
            RemoveComments = m_remove_comments,
            OutputType = m_output_type
        };
        if (m_selected_obj is ICOMSourceCodeFormattable formattable)
        {
            formattable.Format(builder);
        }
        else if (m_selected_obj is IEnumerable<ICOMSourceCodeFormattable> list && list.Any())
        {
            builder.AppendObjects(list);
        }
        SetText(builder.ToString().TrimEnd());
    }

    internal object SelectedObject
    {
        get => m_selected_obj;
        set
        {
            m_selected_obj = value;
            Format();
        }
    }

    private void iDLToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        m_output_type = COMSourceCodeBuilderType.Idl;
        iDLToolStripMenuItem.Checked = true;
        cToolStripMenuItem.Checked = false;
        genericToolStripMenuItem.Checked = false;
    }

    private void cToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        m_output_type = COMSourceCodeBuilderType.Cpp;
        iDLToolStripMenuItem.Checked = false;
        cToolStripMenuItem.Checked = true;
        genericToolStripMenuItem.Checked = false;
    }

    private void genericToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        m_output_type = COMSourceCodeBuilderType.Generic;
        iDLToolStripMenuItem.Checked = false;
        cToolStripMenuItem.Checked = false;
        genericToolStripMenuItem.Checked = true;
    }

    private void removeCommentsToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        m_remove_comments = !m_remove_comments;
        removeCommentsToolStripMenuItem.Checked = m_remove_comments;
    }

    private void removeComplexTypesToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        m_remove_complex_types = !m_remove_complex_types;
        removeComplexTypesToolStripMenuItem.Checked = m_remove_complex_types;
    }

    private void exportToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        using SaveFileDialog dlg = new();
        dlg.Filter = "All Files (*.*)|*.*";
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                File.WriteAllText(dlg.FileName, textEditor.Text);
            }
            catch (Exception ex)
            {
                EntryPoint.ShowError(this, ex);
            }
        }
    }
}
