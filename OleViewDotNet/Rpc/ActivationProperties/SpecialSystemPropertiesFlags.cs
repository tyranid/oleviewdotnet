//    Copyright (C) James Forshaw 2024
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

namespace OleViewDotNet.Rpc.ActivationProperties;

[Flags]
public enum SpecialSystemPropertiesFlags
{
    NONE = 0,
    USE_CONSOLE_SESSION = 0x1,
    USE_DEFAULT_AUTHN_LVL = 0x2,
    USE_SERVER_PID = 0x4,
    USE_LUA_LEVEL_ADMIN = 0x8,
    COAUTH_USER_IS_NULL = 0x10,
    COAUTH_DOMAIN_IS_NULL = 0x20,
    COAUTH_PWD_IS_NULL = 0x40,
    USE_LUA_LEVEL_HIGHEST = 0x80,
}
