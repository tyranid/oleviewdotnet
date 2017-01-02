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
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace OleViewDotNet
{
    public class COMTypeLibEntry : IComparable<COMTypeLibEntry>, IXmlSerializable
    {
        private List<COMTypeLibVersionEntry> LoadFromKey(RegistryKey key)
        {
            List<COMTypeLibVersionEntry> ret = new List<COMTypeLibVersionEntry>();
            foreach (string version in key.GetSubKeyNames())
            {
                using (RegistryKey subKey = key.OpenSubKey(version))
                {
                    if (subKey != null)
                    {
                        ret.Add(new COMTypeLibVersionEntry(version, TypelibId, subKey));
                    }
                }
            }
            return ret;
        }

        public Guid TypelibId { get; private set; }
        public IEnumerable<COMTypeLibVersionEntry> Versions { get; private set; }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMTypeLibEntry right = obj as COMTypeLibEntry;
            if (right == null)
            {
                return false;
            }

            return TypelibId == right.TypelibId && Versions.SequenceEqual(right.Versions);
        }

        public override int GetHashCode()
        {
            return TypelibId.GetHashCode() ^ Versions.GetEnumHashCode();
        }

        public COMTypeLibEntry(Guid typelibid, RegistryKey rootKey)
        {
            TypelibId = typelibid;
            Versions = LoadFromKey(rootKey).AsReadOnly();
        }

        internal COMTypeLibEntry()
        {
        }

        public int CompareTo(COMTypeLibEntry other)
        {
            return TypelibId.CompareTo(other.TypelibId);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            TypelibId = reader.ReadGuid("libid");
            reader.Read();
            Versions = reader.ReadSerializableObjects("libvers", () => new COMTypeLibVersionEntry(TypelibId));
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteGuid("libid", TypelibId);
            writer.WriteSerializableObjects("libvers", Versions);
        }
    }

    public class COMTypeLibVersionEntry : IXmlSerializable
    {
        public Guid TypelibId { get; private set; }
        public string Version { get; private set; }
        public string Name { get; private set; }
        public string Win32Path { get; private set; }
        public string Win64Path { get; private set; }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMTypeLibVersionEntry right = obj as COMTypeLibVersionEntry;
            if (right == null)
            {
                return false;
            }

            return Version == right.Version && Name == right.Name 
                && Win32Path == right.Win32Path && Win64Path == right.Win64Path;
        }

        public override int GetHashCode()
        {
            return Version.GetSafeHashCode() ^ Name.GetSafeHashCode() ^ Win32Path.GetSafeHashCode() ^ Win64Path.GetSafeHashCode();
        }

        public string NativePath
        {
            get
            {
                if (((Environment.Is64BitProcess) && !String.IsNullOrWhiteSpace(Win64Path)) || String.IsNullOrWhiteSpace(Win32Path))
                {
                    return Win64Path;
                }
                else
                {
                    return Win32Path;
                }

            }
        }

        internal COMTypeLibVersionEntry(string version, Guid typelibid, RegistryKey key) 
            : this(typelibid)
        {
            Version = version;
            Name = key.GetValue(null) as string;
            if (String.IsNullOrWhiteSpace(Name))
            {
                Name = typelibid.ToString();
            }

            // TODO: Correctly handle locale specific typelibs
            // We can't be sure of there being a 0 LCID, leave for now
            using (RegistryKey subKey = key.OpenSubKey("0\\win32"))
            {
                if (subKey != null)
                {
                    Win32Path = subKey.GetValue(null) as string;
                }
            }

            using (RegistryKey subKey = key.OpenSubKey("0\\win64"))
            {
                if (subKey != null)
                {
                    Win64Path = subKey.GetValue(null) as string;
                }
            }
        }

        public COMTypeLibVersionEntry(Guid typelibid)
        {
            TypelibId = typelibid;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Version = reader.GetAttribute("ver");
            Name = reader.GetAttribute("name");
            Win32Path = reader.GetAttribute("win32");
            Win64Path = reader.GetAttribute("win64");
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteOptionalAttributeString("ver", Version);
            writer.WriteOptionalAttributeString("name", Name);
            writer.WriteOptionalAttributeString("win32", Win32Path);
            writer.WriteOptionalAttributeString("win64", Win64Path);
        }
    }
}
