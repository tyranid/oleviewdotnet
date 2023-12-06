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

namespace OleViewDotNet.Interop;

[Flags]
public enum STGM
{
    READ = 0x00000000,
    WRITE = 0x00000001,
    READWRITE = 0x00000002,
    SHARE_DENY_NONE = 0x00000040,
    SHARE_DENY_READ = 0x00000030,
    SHARE_DENY_WRITE = 0x00000020,
    SHARE_EXCLUSIVE = 0x00000010,
    PRIORITY = 0x00040000,
    CREATE = 0x00001000,
    CONVERT = 0x00020000,
    FAILIFTHERE = READ,
    DIRECT = READ,
    TRANSACTED = 0x00010000,
    NOSCRATCH = 0x00100000,
    NOSNAPSHOT = 0x00200000,
    SIMPLE = 0x08000000,
    DIRECT_SWMR = 0x00400000,
    DELETEONRELEASE = 0x04000000
}
