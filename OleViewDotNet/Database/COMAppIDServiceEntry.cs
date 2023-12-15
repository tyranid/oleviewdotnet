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

using NtApiDotNet.Win32;
using OleViewDotNet.Utilities;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet.Database;

public class COMAppIDServiceEntry : IXmlSerializable
{
    public string DisplayName { get; private set; }
    public string Name { get; private set; }
    public ServiceType ServiceType { get; private set; }
    public string UserName { get; private set; }
    public string ImagePath { get; private set; }
    public string ServiceDll { get; private set; }
    public ServiceLaunchProtectedType ProtectionLevel { get; private set; }

    internal COMAppIDServiceEntry(Win32Service service)
    {
        DisplayName = service.DisplayName;
        Name = service.Name;
        ServiceType = service.ServiceType;
        ServiceDll = service.ServiceDll;
        ImagePath = service.ImagePath;
        UserName = service.UserName;
        ProtectionLevel = service.LaunchProtected;
    }

    internal COMAppIDServiceEntry()
    {
    }

    XmlSchema IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        DisplayName = reader.ReadString("display");
        Name = reader.ReadString("name");
        ServiceType = reader.ReadEnum<ServiceType>("type");
        UserName = reader.ReadString("user");
        ImagePath = reader.ReadString("path");
        ServiceDll = reader.ReadString("dll");
        ProtectionLevel = reader.ReadEnum<ServiceLaunchProtectedType>("prot");
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteOptionalAttributeString("display", DisplayName);
        writer.WriteOptionalAttributeString("name", Name);
        writer.WriteEnum("type", ServiceType);
        writer.WriteOptionalAttributeString("user", UserName);
        writer.WriteOptionalAttributeString("path", ImagePath);
        writer.WriteOptionalAttributeString("dll", ServiceDll);
        writer.WriteEnum("prot", ProtectionLevel);
    }

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMAppIDServiceEntry right)
        {
            return false;
        }

        return DisplayName == right.DisplayName && Name == right.Name && ServiceType == right.ServiceType && UserName == right.UserName && ProtectionLevel == right.ProtectionLevel;
    }

    public override int GetHashCode()
    {
        return DisplayName.GetSafeHashCode() ^ Name.GetSafeHashCode() ^ ServiceType.GetHashCode() ^ UserName.GetSafeHashCode() ^ ProtectionLevel.GetHashCode();
    }

    public override string ToString()
    {
        return DisplayName;
    }
}
