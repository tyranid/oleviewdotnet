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

using OleViewDotNet.Database;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Wrappers;

public abstract class BaseComWrapper
{
    internal IEnumerable<COMInterfaceEntry> _interfaces;

    public string InterfaceName { get; }
    public Guid Iid { get; }
    public abstract BaseComWrapper QueryInterface(Guid iid);
    public abstract object Unwrap();

    protected BaseComWrapper(Guid iid, string name)
    {
        InterfaceName = name;
        Iid = iid;
    }
}

public abstract class BaseComWrapper<T> : BaseComWrapper, IDisposable where T : class
{
    protected readonly T _object;

    protected BaseComWrapper(object obj)
        : base(typeof(T).GUID, typeof(T).Name)
    {
        System.Diagnostics.Debug.Assert(typeof(T).IsInterface);
        _object = (T)obj;
    }

    public override BaseComWrapper QueryInterface(Guid iid)
    {
        return COMWrapperFactory.Wrap(_object, COMUtilities.GetInterfaceType(iid));
    }

    public override object Unwrap()
    {
        return _object;
    }

    void IDisposable.Dispose()
    {
        Marshal.ReleaseComObject(_object);
    }
}