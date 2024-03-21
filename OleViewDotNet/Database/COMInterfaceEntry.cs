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
using OleViewDotNet.TypeLib;
using OleViewDotNet.Proxy;
using OleViewDotNet.Utilities;
using OleViewDotNet.Utilities.Format;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet.Database;

public class COMInterfaceEntry : IComparable<COMInterfaceEntry>, IXmlSerializable, 
    ICOMGuid, ICOMSourceCodeFormattable, ICOMSourceCodeParsable
{
    private readonly COMRegistry m_registry;
    private ICOMSourceCodeFormattable m_formattable;

    public int CompareTo(COMInterfaceEntry right)
    {
        return string.Compare(Name, right.Name);
    }

    private void LoadFromKey(RegistryKey key)
    {
        string name = key.GetValue(null) as string;
        if (!string.IsNullOrWhiteSpace(name?.Trim()) || m_registry.IidNameCache.TryGetValue(Iid, out name))
        {
            InternalName = name;
            m_registry.IidNameCache.TryAdd(Iid, InternalName);
        }
        else
        {
            InternalName = string.Empty;
        }

        ProxyClsid = key.ReadGuid("ProxyStubCLSID32", null);
        NumMethods = key.ReadInt("NumMethods", null);

        if (NumMethods < 3)
        {
            NumMethods = 3;
        }

        TypeLib = key.ReadGuid("TypeLib", null);
        TypeLibVersion = key.ReadString("TypeLib", "Version");
        Base = key.ReadString("BaseInterface", null);
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
        InternalName = name;
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
        m_registry.IidNameCache.TryAdd(Iid, InternalName);
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
            InternalName = name;
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

    public bool IsOleControl => Iid == COMKnownGuids.IID_IOleControl;

    public bool IsDispatch => Iid == COMKnownGuids.IID_IDispatch;

    public bool IsMarshal => Iid == COMKnownGuids.IID_IMarshal;

    public bool IsPersistStream => (Iid == COMKnownGuids.IID_IPersistStream) || (Iid == COMKnownGuids.IID_IPersistStreamInit);

    public bool IsClassFactory => Iid == typeof(IClassFactory).GUID;

    internal string InternalName { get; set; }

    private static COMInterfaceEntry CreateBuiltinEntry(COMRegistry registry, Guid iid, string name, int num_methods)
    {
        return new COMInterfaceEntry(registry)
        {
            Base = "",
            Iid = iid,
            ProxyClsid = Guid.Empty,
            NumMethods = num_methods,
            InternalName = name,
            TypeLibVersion = string.Empty,
            Source = COMRegistryEntrySource.Builtin
        };
    }

    public static COMInterfaceEntry CreateKnownInterface(COMRegistry registry, COMKnownInterfaces known)
    {
        return known switch
        {
            COMKnownInterfaces.IUnknown => CreateBuiltinEntry(registry, COMKnownGuids.IID_IUnknown, "IUnknown", 3),
            COMKnownInterfaces.IMarshal => CreateBuiltinEntry(registry, COMKnownGuids.IID_IMarshal, "IMarshal", 9),
            COMKnownInterfaces.IMarshal2 => CreateBuiltinEntry(registry, COMKnownGuids.IID_IMarshal2, "IMarshal2", 9),
            COMKnownInterfaces.IPSFactoryBuffer => CreateBuiltinEntry(registry, COMKnownGuids.IID_IPSFactoryBuffer, "IPSFactoryBuffer", 4),
            COMKnownInterfaces.IMarshalEnvoy => CreateBuiltinEntry(registry, COMKnownGuids.IID_IMarshalEnvoy, "IMarshalEnvoy", 7),
            COMKnownInterfaces.IStdMarshalInfo => CreateBuiltinEntry(registry, COMKnownGuids.IID_IStdMarshalInfo, "IStdMarshalInfo", 4),
            _ => null,
        };
    }

    public string Name => string.IsNullOrWhiteSpace(InternalName) ? Iid.FormatGuid() : COMUtilities.DemangleWinRTName(InternalName);

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
            if (typelib is not null)
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

    Guid ICOMGuid.ComGuid => Iid;

    bool ICOMSourceCodeFormattable.IsFormattable => RuntimeInterface 
                || TypeLibVersionEntry is not null || ProxyClassEntry is not null;

    private bool CheckForParsed()
    {
        if (m_formattable is not null)
            return true;
        if (RuntimeInterface && RuntimeMetadata.Interfaces.TryGetValue(Iid, out Type type))
        {
            m_formattable = new SourceCodeFormattableType(type);
        }
        else if (TypeLibVersionEntry?.IsParsed == true 
            && TypeLibVersionEntry.Parse().InterfacesByIid.TryGetValue(Iid, out COMTypeLibInterface intf))
        {
            m_formattable = intf;
        }
        else if (COMProxyInterface.TryGetFromIID(this, out COMProxyInterface proxy))
        {
            m_formattable = proxy;
        }
        return m_formattable is not null;
    }

    bool ICOMSourceCodeParsable.IsSourceCodeParsed => CheckForParsed();

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

        return InternalName == right.InternalName && Iid == right.Iid && ProxyClsid == right.ProxyClsid
            && NumMethods == right.NumMethods && Base == right.Base && TypeLib == right.TypeLib
            && TypeLibVersion == right.TypeLibVersion && RuntimeInterface == right.RuntimeInterface
            && Source == right.Source;
    }

    public override int GetHashCode()
    {
        return InternalName.GetSafeHashCode() ^ Iid.GetHashCode() ^ ProxyClsid.GetHashCode() ^ NumMethods.GetHashCode() 
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
        InternalName = reader.ReadString("name");
        Iid = reader.ReadGuid("iid");
        ProxyClsid = reader.ReadGuid("proxy");
        NumMethods = reader.ReadInt("num");
        Base = reader.ReadString("base");
        TypeLibVersion = reader.ReadString("ver");
        TypeLib = reader.ReadGuid("tlib");
        RuntimeInterface = reader.ReadBool("rt");
        Source = reader.ReadEnum<COMRegistryEntrySource>("src");

        if (!string.IsNullOrWhiteSpace(InternalName))
        {
            m_registry.IidNameCache.TryAdd(Iid, InternalName);
        }
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteOptionalAttributeString("name", InternalName);
        writer.WriteGuid("iid", Iid);
        writer.WriteGuid("proxy", ProxyClsid);
        writer.WriteInt("num", NumMethods);
        writer.WriteOptionalAttributeString("base", Base);
        writer.WriteOptionalAttributeString("ver", TypeLibVersion);
        writer.WriteGuid("tlib", TypeLib);
        writer.WriteBool("rt", RuntimeInterface);
        writer.WriteEnum("src", Source);
    }

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        m_formattable?.Format(builder);
    }

    void ICOMSourceCodeParsable.ParseSourceCode()
    {
        if (CheckForParsed())
            return;
        // If the runtime interface exists, that's already populated in CheckForParsed.
        if (TypeLibVersionEntry is not null)
        {
            var typelib = TypeLibVersionEntry.Parse();
            if (typelib.InterfacesByIid.TryGetValue(Iid, out COMTypeLibInterface intf))
            {
                m_formattable = intf;
            }
            else
            {
                m_formattable = new SourceCodeFormattableText("ERROR: Can't find type library for IID.");
            }
        }
        else if (HasProxy)
        {
            m_formattable = COMProxyInterface.GetFromIID(this);
        }
        else
        {
            m_formattable = new SourceCodeFormattableText("ERROR: Can't parse the interface source.");
        }
    }
}
