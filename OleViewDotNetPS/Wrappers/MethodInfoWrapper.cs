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

using System.Reflection;

namespace OleViewDotNetPS.Wrappers;

public sealed class MethodInfoWrapper
{
    private readonly object m_obj;
    private readonly MethodInfo m_method;
    private readonly ParameterInfo[] m_params;
    private readonly object[] m_args;
    private object m_ret;

    internal MethodInfoWrapper(object obj, MethodInfo method)
    {
        m_obj = obj;
        m_method = method;
        m_params = m_method.GetParameters();
        m_args = new object[m_params.Length];
    }

    public void Set<T>(int index, T value)
    {
        m_args[index] = value;
    }

    public void SetRef<T>(int index, ref T value)
    {
        Set(index, value);
    }

    public void Get<T>(int index, out T value)
    {
        value = (T)m_args[index];
    }

    public void Invoke()
    {
        m_ret = m_method.Invoke(m_obj, m_args);
    }

    public T GetRet<T>()
    {
        return (T)m_ret;
    }
}
