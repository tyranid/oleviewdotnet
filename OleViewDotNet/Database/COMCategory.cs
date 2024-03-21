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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using OleViewDotNet.Utilities;

namespace OleViewDotNet.Database;

public class COMCategory : IXmlSerializable, ICOMGuid
{
    private readonly COMRegistry m_registry;
    
    public Guid CategoryID { get; private set; }
    public string Name { get; private set; }
    public IEnumerable<Guid> Clsids { get; private set; }
    public IEnumerable<COMCLSIDEntry> ClassEntries => Clsids.Select(g => m_registry.MapClsidToEntry(g)).Where(e => e is not null);

    Guid ICOMGuid.ComGuid => CategoryID;

    internal COMCategory(COMRegistry registry, Guid catid, IEnumerable<Guid> clsids) 
        : this(registry)
    {
        CategoryID = catid;
        Clsids = clsids.ToArray();
        Name = COMUtilities.GetCategoryName(catid);
    }

    internal COMCategory(COMRegistry registry)
    {
        m_registry = registry;
    }

    XmlSchema IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        Name = reader.ReadString("name");
        CategoryID = reader.ReadGuid("catid");
        Clsids = reader.ReadGuids("clsids").ToArray();
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteOptionalAttributeString("name", Name);
        writer.WriteGuid("catid", CategoryID);
        writer.WriteGuids("clsids", Clsids);
    }

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMCategory right)
        {
            return false;
        }

        return Clsids.SequenceEqual(right.Clsids) && CategoryID == right.CategoryID && Name == right.Name;
    }

    public override int GetHashCode()
    {
        return CategoryID.GetHashCode() ^ Name.GetSafeHashCode() 
            ^ Clsids.GetEnumHashCode();
    }

    public override string ToString()
    {
        return Name;
    }

    public static readonly Guid CATID_TrustedMarshaler = new("00000003-0000-0000-C000-000000000046");
    public static readonly Guid CATID_SafeForScripting = new("7DD95801-9882-11CF-9FA9-00AA006C42C4");
    public static readonly Guid CATID_SafeForInitializing = new("7DD95802-9882-11CF-9FA9-00AA006C42C4");
    public static readonly Guid CATID_Control = new("{40FC6ED4-2438-11CF-A3DB-080036F12502}");
    public static readonly Guid CATID_Insertable = new("{40FC6ED3-2438-11CF-A3DB-080036F12502}");
    public static readonly Guid CATID_Document = new("{40fc6ed8-2438-11cf-a3db-080036f12502}");
}
