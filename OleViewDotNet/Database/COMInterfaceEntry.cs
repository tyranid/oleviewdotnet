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
using OleViewDotNet.Interop;
using OleViewDotNet.Interop.SxS;
using OleViewDotNet.Utilities;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet.Database;

public class COMInterfaceEntry : IComparable<COMInterfaceEntry>, IXmlSerializable, IComGuid
{
    private readonly COMRegistry m_registry;
    private string m_name;

    public int CompareTo(COMInterfaceEntry right)
    {
        return string.Compare(Name, right.Name);
    }

    private void LoadFromKey(RegistryKey key)
    {
        string name = key.GetValue(null) as string;
        if (!string.IsNullOrWhiteSpace(name?.Trim()) || m_registry.IidNameCache.TryGetValue(Iid, out name))
        {
            m_name = name;
            m_registry.IidNameCache.TryAdd(Iid, m_name);
        }
        else
        {
            m_name = string.Empty;
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
        m_name = name;
        TypeLibVersion = string.Empty;
    }

    internal COMInterfaceEntry(COMRegistry registry, ActCtxComInterfaceRedirection intf_redirection) 
        : this(registry, intf_redirection.Iid, intf_redirection.ProxyStubClsid32, intf_redirection.NumMethods, 
              string.Empty, intf_redirection.Name)
    {
        TypeLib = intf_redirection.TypeLibraryId;
        Source = COMRegistryEntrySource.ActCtx;
    }

    internal COMInterfaceEntry(COMRegistry registry, Type type) 
        : this(registry, type.GUID, Guid.Empty, type.GetMethods().Length + 6, "IInspectable", type.FullName)
    {
        m_registry.IidNameCache.TryAdd(Iid, m_name);
        RuntimeInterface = true;
        Source = COMRegistryEntrySource.Metadata;
    }

    public COMInterfaceEntry(COMRegistry registry, Guid iid, RegistryKey rootKey)
        : this(registry, iid, Guid.Empty, 3, "IUnknown", string.Empty)
    {
        LoadFromKey(rootKey);
    }

    public COMInterfaceEntry(COMRegistry registry, Guid iid)
        : this(registry, iid, Guid.Empty, 3, "IUnknown", string.Empty)
    {
        if (m_registry.IidNameCache.TryGetValue(iid, out string name))
        {
            m_name = name;
        }
    }

    internal COMInterfaceEntry(COMRegistry registry, COMPackagedInterfaceEntry entry) 
        : this(registry, entry.Iid, entry.ProxyStubCLSID, 3, "IUnknown", string.Empty)
    {
        if (entry.UseUniversalMarshaler)
        {
            ProxyClsid = new Guid("00020424-0000-0000-C000-000000000046");
        }
        TypeLib = entry.TypeLibId;
        TypeLibVersion = entry.TypeLibVersionNumber;
        Source = COMRegistryEntrySource.Packaged;
    }

    public static Guid IID_IUnknown => new("{00000000-0000-0000-C000-000000000046}");

    public static Guid IID_IMarshal => new("{00000003-0000-0000-C000-000000000046}");

    public static Guid IID_IMarshal2 => new("000001CF-0000-0000-C000-000000000046");

    public static Guid IID_IContextMarshaler => new("000001D8-0000-0000-C000-000000000046");

    public static Guid IID_IStdMarshalInfo => new("00000018-0000-0000-C000-000000000046");

    public static Guid IID_IMarshalEnvoy => new("000001C8-0000-0000-C000-000000000046");

    public static Guid IID_IDispatch => new("00020400-0000-0000-c000-000000000046");

    public static Guid IID_IOleControl => new("{b196b288-bab4-101a-b69c-00aa00341d07}");

    public static Guid IID_IPersistStream => typeof(IPersistStream).GUID;

    public static Guid IID_IPersistStreamInit => typeof(IPersistStreamInit).GUID;

    public static Guid IID_IPSFactoryBuffer => new("D5F569D0-593B-101A-B569-08002B2DBF7A");

    public static Guid IID_IInspectable => new("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90");

    public bool IsOleControl => Iid == IID_IOleControl;

    public bool IsDispatch => Iid == IID_IDispatch;

    public bool IsMarshal => Iid == IID_IMarshal;

    public bool IsPersistStream => (Iid == IID_IPersistStream) || (Iid == IID_IPersistStreamInit);

    public bool IsClassFactory => Iid == typeof(IClassFactory).GUID;

    private static COMInterfaceEntry CreateBuiltinEntry(COMRegistry registry, Guid iid, string name, int num_methods)
    {
        return new COMInterfaceEntry(registry)
        {
            Base = "",
            Iid = iid,
            ProxyClsid = Guid.Empty,
            NumMethods = num_methods,
            m_name = name,
            TypeLibVersion = string.Empty,
            Source = COMRegistryEntrySource.Builtin
        };
    }

    public static COMInterfaceEntry CreateKnownInterface(COMRegistry registry, COMKnownInterfaces known)
    {
        return known switch
        {
            COMKnownInterfaces.IUnknown => CreateBuiltinEntry(registry, IID_IUnknown, "IUnknown", 3),
            COMKnownInterfaces.IMarshal => CreateBuiltinEntry(registry, IID_IMarshal, "IMarshal", 9),
            COMKnownInterfaces.IMarshal2 => CreateBuiltinEntry(registry, IID_IMarshal2, "IMarshal2", 9),
            COMKnownInterfaces.IPSFactoryBuffer => CreateBuiltinEntry(registry, IID_IPSFactoryBuffer, "IPSFactoryBuffer", 4),
            COMKnownInterfaces.IMarshalEnvoy => CreateBuiltinEntry(registry, IID_IMarshalEnvoy, "IMarshalEnvoy", 7),
            COMKnownInterfaces.IStdMarshalInfo => CreateBuiltinEntry(registry, IID_IStdMarshalInfo, "IStdMarshalInfo", 4),
            _ => null,
        };
    }

    public string Name => string.IsNullOrWhiteSpace(m_name) ? Iid.FormatGuid() : COMUtilities.DemangleWinRTName(m_name);

    public Guid Iid
    {
        get; private set;
    }

    public COMInterfaceEntry InterfaceEntry => m_registry.Interfaces.GetGuidEntry(Iid);

    public Guid ProxyClsid
    {
        get; private set;
    }

    public COMCLSIDEntry ProxyClassEntry => m_registry.Clsids.GetGuidEntry(ProxyClsid);

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

    public COMTypeLibEntry TypeLibEntry => m_registry.Typelibs.GetGuidEntry(TypeLib);

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

    public bool HasTypeLib => TypeLib != Guid.Empty;

    public bool HasProxy => ProxyClsid != Guid.Empty;

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

        if (obj is not COMInterfaceEntry right)
        {
            return false;
        }

        return m_name == right.m_name && Iid == right.Iid && ProxyClsid == right.ProxyClsid
            && NumMethods == right.NumMethods && Base == right.Base && TypeLib == right.TypeLib
            && TypeLibVersion == right.TypeLibVersion && RuntimeInterface == right.RuntimeInterface
            && Source == right.Source;
    }

    public override int GetHashCode()
    {
        return m_name.GetSafeHashCode() ^ Iid.GetHashCode() ^ ProxyClsid.GetHashCode() ^ NumMethods.GetHashCode() 
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
        m_name = reader.ReadString("name");
        Iid = reader.ReadGuid("iid");
        ProxyClsid = reader.ReadGuid("proxy");
        NumMethods = reader.ReadInt("num");
        Base = reader.ReadString("base");
        TypeLibVersion = reader.ReadString("ver");
        TypeLib = reader.ReadGuid("tlib");
        RuntimeInterface = reader.ReadBool("rt");
        Source = reader.ReadEnum<COMRegistryEntrySource>("src");

        if (!string.IsNullOrWhiteSpace(m_name))
        {
            m_registry.IidNameCache.TryAdd(Iid, m_name);
        }
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteOptionalAttributeString("name", m_name);
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
