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

namespace OleViewDotNet.Database
{
    public class COMTypeLibEntry : IComparable<COMTypeLibEntry>, IXmlSerializable, IComGuid
    {
        private readonly COMRegistry m_registry;

        private IEnumerable<COMTypeLibVersionEntry> LoadFromLocales(string name, string version, RegistryKey key)
        {
            List<COMTypeLibVersionEntry> entries = new List<COMTypeLibVersionEntry>();
            foreach (string locale in key.GetSubKeyNames())
            {
                int locale_int;
                if (int.TryParse(locale, out locale_int))
                {
                    using (RegistryKey subkey = key.OpenSubKey(locale))
                    {
                        if (subkey != null)
                        {
                            COMTypeLibVersionEntry entry = new COMTypeLibVersionEntry(m_registry, 
                                name, version, TypelibId, locale_int, subkey);
                            if (!string.IsNullOrWhiteSpace(entry.NativePath))
                            {
                                entries.Add(entry);
                            }
                        }
                    }
                }
            }
            return entries;
        }

        private List<COMTypeLibVersionEntry> LoadFromKey(RegistryKey key)
        {
            List<COMTypeLibVersionEntry> ret = new List<COMTypeLibVersionEntry>();
            foreach (string version in key.GetSubKeyNames())
            {
                using (RegistryKey subKey = key.OpenSubKey(version))
                {
                    if (subKey != null)
                    {
                        ret.AddRange(LoadFromLocales(subKey.GetValue(null, string.Empty).ToString(), version, subKey));
                    }
                }
            }
            return ret;
        }

        public Guid TypelibId { get; private set; }
        public IEnumerable<COMTypeLibVersionEntry> Versions { get; private set; }
        public string Name { get; private set; }
        public COMRegistryEntrySource Source { get; private set; }

        Guid IComGuid.ComGuid => TypelibId;

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

            return TypelibId == right.TypelibId && Versions.SequenceEqual(right.Versions) && Source == right.Source;
        }

        public override int GetHashCode()
        {
            return TypelibId.GetHashCode() ^ Versions.GetEnumHashCode() ^ Source.GetHashCode();
        }

        public COMTypeLibEntry(COMRegistry registry, Guid typelibid, RegistryKey rootKey) : this(registry)
        {
            TypelibId = typelibid;
            Source = rootKey.GetSource();
            Versions = LoadFromKey(rootKey).AsReadOnly();
            Name = Versions.Select(v => v.Name).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? TypelibId.FormatGuid();
        }

        internal COMTypeLibEntry(COMRegistry registry, COMPackagedTypeLibEntry typelib) : this(registry)
        {
            TypelibId = typelib.TypeLibId;
            Source = COMRegistryEntrySource.Packaged;
            Versions = typelib.Versions.Select(v => new COMTypeLibVersionEntry(registry, typelib.TypeLibId, v)).ToList();
            Name = Versions.Select(v => v.Name).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? TypelibId.FormatGuid();
        }

        internal COMTypeLibEntry(COMRegistry registry, ActCtxComTypeLibraryRedirection typelib_redirection) 
            : this(registry)
        {
            TypelibId = typelib_redirection.TypeLibraryId;
            Name = TypelibId.FormatGuid();
            List<COMTypeLibVersionEntry> versions = new List<COMTypeLibVersionEntry>();
            versions.Add(new COMTypeLibVersionEntry(registry, typelib_redirection));
            Versions = versions.AsReadOnly();
            Source = COMRegistryEntrySource.ActCtx;
        }

        internal COMTypeLibEntry(COMRegistry registry)
        {
            m_registry = registry;
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
            Name = reader.ReadString("name");
            Source = reader.ReadEnum<COMRegistryEntrySource>("src");
            reader.Read();
            Versions = reader.ReadSerializableObjects("libvers", () => new COMTypeLibVersionEntry(m_registry, TypelibId));
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteGuid("libid", TypelibId);
            writer.WriteOptionalAttributeString("name", Name);
            writer.WriteEnum("src", Source);
            writer.WriteSerializableObjects("libvers", Versions);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class COMTypeLibVersionEntry : IXmlSerializable, IComGuid
    {
        private readonly COMRegistry m_registry;

        public Guid TypelibId { get; private set; }
        public string Version { get; private set; }
        public string Name { get; private set; }
        public string Win32Path { get; private set; }
        public string Win64Path { get; private set; }
        public int Locale { get; private set; }
        public COMRegistryEntrySource Source { get; private set; }

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
                && Win32Path == right.Win32Path && Win64Path == right.Win64Path && Locale == right.Locale
                && Source == right.Source;
        }

        public override int GetHashCode()
        {
            return Version.GetSafeHashCode() ^ Name.GetSafeHashCode() ^ Win32Path.GetSafeHashCode() ^ Win64Path.GetSafeHashCode() ^ Source.GetHashCode();
        }

        public string NativePath
        {
            get
            {
                if (((Environment.Is64BitProcess) && !string.IsNullOrWhiteSpace(Win64Path)) || string.IsNullOrWhiteSpace(Win32Path))
                {
                    return Win64Path;
                }
                else
                {
                    return Win32Path;
                }

            }
        }

        Guid IComGuid.ComGuid => TypelibId;

        internal COMTypeLibVersionEntry(COMRegistry registry, string name, string version, Guid typelibid, int locale, RegistryKey key) 
            : this(registry, typelibid)
        {
            Version = version;
            Locale = locale;
            Name = name;

            // We can't be sure of there being a 0 LCID, leave for now
            using (RegistryKey subKey = key.OpenSubKey("win32"))
            {
                if (subKey != null)
                {
                    Win32Path = subKey.GetValue(null) as string;
                }
            }

            using (RegistryKey subKey = key.OpenSubKey("win64"))
            {
                if (subKey != null)
                {
                    Win64Path = subKey.GetValue(null) as string;
                }
            }
            Source = key.GetSource();
        }

        internal COMTypeLibVersionEntry(COMRegistry registry, Guid typelibid, COMPackagedTypeLibVersionEntry entry)
            : this(registry, typelibid)
        {
            Version = entry.Version;
            Locale = entry.LocaleId;
            Name = entry.DisplayName;
            Win32Path = entry.Win32Path;
            Win64Path = entry.Win64Path;
            Source = COMRegistryEntrySource.Packaged;
        }

        public COMTypeLibVersionEntry(COMRegistry registry, Guid typelibid)
        {
            m_registry = registry;
            TypelibId = typelibid;
        }

        public COMTypeLibVersionEntry(COMRegistry registry, ActCtxComTypeLibraryRedirection typelib_redirection) 
            : this(registry, typelib_redirection.TypeLibraryId)
        {
            Name = TypelibId.FormatGuid();
            Win32Path = typelib_redirection.FullPath;
            Win64Path = typelib_redirection.FullPath;
            Source = COMRegistryEntrySource.ActCtx;
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
            Locale = reader.ReadInt("locale");
            Source = reader.ReadEnum<COMRegistryEntrySource>("src");
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteOptionalAttributeString("ver", Version);
            writer.WriteOptionalAttributeString("name", Name);
            writer.WriteOptionalAttributeString("win32", Win32Path);
            writer.WriteOptionalAttributeString("win64", Win64Path);
            writer.WriteInt("locale", Locale);
            writer.WriteEnum("src", Source);
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name) ? TypelibId.FormatGuid() : Name;
        }
    }
}
