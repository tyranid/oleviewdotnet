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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Wrappers;

public abstract class BaseComWrapperReflection : BaseComWrapper, IDisposable
{
    protected readonly object _object;
    protected readonly InvokeHelper[] _methods;

    protected class InvokeHelper
    {
        private readonly MethodInfo _method;
        private readonly object[] _args;
        private readonly object _object;

        public InvokeHelper(object obj, MethodInfo method)
        {
            _method = method;
            _args = new object[_method.GetParameters().Length];
            _object = obj;
        }

        public void SetArg<T>(int index, T obj)
        {
            _args[index] = obj;
        }

        public T GetArg<T>(int index)
        {
            return (T)_args[index];
        }

        public void Clear()
        {
            for (int i = 0; i < _args.Length; ++i)
            {
                _args[i] = null;
            }
        }

        public T Invoke<T>()
        {
            return (T)_method.Invoke(_object, _args);
        }
    }

    private BaseComWrapperReflection(object obj, Type type)
        : base(type.GUID, type.Name)
    {
        System.Diagnostics.Debug.Assert(type.IsInterface);
        _object = obj;
        _methods = type.GetMethods().Select(m => new InvokeHelper(obj, m)).ToArray();
    }

    protected BaseComWrapperReflection(object obj, string type_name)
        : this(obj, Type.GetType(type_name))
    {
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