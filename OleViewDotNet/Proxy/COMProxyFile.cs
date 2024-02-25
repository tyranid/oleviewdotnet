//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
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

using NtApiDotNet.Ndr;
using NtApiDotNet.Win32;
using OleViewDotNet.Database;
using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet.Proxy;

public class COMProxyFile : IProxyFormatter, ICOMSourceCodeFormattable
{
    #region Private Members
    private readonly COMRegistry m_registry;

    private COMProxyFile(string path, COMCLSIDEntry clsid, ISymbolResolver resolver, COMRegistry registry)
    {
        ClassEntry = clsid ?? new COMCLSIDEntry(registry, Guid.Empty, COMServerType.UnknownServer);
        m_registry = registry;

        NdrParser parser = new(resolver);
        Entries = parser.ReadFromComProxyFile(path, Clsid).Select(GetInterfaceInstance).ToList().AsReadOnly();
        ComplexTypes = parser.ComplexTypes.Select(t => new COMProxyComplexType(t)).ToList().AsReadOnly();
        Path = clsid?.DefaultServer ?? path;
        foreach (var entry in Entries)
        {
            if (!string.IsNullOrWhiteSpace(entry.Name))
            {
                m_registry.IidNameCache.TryAdd(entry.Iid, entry.Name);
            }
            else
            {
                if (m_registry.IidNameCache.TryGetValue(entry.Iid, out string name))
                {
                    entry.Entry.Name = name;
                }
                else
                {
                    entry.Entry.Name = $"intf_{entry.Iid.ToString().Replace('-', '_')}";
                }
            }
        }
    }

    private IEnumerable<NdrBaseTypeReference> GetStructTypes(NdrComplexTypeReference complex_type)
    {
        if (complex_type is NdrBaseStructureTypeReference struct_type)
        {
            return struct_type.MembersTypes;
        }
        else if (complex_type is NdrUnionTypeReference union_type)
        {
            return union_type.Arms.Arms.Select(a => a.ArmType);
        }
        return Array.Empty<NdrBaseTypeReference>();
    }

    private void GetComplexTypes(HashSet<NdrComplexTypeReference> complex_types, NdrBaseTypeReference type)
    {
        if (type is NdrComplexTypeReference complex_type)
        {
            if (complex_types.Add(complex_type))
            {
                foreach (var member_type in GetStructTypes(complex_type))
                {
                    GetComplexTypes(complex_types, member_type);
                }
            }
        }
        else if (type is NdrBaseArrayTypeReference array_type)
        {
            GetComplexTypes(complex_types, array_type.ElementType);
        }
        else if (type is NdrPointerTypeReference pointer_type)
        {
            GetComplexTypes(complex_types, pointer_type.Type);
        }
    }

    private COMProxyInterface GetInterfaceInstance(NdrComProxyDefinition proxy)
    {
        HashSet<NdrComplexTypeReference> complex_types = new();
        foreach (var proc in proxy.Procedures)
        {
            GetComplexTypes(complex_types, proc.ReturnValue.Type);
            foreach (var p in proc.Params)
            {
                GetComplexTypes(complex_types, p.Type);
            }
        }
        return new COMProxyInterface(ClassEntry, 
            proxy, complex_types, m_registry, this);
    }

    private COMProxyFile(string path, ISymbolResolver resolver, COMRegistry registry)
        : this(path, null, resolver, registry)
    {
    }

    private static readonly Dictionary<Guid, COMProxyFile> m_proxies = new();
    private static readonly Dictionary<string, COMProxyFile> m_proxies_by_file = new(StringComparer.OrdinalIgnoreCase);
    #endregion

    #region Public Properties
    public IReadOnlyList<COMProxyInterface> Entries { get; private set; }

    public IReadOnlyList<COMProxyComplexType> ComplexTypes { get; private set; }

    public string Path { get; }

    public Guid Clsid => ClassEntry.Clsid;

    public COMCLSIDEntry ClassEntry { get; }

    bool ICOMSourceCodeFormattable.IsFormattable => true;
    #endregion

    #region Internal Members
    internal COMProxyFile(IEnumerable<NdrComProxyDefinition> entries,
                              IEnumerable<NdrComplexTypeReference> complex_types,
                              COMCLSIDEntry clsid,
                              COMRegistry registry)
    {
        Entries = entries.Select(GetInterfaceInstance).ToList().AsReadOnly();
        ComplexTypes = complex_types.Select(t => new COMProxyComplexType(t)).ToList().AsReadOnly();
        ClassEntry = clsid;
        m_registry = registry;
    }
    #endregion

    #region Static Members
    public static bool TryGetFromCLSID(COMCLSIDEntry clsid, out COMProxyFile proxy)
    {
        return m_proxies.TryGetValue(clsid.Clsid, out proxy);
    }

    public static COMProxyFile GetFromCLSID(COMCLSIDEntry clsid, ISymbolResolver resolver)
    {
        if (clsid.IsAutomationProxy)
        {
            throw new ArgumentException("Can't get proxy for automation interfaces.");
        }
        if (m_proxies.TryGetValue(clsid.Clsid, out COMProxyFile proxy))
        {
            return proxy;
        }
        proxy = new(clsid.DefaultServer, clsid, resolver, clsid.Database);
        m_proxies[clsid.Clsid] = proxy;
        return proxy;
    }

    public static COMProxyFile GetFromFile(string path, ISymbolResolver resolver, COMRegistry registry)
    {
        if (m_proxies_by_file.ContainsKey(path))
        {
            return m_proxies_by_file[path];
        }
        else
        {
            COMProxyFile proxy = new(path, resolver, registry);
            m_proxies_by_file[path] = proxy;
            return proxy;
        }
    }
    #endregion

    #region Public Methods
    public string FormatText(ProxyFormatterFlags flags = ProxyFormatterFlags.None)
    {
        COMSourceCodeBuilder builder = new(m_registry);
        builder.HideComments = flags.HasFlag(ProxyFormatterFlags.RemoveComments);
        builder.InterfacesOnly = flags.HasFlag(ProxyFormatterFlags.RemoveComplexTypes);
        ((ICOMSourceCodeFormattable)this).Format(builder);
        return builder.ToString();
    }

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        INdrFormatter formatter = builder.GetNdrFormatter();
        if (!builder.InterfacesOnly)
        {
            foreach (var type in ComplexTypes)
            {
                builder.AppendLine(formatter.FormatComplexType(type.Entry));
            }
            builder.AppendLine();
        }

        foreach (var proxy in Entries)
        {
            builder.AppendLine(formatter.FormatComProxy(proxy.Entry));
        }
    }
    #endregion
}
