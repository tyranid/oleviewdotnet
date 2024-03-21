//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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
using OleViewDotNet.Interop;
using OleViewDotNet.Security;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet.Database;

public class COMRuntimeClassEntry : IComparable<COMRuntimeClassEntry>, IXmlSerializable, ICOMClassEntry, ICOMAccessSecurity
{
    private List<COMInterfaceInstance> m_interfaces;
    private List<COMInterfaceInstance> m_factory_interfaces;
    private readonly COMRegistry m_registry;

    public const string DefaultActivationPermission = "O:SYG:SYD:(A;;CCDCSW;;;AC)(A;;CCDCSW;;;PS)(A;;CCDCSW;;;SY)(A;;CCDCSW;;;LS)(A;;CCDCSW;;;NS)(XA;;CCDCSW;;;IU;(!(WIN://ISMULTISESSIONSKU)))S:(ML;;NX;;;LW)";

    public string Name { get; private set; }
    public Guid Clsid { get; private set; }
    public string DllPath { get; private set; }
    public string DllName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(DllPath))
            {
                return string.Empty;
            }
            return MiscUtilities.GetFileName(DllPath);
        }
    }
    public string PackageId { get; private set; }
    public string PackageName => Package?.Name ?? string.Empty;
    public AppxPackageName Package => AppxPackageName.FromFullName(PackageId);

    public bool RuntimeClass => string.IsNullOrEmpty(PackageId);
    public string Server { get; private set; }
    public string DefaultServer => Server;
    public bool HasServer => !string.IsNullOrWhiteSpace(Server);
    COMServerType ICOMClassEntry.DefaultServerType => Server is null ? COMServerType.InProcServer32 : COMServerType.LocalServer32;

    public COMRuntimeServerEntry ServerEntry => m_registry.MapRuntimeClassToServerEntry(this);

    public ActivationType ActivationType { get; private set; }

    public COMSecurityDescriptor Permissions
    {
        get; private set;
    }

    public bool HasPermission => Permissions is not null;

    public COMSecurityDescriptor ServerPermissions => ServerEntry?.Permissions;

    public bool HasServerPermission => ServerPermissions is not null;

    public TrustLevel TrustLevel
    {
        get; private set;
    }
    public ThreadingType Threading
    {
        get; private set;
    }
    public bool ActivateInSharedBroker
    {
        get; private set;
    }

    public Type RuntimeType => Type.GetType($"{Name}, Windows, ContentType=WindowsRuntime");

    public COMRegistryEntrySource Source
    {
        get; private set;
    }

    private void LoadFromKey(RegistryKey key)
    {
        Clsid = key.ReadGuid(null, "CLSID");
        ActivationType = (ActivationType)key.ReadInt(null, "ActivationType");
        TrustLevel = (TrustLevel)key.ReadInt(null, "TrustLevel");
        Threading = (ThreadingType)key.ReadInt(null, "Threading");
        DllPath = key.ReadString(null, "DllPath");
        Server = key.ReadString(null, "Server");
        Permissions = key.ReadSecurityDescriptor(valueName: "Permissions");
        ActivateInSharedBroker = key.ReadInt(null, "ActivateInSharedBroker") != 0;
    }

    internal COMRuntimeClassEntry(COMRegistry registry, string package_id, string name) 
        : this(registry)
    {
        Name = name;
        DllPath = string.Empty;
        Server = string.Empty;
        PackageId = package_id ?? string.Empty;
    }

    public COMRuntimeClassEntry(COMRegistry registry, 
        string package_id, string name, RegistryKey rootKey) 
        : this(registry, package_id, name)
    {
        LoadFromKey(rootKey);
        Source = rootKey.GetSource();
    }

    internal COMRuntimeClassEntry(COMRegistry registry)
    {
        m_registry = registry;
    }

    XmlSchema IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        Clsid = reader.ReadGuid("clsid");
        Name = reader.ReadString("name");
        DllPath = reader.ReadString("dllpath");
        Server = reader.ReadString("server");
        ActivationType = reader.ReadEnum<ActivationType>("type");
        Permissions = reader.ReadSecurityDescriptor("perms");
        TrustLevel = reader.ReadEnum<TrustLevel>("trust");
        Threading = reader.ReadEnum<ThreadingType>("thread");
        ActivateInSharedBroker = reader.ReadBool("shared");
        PackageId = reader.ReadString("pkg");
        Source = reader.ReadEnum<COMRegistryEntrySource>("src");
        InterfacesLoaded = reader.ReadBool("loaded");
        if (InterfacesLoaded)
        {
            m_interfaces = reader.ReadSerializableObjects("ints", () => new COMInterfaceInstance(m_registry)).ToList();
            m_factory_interfaces = reader.ReadSerializableObjects("facts", () => new COMInterfaceInstance(m_registry)).ToList();
        }
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteGuid("clsid", Clsid);
        writer.WriteAttributeString("name", Name);
        writer.WriteOptionalAttributeString("dllpath", DllPath);
        writer.WriteOptionalAttributeString("server", Server);
        writer.WriteEnum("type", ActivationType);
        writer.WriteEnum("trust", TrustLevel);
        writer.WriteSecurityDescriptor("perms", Permissions);
        writer.WriteEnum("thread", Threading);
        writer.WriteBool("shared", ActivateInSharedBroker);
        writer.WriteOptionalAttributeString("pkg", PackageId);
        writer.WriteEnum("src", Source);
        writer.WriteBool("loaded", InterfacesLoaded);
        if (InterfacesLoaded)
        {
            writer.WriteSerializableObjects("ints", m_interfaces);
            writer.WriteSerializableObjects("facts", m_factory_interfaces);
        }
    }

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMRuntimeClassEntry right)
        {
            return false;
        }

        return Clsid == right.Clsid && Name == right.Name && DllPath == right.DllPath && Server == right.Server
            && ActivationType == right.ActivationType && TrustLevel == right.TrustLevel &&
            Permissions.SDIsEqual(right.Permissions) && Threading == right.Threading && ActivateInSharedBroker == right.ActivateInSharedBroker
            && PackageId == right.PackageId && Source == right.Source;
    }

    public override int GetHashCode()
    {
        return Clsid.GetHashCode() ^ Name.GetSafeHashCode() ^ DllPath.GetSafeHashCode()
            ^ Server.GetSafeHashCode() ^ ActivationType.GetHashCode() ^ TrustLevel.GetHashCode()
            ^ Permissions.GetSDHashCode() ^ Threading.GetHashCode() ^ ActivateInSharedBroker.GetHashCode()
            ^ PackageId.GetSafeHashCode() ^ Source.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }

    int IComparable<COMRuntimeClassEntry>.CompareTo(COMRuntimeClassEntry other)
    {
        return Server.CompareTo(other.Server);
    }

    private async Task<COMEnumerateInterfaces> GetSupportedInterfacesInternal(COMAccessToken token)
    {
        try
        {
            return await COMEnumerateInterfaces.GetInterfacesOOP(this, m_registry, token);
        }
        catch (Win32Exception)
        {
            throw;
        }
    }

    public async Task<bool> LoadSupportedInterfacesAsync(bool refresh, COMAccessToken token)
    {
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
    /// <returns>Returns true if supported interfaces were refreshed.</returns>
    /// <exception cref="Win32Exception">Thrown on error.</exception>
    public bool LoadSupportedInterfaces(bool refresh, COMAccessToken token)
    {
        Task<bool> result = LoadSupportedInterfacesAsync(refresh, token);
        result.Wait();
        if (result.IsFaulted)
        {
            throw result.Exception.InnerException;
        }
        return result.Result;
    }

    private object CreateInstance(string server, bool factory)
    {
        if (!string.IsNullOrWhiteSpace(server))
        {
            throw new ArgumentException("Specifying a remote server is not valid for this class type.", "server");
        }

        IntPtr pObject = IntPtr.Zero;
        try
        {
            int hr;

            if (factory)
            {
                hr = NativeMethods.RoGetActivationFactory(Name, COMKnownGuids.IID_IUnknown, out pObject);
            }
            else
            {
                hr = NativeMethods.RoActivateInstance(Name, out pObject);
            }
            if (hr != 0)
            {
                throw new Win32Exception(hr);
            }

            return Marshal.GetObjectForIUnknown(pObject);
        }
        finally
        {
            if (pObject != IntPtr.Zero)
            {
                Marshal.Release(pObject);
            }
        }
    }

    public object CreateInstanceAsObject(CLSCTX dwContext, string server, COMAuthInfo auth_info = null)
    {
        return CreateInstance(server, false);
    }

    public object CreateClassFactory(CLSCTX dwContext, string server, COMAuthInfo auth_info = null)
    {
        return CreateInstance(server, true);
    }

    public bool SupportsRemoteActivation => false;

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

    COMSecurityDescriptor ICOMAccessSecurity.DefaultAccessPermission => new("O:SYG:SYD:");

    COMSecurityDescriptor ICOMAccessSecurity.DefaultLaunchPermission => new("O:SYG:SYD:");
}
