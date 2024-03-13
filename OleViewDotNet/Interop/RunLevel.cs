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

namespace OleViewDotNet.Interop;

public enum RunLevel : uint
{
    LUA = 0x0,
    HIGHEST = 0x1,
    ADMIN = 0x2,
    MAX_NON_UIA = 0x3,
    LUA_UIA = 0x10,
    HIGHEST_UIA = 0x11,
    ADMIN_UIA = 0x12,
    MAX = 0x13,
    INVALID = 0xFFFFFFFF,
};
