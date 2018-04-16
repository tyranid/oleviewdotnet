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

        static int EnumInterfaces(Queue<string> args, bool runtime_class)
        {
            using (AnonymousPipeClientStream client = new AnonymousPipeClientStream(PipeDirection.Out, args.Dequeue()))
            {
                using (StreamWriter writer = new StreamWriter(client))
                {
                    Guid clsid = Guid.Empty;
                    CLSCTX clsctx = 0;
                    bool sta = false;
                    string activatable_class = string.Empty;
                    if (runtime_class)
                    {
                        activatable_class = args.Dequeue();
                    }
                    else
                    {
                        if (!Guid.TryParse(args.Dequeue(), out clsid))
                        {
                            return 1;
                        }

                        sta = args.Dequeue() == "s";

                        if (!Enum.TryParse(args.Dequeue(), true, out clsctx))
                        {
                            return 1;
                        }
                    }

                    int timeout = 10000;
                    if (args.Count > 0)
                    {
                        if (!int.TryParse(args.Dequeue(), out timeout) || timeout < 0)
                        {
                            return 1;
                        }
                    }

                    COMEnumerateInterfaces intf = new COMEnumerateInterfaces(clsid, clsctx, activatable_class, sta, timeout);
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

        class ActivationFilter : IActivationFilter
        {
            public void HandleActivation(uint dwActivationType, ref Guid rclsid, out Guid pReplacementClsId)
            {
                pReplacementClsId = rclsid;
                System.Diagnostics.Trace.WriteLine(String.Format("{0:X} {1}", dwActivationType, rclsid));
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
            bool enum_runtime = false;
            bool show_help = false;
            bool query_interfaces = false;
            int concurrent_queries = Environment.ProcessorCount;
            bool refresh_interfaces = false;
            bool enable_activation_filter = false;
            COMRegistryMode mode = COMRegistryMode.Merged;
            IEnumerable<COMServerType> server_types = new COMServerType[] { COMServerType.InProcHandler32, COMServerType.InProcServer32, COMServerType.LocalServer32 };
            
            OptionSet opts = new OptionSet() {
                { "i|in=",  "Open a database file.", v => database_file = v },
                { "o|out=", "Save database and exit.", v => save_file = v },
                { "e|enum",  "Enumerate the provided CLSID (GUID).", v => enum_clsid = v != null },
                { "r|rt",  "Enumerate the provided Runtime Class.", v => enum_runtime = v != null },
                { "q|query", "Query all interfaces for database", v => query_interfaces = v != null },
                { "c|conn=", "Number of concurrent interface queries", v => concurrent_queries = int.Parse(v) },
                { "s|server=", "Specify server types for query", v => server_types = ParseServerTypes(v) },
                { "refresh", "Refresh interfaces in query", v => refresh_interfaces = v != null },
                { "m", "Loading mode is machine only.", v => mode = COMRegistryMode.MachineOnly },
                { "u", "Loading mode is user only.", v => mode = COMRegistryMode.UserOnly },
                { "a", "Enable activation filter.", v => enable_activation_filter = v != null },
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

            if (show_help || (enum_clsid && additional_args.Count < 4) || (enum_runtime && additional_args.Count < 2))
            {
                StringWriter writer = new StringWriter();
                writer.WriteLine("Usage: OleViewDotNet [options] [enum args]");
                writer.WriteLine();
                writer.WriteLine("Options:");
                opts.WriteOptionDescriptions(writer);
                MessageBox.Show(writer.ToString(), "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(1);
            }

            if (enum_clsid || enum_runtime)
            {
                try
                {
                    Environment.Exit(EnumInterfaces(new Queue<string>(additional_args), enum_runtime));
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
                        if (!COMUtilities.QueryAllInterfaces(null, registry.Clsids.Values, server_types, concurrent_queries, refresh_interfaces))
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
                    if (enable_activation_filter)
                    {
                        COMUtilities.CoRegisterActivationFilter(new ActivationFilter());
                    }
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
            ShowError(window, ex, false);
        }

        public static void ShowError(IWin32Window window, Exception ex, bool stack_trace)
        {
            MessageBox.Show(window, stack_trace ? ex.ToString() : ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
