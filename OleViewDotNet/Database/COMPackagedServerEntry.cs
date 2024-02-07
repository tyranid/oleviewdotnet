//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2019
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
using OleViewDotNet.Security;
using OleViewDotNet.Utilities;
using System;

namespace OleViewDotNet.Database;

internal class COMPackagedServerEntry
{
    // Server\{Index}
    public string ApplicationDisplayName { get; }
    public string ApplicationId { get; }
    public string Arguments { get; }
    public string DisplayName { get; }
    public string Executable { get; }
    public string CommandLine { get; }
    public string ExecutionPackageFamily { get; }
    public bool IsSystemExecutable { get; }
    public COMSecurityDescriptor LaunchAndActivationPermission { get; }
    public Guid SurrogateAppId { get; }
    public string SystemExecutableArchitecture { get; }

    internal COMPackagedServerEntry(string packagePath, RegistryKey rootKey)
    {
        SurrogateAppId = rootKey.ReadGuid(null, "SurrogateAppId");
        ApplicationDisplayName = rootKey.ReadString(valueName: "ApplicationDisplayName");
        ApplicationId = rootKey.ReadString(valueName: "ApplicationId");
        Arguments = rootKey.ReadString(valueName: "Arguments");
        DisplayName = rootKey.ReadString(valueName: "DisplayName");
        ExecutionPackageFamily = rootKey.ReadString(valueName: "ExecutionPackageName");
        IsSystemExecutable = rootKey.ReadBool("IsSystemExecutable");
        Executable = rootKey.ReadStringPath(IsSystemExecutable ? Environment.GetFolderPath(Environment.SpecialFolder.System) : packagePath, valueName: "Executable");
        LaunchAndActivationPermission = rootKey.ReadSecurityDescriptor(valueName: "LaunchAndActivationPermission");
        SystemExecutableArchitecture = rootKey.ReadString(valueName: "SystemExecutableArchitecture");
        if (!string.IsNullOrWhiteSpace(Arguments))
        {
            CommandLine = $"\"{Executable}\" {Arguments}";
        }
        else
        {
            CommandLine = Executable;
        }
    }
}
