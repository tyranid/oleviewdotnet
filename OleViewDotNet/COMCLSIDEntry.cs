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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace OleViewDotNet
{
    public enum COMServerType
    {
        UnknownServer,
        InProcServer32,
        LocalServer32,
    }

    public enum COMThreadingModel
    {
        None,
        Apartment,
        Both,
        Free,
        Neutral
    }

    [Serializable]
    public class COMCLSIDEntry : IComparable<COMCLSIDEntry>
    {
        private List<string> m_progids;
        private HashSet<Guid> m_categories;
        private List<COMInterfaceEntry> m_proxies;

        private static Guid ControlCategory = new Guid("{40FC6ED4-2438-11CF-A3DB-080036F12502}");
        private static Guid InsertableCategory = new Guid("{40FC6ED3-2438-11CF-A3DB-080036F12502}");
        private static Guid DocumentCategory = new Guid("{40fc6ed8-2438-11cf-a3db-080036f12502}");

        public int CompareTo(COMCLSIDEntry right)
        {
            return String.Compare(Name, right.Name);
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

            RegistryKey serverKey = key.OpenSubKey("InProcServer32");
            ServerType = COMServerType.InProcServer32;
            if (serverKey == null)
            {
                serverKey = key.OpenSubKey("LocalServer32");
                ServerType = COMServerType.LocalServer32;
            }

            if ((serverKey != null) && (serverKey.GetValue(null) != null))
            {
                CmdLine = serverKey.GetValue(null).ToString();
                Server = ProcessFileName(CmdLine, ServerType == COMServerType.LocalServer32);
                string threading_model = serverKey.GetValue("ThreadingModel") as string;
                if (threading_model != null)
                {
                    switch (threading_model.ToLower())
                    {
                        case "both":
                            ThreadingModel = COMThreadingModel.Both;
                            break;
                        case "free":
                            ThreadingModel = COMThreadingModel.Free;
                            break;
                        case "neutral":
                            ThreadingModel = COMThreadingModel.Neutral;
                            break;
                        case "apartment":
                        default:
                            ThreadingModel = COMThreadingModel.Apartment;
                            break;
                    }
                }
                else if (ServerType == COMServerType.LocalServer32)
                {
                    ThreadingModel = COMThreadingModel.Both;
                }

                try
                {
                    // Expand out any short filenames
                    if (Server.Contains("~") && !IsInvalidFileName(Server))
                    {
                        Server = Path.GetFullPath(Server);
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
                Server = String.Empty;
                ServerType = COMServerType.UnknownServer;
            }

            AppID = Guid.Empty;

            try
            {
                object appid = key.GetValue("AppID");
                if ((appid != null) && (appid.ToString().Length > 0))
                {
                    Guid appid_guid;
                    if (Guid.TryParse(appid.ToString(), out appid_guid))
                    {
                        if (appid_guid != Guid.Empty)
                        {
                            if (ServerType == COMServerType.UnknownServer)
                            {
                                ServerType = COMServerType.LocalServer32;
                            }
                        }
                    }
                }
            }
            catch (FormatException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            TypeLib = Guid.Empty;
            string typelib = COMUtilities.ReadStringFromKey(key, "TypeLib", null);
            if (!String.IsNullOrEmpty(typelib))
            {
                Guid typelib_guid;
                if (Guid.TryParse(typelib, out typelib_guid))
                {
                    TypeLib = typelib_guid;
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
            Clsid = clsid;
            m_progids = new List<string>();
            m_proxies = new List<COMInterfaceEntry>();
            m_categories = new HashSet<Guid>();
            Server = String.Empty;
            CmdLine = String.Empty;
            ServerType = COMServerType.UnknownServer;
            ThreadingModel = COMThreadingModel.Apartment;
        }

        public COMCLSIDEntry(Guid clsid, COMServerType type)
            : this(clsid)
        {
            ServerType = type;
        }

        public Guid Clsid { get; private set; }

        public string Name { get; private set; }

        public string Server { get; private set; }

        public string CmdLine { get; private set; }

        public COMServerType ServerType { get; private set; }

        public IEnumerable<string> ProgIDs
        {
            get { return m_progids.AsReadOnly(); }
        }

        public Guid AppID { get; private set; }

        public IEnumerable<COMInterfaceEntry> Proxies
        {
            get { return m_proxies.AsReadOnly(); }
        }

        public Guid TypeLib { get; private set; }

        public Guid[] Categories
        {
            get 
            {
                return m_categories.ToArray();
            }
        }

        public COMThreadingModel ThreadingModel { get; private set; }

        public COMUtilities.CLSCTX CreateContext
        {
            get
            {
                COMUtilities.CLSCTX dwContext = COMUtilities.CLSCTX.CLSCTX_ALL;

                if (ServerType == COMServerType.InProcServer32)
                {
                    dwContext = COMUtilities.CLSCTX.CLSCTX_INPROC_SERVER;             
                }
                else if (ServerType == COMServerType.LocalServer32)
                {
                    dwContext = COMUtilities.CLSCTX.CLSCTX_LOCAL_SERVER;             
                }

                return dwContext;
            }           
        }

        public IntPtr CreateInstance(COMUtilities.CLSCTX dwContext)
        {
            IntPtr pInterface = IntPtr.Zero;
            bool blValid = false;

            if (dwContext == COMUtilities.CLSCTX.CLSCTX_ALL)
            {
                if (ServerType == COMServerType.InProcServer32)
                {
                    dwContext = COMUtilities.CLSCTX.CLSCTX_INPROC_SERVER;
                    blValid = true;
                }
                else if (ServerType == COMServerType.LocalServer32)
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
                Guid clsid = Clsid;
                int iError = COMUtilities.CoCreateInstance(ref clsid, IntPtr.Zero, dwContext, ref iid, out pInterface);

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

        public override string ToString()
        {
            return String.Format("COMCLSIDEntry: {0}", Name);
        }
    }
}
