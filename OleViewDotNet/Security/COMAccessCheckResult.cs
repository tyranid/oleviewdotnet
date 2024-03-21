//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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

namespace OleViewDotNet.Security;

public readonly struct COMAccessCheckResult
{
    public string Name => ComObject?.Name ?? string.Empty;
    public COMAccessRights Access { get; }
    public COMAccessRights Launch { get; }
    public ICOMAccessSecurity ComObject { get; }
    public COMSecurityDescriptor AccessSecurity { get; }
    public COMSecurityDescriptor LaunchSecurity { get; }
    public bool IsValid => ComObject is not null;
    public bool LaunchChecked { get; }
    public bool LocalAccess => IsAccessGranted(COMAccessRights.ExecuteLocal);
    public bool RemoteAccess => IsAccessGranted(COMAccessRights.ExecuteRemote);
    public bool LocalActivate => IsLaunchGranted(COMAccessRights.ActivateLocal);
    public bool RemoteActivate => IsLaunchGranted(COMAccessRights.ActivateRemote);
    public bool LocalLaunch => IsLaunchGranted(COMAccessRights.ExecuteLocal);
    public bool RemoteLaunch => IsLaunchGranted(COMAccessRights.ExecuteRemote);

    internal COMAccessCheckResult(COMAccessRights access, COMAccessRights launch, ICOMAccessSecurity obj, 
        COMSecurityDescriptor access_security, COMSecurityDescriptor launch_security, bool launch_checked)
    {
        Access = access;
        Launch = launch;
        ComObject = obj;
        AccessSecurity = access_security;
        LaunchSecurity = launch_security;
        LaunchChecked = launch_checked;
    }

    public bool IsAccessGranted(COMAccessRights desired_access)
    {
        return Access.IsAccessGranted(desired_access);
    }

    public bool IsLaunchGranted(COMAccessRights desired_access)
    {
        return !LaunchChecked || Launch.IsAccessGranted(desired_access);
    }
}