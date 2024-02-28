//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017
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

using NtApiDotNet;
using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Processes.Types;

[StructLayout(LayoutKind.Sequential)]
internal struct SHashChain : ISHashChain
{
    public IntPtr pNext;
    public IntPtr pPrev;

    IntPtr ISHashChain.GetNext()
    {
        return pNext;
    }

    ISHashChain ISHashChain.GetNextChain(NtProcess process)
    {
        if (pNext == IntPtr.Zero)
            return null;
        return process.ReadStruct<SHashChain>(pNext.ToInt64());
    }

    I ISHashChain.GetNextObject<T, I>(NtProcess process, int offset)
    {
        if (pNext == IntPtr.Zero)
            return null;
        return process.ReadStruct<T>(pNext.ToInt64() - offset);
    }

    IntPtr ISHashChain.GetPrev()
    {
        return pPrev;
    }
}
