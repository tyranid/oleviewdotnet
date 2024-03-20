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
using OleViewDotNet.Interop;
using OleViewDotNet.Security;
using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Utilities;

public sealed class COMStandardActivator
{
    private readonly IStandardActivator m_activator;
    private readonly ISpecialSystemPropertiesActivator m_special_props;

    private object GetObject(Action<COSERVERINFO, MULTI_QI[]> func, Guid? iid, string server, COMAuthInfo auth_info)
    {
        MULTI_QI[] qis = new MULTI_QI[1];
        qis[0] = new MULTI_QI(iid ?? COMKnownGuids.IID_IUnknown);
        using var list = new DisposableList();
        using var auth_info_buffer = auth_info?.ToBuffer(list);
        COSERVERINFO server_info = !string.IsNullOrEmpty(server) ? new(server, auth_info_buffer) : null;
        func(server_info, qis);
        list.Add(qis[0]);
        int hr = qis[0].HResult();
        if (hr != 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }
        return qis[0].GetObject();
    }

    public COMStandardActivator()
    {
        Guid clsid = new("0000033C-0000-0000-c000-000000000046");
        m_activator = (IStandardActivator)COMUtilities.CreateInstanceAsObject(clsid,
            typeof(IStandardActivator).GUID, CLSCTX.INPROC_SERVER, null);
        m_special_props = (ISpecialSystemPropertiesActivator)m_activator;
    }

    public object GetClassObject(Guid clsid, CLSCTX clsctx, Guid? iid = null, string server = null, COMAuthInfo auth_info = null)
    {
        return m_activator.StandardGetClassObject(clsid, clsctx, null, iid ?? COMKnownGuids.IID_IUnknown);
    }

    public object CreateInstance(Guid clsid, CLSCTX clsctx, Guid? iid = null, string server = null, COMAuthInfo auth_info = null)
    {
        return GetObject((s, m) => m_activator.StandardCreateInstance(clsid, IntPtr.Zero, clsctx, s, m.Length, m), iid, server, auth_info);
    }

    public object GetInstanceFromFile(string name, STGM grf_mode, CLSCTX clsctx, Guid? iid = null, 
        Guid? clsid = null, string server = null, COMAuthInfo auth_info = null)
    {
        return GetObject((s, m) => m_activator.StandardGetInstanceFromFile(s, clsid.ToOptional(), 
            IntPtr.Zero, clsctx, grf_mode, name, m.Length, m), iid, server, auth_info);
    }

    public object GetInstanceFromIStorage(IStorage stg, CLSCTX clsctx, Guid? iid = null,
                Guid? clsid = null, string server = null, COMAuthInfo auth_info = null)
    {
        return GetObject((s, m) => m_activator.StandardGetInstanceFromIStorage(s, clsid.ToOptional(),
            IntPtr.Zero, clsctx, stg, m.Length, m), iid, server, auth_info);
    }

    public int SessionId
    {
        get
        {
            m_special_props.GetSessionId2(out int session_id, out _, out _);
            return session_id;
        }
    }

    public bool UseConsole
    {
        get
        {
            m_special_props.GetSessionId2(out _, out int use_console, out _);
            return use_console != 0;
        }
    }

    public bool RemoteThisSessionId
    {
        get
        {
            m_special_props.GetSessionId2(out _, out _, out int remote_session_id);
            return remote_session_id != 0;
        }
    }

    public void SetSessionId(int session_id, bool use_console, bool remote_this_session_id)
    {
        m_special_props.SetSessionId(session_id, use_console ? 1 : 0, remote_this_session_id ? 1 : 0);
    }

    public ProcessRequestType ProcessRequestType
    {
        get => m_special_props.GetProcessRequestType();
        set => m_special_props.SetProcessRequestType(value);
    }

    public Guid PartitionId
    {
        get => m_special_props.GetPartitionId();
        set => m_special_props.SetPartitionId(value);
    }

    public bool ClientImpersonating
    {
        get => m_special_props.GetClientImpersonating() != 0;
        set => m_special_props.SetClientImpersonating(value ? 1 : 0);
    }

    public CLSCTX OriginalClsctx
    {
        get => m_special_props.GetOrigClsctx();
        set => m_special_props.SetOrigClsctx(value);
    }

    public RPC_AUTHN_LEVEL DefaultAuthenticationLevel
    {
        get => m_special_props.GetDefaultAuthenticationLevel();
        set => m_special_props.SetDefaultAuthenticationLevel(value);
    }

    public void SetLUARunLevel(RunLevel run_level, IntPtr hwnd)
    {
        m_special_props.SetLUARunLevel(run_level, hwnd);
    }

    public RunLevel LUARunLevel
    {
        get
        {
            m_special_props.GetLUARunLevel(out RunLevel run_level, out _);
            return run_level;
        }
    }

    public IntPtr LUAHwnd
    {
        get
        {
            m_special_props.GetLUARunLevel(out _, out IntPtr hwnd);
            return hwnd;
        }
    }
}