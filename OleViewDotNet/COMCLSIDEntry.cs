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

namespace OleViewDotNet
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

            return Server == right.Server && CommandLine == right.CommandLine 
                && ServerType == right.ServerType && ThreadingModel == right.ThreadingModel;
        }

        public override int GetHashCode()
        {
            return Server.GetHashCode() ^ CommandLine.GetHashCode() 
                ^ ServerType.GetHashCode() ^ ThreadingModel.GetHashCode();
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

                        if (File.Exists(name) || (Path.GetExtension(name) == String.Empty && File.Exists(name + ".exe")))
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
            string threading_model = key.GetValue("ThreadingModel") as string;
            if (threading_model != null)
            {
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
                }
            }
            return COMThreadingModel.Apartment;
        }

        internal COMCLSIDServerEntry(COMServerType server_type)
        {
            Server = String.Empty;
            CommandLine = String.Empty;
            ServerType = server_type;
            ThreadingModel = COMThreadingModel.Apartment;
        }

        internal COMCLSIDServerEntry() : this(COMServerType.UnknownServer)
        {
        }

        internal COMCLSIDServerEntry(RegistryKey key, COMServerType server_type)
            : this(server_type)
        {
            string server_string = key.GetValue(null) as string;

            if (String.IsNullOrWhiteSpace(server_string))
            {
                // TODO: Support weird .NET registration which registers a .NET version string.
                return;
            }

            bool process_command_line = false;
            CommandLine = String.Empty;

            if (server_type == COMServerType.LocalServer32)
            {
                string executable = key.GetValue("ServerExecutable") as string;
                if (executable != null)
                {
                    CommandLine = server_string;
                    server_string = executable;
                }
                else
                {
                    process_command_line = true;
                }
                ThreadingModel = COMThreadingModel.Both;
            }
            else if (server_type == COMServerType.InProcServer32)
            {
                ThreadingModel = ReadThreadingModel(key);
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
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteOptionalAttributeString("server", Server);
            writer.WriteOptionalAttributeString("cmdline", CommandLine);
            writer.WriteEnum("type", ServerType);
            writer.WriteEnum("model", ThreadingModel);
        }
    }

    public class COMCLSIDEntry : IComparable<COMCLSIDEntry>, IXmlSerializable
    {
        private List<COMInterfaceInstance> m_interfaces;
        private List<COMInterfaceInstance> m_factory_interfaces;
        private bool m_loaded_interfaces;

        private static Guid ControlCategory = new Guid("{40FC6ED4-2438-11CF-A3DB-080036F12502}");
        private static Guid InsertableCategory = new Guid("{40FC6ED3-2438-11CF-A3DB-080036F12502}");
        private static Guid DocumentCategory = new Guid("{40fc6ed8-2438-11cf-a3db-080036f12502}");

        public int CompareTo(COMCLSIDEntry right)
        {
            return String.Compare(Name, right.Name);
        }
        
        private void ReadServerKey(Dictionary<COMServerType, COMCLSIDServerEntry> servers, RegistryKey key, COMServerType server_type)
        {
            using (RegistryKey server_key = key.OpenSubKey(server_type.ToString()))
            {
                if (server_key == null)
                {
                    return;
                }

                COMCLSIDServerEntry entry = new COMCLSIDServerEntry(server_key, server_type);
                if (!String.IsNullOrWhiteSpace(entry.Server))
                {
                    servers.Add(server_type, new COMCLSIDServerEntry(server_key, server_type));
                }
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

            if (Name == null)
            {
                Name = Clsid.ToString("B");
            }

            Dictionary<COMServerType, COMCLSIDServerEntry> servers = new Dictionary<COMServerType, COMCLSIDServerEntry>();
            ReadServerKey(servers, key, COMServerType.InProcServer32);
            ReadServerKey(servers, key, COMServerType.LocalServer32);
            ReadServerKey(servers, key, COMServerType.InProcHandler32);
            Servers = new ReadOnlyDictionary<COMServerType, COMCLSIDServerEntry>(servers);

            AppID = COMUtilities.ReadGuidFromKey(key, null, "AppID");
            if (AppID == Guid.Empty)
            {
                AppID = COMUtilities.ReadGuidFromKey(Registry.ClassesRoot,
                        String.Format(@"AppID\{0}", Path.GetFileName(DefaultServer)), "AppID");
            }

            TypeLib = COMUtilities.ReadGuidFromKey(key, "TypeLib", null);
            if (key.HasSubkey("Control"))
            {
                categories.Add(ControlCategory);
            }

            if (key.HasSubkey("Insertable"))
            {
                categories.Add(InsertableCategory);
            }

            if (key.HasSubkey("DocObject"))
            {
                categories.Add(DocumentCategory);
            }

            using (RegistryKey catkey = key.OpenSubKey("Implemented Categories"))
            {
                if (catkey != null)
                {
                    string[] subKeys = catkey.GetSubKeyNames();
                    foreach (string s in subKeys)
                    {
                        Guid g;

                        if (Guid.TryParse(s, out g))
                        {
                            categories.Add(g);
                        }
                    }
                }
            }

            Categories = categories.ToList().AsReadOnly();
            TreatAs = COMUtilities.ReadGuidFromKey(key, "TreatAs", null);
        }

        public COMCLSIDEntry(Guid clsid, RegistryKey rootKey) : this(clsid)
        {
            LoadFromKey(rootKey);
        }

        private COMCLSIDEntry(Guid clsid)
        {
            Clsid = clsid;
            Servers = new Dictionary<COMServerType, COMCLSIDServerEntry>();
        }

        public COMCLSIDEntry(Guid clsid, COMServerType type)
            : this(clsid)
        {
        }

        public Guid Clsid { get; private set; }

        public string Name { get; private set; }

        public string DefaultServer {
            get
            {
                return GetDefaultServer().Server;
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

        public Guid TypeLib { get; private set; }

        public IDictionary<COMServerType, COMCLSIDServerEntry> Servers { get; private set; }

        public IEnumerable<Guid> Categories
        {
            get; private set;
        }

        public COMThreadingModel DefaultThreadingModel
        {
            get
            {
                return GetDefaultServer().ThreadingModel;
            }
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

            // We don't consider the loaded interfaces.
            return Clsid == right.Clsid && Name == right.Name && TreatAs == right.TreatAs && AppID == right.AppID 
                && TypeLib == right.TypeLib && Servers.Values.SequenceEqual(right.Servers.Values);
        }

        public override int GetHashCode()
        {
            return Clsid.GetHashCode() ^ Name.GetSafeHashCode() ^ TreatAs.GetHashCode() 
                ^ AppID.GetHashCode() ^ TypeLib.GetHashCode() ^ Servers.Values.GetEnumHashCode();
        }

        private async Task<COMEnumerateInterfaces> GetSupportedInterfacesInternal()
        {
            try
            {
                return await COMEnumerateInterfaces.GetInterfacesOOP(this);
            }
            catch (Win32Exception)
            {
                throw;
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

            return new COMCLSIDServerEntry(AppID != Guid.Empty 
                ? COMServerType.LocalServer32 : COMServerType.UnknownServer);
        }

        /// <summary>
        /// Get list of supported Interface IIDs (that we know about)
        /// NOTE: This will load the object itself to check what is supported, it _might_ crash the app
        /// The returned array is cached so subsequent calls to this function return without calling into COM
        /// </summary>        
        /// <param name="refresh">Force the supported interface list to refresh</param>
        /// <returns>Returns true if supported interfaces were refreshed.</returns>
        /// <exception cref="Win32Exception">Thrown on error.</exception>
        public async Task<bool> LoadSupportedInterfacesAsync(bool refresh)
        {
            if (refresh || !m_loaded_interfaces)
            {
                COMEnumerateInterfaces enum_int = await GetSupportedInterfacesInternal();
                m_interfaces = new List<COMInterfaceInstance>(enum_int.Interfaces);
                m_factory_interfaces = new List<COMInterfaceInstance>(enum_int.FactoryInterfaces);
                m_loaded_interfaces = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get list of supported Interface IIDs Synchronously
        /// </summary>        
        /// <param name="refresh">Force the supported interface list to refresh</param>
        /// <returns>Returns true if supported interfaces were refreshed.</returns>
        /// <exception cref="Win32Exception">Thrown on error.</exception>
        public bool LoadSupportedInterfaces(bool refresh)
        {
            Task<bool> result = LoadSupportedInterfacesAsync(refresh);
            result.Wait();
            if (result.IsFaulted)
            {
                throw result.Exception.InnerException;
            }
            return result.Result;
        }

        /// <summary>
        /// Get list of interfaces.
        /// </summary>
        /// <remarks>You must have called LoadSupportedInterfaces before this call to get any useful output.</remarks>
        public IEnumerable<COMInterfaceInstance> Interfaces
        {
            get
            {
                if (m_loaded_interfaces)
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
                if (m_loaded_interfaces)
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
                CLSCTX dwContext = CLSCTX.CLSCTX_ALL;

                if (DefaultServerType == COMServerType.InProcServer32)
                {
                    dwContext = CLSCTX.CLSCTX_INPROC_SERVER;
                }
                else if (DefaultServerType == COMServerType.LocalServer32)
                {
                    dwContext = CLSCTX.CLSCTX_LOCAL_SERVER;
                }
                else if (DefaultServerType == COMServerType.InProcHandler32)
                {
                    dwContext = CLSCTX.CLSCTX_INPROC_HANDLER;
                }

                return dwContext;
            }           
        }

        public IntPtr CreateInstance(CLSCTX dwContext)
        {
            IntPtr pInterface = IntPtr.Zero;
            
            if (dwContext == CLSCTX.CLSCTX_ALL)
            {
                if (DefaultServerType == COMServerType.InProcServer32)
                {
                    dwContext = CLSCTX.CLSCTX_INPROC_SERVER;
                }
                else if (DefaultServerType == COMServerType.LocalServer32)
                {
                    dwContext = CLSCTX.CLSCTX_LOCAL_SERVER;
                }
                else
                {
                    dwContext = CLSCTX.CLSCTX_SERVER;
                }
            }
            
            Guid iid = COMInterfaceEntry.CreateKnownInterface(COMInterfaceEntry.KnownInterfaces.IUnknown).Iid;
            Guid clsid = Clsid;
            int iError = COMUtilities.CoCreateInstance(ref clsid, IntPtr.Zero, dwContext, ref iid, out pInterface);

            if (iError != 0)
            {
                Marshal.ThrowExceptionForHR(iError);
            }

            return pInterface;
        }

        public object CreateInstanceAsObject(CLSCTX dwContext)
        {
            IntPtr pObject = CreateInstance(dwContext);
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
            IntPtr obj;
            Guid iid = COMInterfaceEntry.IID_IUnknown;
            Guid clsid = Clsid;

            int hr = COMUtilities.CoGetClassObject(ref clsid, CreateContext, IntPtr.Zero, ref iid, out obj);
            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            object ret = Marshal.GetObjectForIUnknown(obj);
            Marshal.Release(obj);
            return ret;
        }

        public override string ToString()
        {
            return String.Format("COMCLSIDEntry: {0}", Name);
        }

        internal COMCLSIDEntry()
        {
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
            m_loaded_interfaces = reader.ReadBool("loaded");
            Name = reader.ReadString("name");
            if (m_loaded_interfaces)
            {
                m_interfaces = reader.ReadSerializableObjects("ints", () => new COMInterfaceInstance()).ToList();
                m_factory_interfaces = reader.ReadSerializableObjects("facts", () => new COMInterfaceInstance()).ToList();
            }
            Servers = new ReadOnlyDictionary<COMServerType, COMCLSIDServerEntry>(
                    reader.ReadSerializableObjects("servers", () => new COMCLSIDServerEntry()).ToDictionary(e => e.ServerType));
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteGuid("clsid", Clsid);
            writer.WriteGuid("appid", AppID);
            writer.WriteGuid("tlib", TypeLib);
            writer.WriteGuids("catids", Categories);
            writer.WriteGuid("treatas", TreatAs);
            writer.WriteBool("loaded", m_loaded_interfaces);
            writer.WriteAttributeString("name", Name);
            if (m_loaded_interfaces)
            {
                writer.WriteSerializableObjects("ints", m_interfaces);
                writer.WriteSerializableObjects("facts", m_factory_interfaces);
            }
            writer.WriteSerializableObjects("servers", Servers.Values);
        }
    }
}
