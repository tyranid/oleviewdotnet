using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet
{
    public partial class CreateIStreamForm : Form
    {      
        public CreateIStreamForm()
        {
            InitializeComponent();
        }

        public IStreamImpl Stream { get; private set; }

        private void btnCreateRead_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files (*.*)|*.*";
            dlg.ShowReadOnly = false;
            this.DialogResult = dlg.ShowDialog();
            if (this.DialogResult == DialogResult.OK)
            {
                try
                {
                    Stream = new IStreamImpl(dlg.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
            Close();
        }

        private void btnCreateWrite_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files (*.*)|*.*";

            this.DialogResult = dlg.ShowDialog();

            if (this.DialogResult == DialogResult.OK)
            {
                try
                {
                    Stream = new IStreamImpl(dlg.FileName, System.IO.FileMode.CreateNew, System.IO.FileAccess.ReadWrite, System.IO.FileShare.Read);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            Close();
        }
    }
}
