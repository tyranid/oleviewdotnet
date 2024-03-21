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
using System.Collections.Generic;

namespace OleViewDotNet.Database;

internal class COMPackagedEntry
{
    public string PackageId { get; }
    public string PackagePath { get; }
    public IReadOnlyDictionary<int, COMPackagedServerEntry> Servers { get; }
    public IReadOnlyDictionary<Guid, COMPackagedClassEntry> Classes { get; }
    public IReadOnlyDictionary<Guid, COMPackagedTreatAsClassEntry> TreatAs { get; }
    public IReadOnlyDictionary<Guid, COMPackagedInterfaceEntry> Interfaces { get; }
    public IReadOnlyDictionary<Guid, COMPackagedProxyStubEntry> ProxyStubs { get; }
    public IReadOnlyDictionary<Guid, COMPackagedTypeLibEntry> TypeLibs { get; }

    private delegate bool KeyMapFunction<K>(string name, out K key);

    private static Dictionary<K, T> ReadRegistryKeys<K, T>(RegistryKey rootKey, string rootName, string packagePath, KeyMapFunction<K> keyMap, Func<K, string, RegistryKey, T> valueMap)
    {
        var result = new Dictionary<K, T>();
        using (var subkey = rootKey.OpenSubKeySafe(rootName))
        {
            if (subkey is null)
            {
                return result;
            }

            foreach (var name in subkey.GetSubKeyNames())
            {
                if (!keyMap(name, out K key))
                {
                    continue;
                }

                using var valueKey = subkey.OpenSubKeySafe(name);
                if (valueKey is not null)
                {
                    result[key] = valueMap(key, packagePath, valueKey);
                }
            }
        }

        return result;
    }

    private static Dictionary<Guid, T> ReadGuidRegistryKeys<T>(RegistryKey rootKey, string rootName, string packagePath, Func<Guid, string, RegistryKey, T> valueMap)
    {
        return ReadRegistryKeys(rootKey, rootName, packagePath, new KeyMapFunction<Guid>(Guid.TryParse), valueMap);
    }

    private static Dictionary<int, COMPackagedServerEntry> ReadServers(string packagePath, RegistryKey rootKey)
    {
        return ReadRegistryKeys(rootKey, "Server", packagePath, new KeyMapFunction<int>(int.TryParse), (k, pp, reg) => new COMPackagedServerEntry(pp, reg));
    }

    private static Dictionary<Guid, COMPackagedClassEntry> ReadClasses(string packagePath, RegistryKey rootKey)
    {
        return ReadGuidRegistryKeys(rootKey, "Class", packagePath, 
            (key, pp, reg) => new COMPackagedClassEntry(key, pp, reg));
    }

    private static Dictionary<Guid, COMPackagedInterfaceEntry> ReadInterfaces(string packagePath, RegistryKey rootKey)
    {
        return ReadGuidRegistryKeys(rootKey, "Interface", packagePath, 
            (key, pp, reg) => new COMPackagedInterfaceEntry(key, reg));
    }

    private static Dictionary<Guid, COMPackagedTreatAsClassEntry> ReadTreatAs(RegistryKey rootKey)
    {
        return ReadGuidRegistryKeys(rootKey, "TreatAsClass", string.Empty,
            (key, pp, reg) => new COMPackagedTreatAsClassEntry(reg));
    }

    private static Dictionary<Guid, COMPackagedProxyStubEntry> ReadProxyStubs(string packagePath, RegistryKey rootKey)
    {
        return ReadGuidRegistryKeys(rootKey, "ProxyStub", packagePath,
            (key, pp, reg) => new COMPackagedProxyStubEntry(key, pp, reg));
    }

    private static IEnumerable<COMPackagedTypeLibVersionEntry> ReadTypeLibVersions(string packagePath, RegistryKey rootKey)
    {
        List<COMPackagedTypeLibVersionEntry> result = new();

        foreach (var name in rootKey.GetSubKeyNames())
        {
            using var subkey = rootKey.OpenSubKeySafe(name);
            if (subkey is null)
            {
                continue;
            }
            result.Add(new COMPackagedTypeLibVersionEntry(name, packagePath, subkey));
        }

        return result;
    }

    private static Dictionary<Guid, COMPackagedTypeLibEntry> ReadTypeLibs(string packagePath, RegistryKey rootKey)
    {
        return ReadGuidRegistryKeys(rootKey, "TypeLib", packagePath,
            (key, pp, reg) => new COMPackagedTypeLibEntry(key, ReadTypeLibVersions(pp, reg)));
    }

    internal COMPackagedEntry(string packageId, RegistryKey rootKey)
    {
        PackageId = packageId;
        PackagePath = MiscUtilities.GetPackagePath(packageId);
        Servers = ReadServers(PackagePath, rootKey);
        Classes = ReadClasses(PackagePath, rootKey);
        TreatAs = ReadTreatAs(rootKey);
        Interfaces = ReadInterfaces(PackagePath, rootKey);
        ProxyStubs = ReadProxyStubs(PackagePath, rootKey);
        TypeLibs = ReadTypeLibs(PackagePath, rootKey);
    }
}
