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
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using OleViewDotNet.Utilities;
using OleViewDotNet.Interop.SxS;
using OleViewDotNet.TypeLib;
using OleViewDotNet.Utilities.Format;
using System.Collections.Generic;

namespace OleViewDotNet.Database;

public class COMTypeLibVersionEntry : IXmlSerializable, ICOMGuid, ICOMSourceCodeFormattable, ICOMSourceCodeParsable
{
    private readonly COMRegistry m_registry;
    private Lazy<COMTypeLib> m_typelib;

    private COMTypeLib ParseInternal()
    {
        var type_lib = COMTypeLib.FromFile(NativePath);
        foreach (var intf in type_lib.Interfaces)
        {
            if (intf.Name != string.Empty)
            {
                m_registry.IidNameCache.TryAdd(intf.Uuid, intf.Name);
            }
        }
        return type_lib;
    }

    public Guid TypelibId { get; private set; }
    public string Version { get; private set; }
    public string Name { get; private set; }
    public string Win32Path { get; private set; }
    public string Win64Path { get; private set; }
    public int Locale { get; private set; }
    public COMRegistryEntrySource Source { get; private set; }
    internal bool IsParsed => m_typelib?.IsValueCreated ?? false;

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMTypeLibVersionEntry right)
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

    public string NativePath => (Environment.Is64BitProcess && !string.IsNullOrWhiteSpace(Win64Path)) || string.IsNullOrWhiteSpace(Win32Path)
                ? Win64Path
                : Win32Path;

    public COMTypeLib Parse()
    {
        m_typelib ??= new(ParseInternal);
        return m_typelib.Value;
    }

    Guid ICOMGuid.ComGuid => TypelibId;

    bool ICOMSourceCodeParsable.IsSourceCodeParsed => IsParsed;

    bool ICOMSourceCodeFormattable.IsFormattable => true;

    internal COMTypeLibVersionEntry(COMRegistry registry, string name, string version, Guid typelibid, int locale, RegistryKey key) 
        : this(registry, typelibid)
    {
        Version = version;
        Locale = locale;
        Name = name;

        // We can't be sure of there being a 0 LCID, leave for now
        using (RegistryKey subKey = key.OpenSubKey("win32"))
        {
            if (subKey is not null)
            {
                Win32Path = subKey.GetValue(null) as string;
            }
        }

        using (RegistryKey subKey = key.OpenSubKey("win64"))
        {
            if (subKey is not null)
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

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        if (IsParsed)
        {
            ((ICOMSourceCodeFormattable)m_typelib.Value).Format(builder);
        }
    }

    void ICOMSourceCodeParsable.ParseSourceCode()
    {
        Parse();
    }
}
