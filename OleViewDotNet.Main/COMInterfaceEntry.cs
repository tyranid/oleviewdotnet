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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet
{
    public class COMInterfaceEntry : IComparable<COMInterfaceEntry>, IXmlSerializable
    {
        private static ConcurrentDictionary<Guid, string> m_iidtoname = new ConcurrentDictionary<Guid, string>();
        private readonly COMRegistry m_registry;

        internal static string MapIidToName(Guid iid)
        {
            string ret;
            if (m_iidtoname.TryGetValue(iid, out ret))
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
            return String.Compare(Name, right.Name);
        }

        private void LoadFromKey(RegistryKey key)
        {
            string name = key.GetValue(null) as string;
            if (!String.IsNullOrWhiteSpace(name))
            {
                Name = COMUtilities.DemangleWinRTName(name.ToString());
                CacheIidToName(Iid, Name);
            }
            else
            {
                Name = Iid.FormatGuidDefault();
            }

            ProxyClsid = COMUtilities.ReadGuidFromKey(key, "ProxyStubCLSID32", null);
            NumMethods = COMUtilities.ReadIntFromKey(key, "NumMethods", null);

            if (NumMethods < 3)
            {
                NumMethods = 3;
            }

            TypeLib = COMUtilities.ReadGuidFromKey(key, "TypeLib", null);
            TypeLibVersion = COMUtilities.ReadStringFromKey(key, "TypeLib", "Version");
            Base = COMUtilities.ReadStringFromKey(key, "BaseInterface", null);
            if (Base.Length == 0)
            {
                Base = "IUnknown";
            }
        }

        internal COMInterfaceEntry(COMRegistry registry)
        {
            m_registry = registry;
        }

        private COMInterfaceEntry(COMRegistry registry, Guid iid, Guid proxyclsid, int nummethods, string baseName, string name) : this(registry)
        {
            Iid = iid;
            ProxyClsid = proxyclsid;
            NumMethods = nummethods;
            Base = baseName;
            Name = name;
            TypeLibVersion = String.Empty;
        }

        internal COMInterfaceEntry(COMRegistry registry, Type type) : this(registry, type.GUID, Guid.Empty, type.GetMethods().Length + 6, "IInspectable", type.FullName)
        {
            CacheIidToName(Iid, Name);
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
            COMInterfaceEntry ent = null;
            switch (known)
            {
                case KnownInterfaces.IUnknown:
                    ent = new COMInterfaceEntry(registry);
                    ent.Base = "";
                    ent.Iid = IID_IUnknown;
                    ent.ProxyClsid = Guid.Empty;
                    ent.NumMethods = 3;
                    ent.Name = "IUnknown";
                    ent.TypeLibVersion = String.Empty;
                    break;
                case KnownInterfaces.IMarshal:
                    ent = new COMInterfaceEntry(registry);
                    ent.Base = "";
                    ent.Iid = IID_IMarshal;
                    ent.ProxyClsid = Guid.Empty;
                    ent.NumMethods = 9;
                    ent.Name = "IMarshal";
                    ent.TypeLibVersion = String.Empty;
                    break;
                case KnownInterfaces.IPSFactoryBuffer:
                    ent = new COMInterfaceEntry(registry);
                    ent.Base = "";
                    ent.Iid = IID_IPSFactoryBuffer;
                    ent.ProxyClsid = Guid.Empty;
                    ent.NumMethods = 4;
                    ent.Name = "IPSFactoryBuffer";
                    ent.TypeLibVersion = String.Empty;
                    break;
            }

            return ent;
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
                && NumMethods == right.NumMethods && Base == right.Base && TypeLib == right.TypeLib && TypeLibVersion == right.TypeLibVersion;
        }

        public override int GetHashCode()
        {
            return Name.GetSafeHashCode() ^ Iid.GetHashCode() ^ ProxyClsid.GetHashCode() ^ NumMethods.GetHashCode() 
                ^ Base.GetSafeHashCode() ^ TypeLib.GetHashCode() ^ TypeLibVersion.GetSafeHashCode();
        }

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
            Name = reader.ReadString("name");
            Iid = reader.ReadGuid("iid");
            ProxyClsid = reader.ReadGuid("proxy");
            NumMethods = reader.ReadInt("num");
            Base = reader.ReadString("base");
            TypeLibVersion = reader.ReadString("ver");
            TypeLib = reader.ReadGuid("tlib");

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
        }
    }
}
