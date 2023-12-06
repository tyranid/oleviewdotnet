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

namespace OleViewDotNet.Database;

internal class COMPackagedTypeLibVersionEntry
{
    public string Version { get; }
    public string DisplayName { get; }
    public int Flags { get; }
    public string HelpDirectory { get; }
    public int LocaleId { get; }
    public string Win32Path { get; }
    public string Win64Path { get; }

    internal COMPackagedTypeLibVersionEntry(string version, string packagePath, RegistryKey rootKey)
    {
        Version = version;
        DisplayName = rootKey.ReadString(valueName: "DisplayName");
        Flags = rootKey.ReadInt(null, valueName: "Flags");
        HelpDirectory = rootKey.ReadStringPath(packagePath, valueName: "HelpDirectory");
        LocaleId = rootKey.ReadInt(null, "LocaleId");
        Win32Path = rootKey.ReadStringPath(packagePath, valueName: "Win32Path");
        Win64Path = rootKey.ReadStringPath(packagePath, valueName: "Win64Path");
    }
}
