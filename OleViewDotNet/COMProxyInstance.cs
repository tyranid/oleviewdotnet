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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using NtApiDotNet.Ndr;
using NtApiDotNet.Win32;

namespace OleViewDotNet
{
    [StructLayout(LayoutKind.Sequential)]
    struct ProxyFileInfo
    {
        public IntPtr pProxyVtblList;
        public IntPtr pStubVtblList;
        public IntPtr pNamesArray;
        public IntPtr pDelegatedIIDs;
        public IntPtr pIIDLookupRtn;
        public ushort TableSize;
        public ushort TableVersion;
        
        public string[] GetNames()
        {
            return COMUtilities.ReadPointerArray(pNamesArray, TableSize, i => Marshal.PtrToStringAnsi(i));
        }

        public Guid[] GetBaseIids()
        {
            return COMUtilities.ReadPointerArray(pDelegatedIIDs, TableSize, i => COMUtilities.ReadGuid(i));
        }

        public CInterfaceStubHeader[] GetStubs()
        {
            return COMUtilities.ReadPointerArray<CInterfaceStubHeader>(pStubVtblList, TableSize);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct CInterfaceStubHeader
    {
        public IntPtr piid;
        public IntPtr pServerInfo;
        public int DispatchTableCount;
        public IntPtr pDispatchTable;

        public Guid GetIid()
        {
            return COMUtilities.ReadGuid(piid);
        }
    }

    public class COMProxyInstanceEntry
    {
        public string Name { get; private set; }
        public Guid Iid { get; private set; }
        public Guid BaseIid { get; private set; }
        public int DispatchCount { get; private set; }
        public NdrProcedureDefinition[] Procs { get; private set; }

        internal COMProxyInstanceEntry(string name, 
            Guid iid, Guid base_iid, int dispatch_count, NdrProcedureDefinition[] procs)
        {
            Name = COMUtilities.DemangleWinRTName(name);
            Iid = iid;
            BaseIid = base_iid == Guid.Empty ? COMInterfaceEntry.IID_IUnknown : base_iid;
            DispatchCount = dispatch_count;
            Procs = procs;
        }

        public string Format(IDictionary<Guid, string> iids_to_names)
        {
            INdrFormatter formatter = DefaultNdrFormatter.Create(iids_to_names);
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("[Guid(\"{0}\")]", Iid).AppendLine();
            string base_name = iids_to_names.ContainsKey(BaseIid) ?
                iids_to_names[BaseIid] : String.Format("/* Unknown IID {0} */ IUnknown", BaseIid);
            builder.AppendFormat("interface {0} : {1} {{", Name, base_name).AppendLine();
            foreach (NdrProcedureDefinition proc in Procs)
            {
                builder.AppendFormat("    {0}", formatter.FormatProcedure(proc)).AppendLine();
            }
            builder.AppendLine("}").AppendLine();
            return builder.ToString();
        }
    }

    public class COMProxyInstance
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void GetProxyDllInfo(out IntPtr pInfo, out IntPtr pId);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int DllGetClassObject(ref Guid clsid, ref Guid riid, out IntPtr ppv);

        public IEnumerable<COMProxyInstanceEntry> Entries { get; private set; }

        public IEnumerable<NdrComplexTypeReference> ComplexTypes { get; private set; }

        internal COMProxyInstance(IEnumerable<COMProxyInstanceEntry> entries, 
                                  IEnumerable<NdrComplexTypeReference> complex_types)
        {
            Entries = new List<COMProxyInstanceEntry>(entries).AsReadOnly();
            ComplexTypes = new List<NdrComplexTypeReference>(complex_types).AsReadOnly();
        }
        
        private NdrProcedureDefinition[] ReadProcs(NdrParser parser, Guid base_iid, CInterfaceStubHeader stub)
        {
            int start_ofs = 3;
            if (base_iid == COMInterfaceEntry.IID_IDispatch)
            {
                start_ofs = 7;
            }

            return parser.ReadFromMidlServerInfo(stub.pServerInfo, start_ofs, stub.DispatchTableCount).ToArray();
        }

        private static IntPtr FindProxyDllInfo(SafeLibraryHandle lib, Guid clsid)
        {
            try
            {
                GetProxyDllInfo get_proxy_dllinfo = lib.GetFunctionPointer<GetProxyDllInfo>();
                IntPtr pInfo;
                IntPtr pId;
                get_proxy_dllinfo(out pInfo, out pId);
                return pInfo;
            }
            catch (Win32Exception)
            {
            }

            IntPtr psfactory = IntPtr.Zero;
            try
            {
                DllGetClassObject dll_get_class_object = lib.GetFunctionPointer<DllGetClassObject>();
                Guid IID_IPSFactoryBuffer = COMInterfaceEntry.IID_IPSFactoryBuffer;

                int hr = dll_get_class_object(ref clsid, ref IID_IPSFactoryBuffer, out psfactory);
                if (hr != 0)
                {
                    throw new Win32Exception(hr);
                }

                // The PSFactoryBuffer object seems to be structured like on Win10 at least.
                // VTABLE*
                // Reference Count
                // ProxyFileInfo*

                IntPtr pInfo = Marshal.ReadIntPtr(psfactory, 2 * IntPtr.Size);
                // TODO: Should add better checks here, 
                // for example VTable should be in COMBASE and the pointer should be in the
                // server DLL's rdata section. But this is probably good enough for now.
                using (SafeLibraryHandle module = COMUtilities.SafeGetModuleHandle(pInfo))
                {
                    if (module == null || lib.DangerousGetHandle() != module.DangerousGetHandle())
                    {
                        return IntPtr.Zero;
                    }
                }

                return pInfo;
            }
            catch (Win32Exception)
            {
                return IntPtr.Zero;
            }
            finally
            {
                if (psfactory != IntPtr.Zero)
                {
                    Marshal.Release(psfactory);
                }
            }
        }

        private bool InitFromFileInfo(IntPtr pInfo, ISymbolResolver resolver)
        {
            List<COMProxyInstanceEntry> entries = new List<COMProxyInstanceEntry>();
            List<NdrComplexTypeReference> complex_types = new List<NdrComplexTypeReference>();
            NdrParser parser = new NdrParser(resolver);

            foreach (var file_info in COMUtilities.EnumeratePointerList<ProxyFileInfo>(pInfo))
            {
                string[] names = file_info.GetNames();
                CInterfaceStubHeader[] stubs = file_info.GetStubs();
                Guid[] base_iids = file_info.GetBaseIids();
                for (int i = 0; i < names.Length; ++i)
                {
                    entries.Add(new COMProxyInstanceEntry(names[i], stubs[i].GetIid(),
                        base_iids[i], stubs[i].DispatchTableCount, ReadProcs(parser, base_iids[i], stubs[i])));
                }
            }

            complex_types.AddRange(parser.Types.OfType<NdrBaseStructureTypeReference>());
            complex_types.AddRange(parser.Types.OfType<NdrUnionTypeReference>());
            Entries = entries.AsReadOnly();
            ComplexTypes = complex_types.AsReadOnly();
            return true;
        }

        private bool InitFromFile(string path, Guid clsid, ISymbolResolver resolver)
        {
            using (SafeLibraryHandle lib = COMUtilities.SafeLoadLibrary(path))
            {
                IntPtr pInfo = FindProxyDllInfo(lib, clsid);
                if (pInfo == IntPtr.Zero)
                {
                    return false;
                }

                return InitFromFileInfo(pInfo, resolver);
            }
        }

        public COMProxyInstance(string path, ISymbolResolver resolver)
        {
            if (!InitFromFile(path, Guid.Empty, resolver))
            {
                throw new ArgumentException("Can't find proxy information in server DLL");
            }
        }

        private COMProxyInstance(COMCLSIDEntry clsid, ISymbolResolver resolver)
        {
            if (!InitFromFile(clsid.DefaultServer, clsid.Clsid, resolver))
            {
                throw new ArgumentException("Can't find proxy information in server DLL");
            }
        }

        private static Dictionary<Guid, COMProxyInstance> m_proxies = new Dictionary<Guid, COMProxyInstance>();
        private static Dictionary<string, COMProxyInstance> m_proxies_by_file = new Dictionary<string, COMProxyInstance>(StringComparer.OrdinalIgnoreCase);

        public static COMProxyInstance GetFromCLSID(COMCLSIDEntry clsid, ISymbolResolver resolver)
        {
            if (m_proxies.ContainsKey(clsid.Clsid))
            {
                return m_proxies[clsid.Clsid];
            }
            else
            {
                COMProxyInstance proxy = new COMProxyInstance(clsid, resolver);
                m_proxies[clsid.Clsid] = proxy;
                return proxy;
            }
        }

        public static COMProxyInstance GetFromFile(string path, ISymbolResolver resolver)
        {
            if (m_proxies_by_file.ContainsKey(path))
            {
                return m_proxies_by_file[path];
            }
            else
            {
                COMProxyInstance proxy = new COMProxyInstance(path, resolver);
                m_proxies_by_file[path] = proxy;
                return proxy;
            }
        }
    }
}
