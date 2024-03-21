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
using System.Linq;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class SourceCodeNameEditor : Form
{
    private readonly ICOMSourceCodeEditable m_obj;
    private TreeNode CreateTree(ICOMSourceCodeEditable obj)
    {
        TreeNode node = new(obj.Name);
        node.Tag = obj;
        foreach (var member in obj.Members)
        {
            node.Nodes.Add(CreateTree(member));
        }

        return node;
    }

    private void SetupTree()
    {
        var root = CreateTree(m_obj);
        treeViewEditor.Nodes.Add(root);
        root.Expand();
    }

    public SourceCodeNameEditor(ICOMSourceCodeEditable obj)
    {
        InitializeComponent();
        m_obj = obj;
        SetupTree();
        Text = $"Editing '{obj.Name}' Names";
    }

    private void UpdateNames(TreeNode root)
    {
        if (root.Tag is ICOMSourceCodeEditable editable)
        {
            editable.Name = root.Text;
            foreach (TreeNode node in root.Nodes)
            {
                UpdateNames(node);
            }
        }
    }

    private void btnOK_Click(object sender, System.EventArgs e)
    {
        foreach (TreeNode node in treeViewEditor.Nodes)
        {
            UpdateNames(node);
        }
        DialogResult = DialogResult.OK;
        Close();
    }

    private void treeViewEditor_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Label))
        {
            MessageBox.Show(this, "Name can't be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.CancelEdit = true;
        }
    }

    private void renameToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (treeViewEditor.SelectedNode is not null)
        {
            treeViewEditor.SelectedNode.BeginEdit();
        }
    }

    private void PasteNames(Func<string, string[]> parse_names)
    {
        try
        {
            TreeNode node = treeViewEditor.SelectedNode;
            if (node is null)
            {
                return;
            }

            string text = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            string[] names = parse_names(text);

            if (names.Length != node.Nodes.Count)
            {
                var result = MessageBox.Show(this, "Number of names does not match number of children. Do you want to continue?",
                    "Question?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                    return;
            }

            for (int i = 0; i < Math.Min(names.Length, node.Nodes.Count); ++i)
            {
                node.Nodes[i].Text = names[i];
            }
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(this, ex);
        }
    }

    private void pasteWinDBGDqsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        static string[] parse_names(string t)
        {
            string[] lines = t.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
            for (int i = 0; i < lines.Length; ++i)
            {
                string[] values = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 3)
                    continue;
                string name = values[2];
                int index = name.IndexOf('!');
                if (index >= 0)
                {
                    name = name.Substring(index + 1);
                }
                index = name.IndexOf("::");
                if (index >= 0)
                {
                    name = name.Substring(index + 2);
                }
                index = name.IndexOf('<');
                if (index >= 0)
                {
                    name = name.Substring(index + 1);
                }
                lines[i] = name;
            }
            return lines.Where(l => l.Trim().Length > 0).ToArray();
        }
        PasteNames(parse_names);
    }

    private void resetNamesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        treeViewEditor.Nodes.Clear();
        SetupTree();
    }

    private void pasteListToolStripMenuItem_Click(object sender, EventArgs e)
    {
        PasteNames(t => t.Split('\n').Select(s => s.Trim())
                .Where(s => s.Length > 0 && !s.StartsWith("#")).ToArray());
    }
}
