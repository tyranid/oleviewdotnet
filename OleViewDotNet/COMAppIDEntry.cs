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
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace OleViewDotNet
{
    [Flags]
    public enum COMAppIDFlags
    {
        None = 0,
        ActivateIUServerInDesktop = 0x1,
        SecureServerProcessSDAndBind = 0x2,
        IssueActivationRpcAtIdentify = 0x4,
        IUServerUnmodifiedLogonToken = 0x8,
        IUServerSelfSidInLaunchPermission = 0x10,
        IUServerActivateInClientSessionOnly = 0x20,
        Reserved1 = 0x40,
        Reserved2 = 0x80,
    }

    public class COMAppIDEntry : IComparable<COMAppIDEntry>, IXmlSerializable
    {     
        public COMAppIDEntry(Guid appId, RegistryKey key)
        {
            AppId = appId;
            LoadFromKey(key);
        }

        private static string ConvertSD(byte[] sd)
        {
            if (sd != null && sd.Length > 0)
            {
                try
                {
                    return COMSecurity.GetStringSDForSD(sd);
                }
                catch (Win32Exception)
                {
                }
            }
            return String.Empty;
        }

        private void LoadFromKey(RegistryKey key)
        {
            LocalService = key.GetValue("LocalService") as string;
            RunAs = key.GetValue("RunAs") as string;
            string name = key.GetValue(null) as string;
            if (!String.IsNullOrWhiteSpace(name))
            {
                Name = name.ToString();
            }
            else
            {
                Name = AppId.ToString("B");
            }

            AccessPermission = ConvertSD(key.GetValue("AccessPermission") as byte[]);
            LaunchPermission = ConvertSD(key.GetValue("LaunchPermission") as byte[]);

            DllSurrogate = key.GetValue("DllSurrogate") as string;
            if (DllSurrogate != null)
            {
                if (String.IsNullOrWhiteSpace(DllSurrogate))
                {
                    DllSurrogate = "dllhost.exe";
                }
                else
                {
                    string dllexe = key.GetValue("DllSurrogateExecutable") as string;
                    if (!String.IsNullOrWhiteSpace(dllexe))
                    {
                        DllSurrogate = dllexe;
                    }
                }
            }
            else
            {
                DllSurrogate = String.Empty;
            }

            object flags = key.GetValue("AppIDFlags");
            if (flags != null)
            {
                Flags = (COMAppIDFlags)flags;
            }

            if (String.IsNullOrWhiteSpace(RunAs) && !String.IsNullOrWhiteSpace(LocalService))
            {
                using (RegistryKey serviceKey = Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\" + LocalService))
                {
                    if (serviceKey != null)
                    {
                        RunAs = serviceKey.GetValue("ObjectName") as string;
                    }
                }
            }

            if (String.IsNullOrWhiteSpace(RunAs))
            {
                RunAs = String.Empty;
            }

            if (String.IsNullOrWhiteSpace(LocalService))
            {
                LocalService = String.Empty;
            }
        }

        public int CompareTo(COMAppIDEntry other)
        {
            return AppId.CompareTo(other.AppId);
        }

        public Guid AppId { get; private set; }        

        public string DllSurrogate { get; private set; }

        public string LocalService { get; private set; }

        public string RunAs { get; private set; }

        public string Name { get; private set; }

        public COMAppIDFlags Flags { get; private set; }

        public string LaunchPermission
        {
            get; private set; 
        }

        public string AccessPermission
        {
            get; private set; 
        }

        public bool HasLaunchPermission
        {        
            get { return !String.IsNullOrWhiteSpace(LaunchPermission); }
        }

        public bool HasAccessPermission
        {
            get { return !String.IsNullOrWhiteSpace(AccessPermission); }
        }

        public override string ToString()
        {
            return String.Format("COMAppIDEntry: {0}", Name);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMAppIDEntry right = obj as COMAppIDEntry;
            if (right == null)
            {
                return false;
            }

            return AppId == right.AppId && DllSurrogate == right.DllSurrogate && LocalService == right.LocalService && RunAs == right.RunAs && Name == right.Name && Flags == right.Flags
                && LaunchPermission == right.LaunchPermission && AccessPermission == right.AccessPermission;
        }

        public override int GetHashCode()
        {
            return AppId.GetHashCode() ^ DllSurrogate.GetSafeHashCode() ^ LocalService.GetSafeHashCode() 
                ^ RunAs.GetSafeHashCode() ^ Name.GetSafeHashCode() ^ Flags.GetHashCode() ^
                LaunchPermission.GetSafeHashCode() ^ AccessPermission.GetSafeHashCode();
        }

        internal COMAppIDEntry()
        {
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            AppId = reader.ReadGuid("appid");
            DllSurrogate = reader.ReadString("dllsurrogate");
            LocalService = reader.ReadString("localservice");
            RunAs = reader.ReadString("runas");
            Name = reader.ReadString("name");
            Flags = (COMAppIDFlags)Enum.Parse(typeof(COMAppIDFlags), reader.ReadString("flags"), true);
            LaunchPermission = reader.ReadString("launchperm");
            AccessPermission = reader.ReadString("accessperm");
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteGuid("appid", AppId);
            writer.WriteOptionalAttributeString("dllsurrogate", DllSurrogate);
            writer.WriteOptionalAttributeString("localservice", LocalService);
            writer.WriteOptionalAttributeString("runas", RunAs);
            writer.WriteOptionalAttributeString("name", Name);
            writer.WriteAttributeString("flags", Flags.ToString());
            writer.WriteOptionalAttributeString("launchperm", LaunchPermission);
            writer.WriteOptionalAttributeString("accessperm", AccessPermission);
        }
    }
}
