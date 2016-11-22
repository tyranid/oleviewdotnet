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

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;

namespace OleViewDotNet
{
    public partial class LoadingDialog : Form
    {
        private BackgroundWorker m_worker;
        private RegistryKey m_rootKey;
        private string m_keyPath;
        private string m_dbpath;

        public LoadingDialog(bool user_only, string dbpath)
        {
            m_rootKey = user_only ? Registry.CurrentUser : Registry.ClassesRoot;
            m_keyPath = user_only ? @"Software\Classes" : null;
            m_dbpath = dbpath;
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
            if (m_dbpath != null)
            {
                COMRegistry.Load(m_dbpath);
            }
            else
            {
                if (m_keyPath != null)
                {
                    using (RegistryKey key = m_rootKey.OpenSubKey(m_keyPath))
                    {
                        COMRegistry.Load(key);
                    }
                }
                else
                {
                    COMRegistry.Load(m_rootKey);
                }
            }
        }

        private void RunWorkerCompletedCallback(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                DialogResult = DialogResult.Cancel;
                Error = e.Error;
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
            Close();
        }

        public Exception Error { get; private set; }
    }
}
