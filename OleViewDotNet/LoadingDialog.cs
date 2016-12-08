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

using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class LoadingDialog : Form
    {
        private class ReportProgress : IProgressReporter
        {
            private bool _cancel;
            private Action<string> _report;

            public void Cancel()
            {
                _cancel = true;
            }

            public void ReportEvent(string data)
            {
                if (_cancel)
                {
                    throw new OperationCanceledException();
                }
                _report(data);
            }

            public ReportProgress(Action<string> report)
            {
                _report = report;
            }
        }


        private ReportProgress m_progress;
        private BackgroundWorker m_worker;
        private RegistryKey m_rootKey;
        private string m_keyPath;
        private string m_dbpath;

        public LoadingDialog(bool user_only, string dbpath)
        {
            m_progress = new ReportProgress(SetLabel);
            m_rootKey = user_only ? Registry.CurrentUser : Registry.ClassesRoot;
            m_keyPath = user_only ? @"Software\Classes" : null;
            m_dbpath = dbpath;
            m_worker = new BackgroundWorker();
            m_worker.DoWork += new DoWorkEventHandler(DoWorkEntry);
            m_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompletedCallback);
            InitializeComponent();
        }

        private void SetLabel(string value)
        {
            Action set = () => SetLabel(value);
            if (InvokeRequired)
            {
                Invoke(set);
            }
            else
            {
                lblProgress.Text = String.Format("Currently Loading {0}. Please Wait.", value);
            }
        }

        private void LoadingDialog_Load(object sender, EventArgs e)
        {                        
            m_worker.RunWorkerAsync();
        }

        private void DoWorkEntry(object sender, DoWorkEventArgs e)
        {
            if (m_dbpath != null)
            {
                Instance = COMRegistry.Load(m_dbpath, m_progress);
            }
            else
            {
                if (m_keyPath != null)
                {
                    using (RegistryKey key = m_rootKey.OpenSubKey(m_keyPath))
                    {
                        Instance = COMRegistry.Load(key, m_progress);
                    }
                }
                else
                {
                    Instance = COMRegistry.Load(m_rootKey, m_progress);
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
        public COMRegistry Instance { get; private set; }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            m_progress.Cancel();
        }
    }
}
