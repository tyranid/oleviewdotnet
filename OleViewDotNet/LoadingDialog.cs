using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OleViewDotNet
{
    public partial class LoadingDialog : Form
    {
        private BackgroundWorker m_worker;
        private RegistryKey m_rootKey;

        public COMRegistry LoadedReg { get; private set; }


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
            LoadedReg = new COMRegistry(m_rootKey);
        }

        private void RunWorkerCompletedCallback(object sender, RunWorkerCompletedEventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
