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
using System.Security.AccessControl;

namespace OleViewDotNet
{
    public class COMAppIDEntry : IComparable<COMAppIDEntry>
    {
        private Guid m_appId;
        private string m_service;
        private string m_runas;
        private string m_name;
        private byte[] m_access;
        private byte[] m_launch;

        public COMAppIDEntry(Guid appId, RegistryKey key)
        {
            m_appId = appId;
            LoadFromKey(key);
        }

        private void LoadFromKey(RegistryKey key)
        {
            m_service = key.GetValue("LocalService") as string;
            m_runas = key.GetValue("RunAs") as string;
            string name = key.GetValue(null) as string;
            if (!String.IsNullOrWhiteSpace(name))
            {
                m_name = name.ToString();
            }
            else
            {
                m_name = m_appId.ToString("B");
            }

            m_access = key.GetValue("AccessPermission") as byte[];
            m_launch = key.GetValue("LaunchPermission") as byte[];

            if (String.IsNullOrWhiteSpace(m_runas) && !String.IsNullOrWhiteSpace(m_service))
            {
                using (RegistryKey serviceKey = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\" + m_service))
                {
                    if (serviceKey != null)
                    {
                        m_runas = serviceKey.GetValue("ObjectName") as string;
                    }
                }
            }
        }

        public int CompareTo(COMAppIDEntry other)
        {
            return m_appId.CompareTo(other.m_appId);
        }

        public Guid AppId
        {
            get
            {
                return m_appId;
            }
        }

        public string LocalService
        {
            get { return m_service; }
        }

        public string RunAs
        {
            get { return m_runas; }
        }

        public string Name
        {
            get { return m_name; }
        }

        public byte[] LaunchPermission
        {
            get { return m_launch; }
        }

        public byte[] AccessPermission
        {
            get { return m_access; }
        }

        public override string ToString()
        {
            return String.Format("COMAppIDEntry: {0}", m_name);
        }
    }
}
