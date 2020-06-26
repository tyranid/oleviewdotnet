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
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet.Database
{
    public class COMInterfaceEntry : IComparable<COMInterfaceEntry>, IXmlSerializable, IComGuid
    {
        private static ConcurrentDictionary<Guid, string> m_iidtoname = new ConcurrentDictionary<Guid, string>();
        private readonly COMRegistry m_registry;

        internal static string MapIidToName(Guid iid)
        {
            if (m_iidtoname.TryGetValue(iid, out string ret))
            {
                return ret;
            }
            return iid.FormatGuid();
        }

        internal static void CacheIidToName(Guid iid, string name)
        {
            m_iidtoname.TryAdd(iid, name);
        }

        public int CompareTo(COMInterfaceEntry right)
        {
            return string.Compare(Name, right.Name);
        }

        private void LoadFromKey(RegistryKey key)
        {
            string name = key.GetValue(null) as string;
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = COMUtilities.DemangleWinRTName(name.ToString());
                CacheIidToName(Iid, Name);
            }
            else
            {
                Name = Iid.FormatGuidDefault();
            }

            ProxyClsid = COMUtilities.ReadGuid(key, "ProxyStubCLSID32", null);
            NumMethods = COMUtilities.ReadInt(key, "NumMethods", null);

            if (NumMethods < 3)
            {
                NumMethods = 3;
            }

            TypeLib = COMUtilities.ReadGuid(key, "TypeLib", null);
            TypeLibVersion = COMUtilities.ReadString(key, "TypeLib", "Version");
            Base = COMUtilities.ReadString(key, "BaseInterface", null);
            if (Base.Length == 0)
            {
                Base = "IUnknown";
            }
            Source = key.GetSource();
        }

        internal COMInterfaceEntry(COMRegistry registry)
        {
            m_registry = registry;
        }

        private COMInterfaceEntry(COMRegistry registry, Guid iid, Guid proxyclsid, 
            int nummethods, string baseName, string name) : this(registry)
        {
            Iid = iid;
            ProxyClsid = proxyclsid;
            NumMethods = nummethods;
            Base = baseName;
            Name = name;
            TypeLibVersion = string.Empty;
        }

        internal COMInterfaceEntry(COMRegistry registry, ActCtxComInterfaceRedirection intf_redirection) 
            : this(registry, intf_redirection.Iid, intf_redirection.ProxyStubClsid32, intf_redirection.NumMethods, 
                  string.Empty, intf_redirection.Name)
        {
            TypeLib = intf_redirection.TypeLibraryId;
            Source = COMRegistryEntrySource.ActCtx;
        }

        internal COMInterfaceEntry(COMRegistry registry, Type type) : this(registry, type.GUID, Guid.Empty, type.GetMethods().Length + 6, "IInspectable", type.FullName)
        {
            CacheIidToName(Iid, Name);
            RuntimeInterface = true;
            Source = COMRegistryEntrySource.Metadata;
        }

        public COMInterfaceEntry(COMRegistry registry, Guid iid, RegistryKey rootKey)
            : this(registry, iid, Guid.Empty, 3, "IUnknown", "")
        {
            LoadFromKey(rootKey);
        }

        public COMInterfaceEntry(COMRegistry registry, Guid iid)
            : this(registry, iid, Guid.Empty, 3, "IUnknown", MapIidToName(iid))
        {
        }

        internal COMInterfaceEntry(COMRegistry registry, COMPackagedInterfaceEntry entry) 
            : this(registry, entry.Iid, entry.ProxyStubCLSID, 3, "IUnknown", entry.Iid.FormatGuidDefault())
        {
            if (entry.UseUniversalMarshaler)
            {
                ProxyClsid = new Guid("00020424-0000-0000-C000-000000000046");
            }
            TypeLib = entry.TypeLibId;
            TypeLibVersion = entry.TypeLibVersionNumber;
            Source = COMRegistryEntrySource.Packaged;
        }

        public enum KnownInterfaces
        {
            IUnknown,
            IMarshal,
            IPSFactoryBuffer,
        }

        public static Guid IID_IUnknown
        {
            get { return new Guid("{00000000-0000-0000-C000-000000000046}"); }
        }

        public static Guid IID_IMarshal
        {
            get { return new Guid("{00000003-0000-0000-C000-000000000046}"); }
        }

        public static Guid IID_IDispatch
        {
            get { return new Guid("00020400-0000-0000-c000-000000000046"); }
        }

        public static Guid IID_IOleControl
        {
            get { return new Guid("{b196b288-bab4-101a-b69c-00aa00341d07}"); }
        }

        public static Guid IID_IPersistStream
        {
            get { return typeof(IPersistStream).GUID; }
        }

        public static Guid IID_IPersistStreamInit
        {
            get { return typeof(IPersistStreamInit).GUID; }
        }

        public static Guid IID_IPSFactoryBuffer
        {
            get { return new Guid("D5F569D0-593B-101A-B569-08002B2DBF7A"); }
        }

        public static Guid IID_IInspectable
        {
            get { return new Guid("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90"); }
        }

        public bool IsOleControl
        {
            get { return (Iid == IID_IOleControl); }
        }

        public bool IsDispatch
        {
            get { return (Iid == IID_IDispatch); }
        }

        public bool IsMarshal
        {
            get { return (Iid == IID_IMarshal); }
        }

        public bool IsPersistStream
        {
            get { return (Iid == IID_IPersistStream) || (Iid == IID_IPersistStreamInit); }
        }

        public bool IsClassFactory
        {
            get { return Iid == typeof(IClassFactory).GUID; }
        }

        public static COMInterfaceEntry CreateKnownInterface(COMRegistry registry, KnownInterfaces known)
        {
            switch (known)
            {
                case KnownInterfaces.IUnknown:
                    return new COMInterfaceEntry(registry)
                    {
                        Base = "",
                        Iid = IID_IUnknown,
                        ProxyClsid = Guid.Empty,
                        NumMethods = 3,
                        Name = "IUnknown",
                        TypeLibVersion = String.Empty,
                        Source = COMRegistryEntrySource.Builtin
                    };
                case KnownInterfaces.IMarshal:
                    return new COMInterfaceEntry(registry)
                    {
                        Base = "",
                        Iid = IID_IMarshal,
                        ProxyClsid = Guid.Empty,
                        NumMethods = 9,
                        Name = "IMarshal",
                        TypeLibVersion = String.Empty,
                        Source = COMRegistryEntrySource.Builtin
                    };
                case KnownInterfaces.IPSFactoryBuffer:
                    return new COMInterfaceEntry(registry)
                    {
                        Base = "",
                        Iid = IID_IPSFactoryBuffer,
                        ProxyClsid = Guid.Empty,
                        NumMethods = 4,
                        Name = "IPSFactoryBuffer",
                        TypeLibVersion = String.Empty,
                        Source = COMRegistryEntrySource.Builtin
                    };
            }

            return null;
        }

        public string Name
        {
            get; private set;
        }

        public Guid Iid
        {
            get; private set;
        }

        public COMInterfaceEntry InterfaceEntry
        {
            get
            {
                return m_registry.Interfaces.GetGuidEntry(Iid);
            }
        }

        public Guid ProxyClsid
        {
            get; private set;
        }

        public COMCLSIDEntry ProxyClassEntry
        {
            get
            {
                return m_registry.Clsids.GetGuidEntry(ProxyClsid);
            }
        }

        public int NumMethods
        {
            get; private set;
        }

        public string Base
        {
            get; private set;
        }

        public Guid TypeLib
        {
            get; private set;
        }

        public string TypeLibVersion
        {
            get; private set;
        }

        public COMTypeLibEntry TypeLibEntry
        {
            get
            {
                return m_registry.Typelibs.GetGuidEntry(TypeLib);
            }
        }

        public COMTypeLibVersionEntry TypeLibVersionEntry
        {
            get
            {
                var typelib = TypeLibEntry;
                if (typelib != null)
                {
                    return typelib.Versions.Where(v => v.Version == TypeLibVersion).FirstOrDefault();
                }
                return null;
            }
        }

        public bool HasTypeLib
        {
            get { return TypeLib != Guid.Empty; }
        }

        public bool HasProxy
        {
            get { return ProxyClsid != Guid.Empty; }
        }

        public bool RuntimeInterface
        {
            get; internal set;
        }

        public COMRegistryEntrySource Source
        {
            get; private set;
        }

        Guid IComGuid.ComGuid => Iid;

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            COMInterfaceEntry right = obj as COMInterfaceEntry;
            if (right == null)
            {
                return false;
            }

            return Name == right.Name && Iid == right.Iid && ProxyClsid == right.ProxyClsid
                && NumMethods == right.NumMethods && Base == right.Base && TypeLib == right.TypeLib
                && TypeLibVersion == right.TypeLibVersion && RuntimeInterface == right.RuntimeInterface
                && Source == right.Source;
        }

        public override int GetHashCode()
        {
            return Name.GetSafeHashCode() ^ Iid.GetHashCode() ^ ProxyClsid.GetHashCode() ^ NumMethods.GetHashCode() 
                ^ Base.GetSafeHashCode() ^ TypeLib.GetHashCode() ^ TypeLibVersion.GetSafeHashCode() ^ RuntimeInterface.GetHashCode()
                ^ Source.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        public bool TestInterface(IntPtr obj)
        {
            Guid iid = Iid;
            if (Marshal.QueryInterface(obj, ref iid, out IntPtr pRequested) == 0)
            {
                Marshal.Release(pRequested);
                return true;
            }
            return false;
        }

        public bool TestInterface(object obj)
        {
            IntPtr punk = IntPtr.Zero;
            try
            {
                punk = Marshal.GetIUnknownForObject(obj);
                return TestInterface(punk);
            }
            finally
            {
                if (punk != IntPtr.Zero)
                    Marshal.Release(punk);
            }
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            Name = reader.ReadString("name");
            Iid = reader.ReadGuid("iid");
            ProxyClsid = reader.ReadGuid("proxy");
            NumMethods = reader.ReadInt("num");
            Base = reader.ReadString("base");
            TypeLibVersion = reader.ReadString("ver");
            TypeLib = reader.ReadGuid("tlib");
            RuntimeInterface = reader.ReadBool("rt");
            Source = reader.ReadEnum<COMRegistryEntrySource>("src");

            m_iidtoname.TryAdd(Iid, Name);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteOptionalAttributeString("name", Name);
            writer.WriteGuid("iid", Iid);
            writer.WriteGuid("proxy", ProxyClsid);
            writer.WriteInt("num", NumMethods);
            writer.WriteOptionalAttributeString("base", Base);
            writer.WriteOptionalAttributeString("ver", TypeLibVersion);
            writer.WriteGuid("tlib", TypeLib);
            writer.WriteBool("rt", RuntimeInterface);
            writer.WriteEnum("src", Source);
        }
    }
}
