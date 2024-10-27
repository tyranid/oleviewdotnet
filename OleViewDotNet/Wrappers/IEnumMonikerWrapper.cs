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
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Wrappers;

public class IEnumMonikerWrapper : BaseComWrapper<IEnumMoniker>
{
    public IEnumMonikerWrapper(object obj, COMRegistry registry) : base(obj, registry)
    {
    }

    public int Next(int celt, IMonikerWrapper[] rgelt, IntPtr pceltFetched)
    {
        var monikers = rgelt.Select(m => m.UnwrapTyped()).ToArray();
        return _object.Next(celt, monikers, pceltFetched);
    }

    public int Skip(int celt)
    {
        return _object.Skip(celt);
    }

    public void Reset()
    {
        _object.Reset();
    }

    public IEnumMonikerWrapper Clone()
    {
        _object.Clone(out IEnumMoniker out_enum);
        return new IEnumMonikerWrapper(out_enum, m_registry);
    }
}
