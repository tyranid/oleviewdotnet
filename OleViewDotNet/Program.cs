//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
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
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Forms;

namespace OleViewDotNet
{
    public static class Program
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
            Environment.Exit(1);
        }

        private static MainForm _mainForm;

        static public MainForm GetMainForm()
        {
            return _mainForm;
        }

        static int EnumInterfaces(string[] args)
        {
            using (AnonymousPipeClientStream client = new AnonymousPipeClientStream(PipeDirection.Out, args[0]))
            {
                using (StreamWriter writer = new StreamWriter(client))
                {
                    Guid clsid;
                    if (!Guid.TryParse(args[1], out clsid))
                    {
                        return 1;
                    }

                    bool sta = args[2] == "s";

                    COMUtilities.CLSCTX clsctx;
                    if (!Enum.TryParse(args[3], true, out clsctx))
                    {
                        return 1;
                    }

                    COMEnumerateInterfaces intf = new COMEnumerateInterfaces(clsid, clsctx, sta);
                    if (intf.Exception != null)
                    {
                        writer.WriteLine("ERROR:{0:X08}", intf.Exception.NativeErrorCode);
                        return 1;
                    }
                    else
                    {
                        foreach (Guid guid in intf.Guids)
                        {
                            writer.WriteLine("{0}", guid);
                        }
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length > 3)
            {
                try
                {
                    Environment.Exit(EnumInterfaces(args));
                }
                catch
                {
                    Environment.Exit(42);
                }
            }
            else
            {
                AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using (LoadingDialog loader = new LoadingDialog(Microsoft.Win32.Registry.ClassesRoot))
                {
                    if (loader.ShowDialog() == DialogResult.OK)
                    {
                        using (_mainForm = new MainForm())
                        {
                            Application.Run(_mainForm);
                        }
                    }
                }
            }
        }
    }
}
