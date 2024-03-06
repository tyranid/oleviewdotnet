//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2024
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
using System;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class SourceCodeNameEditor : Form
{
    private readonly ICOMSourceCodeEditable m_obj;

    public SourceCodeNameEditor(ICOMSourceCodeEditable obj)
    {
        InitializeComponent();
        m_obj = obj;

        textBoxName.Text = obj.Name;
        foreach (var member in obj.Members)
        {
            ListViewItem item = new(member.Name);
            listViewNames.Items.Add(item);
            item.Tag = member;
        }
        listViewNames.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        Text = $"Editing {obj.Name}";
    }

    private void btnOK_Click(object sender, System.EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                throw new FormatException("Name can't be empty.");
            }
            m_obj.Name = textBoxName.Text;
            foreach (ListViewItem item in listViewNames.Items)
            {
                ICOMSourceCodeEditable member = (ICOMSourceCodeEditable)item.Tag;
                member.Name = item.Text;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private void listViewNames_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(e.Label))
            {
                throw new FormatException("Name can't be empty.");
            }

            listViewNames.Items[e.Item].Text = e.Label;
            listViewNames.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
            e.CancelEdit = true;
        }
    }
}
