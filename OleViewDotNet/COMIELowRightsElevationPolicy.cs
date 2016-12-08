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
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace OleViewDotNet
{
    public class COMIELowRightsElevationPolicy : IComparable<COMIELowRightsElevationPolicy>, IXmlSerializable
    {
        public enum ElevationPolicy
        {
            NoRun = 0,
            RunAtCurrent = 1,
            RunAfterPrompt = 2,
            RunAtMedium = 3,
        }

        public string Name { get; private set; }
        public Guid Uuid { get; private set; }
        public Guid Clsid { get; private set; }
        public string AppPath { get; private set; }
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

        private void LoadFromRegistry(RegistryKey key)
        {
            List<Guid> clsidList = new List<Guid>();

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
                    Clsid = cls;
                }
            }
            
            string appName = (string)key.GetValue("AppName", null);
            string appPath = (string)key.GetValue("AppPath");

            if ((appName != null) && (appPath != null))
            {
                try
                {
                    Name = HandleNulTerminate(appName);
                    AppPath = Path.Combine(HandleNulTerminate(appPath), Name).ToLower();   
                }
                catch (ArgumentException)
                {
                }
            }
        }

        public COMIELowRightsElevationPolicy(Guid guid, RegistryKey key)
        {            
            Uuid = guid;
            Name = Uuid.ToString("B");
            LoadFromRegistry(key);
        }

        internal COMIELowRightsElevationPolicy()
        {
        }

        public int CompareTo(COMIELowRightsElevationPolicy other)
        {
            return Uuid.CompareTo(other.Uuid);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Name = reader.GetAttribute("name");
            Uuid = reader.ReadGuid("uuid");
            Clsid = reader.ReadGuid("clsid");
            AppPath = reader.GetAttribute("path");
            Policy = reader.ReadEnum<ElevationPolicy>("policy");
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("name", Name);
            writer.WriteGuid("uuid", Uuid);
            writer.WriteGuid("clsid", Clsid);
            writer.WriteOptionalAttributeString("path", AppPath);
            writer.WriteEnum("policy", Policy);            
        }
    }
}
