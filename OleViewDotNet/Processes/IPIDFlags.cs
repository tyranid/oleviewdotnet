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

using System;

namespace OleViewDotNet.Processes;

[Flags]
public enum IPIDFlags : uint
{
    IPIDF_CONNECTING = 0x1,
    IPIDF_DISCONNECTED = 0x2,
    IPIDF_SERVERENTRY = 0x4,
    IPIDF_NOPING = 0x8,
    IPIDF_COPY = 0x10,
    IPIDF_VACANT = 0x80,
    IPIDF_NONNDRSTUB = 0x100,
    IPIDF_NONNDRPROXY = 0x200,
    IPIDF_NOTIFYACT = 0x400,
    IPIDF_TRIED_ASYNC = 0x800,
    IPIDF_ASYNC_SERVER = 0x1000,
    IPIDF_DEACTIVATED = 0x2000,
    IPIDF_WEAKREFCACHE = 0x4000,
    IPIDF_STRONGREFCACHE = 0x8000,
    IPIDF_UNSECURECALLSALLOWED = 0x10000,
}
