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
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OleViewDotNetPS.Wrappers;

public abstract class BaseComReflectionWrapper : BaseComWrapper
{
    private readonly object m_object;
    private readonly MethodInfo[] m_methods;

    protected MethodInfoWrapper Get(int index)
    {
        return new(m_object, m_methods[index]);
    }

    private BaseComReflectionWrapper(object obj, Type type, COMRegistry registry)
        : base(type.GUID, type.Name, registry)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        System.Diagnostics.Debug.Assert(type.IsInterface);
        m_object = obj ?? throw new ArgumentNullException(nameof(obj));
        m_methods = type.GetMethods();
    }

    protected BaseComReflectionWrapper(object obj, string type, COMRegistry registry) 
        : this(obj, Type.GetType(type), registry)
    {
    }

    public override object Unwrap()
    {
        return m_object;
    }

    protected override void OnDispose()
    {
        Marshal.ReleaseComObject(m_object);
    }
}
