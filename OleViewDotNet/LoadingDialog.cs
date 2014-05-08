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

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OleViewDotNet
{
    public partial class LoadingDialog : Form
    {
        private BackgroundWorker m_worker;
        private RegistryKey m_rootKey;

        public LoadingDialog(RegistryKey rootKey)
        {
            m_rootKey = rootKey;
            m_worker = new BackgroundWorker();
            m_worker.DoWork += new DoWorkEventHandler(DoWorkEntry);
            m_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompletedCallback);
            InitializeComponent();
        }

        private void LoadingDialog_Load(object sender, EventArgs e)
        {                        
            m_worker.RunWorkerAsync();
        }

        private void DoWorkEntry(object sender, DoWorkEventArgs e)
        {
            COMRegistry.Load(m_rootKey);
        }

        private void RunWorkerCompletedCallback(object sender, RunWorkerCompletedEventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
