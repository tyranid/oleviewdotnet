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
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Xml;

namespace OleViewDotNet
{
    public enum COMRegistryMode
    {
        Merged,
        MachineOnly,
        UserOnly,
        Diff,
    }

    public enum COMRegistryDiffMode
    {
        LeftOnly,
        RightOnly,        
    }

    /// <summary>
    /// Class to hold information about the current COM registration information
    /// </summary>
    public class COMRegistry
    {
        #region Private Member Variables

        private string m_name;

        // These are loaded from the registry.
        private SortedDictionary<Guid, COMCLSIDEntry> m_clsids;        
        private SortedDictionary<Guid, COMInterfaceEntry> m_interfaces;
        private SortedDictionary<string, COMProgIDEntry> m_progids;
        private SortedDictionary<Guid, COMCategory> m_categories;
        private List<COMIELowRightsElevationPolicy> m_lowrights;
        private SortedDictionary<Guid, COMAppIDEntry> m_appid;
        private SortedDictionary<Guid, COMTypeLibEntry> m_typelibs;
        private List<COMMimeType> m_mimetypes;
        private List<Guid> m_preapproved;

        // These are built on demand, just different views.
        private COMCLSIDEntry[] m_clsidbyname;
        private COMInterfaceEntry[] m_interfacebyname;
        private SortedDictionary<string, List<COMCLSIDEntry>> m_clsidbyserver;
        private SortedDictionary<string, List<COMCLSIDEntry>> m_clsidbylocalserver;
        private SortedDictionary<string, List<COMCLSIDEntry>> m_clsidwithsurrogate;
        private Dictionary<Guid, List<COMProgIDEntry>> m_progidsbyclsid;
        private Dictionary<Guid, IEnumerable<COMInterfaceEntry>> m_proxiesbyclsid;
        private Dictionary<Guid, string> m_iids_to_names;

        #endregion

        #region Public Properties

        public SortedDictionary<Guid, COMCLSIDEntry> Clsids
        {
            get 
            {
                return m_clsids; 
            }
        }

        public SortedDictionary<Guid, COMInterfaceEntry> Interfaces
        {
            get 
            {
                return m_interfaces; 
            }
        }

        public SortedDictionary<string, COMProgIDEntry> Progids
        {
            get 
            {
                return m_progids; 
            }
        }

        private SortedDictionary<string, List<COMCLSIDEntry>> GetClsidsByString(Func<COMCLSIDEntry, bool> filter, Func<COMCLSIDEntry, string> key_selector)
        {
            var grouping = m_clsids.Values.Where(filter).GroupBy(key_selector, StringComparer.OrdinalIgnoreCase);
            return new SortedDictionary<string, List<COMCLSIDEntry>>(grouping.ToDictionary(e => e.Key, e => e.ToList(),
                StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, List<COMCLSIDEntry>> ClsidsByServer
        {
            get 
            {
                if (m_clsidbyserver == null)
                {
                    m_clsidbyserver = GetClsidsByString(e => !String.IsNullOrWhiteSpace(e.Server) && e.ServerType != COMServerType.UnknownServer,
                        e => e.Server);
                }

                return m_clsidbyserver;
            }
        }

        public IDictionary<string, List<COMCLSIDEntry>> ClsidsByLocalServer
        {
            get
            {
                if (m_clsidbylocalserver == null)
                {
                    m_clsidbylocalserver = GetClsidsByString(e => !String.IsNullOrWhiteSpace(e.Server) && e.ServerType == COMServerType.LocalServer32,
                        e => e.Server);
                }

                return m_clsidbylocalserver;
            }
        }

        public IDictionary<string, List<COMCLSIDEntry>> ClsidsWithSurrogate
        {
            get
            {
                if (m_clsidwithsurrogate == null)
                {
                    m_clsidwithsurrogate = GetClsidsByString(e => m_appid.ContainsKey(e.AppID) && !String.IsNullOrWhiteSpace(m_appid[e.AppID].DllSurrogate),
                                                             e => m_appid[e.AppID].DllSurrogate);
                }
                return m_clsidwithsurrogate;
            }
        }

        public IEnumerable<COMCLSIDEntry> ClsidsByName
        {
            get 
            {
                if (m_clsidbyname == null)
                {
                    m_clsidbyname = m_clsids.Values.OrderBy(e => e.Name).ToArray();
                }

                return m_clsidbyname; 
            }
        }

        public IEnumerable<COMInterfaceEntry> InterfacesByName
        {
            get
            {
                if (m_interfacebyname == null)
                {
                    m_interfacebyname = m_interfaces.Values.OrderBy(i => i.Name).ToArray();
                }

                return m_interfacebyname;
            }
        }

        public IDictionary<Guid, string> InterfacesToNames
        {
            get
            {
                if (m_iids_to_names == null)
                {
                    m_iids_to_names = m_interfaces.ToDictionary(p => p.Key, p => p.Value.Name);
                }
                return m_iids_to_names;
            }
        }

        public IDictionary<Guid, COMCategory> ImplementedCategories
        {
            get { return m_categories; }
        }

        public IEnumerable<COMCLSIDEntry> PreApproved
        {
            get { return m_preapproved.Select(g => MapClsidToEntry(g)).Where(e => e != null); }
        }

        public COMIELowRightsElevationPolicy[] LowRights
        {
            get { return m_lowrights.ToArray(); }
        }

        public IDictionary<Guid, COMAppIDEntry> AppIDs
        {
            get { return m_appid; }
        }

        public IEnumerable<IGrouping<Guid, COMCLSIDEntry>> ClsidsByAppId
        {
            get
            {
                return m_clsids.Values.Where(c => c.AppID != Guid.Empty).GroupBy(c => c.AppID);
            }
        }

        public IDictionary<Guid, COMTypeLibEntry> Typelibs
        {
            get { return m_typelibs; }
        }

        public IDictionary<Guid, IEnumerable<COMInterfaceEntry>> ProxiesByClsid
        {
            get
            {
                if (m_proxiesbyclsid == null)
                {
                    m_proxiesbyclsid = m_interfaces.Values.Where(i => i.ProxyClsid != Guid.Empty).GroupBy(i => i.ProxyClsid).ToDictionary(e => e.Key, e => e.AsEnumerable());
                }
                return m_proxiesbyclsid;
            }
        }

        public IEnumerable<COMMimeType> MimeTypes
        {
            get { return m_mimetypes; }
        }

        public string CreatedDate
        {
            get; private set;
        }

        public string CreatedMachine
        {
            get; private set; 
        }

        public COMRegistryMode LoadingMode
        {
            get; private set;
        }

        public string CreatedUser
        {
            get; private set;
        }

        public bool SixtyFourBit
        {
            get; private set; 
        }

        public string FilePath
        {
            get; private set; 
        }

        public string Name
        {
            get
            {
                return m_name;
            }

            set
            {
                m_name = value ?? String.Empty;
            }
        }

        #endregion

        #region Public Methods

        private class DummyProgress : IProgress<string>
        {
            public void Report(string data)
            {
            }
        }

        public static COMRegistry Load(COMRegistryMode mode, SecurityIdentifier user, IProgress<string> progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            if (user == null)
            {
                using (WindowsIdentity id = WindowsIdentity.GetCurrent())
                {
                    user = id.User;
                }
            }

            return new COMRegistry(mode, user, progress);
        }

        public static COMRegistry Load(COMRegistryMode mode)
        {
            return Load(mode, null, new DummyProgress());
        }

        public void Save(string path)
        {
            Save(path, new DummyProgress());
        }

        public void Save(string path, IProgress<string> progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            using (XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.WriteStartElement("comregistry");                
                writer.WriteAttributeString("created", CreatedDate);
                writer.WriteAttributeString("machine", CreatedMachine);
                writer.WriteBool("sixfour", SixtyFourBit);
                writer.WriteEnum("mode", LoadingMode);
                writer.WriteAttributeString("user", CreatedUser);
                progress.Report("CLSIDs");
                writer.WriteSerializableObjects("clsids", m_clsids.Values);
                progress.Report("ProgIDs");
                writer.WriteSerializableObjects("progids", m_progids.Values);
                progress.Report("MIME Types");
                writer.WriteSerializableObjects("mimetypes", m_mimetypes);
                progress.Report("AppIDs");
                writer.WriteSerializableObjects("appids", m_appid.Values);
                progress.Report("Interfaces");
                writer.WriteSerializableObjects("intfs", m_interfaces.Values);
                progress.Report("Categories");
                writer.WriteSerializableObjects("catids", m_categories.Values);
                progress.Report("LowRights");
                writer.WriteSerializableObjects("lowies", m_lowrights);
                progress.Report("TypeLibs");
                writer.WriteSerializableObjects("typelibs", m_typelibs.Values);
                progress.Report("PreApproved");
                writer.WriteStartElement("preapp");
                writer.WriteGuids("clsids", m_preapproved);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        public static COMRegistry Load(string path, IProgress<string> progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            return new COMRegistry(path, progress);   
        }

        public static COMRegistry Load(string path)
        {
            return Load(path, new DummyProgress());
        }

        static IEnumerable<T> DiffLists<T>(IEnumerable<T> left, IEnumerable<T> right, COMRegistryDiffMode mode)
        {
            switch (mode)
            {
                case COMRegistryDiffMode.LeftOnly:
                    return left.Except(right);
                case COMRegistryDiffMode.RightOnly:
                    return right.Except(left);
                default:
                    throw new ArgumentException(nameof(mode));
            }
        }

        static SortedDictionary<S, T> DiffDicts<S, T>(IDictionary<S, T> left, IDictionary<S, T> right, COMRegistryDiffMode mode, Func<T, S> key_selector)
        {
            return DiffLists(left.Values, right.Values, mode).ToSortedDictionary(key_selector);
        }

        public static COMRegistry Diff(COMRegistry left, COMRegistry right, COMRegistryDiffMode mode, IProgress<string> progress)
        {
            COMRegistry ret = new COMRegistry(COMRegistryMode.Diff);
            progress.Report("CLSIDs");
            ret.m_clsids = DiffDicts(left.m_clsids, right.m_clsids, mode, p => p.Clsid);
            progress.Report("ProgIDs");
            ret.m_progids = DiffDicts(left.m_progids, right.m_progids, mode, p => p.ProgID);
            progress.Report("MIME Types");
            ret.m_mimetypes = DiffLists(left.m_mimetypes, right.m_mimetypes, mode).ToList();
            progress.Report("AppIDs");
            ret.m_appid = DiffDicts(left.m_appid, right.m_appid, mode, p => p.AppId);
            progress.Report("Interfaces");
            ret.m_interfaces = DiffDicts(left.m_interfaces, right.m_interfaces, mode, p => p.Iid);
            progress.Report("Categories");
            ret.m_categories = DiffDicts(left.m_categories, right.m_categories, mode, p => p.CategoryID);
            progress.Report("LowRights");
            ret.m_lowrights = DiffLists(left.m_lowrights, right.m_lowrights, mode).ToList();
            progress.Report("TypeLibs");
            ret.m_typelibs = DiffDicts(left.m_typelibs, right.m_typelibs, mode, p => p.TypelibId);
            progress.Report("PreApproved");
            ret.m_preapproved = DiffLists(left.m_preapproved, right.m_preapproved, mode).ToList();
            return ret;
        }

        /// <summary>
        /// Get the list of supported interfaces from an IUnknown pointer
        /// </summary>
        /// <param name="pObject">The IUnknown pointer</param>
        /// <returns>List of interfaces supported</returns>
        public IEnumerable<COMInterfaceEntry> GetInterfacesForIUnknown(IntPtr pObject)
        {
            List<COMInterfaceEntry> ents = new List<COMInterfaceEntry>();
            foreach (COMInterfaceEntry intEnt in Interfaces.Values)
            {
                Guid currIID = intEnt.Iid;
                IntPtr pRequested;

                if (Marshal.QueryInterface(pObject, ref currIID, out pRequested) == 0)
                {
                    Marshal.Release(pRequested);
                    ents.Add(intEnt);
                }
            }
            return ents.OrderBy(i => i.Name);
        }

        /// <summary>
        /// Get list of supported interfaces for a COM wrapper
        /// </summary>
        /// <param name="obj">COM Wrapper Object</param>
        /// <returns>List of interfaces supported</returns>
        public COMInterfaceEntry[] GetInterfacesForObject(object obj)
        {
            COMInterfaceEntry[] ret;

            IntPtr pObject = Marshal.GetIUnknownForObject(obj);
            ret = GetInterfacesForIUnknown(pObject).ToArray();
            Marshal.Release(pObject);

            return ret;
        }

        public COMInterfaceEntry[] GetProxiesForClsid(COMCLSIDEntry clsid)
        {
            if (ProxiesByClsid.ContainsKey(clsid.Clsid))
            {
                return ProxiesByClsid[clsid.Clsid].ToArray();
            }
            else
            {
                return new COMInterfaceEntry[0];
            }
        }

        /// <summary>
        /// Map an IID to an interface object.
        /// </summary>
        /// <param name="iid">The interface to map.</param>
        /// <returns>The mapped interface.</returns>
        public COMInterfaceEntry MapIidToInterface(Guid iid)
        {
            if (m_interfaces.ContainsKey(iid))
            {
                return m_interfaces[iid];
            }
            else
            {
                return new COMInterfaceEntry(iid);
            }
        }

        /// <summary>
        /// Map a CLSID to an object.
        /// </summary>
        /// <param name="clsid">The CLSID to map.</param>
        /// <returns>The object or null if not available.</returns>
        public COMCLSIDEntry MapClsidToEntry(Guid clsid)
        {
            if (m_clsids.ContainsKey(clsid))
            {
                return m_clsids[clsid];
            }

            if (clsid == Guid.Empty)
            {
                return null;
            }

            return new COMCLSIDEntry(clsid, COMServerType.UnknownServer);
        }

        public COMTypeLibVersionEntry GetTypeLibVersionEntry(Guid typelib, string version)
        {
            if (!m_typelibs.ContainsKey(typelib))
            {
                return null;
            }

            COMTypeLibEntry entry = m_typelibs[typelib];
            foreach (var ver in entry.Versions)
            {
                if (ver.Version == version)
                {
                    return ver;
                }
            }

            return null;
        }

        public COMCLSIDEntry GetFileClass(string filename)
        {
            Guid clsid;
            COMUtilities.GetClassFile(filename, out clsid);
            return MapClsidToEntry(clsid);
        }
        
        public IEnumerable<COMProgIDEntry> GetProgIdsForClsid(Guid clsid)
        {
            if (m_progidsbyclsid == null)
            {
                m_progidsbyclsid = m_progids.Values.GroupBy(p => p.Clsid).ToDictionary(g => g.Key, g => g.ToList());
            }

            if (m_progidsbyclsid.ContainsKey(clsid))
            {
                return m_progidsbyclsid[clsid].AsReadOnly();
            }
            else
            {
                return new COMProgIDEntry[0];
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(m_name))
            {
                builder.AppendFormat("{0} - ", m_name);
            }

            builder.AppendFormat("Created: {0}", CreatedDate);
            if (!String.IsNullOrWhiteSpace(FilePath))
            {
                builder.AppendFormat(" Path: {0}", FilePath);
            }
            return builder.ToString();
        }

        #endregion

        #region Private Methods

        private static RegistryKey OpenClassesKey(COMRegistryMode mode, SecurityIdentifier user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            switch (mode)
            {
                case COMRegistryMode.Merged:
                    return Registry.ClassesRoot;
                case COMRegistryMode.MachineOnly:
                    return Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes");
                case COMRegistryMode.UserOnly:
                    return Registry.Users.OpenSubKey(String.Format(@"{0}\SOFTWARE\Classes", user.Value));
                default:
                    throw new ArgumentException("Invalid mode", "mode");
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        private COMRegistry(COMRegistryMode mode, SecurityIdentifier user, IProgress<string> progress) 
            : this(mode)
        {
            using (RegistryKey classes_key = OpenClassesKey(mode, user))
            {
                progress.Report("AppIDs");
                LoadAppIDs(classes_key);
                progress.Report("CLSIDs");
                LoadCLSIDs(classes_key);
                progress.Report("ProgIDs");
                LoadProgIDs(classes_key);
                progress.Report("Interfaces");
                LoadInterfaces(classes_key);
                progress.Report("MIME Types");
                LoadMimeTypes(classes_key);
                progress.Report("PreApproved");
                LoadPreApproved(mode, user);
                progress.Report("LowRights");
                LoadLowRights(mode, user);
                progress.Report("TypeLibs");
                LoadTypelibs(classes_key);
            }

            try
            {
                NTAccount account = (NTAccount)user.Translate(typeof(NTAccount));
                CreatedUser = account.Value;
            }
            catch
            {
                CreatedUser = user.Value;
            }
        }

        private COMRegistry(string path, IProgress<string> progress)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Prohibit;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            using (XmlReader reader = XmlReader.Create(path, settings))
            {
                if (!reader.IsStartElement("comregistry"))
                {
                    throw new XmlException("Invalid root node");
                }

                CreatedDate = reader.GetAttribute("created");
                CreatedMachine = reader.GetAttribute("machine");
                SixtyFourBit = reader.ReadBool("sixfour");
                LoadingMode = reader.ReadEnum<COMRegistryMode>("mode");
                CreatedUser = reader.GetAttribute("user");
                progress.Report("CLSIDs");
                m_clsids = reader.ReadSerializableObjects("clsids", () => new COMCLSIDEntry()).ToSortedDictionary(p => p.Clsid);
                progress.Report("ProgIDs");
                m_progids = reader.ReadSerializableObjects("progids", () => new COMProgIDEntry()).ToSortedDictionary(p => p.ProgID);
                progress.Report("MIME Types");
                m_mimetypes = reader.ReadSerializableObjects("mimetypes", () => new COMMimeType()).ToList();
                progress.Report("AppIDs");
                m_appid = reader.ReadSerializableObjects("appids", () => new COMAppIDEntry()).ToSortedDictionary(p => p.AppId);
                progress.Report("Interfaces");
                m_interfaces = reader.ReadSerializableObjects("intfs", () => new COMInterfaceEntry()).ToSortedDictionary(p => p.Iid);
                progress.Report("Categories");
                m_categories = reader.ReadSerializableObjects("catids", () => new COMCategory()).ToSortedDictionary(p => p.CategoryID);
                progress.Report("LowRights");
                m_lowrights = reader.ReadSerializableObjects("lowies", () => new COMIELowRightsElevationPolicy()).ToList();
                progress.Report("TypeLibs");
                m_typelibs = reader.ReadSerializableObjects("typelibs", () => new COMTypeLibEntry()).ToSortedDictionary(p => p.TypelibId);
                progress.Report("PreApproved");
                if (reader.IsStartElement("preapp"))
                {
                    m_preapproved = reader.ReadGuids("clsids").ToList();
                    reader.Read();
                }
                reader.ReadEndElement();
            }
            FilePath = path;
        }

        private COMRegistry(COMRegistryMode mode)
        {
            LoadingMode = mode;
            CreatedDate = DateTime.Now.ToLongDateString();
            CreatedMachine = Environment.MachineName;
            SixtyFourBit = Environment.Is64BitProcess;
        }

        private static void AddEntryToDictionary(Dictionary<string, List<COMCLSIDEntry>> dict, COMCLSIDEntry entry)
        {
            List<COMCLSIDEntry> list = null;
            string strServer = entry.Server.ToLower();
            if (dict.ContainsKey(strServer))
            {
                list = dict[strServer];
            }
            else
            {
                list = new List<COMCLSIDEntry>();
                dict[strServer] = list;
            }
            list.Add(entry);
        }

        /// <summary>
        /// Load CLSID information from the registry key
        /// </summary>
        /// <param name="rootKey">The root registry key, e.g. HKEY_CLASSES_ROOT</param>
        private void LoadCLSIDs(RegistryKey rootKey)
        {
            Dictionary<Guid, COMCLSIDEntry> clsids = new Dictionary<Guid, COMCLSIDEntry>();
            Dictionary<Guid, List<Guid>> categories = new Dictionary<Guid, List<Guid>>();

            using (RegistryKey clsidKey = rootKey.OpenSubKey("CLSID"))
            {
                if (clsidKey != null)
                {
                    string[] subkeys = clsidKey.GetSubKeyNames();
                    foreach (string key in subkeys)
                    {              
                        Guid clsid;

                        if(Guid.TryParse(key, out clsid))
                        {
                            if (!clsids.ContainsKey(clsid))
                            {
                                using (RegistryKey regKey = clsidKey.OpenSubKey(key))
                                {
                                    if (regKey != null)
                                    {
                                        COMCLSIDEntry ent = new COMCLSIDEntry(clsid, regKey);
                                        clsids.Add(clsid, ent);
                                        foreach (Guid catid in ent.Categories)
                                        {
                                            if (!categories.ContainsKey(catid))
                                            {
                                                categories[catid] = new List<Guid>();
                                            }
                                            categories[catid].Add(ent.Clsid);
                                        }
                                    }
                                }
                            }
                        }
                    }                    
                }
            }
            
            m_clsids = new SortedDictionary<Guid, COMCLSIDEntry>(clsids);
            m_categories = categories.ToSortedDictionary(p => p.Key, p => new COMCategory(p.Key, p.Value));
        }

        private void LoadProgIDs(RegistryKey rootKey)
        {
            m_progids = new SortedDictionary<string, COMProgIDEntry>();
            string[] subkeys = rootKey.GetSubKeyNames();
            foreach (string key in subkeys)
            {
                try
                {
                    using (RegistryKey regKey = rootKey.OpenSubKey(key))
                    {
                        Guid clsid = COMUtilities.ReadGuidFromKey(regKey, "CLSID", null);
                        if (clsid != Guid.Empty)
                        {
                            COMProgIDEntry entry = new COMProgIDEntry(key, clsid, regKey);
                            m_progids.Add(key, entry);
                        }
                    }
                }
                catch (FormatException)
                {                    
                }
            }
        }

        /// <summary>
        /// Load interface list from registry
        /// </summary>
        /// <param name="rootKey">Root key of registry</param>
        private void LoadInterfaces(RegistryKey rootKey)
        {
            Dictionary<Guid, COMInterfaceEntry> interfaces = new Dictionary<Guid, COMInterfaceEntry>();
            COMInterfaceEntry unk = COMInterfaceEntry.CreateKnownInterface(COMInterfaceEntry.KnownInterfaces.IUnknown);
            interfaces.Add(unk.Iid, unk);
            unk = COMInterfaceEntry.CreateKnownInterface(COMInterfaceEntry.KnownInterfaces.IMarshal);
            interfaces.Add(unk.Iid, unk);
            using (RegistryKey iidKey = rootKey.OpenSubKey("Interface"))
            {
                if (iidKey != null)
                {
                    string[] subkeys = iidKey.GetSubKeyNames();
                    foreach (string key in subkeys)
                    {
                        Guid iid;

                        if (Guid.TryParse(key, out iid))
                        {
                            if (!interfaces.ContainsKey(iid))
                            {
                                using (RegistryKey regKey = iidKey.OpenSubKey(key))
                                {
                                    if (regKey != null)
                                    {
                                        COMInterfaceEntry ent = new COMInterfaceEntry(iid, regKey);
                                        interfaces.Add(iid, ent);
                                    }
                                }
                            }
                        }

                    }
                }
            }

            m_interfaces = new SortedDictionary<Guid, COMInterfaceEntry>(interfaces);
        }

        IEnumerable<Guid> ReadPreApproved(RegistryKey rootKey)
        {
            List<Guid> ret = new List<Guid>();
            using (RegistryKey key = rootKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Ext\\PreApproved"))
            {
                if (key != null)
                {
                    string[] subkeys = key.GetSubKeyNames();
                    foreach (string s in subkeys)
                    {
                        Guid g;

                        if(Guid.TryParse(s, out g))
                        {
                            ret.Add(g);                            
                        }
                    }
                }
            }
            return ret;
        }

        void LoadPreApproved(COMRegistryMode mode, SecurityIdentifier user)
        {
            m_preapproved = new List<Guid>();
            if (mode == COMRegistryMode.Merged || mode == COMRegistryMode.MachineOnly)
            {
                m_preapproved.AddRange(ReadPreApproved(Registry.LocalMachine));
            }

            if (mode == COMRegistryMode.Merged || mode == COMRegistryMode.UserOnly)
            {
                using (RegistryKey key = Registry.Users.OpenSubKey(user.Value))
                {
                    if (key != null)
                    {
                        m_preapproved.AddRange(ReadPreApproved(key));
                    }
                }
            }
        }

        void LoadTypelibs(RegistryKey rootKey)
        {
            Dictionary<Guid, COMTypeLibEntry> typelibs = new Dictionary<Guid, COMTypeLibEntry>();

            using (RegistryKey key = rootKey.OpenSubKey("TypeLib"))
            {
                if (key != null)
                {
                    string[] subkeys = key.GetSubKeyNames();
                    foreach (string s in subkeys)
                    {
                        Guid g;

                        if (Guid.TryParse(s, out g))
                        {
                            using (RegistryKey subKey = key.OpenSubKey(s))
                            {
                                if (subKey != null)
                                {
                                    COMTypeLibEntry typelib = new COMTypeLibEntry(g, subKey);

                                    typelibs[g] = typelib;
                                }
                            }
                        }
                    }
                }
            }

            m_typelibs = new SortedDictionary<Guid, COMTypeLibEntry>(typelibs);
        }

        private void LoadLowRightsKey(RegistryKey rootKey)
        {
            using (RegistryKey key = rootKey.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Low Rights\ElevationPolicy"))
            {
                if (key != null)
                {
                    string[] subkeys = key.GetSubKeyNames();
                    foreach (string s in subkeys)
                    {
                        Guid g;

                        if (Guid.TryParse(s, out g))
                        {
                            using (RegistryKey rightsKey = key.OpenSubKey(s))
                            {                                
                                COMIELowRightsElevationPolicy entry = new COMIELowRightsElevationPolicy(g, rightsKey);
                                if (entry.Clsid != Guid.Empty || !String.IsNullOrWhiteSpace(entry.AppPath))
                                {
                                    m_lowrights.Add(entry);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadLowRights(COMRegistryMode mode, SecurityIdentifier user)
        {
            m_lowrights = new List<COMIELowRightsElevationPolicy>();

            if (mode == COMRegistryMode.Merged || mode == COMRegistryMode.MachineOnly)
            {
                LoadLowRightsKey(Registry.LocalMachine);
            }

            if (mode == COMRegistryMode.Merged || mode == COMRegistryMode.UserOnly)
            {
                using (RegistryKey key = Registry.Users.OpenSubKey(user.Value))
                {
                    if (key != null)
                    {
                        LoadLowRightsKey(key);
                    }
                }
            }

            m_lowrights.Sort();
        }

        private void LoadMimeTypes(RegistryKey rootKey)
        {
            m_mimetypes = new List<COMMimeType>();
            RegistryKey key = rootKey.OpenSubKey(@"mime\database\content type");
            if (key == null)
            {
                return;
            }

            foreach (string mime_type in key.GetSubKeyNames())
            {
                RegistryKey sub_key = key.OpenSubKey(mime_type);
                if (sub_key != null)
                {
                    COMMimeType obj = new COMMimeType(mime_type, sub_key);
                    if (obj.Clsid != Guid.Empty)
                    {
                        m_mimetypes.Add(obj);
                    }
                }
            }
        }

        private void LoadAppIDs(RegistryKey rootKey)
        {
            m_appid = new SortedDictionary<Guid, COMAppIDEntry>();

            using (RegistryKey appIdKey = rootKey.OpenSubKey("AppID"))
            {
                if (appIdKey != null)
                {
                    string[] subkeys = appIdKey.GetSubKeyNames();
                    foreach (string key in subkeys)
                    {
                        Guid appid;

                        if (Guid.TryParse(key, out appid))
                        {
                            if (!m_appid.ContainsKey(appid))
                            {
                                using (RegistryKey regKey = appIdKey.OpenSubKey(key))
                                {
                                    if (regKey != null)
                                    {
                                        COMAppIDEntry ent = new COMAppIDEntry(appid, regKey);
                                        m_appid.Add(appid, ent);
                                    }
                                }
                            }
                        }
                    }             
                }
            }
        }

        #endregion
    }
}
