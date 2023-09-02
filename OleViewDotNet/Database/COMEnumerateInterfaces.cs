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
using NtApiDotNet;
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

namespace OleViewDotNet.Database
{
    public class COMInterfaceInstance : IXmlSerializable
    {
        private COMRegistry m_registry;

        public Guid Iid { get; private set; }
        public string Module { get; private set; }
        public long VTableOffset { get; private set; }
        internal COMRegistry Database { get { return m_registry; } }
        public string Name
        {
            get
            {
                if (m_registry == null || !m_registry.InterfacesToNames.ContainsKey(Iid))
                {
                    return string.Empty;
                }
                return m_registry.InterfacesToNames[Iid];
            }
        }
        public COMInterfaceEntry InterfaceEntry
        {
            get
            {
                return m_registry?.Interfaces.GetGuidEntry(Iid);
            }
        }

        public COMInterfaceInstance(Guid iid, string module, long vtable_offset, COMRegistry registry) : this(registry)
        {
            Iid = iid;
            Module = module;
            VTableOffset = vtable_offset;
        }

        public COMInterfaceInstance(Guid iid, COMRegistry registry) : this(iid, null, 0, registry)
        {
        }

        public COMInterfaceInstance(COMRegistry registry)
        {
            m_registry = registry;
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Module))
            {
                return string.Format("{0},{1},{2}", Iid, Module, VTableOffset);
            }
            return Iid.ToString();
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
        private readonly HashSet<COMInterfaceInstance> _interfaces;
        private readonly HashSet<COMInterfaceInstance> _factory_interfaces;
        private Guid _clsid;
        private readonly CLSCTX _clsctx;
        private readonly string _activatable_classid;
        private Win32Exception _ex;
        private readonly bool _winrt_component;

        private void QueryInterface(IntPtr punk, Guid iid, Dictionary<IntPtr, string> module_names, HashSet<COMInterfaceInstance> list)
        {
            if (punk != IntPtr.Zero)
            {
                if (Marshal.QueryInterface(punk, ref iid, out IntPtr ppout) == 0)
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
                                vtable.ToInt64() - module.DangerousGetHandle().ToInt64(), null);
                        }
                        else
                        {
                            intf = new COMInterfaceInstance(iid, null);
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
            if (obj is IInspectable inspectable)
            {
                IntPtr iids = IntPtr.Zero;
                try
                {
                    inspectable.GetIids(out int iid_count, out iids);
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

        private void GetInterfacesInternal(NtToken token)
        {
            IntPtr punk = IntPtr.Zero;
            IntPtr pfactory = IntPtr.Zero;
            Guid IID_IUnknown = COMInterfaceEntry.IID_IUnknown;
            CLSCTX clsctx = _clsctx;
            if (token != null)
            {
                clsctx |= CLSCTX.ENABLE_CLOAKING;
            }

            using (var imp = token?.Impersonate())
            {
                int hr = 0;
                if (_winrt_component)
                {
                    hr = COMUtilities.RoGetActivationFactory(_activatable_classid, ref IID_IUnknown, out pfactory);
                }
                else
                {
                    hr = COMUtilities.CoGetClassObject(ref _clsid, clsctx, null, ref IID_IUnknown, out pfactory);
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
                    hr = COMUtilities.CoCreateInstance(ref _clsid, IntPtr.Zero, clsctx,
                                                       ref IID_IUnknown, out punk);
                }
                if (hr != 0)
                {
                    punk = IntPtr.Zero;
                }

                try
                {
                    Guid[] additional_iids = new[] {
                        COMInterfaceEntry.IID_IMarshal,
                        COMInterfaceEntry.IID_IPSFactoryBuffer,
                        COMInterfaceEntry.IID_IStdMarshalInfo,
                        COMInterfaceEntry.IID_IMarshal2
                        };

                    Dictionary<IntPtr, string> module_names = new Dictionary<IntPtr, string>();
                    foreach (var iid in additional_iids)
                    {
                        QueryInterface(punk, iid, module_names, _interfaces);
                        QueryInterface(pfactory, iid, module_names, _factory_interfaces);
                    }

                    var actctx = ActivationContext.FromProcess();
                    if (actctx != null)
                    {
                        foreach (var intf in actctx.ComInterfaces)
                        {
                            QueryInterface(punk, intf.Iid, module_names, _interfaces);
                            QueryInterface(pfactory, intf.Iid, module_names, _factory_interfaces);
                        }
                    }

                    using (RegistryKey interface_key = Registry.ClassesRoot.OpenSubKey("Interface"))
                    {
                        foreach (string iid_string in interface_key.GetSubKeyNames())
                        {
                            if (Guid.TryParse(iid_string, out Guid iid))
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
        }

        private void RunGetInterfaces(object token)
        {
            try
            {
                GetInterfacesInternal((NtToken)token);
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

        private void GetInterfaces(bool sta, int timeout, NtToken token)
        {
            if (timeout > 0)
            {
                Thread timeout_thread = new Thread(ExitProcessThread);
                timeout_thread.Start(timeout);
            }

            if (sta)
            {
                RunGetInterfaces(token);
            }
            else
            {
                Thread th = null;
                th = new Thread(RunGetInterfaces);
                th.SetApartmentState(ApartmentState.MTA);
                th.Start(token);
                th.Join();
            }
        }

        public IEnumerable<COMInterfaceInstance> Interfaces { get { return _interfaces; } }
        public IEnumerable<COMInterfaceInstance> FactoryInterfaces { get { return _factory_interfaces; } }
        public Win32Exception Exception { get { return _ex; } }

        public COMEnumerateInterfaces(Guid clsid, CLSCTX clsctx, string activatable_classid, bool sta, int timeout, NtToken token)
            : this(clsid, clsctx, activatable_classid, new List<COMInterfaceInstance>(), new List<COMInterfaceInstance>())
        {
            GetInterfaces(sta, timeout, token);
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

        public async static Task<COMEnumerateInterfaces> GetInterfacesOOP(COMRuntimeClassEntry ent, COMRegistry registry, NtToken token)
        {
            string apartment = "s";
            if (ent.Threading == ThreadingType.Both
                || ent.Threading == ThreadingType.Mta)
            {
                apartment = "m";
            }
            string command_line = string.Format("{0} {1} {2}", ent.Name, apartment, 
                ent.ActivationType == ActivationType.InProcess ? CLSCTX.INPROC_SERVER : CLSCTX.LOCAL_SERVER);
            var interfaces = await GetInterfacesOOP(command_line, true, registry, token);
            return new COMEnumerateInterfaces(Guid.Empty, 0, ent.Name, interfaces.Interfaces, interfaces.FactoryInterfaces);
        }

        private class InterfaceLists
        {
            public List<COMInterfaceInstance> Interfaces { get; set; }
            public List<COMInterfaceInstance> FactoryInterfaces { get; set; }
        }

        private async static Task<InterfaceLists> GetInterfacesOOP(string command_line, bool runtime_class, COMRegistry registry, NtToken token)
        {
            using (AnonymousPipeServerStream server = new AnonymousPipeServerStream(PipeDirection.In,
                HandleInheritability.Inheritable, 16 * 1024, null))
            {
                using (var imp_token = token?.DuplicateToken(SecurityImpersonationLevel.Impersonation))
                {
                    if (imp_token != null)
                    {
                        imp_token.Inherit = true;
                    }

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
                    List<string> args = new List<string>
                    {
                        runtime_class ? "-r" : "-e",
                        server.GetClientHandleAsString(),
                        command_line
                    };

                    if (imp_token != null)
                    {
                        args.Insert(0, imp_token.Handle.DangerousGetHandle().ToInt32().ToString());
                        args.Insert(0, "-t");
                    }

                    ProcessStartInfo info = new ProcessStartInfo(process, string.Join(" ", args))
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
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
                                    bool factory = false;

                                    if (line.StartsWith("*"))
                                    {
                                        factory = true;
                                        line = line.Substring(1);
                                    }

                                    string[] parts = line.Split(new char[] { ',' }, 3);
                                    if (Guid.TryParse(parts[0], out Guid g))
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
                                            factory_interfaces.Add(new COMInterfaceInstance(g, module_path, vtable_offset, registry));
                                        }
                                        else
                                        {
                                            interfaces.Add(new COMInterfaceInstance(g, module_path, vtable_offset, registry));
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
                                interfaces = new List<COMInterfaceInstance>(new COMInterfaceInstance[] { new COMInterfaceInstance(COMInterfaceEntry.IID_IUnknown, registry) });
                                factory_interfaces = new List<COMInterfaceInstance>(new COMInterfaceInstance[] { new COMInterfaceInstance(COMInterfaceEntry.IID_IUnknown, registry) });
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
        }

        public async static Task<COMEnumerateInterfaces> GetInterfacesOOP(COMCLSIDEntry ent, COMRegistry registry, NtToken token)
        {
            string apartment = "s";
            if (ent.DefaultThreadingModel == COMThreadingModel.Both 
                || ent.DefaultThreadingModel == COMThreadingModel.Free)
            {
                apartment = "m";
            }

            string command_line = string.Format("{0} {1} \"{2}\"", ent.Clsid.ToString("B"), apartment, ent.CreateContext);
            var interfaces = await GetInterfacesOOP(command_line, false, registry, token);
            return new COMEnumerateInterfaces(ent.Clsid, ent.CreateContext, string.Empty, interfaces.Interfaces, interfaces.FactoryInterfaces);
        }
    }
}
