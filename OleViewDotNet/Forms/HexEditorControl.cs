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

using Be.Windows.Forms;
using OleViewDotNet.Utilities;
using System;
using System.IO;
using System.Windows.Forms;

namespace OleViewDotNet.Forms;

internal partial class HexEditorControl : UserControl
{
    private DynamicByteProvider _bytes;

    public HexEditorControl()
    {
        InitializeComponent();
        Bytes = new byte[0];
    }

    public event EventHandler BytesChanged;

    public byte[] Bytes
    {
        get
        {
            return _bytes.Bytes.ToArray();
        }

        set
        {
            _bytes = new DynamicByteProvider(value);
            hexBox.ByteProvider = _bytes;
            _bytes.Changed += Bytes_Changed;
            hexBox.Invalidate();
        }
    }

    private void Bytes_Changed(object sender, EventArgs e)
    {
        BytesChanged?.Invoke(this, new EventArgs());
    }

    public bool ReadOnly
    {
        get
        {
            return hexBox.ReadOnly;
        }

        set
        {
            hexBox.ReadOnly = value;
        }
    }

    private void loadFromFileToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        using OpenFileDialog dlg = new();
        dlg.Filter = "All Files (*.*)|*.*";

        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                Bytes = File.ReadAllBytes(dlg.FileName);
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void saveToFileToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        using SaveFileDialog dlg = new();
        dlg.Filter = "All Files (*.*)|*.*";

        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                File.WriteAllBytes(dlg.FileName, Bytes);
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void copyToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        if (hexBox.CanCopy())
        {
            hexBox.Copy();
        }
    }

    private void pasteToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        if (hexBox.CanPaste())
        {
            hexBox.Paste();
        }
    }

    private void pasteHexToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        if (hexBox.CanPasteHex())
        {
            hexBox.PasteHex();
        }
    }
    
    private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        pasteToolStripMenuItem.Enabled = hexBox.CanPaste();
        pasteHexToolStripMenuItem.Enabled = hexBox.CanPasteHex();
        copyToolStripMenuItem.Enabled = hexBox.CanCopy();
        copyHexToolStripMenuItem.Enabled = hexBox.CanCopy();
        cutToolStripMenuItem.Enabled = hexBox.CanCut();
        copyGuidToolStripMenuItem.Enabled = hexBox.CanCopy() && hexBox.SelectionLength == 16;
    }

    private void copyHexToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        if (hexBox.CanCopy())
        {
            hexBox.CopyHex();
        }
    }

    private void cutToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        if (hexBox.CanCut())
        {
            hexBox.Cut();
        }
    }

    private byte[] GetSelectedBytes()
    {
        byte[] result = new byte[hexBox.SelectionLength]; 
        for (int i = 0; i < result.Length; ++i)
        {
            result[i] = _bytes.ReadByte(hexBox.SelectionStart + i);
        }
        return result;
    }

    private void copyGuidToolStripMenuItem_Click(object sender, System.EventArgs e)
    {
        if (hexBox.CanCopy() && hexBox.SelectionLength == 16)
        {
            byte[] bytes = GetSelectedBytes();
            MiscUtilities.CopyTextToClipboard(new Guid(bytes).FormatGuid());
        }
    }

    private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
        hexBox.SelectAll();
    }

}
