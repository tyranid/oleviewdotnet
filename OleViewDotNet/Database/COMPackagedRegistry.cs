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
using System;
using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet.Database
{
    internal class COMPackagedClassEntry
    {
        // Class\{Clsid}
        public Guid Clsid { get; }
        public string AutoConvertTo { get; }
        public string ConversionReadable { get; }
        public string ConversionReadWritable { get; }
        public string DataFormats { get; }
        public string DefaultFormatName { get; }
        public string DefaultIcon { get; }
        public string DisplayName { get; }
        public string DllPath { get; }
        public bool EnableOleDefaultHandler { get; }
        public List<Guid> ImplementedCategories { get; }
        public bool InsertableObject { get; }
        public string MiscStatusAspects { get; }
        public string MiscStatusDefault { get; }
        public string ProgId { get; }
        public int ServerId { get; }
        public string ShortDisplayName { get; }
        public COMThreadingModel Threading { get; }
        public string ToolboxBitmap32 { get; }
        public List<Tuple<string, string>> Verbs { get; }
        public string VersionIndependentProgId { get; }

        internal COMPackagedClassEntry(Guid clsid, string packagePath, RegistryKey rootKey)
        {
            Clsid = clsid;
            AutoConvertTo = rootKey.ReadString(valueName: "AutoConvertTo");
            ConversionReadable = rootKey.ReadString(valueName: "ConversionReadable");
            ConversionReadWritable = rootKey.ReadString(valueName: "ConversionReadWritable");
            ConversionReadWritable = rootKey.ReadString(valueName: "ConversionReadWritable");
            DataFormats = rootKey.ReadString(valueName: "DataFormats");
            DefaultFormatName = rootKey.ReadString(valueName: "DefaultFormatName");
            DefaultIcon = rootKey.ReadString(valueName: "DefaultIcon");
            DisplayName = rootKey.ReadString(valueName: "DisplayName");
            DllPath = rootKey.ReadStringPath(packagePath, valueName: "DllPath");
            EnableOleDefaultHandler = rootKey.ReadBool("EnableOleDefaultHandler");
            ImplementedCategories = rootKey.ReadValueNames("ImplementedCategories")
                .Select(n => COMUtilities.ReadOptionalGuid(n)).Where(g => g.HasValue)
                .Select(g => g.Value).ToList();
            InsertableObject = rootKey.ReadBool(valueName: "InsertableObject");
            MiscStatusAspects = rootKey.ReadString(valueName: "MiscStatusAspects");
            MiscStatusDefault = rootKey.ReadString(valueName: "MiscStatusDefault");
            ProgId = rootKey.ReadString(valueName: "ProgId");
            ServerId = rootKey.ReadInt(null, "ServerId");
            ShortDisplayName = rootKey.ReadString(valueName: "ShortDisplayName");
            Threading = (COMThreadingModel)rootKey.ReadInt(null, "Threading");
            ToolboxBitmap32 = rootKey.ReadString(valueName: "ToolboxBitmap32");
            Verbs = rootKey.ReadValues("Verbs").Select(v => Tuple.Create(v.Name, v.Value.ToString())).ToList();
            VersionIndependentProgId = rootKey.ReadString(valueName: "VersionIndependentProgId");
        }
    }

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
        public string LaunchAndActivationPermission { get; }
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
            LaunchAndActivationPermission = rootKey.ReadSddl(valueName: "LaunchAndActivationPermission");
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

    internal class COMPackagedTreatAsClassEntry
    {
        // TreatAsClass\{Clsid}
        public string AutoConvertTo { get; }
        public string DisplayName { get; }
        public Guid TreatAs { get; }

        internal COMPackagedTreatAsClassEntry(RegistryKey rootKey)
        {
            AutoConvertTo = rootKey.ReadString(valueName: "AutoConvertTo");
            DisplayName = rootKey.ReadString(valueName: "DisplayName");
            TreatAs = rootKey.ReadGuid(null, "TreatAs");
        }
    }

    internal class COMPackagedInterfaceEntry
    {
        public Guid Iid { get; }
        public Guid ProxyStubCLSID { get; }
        public bool UseUniversalMarshaler { get; }
        public Guid SynchronousInterface { get; }
        public Guid AsynchronousInterface { get; }
        public Guid TypeLibId { get; }
        public string TypeLibVersionNumber { get; }

        internal COMPackagedInterfaceEntry(Guid iid, RegistryKey rootKey)
        {
            Iid = iid;
            ProxyStubCLSID = rootKey.ReadGuid(null, "ProxyStubCLSID");
            UseUniversalMarshaler = rootKey.ReadBool(valueName: "UseUniversalMarshaler");
            SynchronousInterface = rootKey.ReadGuid(null, "SynchronousInterface");
            AsynchronousInterface = rootKey.ReadGuid(null, "AsynchronousInterface");
            TypeLibId = rootKey.ReadGuid(null, "TypeLibId");
            TypeLibVersionNumber = rootKey.ReadString(valueName: "TypeLibVersionNumber");
        }
    }

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

    internal class COMPackagedTypeLibEntry
    {
        public Guid TypeLibId { get; }
        public List<COMPackagedTypeLibVersionEntry> Versions { get; }

        internal COMPackagedTypeLibEntry(Guid typelibId, IEnumerable<COMPackagedTypeLibVersionEntry> versions)
        {
            TypeLibId = typelibId;
            Versions = new List<COMPackagedTypeLibVersionEntry>(versions);
        }
    }

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
                if (subkey == null)
                {
                    return result;
                }

                foreach (var name in subkey.GetSubKeyNames())
                {
                    if (!keyMap(name, out K key))
                    {
                        continue;
                    }

                    using (var valueKey = subkey.OpenSubKeySafe(name))
                    {
                        if (valueKey != null)
                        {
                            result[key] = valueMap(key, packagePath, valueKey);
                        }
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
            List<COMPackagedTypeLibVersionEntry> result = new List<COMPackagedTypeLibVersionEntry>();

            foreach (var name in rootKey.GetSubKeyNames())
            {
                using (var subkey = rootKey.OpenSubKeySafe(name))
                {
                    if (subkey == null)
                    {
                        continue;
                    }
                    result.Add(new COMPackagedTypeLibVersionEntry(name, packagePath, subkey));
                }
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
            PackagePath = COMUtilities.GetPackagePath(packageId);
            Servers = ReadServers(PackagePath, rootKey);
            Classes = ReadClasses(PackagePath, rootKey);
            TreatAs = ReadTreatAs(rootKey);
            Interfaces = ReadInterfaces(PackagePath, rootKey);
            ProxyStubs = ReadProxyStubs(PackagePath, rootKey);
            TypeLibs = ReadTypeLibs(PackagePath, rootKey);
        }
    }

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

            using (var packageKey = rootKey.OpenSubKeySafe("Package"))
            {
                if (packageKey == null)
                {
                    return;
                }

                foreach (var packageName in packageKey.GetSubKeyNames())
                {
                    using (var packageNameKey = packageKey.OpenSubKeySafe(packageName))
                    {
                        if (packageNameKey != null)
                        {
                            packages[packageName] = new COMPackagedEntry(packageName, packageNameKey);
                        }
                    }
                }
            }
        }
    }
}
