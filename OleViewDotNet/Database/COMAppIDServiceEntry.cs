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
using NtApiDotNet;
using NtApiDotNet.Win32;
using OleViewDotNet.Interop;
using OleViewDotNet.Security;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
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

    // From shared svchost configuration.
    public string ServiceHostType { get; private set; }
    public bool InitializeSecurity { get; private set; }
    public bool AllowLowBox { get; private set; }
    public bool AllowInteractiveUsers { get; private set; }
    public bool AllowComCapability { get; private set; }
    public bool AllowCrossContainer { get; private set; }
    public EOLE_AUTHENTICATION_CAPABILITIES AuthenticationCapabilities { get; private set; }
    public RPC_AUTHN_LEVEL AuthenticationLevel { get; private set; }
    public RPC_IMP_LEVEL ImpersonationLevel { get; private set; }
    public Guid AppID { get; private set; }
    public GLOBALOPT_UNMARSHALING_POLICY_VALUES UnmarshallingPolicy { get; private set; }
    public COMSecurityDescriptor AccessPermissions { get; private set; }

    private COMSecurityDescriptor GetAccessPermissions(Win32Service service)
    {
        if (!COMSid.TryParse(service.UserName, out COMSid user))
        {
            user = new COMSid(KnownSids.Null);
        }

        SecurityDescriptor sd = new();
        sd.Owner = new SecurityDescriptorSid(user.Sid, false);
        sd.Group = new SecurityDescriptorSid(user.Sid, false);

        AccessMask mask = COMAccessRights.Execute | COMAccessRights.ExecuteLocal | COMAccessRights.ExecuteRemote;
        AccessMask con_mask = mask | (AllowCrossContainer ? COMAccessRights.ExecuteContainer : 0);

        sd.AddAccessAllowedAce(mask, KnownSids.AuthenticatedUsers);
        if (AllowLowBox)
        {
            sd.AddAccessAllowedAce(con_mask, KnownSids.AllApplicationPackages);
        }
        if (AllowInteractiveUsers)
        {
            sd.AddAccessAllowedAce(con_mask, KnownSids.Interactive);
        }
        if (AllowComCapability)
        {
            sd.AddAccessAllowedAce(mask, NtSecurity.GetCapabilitySid("lpacCom"));
        }

        return new COMSecurityDescriptor(sd);
    }

    private void InitSharedSecurity(Win32Service service)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(service.ServiceHostType))
                return;
            ServiceHostType = service.ServiceHostType;
            using var key = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Svchost\{service.ServiceHostType}");
            if (key is null)
                return;
            InitializeSecurity = key.ReadBool("CoInitializeSecurityParam");
            if (!InitializeSecurity)
                return;
            AllowLowBox = key.ReadBool("CoInitializeSecurityAllowLowBox");
            AllowInteractiveUsers = key.ReadBool("CoInitializeSecurityAllowInteractiveUsers");
            AllowComCapability = key.ReadBool("CoInitializeSecurityAllowComCapability");
            AllowCrossContainer = key.ReadBool("CoInitializeSecurityAllowCrossContainer");
            AppID = key.ReadGuid(null, "CoInitializeSecurityAppID");
            AuthenticationCapabilities = (EOLE_AUTHENTICATION_CAPABILITIES)key.ReadInt(null, "AuthenticationCapabilities");
            AuthenticationLevel = (RPC_AUTHN_LEVEL)key.ReadInt(null, "AuthenticationLevel");
            ImpersonationLevel = (RPC_IMP_LEVEL)key.ReadInt(null, "ImpersonationLevel");
            AccessPermissions = key.ReadSecurityDescriptor("COMAccessPermissionsSD") ?? GetAccessPermissions(service);
        }
        catch
        {
        }
    }

    internal COMAppIDServiceEntry(Win32Service service)
    {
        DisplayName = service.DisplayName;
        Name = service.Name;
        ServiceType = service.ServiceType;
        ServiceDll = service.ServiceDll;
        ImagePath = service.ImagePath;
        UserName = service.UserName;
        ProtectionLevel = service.LaunchProtected;
        InitSharedSecurity(service);
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
        ServiceHostType = reader.ReadString("svchost");
        AllowLowBox = reader.ReadBool("lb");
        AllowInteractiveUsers = reader.ReadBool("iu");
        AllowCrossContainer = reader.ReadBool("con");
        AllowComCapability = reader.ReadBool("cap");
        InitializeSecurity = reader.ReadBool("initsec");
        AuthenticationCapabilities = reader.ReadEnum<EOLE_AUTHENTICATION_CAPABILITIES>("authc");
        AuthenticationLevel = reader.ReadEnum<RPC_AUTHN_LEVEL>("authl");
        ImpersonationLevel = reader.ReadEnum<RPC_IMP_LEVEL>("impl");
        UnmarshallingPolicy = reader.ReadEnum<GLOBALOPT_UNMARSHALING_POLICY_VALUES>("unmar");
        AccessPermissions = reader.ReadSecurityDescriptor("sd");
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

        writer.WriteOptionalAttributeString("svchost", ServiceHostType);
        writer.WriteBool("lb", AllowLowBox);
        writer.WriteBool("iu", AllowInteractiveUsers);
        writer.WriteBool("con", AllowCrossContainer);
        writer.WriteBool("cap", AllowComCapability);
        writer.WriteBool("initsec", InitializeSecurity);
        writer.WriteEnum("authc", AuthenticationCapabilities);
        writer.WriteEnum("authl", AuthenticationLevel);
        writer.WriteEnum("impl", ImpersonationLevel);
        writer.WriteEnum("unmar", UnmarshallingPolicy);
        writer.WriteSecurityDescriptor("sd", AccessPermissions);
    }

    public override string ToString()
    {
        return DisplayName;
    }

    public override bool Equals(object obj)
    {
        return obj is COMAppIDServiceEntry entry &&
               DisplayName == entry.DisplayName &&
               Name == entry.Name &&
               ServiceType == entry.ServiceType &&
               UserName == entry.UserName &&
               ImagePath == entry.ImagePath &&
               ServiceDll == entry.ServiceDll &&
               ProtectionLevel == entry.ProtectionLevel &&
               ServiceHostType == entry.ServiceHostType &&
               InitializeSecurity == entry.InitializeSecurity &&
               AllowLowBox == entry.AllowLowBox &&
               AllowInteractiveUsers == entry.AllowInteractiveUsers &&
               AllowComCapability == entry.AllowComCapability &&
               AllowCrossContainer == entry.AllowCrossContainer &&
               AuthenticationCapabilities == entry.AuthenticationCapabilities &&
               AuthenticationLevel == entry.AuthenticationLevel &&
               ImpersonationLevel == entry.ImpersonationLevel &&
               AppID.Equals(entry.AppID) &&
               UnmarshallingPolicy == entry.UnmarshallingPolicy &&
               EqualityComparer<COMSecurityDescriptor>.Default.Equals(AccessPermissions, entry.AccessPermissions);
    }

    public override int GetHashCode()
    {
        int hashCode = -1441314175;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DisplayName);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + ServiceType.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UserName);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ImagePath);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ServiceDll);
        hashCode = hashCode * -1521134295 + ProtectionLevel.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ServiceHostType);
        hashCode = hashCode * -1521134295 + InitializeSecurity.GetHashCode();
        hashCode = hashCode * -1521134295 + AllowLowBox.GetHashCode();
        hashCode = hashCode * -1521134295 + AllowInteractiveUsers.GetHashCode();
        hashCode = hashCode * -1521134295 + AllowComCapability.GetHashCode();
        hashCode = hashCode * -1521134295 + AllowCrossContainer.GetHashCode();
        hashCode = hashCode * -1521134295 + AuthenticationCapabilities.GetHashCode();
        hashCode = hashCode * -1521134295 + AuthenticationLevel.GetHashCode();
        hashCode = hashCode * -1521134295 + ImpersonationLevel.GetHashCode();
        hashCode = hashCode * -1521134295 + AppID.GetHashCode();
        hashCode = hashCode * -1521134295 + UnmarshallingPolicy.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<COMSecurityDescriptor>.Default.GetHashCode(AccessPermissions);
        return hashCode;
    }

    public static bool operator ==(COMAppIDServiceEntry left, COMAppIDServiceEntry right)
    {
        return EqualityComparer<COMAppIDServiceEntry>.Default.Equals(left, right);
    }

    public static bool operator !=(COMAppIDServiceEntry left, COMAppIDServiceEntry right)
    {
        return !(left == right);
    }
}
