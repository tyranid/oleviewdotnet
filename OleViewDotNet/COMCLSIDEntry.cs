using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace OleViewDotNet
{
    public class COMCLSIDEntry : IComparable<COMCLSIDEntry>
    {
        private Guid m_clsid;
        private string m_name;
        private string m_server;
        private Guid m_appid;
        private Guid m_typelib;
        private List<string> m_progids;
        private Dictionary<Guid, bool> m_categories;

        private static Guid ControlCategory = new Guid("{40FC6ED4-2438-11CF-A3DB-080036F12502}");
        private static Guid InsertableCategory = new Guid("{40FC6ED3-2438-11CF-A3DB-080036F12502}");
        private static Guid DocumentCategory = new Guid("{40fc6ed8-2438-11cf-a3db-080036f12502}");

        /// <summary>
        /// List of the proxies this object implemented 
        /// </summary>
        private List<COMInterfaceEntry> m_proxies;

        public enum ServerType
        {
            UnknownServer,
            InProcServer32,
            LocalServer32,
        }

        ServerType m_servertype;

        public int CompareTo(COMCLSIDEntry right)
        {
            return String.Compare(m_name, right.m_name);
        }

        private void LoadFromKey(RegistryKey key)
        {
            object name = key.GetValue(null);
            m_name = null;
            if (name != null)
            {
                string s = name.ToString().Trim();

                if (s.Length > 0)
                {
                    m_name = name.ToString();
                }
            }

            if (m_name == null)
            {
                m_name = m_clsid.ToString("B");
            }

            RegistryKey serverKey = key.OpenSubKey("InProcServer32");
            m_servertype = ServerType.InProcServer32;
            if (serverKey == null)
            {
                serverKey = key.OpenSubKey("LocalServer32");
                m_servertype = ServerType.LocalServer32;
            }

            if ((serverKey != null) && (serverKey.GetValue(null) != null))
            {
                m_server = serverKey.GetValue(null).ToString();
                serverKey.Close();
            }
            else
            {
                m_server = "";
                m_servertype = ServerType.UnknownServer;
            }

            m_appid = Guid.Empty;

            try
            {
                object appid = key.GetValue("AppID");
                if ((appid != null) && (appid.ToString().Length > 0))
                {
                    m_appid = new Guid(appid.ToString());
                    if (m_appid != Guid.Empty)
                    {
                        if (m_servertype == ServerType.UnknownServer)
                        {
                            m_servertype = ServerType.LocalServer32;
                        }
                    }
                }
            }
            catch (FormatException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            m_typelib = Guid.Empty;
            string typelib = COMUtilities.ReadStringFromKey(key, "TypeLib", null);
            if (!String.IsNullOrEmpty(typelib))
            {
                try
                {
                    m_typelib = new Guid(typelib);
                }
                catch (FormatException e)
                {                    
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }

            string progid = COMUtilities.ReadStringFromKey(key, "ProgID", null);
            if (!String.IsNullOrEmpty(progid))
            {
                AddProgID(progid);
            }
            progid = COMUtilities.ReadStringFromKey(key, "VersionIndependentProgID", null);
            if (!String.IsNullOrEmpty(progid))
            {
                AddProgID(progid);
            }

            if (key.OpenSubKey("Control") != null)
            {
                m_categories.Add(ControlCategory, true);
            }

            if (key.OpenSubKey("Insertable") != null)
            {
                m_categories.Add(InsertableCategory, true);
            }

            if (key.OpenSubKey("DocObject") != null)
            {
                m_categories.Add(DocumentCategory, true);
            }

            RegistryKey categories = key.OpenSubKey("Implemented Categories");
            if (categories != null)
            {
                string[] subKeys = categories.GetSubKeyNames();
                foreach(string s in subKeys)
                {
                    try
                    {
                        Guid g = new Guid(s);
                        if (!m_categories.ContainsKey(g))
                        {
                            m_categories.Add(new Guid(s), true);
                        }
                    }
                    catch (FormatException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
            }
        }

        public void AddProgID(string progid)
        {
            if (!m_progids.Contains(progid))
            {
                m_progids.Add(progid);
            }
        }

        public void AddProxy(COMInterfaceEntry ent)
        {
            m_proxies.Add(ent);
        }

        public COMCLSIDEntry(Guid clsid, RegistryKey rootKey)
        {
            m_clsid = clsid;
            m_progids = new List<string>();
            m_proxies = new List<COMInterfaceEntry>();
            m_categories = new Dictionary<Guid, bool>();
            LoadFromKey(rootKey);
        }

        public Guid Clsid
        {
            get { return m_clsid; }
        }

        public string Name
        {
            get { return m_name; }
        }

        public string Server
        {
            get { return m_server; }
        }

        public ServerType Type
        {
            get { return m_servertype; }
        }

        public string[] ProgIDs
        {
            get { return m_progids.ToArray(); }
        }

        public Guid AppID
        {
            get { return m_appid; }
        }

        public COMInterfaceEntry[] Proxies
        {
            get { return m_proxies.ToArray(); }
        }

        public Guid TypeLib
        {
            get { return m_typelib; }
        }

        public Guid[] Categories
        {
            get 
            { 
                Guid[] cat = new Guid[m_categories.Keys.Count];
                m_categories.Keys.CopyTo(cat, 0);
                return cat;
            }
        }

        public COMUtilities.CLSCTX CreateContext
        {
            get
            {
                COMUtilities.CLSCTX dwContext = COMUtilities.CLSCTX.CLSCTX_ALL;

                if (m_servertype == COMCLSIDEntry.ServerType.InProcServer32)
                {
                    dwContext = COMUtilities.CLSCTX.CLSCTX_INPROC_SERVER;             
                }
                else if (m_servertype == COMCLSIDEntry.ServerType.LocalServer32)
                {
                    dwContext = COMUtilities.CLSCTX.CLSCTX_LOCAL_SERVER;             
                }

                return dwContext;
            }           
        }

        public IntPtr CreateInstance()
        {
            IntPtr pInterface = IntPtr.Zero;
            COMUtilities.CLSCTX dwContext = COMUtilities.CLSCTX.CLSCTX_ALL;
            bool blValid = false;

            if (m_servertype == COMCLSIDEntry.ServerType.InProcServer32)
            {
                dwContext = COMUtilities.CLSCTX.CLSCTX_INPROC_SERVER;
                blValid = true;
            }
            else if (m_servertype == COMCLSIDEntry.ServerType.LocalServer32)
            {
                dwContext = COMUtilities.CLSCTX.CLSCTX_LOCAL_SERVER;
                blValid = true;
            }

            if (blValid)
            {
                Guid iid = COMInterfaceEntry.CreateKnownInterface(COMInterfaceEntry.KnownInterfaces.IUnknown).Iid;

                int iError = COMUtilities.CoCreateInstance(ref m_clsid, IntPtr.Zero, dwContext, ref iid, out pInterface);

                if (iError != 0)
                {
                    throw new Win32Exception(iError);
                }
            }

            return pInterface;
        }

        public object CreateInstanceAsObject()
        {
            IntPtr pObject = CreateInstance();
            object ret = null;

            if (pObject != IntPtr.Zero)
            {
                ret = Marshal.GetObjectForIUnknown(pObject);
                Marshal.Release(pObject);
            }

            return ret;
        }
    }
}
