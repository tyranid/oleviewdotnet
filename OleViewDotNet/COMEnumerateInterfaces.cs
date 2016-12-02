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
using System.Threading;
using System.Threading.Tasks;

namespace OleViewDotNet
{
    public class COMEnumerateInterfaces
    {
        private List<Guid> _guids;
        private List<Guid> _factory_guids;
        private Guid _clsid;
        private CLSCTX _clsctx;
        private Win32Exception _ex;

        private static bool QueryInterface(IntPtr punk, Guid iid)
        {
            if (punk != IntPtr.Zero)
            {
                IntPtr ppout;
                if (Marshal.QueryInterface(punk, ref iid, out ppout) == 0)
                {
                    Marshal.Release(ppout);
                    return true;
                }
            }
            
            return false;
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

            hr = COMUtilities.CoCreateInstance(ref _clsid, IntPtr.Zero, CLSCTX.CLSCTX_SERVER,
                                               ref IID_IUnknown, out punk);
            if (hr != 0)
            {
                punk = IntPtr.Zero;
            }

            try
            {
                if (QueryInterface(punk, COMInterfaceEntry.IID_IMarshal))
                {
                    _guids.Add(COMInterfaceEntry.IID_IMarshal);
                }

                if (QueryInterface(pfactory, COMInterfaceEntry.IID_IMarshal))
                {
                    _factory_guids.Add(COMInterfaceEntry.IID_IMarshal);
                }

                using (RegistryKey interface_key = Registry.ClassesRoot.OpenSubKey("Interface"))
                {
                    foreach (string iid_string in interface_key.GetSubKeyNames())
                    {
                        Guid iid;
                        if (Guid.TryParse(iid_string, out iid))
                        {
                            if (QueryInterface(punk, iid))
                            {
                                _guids.Add(iid);
                            }
                            if (QueryInterface(pfactory, iid))
                            {
                                _factory_guids.Add(iid);
                            }
                        }
                    }
                }
            }
            finally
            {
                Marshal.Release(punk);
            }
        }

        [MTAThread]
        private void MTAEnumThread()
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

        [STAThread]
        private void STAEnumThread()
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

        private void GetInterfaces(bool sta, int timeout)
        {
            Thread th = null;
            if (sta)
            {
                th = new Thread(STAEnumThread);
            }
            else
            {
                th = new Thread(MTAEnumThread);
            }
            th.Start();
            if (!th.Join(timeout))
            {
                th.Abort();
            }
        }

        public IEnumerable<Guid> Guids { get { return _guids; } }
        public IEnumerable<Guid> FactoryGuids { get { return _factory_guids; } }
        public Win32Exception Exception { get { return _ex; } }

        public COMEnumerateInterfaces(Guid clsid, CLSCTX clsctx, bool sta, int timeout) 
            : this(clsid, clsctx, new List<Guid>(), new List<Guid>())
        {
            GetInterfaces(sta, timeout);
        }

        private COMEnumerateInterfaces(Guid clsid, CLSCTX clsctx, List<Guid> guids, List<Guid> factory_guids)
        {
            _guids = guids;
            _factory_guids = factory_guids;
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
                        List<Guid> guids = new List<Guid>();
                        List<Guid> factory_guids = new List<Guid>();
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

                                if (Guid.TryParse(line, out g))
                                {
                                    if (factory)
                                    {
                                        factory_guids.Add(g);
                                    }
                                    else
                                    {
                                        guids.Add(g);
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
                            guids = new List<Guid>(new Guid[] { COMInterfaceEntry.IID_IUnknown });
                            factory_guids = new List<Guid>(new Guid[] { COMInterfaceEntry.IID_IUnknown });
                        }

                        return new COMEnumerateInterfaces(ent.Clsid, ent.CreateContext, guids, factory_guids);
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
