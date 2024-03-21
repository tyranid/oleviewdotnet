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
using OleViewDotNet.Security;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet.Database;

public class COMAppIDEntry : IComparable<COMAppIDEntry>, IXmlSerializable, ICOMAccessSecurity, ICOMGuid
{
    internal COMAppIDEntry(Guid appId, RegistryKey key, COMRegistry registry) : this(registry)
    {
        AppId = appId;
        LoadFromKey(key);
    }

    internal COMAppIDEntry(COMPackagedServerEntry server, COMRegistry registry) : this(registry)
    {
        AppId = server.SurrogateAppId;
        RunAs = string.Empty;
        Name = server.DisplayName;
        LaunchPermission = server.LaunchAndActivationPermission;
        Source = COMRegistryEntrySource.Packaged;
        DllSurrogate = server.Executable;
        if (string.IsNullOrWhiteSpace(DllSurrogate))
        {
            DllSurrogate = "dllhost.exe";
        }
    }

    private void LoadFromKey(RegistryKey key)
    {
        RunAs = key.GetValue("RunAs") as string;
        if (string.IsNullOrWhiteSpace(RunAs))
        {
            RunAs = string.Empty;
        }

        string name = key.GetValue(null) as string;
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.ToString();
        }
        else
        {
            Name = AppId.FormatGuidDefault();
        }

        RegistryAccessPermission = key.ReadSecurityDescriptor("AccessPermission");
        LaunchPermission = key.ReadSecurityDescriptor("LaunchPermission");

        DllSurrogate = key.GetValue("DllSurrogate") as string;
        if (DllSurrogate is not null)
        {
            if (string.IsNullOrWhiteSpace(DllSurrogate))
            {
                DllSurrogate = "dllhost.exe";
            }
            else
            {
                string dllexe = key.GetValue("DllSurrogateExecutable") as string;
                if (!string.IsNullOrWhiteSpace(dllexe))
                {
                    DllSurrogate = dllexe;
                }
            }
        }
        else
        {
            DllSurrogate = string.Empty;
        }

        object flags = key.GetValue("AppIDFlags");
        if (flags is not null)
        {
            Flags = (COMAppIDFlags)flags;
        }

        string local_service = key.GetValue("LocalService") as string;

        if (!string.IsNullOrWhiteSpace(local_service))
        {
            try
            {
                var service = ServiceUtils.GetService(local_service);
                LocalService = new COMAppIDServiceEntry(service);
            }
            catch
            {
            }
        }

        object rotflags = key.GetValue("ROTFlags");
        if (rotflags is not null && rotflags is int)
        {
            RotFlags = (COMAppIDRotFlags)rotflags;
        }

        object bitness = key.GetValue("PreferredServerBitness");
        if (bitness is not null && bitness is int)
        {
            PreferredServerBitness = (PreferredServerBitness)bitness;
        }

        Source = key.GetSource();
    }

    public int CompareTo(COMAppIDEntry other)
    {
        return AppId.CompareTo(other.AppId);
    }

    public Guid AppId { get; private set; }

    public string DllSurrogate { get; private set; }

    public bool HasDllSurrogate => !string.IsNullOrWhiteSpace(DllSurrogate);

    public COMAppIDServiceEntry LocalService { get; private set; }

    public string RunAs { get; private set; }

    public bool HasRunAs => !string.IsNullOrWhiteSpace(RunAs);

    public bool IsInteractiveUser => RunAs?.Equals("Interactive User") ?? false;

    public string Name { get; private set; }

    public COMAppIDFlags Flags { get; private set; }

    public bool IsService => LocalService is not null;

    public string ServiceName => IsService ? LocalService.Name : string.Empty;

    public ServiceLaunchProtectedType ServiceProtectionLevel => IsService ? LocalService.ProtectionLevel : ServiceLaunchProtectedType.None;

    public COMSecurityDescriptor LaunchPermission
    {
        get; private set;
    }

    public COMSecurityDescriptor AccessPermission => LocalService?.AccessPermissions ?? RegistryAccessPermission;

    public COMSecurityDescriptor RegistryAccessPermission
    {
        get; private set;
    }

    public bool HasLaunchPermission => LaunchPermission is not null;

    public bool HasAccessPermission => AccessPermission is not null;

    public bool HasPermission => HasLaunchPermission || HasAccessPermission;

    public bool HasLowILAccess => COMSecurity.GetILForSD(AccessPermission) <= TokenIntegrityLevel.Low;

    public bool HasLowILLaunch => COMSecurity.GetILForSD(LaunchPermission) <= TokenIntegrityLevel.Low;

    public bool HasACAccess =>  COMSecurity.SDHasAC(AccessPermission);

    public bool HasACLaunch => COMSecurity.SDHasAC(LaunchPermission);

    public bool HasRemoteAccess => COMSecurity.SDHasRemoteAccess(AccessPermission);

    public bool HasRemoteLaunch => COMSecurity.SDHasRemoteAccess(LaunchPermission);

    public bool HasContainerAccess => AccessPermission?.HasContainerAccess ?? false;

    public bool HasContainerLaunch => LaunchPermission?.HasContainerAccess ?? false;

    public COMAppIDRotFlags RotFlags
    {
        get; private set;
    }

    public IEnumerable<COMCLSIDEntry> ClassEntries => Database.ClsidsByAppId.ContainsKey(AppId) ? Database.ClsidsByAppId[AppId] : new COMCLSIDEntry[0];

    public PreferredServerBitness PreferredServerBitness
    {
        get; private set;
    }

    public COMRegistryEntrySource Source { get; private set; }

    internal COMRegistry Database { get; }

    COMSecurityDescriptor ICOMAccessSecurity.DefaultAccessPermission => Database.DefaultAccessPermission;

    COMSecurityDescriptor ICOMAccessSecurity.DefaultLaunchPermission => Database.DefaultLaunchPermission;

    Guid ICOMGuid.ComGuid => AppId;

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMAppIDEntry right)
        {
            return false;
        }

        if (LocalService is not null)
        {
            if (!LocalService.Equals(right.LocalService))
            {
                return false;
            }
        }
        else if (right.LocalService is not null)
        {
            return false;
        }

        return AppId == right.AppId && DllSurrogate == right.DllSurrogate && RunAs == right.RunAs && Name == right.Name && Flags == right.Flags
            && LaunchPermission.SDIsEqual(right.LaunchPermission) && AccessPermission.SDIsEqual(right.AccessPermission) && RotFlags == right.RotFlags 
            && PreferredServerBitness == right.PreferredServerBitness && Source == right.Source;
    }

    public override int GetHashCode()
    {
        return AppId.GetHashCode() ^ DllSurrogate.GetSafeHashCode()
            ^ RunAs.GetSafeHashCode() ^ Name.GetSafeHashCode() ^ Flags.GetHashCode() ^
            LaunchPermission.GetSDHashCode() ^ AccessPermission.GetSDHashCode() ^
            LocalService.GetSafeHashCode() ^ RotFlags.GetHashCode() ^ PreferredServerBitness.GetHashCode()
            ^ Source.GetHashCode();
    }

    internal COMAppIDEntry(COMRegistry registry)
    {
        Database = registry;
    }

    XmlSchema IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        AppId = reader.ReadGuid("appid");
        DllSurrogate = reader.ReadString("dllsurrogate");
        RunAs = reader.ReadString("runas");
        Name = reader.ReadString("name");
        string flags = reader.ReadString("flags").Replace("Reserved2", "RequireSideLoadedPackage");
        Flags = (COMAppIDFlags)Enum.Parse(typeof(COMAppIDFlags), flags, true);
        LaunchPermission = reader.ReadSecurityDescriptor("launchperm");
        RegistryAccessPermission = reader.ReadSecurityDescriptor("accessperm");
        RotFlags = reader.ReadEnum<COMAppIDRotFlags>("rot");
        PreferredServerBitness = reader.ReadEnum<PreferredServerBitness>("bit");
        Source = reader.ReadEnum<COMRegistryEntrySource>("src");
        bool has_service = reader.ReadBool("service");
        if (has_service)
        {
            IEnumerable<COMAppIDServiceEntry> service = reader.ReadSerializableObjects<COMAppIDServiceEntry>("service", () => new COMAppIDServiceEntry());
            LocalService = service.FirstOrDefault();
        }
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteGuid("appid", AppId);
        writer.WriteOptionalAttributeString("dllsurrogate", DllSurrogate);
        writer.WriteOptionalAttributeString("runas", RunAs);
        writer.WriteOptionalAttributeString("name", Name);
        writer.WriteOptionalAttributeString("flags", Flags.ToString());
        writer.WriteSecurityDescriptor("launchperm", LaunchPermission);
        writer.WriteSecurityDescriptor("accessperm", RegistryAccessPermission);
        writer.WriteEnum("rot", RotFlags);
        writer.WriteEnum("bit", PreferredServerBitness);
        writer.WriteEnum("src", Source);
        if (LocalService is not null)
        {
            writer.WriteBool("service", true);
            writer.WriteSerializableObjects("service", new COMAppIDServiceEntry[] { LocalService });
        }
    }
}
