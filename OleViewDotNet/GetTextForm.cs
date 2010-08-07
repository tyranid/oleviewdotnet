using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class GetTextForm : Form
    {
        public string Data
        {
            get;
            private set;
        }

        public GetTextForm(string strInitial)
        {
            Data = strInitial;
            InitializeComponent();
        }

        private void GetTextForm_Load(object sender, EventArgs e)
        {
            textBox.Text = Data;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Data = textBox.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
