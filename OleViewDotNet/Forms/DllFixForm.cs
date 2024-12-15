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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OleViewDotNet.Forms
{

    // This form will get DLL/EXE filename and fix DLL/EXE used by ResolveMethod.

    public partial class DllFixForm : Form
    {
        public DllFixForm()
        {
            InitializeComponent();
            if (!Directory.Exists("DLLs")) Directory.CreateDirectory("DLLs");
            String[] fileNames = Directory.GetFiles("DLLs");
            List<String> fileList = new List<String>();
            foreach (String fileName in fileNames)
            {
                if (fileName.EndsWith(".dll") || fileName.EndsWith(".exe"))
                {
                    fileList.Add(fileName);
                }
            }
            fileList.Sort();
            this.comboBox1.DataSource = fileList;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "" && textBox1.Text != "")
            {
                MessageBox.Show("Please Select Only One.");
            }
            else if (comboBox1.Text != "")
            {
                if (!File.Exists(comboBox1.Text))
                {
                    MessageBox.Show("File Doesn't Exist!");
                }
                else
                {
                    ProgramSettings.FixedDll = comboBox1.Text;
                    this.Close();
                }
            }
            else if (textBox1.Text != "")
            {
                if (!File.Exists(textBox1.Text))
                {
                    MessageBox.Show("File Doesn't Exist!");
                }
                else
                {
                    ProgramSettings.FixedDll = textBox1.Text;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Please Input DLL/EXE Path.");
            }
        }
    }
}
