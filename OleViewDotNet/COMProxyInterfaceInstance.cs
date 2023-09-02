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
using System;
using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet
{
    public class COMProxyInterfaceInstance : IProxyFormatter
    {
        /// <summary>
        /// The name of the proxy interface.
        /// </summary>
        public string Name => COMUtilities.DemangleWinRTName(Entry.Name);
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

        public IEnumerable<NdrComplexTypeReference> ComplexTypes { get; }

        public string Path => ClassEntry.DefaultServer;

        public Guid Clsid => ClassEntry.Clsid;

        public COMCLSIDEntry ClassEntry { get; }

        private readonly COMRegistry m_registry;

        private COMProxyInterfaceInstance(COMCLSIDEntry clsid, ISymbolResolver resolver, COMInterfaceEntry intf, COMRegistry registry)
        {
            NdrParser parser = new NdrParser(resolver);
            Entry = parser.ReadFromComProxyFile(clsid.DefaultServer, clsid.Clsid, new Guid[] { intf.Iid }).FirstOrDefault();
            ComplexTypes = parser.ComplexTypes;
            OriginalName = intf.Name;
            ClassEntry = clsid;
            m_registry = registry;
        }

        private static Dictionary<Guid, COMProxyInterfaceInstance> m_proxies = new Dictionary<Guid, COMProxyInterfaceInstance>();

        public static COMProxyInterfaceInstance GetFromIID(COMInterfaceEntry intf, ISymbolResolver resolver)
        {
            if (intf == null || !intf.HasProxy)
            {
                throw new ArgumentException($"Interface {intf.Name} doesn't have a registered proxy");
            }

            COMCLSIDEntry clsid = intf.ProxyClassEntry;
            if (m_proxies.ContainsKey(intf.Iid))
            {
                return m_proxies[intf.Iid];
            }
            else
            {
                var instance = new COMProxyInterfaceInstance(clsid, resolver, intf, clsid.Database);
                m_proxies[intf.Iid] = instance;
                return instance;
            }
        }

        public static COMProxyInterfaceInstance GetFromIID(COMInterfaceInstance intf, ISymbolResolver resolver)
        {
            return GetFromIID(intf.InterfaceEntry, resolver);
        }

        public string FormatText(ProxyFormatterFlags flags)
        {
            return COMUtilities.FormatProxy(m_registry, ComplexTypes, new NdrComProxyDefinition[] { Entry }, flags);
        }

        public string FormatText()
        {
            return FormatText(ProxyFormatterFlags.None);
        }
    }
}
