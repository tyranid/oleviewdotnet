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

public class COMProxyInstance : IProxyFormatter, ICOMSourceCodeFormattable
{
    private readonly COMRegistry m_registry;

    public IReadOnlyList<NdrComProxyDefinition> Entries { get; private set; }

    public IReadOnlyList<NdrComplexTypeReference> ComplexTypes { get; private set; }

    public string Path { get; }

    public Guid Clsid { get; }

    public COMCLSIDEntry ClassEntry { get; }

    internal COMProxyInstance(IEnumerable<NdrComProxyDefinition> entries,
                              IEnumerable<NdrComplexTypeReference> complex_types,
                              COMCLSIDEntry clsid,
                              COMRegistry registry)
    {
        Entries = new List<NdrComProxyDefinition>(entries).AsReadOnly();
        ComplexTypes = new List<NdrComplexTypeReference>(complex_types).AsReadOnly();
        ClassEntry = clsid;
        m_registry = registry;
    }

    private COMProxyInstance(string path, COMCLSIDEntry clsid, ISymbolResolver resolver, COMRegistry registry)
    {
        NdrParser parser = new(resolver);
        Clsid = clsid?.Clsid ?? Guid.Empty;
        Entries = parser.ReadFromComProxyFile(path, Clsid).ToList().AsReadOnly();
        ComplexTypes = parser.ComplexTypes.ToList().AsReadOnly();
        Path = clsid?.DefaultServer ?? path;
        ClassEntry = clsid;
        m_registry = registry;
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
                    entry.Name = name;
                }
                else
                {
                    entry.Name = $"intf_{entry.Iid.ToString().Replace('-', '_')}";
                }
            }
        }
    }

    private COMProxyInstance(string path, ISymbolResolver resolver, COMRegistry registry) 
        : this(path, null, resolver, registry)
    {
    }

    private static readonly Dictionary<Guid, COMProxyInstance> m_proxies = new();
    private static readonly Dictionary<string, COMProxyInstance> m_proxies_by_file = new(StringComparer.OrdinalIgnoreCase);

    public static COMProxyInstance GetFromCLSID(COMCLSIDEntry clsid, ISymbolResolver resolver)
    {
        if (clsid.IsAutomationProxy)
        {
            throw new ArgumentException("Can't get proxy for automation interfaces.");
        }
        if (m_proxies.ContainsKey(clsid.Clsid))
        {
            return m_proxies[clsid.Clsid];
        }
        else
        {
            COMProxyInstance proxy = new(clsid.DefaultServer, clsid, resolver, clsid.Database);
            m_proxies[clsid.Clsid] = proxy;
            return proxy;
        }
    }

    public static COMProxyInstance GetFromFile(string path, ISymbolResolver resolver, COMRegistry registry)
    {
        if (m_proxies_by_file.ContainsKey(path))
        {
            return m_proxies_by_file[path];
        }
        else
        {
            COMProxyInstance proxy = new(path, resolver, registry);
            m_proxies_by_file[path] = proxy;
            return proxy;
        }
    }

    public string FormatText(ProxyFormatterFlags flags = ProxyFormatterFlags.None)
    {
        COMSourceCodeBuilder builder = new(m_registry);
        builder.RemoveComments = flags.HasFlag(ProxyFormatterFlags.RemoveComments);
        builder.RemoveComplexTypes = flags.HasFlag(ProxyFormatterFlags.RemoveComplexTypes);
        ((ICOMSourceCodeFormattable)this).Format(builder);
        return builder.ToString();
    }

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        INdrFormatter formatter = builder.GetNdrFormatter();
        if (!builder.RemoveComplexTypes)
        {
            foreach (var type in ComplexTypes)
            {
                builder.AppendLine(formatter.FormatComplexType(type));
            }
            builder.AppendLine();
        }

        foreach (var proxy in Entries)
        {
            builder.AppendLine(formatter.FormatComProxy(proxy));
        }
    }
}
