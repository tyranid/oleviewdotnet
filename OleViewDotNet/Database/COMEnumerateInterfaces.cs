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
using OleViewDotNet.Interop;
using OleViewDotNet.Security;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ActivationContext = OleViewDotNet.Interop.SxS.ActivationContext;

namespace OleViewDotNet.Database;

public class COMEnumerateInterfaces
{
    #region Private Members
    private readonly HashSet<COMInterfaceInstance> _interfaces;
    private readonly HashSet<COMInterfaceInstance> _factory_interfaces;
    private readonly Guid _clsid;
    private readonly CLSCTX _clsctx;
    private readonly string _activatable_classid;
    private readonly bool _winrt_component;
    private Win32Exception _ex;
    private long _intf_count;

    private void QueryInterface(IntPtr punk, Guid iid, Dictionary<IntPtr, string> module_names, HashSet<COMInterfaceInstance> list)
    {
        if (punk == IntPtr.Zero)
            return;
        Interlocked.Increment(ref _intf_count);
        if (Marshal.QueryInterface(punk, ref iid, out IntPtr ppout) == 0)
        {
            IntPtr vtable = Marshal.ReadIntPtr(ppout, 0);
            COMInterfaceInstance intf;

            using (SafeLoadLibraryHandle module = SafeLoadLibraryHandle.GetModuleHandle(vtable))
            {
                if (_clsctx == CLSCTX.INPROC_SERVER && module is not null)
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

    private void QueryInterface(IntPtr punk, IEnumerable<Guid> iids, Dictionary<IntPtr, string> module_names, HashSet<COMInterfaceInstance> list)
    {
        if (punk == IntPtr.Zero)
            return;

        Guid IID_IMultiQI = typeof(IMultiQI).GUID;

        if (Marshal.QueryInterface(punk, ref IID_IMultiQI, out IntPtr pqi) == 0)
        {
            try
            {
                IMultiQI multi_qi = (IMultiQI)Marshal.GetObjectForIUnknown(punk);
                List<COMInterfaceEntry> ret = new();
                foreach (var part in iids.Partition(50))
                {
                    Interlocked.Increment(ref _intf_count);
                    using DisposableList<MULTI_QI> qi_list = new(part.Select(p => new MULTI_QI(p)));
                    var qis = qi_list.ToArray();
                    int hr = multi_qi.QueryMultipleInterfaces(qis.Length, qis);
                    if (hr < 0)
                        continue;
                    for (int i = 0; i < qis.Length; ++i)
                    {
                        if (qis[i].HResult() == 0)
                        {
                            list.Add(new COMInterfaceInstance(part[i], null));
                        }
                    }
                }
            }
            finally
            {
                Marshal.Release(pqi);
            }
        }
        else
        {
            foreach (var iid in iids)
            {
                QueryInterface(punk, iid, module_names, list);
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

    private static List<Guid> GetInterfacesFromRegistry()
    {
        List<Guid> ret = new();
        using RegistryKey interface_key = Registry.ClassesRoot.OpenSubKey("Interface");
        foreach (string iid_string in interface_key.GetSubKeyNames())
        {
            if (Guid.TryParse(iid_string, out Guid iid))
            {
                ret.Add(iid);
            }
        }
        return ret;
    }

    private void GetInterfacesInternal(NtToken token)
    {
        Guid IID_IUnknown = COMKnownGuids.IID_IUnknown;
        CLSCTX clsctx = _clsctx;
        if (token is not null)
        {
            clsctx |= CLSCTX.ENABLE_CLOAKING;
        }

        using var imp = token?.Impersonate();
        int hr = 0;
        IntPtr pfactory;
        if (_winrt_component)
        {
            hr = NativeMethods.RoGetActivationFactory(_activatable_classid, IID_IUnknown, out pfactory);
        }
        else
        {
            hr = NativeMethods.CoGetClassObject(_clsid, clsctx, null, IID_IUnknown, out pfactory);
            // If CLASS_E_CLASSNOTAVAILABLE then it might be a dodgy registration, try and 
            // load as a local server instead.
            if (hr == -2147221231)
            {
                clsctx = CLSCTX.LOCAL_SERVER;
                hr = NativeMethods.CoGetClassObject(_clsid, clsctx, null, IID_IUnknown, out pfactory);
            }
        }
        // If we can't get class object, no chance we'll get object.
        if (hr != 0)
        {
            throw new Win32Exception(hr);
        }

        IntPtr punk;
        if (_winrt_component)
        {
            hr = NativeMethods.RoActivateInstance(_activatable_classid, out punk);
        }
        else
        {
            hr = NativeMethods.CoCreateInstance(_clsid, IntPtr.Zero, clsctx,
                                               IID_IUnknown, out punk);
        }
        if (hr != 0)
        {
            punk = IntPtr.Zero;
        }

        try
        {
            HashSet<Guid> intfs = new(GetInterfacesFromRegistry())
            {
                COMKnownGuids.IID_IMarshal,
                COMKnownGuids.IID_IPSFactoryBuffer,
                COMKnownGuids.IID_IStdMarshalInfo,
                COMKnownGuids.IID_IMarshal2
            };

            Dictionary<IntPtr, string> module_names = new();
            
            var actctx = ActivationContext.FromProcess();
            if (actctx is not null)
            {
                foreach (var intf in actctx.ComInterfaces)
                {
                    intfs.Add(intf.Iid);
                }
            }

            QueryInterface(punk, intfs, module_names, _interfaces);
            QueryInterface(pfactory, intfs, module_names, _factory_interfaces);
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

    private void ExitProcessThread(object arg)
    {
        int timeout = (int)arg;
        int curr_timeout = 0;
        long last_intf_count = 0;
        while (curr_timeout < timeout)
        {
            Thread.Sleep(1000);
            long next_intf_count = _intf_count;
            if (next_intf_count == last_intf_count)
            {
                curr_timeout += 1000;
            }
            else
            {
                curr_timeout = 0;
                last_intf_count = next_intf_count;
            }
        }
        Environment.Exit(1);
    }

    private void GetInterfaces(bool sta, int timeout, NtToken token)
    {
        if (timeout > 0)
        {
            Thread timeout_thread = new(ExitProcessThread);
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


    private class InterfaceLists
    {
        public List<COMInterfaceInstance> Interfaces { get; set; }
        public List<COMInterfaceInstance> FactoryInterfaces { get; set; }
    }

    private async static Task<InterfaceLists> GetInterfacesOOP(string class_name, bool mta, COMRegistry registry, COMAccessToken token, CLSCTX clsctx)
    {
        using AnonymousPipeServerStream server = new(PipeDirection.In,
            HandleInheritability.Inheritable, 16 * 1024, null);
;
        List<string> args = new() { "-e", class_name };
        if (mta)
            args.Add("-mta");
        using var imp_token = token?.Token.DuplicateToken(SecurityImpersonationLevel.Impersonation);
        if (imp_token is not null)
        {
            args.Add("-t");
            args.Add(imp_token.Handle.DangerousGetHandle().ToInt32().ToString());
        }
        args.Add("-pipe");
        args.Add(server.GetClientHandleAsString());
        args.Add("-clsctx");
        args.Add(clsctx.ToString());

        Win32ProcessConfig config = AppUtilities.GetConfigForArchitecture(registry.Architecture, string.Join(" ", args));
        config.InheritHandleList.Add(server.ClientSafePipeHandle.DangerousGetHandle());
        if (imp_token is not null)
        {
            config.AddInheritedHandle(imp_token);
        }
        config.InheritHandles = true;

        using var process = Win32Process.CreateProcess(config);

        List<COMInterfaceInstance> interfaces = new();
        List<COMInterfaceInstance> factory_interfaces = new();
        server.DisposeLocalCopyOfClientHandle();

        using StreamReader reader = new(server);
        while (true)
        {
            string line = await reader.ReadLineAsync();
            if (line is null)
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

        if (!await process.Process.WaitAsync(5))
        {
            process.Process.Terminate(0);
        }
        int exitCode = process.Process.ExitStatus;
        if (exitCode != 0)
        {
            interfaces = new List<COMInterfaceInstance>(new COMInterfaceInstance[] { new(COMKnownGuids.IID_IUnknown, registry) });
            factory_interfaces = new List<COMInterfaceInstance>(new COMInterfaceInstance[] { new(COMKnownGuids.IID_IUnknown, registry) });
        }
        return new InterfaceLists() { Interfaces = interfaces, FactoryInterfaces = factory_interfaces };
    }
    #endregion

    #region Public Properties
    public IEnumerable<COMInterfaceInstance> Interfaces => _interfaces;
    public IEnumerable<COMInterfaceInstance> FactoryInterfaces => _factory_interfaces;
    public Win32Exception Exception => _ex;
    #endregion

    #region Constructors
    public COMEnumerateInterfaces(Guid clsid, CLSCTX clsctx, string activatable_classid, bool sta, int timeout, NtToken token)
        : this(clsid, clsctx, activatable_classid, new List<COMInterfaceInstance>(), new List<COMInterfaceInstance>())
    {
        GetInterfaces(sta, timeout, token);
    }
    #endregion

    #region Static Methods
    public async static Task<COMEnumerateInterfaces> GetInterfacesOOP(COMRuntimeClassEntry ent, COMRegistry registry, COMAccessToken token)
    {
        bool mta = ent.Threading == ThreadingType.Both || ent.Threading == ThreadingType.Mta;
        CLSCTX clsctx = ent.ActivationType == ActivationType.InProcess ? CLSCTX.INPROC_SERVER : CLSCTX.LOCAL_SERVER;
        var interfaces = await GetInterfacesOOP(ent.Name, mta, registry, token, clsctx);
        return new COMEnumerateInterfaces(Guid.Empty, 0, ent.Name, interfaces.Interfaces, interfaces.FactoryInterfaces);
    }

    public async static Task<COMEnumerateInterfaces> GetInterfacesOOP(COMCLSIDEntry ent, COMRegistry registry, COMAccessToken token)
    {
        bool mta = ent.DefaultThreadingModel == COMThreadingModel.Both || ent.DefaultThreadingModel == COMThreadingModel.Free;

        var interfaces = await GetInterfacesOOP(ent.Clsid.ToString("B"), mta, registry, token, ent.CreateContext);
        return new COMEnumerateInterfaces(ent.Clsid, ent.CreateContext, string.Empty, interfaces.Interfaces, interfaces.FactoryInterfaces);
    }
    #endregion
}
