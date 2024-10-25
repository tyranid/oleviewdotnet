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

using NtApiDotNet.Ndr.Marshal;
using OleViewDotNet.Database;
using OleViewDotNet.TypeManager;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Wrappers;

public abstract class BaseComWrapper : INdrComObject
{
    protected COMRegistry _database;
    internal IEnumerable<COMInterfaceEntry> _interfaces;

    public string InterfaceName { get; }
    public Guid Iid { get; }
    public abstract object Unwrap();

    public BaseComWrapper QueryInterface(Guid iid)
    {
        return COMWrapperFactory.Wrap(Unwrap(), iid, _database);
    }

    internal virtual void SetDatabase(COMRegistry database)
    {
        _database = database;
    }

    INdrComObject INdrComObject.QueryInterface(Guid iid)
    {
        return QueryInterface(iid);
    }

    Guid INdrComObject.GetIid()
    {
        return Iid;
    }

    void IDisposable.Dispose()
    {
        OnDispose();
    }

    protected abstract void OnDispose();

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

    public override object Unwrap()
    {
        return _object;
    }

    protected override void OnDispose()
    {
        Marshal.ReleaseComObject(_object);
    }
}
