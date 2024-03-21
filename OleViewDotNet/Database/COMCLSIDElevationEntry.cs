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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using OleViewDotNet.Utilities;

namespace OleViewDotNet.Database;

public class COMCLSIDElevationEntry : IXmlSerializable
{
    public bool Enabled { get; private set; }
    public string IconReference { get; private set; }
    public IEnumerable<Guid> VirtualServerObjects { get; private set; }
    public bool AutoApproval { get; private set; }

    internal COMCLSIDElevationEntry(RegistryKey key, RegistryKey vso_key, bool auto_approval)
    {
        Enabled = key.ReadInt(null, "Enabled") != 0;
        IconReference = key.ReadString(null, "IconReference");
        HashSet<Guid> vsos = new();
        if (vso_key is not null)
        {
            foreach (string value in vso_key.GetValueNames())
            {
                if (Guid.TryParse(value, out Guid guid))
                {
                    vsos.Add(guid);
                }
            }
        }
        AutoApproval = auto_approval;
        VirtualServerObjects = new List<Guid>(vsos).AsReadOnly();
    }

    internal COMCLSIDElevationEntry()
    {
    }

    XmlSchema IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        Enabled = reader.ReadBool("enabled");
        AutoApproval = reader.ReadBool("auto");
        IconReference = reader.ReadString("icon");
        VirtualServerObjects = reader.ReadGuids("vsos");
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteBool("enabled", Enabled);
        writer.WriteBool("auto", AutoApproval);
        writer.WriteOptionalAttributeString("icon", IconReference);
        writer.WriteGuids("vsos", VirtualServerObjects);
    }

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMCLSIDElevationEntry right)
        {
            return false;
        }

        return Enabled == right.Enabled && IconReference == right.IconReference 
            && AutoApproval == right.AutoApproval && VirtualServerObjects.SequenceEqual(right.VirtualServerObjects);
    }

    public override int GetHashCode()
    {
        return Enabled.GetHashCode() ^ IconReference.GetHashCode() 
            ^ AutoApproval.GetHashCode() ^ VirtualServerObjects.GetEnumHashCode();
    }
}
