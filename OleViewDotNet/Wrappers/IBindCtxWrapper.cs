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

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Wrappers;

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
