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
using System.Collections.Generic;

namespace OleViewDotNet.Database;

internal class COMPackagedRegistry
{
    public IReadOnlyDictionary<string, COMPackagedEntry> Packages { get; }

    internal COMPackagedRegistry()
    {
        Packages = new Dictionary<string, COMPackagedEntry>();
    }

    internal COMPackagedRegistry(RegistryKey rootKey)
    {
        var packages = new Dictionary<string, COMPackagedEntry>();
        Packages = packages;

        using var packageKey = rootKey.OpenSubKeySafe("Package");
        if (packageKey is null)
        {
            return;
        }

        foreach (var packageName in packageKey.GetSubKeyNames())
        {
            using var packageNameKey = packageKey.OpenSubKeySafe(packageName);
            if (packageNameKey is not null)
            {
                packages[packageName] = new COMPackagedEntry(packageName, packageNameKey);
            }
        }
    }
}
