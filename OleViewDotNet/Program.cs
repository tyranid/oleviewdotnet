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

using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
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

        static int EnumInterfaces(bool enum_factory, List<string> args)
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

                    int timeout = 10000;
                    if (args.Count > 4)
                    {
                        if (!int.TryParse(args[4], out timeout) || timeout < 0)
                        {
                            return 1;
                        }
                    }

                    COMEnumerateInterfaces intf = new COMEnumerateInterfaces(clsid, clsctx, sta, timeout, enum_factory);
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
            string database_file = null;
            string save_file = null;
            bool enum_clsid = false;
            bool enum_factory = false;
            bool show_help = false;
            bool user_only = false;

            OptionSet opts = new OptionSet() {
                { "i|in=",  "Open a database file.", v => database_file = v },
                { "o|out=", "Save database and exit.", v => save_file = v },
                { "e|enum",  "Enumerate the provided CLSID (GUID).", v => enum_clsid = v != null },
                { "f|fact",  "Enumerate the provided CLSID factory.", v => enum_clsid = enum_factory = v != null },
                { "u|user",  "Use only current user registrations.", v => user_only = v != null },
                { "h|help",  "Show this message and exit.", v => show_help = v != null },
            };

            List<string> additional_args = opts.Parse(args);

            if (show_help || (enum_clsid && additional_args.Count < 4))
            {
                StringWriter writer = new StringWriter();
                writer.WriteLine("Usage: OleViewDotNet [options] [enum args]");
                writer.WriteLine();
                writer.WriteLine("Options:");
                opts.WriteOptionDescriptions(writer);
                MessageBox.Show(writer.ToString(), "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(1);
            }

            if (enum_clsid)
            {
                try
                {
                    Environment.Exit(EnumInterfaces(enum_factory, additional_args));
                }
                catch
                {
                    Environment.Exit(42);
                }
            }
            else
            {
                Exception error = null;
                AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using (LoadingDialog loader = new LoadingDialog(user_only, database_file))
                {
                    if (loader.ShowDialog() == DialogResult.OK)
                    {
                        if (save_file != null)
                        {
                            try
                            {
                                COMRegistry.Save(save_file);
                            }
                            catch (Exception ex)
                            {
                                error = ex;
                            }
                        }
                    }
                    else
                    {
                        error = loader.Error;
                    }                    
                }

                if (error == null)
                {
                    using (_mainForm = new MainForm())
                    {
                        Application.Run(_mainForm);
                    }
                }
                else
                {
                    MessageBox.Show(error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
