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

using OleViewDotNet.InterfaceViewers;
using System;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace OleViewDotNet
{
    public partial class PersistStreamTypeViewer : DocumentForm
    {
        private object _obj;

        public PersistStreamTypeViewer(string objName, object obj)
        {
            InitializeComponent();
            _obj = obj;
            btnInit.Enabled = obj is IPersistStreamInit;
            Text = objName + " Persist Stream";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (MemoryStream stm = new MemoryStream())
                {
                    COMUtilities.SaveObjectToStream(_obj, stm);
                    hexEditor.Bytes = stm.ToArray();
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnInit_Click(object sender, EventArgs e)
        {
            IPersistStreamInit psi = _obj as IPersistStreamInit;

            if (psi != null)
            {
                try
                {
                    psi.InitNew();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                using (MemoryStream stm = new MemoryStream(hexEditor.Bytes))
                {
                    COMUtilities.LoadObjectFromStream(_obj, stm);                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
