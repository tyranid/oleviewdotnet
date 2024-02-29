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

using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace OleViewDotNet.Forms;

internal partial class DocumentForm : DockContent
{
    private readonly Control _control;

    public DocumentForm(Control c)
    {
        InitializeComponent();
        this.Controls.Add(c);
        _control = c;
        c.Dock = DockStyle.Fill;
        TabText = c.Text;
        c.TextChanged += control_TextChanged;
    }

    private void control_TextChanged(object sender, System.EventArgs e)
    {
        TabText = _control.Text;
    }

    private void closeToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        Close();
    }

    private void closeAllButThisToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        IDockContent[] content = DockPanel.DocumentsToArray();

        foreach (IDockContent c in content)
        {
            if ((c is Form frm) && (frm != this))
            {
                frm.Close();
            }
        }
    }

    private void closeAllToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        closeAllButThisToolStripMenuItem_Click(sender, e);

        Close();
    }

    private void renameToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        using GetTextForm frm = new(TabText);
        frm.Text = "Edit Tab Name";
        if (frm.ShowDialog(this) == DialogResult.OK)
        {
            TabText = frm.Data;
        }
    }
}
