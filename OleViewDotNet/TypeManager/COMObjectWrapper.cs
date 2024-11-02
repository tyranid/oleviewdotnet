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
using OleViewDotNet.Interop;
using System;

namespace OleViewDotNet.TypeManager;

public class COMObjectWrapper : ICOMObjectWrapper, INdrComObject
{
    private readonly object _obj;
    private readonly COMRegistry _database;

    public COMObjectWrapper(object obj, Guid iid, COMRegistry database)
    {
        _obj = obj;
        _database = database;
        Iid = iid;
    }

    public Guid Iid { get; }

    public virtual object Unwrap()
    {
        return _obj;
    }

    void IDisposable.Dispose()
    {
    }

    INdrComObject INdrComObject.QueryInterface(Guid iid)
    {
        using var unk = SafeComObjectHandle.FromObject(_obj, iid);
        return new COMObjectWrapper(_obj, iid, _database);
    }
}