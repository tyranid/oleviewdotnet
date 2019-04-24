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

using OleViewDotNet.Database;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet
{
    public enum ObjectSafetyFlags
    {
        INTERFACESAFE_FOR_UNTRUSTED_CALLER = 0x00000001,
        INTERFACESAFE_FOR_UNTRUSTED_DATA = 0x00000002,
        INTERFACE_USES_DISPEX = 0x00000004,
        INTERFACE_USES_SECURITY_MANAGER = 0x00000008
    }

    [ComImport, Guid("00000000-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IUnknown
    {
    }

    [ComImport, Guid("CB5BDC81-93C1-11CF-8F20-00805F2CD064"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IObjectSafety
    {
        void GetInterfaceSafetyOptions(ref Guid riid, out uint pdwSupportedOptions, out uint pdwEnabledOptions);
        void SetInterfaceSafetyOptions(ref Guid riid, uint dwOptionSetMask, uint dwEnabledOptions);
    }

    [ComImport, Guid("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInspectable
    {
        void GetIids(out int iidCount, out IntPtr iids);
        void GetRuntimeClassName([MarshalAs(UnmanagedType.HString)] out string className);
        void GetTrustLevel(out TrustLevel trustLevel);
    };

    [ComImport, Guid("0000010C-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersist
    {
        void GetClassID(out Guid clsid);
    }

    [ComImport, Guid("00000109-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistStream
    {
        void GetClassID(out Guid clsid);
        [PreserveSig]
        int IsDirty();
        void Load(IStream pStm);
        void Save(IStream pStm, bool fClearDirty);
        void GetSizeMax(out ulong pcbSize);
    }

    [ComImport, Guid("7FD52380-4E07-101B-AE2D-08002B2EC713"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistStreamInit
    {
        void GetClassID(out Guid clsid);
        [PreserveSig]
        int IsDirty();
        void Load(IStream pStm);
        void Save(IStream pStm, bool fClearDirty);
        void GetSizeMax(out ulong pcbSize);
        [PreserveSig]
        int InitNew();
    }

    [StructLayout(LayoutKind.Sequential)]
    public class tagEXCEPINFO
    {
        [MarshalAs(UnmanagedType.U2)]
        public short wCode;
        [MarshalAs(UnmanagedType.U2)]
        public short wReserved;
        [MarshalAs(UnmanagedType.BStr)]
        public string bstrSource;
        [MarshalAs(UnmanagedType.BStr)]
        public string bstrDescription;
        [MarshalAs(UnmanagedType.BStr)]
        public string bstrHelpFile;
        [MarshalAs(UnmanagedType.U4)]
        public int dwHelpContext;
        public IntPtr pvReserved;
        public IntPtr pfnDeferredFillIn;
        [MarshalAs(UnmanagedType.U4)]
        public int scode;
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("3127CA40-446E-11CE-8135-00AA004BB851")]
    public interface IErrorLog
    {
        void AddError([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName_p0, [In, MarshalAs(UnmanagedType.Struct)] tagEXCEPINFO pExcepInfo_p1);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("55272A00-42CB-11CE-8135-00AA004BB851")]
    public interface IPropertyBag
    {
        [PreserveSig]
        int Read([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName, [In, Out] ref object pVar, [In] IErrorLog pErrorLog);
        [PreserveSig]
        int Write([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName, [In] ref object pVar);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("37D84F60-42CB-11CE-8135-00AA004BB851")]
    public interface IPersistPropertyBag
    {
        void GetClassID(out Guid clsid);
        void InitNew();
        void Load([In, MarshalAs(UnmanagedType.Interface)] IPropertyBag pPropBag, [In, MarshalAs(UnmanagedType.Interface)] IErrorLog pErrorLog);
        void Save([In, MarshalAs(UnmanagedType.Interface)] IPropertyBag pPropBag, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty, [In, MarshalAs(UnmanagedType.Bool)] bool fSaveAllProperties);
    }

    [ComImport]
    [Guid("0000000d-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumSTATSTG
    {
        // The user needs to allocate an STATSTG array whose size is celt.
        [PreserveSig]
        int Next(uint celt, [MarshalAs(UnmanagedType.LPArray), Out]System.Runtime.InteropServices.ComTypes.STATSTG[] rgelt, out uint pceltFetched);

        void Skip(uint celt);

        void Reset();

        [return: MarshalAs(UnmanagedType.Interface)]
        IEnumSTATSTG Clone();
    }

    [StructLayout(LayoutKind.Explicit)]
    public class FILETIMEOptional
    {
        [FieldOffset(0)]
        public System.Runtime.InteropServices.ComTypes.FILETIME FileTime;
        [FieldOffset(0)]
        public long QuadPart;

        public FILETIMEOptional(DateTime datetime)
        {
            QuadPart = datetime.ToFileTime();
        }

        public FILETIMEOptional()
        {
        }
    }

    [ComImport, Guid("0000000B-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStorage
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        IStream CreateStream([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.U4)] STGM grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved1, [In, MarshalAs(UnmanagedType.U4)] int reserved2);
        [return: MarshalAs(UnmanagedType.Interface)]
        IStream OpenStream([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, IntPtr reserved1, [In, MarshalAs(UnmanagedType.U4)] STGM grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved2);
        [return: MarshalAs(UnmanagedType.Interface)]
        IStorage CreateStorage([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.U4)] STGM grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved1, [In, MarshalAs(UnmanagedType.U4)] int reserved2);
        [return: MarshalAs(UnmanagedType.Interface)]
        IStorage OpenStorage([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, IntPtr pstgPriority, [In, MarshalAs(UnmanagedType.U4)] STGM grfMode, IntPtr snbExclude, [In, MarshalAs(UnmanagedType.U4)] int reserved);
        void CopyTo(int ciidExclude, [In, MarshalAs(UnmanagedType.LPArray)] Guid[] pIIDExclude, IntPtr snbExclude, [In, MarshalAs(UnmanagedType.Interface)] IStorage stgDest);
        void MoveElementTo([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.Interface)] IStorage stgDest, [In, MarshalAs(UnmanagedType.BStr)] string pwcsNewName, [In, MarshalAs(UnmanagedType.U4)] int grfFlags);
        void Commit(int grfCommitFlags);
        void Revert();
        void EnumElements([In, MarshalAs(UnmanagedType.U4)] int reserved1, IntPtr reserved2, [In, MarshalAs(UnmanagedType.U4)] int reserved3, [MarshalAs(UnmanagedType.Interface)] out IEnumSTATSTG ppVal);
        void DestroyElement([In, MarshalAs(UnmanagedType.BStr)] string pwcsName);
        void RenameElement([In, MarshalAs(UnmanagedType.BStr)] string pwcsOldName, [In, MarshalAs(UnmanagedType.BStr)] string pwcsNewName);
        void SetElementTimes([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In] FILETIMEOptional pctime, [In] FILETIMEOptional patime, [In] FILETIMEOptional pmtime);
        void SetClass([In] ref Guid clsid);
        void SetStateBits(int grfStateBits, int grfMask);
        void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pStatStg, int grfStatFlag);
    }

    [ComImport, Guid("0000010A-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistStorage
    {
        void GetClassID(out Guid pClassID);
        [PreserveSig]
        int IsDirty();
        void InitNew(IStorage pstg);
        void Load(IStorage pstg);
        void Save(IStorage pStgSave, bool fSameAsLoad);
        void SaveCompleted(IStorage pStgNew);
        void HandsOffStorage();
    }

    [ComImport, Guid("79EAC9C9-BAF9-11CE-8C82-00AA004BA90B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistMoniker
    {
        void GetClassID(out Guid pClassID);
        void GetCurMoniker(out IMoniker pMoniker);
        [PreserveSig]
        int IsDirty();
        void Load(bool fFullyAvailable, IMoniker pimkName, IBindCtx pibc, STGM grfMode);
        void Save(IMoniker pimkName, IBindCtx pibc, bool fRemember);
        void SaveCompleted(IMoniker pimkName, IBindCtx pibc);
    }

    [ComImport, Guid("00000001-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IClassFactory
    {
        void CreateInstance([MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter,
            ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);
        void LockServer(bool fLock);
    }

    [ComImport, Guid("804bd226-af47-4d71-b492-443a57610b08"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IElevatedFactoryServer
    {
        void ServerCreateElevatedObject(ref Guid clsid, 
            ref Guid iid, 
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct CATEGORYINFO
    {
        public Guid catid;
        public int lcid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szDescription;
    }

    [Guid("0002E011-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumCATEGORYINFO
    {
        int Next(
            int celt,
            [Out] CATEGORYINFO[] rgelt,
            out int pceltFetched);
        int Skip(int celt);
        int Reset();
        int Clone(out IEnumCATEGORYINFO ppenum);
    }

    [Guid("0002E000-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IEnumGUID
    {
        int Next(
            int celt,
            [Out] Guid[] rgelt,
            out int pceltFetched);
        int Skip(int celt);
        int Reset();
        int Clone(out IEnumCATEGORYINFO ppenum);
    }

    [Guid("0002E013-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ICatInformation
    {
        int EnumCategories(int lcid, out IEnumCATEGORYINFO ppenumCategoryInfo);
        int GetCategoryDesc(ref Guid rcatid, int lcid, out IntPtr pszDesc);
        void EnumClassesOfCategories(int cImplemented,
            Guid[] rgcatidImpl,
            int cRequired,
            Guid[] rgcatidReq,
            out IEnumGUID ppenumClsid);

        void IsClassOfCategories(ref Guid rclsid,
            int cImplemented,
            Guid[] rgcatidImpl,
            int cRequired,
            Guid[] rgcatidReq);

        void EnumImplCategoriesOfClass(
            ref Guid rclsid,
            out IEnumGUID ppenumCatid);

        void EnumReqCategoriesOfClass(
            ref Guid rclsid,
            out IEnumGUID ppenumCatid);
    }

    [Guid("00020400-0000-0000-c000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDispatch
    {
        void GetTypeInfoCount(out uint pctinfo);
        void GetTypeInfo(uint iTypeInfo, uint lcid, out IntPtr pTypeInfo);
        void GetIDsOfNames(ref Guid riid, string[] rszNames, uint cNames, uint lcid, ref int[] dispIDs);
        void Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, System.Runtime.InteropServices.ComTypes.DISPPARAMS[] pDispParams,
                    out VariantWrapper pVarResult, ref System.Runtime.InteropServices.ComTypes.EXCEPINFO pExcepInfo, out uint puArgErr);
    }

    [Flags]
    public enum FILTER_ACTIVATIONTYPE
    {
        UNCATEGORIZED = 0x0,
        FROM_MONIKER = 0x1,
        FROM_DATA = 0x2,
        FROM_STORAGE = 0x4,
        FROM_STREAM = 0x8,
        FROM_FILE = 0x10
    }
    
    [Guid("00000017-0000-0000-C000-000000000046"), 
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IActivationFilter
    {
        void HandleActivation(
            FILTER_ACTIVATIONTYPE dwActivationType,
            ref Guid rclsid,
            out Guid pReplacementClsId);
    };

    [Guid("6040ec14-6557-41f9-a3f7-b1cab7b42120")]
    [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    interface IRuntimeBroker
    {
        [return: MarshalAs(UnmanagedType.IInspectable)] object ActivateInstance([MarshalAs(UnmanagedType.LPWStr)] string instanceName);
        [return: MarshalAs(UnmanagedType.IUnknown)] object GetActivationFactory([MarshalAs(UnmanagedType.LPWStr)] string instanceName, ref Guid uuid);
        void SetErrorFlags(uint error_flags);
        uint GetErrorFlags();
        void DebuggerAddRef();
        void DebuggerRelease();
        [return: MarshalAs(UnmanagedType.IUnknown)] object GetClipboardBroker();
    }

    public enum RPCOPT_PROPERTIES
    {
        RPCTIMEOUT = 0x1,
        SERVER_LOCALITY = 0x2,
        RESERVED1 = 0x4,
        RESERVED2 = 0x5,
        RESERVED3 = 0x8,
        RESERVED4 = 0x10
    }

    public enum RPCOPT_SERVER_LOCALITY_VALUES
    {
        PROCESS_LOCAL = 0,
        MACHINE_LOCAL = 1,
        REMOTE = 2
    }

    [Guid("00000144-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IRpcOptions
    {
        void Set([MarshalAs(UnmanagedType.IUnknown)] object pPrx,
                    RPCOPT_PROPERTIES dwProperty,
                    IntPtr dwValue);

        void Query(
            [MarshalAs(UnmanagedType.IUnknown)] object pPrx,
            RPCOPT_PROPERTIES dwProperty,
            out IntPtr pdwValue);
    }

    public enum RegistrationScope
    {
        PerMachine = 0x0,
        PerUser = 0x1,
        InboxApp = 0x2,
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIInspectable), Guid("905754d7-1cc5-404e-a9d4-c517bc35e88d")]
    public interface IExtensionRegistration
    {
        string ActivatableClassId
        {
            [return: MarshalAs(UnmanagedType.HString)]
            get;
        }

        string ContractId
        {
            [return: MarshalAs(UnmanagedType.HString)]
            get;
        }

        string PackageId
        {
            [return: MarshalAs(UnmanagedType.HString)]
            get;
        }

        string ExtensionId
        {
            [return: MarshalAs(UnmanagedType.HString)]
            get;
        }


        object ActivatableClassRegistration // Windows::Foundation::IActivatableClassRegistration
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            get;
        }

        string Vendor
        {
            [return: MarshalAs(UnmanagedType.HString)]
            get;
        }

        string Icon
        {
            [return: MarshalAs(UnmanagedType.HString)]
            get;
        }

        string DisplayName
        {
            [return: MarshalAs(UnmanagedType.HString)]
            get;
        }

        string Description
        {
            [return: MarshalAs(UnmanagedType.HString)]
            get;
        }

        RegistrationScope RegistrationScope
        {
            get;
        }

        IReadOnlyDictionary<string, object> Attributes
        {
            get;
        }

        int OutOfProcActivationFlags
        {
            get; set;
        }

        [return: MarshalAs(UnmanagedType.IInspectable)]
        object Activate();
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("533148e2-ee0a-4b06-8500-7fda28f92ae2")]
    interface IExtensionActivationContext
    {
        ulong HostId { get; set; }
        ulong UserContext { get; set; }
        ulong ComponentProcessId { get; set; }
        ulong RacActivationTokenId { get; set; }
        IntPtr LpacAttributes { get; set; }
        ulong ConsoleHandlesId { get; set; }
        uint AAMActivationId { get; set; }
    }
}
