using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OleViewDotNet
{
    public class COMIELowRightsElevationPolicy : IComparable<COMIELowRightsElevationPolicy>
    {
        public enum ElevationPolicy
        {
            NoRun = 0,
            RunAtCurrent = 1,
            RunAtMedium = 3,
        }

        public string Name { get; private set; }
        public Guid Uuid { get; private set; }
        public COMCLSIDEntry[] Clsids { get; private set; }
        public ElevationPolicy Policy { get; private set; }

        private static string HandleNulTerminate(string s)
        {
            int index = s.IndexOf('\0');
            if (index >= 0)
            {
                return s.Substring(0, index);
            }
            else
            {
                return s;
            }
        }

        private void LoadFromRegistry(SortedDictionary<Guid, COMCLSIDEntry> clsids, SortedDictionary<string, List<COMCLSIDEntry>> servers, RegistryKey key)
        {
            List<COMCLSIDEntry> clsidList = new List<COMCLSIDEntry>();
            Policy = (ElevationPolicy)key.GetValue("Policy", 0);
            string clsid = (string)key.GetValue("CLSID");            

            if (clsid != null)
            {
                Guid cls;

                if (Guid.TryParse(clsid, out cls))
                {
                    if (clsids.ContainsKey(cls))
                    {
                        clsidList.Add(clsids[cls]);
                    }
                    else
                    {
                        // Add dummy entry
                        clsidList.Add(new COMCLSIDEntry(cls, COMCLSIDEntry.ServerType.LocalServer32));
                    }
                }
            }
            else
            {                
                string appName = (string)key.GetValue("AppName", null);
                string appPath = (string)key.GetValue("AppPath");

                if ((appName != null) && (appPath != null))
                {
                    try
                    {
                        string path = Path.Combine(HandleNulTerminate(appPath), HandleNulTerminate(appName)).ToLower();

                        if (servers.ContainsKey(path))
                        {
                            clsidList.AddRange(servers[path]);
                        }

                        Name = HandleNulTerminate(appName);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            Clsids = clsidList.ToArray();
        }

        public COMIELowRightsElevationPolicy(Guid guid, SortedDictionary<Guid, COMCLSIDEntry> clsids, SortedDictionary<string, List<COMCLSIDEntry>> servers, RegistryKey key)
        {
            Uuid = guid;
            Name = Uuid.ToString("B");
            LoadFromRegistry(clsids, servers, key);
        }

        public int CompareTo(COMIELowRightsElevationPolicy other)
        {
            return Uuid.CompareTo(other.Uuid);
        }
    }
}
