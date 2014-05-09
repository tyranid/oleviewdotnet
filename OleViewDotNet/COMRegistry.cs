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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Linq;

namespace OleViewDotNet
{
    class EnumerateInterfaces
    {
        static public Guid[] GetInterfaces(COMCLSIDEntry ent)
        {            
            string strProcess;

            string appDirectory = COMUtilities.GetAppDirectory();

            // Try sepecific bit version first
            strProcess = Path.Combine(appDirectory, Environment.Is64BitProcess ? "EnumerateInterfaces64.exe" : "EnumerateInterfaces32.exe");
            if (!File.Exists(strProcess))
            {
                strProcess = Path.Combine(appDirectory, "EnumerateInterfaces.exe");
            }

            Process proc = new Process();
            ProcessStartInfo info = new ProcessStartInfo(strProcess, String.Format("{0} s {1} {2}",
                ent.Clsid.ToString("B"), (int)ent.CreateContext, COMInterfaceEntry.IID_IMarshal.ToString("B")));
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            proc.StartInfo = info;
            
            proc.Start();            
            
            string strOutput = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
                        
            List<Guid> guids = new List<Guid>();
            TextReader reader = new StringReader(strOutput);
            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }

                if (line.StartsWith("ERROR:"))
                {
                    uint errorCode;
                    try
                    {
                        errorCode = uint.Parse(line.Substring(6), System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    catch (FormatException ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        errorCode = 0x80004005;
                    }

                    throw new Win32Exception((int)errorCode);
                }
                else
                {
                    Guid g;

                    if (Guid.TryParse(line, out g))
                    {
                        guids.Add(g);
                    }                    
                }
            }

            int exitCode = proc.ExitCode;

            proc.Close();

            if (exitCode == 0)
            {
                return guids.ToArray();
            }
            else
            {
                return new Guid[] { COMInterfaceEntry.IID_IUnknown };
            }            
        }        
    }

    /// <summary>
    /// Class to hold information about the current COM registration information
    /// </summary>
    public class COMRegistry
    {
        #region Private Member Variables
        private SortedDictionary<Guid, COMCLSIDEntry> m_clsids;        
        private SortedDictionary<Guid, COMInterfaceEntry> m_interfaces;
        private SortedDictionary<string, COMProgIDEntry> m_progids;
        private SortedDictionary<string, List<COMCLSIDEntry>> m_clsidbyserver;
        private SortedDictionary<string, List<COMCLSIDEntry>> m_clsidbylocalserver;        
        private COMCLSIDEntry[] m_clsidbyname;
        private COMInterfaceEntry[] m_interfacebyname;
        private Dictionary<Guid, COMInterfaceEntry[]> m_supportediids;
        private Dictionary<Guid, List<COMCLSIDEntry>> m_categories;
        private List<COMCLSIDEntry> m_preapproved;
        private List<COMIELowRightsElevationPolicy> m_lowrights;
        private SortedDictionary<Guid, COMAppIDEntry> m_appid;        

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

        public SortedDictionary<string, List<COMCLSIDEntry>> ClsidsByServer
        {
            get 
            {
                return m_clsidbyserver; 
            }
        }

        public SortedDictionary<string, List<COMCLSIDEntry>> ClsidsByLocalServer
        {
            get
            {
                return m_clsidbylocalserver;
            }
        }

        public COMCLSIDEntry[] ClsidsByName
        {
            get 
            {
                return m_clsidbyname; 
            }
        }

        public COMInterfaceEntry[] InterfacesByName
        {
            get
            {
                return m_interfacebyname;
            }
        }

        public Dictionary<Guid, List<COMCLSIDEntry>> ImplementedCategories
        {
            get { return m_categories; }
        }

        public COMCLSIDEntry[] PreApproved
        {
            get { return m_preapproved.ToArray(); }
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
        public static COMRegistry Instance
        {
            get { return _instance; }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Default constructor
        /// </summary>
        private COMRegistry(RegistryKey rootKey)
        {
            m_supportediids = new Dictionary<Guid, COMInterfaceEntry[]>();
            LoadCLSIDs(rootKey);
            LoadProgIDs(rootKey);
            LoadInterfaces(rootKey);
            LoadPreApproved();
            LoadLowRights();
            LoadAppIDs(rootKey);
            InterfaceViewers.InterfaceViewers.LoadInterfaceViewers();
            COMUtilities.LoadTypeLibAssemblies();
        }

        private static COMRegistry _instance;


        public static void Load(RegistryKey rootKey)
        {
            _instance = new COMRegistry(rootKey);
        }


        /// <summary>
        /// Get the list of supported interfaces from an IUnknown pointer
        /// </summary>
        /// <param name="pObject">The IUnknown pointer</param>
        /// <returns>List of interfaces supported</returns>
        public COMInterfaceEntry[] GetInterfacesForIUnknown(IntPtr pObject)
        {
            List<COMInterfaceEntry> list = new List<COMInterfaceEntry>();

            foreach (COMInterfaceEntry intEnt in m_interfacebyname)
            {
                Guid currIID = intEnt.Iid;
                IntPtr pRequested;

                if (Marshal.QueryInterface(pObject, ref currIID, out pRequested) == 0)
                {
                    list.Add(intEnt);
                    Marshal.Release(pRequested);
                }
            }

            return list.ToArray();
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
            ret = GetInterfacesForIUnknown(pObject);
            Marshal.Release(pObject);

            return ret;
        }

        /// <summary>
        /// Get list of supported Interface IIDs (that we know about)
        /// NOTE: This will load the object itself to check what is supported, it _might_ crash the app
        /// The returned array is cached so subsequent calls to this function return without calling into COM
        /// </summary>
        /// <param name="ent">The entry to get the interfaces for</param>
        /// <param name="bRefresh">Force the supported interface list to refresh</param>
        /// <returns>An array of supported interfaces</returns>
        public COMInterfaceEntry[] GetSupportedInterfaces(COMCLSIDEntry ent, bool bRefresh)
        {
            COMInterfaceEntry[] retList = null;

            if (ent != null)
            {
                if (m_supportediids.ContainsKey(ent.Clsid))
                {
                    retList = m_supportediids[ent.Clsid];
                }
                else
                {
                    Guid[] guids = EnumerateInterfaces.GetInterfaces(ent);
                    List<COMInterfaceEntry> ents = new List<COMInterfaceEntry>();

                    foreach (Guid g in guids)
                    {
                        if (m_interfaces.ContainsKey(g))
                        {
                            ents.Add(m_interfaces[g]);
                        }
                        else
                        {
                            ents.Add(new COMInterfaceEntry(g));
                        }
                    }

                    ents.Sort();
                    retList = ents.ToArray();
                    m_supportediids[ent.Clsid] = retList;
                }
            }
            else
            {
                retList = new COMInterfaceEntry[0];
            }

            return retList;
        }

        public void DoSerialization(string strFileName)
        {
        }

#endregion

        #region Private Methods

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
            Dictionary<string, List<COMCLSIDEntry>> clsidbyserver = new Dictionary<string, List<COMCLSIDEntry>>();
            Dictionary<string, List<COMCLSIDEntry>> clsidbylocalserver = new Dictionary<string, List<COMCLSIDEntry>>();  
            m_categories = new Dictionary<Guid, List<COMCLSIDEntry>>();

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
                                        if (!String.IsNullOrEmpty(ent.Server) && ent.Type != (COMCLSIDEntry.ServerType.UnknownServer))
                                        {
                                            AddEntryToDictionary(clsidbyserver, ent);
                                            if (ent.Type == COMCLSIDEntry.ServerType.LocalServer32)
                                            {
                                                AddEntryToDictionary(clsidbylocalserver, ent);
                                            }
                                        }

                                        if (ent.Categories.Length > 0)
                                        {
                                            foreach (Guid catid in ent.Categories)
                                            {
                                                List<COMCLSIDEntry> list = null;
                                                if (m_categories.ContainsKey(catid))
                                                {
                                                    list = m_categories[catid];
                                                }
                                                else
                                                {
                                                    list = new List<COMCLSIDEntry>();
                                                    m_categories[catid] = list;
                                                }
                                                list.Add(ent);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }                    
                }
            }

            int pos = 0;
            m_clsidbyname = new COMCLSIDEntry[clsids.Count];
            foreach (COMCLSIDEntry ent in clsids.Values)
            {
                m_clsidbyname[pos++] = ent;
            }
            Array.Sort(m_clsidbyname);

            m_clsids = new SortedDictionary<Guid, COMCLSIDEntry>(clsids);
            m_clsidbyserver = new SortedDictionary<string, List<COMCLSIDEntry>>(clsidbyserver);
            m_clsidbylocalserver = new SortedDictionary<string, List<COMCLSIDEntry>>(clsidbylocalserver);
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
                        using (RegistryKey clsidKey = regKey.OpenSubKey("CLSID"))
                        {
                            if (clsidKey != null)
                            {
                                Guid clsid;
                                object clsidString = clsidKey.GetValue(null);
                                if (clsidString != null)
                                {
                                    COMCLSIDEntry entry = null;
                                    clsid = new Guid(clsidString.ToString());
                                    if (m_clsids.ContainsKey(clsid))
                                    {
                                        entry = m_clsids[clsid];
                                    }
                                    m_progids.Add(key, new COMProgIDEntry(key, clsid, entry, regKey));
                                }
                            }
                        }
                    }
                }
                catch (FormatException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
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
                                        if (ent.ProxyClsid != Guid.Empty)
                                        {
                                            if (m_clsids.ContainsKey(ent.ProxyClsid))
                                            {
                                                m_clsids[ent.ProxyClsid].AddProxy(ent);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }

            int pos = 0;
            m_interfacebyname = new COMInterfaceEntry[interfaces.Count];
            foreach (COMInterfaceEntry ent in interfaces.Values)
            {
                m_interfacebyname[pos++] = ent;
            }
            Array.Sort(m_interfacebyname);

            m_interfaces = new SortedDictionary<Guid, COMInterfaceEntry>(interfaces);
        }

        void LoadPreApproved()
        {
            m_preapproved = new List<COMCLSIDEntry>();
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Ext\\PreApproved"))
            {
                if (key != null)
                {
                    string[] subkeys = key.GetSubKeyNames();
                    foreach (string s in subkeys)
                    {
                        Guid g;

                        if(Guid.TryParse(s, out g))
                        {
                            if (m_clsids.ContainsKey(g))
                            {
                                m_preapproved.Add(m_clsids[g]);
                            }
                        }
                    }
                }
            }
        }

        private void LoadLowRightsKey(RegistryKey rootKey)
        {
            using (RegistryKey key = rootKey.OpenSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\Low Rights\\ElevationPolicy"))
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
                                COMIELowRightsElevationPolicy entry = new COMIELowRightsElevationPolicy(g, m_clsids, m_clsidbyserver, rightsKey);
                                if (entry.Clsids.Length > 0)
                                {
                                    m_lowrights.Add(entry);
                                }
                            }
                        }
                    }
                }
            }
        }

        void LoadLowRights()
        {
            m_lowrights = new List<COMIELowRightsElevationPolicy>();
            LoadLowRightsKey(Registry.LocalMachine);
            LoadLowRightsKey(Registry.CurrentUser);
            m_lowrights.Sort();
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
