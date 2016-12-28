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
using System.Linq;
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

        private static ApplicationContext _appContext;

        static public MainForm GetMainForm(COMRegistry registry)
        {
            foreach (MainForm form in Application.OpenForms.OfType<MainForm>())
            {
                if (form.Registry == registry)
                {
                    return form;
                }
            }

            // Fall back to the main form.
            return (MainForm)_appContext.MainForm;
        }

        static int EnumInterfaces(List<string> args)
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

                    CLSCTX clsctx;
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

                    COMEnumerateInterfaces intf = new COMEnumerateInterfaces(clsid, clsctx, sta, timeout);
                    if (intf.Exception != null)
                    {
                        writer.WriteLine("ERROR:{0:X08}", intf.Exception.NativeErrorCode);
                        return 1;
                    }
                    else
                    {
                        foreach (COMInterfaceInstance entry in intf.Interfaces)
                        {
                            writer.WriteLine("{0}", entry);
                        }
                        foreach (COMInterfaceInstance entry in intf.FactoryInterfaces)
                        {
                            writer.WriteLine("*{0}", entry);
                        }
                        return 0;
                    }
                }
            }
        }

        class MultiApplicationContext : ApplicationContext
        {
            public MultiApplicationContext(Form main_form) : base(main_form)
            {
            }

            protected override void OnMainFormClosed(object sender, EventArgs e)
            {
                IEnumerable<MainForm> forms = Application.OpenForms.OfType<MainForm>();
                if (forms.Count() == 0)
                {
                    base.OnMainFormClosed(sender, e);
                }
                else
                {
                    MainForm = forms.First();
                }
            }
        }

        static IEnumerable<COMServerType> ParseServerTypes(string servers)
        {
            string[] ss = servers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return ss.Select(s => (COMServerType)Enum.Parse(typeof(COMServerType), s, true)).ToArray();
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
            bool show_help = false;
            bool query_interfaces = false;
            int concurrent_queries = Environment.ProcessorCount;
            COMRegistryMode mode = COMRegistryMode.Merged;
            IEnumerable<COMServerType> server_types = new COMServerType[] { COMServerType.InProcHandler32, COMServerType.InProcServer32, COMServerType.LocalServer32 };

            OptionSet opts = new OptionSet() {
                { "i|in=",  "Open a database file.", v => database_file = v },
                { "o|out=", "Save database and exit.", v => save_file = v },
                { "e|enum",  "Enumerate the provided CLSID (GUID).", v => enum_clsid = v != null },
                { "q|query", "Query all interfaces for database", v => query_interfaces = v != null },
                { "c|conn=", "Number of concurrent interface queries", v => concurrent_queries = int.Parse(v) },
                { "s|server=", "Specify server types for query", v => server_types = ParseServerTypes(v) },
                { "m",  "Loading mode is machine only.", v => mode = COMRegistryMode.MachineOnly },
                { "u",  "Loading mode is user only.", v => mode = COMRegistryMode.UserOnly },
                { "h|help",  "Show this message and exit.", v => show_help = v != null },
            };

            List<string> additional_args = new List<string>();

            try
            {
                additional_args = opts.Parse(args);
            }
            catch
            {
                show_help = true;
            }

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
                    Environment.Exit(EnumInterfaces(additional_args));
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

                try
                {
                    COMRegistry registry = database_file != null ? COMUtilities.LoadRegistry(null, database_file)
                        : COMUtilities.LoadRegistry(null, mode);

                    if (query_interfaces)
                    {
                        if (!COMUtilities.QueryAllInterfaces(null, registry, server_types, concurrent_queries))
                        {
                            Environment.Exit(1);
                        }
                    }

                    if (save_file != null)
                    {
                        registry.Save(save_file);
                        Environment.Exit(0);
                    }

                    _appContext = new MultiApplicationContext(new MainForm(registry));
                    Application.Run(_appContext);
                }
                catch (Exception ex)
                {
                    if (!(ex is OperationCanceledException))
                    {
                        ShowError(null, ex);
                    }
                }
            }
        }

        public static void ShowError(IWin32Window window, Exception ex)
        {
            MessageBox.Show(window, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
