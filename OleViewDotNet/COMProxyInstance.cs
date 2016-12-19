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
        public string Name { get; private set; }
        public Guid Iid { get; private set; }
        public Guid BaseIid { get; private set; }
        public int DispatchCount { get; private set; }
        public NdrProcedureDefinition[] Procs { get; private set; }

        internal COMProxyInstanceEntry(string name, Guid iid, Guid base_iid, int dispatch_count, NdrProcedureDefinition[] procs)
        {
            Name = name;
            Iid = iid;
            BaseIid = base_iid == Guid.Empty ? COMInterfaceEntry.IID_IUnknown : base_iid;
            DispatchCount = dispatch_count;
            Procs = procs;
        }

        public string Format(IDictionary<Guid, string> iids_to_names)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("[Guid(\"{0}\")]", Iid).AppendLine();
            string base_name = iids_to_names.ContainsKey(BaseIid) ? 
                iids_to_names[BaseIid] : String.Format("/* Unknown IID {0} */ IUnknown", BaseIid);
            builder.AppendFormat("interface {0} : {1} {{", Name, base_name).AppendLine();
            foreach (NdrProcedureDefinition proc in Procs)
            {
                builder.AppendFormat("    {0}", proc.FormatProcedure(iids_to_names)).AppendLine();
            }
            builder.AppendLine("}").AppendLine();
            return builder.ToString();
        }
    }

    public class COMProxyInstance
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void GetProxyDllInfo(out IntPtr pInfo, out IntPtr pId);

        public IEnumerable<COMProxyInstanceEntry> Entries { get; private set; }

        private NdrProcedureDefinition[] ReadProcs(Guid base_iid, CInterfaceStubHeader stub)
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
                    procs.Add(new NdrProcedureDefinition(stub_desc, server_info.ProcString + fmt_ofs, type_desc));
                }
                start_ofs++;
            }
            return procs.ToArray();
        }

        private static IntPtr FindProxyDllInfo(SafeLibraryHandle lib)
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
                return IntPtr.Zero;
            }
        }

        public COMProxyInstance(COMCLSIDEntry clsid)
        {
            using (SafeLibraryHandle lib = COMUtilities.SafeLoadLibrary(clsid.Server))
            {
                List<COMProxyInstanceEntry> entries = new List<COMProxyInstanceEntry>();
                IntPtr pInfo = FindProxyDllInfo(lib);
                if (pInfo == IntPtr.Zero)
                {
                    throw new ArgumentException("Can't find proxy information in server DLL");
                }

                foreach (var file_info in COMUtilities.EnumeratePointerList<ProxyFileInfo>(pInfo))
                {
                    string[] names = file_info.GetNames();
                    CInterfaceStubHeader[] stubs = file_info.GetStubs();
                    Guid[] base_iids = file_info.GetBaseIids();

                    for (int i = 0; i < names.Length; ++i)
                    {
                        entries.Add(new COMProxyInstanceEntry(names[i], stubs[i].GetIid(), 
                            base_iids[i], stubs[i].DispatchTableCount, ReadProcs(base_iids[i], stubs[i])));
                    }
                }
                Entries = entries;
            }
        }
    }
}
