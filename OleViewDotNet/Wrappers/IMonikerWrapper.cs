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
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Wrappers;

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

    public BaseComWrapper BindToObject(IBindCtxWrapper pbc, IMonikerWrapper pmkToLeft, Guid riidResult)
    {
        _object.BindToObject(pbc.UnwrapTyped(), pmkToLeft.UnwrapTyped(), ref riidResult, out object ppvResult);
        return COMWrapperFactory.Wrap(ppvResult, riidResult, _database);
    }

    public BaseComWrapper BindToStorage(IBindCtxWrapper pbc, IMonikerWrapper pmkToLeft, Guid riid)
    {
        _object.BindToStorage(pbc.UnwrapTyped(), pmkToLeft.UnwrapTyped(), ref riid, out object ppvObj);
        return COMWrapperFactory.Wrap(ppvObj, riid, _database);
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
