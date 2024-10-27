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
using System.Runtime.InteropServices;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;

namespace OleViewDotNetPS.Wrappers;

public sealed class IInspectableWrapper : BaseComWrapper<IInspectable>
{
    public IInspectableWrapper(object obj, COMRegistry registry) : base(obj, registry)
    {
    }

    public Guid[] GetIids()
    {
        _object.GetIids(out int count, out IntPtr iids);
        try
        {
            Guid[] ret = new Guid[count];
            for (int i = 0; i < count; ++i)
            {
                IntPtr ptr = iids + 16 * i;
                ret[i] = Marshal.PtrToStructure<Guid>(ptr);
            }
            return ret;
        }
        finally
        {
            Marshal.FreeCoTaskMem(iids);
        }
    }

    public string GetRuntimeClassName()
    {
        _object.GetRuntimeClassName(out string class_name);
        return class_name;
    }

    public TrustLevel GetTrustLevel()
    {
        _object.GetTrustLevel(out TrustLevel trust_level);
        return trust_level;
    }
}