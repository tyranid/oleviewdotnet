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

        public MIDL_SERVER_INFO GetServerInfo()
        {
            if (pServerInfo == IntPtr.Zero)
            {
                return new MIDL_SERVER_INFO();
            }
            return Marshal.PtrToStructure<MIDL_SERVER_INFO>(pServerInfo);
        }

        public Guid GetIid()
        {
            return COMUtilities.ReadGuid(piid);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MIDL_STUB_DESC
    {
        public IntPtr RpcInterfaceInformation;
        public IntPtr pfnAllocate;
        public IntPtr pfnFree;
        public IntPtr pGenericBindingInfo;
        public IntPtr apfnNdrRundownRoutines;
        public IntPtr aGenericBindingRoutinePairs;
        public IntPtr apfnExprEval;
        public IntPtr aXmitQuintuple;
        public IntPtr pFormatTypes;
        public int fCheckBounds;
        /* Ndr library version. */
        public int Version;
        public IntPtr pMallocFreeStruct;
        public int MIDLVersion;
        public IntPtr CommFaultOffsets;
        // New fields for version 3.0+
        public IntPtr aUserMarshalQuadruple;
        // Notify routines - added for NT5, MIDL 5.0
        public IntPtr NotifyRoutineTable;
        public IntPtr mFlags;
        // International support routines - added for 64bit post NT5
        public IntPtr CsRoutineTables;
        public IntPtr ProxyServerInfo;
        public IntPtr pExprInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MIDL_SERVER_INFO
    {
        public IntPtr pStubDesc;
        public IntPtr DispatchTable;
        public IntPtr ProcString;
        public IntPtr FmtStringOffset;
        public IntPtr ThunkTable;
        public IntPtr pTransferSyntax;
        public IntPtr nCount;
        public IntPtr pSyntaxInfo;

        public MIDL_STUB_DESC GetStubDesc()
        {
            if (pStubDesc == IntPtr.Zero)
            {
                return new MIDL_STUB_DESC();
            }
            return Marshal.PtrToStructure<MIDL_STUB_DESC>(pStubDesc);
        }
    }

    public class COMProxyInstanceEntry
    {
        private COMProxyInstance _instance;

        public string Name { get; private set; }
        public Guid Iid { get; private set; }
        public Guid BaseIid { get; private set; }
        public int DispatchCount { get; private set; }
        public NdrProcedureDefinition[] Procs { get; private set; }

        internal COMProxyInstanceEntry(COMProxyInstance instance, string name, Guid iid, Guid base_iid, int dispatch_count, NdrProcedureDefinition[] procs)
        {
            _instance = instance;
            Name = COMUtilities.DemangleWinRTName(name);
            Iid = iid;
            BaseIid = base_iid == Guid.Empty ? COMInterfaceEntry.IID_IUnknown : base_iid;
            DispatchCount = dispatch_count;
            Procs = procs;
        }

        public string Format(NdrFormatContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("[Guid(\"{0}\")]", Iid).AppendLine();
            string base_name = context.IidsToNames.ContainsKey(BaseIid) ? 
                context.IidsToNames[BaseIid] : String.Format("/* Unknown IID {0} */ IUnknown", BaseIid);
            builder.AppendFormat("interface {0} : {1} {{", Name, base_name).AppendLine();
            foreach (NdrProcedureDefinition proc in Procs)
            {
                builder.AppendFormat("    {0}", proc.FormatProcedure(context)).AppendLine();
            }
            builder.AppendLine("}").AppendLine();
            return builder.ToString();
        }

        public string Format(IDictionary<Guid, string> iids_to_names)
        {
            NdrFormatContext context = new NdrFormatContext(iids_to_names, _instance.ComplexTypesWithNames, true);
            return Format(context);
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

        private static string GetComplexTypeName(NdrComplexTypeReference type, int index)
        {
            if (type is NdrBaseStructureTypeReference)
            {
                return String.Format("Struct_{0}", index);
            }
            else if (type is NdrUnionTypeReference)
            {
                return String.Format("Union_{0}", index);
            }
            else
            {
                return String.Format("UnknownType_{0}", index);
            }
        }
        
        public Dictionary<NdrComplexTypeReference, string> ComplexTypesWithNames
        {
            get
            {
                return ComplexTypes.Select((s, i) =>
                    new Tuple<NdrComplexTypeReference, string>(s, GetComplexTypeName(s, i))).ToDictionary(v => v.Item1, v => v.Item2);
            }
        }
        
        private NdrProcedureDefinition[] ReadProcs(Dictionary<IntPtr, NdrBaseTypeReference> type_cache, Guid base_iid, CInterfaceStubHeader stub)
        {
            MIDL_SERVER_INFO server_info = stub.GetServerInfo();
            MIDL_STUB_DESC stub_desc = server_info.GetStubDesc();
            IntPtr type_desc = stub_desc.pFormatTypes;
            int start_ofs = 3;
            if (base_iid == COMInterfaceEntry.IID_IDispatch)
            {
                start_ofs = 7;
            }

            List<NdrProcedureDefinition> procs = new List<NdrProcedureDefinition>();
            while (start_ofs < stub.DispatchTableCount)
            {
                int fmt_ofs = Marshal.ReadInt16(server_info.FmtStringOffset, start_ofs * 2);
                if (fmt_ofs >= 0)
                {
                    procs.Add(new NdrProcedureDefinition(type_cache, stub_desc, server_info.ProcString + fmt_ofs, type_desc));
                }
                start_ofs++;
            }
            return procs.ToArray();
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

        private bool InitFromFileInfo(IntPtr pInfo)
        {
            List<COMProxyInstanceEntry> entries = new List<COMProxyInstanceEntry>();
            List<NdrComplexTypeReference> complex_types = new List<NdrComplexTypeReference>();

            foreach (var file_info in COMUtilities.EnumeratePointerList<ProxyFileInfo>(pInfo))
            {
                string[] names = file_info.GetNames();
                CInterfaceStubHeader[] stubs = file_info.GetStubs();
                Guid[] base_iids = file_info.GetBaseIids();
                Dictionary<IntPtr, NdrBaseTypeReference> type_cache
                    = new Dictionary<IntPtr, NdrBaseTypeReference>();
                for (int i = 0; i < names.Length; ++i)
                {
                    entries.Add(new COMProxyInstanceEntry(this, names[i], stubs[i].GetIid(),
                        base_iids[i], stubs[i].DispatchTableCount, ReadProcs(type_cache, base_iids[i], stubs[i])));
                }
                complex_types.AddRange(type_cache.Values.OfType<NdrBaseStructureTypeReference>());
                complex_types.AddRange(type_cache.Values.OfType<NdrUnionTypeReference>());
            }
            Entries = entries.AsReadOnly();
            ComplexTypes = complex_types.AsReadOnly();
            return true;
        }

        private bool InitFromFile(string path, Guid clsid)
        {
            using (SafeLibraryHandle lib = COMUtilities.SafeLoadLibrary(path))
            {
                IntPtr pInfo = FindProxyDllInfo(lib, clsid);
                if (pInfo == IntPtr.Zero)
                {
                    return false;
                }

                return InitFromFileInfo(pInfo);
            }
        }

        public COMProxyInstance(string path)
        {
            if (!InitFromFile(path, Guid.Empty))
            {
                throw new ArgumentException("Can't find proxy information in server DLL");
            }
        }

        private COMProxyInstance(COMCLSIDEntry clsid)
        {
            if (!InitFromFile(clsid.DefaultServer, clsid.Clsid))
            {
                throw new ArgumentException("Can't find proxy information in server DLL");
            }
        }

        private static Dictionary<Guid, COMProxyInstance> m_proxies = new Dictionary<Guid, COMProxyInstance>();
        private static Dictionary<string, COMProxyInstance> m_proxies_by_file = new Dictionary<string, COMProxyInstance>(StringComparer.OrdinalIgnoreCase);

        public static COMProxyInstance GetFromCLSID(COMCLSIDEntry clsid)
        {
            if (m_proxies.ContainsKey(clsid.Clsid))
            {
                return m_proxies[clsid.Clsid];
            }
            else
            {
                COMProxyInstance proxy = new COMProxyInstance(clsid);
                m_proxies[clsid.Clsid] = proxy;
                return proxy;
            }
        }

        public static COMProxyInstance GetFromFile(string path)
        {
            if (m_proxies_by_file.ContainsKey(path))
            {
                return m_proxies_by_file[path];
            }
            else
            {
                COMProxyInstance proxy = new COMProxyInstance(path);
                m_proxies_by_file[path] = proxy;
                return proxy;
            }
        }
    }
}
