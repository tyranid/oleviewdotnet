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
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Collections.ObjectModel;
using NtApiDotNet;
using OleViewDotNet.Interop;
using OleViewDotNet.Utilities;

namespace OleViewDotNet.Database;

public class COMCLSIDEntry : IComparable<COMCLSIDEntry>, IXmlSerializable, ICOMClassEntry, ICOMAccessSecurity, IComGuid
{
    private List<COMInterfaceInstance> m_interfaces;
    private List<COMInterfaceInstance> m_factory_interfaces;

    private static HashSet<Guid> LoadAppActivatableClsids()
    {
        HashSet<Guid> app_activatable = new();
        using (RegistryKey activatable_key = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\WindowsRuntime\AllowedCOMCLSIDs"))
        {
            if (activatable_key != null)
            {
                foreach (string clsid in activatable_key.GetSubKeyNames())
                {
                    if (Guid.TryParse(clsid, out Guid guid))
                    {
                        app_activatable.Add(guid);
                    }
                }
            }
        }
        return app_activatable;
    }

    private static readonly HashSet<Guid> _app_activatable = LoadAppActivatableClsids();

    public int CompareTo(COMCLSIDEntry right)
    {
        return string.Compare(Name, right.Name);
    }
    
    private COMCLSIDServerEntry ReadServerKey(Dictionary<COMServerType, COMCLSIDServerEntry> servers, RegistryKey key, COMServerType server_type)
    {
        using RegistryKey server_key = key.OpenSubKey(server_type.ToString());
        if (server_key == null)
        {
            return null;
        }

        COMCLSIDServerEntry entry = new(server_key, server_type);
        if (!string.IsNullOrWhiteSpace(entry.Server))
        {
            servers.Add(server_type, new COMCLSIDServerEntry(server_key, server_type));
        }
        return entry;
    }

    private void LoadFromKey(RegistryKey key)
    {
        HashSet<Guid> categories = new();
        object name = key.GetValue(null);
        Name = null;
        if (name != null)
        {
            string s = name.ToString().Trim();

            if (s.Length > 0)
            {
                Name = name.ToString();
            }
        }

        bool fake_name = false;
        if (Name == null)
        {
            fake_name = true;
            Name = Clsid.FormatGuidDefault();
        }

        Dictionary<COMServerType, COMCLSIDServerEntry> servers = new();
        COMCLSIDServerEntry inproc_server = ReadServerKey(servers, key, COMServerType.InProcServer32);
        ReadServerKey(servers, key, COMServerType.LocalServer32);
        ReadServerKey(servers, key, COMServerType.InProcHandler32);
        Servers = new ReadOnlyDictionary<COMServerType, COMCLSIDServerEntry>(servers);

        if (fake_name && inproc_server != null && inproc_server.HasDotNet)
        {
            Name = string.Format("{0}, {1}", inproc_server.DotNet.ClassName, inproc_server.DotNet.AssemblyName);
        }

        AppID = COMUtilities.ReadGuid(key, null, "AppID");
        if (AppID == Guid.Empty)
        {
            AppID = COMUtilities.ReadGuid(Registry.ClassesRoot,
                    string.Format(@"AppID\{0}", COMUtilities.GetFileName(DefaultServer)), "AppID");
        }

        if (AppID != Guid.Empty && !servers.ContainsKey(COMServerType.LocalServer32))
        {
            servers.Add(COMServerType.LocalServer32, new COMCLSIDServerEntry(COMServerType.LocalServer32, "<APPID HOSTED>"));
        }

        TypeLib = COMUtilities.ReadGuid(key, "TypeLib", null);
        if (key.HasSubkey("Control"))
        {
            categories.Add(COMCategory.CATID_Control);
        }

        if (key.HasSubkey("Insertable"))
        {
            categories.Add(COMCategory.CATID_Insertable);
        }

        if (key.HasSubkey("DocObject"))
        {
            categories.Add(COMCategory.CATID_Document);
        }

        using (RegistryKey catkey = key.OpenSubKey("Implemented Categories"))
        {
            if (catkey != null)
            {
                string[] subKeys = catkey.GetSubKeyNames();
                foreach (string s in subKeys)
                {
                    if (Guid.TryParse(s, out Guid g))
                    {
                        categories.Add(g);
                    }
                }
            }
        }

        Categories = categories.ToList().AsReadOnly();
        TreatAs = COMUtilities.ReadGuid(key, "TreatAs", null);

        using (RegistryKey elev_key = key.OpenSubKey("Elevation"),
            vso_key = key.OpenSubKey("VirtualServerObjects"))
        {
            if (elev_key != null)
            {
                using var base_key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                    Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default);
                int auto_approval = COMUtilities.ReadInt(base_key,
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\UAC\COMAutoApprovalList", Clsid.ToString("B"));
                Elevation = new COMCLSIDElevationEntry(elev_key, vso_key, auto_approval != 0);
            }
        }

        ActivatableFromApp = _app_activatable.Contains(Clsid);
        using (RegistryKey trustkey = Registry.LocalMachine.OpenSubKey(@"Software\Classes\Unmarshalers\System\" + Clsid.ToString("B")))
        {
            TrustedMarshaller = trustkey != null ? true : categories.Contains(COMCategory.CATID_TrustedMarshaler);
        }

        Source = key.GetSource();
    }

    internal COMCLSIDEntry(COMRegistry registry, Guid clsid, COMPackagedEntry packageEntry,
        COMPackagedClassEntry classEntry) : this(registry, clsid)
    {
        Source = COMRegistryEntrySource.Packaged;
        Name = classEntry.DisplayName;
        PackageId = packageEntry.PackageId;

        Dictionary<COMServerType, COMCLSIDServerEntry> servers = new();
        if (!string.IsNullOrWhiteSpace(classEntry.DllPath))
        {
            servers.Add(COMServerType.InProcServer32, new COMCLSIDServerEntry(COMServerType.InProcServer32, classEntry.DllPath, classEntry.Threading));
        }

        if (packageEntry.Servers.ContainsKey(classEntry.ServerId))
        {
            COMPackagedServerEntry server = packageEntry.Servers[classEntry.ServerId];
            AppID = server.SurrogateAppId;
            string serverPath = "<APPID HOSTED>";
            string commandLine = string.Empty;
            if (AppID == Guid.Empty)
            {
                serverPath = server.Executable;
                commandLine = server.CommandLine;
            }
            servers.Add(COMServerType.LocalServer32, new COMCLSIDServerEntry(COMServerType.LocalServer32, serverPath, commandLine));
        }

        Servers = new ReadOnlyDictionary<COMServerType, COMCLSIDServerEntry>(servers);
        Categories = classEntry.ImplementedCategories.AsReadOnly();
    }

    internal COMCLSIDEntry(COMRegistry registry, Guid clsid, COMPackagedEntry packageEntry,
        COMPackagedProxyStubEntry proxyEntry) : this(registry, clsid)
    {
        Source = COMRegistryEntrySource.Packaged;
        Name = proxyEntry.DisplayName;
        PackageId = packageEntry.PackageId;

        Dictionary<COMServerType, COMCLSIDServerEntry> servers = new();
        string dllPath = Environment.Is64BitProcess ? proxyEntry.DllPath_x64 : proxyEntry.DllPath_x86;
        if (string.IsNullOrWhiteSpace(dllPath))
        {
            dllPath = proxyEntry.DllPath;
        }
        if (!string.IsNullOrWhiteSpace(dllPath))
        {
            servers.Add(COMServerType.InProcServer32, new COMCLSIDServerEntry(COMServerType.InProcServer32, dllPath, COMThreadingModel.Both));
        }

        Servers = new ReadOnlyDictionary<COMServerType, COMCLSIDServerEntry>(servers);
    }

    public COMCLSIDEntry(COMRegistry registry, Guid clsid, RegistryKey rootKey) : this(registry, clsid)
    {
        LoadFromKey(rootKey);
    }

    private COMCLSIDEntry(COMRegistry registry, Guid clsid) : this(registry)
    {
        Clsid = clsid;
        Servers = new Dictionary<COMServerType, COMCLSIDServerEntry>();
        Name = string.Empty;
        Categories = new Guid[0];
        PackageId = string.Empty;
    }

    public COMCLSIDEntry(COMRegistry registry, Guid clsid, COMServerType type)
        : this(registry, clsid)
    {
        Name = clsid.ToString();
    }

    internal COMCLSIDEntry(COMRegistry registry)
    {
        Database = registry;
    }

    internal COMCLSIDEntry(COMRegistry registry, ActCtxComServerRedirection com_server)
        : this(registry, com_server.Clsid)
    {
        Clsid = com_server.Clsid;
        TypeLib = com_server.TypeLibraryId;
        Servers[COMServerType.InProcServer32] = 
            new COMCLSIDServerEntry(COMServerType.InProcServer32, com_server.FullPath, com_server.ThreadingModel);
        Name = string.IsNullOrWhiteSpace(com_server.ProgId) ? Clsid.ToString() : com_server.ProgId;
        Source = COMRegistryEntrySource.ActCtx;
    }

    public Guid Clsid { get; private set; }

    public string Name { get; private set; }

    public string DefaultServer => GetDefaultServer().Server;

    public string DefaultServerName => COMUtilities.GetFileName(DefaultServer);

    public string DefaultCmdLine => GetDefaultServer().CommandLine;

    public COMServerType DefaultServerType => GetDefaultServer().ServerType;

    public Guid TreatAs { get; private set; }

    public Guid AppID { get; private set; }

    public COMAppIDEntry AppIDEntry => Database.AppIDs.GetGuidEntry(AppID);

    public bool HasAppID => AppIDEntry != null;

    public Guid TypeLib { get; private set; }

    public COMTypeLibEntry TypeLibEntry => Database.Typelibs.GetGuidEntry(TypeLib);

    public bool HasTypeLib => TypeLib != Guid.Empty;

    public bool CanElevate => Elevation != null && Elevation.Enabled;

    public bool AutoElevation => CanElevate && Elevation.AutoApproval;

    public COMCLSIDElevationEntry Elevation { get; private set; }

    public IDictionary<COMServerType, COMCLSIDServerEntry> Servers { get; private set; }

    public IEnumerable<Guid> Categories
    {
        get; private set;
    }

    public IEnumerable<COMCategory> CategoryEntries => Categories.Select(c => Database.ImplementedCategories.GetGuidEntry(c)).Where(c => c != null);

    public IEnumerable<string> ProgIds => ProgIdEntries.Select(p => p.ProgID);

    public IEnumerable<COMProgIDEntry> ProgIdEntries => Database.GetProgIdsForClsid(Clsid);

    public COMThreadingModel DefaultThreadingModel => GetDefaultServer().ThreadingModel;

    public bool ActivatableFromApp
    {
        get; private set;
    }

    /// <summary>
    /// True if this class is a trusted marshaller.
    /// </summary>
    public bool TrustedMarshaller
    {
        get; private set;
    }

    /// <summary>
    /// True if this class has the trusted marshaller category.
    /// </summary>
    public bool TrustedMarshallerCategory => Categories.Contains(COMCategory.CATID_TrustedMarshaler);

    /// <summary>
    /// True if this class is marked as safe to script.
    /// </summary>
    public bool SafeForScripting => Categories.Contains(COMCategory.CATID_SafeForScripting);

    /// <summary>
    /// True if this class is marked as safe to initialize.
    /// </summary>
    public bool SafeForInitializing => Categories.Contains(COMCategory.CATID_SafeForInitializing);

    public bool HasLaunchPermission
    {
        get
        {
            var appid = AppIDEntry;
            return appid != null ? appid.HasLaunchPermission : false;
        }
    }

    public bool HasAccessPermission
    {
        get
        {
            var appid = AppIDEntry;
            return appid != null ? appid.HasAccessPermission : false;
        }
    }

    public bool HasPermission => HasLaunchPermission || HasAccessPermission;

    public string LaunchPermission
    {
        get
        {
            var appid = AppIDEntry;
            return appid != null ? appid.LaunchPermission : string.Empty;
        }
    }

    public string AccessPermission
    {
        get
        {
            var appid = AppIDEntry;
            return appid != null ? appid.AccessPermission : string.Empty;
        }
    }

    public bool HasRunAs
    {
        get
        {
            var appid = AppIDEntry;
            return appid != null ? appid.HasRunAs : false;
        }
    }

    public string RunAs
    {
        get
        {
            var appid = AppIDEntry;
            return appid != null ? appid.RunAs : string.Empty;
        }
    }

    public string PackageId { get; private set; }

    public COMRegistryEntrySource Source
    {
        get; private set;
    }

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMCLSIDEntry right)
        {
            return false;
        }

        if (Elevation != null)
        {
            if (!Elevation.Equals(right.Elevation))
            {
                return false;
            }
        }
        else if (right.Elevation != null)
        {
            return false;
        }

        // We don't consider the loaded interfaces.
        return Clsid == right.Clsid && Name == right.Name && TreatAs == right.TreatAs && AppID == right.AppID
            && TypeLib == right.TypeLib && Servers.Values.SequenceEqual(right.Servers.Values)
            && ActivatableFromApp == right.ActivatableFromApp && TrustedMarshaller == right.TrustedMarshaller
            && Source == right.Source && PackageId == right.PackageId;
    }

    public override int GetHashCode()
    {
        return Clsid.GetHashCode() ^ Name.GetSafeHashCode() ^ TreatAs.GetHashCode()
            ^ AppID.GetHashCode() ^ TypeLib.GetHashCode() ^ Servers.Values.GetEnumHashCode()
            ^ Elevation.GetSafeHashCode() ^ ActivatableFromApp.GetHashCode() ^ TrustedMarshaller.GetHashCode()
            ^ Source.GetHashCode() ^ PackageId.GetSafeHashCode();
    }

    private async Task<COMEnumerateInterfaces> GetSupportedInterfacesInternal(NtToken token)
    {
        try
        {
            return await COMEnumerateInterfaces.GetInterfacesOOP(this, Database, token);
        }
        catch (Win32Exception)
        {
            throw;
        }
        catch (AggregateException agg)
        {
            throw agg.InnerException;
        }
    }

    private COMCLSIDServerEntry GetDefaultServer()
    {
        if (Servers.ContainsKey(COMServerType.InProcServer32))
        {
            return Servers[COMServerType.InProcServer32];
        }

        if (Servers.ContainsKey(COMServerType.LocalServer32))
        {
            return Servers[COMServerType.LocalServer32];
        }

        if (Servers.ContainsKey(COMServerType.InProcHandler32))
        {
            return Servers[COMServerType.InProcHandler32];
        }

        return new COMCLSIDServerEntry(COMServerType.UnknownServer);
    }

    /// <summary>
    /// Get list of supported Interface IIDs (that we know about)
    /// NOTE: This will load the object itself to check what is supported, it _might_ crash the app
    /// The returned array is cached so subsequent calls to this function return without calling into COM
    /// </summary>
    /// <param name="refresh">Force the supported interface list to refresh</param>
    /// <param name="token">Token to use when checking for the interfaces.</param>
    /// <returns>Returns true if supported interfaces were refreshed.</returns>
    /// <exception cref="Win32Exception">Thrown on error.</exception>
    public async Task<bool> LoadSupportedInterfacesAsync(bool refresh, NtToken token)
    {
        if (Clsid == Guid.Empty)
        {
            return false;
        }

        if (refresh || !InterfacesLoaded)
        {
            COMEnumerateInterfaces enum_int = await GetSupportedInterfacesInternal(token);
            m_interfaces = new List<COMInterfaceInstance>(enum_int.Interfaces);
            m_factory_interfaces = new List<COMInterfaceInstance>(enum_int.FactoryInterfaces);
            InterfacesLoaded = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get list of supported Interface IIDs Synchronously
    /// </summary>
    /// <param name="refresh">Force the supported interface list to refresh</param>
    /// <param name="token">Token to use when querying for the interfaces.</param>
    /// <returns>Returns true if supported interfaces were refreshed.</returns>
    /// <exception cref="Win32Exception">Thrown on error.</exception>
    public bool LoadSupportedInterfaces(bool refresh, NtToken token)
    {
        Task<bool> result = LoadSupportedInterfacesAsync(refresh, token);
        try
        {
            result.Wait();
            if (result.IsFaulted)
            {
                throw result.Exception.InnerException;
            }
            return result.Result;
        }
        catch (Exception ex)
        {
            if (ex is AggregateException agg)
            {
                throw agg.InnerException;
            }
            throw;
        }
    }

    /// <summary>
    /// Indicates that the class' interface list has been loaded.
    /// </summary>
    public bool InterfacesLoaded { get; private set; }

    /// <summary>
    /// Get list of interfaces.
    /// </summary>
    /// <remarks>You must have called LoadSupportedInterfaces before this call to get any useful output.</remarks>
    public IEnumerable<COMInterfaceInstance> Interfaces
    {
        get
        {
            if (InterfacesLoaded)
            {
                return m_interfaces.AsReadOnly();
            }
            else
            {
                return new COMInterfaceInstance[0];
            }
        }
    }

    /// <summary>
    /// Get list of factory interfaces.
    /// </summary>
    /// <remarks>You must have called LoadSupportedFactoryInterfaces before this call to get any useful output.</remarks>
    public IEnumerable<COMInterfaceInstance> FactoryInterfaces
    {
        get
        {
            if (InterfacesLoaded)
            {
                return m_factory_interfaces.AsReadOnly();
            }
            else
            {
                return new COMInterfaceInstance[0];
            }
        }
    }

    public CLSCTX CreateContext
    {
        get
        {
            CLSCTX dwContext = CLSCTX.ALL;

            if (DefaultServerType == COMServerType.InProcServer32)
            {
                dwContext = CLSCTX.INPROC_SERVER;
            }
            else if (DefaultServerType == COMServerType.LocalServer32)
            {
                dwContext = CLSCTX.LOCAL_SERVER;
            }
            else if (DefaultServerType == COMServerType.InProcHandler32)
            {
                dwContext = CLSCTX.INPROC_HANDLER;
            }

            return dwContext;
        }
    }

    public IntPtr CreateInstance(CLSCTX dwContext, string server)
    {
        if (dwContext == CLSCTX.ALL)
        {
            if (DefaultServerType == COMServerType.InProcServer32)
            {
                dwContext = CLSCTX.INPROC_SERVER;
            }
            else if (DefaultServerType == COMServerType.LocalServer32)
            {
                dwContext = CLSCTX.LOCAL_SERVER;
            }
            else if (DefaultServerType == COMServerType.InProcHandler32)
            {
                dwContext = CLSCTX.INPROC_HANDLER;
            }
            else
            {
                dwContext = CLSCTX.SERVER;
            }
        }
        
        Guid iid = COMInterfaceEntry.CreateKnownInterface(Database, COMKnownInterfaces.IUnknown).Iid;
        return COMUtilities.CreateInstance(Clsid, iid, dwContext, server);
    }

    public object CreateInstanceAsObject(CLSCTX dwContext, string server)
    {
        IntPtr pObject = CreateInstance(dwContext, server);
        object ret = null;

        if (pObject != IntPtr.Zero)
        {
            ret = Marshal.GetObjectForIUnknown(pObject);
            Marshal.Release(pObject);
        }

        return ret;
    }

    public object CreateClassFactory()
    {
        return CreateClassFactory(CreateContext, null);
    }

    public object CreateClassFactory(CLSCTX dwContext, string server)
    {
        return COMUtilities.CreateClassFactory(Clsid, COMInterfaceEntry.IID_IUnknown, dwContext, server);
    }

    public bool SupportsRemoteActivation => true;

    internal COMRegistry Database { get; }

    string ICOMAccessSecurity.DefaultAccessPermission => Database.DefaultAccessPermission;

    string ICOMAccessSecurity.DefaultLaunchPermission => Database.DefaultLaunchPermission;

    Guid IComGuid.ComGuid => Clsid;

    public override string ToString()
    {
        return Name;
    }

    XmlSchema IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        Clsid = reader.ReadGuid("clsid");
        AppID = reader.ReadGuid("appid");
        TypeLib = reader.ReadGuid("tlib");
        Categories = reader.ReadGuids("catids");
        TreatAs = reader.ReadGuid("treatas");
        InterfacesLoaded = reader.ReadBool("loaded");
        ActivatableFromApp = reader.ReadBool("activatable");
        TrustedMarshaller = reader.ReadBool("trusted");
        bool elevate = reader.ReadBool("elevate");
        Source = reader.ReadEnum<COMRegistryEntrySource>("src");
        PackageId = reader.ReadString("pkg");
        Name = reader.ReadString("name");
        if (InterfacesLoaded)
        {
            m_interfaces = reader.ReadSerializableObjects("ints", () => new COMInterfaceInstance(Database)).ToList();
            m_factory_interfaces = reader.ReadSerializableObjects("facts", () => new COMInterfaceInstance(Database)).ToList();
        }
        Servers = new ReadOnlyDictionary<COMServerType, COMCLSIDServerEntry>(
                reader.ReadSerializableObjects("servers", () => new COMCLSIDServerEntry()).ToDictionary(e => e.ServerType));

        if (elevate)
        {
            IEnumerable<COMCLSIDElevationEntry> elevation = 
                reader.ReadSerializableObjects("elevation", () => new COMCLSIDElevationEntry());
            Elevation = elevation.FirstOrDefault();
        }
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteGuid("clsid", Clsid);
        writer.WriteGuid("appid", AppID);
        writer.WriteGuid("tlib", TypeLib);
        writer.WriteGuids("catids", Categories);
        writer.WriteGuid("treatas", TreatAs);
        writer.WriteBool("loaded", InterfacesLoaded);
        writer.WriteBool("activatable", ActivatableFromApp);
        writer.WriteBool("trusted", TrustedMarshaller);
        writer.WriteBool("elevate", Elevation != null);
        writer.WriteEnum("src", Source);
        writer.WriteOptionalAttributeString("pkg", PackageId);
        writer.WriteOptionalAttributeString("name", Name);
        if (InterfacesLoaded)
        {
            writer.WriteSerializableObjects("ints", m_interfaces);
            writer.WriteSerializableObjects("facts", m_factory_interfaces);
        }
        writer.WriteSerializableObjects("servers", Servers.Values);
        if (Elevation != null)
        {
            writer.WriteSerializableObjects("elevation", new COMCLSIDElevationEntry[] { Elevation });
        }
    }
}
