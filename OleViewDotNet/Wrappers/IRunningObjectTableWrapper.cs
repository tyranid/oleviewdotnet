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

using OleViewDotNet.Interop;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Wrappers;

public sealed class IRunningObjectTableWrapper : BaseComWrapper<IRunningObjectTable>
{
    public IRunningObjectTableWrapper(object obj)
        : base(obj)
    {
    }

    public int Register(int grfFlags, BaseComWrapper punkObject, IMonikerWrapper pmkObjectName)
    {
        return _object.Register(grfFlags, punkObject.Unwrap(), pmkObjectName.UnwrapTyped());
    }

    public void Revoke(int dwRegister)
    {
        _object.Revoke(dwRegister);
    }

    public int IsRunning(IMonikerWrapper pmkObjectName)
    {
        return _object.IsRunning(pmkObjectName.UnwrapTyped());
    }

    public int GetObject(IMonikerWrapper pmkObjectName, out BaseComWrapper ppunkObject)
    {
        int hr = _object.GetObject(pmkObjectName.UnwrapTyped(), out object obj);
        ppunkObject = COMWrapperFactory.Wrap(obj, COMKnownGuids.IID_IUnknown, _database);
        return hr;
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
