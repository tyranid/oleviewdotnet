﻿//    This file is part of OleViewDotNet.
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
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

public partial class SourceCodeViewerControl : UserControl
{
    private COMRegistry m_registry;
    private object m_selected_obj;
    private ICOMSourceCodeFormattable m_formattable_obj;
    private COMSourceCodeBuilderType m_output_type;
    private bool m_remove_comments;
    private bool m_remove_complex_types;

    public SourceCodeViewerControl()
    {
        InitializeComponent();
        textEditor.SetHighlighting("C#");
        textEditor.IsReadOnly = true;
        m_remove_comments = true;
        toolStripMenuItemRemoveComments.Checked = true;
        m_remove_complex_types = true;
        toolStripMenuItemRemoveComplexTypes.Checked = true;
        toolStripMenuItemIDLOutputType.Checked = true;
        m_output_type = COMSourceCodeBuilderType.Idl;
        SetText(string.Empty);
    }

    private void SetText(string text)
    {
        textEditor.Text = text.TrimEnd();
        textEditor.Refresh();
    }

    internal void SetRegistry(COMRegistry registry)
    {
        m_registry = registry;
    }

    private static bool IsParsed(ICOMSourceCodeFormattable obj)
    {
        if (obj is ICOMSourceCodeParsable parsable)
        {
            return parsable.IsParsed;
        }
        return true;
    }

    internal void Format(COMSourceCodeBuilder builder, ICOMSourceCodeFormattable formattable)
    {
        if (!IsParsed(formattable))
        {
            builder.AppendLine($"'{m_selected_obj}' needs to be parsed before it can be shown.");
        }
        else
        {
            formattable.Format(builder);
        }
    }

    internal void Format()
    {
        COMSourceCodeBuilder builder = new(m_registry)
        {
            RemoveComplexTypes = m_remove_complex_types,
            RemoveComments = m_remove_comments,
            OutputType = m_output_type
        };

        if (m_formattable_obj?.IsFormattable == true)
        {
            Format(builder, m_formattable_obj);
        }
        else
        {
            builder.AppendLine(m_selected_obj == null ? 
                "No formattable object selected" 
                : $"'{m_selected_obj}' is not formattable.");
        }
        SetText(builder.ToString());
    }

    internal object SelectedObject
    {
        get => m_selected_obj;
        set
        {
            m_selected_obj = value;
            if (value is IEnumerable<ICOMSourceCodeFormattable> list)
            {
                m_formattable_obj = new FormattableList(list);
            }
            else if (value is ICOMSourceCodeFormattable formattable)
            {
                m_formattable_obj = formattable;
            }
            else
            {
                m_formattable_obj = null;
            }

            parseSourceCodeToolStripMenuItem.Enabled = m_formattable_obj != null && !IsParsed(m_formattable_obj);
            Format();
        }
    }

    private void toolStripMenuItemIDLOutputType_Click(object sender, System.EventArgs e)
    {
        m_output_type = COMSourceCodeBuilderType.Idl;
        toolStripMenuItemIDLOutputType.Checked = true;
        toolStripMenuItemCppOutputType.Checked = false;
        toolStripMenuItemGenericOutputType.Checked = false;
    }

    private void toolStripMenuItemCppOutputType_Click(object sender, System.EventArgs e)
    {
        m_output_type = COMSourceCodeBuilderType.Cpp;
        toolStripMenuItemIDLOutputType.Checked = false;
        toolStripMenuItemCppOutputType.Checked = true;
        toolStripMenuItemGenericOutputType.Checked = false;
    }

    private void toolStripMenuItemGenericOutputType_Click(object sender, System.EventArgs e)
    {
        m_output_type = COMSourceCodeBuilderType.Generic;
        toolStripMenuItemIDLOutputType.Checked = false;
        toolStripMenuItemCppOutputType.Checked = false;
        toolStripMenuItemGenericOutputType.Checked = true;
    }

    private void toolStripMenuItemRemoveComments_Click(object sender, System.EventArgs e)
    {
        m_remove_comments = !m_remove_comments;
        toolStripMenuItemRemoveComments.Checked = m_remove_comments;
    }

    private void toolStripMenuItemRemoveComplexTypes_Click(object sender, System.EventArgs e)
    {
        m_remove_complex_types = !m_remove_complex_types;
        toolStripMenuItemRemoveComplexTypes.Checked = m_remove_complex_types;
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

    private void parseSourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            if (m_formattable_obj is ICOMSourceCodeParsable parsable && !parsable.IsParsed)
            {
                parsable.ParseSourceCode();
                parseSourceCodeToolStripMenuItem.Enabled = false;
                Format();
            }
        }
        catch (Exception ex)
        {
            m_formattable_obj = new SimpleTextFormattable($"ERROR: {ex.Message}");
        }
    }
}