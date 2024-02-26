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

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
using OleViewDotNet.Interop;

namespace OleViewDotNet.Wrappers;

public sealed class IUnknownWrapper : BaseComWrapper<IUnknown>
{
    public IUnknownWrapper(object obj) : base(obj)
    {
    }
}

public sealed class IClassFactoryWrapper : BaseComWrapper<IClassFactory>
{
    public IClassFactoryWrapper(object obj) : base(obj)
    {
    }

    public void CreateInstance(object pUnkOuter, in Guid riid, out object ppvObject)
    {
        _object.CreateInstance(pUnkOuter, riid, out ppvObject);
    }

    public void LockServer(bool fLock)
    {
        _object.LockServer(fLock);
    }
}

public sealed class IActivationFactoryWrapper : BaseComWrapper<IActivationFactory>
{
    public IActivationFactoryWrapper(object obj) : base(obj)
    {
    }

    public object ActivateInstance()
    {
        return _object.ActivateInstance();
    }
}

public sealed class IBindCtxWrapper : BaseComWrapper<IBindCtx>
{
    public IBindCtxWrapper(object obj)
        : base(obj)
    {
    }

    public void RegisterObjectBound(object punk)
    {
        _object.RegisterObjectBound(punk);
    }

    public void RevokeObjectBound(object punk)
    {
        _object.RevokeObjectBound(punk);
    }

    public void ReleaseBoundObjects()
    {
        _object.ReleaseBoundObjects();
    }

    public void SetBindOptions([In] ref System.Runtime.InteropServices.ComTypes.BIND_OPTS pbindopts)
    {
        _object.SetBindOptions(ref pbindopts);
    }

    public System.Runtime.InteropServices.ComTypes.BIND_OPTS GetBindOptions()
    {
        System.Runtime.InteropServices.ComTypes.BIND_OPTS ret = new();
        _object.GetBindOptions(ref ret);
        return ret;
    }

    public IRunningObjectTableWrapper GetRunningObjectTable()
    {
        _object.GetRunningObjectTable(out IRunningObjectTable rot);
        return new IRunningObjectTableWrapper(rot);
    }

    public void RegisterObjectParam(string pszKey, object punk)
    {
        _object.RegisterObjectParam(pszKey, punk);
    }

    public void GetObjectParam(string pszKey, out object ppunk)
    {
        _object.GetObjectParam(pszKey, out ppunk);
    }

    public void EnumObjectParam(out IEnumString ppenum)
    {
        _object.EnumObjectParam(out ppenum);
    }

    public int RevokeObjectParam(string pszKey)
    {
        return _object.RevokeObjectParam(pszKey);
    }
}

public sealed class IMonikerWrapper : BaseComWrapper<IMoniker>
{
    public IMonikerWrapper(object obj) : base(obj)
    {
    }

    public Guid GetClassID()
    {
        _object.GetClassID(out Guid pClassID);
        return pClassID;
    }

    public int IsDirty()
    {
        return _object.IsDirty();
    }

    public void Load(IStreamWrapper pStm)
    {
        _object.Load(pStm.UnwrapTyped());
    }

    public void Save(IStreamWrapper pStm, bool fClearDirty)
    {
        _object.Save(pStm.UnwrapTyped(), fClearDirty);
    }

    public long GetSizeMax()
    {
        _object.GetSizeMax(out long pcbSize);
        return pcbSize;
    }

    public object BindToObject(IBindCtxWrapper pbc, IMonikerWrapper pmkToLeft, Guid riidResult)
    {
        _object.BindToObject(pbc.UnwrapTyped(), pmkToLeft.UnwrapTyped(), ref riidResult, out object ppvResult);
        return ppvResult;
    }

    public object BindToStorage(IBindCtxWrapper pbc, IMonikerWrapper pmkToLeft, Guid riid)
    {
        _object.BindToStorage(pbc.UnwrapTyped(), pmkToLeft.UnwrapTyped(), ref riid, out object ppvObj);
        return ppvObj;
    }

    public IMonikerWrapper Reduce(IBindCtxWrapper pbc, int dwReduceHowFar, ref IMoniker ppmkToLeft)
    {
        _object.Reduce(pbc.UnwrapTyped(), dwReduceHowFar, ppmkToLeft, out IMoniker mk);
        return new IMonikerWrapper(mk);
    }

    public IMonikerWrapper ComposeWith(IMonikerWrapper pmkRight, bool fOnlyIfNotGeneric)
    {
        _object.ComposeWith(pmkRight.UnwrapTyped(), fOnlyIfNotGeneric, out IMoniker out_mk);
        return new IMonikerWrapper(out_mk);
    }

    public void ComposeWith(IMonikerWrapper pmkRight, bool fOnlyIfNotGeneric, out IMonikerWrapper wrapper)
    {
        _object.ComposeWith(pmkRight.UnwrapTyped(), fOnlyIfNotGeneric, out IMoniker out_mk);
        wrapper = new IMonikerWrapper(out_mk);
    }

    public IEnumMonikerWrapper Enum(bool fForward)
    {
        _object.Enum(fForward, out IEnumMoniker ppenumMoniker);
        return new IEnumMonikerWrapper(ppenumMoniker);
    }

    public int IsEqual(IMonikerWrapper pmkOtherMoniker)
    {
        return _object.IsEqual(pmkOtherMoniker.UnwrapTyped());
    }

    public void Hash(out int pdwHash)
    {
        _object.Hash(out pdwHash);
    }

    public int IsRunning(IBindCtxWrapper pbc, IMonikerWrapper pmkToLeft, IMonikerWrapper pmkNewlyRunning)
    {
        return _object.IsRunning(pbc.UnwrapTyped(), pmkToLeft.UnwrapTyped(), pmkNewlyRunning.UnwrapTyped());
    }

    public System.Runtime.InteropServices.ComTypes.FILETIME GetTimeOfLastChange(IBindCtxWrapper pbc, IMonikerWrapper pmkToLeft)
    {
        _object.GetTimeOfLastChange(pbc.UnwrapTyped(), pmkToLeft.UnwrapTyped(), out System.Runtime.InteropServices.ComTypes.FILETIME pFileTime);
        return pFileTime;
    }

    public IMonikerWrapper Inverse()
    {
        _object.Inverse(out IMoniker ppmk);
        return new IMonikerWrapper(ppmk);
    }

    public IMonikerWrapper CommonPrefixWith(IMonikerWrapper pmkOther)
    {
        _object.CommonPrefixWith(pmkOther.UnwrapTyped(), out IMoniker out_mk);
        return new IMonikerWrapper(out_mk);
    }

    public IMonikerWrapper RelativePathTo(IMonikerWrapper pmkOther)
    {
        _object.RelativePathTo(pmkOther.UnwrapTyped(), out IMoniker out_mk);
        return new IMonikerWrapper(out_mk);
    }

    public string GetDisplayName(IBindCtxWrapper pbc, IMonikerWrapper pmkToLeft)
    {
        _object.GetDisplayName(pbc.UnwrapTyped(), pmkToLeft.UnwrapTyped(), out string ppszDisplayName);
        return ppszDisplayName;
    }

    public IMonikerWrapper ParseDisplayName(IBindCtxWrapper pbc, IMonikerWrapper pmkToLeft, string pszDisplayName, out int pchEaten)
    {
        _object.ParseDisplayName(pbc.UnwrapTyped(), pmkToLeft.UnwrapTyped(), pszDisplayName, out pchEaten, out IMoniker out_mk);
        return new IMonikerWrapper(out_mk);
    }

    public int IsSystemMoniker(out int pdwMksys)
    {
        return _object.IsSystemMoniker(out pdwMksys);
    }
}

public class IEnumMonikerWrapper : BaseComWrapper<IEnumMoniker>
{
    public IEnumMonikerWrapper(object obj) : base(obj)
    {
    }

    public int Next(int celt, IMoniker[] rgelt, IntPtr pceltFetched)
    {
        return _object.Next(celt, rgelt, pceltFetched);
    }

    public int Skip(int celt)
    {
        return _object.Skip(celt);
    }

    public void Reset()
    {
        _object.Reset();
    }

    public IEnumMonikerWrapper Clone()
    {
        _object.Clone(out IEnumMoniker out_enum);
        return new IEnumMonikerWrapper(out_enum);
    }
}

public class IStreamWrapper : BaseComWrapper<IStream>
{
    public IStreamWrapper(object obj) : base(obj)
    {
    }

    public void Read(byte[] pv, int cb, IntPtr pcbRead)
    {
        _object.Read(pv, cb, pcbRead);
    }

    public void Write(byte[] pv, int cb, IntPtr pcbWritten)
    {
        _object.Write(pv, cb, pcbWritten);
    }

    public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
    {
        _object.Seek(dlibMove, dwOrigin, plibNewPosition);
    }

    public void SetSize(long libNewSize)
    {
        _object.SetSize(libNewSize);
    }

    public void CopyTo(IStreamWrapper pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
    {
        _object.CopyTo(pstm.UnwrapTyped(), cb, pcbRead, pcbWritten);
    }

    public void Commit(int grfCommitFlags)
    {
        _object.Commit(grfCommitFlags);
    }

    public void Revert()
    {
        _object.Revert();
    }

    public void LockRegion(long libOffset, long cb, int dwLockType)
    {
        _object.LockRegion(libOffset, cb, dwLockType);
    }

    public void UnlockRegion(long libOffset, long cb, int dwLockType)
    {
        _object.UnlockRegion(libOffset, cb, dwLockType);
    }

    public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
    {
        Stat(out pstatstg, grfStatFlag);
    }

    public IStreamWrapper Clone()
    {
        _object.Clone(out IStream stm);
        return new IStreamWrapper(stm);
    }
}

public sealed class IEnumStringWrapper : BaseComWrapper<IEnumString>
{
    public IEnumStringWrapper([In] object obj)
        : base(obj)
    {
    }

    public int Next(int celt, [Out] string[] rgelt, IntPtr pceltFetched)
    {
        return _object.Next(celt, rgelt, pceltFetched);
    }

    public int Skip(int celt)
    {
        return _object.Skip(celt);
    }

    public void Reset()
    {
        _object.Reset();
    }

    public IEnumStringWrapper Clone()
    {
        _object.Clone(out IEnumString ppenum2);
        return new IEnumStringWrapper(ppenum2);
    }
}

public sealed class IRunningObjectTableWrapper : BaseComWrapper<IRunningObjectTable>
{
    public IRunningObjectTableWrapper([In] object obj)
        : base(obj)
    {
    }

    public int Register(int grfFlags, object punkObject, IMonikerWrapper pmkObjectName)
    {
        return _object.Register(grfFlags, punkObject, pmkObjectName.UnwrapTyped());
    }

    public void Revoke(int dwRegister)
    {
        _object.Revoke(dwRegister);
    }

    public int IsRunning(IMonikerWrapper pmkObjectName)
    {
        return _object.IsRunning(pmkObjectName.UnwrapTyped());
    }

    public int GetObject(IMonikerWrapper pmkObjectName, out object ppunkObject)
    {
        return _object.GetObject(pmkObjectName.UnwrapTyped(), out ppunkObject);
    }

    public void NoteChangeTime(int dwRegister, System.Runtime.InteropServices.ComTypes.FILETIME pfiletime)
    {
        _object.NoteChangeTime(dwRegister, ref pfiletime);
    }

    public int GetTimeOfLastChange(IMonikerWrapper pmkObjectName, out System.Runtime.InteropServices.ComTypes.FILETIME pfiletime)
    {
        return _object.GetTimeOfLastChange(pmkObjectName.UnwrapTyped(), out pfiletime);
    }

    public IEnumMonikerWrapper EnumRunning()
    {
        _object.EnumRunning(out IEnumMoniker ppenumMoniker2);
        return new IEnumMonikerWrapper(ppenumMoniker2);
    }
}

public sealed class TypeLibInstanceConverter
{

}