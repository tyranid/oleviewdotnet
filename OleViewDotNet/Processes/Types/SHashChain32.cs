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
internal struct SHashChain32 : ISHashChain
{
    public int pNext;
    public int pPrev;

    IntPtr ISHashChain.GetNext()
    {
        return new IntPtr(pNext);
    }

    IntPtr ISHashChain.GetPrev()
    {
        return new IntPtr(pPrev);
    }

    ISHashChain ISHashChain.GetNextChain(NtProcess process)
    {
        if (pNext == 0)
            return null;
        return process.ReadStruct<SHashChain32>(pNext);
    }

    I ISHashChain.GetNextObject<T, I>(NtProcess process, int offset)
    {
        if (pNext == 0)
            return null;
        return process.ReadStruct<T>(pNext - offset);
    }
}
