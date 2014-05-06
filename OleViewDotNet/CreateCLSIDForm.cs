using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public partial class CreateCLSIDForm : Form
    {
        public Guid Clsid { get; private set; }
        public COMUtilities.CLSCTX ClsCtx { get; private set; }

        public CreateCLSIDForm()
        {
            InitializeComponent();            
            ClsCtx = COMUtilities.CLSCTX.CLSCTX_SERVER;
            comboBoxClsCtx.Items.Add(COMUtilities.CLSCTX.CLSCTX_SERVER);
            comboBoxClsCtx.Items.Add(COMUtilities.CLSCTX.CLSCTX_INPROC_SERVER);
            comboBoxClsCtx.Items.Add(COMUtilities.CLSCTX.CLSCTX_LOCAL_SERVER);
            comboBoxClsCtx.Items.Add(COMUtilities.CLSCTX.CLSCTX_ACTIVATE_32_BIT_SERVER | COMUtilities.CLSCTX.CLSCTX_LOCAL_SERVER);
            comboBoxClsCtx.Items.Add(COMUtilities.CLSCTX.CLSCTX_ACTIVATE_64_BIT_SERVER | COMUtilities.CLSCTX.CLSCTX_LOCAL_SERVER);
            comboBoxClsCtx.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Guid clsid;

            if ((Guid.TryParse(textBoxCLSID.Text.Trim(), out clsid) && (comboBoxClsCtx.SelectedItem != null)))
            {
                Clsid = clsid;
                ClsCtx = (COMUtilities.CLSCTX)comboBoxClsCtx.SelectedItem;

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(this, "Invalid CLSID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
