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
using NtApiDotNet.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet
{
    public class COMInterfaceInstance : IXmlSerializable
    {
        public Guid Iid { get; private set; }
        public string Module { get; private set; }
        public long VTableOffset { get; private set; }

        public COMInterfaceInstance(Guid iid, string module, long vtable_offset)
        {
            Iid = iid;
            Module = module;
            VTableOffset = vtable_offset;
        }

        public COMInterfaceInstance(Guid iid) : this(iid, null, 0)
        {
        }

        public COMInterfaceInstance()
        {
        }

        public override string ToString()
        {
            if (!String.IsNullOrWhiteSpace(Module))
            {
                return String.Format("{0},{1},{2}", Iid, Module, VTableOffset);
            }
            return String.Format("{0}", Iid);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Iid = reader.ReadGuid("iid");
            Module = reader.GetAttribute("mod");
            VTableOffset = reader.ReadLong("ofs");
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteGuid("iid", Iid);
            writer.WriteOptionalAttributeString("mod", Module);
            writer.WriteLong("ofs", VTableOffset);
        }
    }

    public class COMEnumerateInterfaces
    {
        private HashSet<COMInterfaceInstance> _interfaces;
        private HashSet<COMInterfaceInstance> _factory_interfaces;
        private Guid _clsid;
        private CLSCTX _clsctx;
        private string _activatable_classid;
        private Win32Exception _ex;
        private bool _winrt_component;

        const int GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS = 0x00000004;

        private void QueryInterface(IntPtr punk, Guid iid, Dictionary<IntPtr, string> module_names, HashSet<COMInterfaceInstance> list)
        {
            if (punk != IntPtr.Zero)
            {
                IntPtr ppout;
                if (Marshal.QueryInterface(punk, ref iid, out ppout) == 0)
                {
                    IntPtr vtable = Marshal.ReadIntPtr(ppout, 0);
                    COMInterfaceInstance intf;

                    using (SafeLoadLibraryHandle module = SafeLoadLibraryHandle.GetModuleHandle(vtable))
                    {
                        if (_clsctx == CLSCTX.INPROC_SERVER && module != null)
                        {
                            if (!module_names.ContainsKey(module.DangerousGetHandle()))
                            {
                                module_names[module.DangerousGetHandle()] = module.Name;
                            }
                            intf = new COMInterfaceInstance(iid,
                                module_names[module.DangerousGetHandle()],
                                vtable.ToInt64() - module.DangerousGetHandle().ToInt64());
                        }
                        else
                        {
                            intf = new COMInterfaceInstance(iid);
                        }
                    }


                    list.Add(intf);
                    Marshal.Release(ppout);
                }
            }
        }

        private void QueryInspectableInterfaces(IntPtr punk, Dictionary<IntPtr, string> module_names, HashSet<COMInterfaceInstance> list)
        {
            if (punk == IntPtr.Zero)
            {
                return;
            }
            object obj = Marshal.GetObjectForIUnknown(punk);
            IInspectable inspectable = obj as IInspectable;
            if (inspectable != null)
            {
                IntPtr iids = IntPtr.Zero;
                try
                {
                    int iid_count;
                    inspectable.GetIids(out iid_count, out iids);
                    for (int i = 0; i < iid_count; ++i)
                    {
                        byte[] buffer = new byte[16];
                        Marshal.Copy(iids + i * 16, buffer, 0, buffer.Length);
                        QueryInterface(punk, new Guid(buffer), module_names, list);
                    }
                }
                catch
                {
                }
                finally
                {
                    if (iids != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(iids);
                    }
                }
            }
        }

        private void GetInterfacesInternal()
        {
            IntPtr punk = IntPtr.Zero;
            IntPtr pfactory = IntPtr.Zero;
            Guid IID_IUnknown = COMInterfaceEntry.IID_IUnknown;

            int hr = 0;
            if (_winrt_component)
            {
                hr = COMUtilities.RoGetActivationFactory(_activatable_classid, ref IID_IUnknown, out pfactory);
            }
            else
            {
                hr = COMUtilities.CoGetClassObject(ref _clsid, _clsctx, null, ref IID_IUnknown, out pfactory);
            }
            // If we can't get class object, no chance we'll get object.
            if (hr != 0)
            {
                throw new Win32Exception(hr);
            }

            if (_winrt_component)
            {
                hr = COMUtilities.RoActivateInstance(_activatable_classid, out punk);
            }
            else
            {
                hr = COMUtilities.CoCreateInstance(ref _clsid, IntPtr.Zero, _clsctx,
                                               ref IID_IUnknown, out punk);
            }
            if (hr != 0)
            {
                punk = IntPtr.Zero;
            }

            try
            {
                Dictionary<IntPtr, string> module_names = new Dictionary<IntPtr, string>();
                QueryInterface(punk, COMInterfaceEntry.IID_IMarshal, module_names, _interfaces);
                QueryInterface(pfactory, COMInterfaceEntry.IID_IMarshal, module_names, _factory_interfaces);
                QueryInterface(punk, COMInterfaceEntry.IID_IPSFactoryBuffer, module_names, _interfaces);
                QueryInterface(pfactory, COMInterfaceEntry.IID_IPSFactoryBuffer, module_names, _factory_interfaces);

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

                QueryInspectableInterfaces(punk, module_names, _interfaces);
                QueryInspectableInterfaces(pfactory, module_names, _factory_interfaces);
            }
            finally
            {
                if (pfactory != IntPtr.Zero)
                {
                    Marshal.Release(pfactory);
                }

                if (punk != IntPtr.Zero)
                {
                    Marshal.Release(punk);
                }
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

        public COMEnumerateInterfaces(Guid clsid, CLSCTX clsctx, string activatable_classid, bool sta, int timeout)
            : this(clsid, clsctx, activatable_classid, new List<COMInterfaceInstance>(), new List<COMInterfaceInstance>())
        {
            GetInterfaces(sta, timeout);
        }

        private class InterfaceInstanceComparer : IEqualityComparer<COMInterfaceInstance>
        {
            public bool Equals(COMInterfaceInstance x, COMInterfaceInstance y)
            {
                return x.Iid == y.Iid;
            }

            public int GetHashCode(COMInterfaceInstance obj)
            {
                return obj.Iid.GetHashCode();
            }
        }

        private COMEnumerateInterfaces(Guid clsid, CLSCTX clsctx, string activatable_classid, List<COMInterfaceInstance> interfaces, List<COMInterfaceInstance> factory_interfaces)
        {
            _interfaces = new HashSet<COMInterfaceInstance>(interfaces, new InterfaceInstanceComparer());
            _factory_interfaces = new HashSet<COMInterfaceInstance>(factory_interfaces, new InterfaceInstanceComparer());
            _clsid = clsid;
            _clsctx = clsctx;
            _activatable_classid = activatable_classid;
            _winrt_component = !string.IsNullOrWhiteSpace(_activatable_classid);
        }

        public async static Task<COMEnumerateInterfaces> GetInterfacesOOP(COMRuntimeClassEntry ent)
        {
            string apartment = "s";
            if (ent.Threading == ThreadingType.Both
                || ent.Threading == ThreadingType.Mta)
            {
                apartment = "m";
            }
            string command_line = string.Format("{0} {1} {2}", ent.Name, apartment, 
                ent.ActivationType == ActivationType.InProcess ? CLSCTX.INPROC_SERVER : CLSCTX.LOCAL_SERVER);
            var interfaces = await GetInterfacesOOP(command_line, true);
            return new COMEnumerateInterfaces(Guid.Empty, 0, ent.Name, interfaces.Interfaces, interfaces.FactoryInterfaces);
        }

        private class InterfaceLists
        {
            public List<COMInterfaceInstance> Interfaces { get; set; }
            public List<COMInterfaceInstance> FactoryInterfaces { get; set; }
        }

        private async static Task<InterfaceLists> GetInterfacesOOP(string command_line, bool runtime_class)
        {
            using (AnonymousPipeServerStream server = new AnonymousPipeServerStream(PipeDirection.In,
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

                Process proc = new Process();
                ProcessStartInfo info = new ProcessStartInfo(process, string.Format("{0} {1} {2}",
                    runtime_class ? "-r" : "-e", server.GetClientHandleAsString(), command_line));
                info.UseShellExecute = false;
                info.CreateNoWindow = true;
                proc.StartInfo = info;
                proc.Start();
                try
                {
                    List<COMInterfaceInstance> interfaces = new List<COMInterfaceInstance>();
                    List<COMInterfaceInstance> factory_interfaces = new List<COMInterfaceInstance>();
                    server.DisposeLocalCopyOfClientHandle();

                    using (StreamReader reader = new StreamReader(server))
                    {
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
                        return new InterfaceLists() { Interfaces = interfaces, FactoryInterfaces = factory_interfaces };
                    }
                }
                finally
                {
                    proc.Close();
                }
            }
        }

        public async static Task<COMEnumerateInterfaces> GetInterfacesOOP(COMCLSIDEntry ent)
        {
            string apartment = "s";
            if (ent.DefaultThreadingModel == COMThreadingModel.Both 
                || ent.DefaultThreadingModel == COMThreadingModel.Free)
            {
                apartment = "m";
            }

            string command_line = string.Format("{0} {1} \"{2}\"", ent.Clsid.ToString("B"), apartment, ent.CreateContext);
            var interfaces = await GetInterfacesOOP(command_line, false);
            return new COMEnumerateInterfaces(ent.Clsid, ent.CreateContext, string.Empty, interfaces.Interfaces, interfaces.FactoryInterfaces);
        }
    }
}
