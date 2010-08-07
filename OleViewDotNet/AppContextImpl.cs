using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace OleViewDotNet
{
    /// <summary>
    /// Implementation of the application context
    /// </summary>
    class AppContextImpl : ApplicationContext
    {
        private COMRegistry m_comRegistry;
        private int m_formCount;

        private void OnFormClosed(object sender, EventArgs e)
        {
            m_formCount--;
            if (m_formCount == 0)
            {
                try
                {
                    PowerShellInstance.Close();
                }
                catch (Exception)
                {
                }
                ExitThread();
            }
        }

        public void CreateNewMainForm()
        {
            ++m_formCount;
            MainForm frm = new MainForm(m_comRegistry);
            frm.FormClosed += OnFormClosed;
            frm.Show();
        }

        public COMRegistry GetCOMRegistry()
        {
            return m_comRegistry;
        }

        public AppContextImpl(COMRegistry comRegistry)
        {
            m_comRegistry = comRegistry;
            CreateNewMainForm();
        }
    }
}
