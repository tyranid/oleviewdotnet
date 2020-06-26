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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Collections.ObjectModel;
using NtApiDotNet;

namespace OleViewDotNet.Database
{
    public enum COMServerType
    {
        UnknownServer = 0,
        InProcServer32 = 1,
        LocalServer32 = 2,
        InProcHandler32 = 3,
    }

    public enum COMThreadingModel
    {
        None,
        Apartment,
        Both,
        Free,
        Neutral
    }

    public class COMCLSIDElevationEntry : IXmlSerializable
    {
        public bool Enabled { get; private set; }
        public string IconReference { get; private set; }
        public IEnumerable<Guid> VirtualServerObjects { get; private set; }
        public bool AutoApproval { get; private set; }

        internal COMCLSIDElevationEntry(RegistryKey key, RegistryKey vso_key, bool auto_approval)
        {
            Enabled = COMUtilities.ReadInt(key, null, "Enabled") != 0;
            IconReference = COMUtilities.ReadString(key, null, "IconReference");
            HashSet<Guid> vsos = new HashSet<Guid>();
            if (vso_key != null)
            {
                foreach (string value in vso_key.GetValueNames())
                {
                    if (Guid.TryParse(value, out Guid guid))
                    {
                        vsos.Add(guid);
                    }
                }
            }
            AutoApproval = auto_approval;
            VirtualServerObjects = new List<Guid>(vsos).AsReadOnly();
        }

        internal COMCLSIDElevationEntry()
        {
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Enabled = reader.ReadBool("enabled");
            AutoApproval = reader.ReadBool("auto");
            IconReference = reader.ReadString("icon");
            VirtualServerObjects = reader.ReadGuids("vsos");
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteBool("enabled", Enabled);
            writer.WriteBool("auto", AutoApproval);
            writer.WriteOptionalAttributeString("icon", IconReference);
            writer.WriteGuids("vsos", VirtualServerObjects);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMCLSIDElevationEntry right = obj as COMCLSIDElevationEntry;
            if (right == null)
            {
                return false;
            }

            return Enabled == right.Enabled && IconReference == right.IconReference 
                && AutoApproval == right.AutoApproval && VirtualServerObjects.SequenceEqual(right.VirtualServerObjects);
        }

        public override int GetHashCode()
        {
            return Enabled.GetHashCode() ^ IconReference.GetHashCode() 
                ^ AutoApproval.GetHashCode() ^ VirtualServerObjects.GetEnumHashCode();
        }
    }

    public class COMCLSIDServerDotNetEntry : IXmlSerializable
    {
        public string AssemblyName { get; private set; }
        public string ClassName { get; private set; }
        public string CodeBase { get; private set; }
        public string RuntimeVersion { get; private set; }

        internal COMCLSIDServerDotNetEntry()
        {
        }

        internal COMCLSIDServerDotNetEntry(RegistryKey key)
        {
            AssemblyName = COMUtilities.ReadString(key, null, "Assembly");
            ClassName = COMUtilities.ReadString(key, null, "Class");
            CodeBase = COMUtilities.ReadString(key, null, "CodeBase");
            RuntimeVersion = COMUtilities.ReadString(key, null, "RuntimeVersion");
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            AssemblyName = reader.ReadString("asm");
            ClassName = reader.ReadString("cls");
            CodeBase = reader.ReadString("code");
            RuntimeVersion = reader.ReadString("ver");
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteOptionalAttributeString("asm", AssemblyName);
            writer.WriteOptionalAttributeString("cls", ClassName);
            writer.WriteOptionalAttributeString("code", CodeBase);
            writer.WriteOptionalAttributeString("ver", RuntimeVersion);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMCLSIDServerDotNetEntry right = obj as COMCLSIDServerDotNetEntry;
            if (right == null)
            {
                return false;
            }

            return AssemblyName.Equals(right.AssemblyName) && ClassName.Equals(right.ClassName) 
                && CodeBase.Equals(right.CodeBase) && RuntimeVersion.Equals(right.RuntimeVersion);
        }

        public override int GetHashCode()
        {
            return AssemblyName.GetHashCode() ^ ClassName.GetHashCode()
                ^ CodeBase.GetHashCode() ^ RuntimeVersion.GetHashCode();
        }
    }

    public class COMCLSIDServerEntry : IXmlSerializable
    {
        /// <summary>
        /// The absolute path to the server.
        /// </summary>
        public string Server { get; private set; }
        /// <summary>
        /// Command line for local server.
        /// </summary>
        public string CommandLine { get; private set; }
        /// <summary>
        /// The type of server entry.
        /// </summary>
        public COMServerType ServerType { get; private set; }
        /// <summary>
        /// The threading model.
        /// </summary>
        public COMThreadingModel ThreadingModel { get; private set; }
        public COMCLSIDServerDotNetEntry DotNet { get; private set; }
        public bool HasDotNet { get { return DotNet != null; } }
        public string RawServer { get; private set; }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMCLSIDServerEntry right = obj as COMCLSIDServerEntry;
            if (right == null)
            {
                return false;
            }

            return Server == right.Server
                && CommandLine == right.CommandLine
                && ServerType == right.ServerType 
                && ThreadingModel == right.ThreadingModel
                && RawServer == right.RawServer;
        }

        public override int GetHashCode()
        {
            return Server.GetHashCode() ^ CommandLine.GetHashCode() 
                ^ ServerType.GetHashCode() ^ ThreadingModel.GetHashCode()
                ^ RawServer.GetSafeHashCode();
        }

        private static bool IsInvalidFileName(string filename)
        {
            return filename.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
        }

        private static string ProcessFileName(string filename, bool process_command_line)
        {
            string temp = filename.Trim();

            if (temp.StartsWith("\""))
            {
                int lastIndex = temp.IndexOf('"', 1);
                if (lastIndex >= 1)
                {
                    filename = temp.Substring(1, lastIndex - 1);
                }
            }
            else if (process_command_line && temp.Contains(" "))
            {
                if (!File.Exists(temp))
                {
                    int index = temp.IndexOf(' ');
                    while (index > 0)
                    {
                        string name = temp.Substring(0, index);

                        if (File.Exists(name) || (Path.GetExtension(name) == string.Empty && File.Exists(name + ".exe")))
                        {
                            filename = name;
                            break;
                        }

                        index = temp.IndexOf(' ', index + 1);
                    }


                    if (index < 0)
                    {
                        // We've run out of options, just take the first string
                        filename = temp.Split(' ')[0];
                    }
                }
            }

            filename = filename.Trim();

            try
            {
                // Expand out any short filenames
                if (filename.Contains("~") && !IsInvalidFileName(filename))
                {
                    filename = Path.GetFullPath(filename);
                }
            }
            catch (IOException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (ArgumentException)
            {
            }

            return filename;
        }

        private static COMThreadingModel ReadThreadingModel(RegistryKey key)
        {
            string threading_model = key.ReadString(valueName: "ThreadingModel");
            switch (threading_model.ToLower())
            {
                case "both":
                    return COMThreadingModel.Both;
                case "free":
                    return COMThreadingModel.Free;
                case "neutral":
                    return COMThreadingModel.Neutral;
                case "apartment":
                    return COMThreadingModel.Apartment;
                default:
                    return COMThreadingModel.None;
            }
        }

        internal COMCLSIDServerEntry(COMServerType server_type, string server, COMThreadingModel threading_model)
        {
            Server = server;
            CommandLine = string.Empty;
            ServerType = server_type;
            ThreadingModel = threading_model;
        }

        internal COMCLSIDServerEntry(COMServerType server_type, string server, string commandLine)
            : this(server_type, server, COMThreadingModel.Apartment)
        {
            CommandLine = commandLine;
        }

        internal COMCLSIDServerEntry(COMServerType server_type, string server) 
            : this(server_type, server, COMThreadingModel.Apartment)
        {
        }

        internal COMCLSIDServerEntry(COMServerType server_type) 
            : this(server_type, string.Empty)
        {
        }

        internal COMCLSIDServerEntry() : this(COMServerType.UnknownServer)
        {
        }

        internal COMCLSIDServerEntry(RegistryKey key, COMServerType server_type)
            : this(server_type)
        {
            string server_string = key.ReadString();
            RawServer = key.ReadString(options: RegistryValueOptions.DoNotExpandEnvironmentNames);

            if (string.IsNullOrWhiteSpace(server_string))
            {
                // TODO: Support weird .NET registration which registers a .NET version string.
                return;
            }

            bool process_command_line = false;
            CommandLine = string.Empty;

            if (server_type == COMServerType.LocalServer32)
            {
                string executable = key.ReadString(valueName: "ServerExecutable");
                if (!string.IsNullOrWhiteSpace(executable))
                {
                    server_string = executable;
                }
                else
                {
                    process_command_line = true;
                }
                CommandLine = server_string;
                ThreadingModel = COMThreadingModel.Both;
            }
            else if (server_type == COMServerType.InProcServer32)
            {
                ThreadingModel = ReadThreadingModel(key);
                if (key.GetValue("Assembly") != null)
                {
                    DotNet = new COMCLSIDServerDotNetEntry(key);
                }
            }
            else
            {
                ThreadingModel = COMThreadingModel.Apartment;
            }

            Server = ProcessFileName(server_string, process_command_line);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Server = reader.ReadString("server");
            CommandLine = reader.ReadString("cmdline");
            ServerType = reader.ReadEnum<COMServerType>("type");
            ThreadingModel = reader.ReadEnum<COMThreadingModel>("model");
            RawServer = reader.ReadString("rawserver");
            if (reader.ReadBool("dotnet"))
            {
                IEnumerable<COMCLSIDServerDotNetEntry> service = 
                    reader.ReadSerializableObjects<COMCLSIDServerDotNetEntry>("dotnet", () => new COMCLSIDServerDotNetEntry());
                DotNet = service.FirstOrDefault();
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteOptionalAttributeString("server", Server);
            writer.WriteOptionalAttributeString("cmdline", CommandLine);
            writer.WriteEnum("type", ServerType);
            writer.WriteEnum("model", ThreadingModel);
            writer.WriteOptionalAttributeString("rawserver", RawServer);
            if (DotNet != null)
            {
                writer.WriteBool("dotnet", true);
                writer.WriteSerializableObjects("dotnet", 
                    new COMCLSIDServerDotNetEntry[] { DotNet });
            }
        }

        public override string ToString()
        {
            return Server;
        }
    }

    public interface ICOMClassEntry
    {
        /// <summary>
        /// The name of the class entry.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The clsid of the class entry.
        /// </summary>
        Guid Clsid { get; }
        /// <summary>
        /// The default server name.
        /// </summary>
        string DefaultServer { get; }

        /// <summary>
        /// Get list of supported Interface IIDs (that we know about)
        /// NOTE: This will load the object itself to check what is supported, it _might_ crash the app
        /// The returned array is cached so subsequent calls to this function return without calling into COM
        /// </summary>
        /// <param name="refresh">Force the supported interface list to refresh</param>
        /// <param name="token">Token to use when querying for the interfaces.</param>
        /// <returns>Returns true if supported interfaces were refreshed.</returns>
        /// <exception cref="Win32Exception">Thrown on error.</exception>
        Task<bool> LoadSupportedInterfacesAsync(bool refresh, NtToken token);

        /// <summary>
        /// Get list of supported Interface IIDs Synchronously
        /// </summary>
        /// <param name="refresh">Force the supported interface list to refresh</param>
        /// <param name="token">Token to use when querying for the interfaces.</param>
        /// <returns>Returns true if supported interfaces were refreshed.</returns>
        /// <exception cref="Win32Exception">Thrown on error.</exception>
        bool LoadSupportedInterfaces(bool refresh, NtToken token);

        /// <summary>
        /// Indicates that the class' interface list has been loaded.
        /// </summary>
        bool InterfacesLoaded {  get; }

        /// <summary>
        /// Get list of interfaces.
        /// </summary>
        /// <remarks>You must have called LoadSupportedInterfaces before this call to get any useful output.</remarks>
        IEnumerable<COMInterfaceInstance> Interfaces { get; }

        /// <summary>
        /// Get list of factory interfaces.
        /// </summary>
        /// <remarks>You must have called LoadSupportedFactoryInterfaces before this call to get any useful output.</remarks>
        IEnumerable<COMInterfaceInstance> FactoryInterfaces { get; }

        /// <summary>
        /// Create an instance of this object.
        /// </summary>
        /// <param name="dwContext">The class context.</param>
        /// <param name="server">The remote server address.</param>
        /// <returns>The instance of the object.</returns>
        object CreateInstanceAsObject(CLSCTX dwContext, string server);

        /// <summary>
        /// Create an instance of this object's class factory.
        /// </summary>
        /// <param name="dwContext">The class context.</param>
        /// <param name="server">The remote server address.</param>
        /// <returns>The instance of the object.</returns>
        object CreateClassFactory(CLSCTX dwContext, string server);

        /// <summary>
        /// True if this class supports remote activation.
        /// </summary>
        bool SupportsRemoteActivation { get; }
    }

    public class COMCLSIDEntry : IComparable<COMCLSIDEntry>, IXmlSerializable, ICOMClassEntry, ICOMAccessSecurity, IComGuid
    {
        private List<COMInterfaceInstance> m_interfaces;
        private List<COMInterfaceInstance> m_factory_interfaces;

        private static HashSet<Guid> LoadAppActivatableClsids()
        {
            HashSet<Guid> app_activatable = new HashSet<Guid>();
            using (RegistryKey activatable_key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\WindowsRuntime\AllowedCOMCLSIDs"))
            {
                if (activatable_key != null)
                {
                    foreach (string clsid in activatable_key.GetSubKeyNames())
                    {
                        if (Guid.TryParse(clsid, out Guid guid))
                        {
                            app_activatable.Add(guid);
                        }
                    }
                }
            }
            return app_activatable;
        }

        private static HashSet<Guid> _app_activatable = LoadAppActivatableClsids();

        public int CompareTo(COMCLSIDEntry right)
        {
            return string.Compare(Name, right.Name);
        }
        
        private COMCLSIDServerEntry ReadServerKey(Dictionary<COMServerType, COMCLSIDServerEntry> servers, RegistryKey key, COMServerType server_type)
        {
            using (RegistryKey server_key = key.OpenSubKey(server_type.ToString()))
            {
                if (server_key == null)
                {
                    return null;
                }

                COMCLSIDServerEntry entry = new COMCLSIDServerEntry(server_key, server_type);
                if (!string.IsNullOrWhiteSpace(entry.Server))
                {
                    servers.Add(server_type, new COMCLSIDServerEntry(server_key, server_type));
                }
                return entry;
            }
        }

        private void LoadFromKey(RegistryKey key)
        {
            HashSet<Guid> categories = new HashSet<Guid>();
            object name = key.GetValue(null);
            Name = null;
            if (name != null)
            {
                string s = name.ToString().Trim();

                if (s.Length > 0)
                {
                    Name = name.ToString();
                }
            }

            bool fake_name = false;
            if (Name == null)
            {
                fake_name = true;
                Name = Clsid.FormatGuidDefault();
            }

            Dictionary<COMServerType, COMCLSIDServerEntry> servers = new Dictionary<COMServerType, COMCLSIDServerEntry>();
            COMCLSIDServerEntry inproc_server = ReadServerKey(servers, key, COMServerType.InProcServer32);
            ReadServerKey(servers, key, COMServerType.LocalServer32);
            ReadServerKey(servers, key, COMServerType.InProcHandler32);
            Servers = new ReadOnlyDictionary<COMServerType, COMCLSIDServerEntry>(servers);

            if (fake_name && inproc_server != null && inproc_server.HasDotNet)
            {
                Name = string.Format("{0}, {1}", inproc_server.DotNet.ClassName, inproc_server.DotNet.AssemblyName);
            }

            AppID = COMUtilities.ReadGuid(key, null, "AppID");
            if (AppID == Guid.Empty)
            {
                AppID = COMUtilities.ReadGuid(Registry.ClassesRoot,
                        string.Format(@"AppID\{0}", COMUtilities.GetFileName(DefaultServer)), "AppID");
            }

            if (AppID != Guid.Empty && !servers.ContainsKey(COMServerType.LocalServer32))
            {
                servers.Add(COMServerType.LocalServer32, new COMCLSIDServerEntry(COMServerType.LocalServer32, "<APPID HOSTED>"));
            }

            TypeLib = COMUtilities.ReadGuid(key, "TypeLib", null);
            if (key.HasSubkey("Control"))
            {
                categories.Add(COMCategory.CATID_Control);
            }

            if (key.HasSubkey("Insertable"))
            {
                categories.Add(COMCategory.CATID_Insertable);
            }

            if (key.HasSubkey("DocObject"))
            {
                categories.Add(COMCategory.CATID_Document);
            }

            using (RegistryKey catkey = key.OpenSubKey("Implemented Categories"))
            {
                if (catkey != null)
                {
                    string[] subKeys = catkey.GetSubKeyNames();
                    foreach (string s in subKeys)
                    {
                        if (Guid.TryParse(s, out Guid g))
                        {
                            categories.Add(g);
                        }
                    }
                }
            }

            Categories = categories.ToList().AsReadOnly();
            TreatAs = COMUtilities.ReadGuid(key, "TreatAs", null);

            using (RegistryKey elev_key = key.OpenSubKey("Elevation"),
                vso_key = key.OpenSubKey("VirtualServerObjects"))
            {
                if (elev_key != null)
                {
                    using (var base_key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, 
                        Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default))
                    {
                        int auto_approval = COMUtilities.ReadInt(base_key, 
                            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\UAC\COMAutoApprovalList", Clsid.ToString("B"));
                        Elevation = new COMCLSIDElevationEntry(elev_key, vso_key, auto_approval != 0);
                    }
                }
            }

            ActivatableFromApp = _app_activatable.Contains(Clsid);
            using (RegistryKey trustkey = Registry.LocalMachine.OpenSubKey(@"Software\Classes\Unmarshalers\System\" + Clsid.ToString("B")))
            {
                TrustedMarshaller = trustkey != null ? true : categories.Contains(COMCategory.CATID_TrustedMarshaler);
            }

            Source = key.GetSource();
        }

        internal COMCLSIDEntry(COMRegistry registry, Guid clsid, COMPackagedEntry packageEntry,
            COMPackagedClassEntry classEntry) : this(registry, clsid)
        {
            Source = COMRegistryEntrySource.Packaged;
            Name = classEntry.DisplayName;
            PackageId = packageEntry.PackageId;

            Dictionary<COMServerType, COMCLSIDServerEntry> servers = new Dictionary<COMServerType, COMCLSIDServerEntry>();
            if (!string.IsNullOrWhiteSpace(classEntry.DllPath))
            {
                servers.Add(COMServerType.InProcServer32, new COMCLSIDServerEntry(COMServerType.InProcServer32, classEntry.DllPath, classEntry.Threading));
            }

            if (packageEntry.Servers.ContainsKey(classEntry.ServerId))
            {
                COMPackagedServerEntry server = packageEntry.Servers[classEntry.ServerId];
                AppID = server.SurrogateAppId;
                string serverPath = "<APPID HOSTED>";
                string commandLine = string.Empty;
                if (AppID == Guid.Empty)
                {
                    serverPath = server.Executable;
                    commandLine = server.CommandLine;
                }
                servers.Add(COMServerType.LocalServer32, new COMCLSIDServerEntry(COMServerType.LocalServer32, serverPath, commandLine));
            }

            Servers = new ReadOnlyDictionary<COMServerType, COMCLSIDServerEntry>(servers);
            Categories = classEntry.ImplementedCategories.AsReadOnly();
        }

        internal COMCLSIDEntry(COMRegistry registry, Guid clsid, COMPackagedEntry packageEntry,
            COMPackagedProxyStubEntry proxyEntry) : this(registry, clsid)
        {
            Source = COMRegistryEntrySource.Packaged;
            Name = proxyEntry.DisplayName;
            PackageId = packageEntry.PackageId;

            Dictionary<COMServerType, COMCLSIDServerEntry> servers = new Dictionary<COMServerType, COMCLSIDServerEntry>();
            string dllPath = Environment.Is64BitProcess ? proxyEntry.DllPath_x64 : proxyEntry.DllPath_x86;
            if (string.IsNullOrWhiteSpace(dllPath))
            {
                dllPath = proxyEntry.DllPath;
            }
            if (!string.IsNullOrWhiteSpace(dllPath))
            {
                servers.Add(COMServerType.InProcServer32, new COMCLSIDServerEntry(COMServerType.InProcServer32, dllPath, COMThreadingModel.Both));
            }

            Servers = new ReadOnlyDictionary<COMServerType, COMCLSIDServerEntry>(servers);
        }

        public COMCLSIDEntry(COMRegistry registry, Guid clsid, RegistryKey rootKey) : this(registry, clsid)
        {
            LoadFromKey(rootKey);
        }

        private COMCLSIDEntry(COMRegistry registry, Guid clsid) : this(registry)
        {
            Clsid = clsid;
            Servers = new Dictionary<COMServerType, COMCLSIDServerEntry>();
            Name = string.Empty;
            Categories = new Guid[0];
            PackageId = string.Empty;
        }

        public COMCLSIDEntry(COMRegistry registry, Guid clsid, COMServerType type)
            : this(registry, clsid)
        {
            Name = clsid.ToString();
        }

        internal COMCLSIDEntry(COMRegistry registry)
        {
            Database = registry;
        }

        internal COMCLSIDEntry(COMRegistry registry, ActCtxComServerRedirection com_server)
            : this(registry, com_server.Clsid)
        {
            Clsid = com_server.Clsid;
            TypeLib = com_server.TypeLibraryId;
            Servers[COMServerType.InProcServer32] = 
                new COMCLSIDServerEntry(COMServerType.InProcServer32, com_server.FullPath, com_server.ThreadingModel);
            Name = string.IsNullOrWhiteSpace(com_server.ProgId) ? Clsid.ToString() : com_server.ProgId;
            Source = COMRegistryEntrySource.ActCtx;
        }

        public Guid Clsid { get; private set; }

        public string Name { get; private set; }

        public string DefaultServer
        {
            get
            {
                return GetDefaultServer().Server;
            }
        }

        public string DefaultServerName
        {
            get
            {
                return COMUtilities.GetFileName(DefaultServer);
            }
        }

        public string DefaultCmdLine
        {
            get
            {
                return GetDefaultServer().CommandLine;
            }
        }

        public COMServerType DefaultServerType
        {
            get
            {
                return GetDefaultServer().ServerType;
            }
        }

        public Guid TreatAs { get; private set; }

        public Guid AppID { get; private set; }

        public COMAppIDEntry AppIDEntry
        {
            get
            {
                return Database.AppIDs.GetGuidEntry(AppID);
            }
        }

        public bool HasAppID
        {
            get
            {
                return AppIDEntry != null;
            }
        }

        public Guid TypeLib { get; private set; }

        public COMTypeLibEntry TypeLibEntry
        {
            get
            {
                return Database.Typelibs.GetGuidEntry(TypeLib);
            }
        }

        public bool HasTypeLib
        {
            get
            {
                return TypeLib != Guid.Empty;
            }
        }

        public bool CanElevate
        {
            get
            {
                return Elevation != null && Elevation.Enabled;
            }
        }

        public bool AutoElevation
        {
            get
            {
                return CanElevate && Elevation.AutoApproval;
            }
        }

        public COMCLSIDElevationEntry Elevation { get; private set; }

        public IDictionary<COMServerType, COMCLSIDServerEntry> Servers { get; private set; }

        public IEnumerable<Guid> Categories
        {
            get; private set;
        }

        public IEnumerable<COMCategory> CategoryEntries
        {
            get
            {
                return Categories.Select(c => Database.ImplementedCategories.GetGuidEntry(c)).Where(c => c != null);
            }
        }

        public IEnumerable<string> ProgIds
        {
            get
            {
                return ProgIdEntries.Select(p => p.ProgID);
            }
        }

        public IEnumerable<COMProgIDEntry> ProgIdEntries
        {
            get
            {
                return Database.GetProgIdsForClsid(Clsid);
            }
        }

        public COMThreadingModel DefaultThreadingModel
        {
            get
            {
                return GetDefaultServer().ThreadingModel;
            }
        }

        public bool ActivatableFromApp
        {
            get; private set;
        }

        /// <summary>
        /// True if this class is a trusted marshaller.
        /// </summary>
        public bool TrustedMarshaller
        {
            get; private set;
        }

        /// <summary>
        /// True if this class has the trusted marshaller category.
        /// </summary>
        public bool TrustedMarshallerCategory => Categories.Contains(COMCategory.CATID_TrustedMarshaler);

        /// <summary>
        /// True if this class is marked as safe to script.
        /// </summary>
        public bool SafeForScripting
        {
            get
            {
                return Categories.Contains(COMCategory.CATID_SafeForScripting);
            }
        }

        /// <summary>
        /// True if this class is marked as safe to initialize.
        /// </summary>
        public bool SafeForInitializing
        {
            get
            {
                return Categories.Contains(COMCategory.CATID_SafeForInitializing);
            }
        }

        public bool HasLaunchPermission
        {
            get
            {
                var appid = AppIDEntry;
                return appid != null ? appid.HasLaunchPermission : false;
            }
        }

        public bool HasAccessPermission
        {
            get
            {
                var appid = AppIDEntry;
                return appid != null ? appid.HasAccessPermission : false;
            }
        }

        public bool HasPermission
        {
            get
            {
                return HasLaunchPermission || HasAccessPermission;
            }
        }

        public string LaunchPermission
        {
            get
            {
                var appid = AppIDEntry;
                return appid != null ? appid.LaunchPermission : string.Empty;
            }
        }

        public string AccessPermission
        {
            get
            {
                var appid = AppIDEntry;
                return appid != null ? appid.AccessPermission : string.Empty;
            }
        }

        public bool HasRunAs
        {
            get
            {
                var appid = AppIDEntry;
                return appid != null ? appid.HasRunAs : false;
            }
        }

        public string RunAs
        {
            get
            {
                var appid = AppIDEntry;
                return appid != null ? appid.RunAs : string.Empty;
            }
        }

        public string PackageId { get; private set; }

        public COMRegistryEntrySource Source
        {
            get; private set;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMCLSIDEntry right = obj as COMCLSIDEntry;
            if (right == null)
            {
                return false;
            }

            if (Elevation != null)
            {
                if (!Elevation.Equals(right.Elevation))
                {
                    return false;
                }
            }
            else if (right.Elevation != null)
            {
                return false;
            }

            // We don't consider the loaded interfaces.
            return Clsid == right.Clsid && Name == right.Name && TreatAs == right.TreatAs && AppID == right.AppID
                && TypeLib == right.TypeLib && Servers.Values.SequenceEqual(right.Servers.Values)
                && ActivatableFromApp == right.ActivatableFromApp && TrustedMarshaller == right.TrustedMarshaller
                && Source == right.Source && PackageId == right.PackageId;
        }

        public override int GetHashCode()
        {
            return Clsid.GetHashCode() ^ Name.GetSafeHashCode() ^ TreatAs.GetHashCode()
                ^ AppID.GetHashCode() ^ TypeLib.GetHashCode() ^ Servers.Values.GetEnumHashCode()
                ^ Elevation.GetSafeHashCode() ^ ActivatableFromApp.GetHashCode() ^ TrustedMarshaller.GetHashCode()
                ^ Source.GetHashCode() ^ PackageId.GetSafeHashCode();
        }

        private async Task<COMEnumerateInterfaces> GetSupportedInterfacesInternal(NtToken token)
        {
            try
            {
                return await COMEnumerateInterfaces.GetInterfacesOOP(this, Database, token);
            }
            catch (Win32Exception)
            {
                throw;
            }
            catch (AggregateException agg)
            {
                throw agg.InnerException;
            }
        }

        private COMCLSIDServerEntry GetDefaultServer()
        {
            if (Servers.ContainsKey(COMServerType.InProcServer32))
            {
                return Servers[COMServerType.InProcServer32];
            }

            if (Servers.ContainsKey(COMServerType.LocalServer32))
            {
                return Servers[COMServerType.LocalServer32];
            }

            if (Servers.ContainsKey(COMServerType.InProcHandler32))
            {
                return Servers[COMServerType.InProcHandler32];
            }

            return new COMCLSIDServerEntry(COMServerType.UnknownServer);
        }

        /// <summary>
        /// Get list of supported Interface IIDs (that we know about)
        /// NOTE: This will load the object itself to check what is supported, it _might_ crash the app
        /// The returned array is cached so subsequent calls to this function return without calling into COM
        /// </summary>
        /// <param name="refresh">Force the supported interface list to refresh</param>
        /// <param name="token">Token to use when checking for the interfaces.</param>
        /// <returns>Returns true if supported interfaces were refreshed.</returns>
        /// <exception cref="Win32Exception">Thrown on error.</exception>
        public async Task<bool> LoadSupportedInterfacesAsync(bool refresh, NtToken token)
        {
            if (Clsid == Guid.Empty)
            {
                return false;
            }

            if (refresh || !InterfacesLoaded)
            {
                COMEnumerateInterfaces enum_int = await GetSupportedInterfacesInternal(token);
                m_interfaces = new List<COMInterfaceInstance>(enum_int.Interfaces);
                m_factory_interfaces = new List<COMInterfaceInstance>(enum_int.FactoryInterfaces);
                InterfacesLoaded = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get list of supported Interface IIDs Synchronously
        /// </summary>
        /// <param name="refresh">Force the supported interface list to refresh</param>
        /// <param name="token">Token to use when querying for the interfaces.</param>
        /// <returns>Returns true if supported interfaces were refreshed.</returns>
        /// <exception cref="Win32Exception">Thrown on error.</exception>
        public bool LoadSupportedInterfaces(bool refresh, NtToken token)
        {
            Task<bool> result = LoadSupportedInterfacesAsync(refresh, token);
            try
            {
                result.Wait();
                if (result.IsFaulted)
                {
                    throw result.Exception.InnerException;
                }
                return result.Result;
            }
            catch (Exception ex)
            {
                if (ex is AggregateException agg)
                {
                    throw agg.InnerException;
                }
                throw;
            }
        }

        /// <summary>
        /// Indicates that the class' interface list has been loaded.
        /// </summary>
        public bool InterfacesLoaded { get; private set; }

        /// <summary>
        /// Get list of interfaces.
        /// </summary>
        /// <remarks>You must have called LoadSupportedInterfaces before this call to get any useful output.</remarks>
        public IEnumerable<COMInterfaceInstance> Interfaces
        {
            get
            {
                if (InterfacesLoaded)
                {
                    return m_interfaces.AsReadOnly();
                }
                else
                {
                    return new COMInterfaceInstance[0];
                }
            }
        }

        /// <summary>
        /// Get list of factory interfaces.
        /// </summary>
        /// <remarks>You must have called LoadSupportedFactoryInterfaces before this call to get any useful output.</remarks>
        public IEnumerable<COMInterfaceInstance> FactoryInterfaces
        {
            get
            {
                if (InterfacesLoaded)
                {
                    return m_factory_interfaces.AsReadOnly();
                }
                else
                {
                    return new COMInterfaceInstance[0];
                }
            }
        }

        public CLSCTX CreateContext
        {
            get
            {
                CLSCTX dwContext = CLSCTX.ALL;

                if (DefaultServerType == COMServerType.InProcServer32)
                {
                    dwContext = CLSCTX.INPROC_SERVER;
                }
                else if (DefaultServerType == COMServerType.LocalServer32)
                {
                    dwContext = CLSCTX.LOCAL_SERVER;
                }
                else if (DefaultServerType == COMServerType.InProcHandler32)
                {
                    dwContext = CLSCTX.INPROC_HANDLER;
                }

                return dwContext;
            }
        }

        public IntPtr CreateInstance(CLSCTX dwContext, string server)
        {
            IntPtr pInterface = IntPtr.Zero;
            
            if (dwContext == CLSCTX.ALL)
            {
                if (DefaultServerType == COMServerType.InProcServer32)
                {
                    dwContext = CLSCTX.INPROC_SERVER;
                }
                else if (DefaultServerType == COMServerType.LocalServer32)
                {
                    dwContext = CLSCTX.LOCAL_SERVER;
                }
                else if (DefaultServerType == COMServerType.InProcHandler32)
                {
                    dwContext = CLSCTX.INPROC_HANDLER;
                }
                else
                {
                    dwContext = CLSCTX.SERVER;
                }
            }
            
            Guid iid = COMInterfaceEntry.CreateKnownInterface(Database, COMInterfaceEntry.KnownInterfaces.IUnknown).Iid;
            return COMUtilities.CreateInstance(Clsid, iid, dwContext, server);
        }

        public object CreateInstanceAsObject(CLSCTX dwContext, string server)
        {
            IntPtr pObject = CreateInstance(dwContext, server);
            object ret = null;

            if (pObject != IntPtr.Zero)
            {
                ret = Marshal.GetObjectForIUnknown(pObject);
                Marshal.Release(pObject);
            }

            return ret;
        }

        public object CreateClassFactory()
        {
            return CreateClassFactory(CreateContext, null);
        }

        public object CreateClassFactory(CLSCTX dwContext, string server)
        {
            return COMUtilities.CreateClassFactory(Clsid, COMInterfaceEntry.IID_IUnknown, dwContext, server);
        }

        public bool SupportsRemoteActivation { get { return true; } }

        internal COMRegistry Database { get; }

        string ICOMAccessSecurity.DefaultAccessPermission => Database.DefaultAccessPermission;

        string ICOMAccessSecurity.DefaultLaunchPermission => Database.DefaultLaunchPermission;

        Guid IComGuid.ComGuid => Clsid;

        public override string ToString()
        {
            return Name;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Clsid = reader.ReadGuid("clsid");
            AppID = reader.ReadGuid("appid");
            TypeLib = reader.ReadGuid("tlib");
            Categories = reader.ReadGuids("catids");
            TreatAs = reader.ReadGuid("treatas");
            InterfacesLoaded = reader.ReadBool("loaded");
            ActivatableFromApp = reader.ReadBool("activatable");
            TrustedMarshaller = reader.ReadBool("trusted");
            bool elevate = reader.ReadBool("elevate");
            Source = reader.ReadEnum<COMRegistryEntrySource>("src");
            PackageId = reader.ReadString("pkg");
            Name = reader.ReadString("name");
            if (InterfacesLoaded)
            {
                m_interfaces = reader.ReadSerializableObjects("ints", () => new COMInterfaceInstance(Database)).ToList();
                m_factory_interfaces = reader.ReadSerializableObjects("facts", () => new COMInterfaceInstance(Database)).ToList();
            }
            Servers = new ReadOnlyDictionary<COMServerType, COMCLSIDServerEntry>(
                    reader.ReadSerializableObjects("servers", () => new COMCLSIDServerEntry()).ToDictionary(e => e.ServerType));

            if (elevate)
            {
                IEnumerable<COMCLSIDElevationEntry> elevation = 
                    reader.ReadSerializableObjects("elevation", () => new COMCLSIDElevationEntry());
                Elevation = elevation.FirstOrDefault();
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteGuid("clsid", Clsid);
            writer.WriteGuid("appid", AppID);
            writer.WriteGuid("tlib", TypeLib);
            writer.WriteGuids("catids", Categories);
            writer.WriteGuid("treatas", TreatAs);
            writer.WriteBool("loaded", InterfacesLoaded);
            writer.WriteBool("activatable", ActivatableFromApp);
            writer.WriteBool("trusted", TrustedMarshaller);
            writer.WriteBool("elevate", Elevation != null);
            writer.WriteEnum("src", Source);
            writer.WriteOptionalAttributeString("pkg", PackageId);
            writer.WriteOptionalAttributeString("name", Name);
            if (InterfacesLoaded)
            {
                writer.WriteSerializableObjects("ints", m_interfaces);
                writer.WriteSerializableObjects("facts", m_factory_interfaces);
            }
            writer.WriteSerializableObjects("servers", Servers.Values);
            if (Elevation != null)
            {
                writer.WriteSerializableObjects("elevation", new COMCLSIDElevationEntry[] { Elevation });
            }
        }
    }
}
