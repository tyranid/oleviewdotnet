//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet
{
    public enum IdentityType
    {
        ActivateAsActivator = 0,
        RunAs,
        ActivateAsPackage,
        SessionVirtual,
        SessionUser,
    }

    public enum ServerType
    {
        NormalExe = 0,
        ExeService = 1,
        SvchostService = 2
    }

    public enum InstancingType
    {
        SingleInstance = 0,
        MultipleInstances = 1
    }

    public class COMRuntimeServerEntry : IComparable<COMRuntimeServerEntry>, IXmlSerializable
    {
        public string Identity { get; private set; }
        public string Name { get; private set; }
        public string ServiceName { get; private set; }
        public string ExePath { get; private set; }
        public string Permissions { get; private set; }
        public bool HasPermission
        {
            get { return !String.IsNullOrWhiteSpace(Permissions); }
        }
        public IdentityType IdentityType { get; private set; }
        public ServerType ServerType { get; private set; }
        public InstancingType InstancingType { get; private set; }

        private void LoadFromKey(RegistryKey key)
        {
            IdentityType = (IdentityType)COMUtilities.ReadIntFromKey(key, null, "IdentityType");
            ServerType = (ServerType)COMUtilities.ReadIntFromKey(key, null, "ServerType");
            InstancingType = (InstancingType)COMUtilities.ReadIntFromKey(key, null, "InstancingType");
            Identity = COMUtilities.ReadStringFromKey(key, null, "Identity");
            ServiceName = COMUtilities.ReadStringFromKey(key, null, "ServiceName");
            ExePath = COMUtilities.ReadStringFromKey(key, null, "ExePath");
            Permissions = string.Empty;
            byte[] permissions = key.GetValue("Permissions", new byte[0]) as byte[];
            Permissions = COMSecurity.GetStringSDForSD(permissions);
        }

        internal COMRuntimeServerEntry()
        {
        }

        public COMRuntimeServerEntry(string name, RegistryKey rootKey)
        {
            Name = name;
            LoadFromKey(rootKey);
        }

        int IComparable<COMRuntimeServerEntry>.CompareTo(COMRuntimeServerEntry other)
        {
            return Name.CompareTo(other.Name);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Name = reader.ReadString("name");
            IdentityType = reader.ReadEnum<IdentityType>("idtype");
            ServerType = reader.ReadEnum<ServerType>("servertype");
            InstancingType = reader.ReadEnum<InstancingType>("instancetype");
            ServiceName = reader.ReadString("servicename");
            ExePath = reader.ReadString("exepath");
            Identity = reader.ReadString("identity");
            Permissions = reader.ReadString("perms");
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteOptionalAttributeString("name", Name);
            writer.WriteEnum("idtype", IdentityType);
            writer.WriteEnum("servertype", ServerType);
            writer.WriteEnum("instancetype", InstancingType);
            writer.WriteOptionalAttributeString("servicename", ServiceName);
            writer.WriteOptionalAttributeString("exepath", ExePath);
            writer.WriteOptionalAttributeString("perms", Permissions);
            writer.WriteOptionalAttributeString("identity", Identity);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMRuntimeServerEntry right = obj as COMRuntimeServerEntry;
            if (right == null)
            {
                return false;
            }

            return IdentityType == right.IdentityType && ServerType == right.ServerType &&
                InstancingType == right.InstancingType && ServiceName == right.ServiceName &&
                ExePath == right.ExePath && Permissions == right.Permissions &&
                Identity == right.Identity;
        }

        public override int GetHashCode()
        {
            return IdentityType.GetHashCode() ^ ServerType.GetHashCode() ^ InstancingType.GetHashCode() ^
                ServiceName.GetSafeHashCode() ^ ExePath.GetSafeHashCode() ^ Permissions.GetSafeHashCode() ^
                Identity.GetSafeHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
