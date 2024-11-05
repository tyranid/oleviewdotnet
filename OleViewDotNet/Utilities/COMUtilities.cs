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

using NtApiDotNet;
using NtApiDotNet.Win32;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Security;
using OleViewDotNet.TypeManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Utilities;

public static class COMUtilities
{
    internal static string GetCategoryName(Guid catid)
    {
        Guid clsid = new("{0002E005-0000-0000-C000-000000000046}");
        Guid iid = typeof(ICatInformation).GUID;
        string strDesc = string.Empty;

        if (NativeMethods.CoCreateInstance(clsid, IntPtr.Zero, CLSCTX.INPROC_SERVER, iid, out IntPtr pCatMgr) == 0)
        {
            ICatInformation catInfo = (ICatInformation)Marshal.GetObjectForIUnknown(pCatMgr);

            try
            {
                catInfo.GetCategoryDesc(catid, 0, out IntPtr pStrDesc);
                strDesc = Marshal.PtrToStringUni(pStrDesc);
                Marshal.FreeCoTaskMem(pStrDesc);
            }
            catch (COMException)
            {
            }

            Marshal.ReleaseComObject(catInfo);
            Marshal.Release(pCatMgr);
        }

        if (string.IsNullOrWhiteSpace(strDesc))
        {
            if (catid == new Guid("59fb2056-d625-48d0-a944-1a85b5ab2640"))
            {
                strDesc = "AppContainer Compatible";
            }
            else
            {
                strDesc = catid.FormatGuid();
            }
        }

        return strDesc;
    }


    public static void SaveObjectToStream(object obj, Stream stm)
    {
        IStreamImpl istm = new(stm);


        if (obj is IPersistStream ps)
        {
            ps.Save(istm, false);
        }
        else
        {
            IPersistStreamInit psi = (IPersistStreamInit)obj;

            psi.Save(istm, false);
        }
    }

    public static void LoadObjectFromStream(object obj, Stream stm)
    {
        IStreamImpl istm = new(stm);

        if (obj is IPersistStream ps)
        {
            ps.Load(istm);
        }
        else
        {
            IPersistStreamInit psi = (IPersistStreamInit)obj;

            psi.InitNew();
            psi.Load(istm);
        }
    }

    public static void OleSaveToStream(object obj, Stream stm)
    {
        using BinaryWriter writer = new(stm);
        Guid clsid = GetObjectClass(obj);
        writer.Write(clsid.ToByteArray());
        SaveObjectToStream(obj, stm);
    }

    public static object OleLoadFromStream(Stream stm, out Guid clsid)
    {
        using BinaryReader reader = new(stm);
        clsid = new Guid(reader.ReadBytes(16));
        using var handle = SafeComObjectHandle.CreateInstance(clsid, CLSCTX.SERVER, COMKnownGuids.IID_IUnknown);
        var ret = handle.ToObject();
        LoadObjectFromStream(ret, stm);
        return ret;
    }

    public static object CreateFromMoniker(string moniker, BIND_OPTS3 bind_opts)
    {
        NativeMethods.CoGetObject(moniker, bind_opts, COMKnownGuids.IID_IUnknown, out object ret);
        return ret;
    }

    public static object CreateFromMoniker(string moniker, CLSCTX clsctx)
    {
        BIND_OPTS3 bind_opts = new()
        {
            dwClassContext = clsctx
        };
        return CreateFromMoniker(moniker, bind_opts);
    }

    public static object CreateFromSessionMoniker(Guid clsid, int session_id, bool factory)
    {
        return CreateFromMoniker($"session:{session_id}!{(factory ? "clsid" : "new")}:{clsid}", CLSCTX.LOCAL_SERVER);
    }

    public static object CreateFromElevationMoniker(Guid clsid, bool factory)
    {
        return CreateFromMoniker($"Elevation:Administrator!{(factory ? "clsid" : "new")}:{clsid}", CLSCTX.LOCAL_SERVER);
    }

    public static object UnmarshalObject(Stream stm, Guid iid)
    {
        return NativeMethods.CoUnmarshalInterface(new IStreamImpl(stm), iid);
    }

    public static object UnmarshalObject(byte[] objref)
    {
        return UnmarshalObject(new MemoryStream(objref), COMKnownGuids.IID_IUnknown);
    }

    public static object UnmarshalObject(COMObjRef objref)
    {
        return UnmarshalObject(objref.ToArray());
    }

    public static Guid GetObjectClass(object p)
    {
        Guid ret = Guid.Empty;

        try
        {
            if (p is IPersist persist)
            {
                persist.GetClassID(out ret);
            }
            else if (p is IPersistStream stream)
            {
                stream.GetClassID(out ret);
            }
            else if (p is IPersistStreamInit init)
            {
                init.GetClassID(out ret);
            }
            else if (p is IPersistFile file)
            {
                file.GetClassID(out ret);
            }
            else if (p is IPersistMoniker moniker)
            {
                moniker.GetClassID(out ret);
            }
            else if (p is IPersistStorage storage)
            {
                storage.GetClassID(out ret);
            }
        }
        catch
        {
        }

        return ret;
    }

    public static string GetMonikerDisplayName(IMoniker pmk)
    {
        IBindCtx bindCtx = NativeMethods.CreateBindCtx(0);

        pmk.GetDisplayName(bindCtx, null, out string strDisplayName);
        Marshal.ReleaseComObject(bindCtx);

        return strDisplayName;
    }

    public static byte[] MarshalObject(object obj, Guid iid, MSHCTX mshctx, MSHLFLAGS mshflags)
    {
        MemoryStream stm = new();
        NativeMethods.CoMarshalInterface(new IStreamImpl(stm), iid, obj, mshctx, IntPtr.Zero, mshflags);
        return stm.ToArray();
    }

    public static byte[] MarshalObject(object obj)
    {
        return MarshalObject(obj, COMKnownGuids.IID_IUnknown, MSHCTX.DIFFERENTMACHINE, MSHLFLAGS.NORMAL);
    }

    internal static Dictionary<int, HashSet<string>> GetServicePids()
    {
        var group = ServiceUtils.GetRunningServicesWithProcessIds().GroupBy(s => s.ProcessId);
        return group.ToDictionary(s => s.Key, g => new HashSet<string>(g.Select(s => s.Name), StringComparer.OrdinalIgnoreCase));
    }

    internal static string GetCOMDllName()
    {
        if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "combase.dll")))
        {
            return "combase";
        }
        else
        {
            return "ole32";
        }
    }

    internal static int GetProcessIdFromIPid(Guid ipid)
    {
        return BitConverter.ToUInt16(ipid.ToByteArray(), 4);
    }

    internal static int GetApartmentIdFromIPid(Guid ipid)
    {
        return BitConverter.ToInt16(ipid.ToByteArray(), 6);
    }

    internal static string GetApartmentIdStringFromIPid(Guid ipid)
    {
        int appid = GetApartmentIdFromIPid(ipid);
        return appid switch
        {
            0 => "NTA",
            -1 => "MTA",
            _ => $"STA (Thread ID {appid})",
        };
    }

    internal static ServerInformation? GetServerInformation(object obj, IEnumerable<COMInterfaceEntry> intfs)
    {
        IntPtr intf = Marshal.GetIUnknownForObject(obj);
        IntPtr proxy = IntPtr.Zero;
        try
        {
            foreach (var entry in intfs)
            {
                if (!entry.HasProxy)
                    continue;
                Guid iid = entry.Iid;
                if (Marshal.QueryInterface(intf, ref iid, out proxy) != 0)
                {
                    continue;
                }
                ServerInformation info = new();
                if (NativeMethods.CoDecodeProxy(Process.GetCurrentProcess().Id,
                    proxy.ToInt64(), out info) == 0)
                {
                    return info;
                }
            }
        }
        finally
        {
            Marshal.Release(intf);
            if (proxy != IntPtr.Zero)
            {
                Marshal.Release(proxy);
            }
        }
        return null;
    }

    public static IntPtr CreateInstance(Guid clsid, Guid iid, CLSCTX context, string server, COMAuthInfo auth_info = null)
    {
        IntPtr pInterface = IntPtr.Zero;
        int hr = 0;
        if (!string.IsNullOrWhiteSpace(server))
        {
            MULTI_QI[] qis = new MULTI_QI[1];
            qis[0] = new MULTI_QI(iid);
            using var list = new DisposableList();
            using var auth_info_buffer = auth_info?.ToBuffer(list);
            COSERVERINFO server_info = new(server, auth_info_buffer);
            try
            {
                hr = NativeMethods.CoCreateInstanceEx(clsid, IntPtr.Zero, CLSCTX.REMOTE_SERVER, server_info, 1, qis);
                if (hr == 0)
                {
                    hr = qis[0].HResult();
                    if (hr == 0)
                    {
                        pInterface = qis[0].GetObjectPointer();
                    }
                }
            }
            finally
            {
                ((IDisposable)qis[0]).Dispose();
            }
        }
        else
        {
            hr = NativeMethods.CoCreateInstance(clsid, IntPtr.Zero, context, iid, out pInterface);
        }

        if (hr != 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        return pInterface;
    }

    public static object CreateInstanceAsObject(Guid clsid, Guid iid, CLSCTX context, string server, COMAuthInfo auth_info = null)
    {
        IntPtr pObject = CreateInstance(clsid, iid, context, server, auth_info);
        object ret = null;

        if (pObject != IntPtr.Zero)
        {
            ret = Marshal.GetObjectForIUnknown(pObject);
            Marshal.Release(pObject);
        }

        return ret;
    }

    public static object CreateClassFactory(Guid clsid, Guid iid, CLSCTX context, string server, COMAuthInfo auth_info = null)
    {
        using var list = new DisposableList();
        using var auth_info_buffer = auth_info?.ToBuffer(list);
        COSERVERINFO server_info = !string.IsNullOrWhiteSpace(server) ? new COSERVERINFO(server, auth_info_buffer) : null;

        using var handle = SafeComObjectHandle.GetClassObject(clsid, server_info is not null ? CLSCTX.REMOTE_SERVER
            : context, iid, server_info);
        return handle.ToObject();
    }

    public static object CreateRuntimeClass(string class_name, bool create_in_broker, bool per_user_broker)
    {
        if (create_in_broker)
        {
            IRuntimeBroker broker = CreateBroker(per_user_broker);
            return broker.ActivateInstance(class_name);
        }
        return NativeMethods.RoActivateInstance(class_name);
    }

    public static object CreateActivationFactory(string class_name, Guid iid, bool create_in_broker, bool per_user_broker)
    {
        if (iid == Guid.Empty)
        {
            iid = COMKnownGuids.IID_IUnknown;
        }

        if (create_in_broker)
        {
            IRuntimeBroker broker = CreateBroker(per_user_broker);
            return broker.GetActivationFactory(class_name, iid);
        }
        return NativeMethods.RoGetActivationFactory(class_name, iid);
    }

    private static IMoniker ParseMoniker(IBindCtx bind_context, string moniker_string)
    {
        if (moniker_string == "new")
        {
            using var handle = SafeComObjectHandle.CreateInstance(COMKnownGuids.CLSID_NewMoniker,
                CLSCTX.INPROC_SERVER, COMKnownGuids.IID_IUnknown);
            return (IMoniker)handle.ToObject();
        }
        else
        {
            if (moniker_string.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
                moniker_string.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                moniker_string.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
            {
                int hr = NativeMethods.CreateURLMonikerEx(null, moniker_string, out IMoniker moniker, CreateUrlMonikerFlags.Uniform);
                if (hr != 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
                return moniker;
            }

            return NativeMethods.MkParseDisplayName(bind_context, moniker_string, out int eaten);
        }
    }

    private static IMoniker ParseMoniker(IBindCtx bind_context, string moniker_string, bool composite)
    {
        if (composite)
        {
            IMoniker ret_moniker = null;
            foreach (string m in moniker_string.Split('!'))
            {
                IMoniker moniker = ParseMoniker(bind_context, m);
                ret_moniker?.ComposeWith(moniker, false, out moniker);
                ret_moniker = moniker;
            }
            return ret_moniker;
        }
        else
        {
            return ParseMoniker(bind_context, moniker_string);
        }
    }

    public static IMoniker ParseMoniker(string moniker_string, bool composite)
    {
        IBindCtx bind_context = NativeMethods.CreateBindCtx(0);
        return ParseMoniker(bind_context, moniker_string, composite);
    }

    public static object ParseAndBindMoniker(string moniker_string, bool composite)
    {
        IBindCtx bind_context = NativeMethods.CreateBindCtx(0);
        IMoniker moniker = ParseMoniker(bind_context, moniker_string, composite);
        Guid iid = COMKnownGuids.IID_IUnknown;
        moniker.BindToObject(bind_context, null, ref iid, out object comObj);
        return comObj;
    }

    public static bool IsProxy(object obj)
    {
        return obj is IRpcOptions;
    }

    public static RPCOPT_SERVER_LOCALITY_VALUES GetServerLocality(object obj)
    {
        if (obj is IRpcOptions opts)
        {
            opts.Query(obj, RPCOPT_PROPERTIES.SERVER_LOCALITY, out IntPtr value);
            return (RPCOPT_SERVER_LOCALITY_VALUES)value.ToInt32();
        }
        return RPCOPT_SERVER_LOCALITY_VALUES.PROCESS_LOCAL;
    }

    public static ICOMObjectWrapper CreateStorage(string name, STGM mode, STGFMT format)
    {
        Guid iid = typeof(IStorage).GUID;
        return COMTypeManager.Wrap(NativeMethods.StgCreateStorageEx(name, mode, format, 0, null, IntPtr.Zero, iid), iid, null);
    }

    public static ICOMObjectWrapper CreateReadOnlyStorage(string name)
    {
        return CreateStorage(name, STGM.SHARE_EXCLUSIVE | STGM.READ, STGFMT.Storage);
    }

    public static ICOMObjectWrapper OpenStorage(string name, STGM mode, STGFMT format)
    {
        Guid iid = typeof(IStorage).GUID;
        return COMTypeManager.Wrap(NativeMethods.StgOpenStorageEx(name, mode, format, 0, null, IntPtr.Zero, iid), iid, null);
    }

    public static ICOMObjectWrapper OpenReadOnlyStorage(string name)
    {
        return OpenStorage(name, STGM.SHARE_EXCLUSIVE | STGM.READ, STGFMT.Storage);
    }

    public static void RegisterActivationFilter(IActivationFilter filter)
    {
        int hr = NativeMethods.CoRegisterActivationFilter(filter);
        if (hr != 0)
        {
            throw new Win32Exception(hr);
        }
    }

    public static IRuntimeBroker CreateBroker(bool per_user)
    {
        if (per_user)
        {
            return (IRuntimeBroker)new PerUserRuntimeBrokerClass();
        }
        else
        {
            return (IRuntimeBroker)new RuntimeBrokerClass();
        }
    }

    public static bool SupportsInterface(object obj, Guid iid)
    {
        using SafeComObjectHandle handle = SafeComObjectHandle.FromObject(obj, iid, false);
        return handle != null;
    }
}
