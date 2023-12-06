//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
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

namespace OleViewDotNet.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct MULTI_QI : IDisposable
{
    private OptionalGuid pIID;
    private IntPtr pItf;
    private readonly int hr;

    public object GetObject()
    {
        if (pItf == IntPtr.Zero)
        {
            return null;
        }
        else
        {
            return Marshal.GetObjectForIUnknown(pItf);
        }
    }

    public IntPtr GetObjectPointer()
    {
        if (pItf != IntPtr.Zero)
        {
            Marshal.AddRef(pItf);
        }
        return pItf;
    }

    public int HResult()
    {
        return hr;
    }

    void IDisposable.Dispose()
    {
        ((IDisposable)pIID).Dispose();
        if (pItf != IntPtr.Zero)
        {
            Marshal.Release(pItf);
            pItf = IntPtr.Zero;
        }
    }

    public MULTI_QI(Guid iid)
    {
        pIID = new OptionalGuid(iid);
        pItf = IntPtr.Zero;
        hr = 0;
    }
}
