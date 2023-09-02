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
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet.Database
{
    [Flags]
    public enum COMAppIDFlags
    {
        None = 0,
        ActivateIUServerInDesktop = 0x1,
        SecureServerProcessSDAndBind = 0x2,
        IssueActivationRpcAtIdentify = 0x4,
        IUServerUnmodifiedLogonToken = 0x8,
        IUServerSelfSidInLaunchPermission = 0x10,
        IUServerActivateInClientSessionOnly = 0x20,
        Reserved1 = 0x40,
        RequireSideLoadedPackage = 0x80,
        SessionVirtualAccountServer = 0x100,
        IUServerUnmodifiedClientLogonTokenUser = 0x200,
        IUServerUnmodifiedSessionLogonTokenUser = 0x400,
        AAANoImplicitActivateAsIU = 0x800,
        PerAppBrokerSuspension = 0x1000,
        SuspensionManaged = 0x2000
    }

    [Flags]
    public enum COMAppIDRotFlags
    {
        None = 0,
        AllowAnyClient = 1,
    }

    public enum ServiceProtectionLevel
    {
        None = 0,
        Windows = 1,
        WindowsLight = 2,
        AntimalwareLight = 3
    }

    public enum PreferredServerBitness
    {
        DEFAULT = 0x0,
        FOLLOW_CLIENT = 0x1,
        X86 = 0x2,
        X64 = 0x3,
        ARM32 = 0x4,
        ARM64 = 0x5,
        NATIVE = unchecked((int)0x80000000)
    };

    public class COMAppIDServiceEntry : IXmlSerializable
    {
        public string DisplayName { get; private set; }
        public string Name { get; private set; }
        public ServiceType ServiceType { get; private set; }
        public string UserName { get; private set; }
        public string ImagePath { get; private set; }
        public string ServiceDll { get; private set; }
        public ServiceProtectionLevel ProtectionLevel { get; private set; }

        internal COMAppIDServiceEntry(RegistryKey key, 
            ServiceController service)
        {
            DisplayName = service.DisplayName;
            Name = service.ServiceName;
            ServiceType = service.ServiceType;
            ServiceDll = string.Empty;
            ImagePath = string.Empty;
            UserName = string.Empty;
            if (key != null)
            {
                UserName = key.ReadString(null, "ObjectName");
                ImagePath = key.ReadString(null, "ImagePath");
                ServiceDll = key.ReadString("Parameters", "ServiceDll");
                if (string.IsNullOrEmpty(ServiceDll))
                {
                    ServiceDll = key.ReadString(null, "ServiceDll");
                }
                ProtectionLevel = (ServiceProtectionLevel) COMUtilities.ReadInt(key, null, "LaunchProtected");
            }
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
            ProtectionLevel = reader.ReadEnum<ServiceProtectionLevel>("prot");
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

            COMAppIDServiceEntry right = obj as COMAppIDServiceEntry;
            if (right == null)
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

    public class COMAppIDEntry : IComparable<COMAppIDEntry>, IXmlSerializable, ICOMAccessSecurity, IComGuid
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
            AccessPermission = string.Empty;
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
            string name = key.GetValue(null) as string;
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name.ToString();
            }
            else
            {
                Name = AppId.FormatGuidDefault();
            }

            AccessPermission = key.ReadSddl("AccessPermission");
            LaunchPermission = key.ReadSddl("LaunchPermission");

            DllSurrogate = key.GetValue("DllSurrogate") as string;
            if (DllSurrogate != null)
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
            if (flags != null)
            {
                Flags = (COMAppIDFlags)flags;
            }

            string local_service = key.GetValue("LocalService") as string;

            if (!string.IsNullOrWhiteSpace(local_service))
            {
                try
                {
                    using (RegistryKey serviceKey = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\" + local_service))
                    {
                        using (ServiceController service = new ServiceController(local_service))
                        {
                            LocalService = new COMAppIDServiceEntry(serviceKey, service);
                        }
                    }
                }
                catch
                {
                }
            }

            if (string.IsNullOrWhiteSpace(RunAs))
            {
                RunAs = string.Empty;
            }

            object rotflags = key.GetValue("ROTFlags");
            if (rotflags != null && rotflags is int)
            {
                RotFlags = (COMAppIDRotFlags)rotflags;
            }

            object bitness = key.GetValue("PreferredServerBitness");
            if (bitness != null && bitness is int)
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

        public bool HasDllSurrogate { get { return !string.IsNullOrWhiteSpace(DllSurrogate); } }

        public COMAppIDServiceEntry LocalService { get; private set; }

        public string RunAs { get; private set; }

        public bool HasRunAs { get { return !String.IsNullOrWhiteSpace(RunAs); } }

        public string Name { get; private set; }

        public COMAppIDFlags Flags { get; private set; }

        public bool IsService
        {
            get { return LocalService != null; }
        }

        public string ServiceName
        {
            get
            {
                if (IsService)
                {
                    return LocalService.Name;
                }
                return string.Empty;
            }
        }

        public ServiceProtectionLevel ServiceProtectionLevel
        {
            get
            {
                if (IsService)
                {
                    return LocalService.ProtectionLevel;
                }
                return ServiceProtectionLevel.None;
            }
        }

        public string LaunchPermission
        {
            get; private set;
        }

        public string AccessPermission
        {
            get; private set;
        }

        public bool HasLaunchPermission
        {
            get { return !String.IsNullOrWhiteSpace(LaunchPermission); }
        }

        public bool HasAccessPermission
        {
            get { return !String.IsNullOrWhiteSpace(AccessPermission); }
        }

        public bool HasPermission
        {
            get { return HasLaunchPermission || HasAccessPermission; }
        }

        public bool HasLowILAccess
        {
            get { return COMSecurity.GetILForSD(AccessPermission) <= TokenIntegrityLevel.Low; }
        }

        public bool HasLowILLaunch
        {
            get { return COMSecurity.GetILForSD(LaunchPermission) <= TokenIntegrityLevel.Low; }
        }

        public bool HasACAccess
        {
            get { return COMSecurity.SDHasAC(AccessPermission); }
        }

        public bool HasACLaunch
        {
            get { return COMSecurity.SDHasAC(LaunchPermission); }
        }

        public bool HasRemoteAccess
        {
            get { return COMSecurity.SDHasRemoteAccess(AccessPermission); }
        }

        public bool HasRemoteLaunch
        {
            get { return COMSecurity.SDHasRemoteAccess(LaunchPermission); }
        }

        public COMAppIDRotFlags RotFlags
        {
            get; private set;
        }

        public IEnumerable<COMCLSIDEntry> ClassEntries
        {
            get
            {
                if (Database.ClsidsByAppId.ContainsKey(AppId))
                {
                    return Database.ClsidsByAppId[AppId];
                }
                return new COMCLSIDEntry[0];
            }
        }

        public PreferredServerBitness PreferredServerBitness
        {
            get; private set;
        }

        public COMRegistryEntrySource Source { get; private set; }

        internal COMRegistry Database { get; }

        string ICOMAccessSecurity.DefaultAccessPermission => Database.DefaultAccessPermission;

        string ICOMAccessSecurity.DefaultLaunchPermission => Database.DefaultLaunchPermission;

        Guid IComGuid.ComGuid => AppId;

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

            COMAppIDEntry right = obj as COMAppIDEntry;
            if (right == null)
            {
                return false;
            }

            if (LocalService != null)
            {
                if (!LocalService.Equals(right.LocalService))
                {
                    return false;
                }
            }
            else if (right.LocalService != null)
            {
                return false;
            }

            return AppId == right.AppId && DllSurrogate == right.DllSurrogate && RunAs == right.RunAs && Name == right.Name && Flags == right.Flags
                && LaunchPermission == right.LaunchPermission && AccessPermission == right.AccessPermission && RotFlags == right.RotFlags 
                && PreferredServerBitness == right.PreferredServerBitness && Source == right.Source;
        }

        public override int GetHashCode()
        {
            return AppId.GetHashCode() ^ DllSurrogate.GetSafeHashCode()
                ^ RunAs.GetSafeHashCode() ^ Name.GetSafeHashCode() ^ Flags.GetHashCode() ^
                LaunchPermission.GetSafeHashCode() ^ AccessPermission.GetSafeHashCode() ^
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
            LaunchPermission = reader.ReadString("launchperm");
            AccessPermission = reader.ReadString("accessperm");
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
            writer.WriteOptionalAttributeString("launchperm", LaunchPermission);
            writer.WriteOptionalAttributeString("accessperm", AccessPermission);
            writer.WriteEnum("rot", RotFlags);
            writer.WriteEnum("bit", PreferredServerBitness);
            writer.WriteEnum("src", Source);
            if (LocalService != null)
            {
                writer.WriteBool("service", true);
                writer.WriteSerializableObjects("service", new COMAppIDServiceEntry[] { LocalService });
            }
        }
    }
}
