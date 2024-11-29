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

using OleViewDotNet.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Utilities;

public static class COMRunningObjectTable
{
    public static IReadOnlyList<COMRunningObjectTableEntry> EnumRunning(bool trusted_only)
    {
        NativeMethods.GetRunningObjectTable(trusted_only ? 1 : 0, out IRunningObjectTable rot).CheckHr();
        List<COMRunningObjectTableEntry> entries = new();
        IMoniker[] moniker = new IMoniker[1];
        rot.EnumRunning(out IEnumMoniker enumMoniker);
        while (enumMoniker.Next(1, moniker, IntPtr.Zero) == 0)
        {
            entries.Add(new(rot, moniker[0]));
        }
        return entries.AsReadOnly();
    }
}