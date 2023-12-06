//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014. 2016
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

namespace OleViewDotNet.Database;

public class COMMimeType : IXmlSerializable
{
    private readonly COMRegistry m_registry;

    public string MimeType { get; private set; }
    public Guid Clsid { get; private set; }
    public COMCLSIDEntry ClassEntry => m_registry.Clsids.GetGuidEntry(Clsid);

    public string Extension { get; private set; }
    public string Name => ClassEntry?.Name ?? string.Empty;

    public override string ToString()
    {
        return MimeType;
    }

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMMimeType right)
        {
            return false;
        }

        return MimeType == right.MimeType && 
            Clsid == right.Clsid && Extension == right.Extension;
    }

    public override int GetHashCode()
    {
        return MimeType.GetSafeHashCode() ^ Clsid.GetHashCode() ^ Extension.GetSafeHashCode();
    }

    internal COMMimeType(COMRegistry registry, string mime_type, RegistryKey key) : this(registry)
    {
        string extension = key.GetValue("Extension") as string;
        if ((key.GetValue("CLSID") is string clsid) && Guid.TryParse(clsid, out Guid guid))
        {
            Clsid = guid;
        }
        Extension = extension;
        MimeType = mime_type;
    }

    internal COMMimeType(COMRegistry registry)
    {
        m_registry = registry;
    }

    XmlSchema IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        MimeType = reader.GetAttribute("mimetype");
        Clsid = reader.ReadGuid("clsid");
        Extension = reader.GetAttribute("ext");
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteOptionalAttributeString("mimetype", MimeType);
        writer.WriteGuid("clsid", Clsid);
        writer.WriteOptionalAttributeString("ext", Extension);
    }
}
