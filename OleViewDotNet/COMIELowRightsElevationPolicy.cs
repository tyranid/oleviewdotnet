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

namespace OleViewDotNet
{
    [Serializable]
    public class COMIELowRightsElevationPolicy : IComparable<COMIELowRightsElevationPolicy>
    {
        private COMRegistry m_registry;

        public enum ElevationPolicy
        {
            NoRun = 0,
            RunAtCurrent = 1,
            RunAfterPrompt = 2,
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

            object policyValue = key.GetValue("Policy", 0);

            if (policyValue != null)
            {                
                Policy = (ElevationPolicy)Enum.ToObject(typeof(ElevationPolicy), key.GetValue("Policy", 0));
            }
            
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
                        clsidList.Add(new COMCLSIDEntry(m_registry, cls, COMServerType.LocalServer32));
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

        public COMIELowRightsElevationPolicy(COMRegistry registry, Guid guid, SortedDictionary<Guid, COMCLSIDEntry> clsids, SortedDictionary<string, List<COMCLSIDEntry>> servers, RegistryKey key)
        {
            m_registry = registry;
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
