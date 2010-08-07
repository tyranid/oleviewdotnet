using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OleViewDotNet
{
    static class Program
    {
        /// <summary>
        /// Unhandled exception event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled Exception: " + e.ExceptionObject.ToString(),
                "Unhandled Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        static AppContextImpl m_appContext;        

        static public void CreateNewMainForm()
        {
            if (m_appContext != null)
            {
                m_appContext.CreateNewMainForm();
            }
        }

        static public COMRegistry GetCOMRegistry()
        {
            if (m_appContext != null)
            {
                return m_appContext.GetCOMRegistry();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            LoadingDialog loader = new LoadingDialog(Microsoft.Win32.Registry.ClassesRoot);
            if (loader.ShowDialog() == DialogResult.OK)
            {                
                m_appContext = new AppContextImpl(loader.LoadedReg);
                Application.Run(m_appContext);
            }
        }
    }
}
