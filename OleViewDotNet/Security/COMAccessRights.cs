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
using NtApiDotNet.Utilities.Reflection;
using System;

namespace OleViewDotNet.Security;

[Flags]
public enum COMAccessRights : uint
{
    [SDKName("COM_RIGHTS_EXECUTE")]
    Execute = 1,
    [SDKName("COM_RIGHTS_EXECUTE_LOCAL")]
    ExecuteLocal = 2,
    [SDKName("COM_RIGHTS_EXECUTE_REMOTE")]
    ExecuteRemote = 4,
    [SDKName("COM_RIGHTS_ACTIVATE_LOCAL")]
    ActivateLocal = 8,
    [SDKName("COM_RIGHTS_ACTIVATE_REMOTE")]
    ActivateRemote = 16,
    [SDKName("COM_RIGHTS_EXECUTE_CONTAINER")]
    ExecuteContainer = 32,
    [SDKName("COM_RIGHTS_ACTIVATE_CONTAINER")]
    ActivateContainer = 64,
    [SDKName("GENERIC_READ")]
    GenericRead = GenericAccessRights.GenericRead,
    [SDKName("GENERIC_WRITE")]
    GenericWrite = GenericAccessRights.GenericWrite,
    [SDKName("GENERIC_EXECUTE")]
    GenericExecute = GenericAccessRights.GenericExecute,
    [SDKName("GENERIC_ALL")]
    GenericAll = GenericAccessRights.GenericAll,
}
