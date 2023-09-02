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

using System;
using System.Xml;
using Microsoft.Win32;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace OleViewDotNet.Database
{
    public class COMProgIDEntry : IComparable<COMProgIDEntry>, IXmlSerializable, IComGuid
    {
        private readonly COMRegistry m_registry;

        public COMProgIDEntry(COMRegistry registry, 
            string progid, Guid clsid, RegistryKey rootKey) : this(registry)
        {
            Clsid = clsid;
            ProgID = progid;
            Name = rootKey.GetValue(null, string.Empty).ToString();
            Source = rootKey.GetSource();
        }

        internal COMProgIDEntry(COMRegistry registry,
            ActCtxComProgIdRedirection progid_redirection)
        {
            Clsid = progid_redirection.Clsid;
            ProgID = progid_redirection.ProgId;
            Name = ProgID;
            Source = COMRegistryEntrySource.ActCtx;
        }

        internal COMProgIDEntry(COMRegistry registry,
            string progid, Guid clsid, COMPackagedClassEntry classEntry) : this(registry)
        {
            Clsid = clsid;
            ProgID = progid;
            Name = classEntry.DisplayName;
            Source = COMRegistryEntrySource.Packaged;
        }

        internal COMProgIDEntry(COMRegistry registry)
        {
            m_registry = registry;
        }

        public int CompareTo(COMProgIDEntry right)
        {
            return string.Compare(ProgID, right.ProgID);
        }

        public string ProgID { get; private set; }

        public Guid Clsid { get; private set; }

        public COMCLSIDEntry ClassEntry
        {
            get
            {
                return m_registry.Clsids.GetGuidEntry(Clsid);
            }
        }

        public string Name { get; private set; }

        public COMRegistryEntrySource Source { get; private set; }

        Guid IComGuid.ComGuid => Clsid;

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMProgIDEntry right = obj as COMProgIDEntry;
            if (right == null)
            {
                return false;
            }

            return ProgID == right.ProgID && Name == right.Name && Clsid == right.Clsid
                    && Source == right.Source;
        }

        public override int GetHashCode()
        {
            return ProgID.GetSafeHashCode() ^ Name.GetSafeHashCode() ^ Clsid.GetHashCode()
                ^ Source.GetHashCode();
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            ProgID = reader.ReadString("progid");
            Clsid = reader.ReadGuid("clsid");
            Name = reader.ReadString("name");
            Source = reader.ReadEnum<COMRegistryEntrySource>("src");
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteOptionalAttributeString("progid", ProgID);
            writer.WriteGuid("clsid", Clsid);
            writer.WriteOptionalAttributeString("name", Name);
            writer.WriteEnum("src", Source);
        }
    }
}
