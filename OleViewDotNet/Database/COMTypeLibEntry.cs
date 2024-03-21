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
using OleViewDotNet.Utilities;
using OleViewDotNet.Interop.SxS;

namespace OleViewDotNet.Database;

public class COMTypeLibEntry : IComparable<COMTypeLibEntry>, IXmlSerializable, ICOMGuid
{
    private readonly COMRegistry m_registry;

    private IEnumerable<COMTypeLibVersionEntry> LoadFromLocales(string name, string version, RegistryKey key)
    {
        List<COMTypeLibVersionEntry> entries = new();
        foreach (string locale in key.GetSubKeyNames())
        {
            if (int.TryParse(locale, out int locale_int))
            {
                using RegistryKey subkey = key.OpenSubKey(locale);
                if (subkey is not null)
                {
                    COMTypeLibVersionEntry entry = new(m_registry,
                        name, version, TypelibId, locale_int, subkey);
                    if (!string.IsNullOrWhiteSpace(entry.NativePath))
                    {
                        entries.Add(entry);
                    }
                }
            }
        }
        return entries;
    }

    private List<COMTypeLibVersionEntry> LoadFromKey(RegistryKey key)
    {
        List<COMTypeLibVersionEntry> ret = new();
        foreach (string version in key.GetSubKeyNames())
        {
            using RegistryKey subKey = key.OpenSubKey(version);
            if (subKey is not null)
            {
                ret.AddRange(LoadFromLocales(subKey.GetValue(null, string.Empty).ToString(), version, subKey));
            }
        }
        return ret;
    }

    public Guid TypelibId { get; private set; }
    public IEnumerable<COMTypeLibVersionEntry> Versions { get; private set; }
    public string Name { get; private set; }
    public COMRegistryEntrySource Source { get; private set; }

    Guid ICOMGuid.ComGuid => TypelibId;

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMTypeLibEntry right)
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
        List<COMTypeLibVersionEntry> versions = new();
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
