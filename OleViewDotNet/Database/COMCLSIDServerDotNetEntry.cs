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
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using OleViewDotNet.Utilities;

namespace OleViewDotNet.Database;

public class COMCLSIDServerDotNetEntry : IXmlSerializable
{
    public string AssemblyName { get; private set; }
    public string ClassName { get; private set; }
    public string CodeBase { get; private set; }
    public string RuntimeVersion { get; private set; }

    internal COMCLSIDServerDotNetEntry()
    {
    }

    internal COMCLSIDServerDotNetEntry(RegistryKey key)
    {
        AssemblyName = key.ReadString(null, "Assembly");
        ClassName = key.ReadString(null, "Class");
        CodeBase = key.ReadString(null, "CodeBase");
        RuntimeVersion = key.ReadString(null, "RuntimeVersion");
    }

    XmlSchema IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        AssemblyName = reader.ReadString("asm");
        ClassName = reader.ReadString("cls");
        CodeBase = reader.ReadString("code");
        RuntimeVersion = reader.ReadString("ver");
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteOptionalAttributeString("asm", AssemblyName);
        writer.WriteOptionalAttributeString("cls", ClassName);
        writer.WriteOptionalAttributeString("code", CodeBase);
        writer.WriteOptionalAttributeString("ver", RuntimeVersion);
    }

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMCLSIDServerDotNetEntry right)
        {
            return false;
        }

        return AssemblyName.Equals(right.AssemblyName) && ClassName.Equals(right.ClassName) 
            && CodeBase.Equals(right.CodeBase) && RuntimeVersion.Equals(right.RuntimeVersion);
    }

    public override int GetHashCode()
    {
        return AssemblyName.GetHashCode() ^ ClassName.GetHashCode()
            ^ CodeBase.GetHashCode() ^ RuntimeVersion.GetHashCode();
    }
}
