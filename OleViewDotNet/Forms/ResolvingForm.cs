using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace OleViewDotNet.Forms
{

    // This form is for ResolveMethod.
    public partial class ResolvingForm : Form
    {
        public bool resolveDone;
        public ResolvingForm()
        {
            InitializeComponent();
        }

        public ResolvingForm(List<String> binaryPath)
        {
            InitializeComponent();
            this.progressBar1.Step = 10000 / (binaryPath.Count * 3);
            this.resolveDone = false;
            this.FormClosed += MainFormClosed;
        }

        private void MainFormClosed(object sender, FormClosedEventArgs e)
        {
            resolveDone = true;
            try
            {
                Process[] processes = Process.GetProcessesByName("idat64");
                if (processes.Length == 0) return;
                foreach (Process process in processes)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                    catch { }
                }
            }
            catch { }
        }

        public void Update(String label1, String label2)
        {
            if (label1 != null)
            {
                if (this.label1.InvokeRequired) this.label1.BeginInvoke(new Action(() => this.label1.Text = label1));
                else this.label1.Text = label1;
            }

            if (label2 != null) 
            {
                if (this.label2.InvokeRequired) this.label2.BeginInvoke(new Action(() => this.label2.Text = label2));
                else this.label2.Text = label2;
            }
            else if (label2 != null) this.label2.Text = label2;

            if (this.progressBar1.InvokeRequired) this.progressBar1.BeginInvoke(new Action(() => this.progressBar1.PerformStep()));
            else this.progressBar1.PerformStep();
        }
    }
}
