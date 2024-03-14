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
    public int ProcessId { get; }
    public string ExecutablePath { get; }
    public string Name => Path.GetFileNameWithoutExtension(ExecutablePath);
    public IReadOnlyList<COMIPIDEntry> Ipids { get; }
    public IReadOnlyList<COMIPIDEntry> RunningIpids => Ipids.Where(i => i.IsRunning).ToList().AsReadOnly();
    public bool Is64Bit { get; }
    public Guid AppId { get; }
    public COMSecurityDescriptor AccessPermissions { get; }
    public COMSecurityDescriptor LRpcPermissions { get; }
    public string User => Token.User;
    public COMSid UserSid => Token.UserSid;
    public string RpcEndpoint { get; private set; }
    public EOLE_AUTHENTICATION_CAPABILITIES Capabilities { get; }
    public RPC_AUTHN_LEVEL AuthnLevel { get; }
    public RPC_IMP_LEVEL ImpLevel { get; }
    public IntPtr AccessControl { get; }
    public IntPtr STAMainHWnd { get; }
    public IReadOnlyList<COMProcessClassRegistration> Classes { get; }
    public GLOBALOPT_UNMARSHALING_POLICY_VALUES UnmarshalPolicy { get; }
    public IReadOnlyList<COMIPIDEntry> Clients { get; }
    public IReadOnlyList<COMRuntimeActivableClassEntry> ActivatableClasses { get; }
    public COMProcessToken Token { get; }
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

        return UnmarshalPolicy switch
        {
            GLOBALOPT_UNMARSHALING_POLICY_VALUES.NORMAL => true,
            GLOBALOPT_UNMARSHALING_POLICY_VALUES.HYBRID => !ac,
            GLOBALOPT_UNMARSHALING_POLICY_VALUES.STRONG => false,
            _ => false,
        };
    }

    public bool CustomMarshalAllowed => CustomMarshalAllowedInternal(false);

    public bool CustomMarshalAllowedFromAppContainer => CustomMarshalAllowedInternal(true);

    public string ActivationFilterVTable
    {
        get;
    }

    COMSecurityDescriptor ICOMAccessSecurity.DefaultAccessPermission => null;

    COMSecurityDescriptor ICOMAccessSecurity.DefaultLaunchPermission => null;

    internal COMProcessEntry(int pid, string path, List<COMIPIDEntry> ipids,
        bool is64bit, Guid appid, COMSecurityDescriptor access_perm, COMSecurityDescriptor lrpc_perm, string rpc_endpoint, 
        EOLE_AUTHENTICATION_CAPABILITIES capabilities,
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
