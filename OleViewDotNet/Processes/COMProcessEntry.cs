//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017
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

using OleViewDotNet.Interop;
using OleViewDotNet.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActivationContext = OleViewDotNet.Interop.SxS.ActivationContext;

namespace OleViewDotNet.Processes;

public class COMProcessEntry : ICOMAccessSecurity
{
    public int ProcessId { get; private set; }
    public string ExecutablePath { get; private set; }
    public string Name => Path.GetFileNameWithoutExtension(ExecutablePath);
    public IEnumerable<COMIPIDEntry> Ipids { get; private set; }
    public IEnumerable<COMIPIDEntry> RunningIpids => Ipids.Where(i => i.IsRunning);
    public bool Is64Bit { get; private set; }
    public Guid AppId { get; private set; }
    public string AccessPermissions { get; private set; }
    public string LRpcPermissions { get; private set; }
    public string User => Token.User;
    public string UserSid => Token.UserSid;
    public string RpcEndpoint { get; private set; }
    public EOLE_AUTHENTICATION_CAPABILITIES Capabilities { get; private set; }
    public RPC_AUTHN_LEVEL AuthnLevel { get; private set; }
    public RPC_IMP_LEVEL ImpLevel { get; private set; }
    public IntPtr AccessControl { get; private set; }
    public IntPtr STAMainHWnd { get; private set; }
    public IEnumerable<COMProcessClassRegistration> Classes { get; private set; }
    public GLOBALOPT_UNMARSHALING_POLICY_VALUES UnmarshalPolicy { get; private set; }
    public IEnumerable<COMIPIDEntry> Clients { get; private set; }
    public IEnumerable<COMRuntimeActivableClassEntry> ActivatableClasses { get; private set; }
    public COMProcessToken Token { get; private set; }
    public ActivationContext ActivationContext { get; }
    public IntPtr MTAContext { get; }
    public Guid ProcessSecret { get; }

    public string PackageId => Token.PackageName?.FullName ?? string.Empty;
    public string PackageName => Token.PackageName?.Name ?? string.Empty;
    public ulong PackageHostId => Token.PackageHostId;
    public bool Sandboxed => Token.Sandboxed;

    private bool CustomMarshalAllowedInternal(bool ac)
    {
        if ((Capabilities & EOLE_AUTHENTICATION_CAPABILITIES.NO_CUSTOM_MARSHAL) != 0)
        {
            return false;
        }

        switch (UnmarshalPolicy)
        {
            case GLOBALOPT_UNMARSHALING_POLICY_VALUES.NORMAL:
                return true;
            case GLOBALOPT_UNMARSHALING_POLICY_VALUES.HYBRID:
                return !ac;
            case GLOBALOPT_UNMARSHALING_POLICY_VALUES.STRONG:
                return false;
            default:
                return false;
        }
    }

    public bool CustomMarshalAllowed => CustomMarshalAllowedInternal(false);

    public bool CustomMarshalAllowedFromAppContainer => CustomMarshalAllowedInternal(true);

    public string ActivationFilterVTable
    {
        get;
    }

    string ICOMAccessSecurity.DefaultAccessPermission => string.Empty;

    string ICOMAccessSecurity.DefaultLaunchPermission => string.Empty;

    internal COMProcessEntry(int pid, string path, List<COMIPIDEntry> ipids,
        bool is64bit, Guid appid, string access_perm, string lrpc_perm, string rpc_endpoint, EOLE_AUTHENTICATION_CAPABILITIES capabilities,
        RPC_AUTHN_LEVEL authn_level, RPC_IMP_LEVEL imp_level, GLOBALOPT_UNMARSHALING_POLICY_VALUES unmarshal_policy,
        IntPtr access_control, IntPtr sta_main_hwnd, List<COMProcessClassRegistration> classes,
        string activation_filter_vtable, List<COMIPIDEntry> clients, List<COMRuntimeActivableClassEntry> activatable_classes,
        COMProcessToken token, ActivationContext activation_context, IntPtr mta_context, Guid process_secret)
    {
        ProcessId = pid;
        ExecutablePath = path;
        foreach (var ipid in ipids)
        {
            ipid.Process = this;
        }
        Ipids = ipids.AsReadOnly();
        Is64Bit = is64bit;
        AppId = appid;
        AccessPermissions = access_perm;
        LRpcPermissions = lrpc_perm;
        if (!string.IsNullOrWhiteSpace(rpc_endpoint))
        {
            RpcEndpoint = "OLE" + rpc_endpoint;
        }
        else
        {
            RpcEndpoint = string.Empty;
        }
        Capabilities = capabilities;
        AuthnLevel = authn_level;
        ImpLevel = imp_level;
        AccessControl = access_control;
        STAMainHWnd = sta_main_hwnd;
        Classes = classes.AsReadOnly();
        UnmarshalPolicy = unmarshal_policy;
        foreach (var c in Classes)
        {
            c.Process = this;
        }
        ActivationFilterVTable = activation_filter_vtable;
        foreach (var client in clients)
        {
            client.Process = this;
        }
        Clients = clients.AsReadOnly();
        ActivatableClasses = activatable_classes.AsReadOnly();
        Token = token;
        ActivationContext = activation_context;
        MTAContext = mta_context;
        ProcessSecret = process_secret;
    }

    public override string ToString()
    {
        return $"{ProcessId} {Name}";
    }
}
