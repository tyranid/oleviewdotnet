//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
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
using System.Xml.Schema;
using System.Xml.Serialization;
using OleViewDotNet.Utilities;
using OleViewDotNet.Utilities.Format;

namespace OleViewDotNet.Database;

public class COMInterfaceInstance : IXmlSerializable, ICOMSourceCodeFormattable
{
    public Guid Iid { get; private set; }
    public string Module { get; private set; }
    public long VTableOffset { get; private set; }
    public COMRegistry Database { get; }
    public string Name
    {
        get
        {
            if (Database is null || !Database.InterfacesToNames.ContainsKey(Iid))
            {
                return string.Empty;
            }
            return Database.InterfacesToNames[Iid];
        }
    }
    public COMInterfaceEntry InterfaceEntry => Database?.Interfaces.GetGuidEntry(Iid);

    bool ICOMSourceCodeFormattable.IsFormattable => ((ICOMSourceCodeFormattable)InterfaceEntry)?.IsFormattable ?? false;

    public COMInterfaceInstance(Guid iid, string module, long vtable_offset, COMRegistry registry) : this(registry)
    {
        Iid = iid;
        Module = module;
        VTableOffset = vtable_offset;
    }

    public COMInterfaceInstance(Guid iid, COMRegistry registry) : this(iid, null, 0, registry)
    {
    }

    public COMInterfaceInstance(COMRegistry registry)
    {
        Database = registry;
    }

    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(Module))
        {
            return $"{Iid},{Module},{VTableOffset}";
        }
        return Iid.ToString();
    }

    XmlSchema IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        Iid = reader.ReadGuid("iid");
        Module = reader.GetAttribute("mod");
        VTableOffset = reader.ReadLong("ofs");
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteGuid("iid", Iid);
        writer.WriteOptionalAttributeString("mod", Module);
        writer.WriteLong("ofs", VTableOffset);
    }

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        ((ICOMSourceCodeFormattable)InterfaceEntry)?.Format(builder);
    }
}
