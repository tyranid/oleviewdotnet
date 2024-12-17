using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace OleViewDotNet.Forms
{

    // This form is used by FindInterface.
    public partial class FindInterfaceForm : Form
    {
        public int inProcRetval = -1;
        public int localRetval = -1;
        public FindInterfaceForm()
        {
            InitializeComponent();
        }

        public void UpdateLabel1(String data)
        {
            if (this.label1.InvokeRequired) this.label1.BeginInvoke(new Action(() => this.label1.Text = data));
            else this.label1.Text = data;
        }
        public void UpdateLabel5(String data)
        {
            if (this.label5.InvokeRequired) this.label5.BeginInvoke(new Action(() => this.label5.Text = data));
            else this.label5.Text = data;
        }
        public void UpdateLabel6(String data)
        {
            if (this.label6.InvokeRequired) this.label6.BeginInvoke(new Action(() => this.label6.Text = data));
            else this.label6.Text = data;
        }

        public void RepeatUpdateLabel(object arg)
        {
            bool isInproc = (bool)arg;
            int i = 1;
            while (true)
            {
                Thread.Sleep(300);
                String label = "Running";
                for (int j = 1; j <= i; j++) label += ".";
                if (isInproc) UpdateLabel6(label);
                else UpdateLabel5(label);
                Application.DoEvents();
                i = i % 4 + 1;
            }
        }

    }


}
