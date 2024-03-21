//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016, 2017
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
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;
using OleViewDotNet.Interop;
using OleViewDotNet.Wrappers;
using IS = System.Runtime.InteropServices;

namespace OleViewDotNet.Forms;

internal partial class StorageViewer : UserControl
{
    private static string EscapeStorageName(string name)
    {
        if (name is null)
        {
            return name;
        }
        StringBuilder builder = new();
        foreach (char ch in name)
        {
            if (ch < 32)
            {
                switch (ch)
                {
                    case '\0':
                        builder.Append(@"#0");
                        break;
                    case '\n':
                        builder.Append(@"#n");
                        break;
                    case '\r':
                        builder.Append(@"#r");
                        break;
                    case '\t':
                        builder.Append(@"#t");
                        break;
                    case '#':
                        builder.Append(@"##");
                        break;

                }
                builder.AppendFormat(@"#x{0:X02}", (int)ch);
            }
            else
            {
                builder.Append(ch);
            }
        }
        return builder.ToString();
    }

    private enum ParserState
    {
        None,
        InEscape,
        InHexCode
    }

    private static string UnescapeStorageName(string name)
    {
        if (name is null)
        {
            return name;
        }
        StringBuilder builder = new();
        ParserState current_state = ParserState.None;
        string hexcode = string.Empty;
        foreach (char ch in name)
        {
            if (current_state == ParserState.InEscape)
            {
                switch (ch)
                {
                    case '0':
                        builder.Append('\0');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    case '#':
                        builder.Append('#');
                        break;
                    case 'x':
                        current_state = ParserState.InHexCode;
                        hexcode = string.Empty;
                        break;
                    default:
                        throw new ArgumentException($"Invalid escape character {ch}");
                }
                if (current_state == ParserState.InEscape)
                {
                    current_state = ParserState.None;
                }
            }
            else if (current_state == ParserState.InHexCode)
            {
                hexcode += ch;
                if (hexcode.Length == 2)
                {
                    builder.Append((char)int.Parse(hexcode, System.Globalization.NumberStyles.HexNumber));
                    current_state = ParserState.None;
                }
            }
            else
            {
                if (ch == '#')
                {
                    current_state = ParserState.InEscape;
                }
                else
                {
                    builder.Append(ch);
                }
            }
        }

        if (current_state != ParserState.None)
        {
            throw new ArgumentException("Trailing escape at end of string");
        }

        return name;
    }

    private byte[] ReadStream(IStorage stg, string name, int size)
    {
        IStream stm = stg.OpenStream(name, IntPtr.Zero, STGM.READ | STGM.SHARE_EXCLUSIVE, 0);
        try
        {
            byte[] ret = new byte[size];
            stm.Read(ret, size, IntPtr.Zero);
            return ret;
        }
        finally
        {
            IS.Marshal.ReleaseComObject(stm);
        }
    }

    private readonly IStorage _stg;
    private readonly bool _read_only;

    private void PopulateTree(IStorage stg, TreeNode root)
    {
        stg.Stat(out STATSTG stg_stat, 0);
        root.Tag = new STATSTGWrapper(EscapeStorageName(stg_stat.pwcsName), stg_stat, new byte[0]);
        stg.EnumElements(0, IntPtr.Zero, 0, out IEnumSTATSTG enum_stg);
        STATSTG[] stat = new STATSTG[1];
        while (enum_stg.Next(1, stat, out uint fetched) == 0)
        {
            STGTY type = (STGTY)stat[0].type;
            TreeNode node = new(EscapeStorageName(stat[0].pwcsName));
            byte[] bytes = new byte[0];
            node.ImageIndex = 2;
            node.SelectedImageIndex = 2;
            switch (type)
            {
                case STGTY.Storage:
                    IStorage child_stg = stg.OpenStorage(stat[0].pwcsName, IntPtr.Zero, STGM.READ | STGM.SHARE_EXCLUSIVE, IntPtr.Zero, 0);
                    try
                    {
                        PopulateTree(child_stg, node);
                    }
                    finally
                    {
                        IS.Marshal.ReleaseComObject(child_stg);
                    }
                    node.ImageIndex = 0;
                    node.SelectedImageIndex = 0;
                    break;
                case STGTY.Stream:
                    bytes = ReadStream(stg, stat[0].pwcsName, (int)stat[0].cbSize);
                    break;
                default:
                    break;
            }
            node.Tag = new STATSTGWrapper(EscapeStorageName(stat[0].pwcsName), stat[0], bytes);

            root.Nodes.Add(node);
        }
    }

    private void PopulateTree()
    {
        TreeNode root = new("Root");
        PopulateTree(_stg, root);
        treeViewStorage.Nodes.Add(root);
        root.Expand();
    }

    public StorageViewer(IStorage stg, string filename, bool read_only)
    {
        _stg = stg;
        _read_only = read_only;
        Disposed += StorageViewer_Disposed;
        InitializeComponent();
        PopulateTree();
        Text = $"{filename}";
        hexEditorStream.ReadOnly = read_only;
    }

    private void StorageViewer_Disposed(object sender, EventArgs e)
    {
        IS.Marshal.ReleaseComObject(_stg);
    }

    private void treeViewStorage_AfterSelect(object sender, TreeViewEventArgs e)
    {
        propertyGridStat.SelectedObject = e.Node.Tag;
        if (e.Node.Tag is STATSTGWrapper stat)
        {
            hexEditorStream.Bytes = stat.Bytes;
        }
        else
        {
            hexEditorStream.Bytes = new byte[0];
        }
    }

    private void treeViewStorage_AfterCollapse(object sender, TreeViewEventArgs e)
    {
        int index = e.Node.ImageIndex;
        if (index == 1)
        {
            e.Node.ImageIndex = 0;
            e.Node.SelectedImageIndex = 0;
        }
    }

    private void treeViewStorage_AfterExpand(object sender, TreeViewEventArgs e)
    {
        int index = e.Node.ImageIndex;
        if (index == 0)
        {
            e.Node.ImageIndex = 1;
            e.Node.SelectedImageIndex = 1;
        }
    }
}
