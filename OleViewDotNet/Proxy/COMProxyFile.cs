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

using NtApiDotNet;
using NtApiDotNet.Ndr;
using NtApiDotNet.Win32;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Proxy;

public class COMProxyFile : IProxyFormatter, ICOMSourceCodeFormattable
{
    #region Private Members
    private readonly COMRegistry m_registry;

    private static ISymbolResolver GetProxyParserSymbolResolver()
    {
        if (!ProgramSettings.ProxyParserResolveSymbols)
        {
            return null;
        }

        string dbghelp = ProgramSettings.DbgHelpPath;
        if (string.IsNullOrWhiteSpace(dbghelp))
        {
            return null;
        }

        return SymbolResolver.Create(NtProcess.Current, dbghelp, ProgramSettings.SymbolPath);
    }

    private COMProxyFile(string path, COMCLSIDEntry clsid, COMRegistry registry)
    {
        ClassEntry = clsid ?? new COMCLSIDEntry(registry, Guid.Empty, COMServerType.UnknownServer);
        m_registry = registry;

        using var resolver = GetProxyParserSymbolResolver();
        IntPtr pUnk = IntPtr.Zero;
        if (clsid is not null)
        {
            NativeMethods.CoGetClassObject(clsid.Clsid, CLSCTX.INPROC_SERVER,
                null, typeof(IPSFactoryBuffer).GUID, out pUnk);
        }
        try
        {
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
        finally
        {
            if (pUnk != IntPtr.Zero)
                Marshal.Release(pUnk);
        }
    }

    private COMProxyInterface GetInterfaceInstance(NdrComProxyDefinition proxy)
    {
        return new COMProxyInterface(ClassEntry, new(proxy), m_registry, this);
    }

    private COMProxyFile(string path, COMRegistry registry)
        : this(path, null, registry)
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
                         COMRegistry registry,
                         string name)
    {
        Entries = entries.Select(GetInterfaceInstance).ToList().AsReadOnly();
        ComplexTypes = complex_types.Select(t => new COMProxyComplexType(t)).ToList().AsReadOnly();
        m_registry = registry;
        Path = name;
    }
    #endregion

    #region Static Members
    public static bool TryGetFromCLSID(COMCLSIDEntry clsid, out COMProxyFile proxy)
    {
        return m_proxies.TryGetValue(clsid.Clsid, out proxy);
    }

    public static COMProxyFile GetFromCLSID(COMCLSIDEntry clsid)
    {
        if (clsid.IsAutomationProxy)
        {
            throw new ArgumentException("Can't get proxy for automation interfaces.");
        }
        if (m_proxies.TryGetValue(clsid.Clsid, out COMProxyFile proxy))
        {
            return proxy;
        }
        proxy = new(clsid.DefaultServer, clsid, clsid.Database);
        m_proxies[clsid.Clsid] = proxy;
        return proxy;
    }

    public static COMProxyFile GetFromFile(string path, COMRegistry registry)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));
        }

        if (registry is null)
        {
            throw new ArgumentNullException(nameof(registry));
        }

        if (m_proxies_by_file.ContainsKey(path))
        {
            return m_proxies_by_file[path];
        }
        else
        {
            COMProxyFile proxy = new(path, registry);
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
