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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet
{
    public enum TrustLevel
    {
        BaseTrust = 0,
        PartialTrust = 1,
        FullTrust = 2
    }

    public enum ActivationType
    {
        InProcess = 0,
        OutOfProcess = 1,
        RemoteProcess = 2,
    }

    public enum ThreadingType
    {
        Both = 0,
        Sta = 1,
        Mta = 2
    }

    public class COMRuntimeClassEntry : IComparable<COMRuntimeClassEntry>, IXmlSerializable, ICOMClassEntry, ICOMAccessSecurity
    {
        private List<COMInterfaceInstance> m_interfaces;
        private List<COMInterfaceInstance> m_factory_interfaces;
        private readonly COMRegistry m_registry;

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
                return Path.GetFileName(DllPath);
            }
        }

        public string Server { get; private set; }
        public string DefaultServer { get { return Server; } }
        public bool HasServer
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Server);
            }
        }

        public COMRuntimeServerEntry ServerEntry
        {
            get
            {
                return m_registry.MapRuntimeClassToServerEntry(this);
            }
        }

        public ActivationType ActivationType { get; private set; }

        public string Permissions
        {
            get; private set;
        }
        public bool HasPermission
        {
            get { return !string.IsNullOrWhiteSpace(Permissions); }
        }
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

        private void LoadFromKey(RegistryKey key)
        {
            Clsid = COMUtilities.ReadGuidFromKey(key, null, "CLSID");
            ActivationType = (ActivationType)COMUtilities.ReadIntFromKey(key, null, "ActivationType");
            TrustLevel = (TrustLevel)COMUtilities.ReadIntFromKey(key, null, "TrustLevel");
            Threading = (ThreadingType)COMUtilities.ReadIntFromKey(key, null, "Threading");
            DllPath = COMUtilities.ReadStringFromKey(key, null, "DllPath");
            Server = COMUtilities.ReadStringFromKey(key, null, "Server");
            byte[] permissions = key.GetValue("Permissions", new byte[0]) as byte[];
            Permissions = COMSecurity.GetStringSDForSD(permissions);
            ActivateInSharedBroker = COMUtilities.ReadIntFromKey(key, null, "ActivateInSharedBroker") != 0;
        }

        internal COMRuntimeClassEntry(COMRegistry registry, string name) : this(registry)
        {
            Name = name;
            DllPath = string.Empty;
            Server = string.Empty;
            Permissions = string.Empty;
        }

        public COMRuntimeClassEntry(COMRegistry registry, string name, RegistryKey rootKey) 
            : this(registry, name)
        {
            LoadFromKey(rootKey);
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
            Permissions = reader.ReadString("perms");
            TrustLevel = reader.ReadEnum<TrustLevel>("trust");
            Threading = reader.ReadEnum<ThreadingType>("thread");
            ActivateInSharedBroker = reader.ReadBool("shared");
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteGuid("clsid", Clsid);
            writer.WriteAttributeString("name", Name);
            writer.WriteOptionalAttributeString("dllpath", DllPath);
            writer.WriteOptionalAttributeString("server", Server);
            writer.WriteEnum("type", ActivationType);
            writer.WriteEnum("trust", TrustLevel);
            writer.WriteOptionalAttributeString("perms", Permissions);
            writer.WriteEnum("thread", Threading);
            writer.WriteBool("shared", ActivateInSharedBroker);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMRuntimeClassEntry right = obj as COMRuntimeClassEntry;
            if (right == null)
            {
                return false;
            }

            return Clsid == right.Clsid && Name == right.Name && DllPath == right.DllPath && Server == right.Server
                && ActivationType == right.ActivationType && TrustLevel == right.TrustLevel &&
                Permissions == right.Permissions && Threading == right.Threading && ActivateInSharedBroker == right.ActivateInSharedBroker;
        }

        public override int GetHashCode()
        {
            return Clsid.GetHashCode() ^ Name.GetSafeHashCode() ^ DllPath.GetSafeHashCode()
                ^ Server.GetSafeHashCode() ^ ActivationType.GetHashCode() ^ TrustLevel.GetHashCode()
                ^ Permissions.GetSafeHashCode() ^ Threading.GetHashCode() ^ ActivateInSharedBroker.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        int IComparable<COMRuntimeClassEntry>.CompareTo(COMRuntimeClassEntry other)
        {
            return Server.CompareTo(other.Server);
        }

        private async Task<COMEnumerateInterfaces> GetSupportedInterfacesInternal()
        {
            try
            {
                return await COMEnumerateInterfaces.GetInterfacesOOP(this, m_registry);
            }
            catch (Win32Exception)
            {
                throw;
            }
        }

        public async Task<bool> LoadSupportedInterfacesAsync(bool refresh)
        {
            if (Clsid == Guid.Empty)
            {
                return false;
            }

            if (refresh || !InterfacesLoaded)
            {
                COMEnumerateInterfaces enum_int = await GetSupportedInterfacesInternal();
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
        public bool LoadSupportedInterfaces(bool refresh)
        {
            Task<bool> result = LoadSupportedInterfacesAsync(refresh);
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
                    Guid iid = COMInterfaceEntry.IID_IUnknown;
                    hr = COMUtilities.RoGetActivationFactory(Name, ref iid, out pObject);
                }
                else
                {
                    hr = COMUtilities.RoActivateInstance(Name, out pObject);
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

        public object CreateInstanceAsObject(CLSCTX dwContext, string server)
        {
            return CreateInstance(server, false);
        }

        public object CreateClassFactory(CLSCTX dwContext, string server)
        {
            return CreateInstance(server, true);
        }

        public bool SupportsRemoteActivation { get { return false; } }

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

        string ICOMAccessSecurity.DefaultAccessPermission => "O:SYG:SYD:";

        string ICOMAccessSecurity.DefaultLaunchPermission => "O:SYG:SYD:";
    }
}
