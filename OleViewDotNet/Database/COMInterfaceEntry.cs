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
using OleViewDotNet.Proxy;
using OleViewDotNet.TypeLib;
using OleViewDotNet.Utilities;
using OleViewDotNet.Utilities.Format;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OleViewDotNet.Database;

public class COMInterfaceEntry : COMRegistryEntry, IComparable<COMInterfaceEntry>, IXmlSerializable,
    ICOMGuid, ICOMSourceCodeFormattable, ICOMSourceCodeParsable, ICOMRuntimeType
{
    #region Private Members
    private ICOMSourceCodeFormattable m_formattable;

    private void LoadFromKey(RegistryKey key)
    {
        string name = key.GetValue(null) as string;
        if (!string.IsNullOrWhiteSpace(name?.Trim()) || Database.IidNameCache.TryGetValue(Iid, out name))
        {
            InternalName = name;
            Database.IidNameCache.TryAdd(Iid, InternalName);
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

    private bool CheckForParsed()
    {
        if (m_formattable is not null)
            return true;
        if (HasRuntimeType && TryGetRuntimeType(out Type type))
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

    #endregion

    #region Public Methods
    public int CompareTo(COMInterfaceEntry right)
    {
        return string.Compare(Name, right.Name);
    }

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
            && TypeLibVersion == right.TypeLibVersion && RuntimeTypeName == right.RuntimeTypeName
            && Source == right.Source && IsWinRTType == right.IsWinRTType;
    }

    public override int GetHashCode()
    {
        return InternalName.GetSafeHashCode() ^ Iid.GetHashCode() ^ ProxyClsid.GetHashCode() ^ NumMethods.GetHashCode()
            ^ Base.GetSafeHashCode() ^ TypeLib.GetHashCode() ^ TypeLibVersion.GetSafeHashCode() ^ RuntimeTypeName.GetSafeHashCode()
            ^ Source.GetHashCode() ^ IsWinRTType.GetHashCode();
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
    #endregion

    #region Constructors
    internal COMInterfaceEntry(COMRegistry registry) : base(registry)
    {
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
        Database.IidNameCache.TryAdd(Iid, InternalName);
        RuntimeTypeName = type.AssemblyQualifiedName;
        Source = COMRegistryEntrySource.Metadata;
        IsWinRTType = true;
    }

    public COMInterfaceEntry(COMRegistry registry, Guid iid, RegistryKey rootKey)
        : this(registry, iid, Guid.Empty, 3, "IUnknown", string.Empty)
    {
        LoadFromKey(rootKey);
    }

    public COMInterfaceEntry(COMRegistry registry, Guid iid)
        : this(registry, iid, Guid.Empty, 3, "IUnknown", string.Empty)
    {
        if (Database.IidNameCache.TryGetValue(iid, out string name))
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
    #endregion

    #region Public Properties
    public bool IsOleControl => Iid == COMKnownGuids.IID_IOleControl;

    public bool IsDispatch => Iid == COMKnownGuids.IID_IDispatch;

    public bool IsMarshal => Iid == COMKnownGuids.IID_IMarshal;

    public bool IsPersistStream => (Iid == COMKnownGuids.IID_IPersistStream) || (Iid == COMKnownGuids.IID_IPersistStreamInit);

    public bool IsClassFactory => Iid == typeof(IClassFactory).GUID;

    internal string InternalName { get; set; }

    public string Name => string.IsNullOrWhiteSpace(InternalName) ? Iid.FormatGuid() : WinRTNameUtils.DemangleName(InternalName);

    public Guid Iid
    {
        get; private set;
    }

    public COMInterfaceEntry InterfaceEntry => Database.Interfaces.GetGuidEntry(Iid);

    public Guid ProxyClsid
    {
        get; private set;
    }

    public COMCLSIDEntry ProxyClassEntry => Database.Clsids.GetGuidEntry(ProxyClsid);

    public bool IsAutomationProxy => ProxyClassEntry?.IsAutomationProxy ?? false;

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

    public COMTypeLibEntry TypeLibEntry => Database.Typelibs.GetGuidEntry(TypeLib);

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

    public bool IsWinRTType { get; internal set; }
    #endregion

    #region Static Members
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
    #endregion

    #region ICOMGuid Implementation
    Guid ICOMGuid.ComGuid => Iid;
    #endregion

    #region ICOMSourceCodeFormattable Implementation
    bool ICOMSourceCodeFormattable.IsFormattable => HasRuntimeType 
                || TypeLibVersionEntry is not null || ProxyClassEntry is not null;

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        m_formattable?.Format(builder);
    }
    #endregion

    #region ICOMSourceCodeParsable Implementation
    bool ICOMSourceCodeParsable.IsSourceCodeParsed => CheckForParsed();

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
    #endregion

    #region IXmlSerializable Implementation
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
        IsWinRTType = reader.ReadBool("rt");
        RuntimeTypeName = reader.ReadString("rta");
        Source = reader.ReadEnum<COMRegistryEntrySource>("src");

        if (!string.IsNullOrWhiteSpace(InternalName))
        {
            Database.IidNameCache.TryAdd(Iid, InternalName);
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
        writer.WriteBool("rt", IsWinRTType);
        writer.WriteOptionalAttributeString("rta", RuntimeTypeName);
        writer.WriteEnum("src", Source);
    }
    #endregion

    #region ICOMRuntimeType Implementation
    public string RuntimeTypeName
    {
        get; internal set;
    }

    public bool HasRuntimeType => !string.IsNullOrEmpty(RuntimeTypeName);

    public Type GetRuntimeType()
    {
        if (!HasRuntimeType)
        {
            return null;
        }
        return Type.GetType(RuntimeTypeName);
    }

    public bool TryGetRuntimeType(out Type type)
    {
        try
        {
            type = GetRuntimeType();
            return type is not null;
        }
        catch
        {
            type = null;
            return false;
        }
    }
    #endregion
}
