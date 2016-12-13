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
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Windows.Forms;
using System.Reflection;

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

        internal static COMRegistry LoadRegistry(IWin32Window window, 
            Func<IProgress<string>, object> worker)
        {
            using (WaitingDialog loader = new WaitingDialog(worker))
            {
                if (loader.ShowDialog(window) == DialogResult.OK)
                {
                    return loader.Result as COMRegistry;
                }
                else
                {
                    throw loader.Error;
                }
            }
        }

        internal static COMRegistry LoadRegistry(IWin32Window window, COMRegistryMode mode)
        {
            if (mode == COMRegistryMode.Diff)
            {
                throw new ArgumentException("Can't load a diff registry");
            }
            return LoadRegistry(window, progress => COMRegistry.Load(mode, null, progress));
        }

        internal static COMRegistry LoadRegistry(IWin32Window window, string database_file)
        {
            return LoadRegistry(window, progress => COMRegistry.Load(database_file, progress));
        }

        internal static COMRegistry DiffRegistry(IWin32Window window, COMRegistry left, COMRegistry right, COMRegistryDiffMode mode)
        {
            return LoadRegistry(window, progress => COMRegistry.Diff(left, right, mode, progress));
        }

        internal static Assembly LoadTypeLib(IWin32Window window, string path)
        {
            using (WaitingDialog dlg = new WaitingDialog(p => COMUtilities.LoadTypeLib(path, p), s => s))
            {
                dlg.Text = String.Format("Loading TypeLib {0}", path);
                dlg.CancelEnabled = false;
                if (dlg.ShowDialog(window) == DialogResult.OK)
                {
                    return (Assembly)dlg.Result;
                }
                else if ((dlg.Error != null) && !(dlg.Error is OperationCanceledException))
                {
                    Program.ShowError(window, dlg.Error);
                }
                return null;
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
            bool show_help = false;
            COMRegistryMode mode = COMRegistryMode.Merged;
            
            OptionSet opts = new OptionSet() {
                { "i|in=",  "Open a database file.", v => database_file = v },
                { "o|out=", "Save database and exit.", v => save_file = v },
                { "e|enum",  "Enumerate the provided CLSID (GUID).", v => enum_clsid = v != null },
                { "m",  "Loading mode is machine only.", v => mode = COMRegistryMode.MachineOnly },
                { "u",  "Loading mode is user only.", v => mode = COMRegistryMode.UserOnly },
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
                    _appContext = new MultiApplicationContext(new MainForm(
                        database_file != null ? LoadRegistry(null, database_file) : LoadRegistry(null, mode)));
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
