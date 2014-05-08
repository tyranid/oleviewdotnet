//    This file is part of OleViewDotNet.
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
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.Security;
using System.Linq;

namespace OleViewDotNet
{
    public class COMCLSIDEntry : IComparable<COMCLSIDEntry>
    {
        private Guid m_clsid;
        private string m_name;
        private string m_server;
        private string m_cmdline;
        private Guid m_appid;
        private Guid m_typelib;
        private List<string> m_progids;
        private HashSet<Guid> m_categories;

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

        private static bool IsInvalidFileName(string filename)
        {                        
            return filename.IndexOfAny(Path.GetInvalidPathChars()) >= 0;                
        }

        private static string ProcessFileName(string filename, bool localServer)
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
            else if (localServer && temp.Contains(" "))
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

            return filename;
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
                m_cmdline = serverKey.GetValue(null).ToString();
                m_server = ProcessFileName(m_cmdline, m_servertype == ServerType.LocalServer32);

                try
                {
                    // Expand out any short filenames
                    if (m_server.Contains("~") && !IsInvalidFileName(m_server))
                    {
                        m_server = Path.GetFullPath(m_server);
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
                    if (Guid.TryParse(appid.ToString(), out m_appid))
                    {
                        if (m_appid != Guid.Empty)
                        {
                            if (m_servertype == ServerType.UnknownServer)
                            {
                                m_servertype = ServerType.LocalServer32;
                            }
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
                Guid.TryParse(typelib, out m_typelib);
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
                m_categories.Add(ControlCategory);
            }

            if (key.OpenSubKey("Insertable") != null)
            {
                m_categories.Add(InsertableCategory);
            }

            if (key.OpenSubKey("DocObject") != null)
            {
                m_categories.Add(DocumentCategory);
            }

            RegistryKey categories = key.OpenSubKey("Implemented Categories");
            if (categories != null)
            {
                string[] subKeys = categories.GetSubKeyNames();
                foreach(string s in subKeys)
                {
                    Guid g;

                    if (Guid.TryParse(s, out g))
                    {
                        m_categories.Add(g);
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

        public COMCLSIDEntry(Guid clsid, RegistryKey rootKey) : this(clsid)
        {            
            LoadFromKey(rootKey);
        }

        private COMCLSIDEntry(Guid clsid)
        {
            m_clsid = clsid;
            m_progids = new List<string>();
            m_proxies = new List<COMInterfaceEntry>();
            m_categories = new HashSet<Guid>();
            m_server = String.Empty;
            m_cmdline = String.Empty;
            m_servertype = ServerType.UnknownServer;
        }

        public COMCLSIDEntry(Guid clsid, ServerType type)
            : this(clsid)
        {
            m_servertype = type;
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

        public string CmdLine
        {
            get { return m_cmdline; }
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
                return m_categories.ToArray();
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

        public IntPtr CreateInstance(COMUtilities.CLSCTX dwContext)
        {
            IntPtr pInterface = IntPtr.Zero;
            //COMUtilities.CLSCTX dwContext = COMUtilities.CLSCTX.CLSCTX_ALL;
            bool blValid = false;

            if (dwContext == COMUtilities.CLSCTX.CLSCTX_ALL)
            {
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
            }
            else
            {
                blValid = true;
            }

            if (blValid)
            {
                Guid iid = COMInterfaceEntry.CreateKnownInterface(COMInterfaceEntry.KnownInterfaces.IUnknown).Iid;

                int iError = COMUtilities.CoCreateInstance(ref m_clsid, IntPtr.Zero, dwContext, ref iid, out pInterface);

                if (iError != 0)
                {
                    Marshal.ThrowExceptionForHR(iError);
                }
            }

            return pInterface;
        }

        public object CreateInstanceAsObject(COMUtilities.CLSCTX dwContext)
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
    }
}
