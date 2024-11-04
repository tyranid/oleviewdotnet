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
using System.Runtime.InteropServices;

namespace OleViewDotNetPS.Wrappers;

public abstract class BaseComWrapper : INdrComObject, ICOMObjectWrapper
{
    protected readonly COMRegistry m_registry;

    public string InterfaceName { get; }
    public Guid Iid { get; }
    public abstract object Unwrap();

    public ICOMObjectWrapper QueryInterface(Guid iid)
    {
        return COMTypeManager.Wrap(Unwrap(), iid, m_registry);
    }

    INdrComObject INdrComObject.QueryInterface(Guid iid)
    {
        return QueryInterface(iid);
    }

    void IDisposable.Dispose()
    {
        OnDispose();
    }

    protected abstract void OnDispose();

    protected BaseComWrapper(Guid iid, string name, COMRegistry registry)
    {
        InterfaceName = name;
        Iid = iid;
        m_registry = registry;
    }
}

public abstract class BaseComWrapper<T> : BaseComWrapper where T : class
{
    protected readonly T _object;

    protected BaseComWrapper(object obj, COMRegistry registry)
        : this(obj, typeof(T).GUID, typeof(T).Name, registry)
    {
    }

    private protected BaseComWrapper(object obj, Guid iid, string name, COMRegistry registry) 
        : base(iid, name, registry)
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
