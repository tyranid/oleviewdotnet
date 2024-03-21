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
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using OleViewDotNet.Utilities;

namespace OleViewDotNet.Database;

public enum IEElevationPolicy
{
    NoRun = 0,
    RunAtCurrent = 1,
    RunAfterPrompt = 2,
    RunAtMedium = 3,
    BlockCOM = 0x10,
    KillBit = 0x20,
}

public class COMIELowRightsElevationPolicy : IComparable<COMIELowRightsElevationPolicy>, IXmlSerializable
{
    private readonly COMRegistry m_registry;
    
    public string Name { get; private set; }
    public Guid Uuid { get; private set; }
    public Guid Clsid { get; private set; }
    public COMCLSIDEntry ClassEntry => m_registry.MapClsidToEntry(Clsid);
    public string AppPath { get; private set; }
    public IEElevationPolicy Policy { get; private set; }
    public COMRegistryEntrySource Source { get; private set; }

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMIELowRightsElevationPolicy right)
        {
            return false;
        }

        return Name == right.Name && Uuid == right.Uuid && Clsid == right.Clsid
            && AppPath == right.AppPath && Policy == right.Policy && Source == right.Source;
    }

    public override int GetHashCode()
    {
        return Name.GetSafeHashCode() ^ Uuid.GetHashCode() 
            ^ Clsid.GetHashCode() ^ AppPath.GetSafeHashCode() ^ Policy.GetHashCode()
            ^ Source.GetHashCode();
    }

    private static string HandleNulTerminate(string s)
    {
        int index = s.IndexOf('\0');
        if (index >= 0)
        {
            return s.Substring(0, index);
        }
        else
        {
            return s;
        }
    }

    private void LoadFromRegistry(RegistryKey key)
    {
        object policyValue = key.GetValue("Policy", 0);

        if (policyValue is not null && !string.IsNullOrEmpty(policyValue.ToString()))
        {
            Policy = (IEElevationPolicy)Enum.ToObject(typeof(IEElevationPolicy), policyValue);
        }
        
        string clsid = (string)key.GetValue("CLSID");
        if (clsid is not null)
        {

            if (Guid.TryParse(clsid, out Guid cls))
            {
                Clsid = cls;
            }
        }
        
        string appName = (string)key.GetValue("AppName", null);
        string appPath = (string)key.GetValue("AppPath");

        if ((appName is not null) && (appPath is not null))
        {
            try
            {
                Name = HandleNulTerminate(appName);
                AppPath = Path.Combine(HandleNulTerminate(appPath), Name).ToLower();
            }
            catch (ArgumentException)
            {
            }
        }
    }

    public COMIELowRightsElevationPolicy(COMRegistry registry, Guid guid, COMRegistryEntrySource source, RegistryKey key) 
        : this(registry)
    {
        Uuid = guid;
        Name = Uuid.FormatGuidDefault();
        Source = source;
        LoadFromRegistry(key);
    }

    internal COMIELowRightsElevationPolicy(COMRegistry registry)
    {
        m_registry = registry;
    }

    public int CompareTo(COMIELowRightsElevationPolicy other)
    {
        return Uuid.CompareTo(other.Uuid);
    }

    XmlSchema IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        Name = reader.GetAttribute("name");
        Uuid = reader.ReadGuid("uuid");
        Clsid = reader.ReadGuid("clsid");
        AppPath = reader.GetAttribute("path");
        Policy = reader.ReadEnum<IEElevationPolicy>("policy");
        Source = reader.ReadEnum<COMRegistryEntrySource>("src");
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteOptionalAttributeString("name", Name);
        writer.WriteGuid("uuid", Uuid);
        writer.WriteGuid("clsid", Clsid);
        writer.WriteOptionalAttributeString("path", AppPath);
        writer.WriteEnum("policy", Policy);
        writer.WriteEnum("src", Source);
    }
}
