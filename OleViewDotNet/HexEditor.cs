//    This file is part of OleViewDotNet.
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
using System.IO;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class HexEditor : UserControl
    {
        DynamicByteProvider _bytes;

        public HexEditor()
        {
            InitializeComponent();
            Bytes = new byte[0];
        }

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
                hexBox.Invalidate();
            }
        }

        private void loadFromFileToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
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
        }

        private void saveToFileToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
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
        }
    }
}
