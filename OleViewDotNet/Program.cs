//    This file is part of OleViewDotNet.
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

            using (LoadingDialog loader = new LoadingDialog(Microsoft.Win32.Registry.ClassesRoot))
            {
                if (loader.ShowDialog() == DialogResult.OK)
                {
                    m_appContext = new AppContextImpl(loader.LoadedReg);
                    Application.Run(m_appContext);
                }
            }
        }
    }
}
