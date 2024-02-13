//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
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
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

internal sealed class COMVarDesc : IDisposable
{
    private readonly ITypeInfo _type_info;
    private readonly IntPtr _ptr;

    public VARDESC Descriptor { get; }

    public COMVarDesc(ITypeInfo type_info, IntPtr ptr)
    {
        _type_info = type_info;
        _ptr = ptr;
        Descriptor = ptr.GetStructure<VARDESC>();
    }

    void IDisposable.Dispose()
    {
        _type_info.ReleaseVarDesc(_ptr);
    }
}
