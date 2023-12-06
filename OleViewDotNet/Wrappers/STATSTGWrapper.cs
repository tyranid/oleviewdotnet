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

using OleViewDotNet.Interop;
using System;
using System.ComponentModel;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Wrappers;

public sealed class STATSTGWrapper
{
    private static DateTime FromFileTime(ComTypes.FILETIME filetime)
    {
        long result = (uint)filetime.dwLowDateTime | (long)filetime.dwHighDateTime << 32;
        return DateTime.FromFileTime(result);
    }

    internal STATSTGWrapper(string name, ComTypes.STATSTG stat, byte[] bytes)
    {
        Name = name;
        Type = (STGTY)stat.type;
        Size = stat.cbSize;
        ModifiedTime = FromFileTime(stat.mtime);
        CreationTime = FromFileTime(stat.ctime);
        AccessTime = FromFileTime(stat.atime);
        Mode = (STGM)stat.grfMode;
        LocksSupported = stat.grfLocksSupported;
        Clsid = stat.clsid;
        StateBits = stat.grfStateBits;
        Bytes = bytes;
    }

    public string Name { get; private set; }
    public STGTY Type { get; private set; }
    public long Size { get; private set; }
    public DateTime ModifiedTime { get; private set; }
    public DateTime CreationTime { get; private set; }
    public DateTime AccessTime { get; private set; }
    public STGM Mode { get; private set; }
    public int LocksSupported { get; private set; }
    public Guid Clsid { get; private set; }
    public int StateBits { get; private set; }
    [Browsable(false)]
    public byte[] Bytes { get; private set; }
}
