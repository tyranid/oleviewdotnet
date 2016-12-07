//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OleViewDotNet
{
    [Serializable]
    public class COMInterfaceInstance
    {
        public Guid Iid { get; private set; }
        public string ModulePath { get; private set; }
        public long VTableOffset { get; private set; }

        public COMInterfaceInstance(Guid iid, string module_path, long vtable_offset)
        {
            Iid = iid;
            ModulePath = module_path;
            VTableOffset = vtable_offset;
        }

        public COMInterfaceInstance(Guid iid) : this(iid, null, 0)
        {
        }

        public override string ToString()
        {
            if (!String.IsNullOrWhiteSpace(ModulePath))
            {
                return String.Format("{0},{1},{2}", Iid, ModulePath, VTableOffset);
            }
            return String.Format("{0}", Iid);
        }
    }

    public class COMEnumerateInterfaces
    {
        private List<COMInterfaceInstance> _interfaces;
        private List<COMInterfaceInstance> _factory_interfaces;
        private Guid _clsid;
        private CLSCTX _clsctx;
        private Win32Exception _ex;

        const int GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS = 0x00000004;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool GetModuleHandleEx(int dwFlags, IntPtr lpModuleName, out IntPtr phModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

        static string GetModuleFileName(IntPtr hModule)
        {
            StringBuilder builder = new StringBuilder(260);
            int result = GetModuleFileName(hModule, builder, builder.Capacity);
            if (result > 0)
            {
                string path = builder.ToString();
                int index = path.LastIndexOf('\\');
                if (index < 0)
                    index = path.LastIndexOf('/');
                if (index < 0)
                {
                    return path;
                }
                return path.Substring(index + 1);
            }
            return "Unknown";
        }
        
        private void QueryInterface(IntPtr punk, Guid iid, Dictionary<IntPtr, string> module_names, List<COMInterfaceInstance> list)
        {
            if (punk != IntPtr.Zero)
            {
                IntPtr ppout;
                if (Marshal.QueryInterface(punk, ref iid, out ppout) == 0)
                {
                    IntPtr vtable = Marshal.ReadIntPtr(ppout, 0);
                    IntPtr module;
                    COMInterfaceInstance intf;

                    if (_clsctx == CLSCTX.CLSCTX_INPROC_SERVER && GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS, vtable, out module))
                    {
                        if (!module_names.ContainsKey(module))
                        {
                            module_names[module] = GetModuleFileName(module);
                        }
                        intf = new COMInterfaceInstance(iid, module_names[module], vtable.ToInt64() - module.ToInt64());
                    }
                    else
                    {
                        intf = new COMInterfaceInstance(iid);
                    }


                    list.Add(intf);
                    Marshal.Release(ppout);
                }
            }
        }

        private void GetInterfacesInternal()
        {
            IntPtr punk = IntPtr.Zero;
            IntPtr pfactory = IntPtr.Zero;           
            Guid IID_IUnknown = COMInterfaceEntry.IID_IUnknown;

            int hr = 0;
            hr = COMUtilities.CoGetClassObject(ref _clsid, _clsctx, IntPtr.Zero, ref IID_IUnknown, out pfactory);
            // If we can't get class object, no chance we'll get object.
            if (hr != 0)
            {
                throw new Win32Exception(hr);
            }

            hr = COMUtilities.CoCreateInstance(ref _clsid, IntPtr.Zero, _clsctx,
                                               ref IID_IUnknown, out punk);
            if (hr != 0)
            {
                punk = IntPtr.Zero;
            }

            try
            {
                Dictionary<IntPtr, string> module_names = new Dictionary<IntPtr, string>();
                QueryInterface(punk, COMInterfaceEntry.IID_IMarshal, module_names, _interfaces);
                QueryInterface(pfactory, COMInterfaceEntry.IID_IMarshal, module_names, _factory_interfaces);

                using (RegistryKey interface_key = Registry.ClassesRoot.OpenSubKey("Interface"))
                {
                    foreach (string iid_string in interface_key.GetSubKeyNames())
                    {
                        Guid iid;
                        if (Guid.TryParse(iid_string, out iid))
                        {
                            QueryInterface(punk, iid, module_names, _interfaces);
                            QueryInterface(pfactory, iid, module_names, _factory_interfaces);
                        }
                    }
                }
            }
            finally
            {
                Marshal.Release(punk);
            }
        }

        private void RunGetInterfaces()
        {
            try
            {
                GetInterfacesInternal();
            }
            catch (Win32Exception ex)
            {
                _ex = ex;
            }
        }

        private void ExitProcessThread(object timeout)
        {
            Thread.Sleep((int)timeout);
            Environment.Exit(1);
        }

        private void GetInterfaces(bool sta, int timeout)
        {
            Thread timeout_thread = new Thread(ExitProcessThread);
            timeout_thread.Start(timeout);
            if (sta)
            {
                RunGetInterfaces();
            }
            else
            {
                Thread th = null;
                th = new Thread(RunGetInterfaces);
                th.SetApartmentState(ApartmentState.MTA);
                th.Start();
                th.Join();
            }
        }

        public IEnumerable<COMInterfaceInstance> Interfaces { get { return _interfaces; } }
        public IEnumerable<COMInterfaceInstance> FactoryInterfaces { get { return _factory_interfaces; } }
        public Win32Exception Exception { get { return _ex; } }

        public COMEnumerateInterfaces(Guid clsid, CLSCTX clsctx, bool sta, int timeout) 
            : this(clsid, clsctx, new List<COMInterfaceInstance>(), new List<COMInterfaceInstance>())
        {
            GetInterfaces(sta, timeout);
        }

        private COMEnumerateInterfaces(Guid clsid, CLSCTX clsctx, List<COMInterfaceInstance> interfaces, List<COMInterfaceInstance> factory_interfaces)
        {
            _interfaces = interfaces;
            _factory_interfaces = factory_interfaces;
            _clsid = clsid;
            _clsctx = clsctx;
        }

        public async static Task<COMEnumerateInterfaces> GetInterfacesOOP(COMCLSIDEntry ent)
        {
            using (AnonymousPipeServerStream server = new AnonymousPipeServerStream(System.IO.Pipes.PipeDirection.In,
                HandleInheritability.Inheritable, 16 * 1024, null))
            {
                string process = null;
                if (!Environment.Is64BitOperatingSystem || Environment.Is64BitProcess)
                {
                    process = COMUtilities.GetExePath();
                }
                else
                {
                    process = COMUtilities.Get32bitExePath();
                }

                string apartment = "s";
                if (ent.ThreadingModel == COMThreadingModel.Both 
                    || ent.ThreadingModel == COMThreadingModel.Free)
                {
                    apartment = "m";
                }

                Process proc = new Process();
                ProcessStartInfo info = new ProcessStartInfo(process, String.Format("{0} {1} {2} {3} \"{4}\"",
                    "-e", server.GetClientHandleAsString(), ent.Clsid.ToString("B"), apartment, ent.CreateContext));
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                proc.StartInfo = info;
                proc.Start();
                try
                {
                    server.DisposeLocalCopyOfClientHandle();

                    using (StreamReader reader = new StreamReader(server))
                    {
                        List<COMInterfaceInstance> interfaces = new List<COMInterfaceInstance>();
                        List<COMInterfaceInstance> factory_interfaces = new List<COMInterfaceInstance>();
                        while (true)
                        {
                            string line = await reader.ReadLineAsync();
                            if (line == null)
                            {
                                break;
                            }

                            if (line.StartsWith("ERROR:"))
                            {
                                uint errorCode;
                                try
                                {
                                    errorCode = uint.Parse(line.Substring(6), System.Globalization.NumberStyles.AllowHexSpecifier);
                                }
                                catch (FormatException ex)
                                {
                                    Debug.WriteLine(ex.ToString());
                                    errorCode = 0x80004005;
                                }

                                throw new Win32Exception((int)errorCode);
                            }
                            else
                            {
                                Guid g;
                                bool factory = false;

                                if (line.StartsWith("*"))
                                {
                                    factory = true;
                                    line = line.Substring(1);
                                }

                                string[] parts = line.Split(new char[] { ',' }, 3);
                                if (Guid.TryParse(parts[0], out g))
                                {
                                    string module_path = null;
                                    long vtable_offset = 0;
                                    if (parts.Length == 3)
                                    {
                                        module_path = parts[1];
                                        long.TryParse(parts[2], out vtable_offset);
                                    }

                                    if (factory)
                                    {
                                        factory_interfaces.Add(new COMInterfaceInstance(g, module_path, vtable_offset));
                                    }
                                    else
                                    {
                                        interfaces.Add(new COMInterfaceInstance(g, module_path, vtable_offset));
                                    }
                                }
                            }
                        }

                        if (!proc.WaitForExit(5000))
                        {
                            proc.Kill();
                        }
                        int exitCode = proc.ExitCode;
                        if (exitCode != 0)
                        {
                            interfaces = new List<COMInterfaceInstance>(new COMInterfaceInstance[] { new COMInterfaceInstance(COMInterfaceEntry.IID_IUnknown) });
                            factory_interfaces = new List<COMInterfaceInstance>(new COMInterfaceInstance[] { new COMInterfaceInstance(COMInterfaceEntry.IID_IUnknown) });
                        }

                        return new COMEnumerateInterfaces(ent.Clsid, ent.CreateContext, interfaces, factory_interfaces);
                    }
                }
                finally
                {
                    proc.Close();
                }
            }
        }
    }
}
