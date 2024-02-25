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

using NtApiDotNet.Ndr;
using NtApiDotNet.Win32;
using OleViewDotNet.Database;
using OleViewDotNet.Utilities;
using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet.Proxy;

public sealed class COMProxyInterface : IProxyFormatter, ICOMSourceCodeFormattable
{
    #region Private Members
    private static readonly Dictionary<Guid, COMProxyInterface> m_proxies = new();
    private readonly COMRegistry m_registry;
    #endregion

    #region Public Properties
    /// <summary>
    /// The name of the proxy interface.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Original name of the interface.
    /// </summary>
    public string OriginalName { get; }
    /// <summary>
    /// The IID of the proxy interface.
    /// </summary>
    public Guid Iid => Entry.Iid;
    /// <summary>
    /// The base IID of the proxy interface.
    /// </summary>
    public Guid BaseIid => Entry.BaseIid;
    /// <summary>
    /// Get name of the base IID if known.
    /// </summary>
    public string BaseIidName => m_registry.MapIidToInterface(BaseIid).Name;
    /// <summary>
    /// The number of dispatch methods on the interface.
    /// </summary>
    public int DispatchCount => Entry.DispatchCount;
    /// <summary>
    /// List of parsed procedures for the interface.
    /// </summary>
    public IList<NdrProcedureDefinition> Procedures => Entry.Procedures;

    public NdrComProxyDefinition Entry { get; }

    public IReadOnlyList<NdrComplexTypeReference> ComplexTypes { get; }

    public string Path => ClassEntry.DefaultServer;

    public Guid Clsid => ClassEntry.Clsid;

    public COMCLSIDEntry ClassEntry { get; }

    public COMProxyFile ProxyFile { get; }

    bool ICOMSourceCodeFormattable.IsFormattable => true;
    #endregion

    #region Internal Members
    internal COMProxyInterface(COMCLSIDEntry clsid, NdrComProxyDefinition entry,
            IEnumerable<NdrComplexTypeReference> complex_types, COMRegistry registry, COMProxyFile proxy)
    {
        ClassEntry = clsid;
        Entry = entry;
        ProxyFile = proxy;
        m_registry = registry;
        if (string.IsNullOrWhiteSpace(Entry.Name))
        {
            Name = m_registry.MapIidToInterface(Iid).Name;
        }
        else
        {
            Name = COMUtilities.DemangleWinRTName(Entry.Name, Iid);
        }
        ComplexTypes = complex_types.ToList().AsReadOnly();
        if (!m_proxies.ContainsKey(Iid))
        {
            m_proxies.Add(Iid, this);
        }
    }
    #endregion

    #region Public Static Methods
    public static bool TryGetFromIID(COMInterfaceEntry intf, out COMProxyInterface proxy)
    {
        return m_proxies.TryGetValue(intf.Iid, out proxy);
    }

    public static COMProxyInterface GetFromIID(COMInterfaceEntry intf, ISymbolResolver resolver)
    {
        if (intf == null || !intf.HasProxy)
        {
            throw new ArgumentException($"Interface {intf.Name} doesn't have a registered proxy");
        }

        if (m_proxies.TryGetValue(intf.Iid, out COMProxyInterface instance))
        {
            return instance;
        }

        COMCLSIDEntry clsid = intf.ProxyClassEntry;
        if (clsid.IsAutomationProxy)
        {
            throw new ArgumentException("Can't get proxy for automation interfaces.");
        }

        COMProxyFile.GetFromCLSID(clsid, resolver);
        if (!m_proxies.TryGetValue(intf.Iid, out instance))
        {
            throw new ArgumentException($"No Proxy Found for IID {intf.Iid}");
        }

        return instance;
    }

    public static COMProxyInterface GetFromIID(COMInterfaceInstance intf, ISymbolResolver resolver)
    {
        return GetFromIID(intf.InterfaceEntry, resolver);
    }
    #endregion

    #region Public Methods
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
        if (!builder.RemoveComplexTypes && ComplexTypes.Count > 0)
        {
            foreach (var type in ComplexTypes)
            {
                builder.AppendLine(formatter.FormatComplexType(type));
            }
            builder.AppendLine();
        }

        builder.AppendLine(formatter.FormatComProxy(Entry));
    }
    #endregion
}
