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
using OleViewDotNet.Utilities;
using System;

namespace OleViewDotNet.Database;

internal class COMPackagedProxyStubEntry
{
    public Guid Clsid { get; }
    public string DisplayName { get; }
    public string DllPath { get; }
    public string DllPath_x86 { get; }
    public string DllPath_x64 { get; }
    public string DllPath_arm { get; }
    public string DllPath_arm64 { get; }

    internal COMPackagedProxyStubEntry(Guid clsid, string packagePath, RegistryKey rootKey)
    {
        Clsid = clsid;
        DisplayName = rootKey.ReadString(valueName: "DisplayName");
        DllPath = rootKey.ReadStringPath(packagePath, valueName: "DllPath");
        DllPath_x86 = rootKey.ReadStringPath(packagePath, valueName: "DllPath_x86");
        DllPath_x64 = rootKey.ReadStringPath(packagePath, valueName: "DllPath_x64");
        DllPath_arm = rootKey.ReadStringPath(packagePath, valueName: "DllPath_arm");
        DllPath_arm64 = rootKey.ReadStringPath(packagePath, valueName: "DllPath_arm64");
    }
}
