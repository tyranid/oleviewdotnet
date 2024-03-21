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
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet.Database;

public class COMRuntimeExtensionEntry : IXmlSerializable
{
    #region Private Members
    private readonly COMRegistry m_registry;

    private void LoadFromKey(RegistryKey key)
    {
        var custom_properties = new Dictionary<string, string>();
        using (var prop_key = key.OpenSubKeySafe("CustomProperties"))
        {
            if (prop_key is not null)
            {
                foreach (var value_name in prop_key.GetValueNames())
                {
                    custom_properties[value_name] = prop_key.GetValue(value_name).ToString();
                }
            }
        }
        CustomProperties = custom_properties;
        Description = key.ReadString(null, "Description");
        DisplayName = key.ReadString(null, "DisplayName");
        Icon = key.ReadString(null, "Icon");
        Vendor = key.ReadString(null, "Vendor");
    }

    #endregion

    #region Constructors
    internal COMRuntimeExtensionEntry(COMRegistry registry)
    {
        m_registry = registry;
    }

    internal COMRuntimeExtensionEntry(string package_id, string contract_id, string id, 
        RegistryKey key, COMRegistry registry) : this(registry)
    {
        PackageId = package_id;
        ContractId = contract_id;
        AppId = id;
        LoadFromKey(key);
        Source = key.GetSource();
    }

    #endregion

    #region XML Serialization
    XmlSchema IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        PackageId = reader.ReadString("pkg");
        ContractId = reader.ReadString("contract");
        AppId = reader.ReadString("appid");
        Description = reader.ReadString("desc");
        DisplayName = reader.ReadString("name");
        Icon = reader.ReadString("icon");
        Vendor = reader.ReadString("vend");
        Source = reader.ReadEnum<COMRegistryEntrySource>("src");
        CustomProperties = reader.ReadDictionary("props");
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteOptionalAttributeString("pkg", PackageId);
        writer.WriteOptionalAttributeString("contract", ContractId);
        writer.WriteOptionalAttributeString("appid", AppId);
        writer.WriteOptionalAttributeString("desc", Description);
        writer.WriteOptionalAttributeString("name", DisplayName);
        writer.WriteOptionalAttributeString("icon", Icon);
        writer.WriteOptionalAttributeString("vend", Vendor);
        writer.WriteEnum("src", Source);
        writer.WriteDictionary(CustomProperties, "props");
    }
    #endregion

    #region Public Methods
    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMRuntimeExtensionEntry right)
        {
            return false;
        }

        return AppId == right.AppId && PackageId == right.PackageId && ContractId == right.ContractId && Description == right.Description
            && DisplayName == right.DisplayName && Icon == right.Icon && Vendor == right.Vendor && MiscUtilities.EqualsDictionary(CustomProperties, right.CustomProperties)
            && Source == right.Source;
    }

    public override int GetHashCode()
    {
        return AppId.GetHashCode() ^ PackageId.GetHashCode() ^ ContractId.GetHashCode() ^ Description.GetHashCode()
            ^ DisplayName.GetHashCode() ^ Icon.GetHashCode() ^ Vendor.GetHashCode() ^ MiscUtilities.GetHashCodeDictionary(CustomProperties)
            ^ Source.GetHashCode();
    }

    public override string ToString()
    {
        return $"{ContractId}!{PackageId}+{AppId}";
    }
    #endregion

    #region Public Properties
    public string PackageId { get; private set; }
    public string PackageName => Package?.Name ?? string.Empty;
    public AppxPackageName Package => AppxPackageName.FromFullName(PackageId);
    public string ContractId { get; private set; }
    public string AppId { get; private set; }
    public string Description { get; private set; }
    public string DisplayName { get; private set; }
    public string Icon { get; private set; }
    public string Vendor { get; private set; }
    public IReadOnlyDictionary<string, string> CustomProperties { get; private set; }
    public string Protocol
    {
        get
        {
            if (ContractId.Equals("windows.protocol", StringComparison.OrdinalIgnoreCase) 
                && CustomProperties.ContainsKey("Name"))
            {
                return CustomProperties["Name"];
            }
            return string.Empty;
        }
    }
    public COMRegistryEntrySource Source { get; private set; }

    #endregion
}
