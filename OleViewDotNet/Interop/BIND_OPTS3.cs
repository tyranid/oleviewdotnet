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
public class BIND_OPTS3
{
    private readonly int cbStruct;
    public int grfFlags;
    public int grfMode;
    public int dwTickCountDeadline;
    public int dwTrackFlags;
    public CLSCTX dwClassContext;
    public int locale;
    public IntPtr pServerInfo;
    public IntPtr hwnd;

    public BIND_OPTS3()
    {
        cbStruct = Marshal.SizeOf(this);
    }
}
