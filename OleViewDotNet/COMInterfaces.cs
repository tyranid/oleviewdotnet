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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet
{
    [ComImport, Guid("0000010C-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersist
    {
        void GetClassID(out Guid clsid);
    }

    [ComImport, Guid("00000109-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistStream
    {
        void GetClassID(out Guid clsid);
        int IsDirty();
        void Load(IStream pStm);
        void Save(IStream pStm, bool fClearDirty);
        void GetSizeMax(out ulong pcbSize);
    }

    [ComImport, Guid("7FD52380-4E07-101B-AE2D-08002B2EC713"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistStreamInit
    {
        void GetClassID(out Guid clsid);
        int IsDirty();
        void Load(IStream pStm);
        void Save(IStream pStm, bool fClearDirty);
        void GetSizeMax(out ulong pcbSize);
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

    [ComImport, Guid("0000000B-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStorage
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        IStream CreateStream([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved1, [In, MarshalAs(UnmanagedType.U4)] int reserved2);
        [return: MarshalAs(UnmanagedType.Interface)]
        IStream OpenStream([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, IntPtr reserved1, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved2);
        [return: MarshalAs(UnmanagedType.Interface)]
        IStorage CreateStorage([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved1, [In, MarshalAs(UnmanagedType.U4)] int reserved2);
        [return: MarshalAs(UnmanagedType.Interface)]
        IStorage OpenStorage([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, IntPtr pstgPriority, [In, MarshalAs(UnmanagedType.U4)] int grfMode, IntPtr snbExclude, [In, MarshalAs(UnmanagedType.U4)] int reserved);
        void CopyTo(int ciidExclude, [In, MarshalAs(UnmanagedType.LPArray)] Guid[] pIIDExclude, IntPtr snbExclude, [In, MarshalAs(UnmanagedType.Interface)] IStorage stgDest);
        void MoveElementTo([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.Interface)] IStorage stgDest, [In, MarshalAs(UnmanagedType.BStr)] string pwcsNewName, [In, MarshalAs(UnmanagedType.U4)] int grfFlags);
        void Commit(int grfCommitFlags);
        void Revert();
        void EnumElements([In, MarshalAs(UnmanagedType.U4)] int reserved1, IntPtr reserved2, [In, MarshalAs(UnmanagedType.U4)] int reserved3, [MarshalAs(UnmanagedType.Interface)] out object ppVal);
        void DestroyElement([In, MarshalAs(UnmanagedType.BStr)] string pwcsName);
        void RenameElement([In, MarshalAs(UnmanagedType.BStr)] string pwcsOldName, [In, MarshalAs(UnmanagedType.BStr)] string pwcsNewName);
        void SetElementTimes([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In] System.Runtime.InteropServices.ComTypes.FILETIME pctime, [In] System.Runtime.InteropServices.ComTypes.FILETIME patime, [In] System.Runtime.InteropServices.ComTypes.FILETIME pmtime);
        void SetClass([In] ref Guid clsid);
        void SetStateBits(int grfStateBits, int grfMask);
        void Stat([Out] System.Runtime.InteropServices.ComTypes.STATSTG pStatStg, int grfStatFlag);
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

    [Guid("965FC360-16FF-11d0-91CB-00AA00BBB723"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
    interface ISecurityInformation
    {
        // *** ISecurityInformation methods ***
        void GetObjectInformation(IntPtr pObjectInfo);
        void GetSecurity(SecurityInformation RequestedInformation,
                        out IntPtr ppSecurityDescriptor,
                        [MarshalAs(UnmanagedType.Bool)] bool fDefault);

        void SetSecurity(SecurityInformation SecurityInformation,
                        IntPtr pSecurityDescriptor);

        void GetAccessRights(ref Guid pguidObjectType,
                            SiObjectInfoFlags dwFlags, // SI_EDIT_AUDITS, SI_EDIT_PROPERTIES
                            out IntPtr ppAccess,
                            out uint pcAccesses,
                            out uint piDefaultAccess);

        void MapGeneric(ref Guid pguidObjectType,
                        IntPtr pAceFlags,
                        ref uint pMask);

        void GetInheritTypes(out IntPtr ppInheritTypes,
                            out uint pcInheritTypes);
        void PropertySheetPageCallback(IntPtr hwnd, uint uMsg, int uPage);
    }

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
