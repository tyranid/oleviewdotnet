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
using System;

namespace OleViewDotNet.TypeManager;

public sealed class COMObjectWrapper : ICOMObjectWrapper, INdrComObject
{
    private readonly object m_obj;
    private readonly COMRegistry m_registry;

    public COMObjectWrapper(object obj, Guid iid, Type type, COMRegistry registry)
    {
        m_obj = obj;
        m_registry = registry;
        Iid = iid;
        Type = type;
    }

    public Guid Iid { get; }

    public Type Type { get; }

    public string Name => Type.Name;

    public object Unwrap()
    {
        return m_obj;
    }

    void IDisposable.Dispose()
    {
    }

    INdrComObject INdrComObject.QueryInterface(Guid iid)
    {
        return COMTypeManager.Wrap(m_obj, iid, m_registry);
    }
}
