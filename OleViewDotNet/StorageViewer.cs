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
using System.ComponentModel;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class StorageViewer : UserControl
    {
        class STATSTGWrapper
        {
            private static DateTime FromFileTime(FILETIME filetime)
            {
                long result = (uint)filetime.dwLowDateTime | (((long)filetime.dwHighDateTime) << 32);
                return DateTime.FromFileTime(result);
            }

            public STATSTGWrapper(STATSTG stat, byte[] bytes)
            {
                Name = stat.pwcsName;
                Type = (STGTY)stat.type;
                Size = stat.cbSize;
                ModifiedTime = FromFileTime(stat.mtime);
                CreationTime = FromFileTime(stat.ctime);
                AccessTime = FromFileTime(stat.atime);
                Mode = (STGM)stat.grfMode;
                LocksSupported = stat.grfLocksSupported;
                Clsid = stat.clsid;
                StateBits = stat.grfStateBits;
                Bytes = bytes;
            }

            public string Name { get; private set; }
            public STGTY Type { get; private set; }
            public long Size { get; private set; }
            public DateTime ModifiedTime { get; private set; }
            public DateTime CreationTime { get; private set; }
            public DateTime AccessTime { get; private set; }
            public STGM Mode { get; private set; }
            public int LocksSupported { get; private set; }
            public Guid Clsid { get; private set; }
            public int StateBits { get; private set; }
            [Browsable(false)]
            public byte[] Bytes { get; private set; }
        }

        private byte[] ReadStream(IStorage stg, string name, int size)
        {
            IStream stm = stg.OpenStream(name, IntPtr.Zero, STGM.READ | STGM.SHARE_EXCLUSIVE, 0);
            byte[] ret = new byte[size];
            stm.Read(ret, size, IntPtr.Zero);
            return ret;
        }

        IStorage _stg;

        private void PopulateTree(IStorage stg, TreeNode root)
        {
            IEnumSTATSTG enum_stg;
            stg.EnumElements(0, IntPtr.Zero, 0, out enum_stg);
            STATSTG[] stat = new STATSTG[1];
            uint fetched;
            while (enum_stg.Next(1, stat, out fetched) == 0)
            {
                STGTY type = (STGTY)stat[0].type;
                TreeNode node = new TreeNode(stat[0].pwcsName);
                byte[] bytes = new byte[0];
                node.ImageIndex = 2;
                node.SelectedImageIndex = 2;
                switch (type)
                {
                    case STGTY.Storage:
                        PopulateTree(stg.OpenStorage(stat[0].pwcsName, IntPtr.Zero, STGM.READ | STGM.SHARE_EXCLUSIVE, IntPtr.Zero, 0), node);
                        node.ImageIndex = 0;
                        node.SelectedImageIndex = 0;
                        break;
                    case STGTY.Stream:
                        bytes = ReadStream(stg, stat[0].pwcsName, (int)stat[0].cbSize);
                        break;
                    default:
                        break;
                }
                node.Tag = new STATSTGWrapper(stat[0], bytes);

                root.Nodes.Add(node);
            }
        }

        private void PopulateTree()
        {
            TreeNode root = new TreeNode("Root");
            PopulateTree(_stg, root);
            treeViewStorage.Nodes.Add(root);
            root.Expand();
        }

        public StorageViewer(IStorage stg, string filename)
        {
            _stg = stg;
            Disposed += StorageViewer_Disposed;
            InitializeComponent();
            PopulateTree();
            Text = string.Format("Storage {0}", filename);
        }

        private void StorageViewer_Disposed(object sender, EventArgs e)
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(_stg);
        }

        private void treeViewStorage_AfterSelect(object sender, TreeViewEventArgs e)
        {
            propertyGridStat.SelectedObject = e.Node.Tag;
            STATSTGWrapper stat = e.Node.Tag as STATSTGWrapper;
            if (stat != null)
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
}
