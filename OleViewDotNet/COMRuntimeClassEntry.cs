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
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet
{
    public enum TrustLevel
    {
        BaseTrust = 0,
        PartialTrust = 1,
        FullTrust = 2
    }

    public enum ActivationType
    {
        InProcess = 0,
        OutOfProcess = 1
    }

    public class COMRuntimeClassEntry : IXmlSerializable
    {
        public string Name { get; private set; }
        public Guid Clsid { get; private set; }
        public string DllPath { get; private set; }
        public string Server { get; private set; }
        public ActivationType ActivationType { get; private set; }
        public string Permissions
        {
            get; private set;
        }
        public bool HasPermission
        {
            get { return !String.IsNullOrWhiteSpace(Permissions); }
        }
        public TrustLevel TrustLevel
        {
            get; private set;
        }
        public int Threading
        {
            get; private set;
        }

        private void LoadFromKey(RegistryKey key)
        {
            Clsid = COMUtilities.ReadGuidFromKey(key, null, "CLSID");
            ActivationType = (ActivationType)COMUtilities.ReadIntFromKey(key, null, "ActivationType");
            TrustLevel = (TrustLevel)COMUtilities.ReadIntFromKey(key, null, "TrustLevel");
            Threading = COMUtilities.ReadIntFromKey(key, null, "Threading");
            DllPath = COMUtilities.ReadStringFromKey(key, null, "DllPath");
            Server = COMUtilities.ReadStringFromKey(key, null, "Server");
            Permissions = string.Empty;
            byte[] permissions = key.GetValue("Permissions", new byte[0]) as byte[];
            Permissions = COMSecurity.GetStringSDForSD(permissions);
        }

        public COMRuntimeClassEntry(string name, RegistryKey rootKey)
        {
            Name = name;
            LoadFromKey(rootKey);
        }

        internal COMRuntimeClassEntry()
        {
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Clsid = reader.ReadGuid("clsid");
            Name = reader.ReadString("name");
            DllPath = reader.ReadString("dllpath");
            Server = reader.ReadString("server");
            ActivationType = reader.ReadEnum<ActivationType>("type");
            Permissions = reader.ReadString("perms");
            TrustLevel = reader.ReadEnum<TrustLevel>("trust");
            Threading = reader.ReadInt("thread");
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteGuid("clsid", Clsid);
            writer.WriteAttributeString("name", Name);
            writer.WriteOptionalAttributeString("dllpath", DllPath);
            writer.WriteOptionalAttributeString("server", Server);
            writer.WriteEnum("type", ActivationType);
            writer.WriteEnum("trust", TrustLevel);
            writer.WriteOptionalAttributeString("perms", Permissions);
            writer.WriteInt("thread", Threading);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMRuntimeClassEntry right = obj as COMRuntimeClassEntry;
            if (right == null)
            {
                return false;
            }

            return Clsid == right.Clsid && Name == right.Name && DllPath == right.DllPath && Server == right.Server
                && ActivationType == right.ActivationType && TrustLevel == right.TrustLevel &&
                Permissions == right.Permissions && Threading == right.Threading;
        }

        public override int GetHashCode()
        {
            return Clsid.GetHashCode() ^ Name.GetSafeHashCode() ^ DllPath.GetSafeHashCode()
                ^ Server.GetSafeHashCode() ^ ActivationType.GetHashCode() ^ TrustLevel.GetHashCode()
                ^ Permissions.GetSafeHashCode() & Threading.GetHashCode();
        }
    }
}
