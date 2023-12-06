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

using System;

namespace OleViewDotNet.Database;

[Flags]
public enum COMAppIDFlags
{
    None = 0,
    ActivateIUServerInDesktop = 0x1,
    SecureServerProcessSDAndBind = 0x2,
    IssueActivationRpcAtIdentify = 0x4,
    IUServerUnmodifiedLogonToken = 0x8,
    IUServerSelfSidInLaunchPermission = 0x10,
    IUServerActivateInClientSessionOnly = 0x20,
    Reserved1 = 0x40,
    RequireSideLoadedPackage = 0x80,
    SessionVirtualAccountServer = 0x100,
    IUServerUnmodifiedClientLogonTokenUser = 0x200,
    IUServerUnmodifiedSessionLogonTokenUser = 0x400,
    AAANoImplicitActivateAsIU = 0x800,
    PerAppBrokerSuspension = 0x1000,
    SuspensionManaged = 0x2000
}
