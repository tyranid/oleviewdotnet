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
public enum EOLE_AUTHENTICATION_CAPABILITIES
{
    NONE = 0,
    MUTUAL_AUTH = 0x1,
    STATIC_CLOAKING = 0x20,
    DYNAMIC_CLOAKING = 0x40,
    ANY_AUTHORITY = 0x80,
    MAKE_FULLSIC = 0x100,
    DEFAULT = 0x800,
    SECURE_REFS = 0x2,
    ACCESS_CONTROL = 0x4,
    APPID = 0x8,
    DYNAMIC = 0x10,
    REQUIRE_FULLSIC = 0x200,
    AUTO_IMPERSONATE = 0x400,
    NO_CUSTOM_MARSHAL = 0x2000,
    DISABLE_AAA = 0x1000
}
