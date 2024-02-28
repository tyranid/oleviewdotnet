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

namespace OleViewDotNet.Processes.Types;

[Flags]
internal enum SMFLAGS
{
    SMFLAGS_CLIENT_SIDE = 0x1,
    SMFLAGS_PENDINGDISCONNECT = 0x2,
    SMFLAGS_REGISTEREDOID = 0x4,
    SMFLAGS_DISCONNECTED = 0x8,
    SMFLAGS_FIRSTMARSHAL = 0x10,
    SMFLAGS_HANDLER = 0x20,
    SMFLAGS_WEAKCLIENT = 0x40,
    SMFLAGS_IGNORERUNDOWN = 0x80,
    SMFLAGS_CLIENTMARSHALED = 0x100,
    SMFLAGS_NOPING = 0x200,
    SMFLAGS_TRIEDTOCONNECT = 0x400,
    SMFLAGS_CSTATICMARSHAL = 0x800,
    SMFLAGS_USEAGGSTDMARSHAL = 0x1000,
    SMFLAGS_SYSTEM = 0x2000,
    SMFLAGS_DEACTIVATED = 0x4000,
    SMFLAGS_FTM = 0x8000,
    SMFLAGS_CLIENTPOLICYSET = 0x10000,
    SMFLAGS_APPDISCONNECT = 0x20000,
    SMFLAGS_SYSDISCONNECT = 0x40000,
    SMFLAGS_RUNDOWNDISCONNECT = 0x80000,
    SMFLAGS_CLEANEDUP = 0x100000,
    SMFLAGS_LIGHTNA = 0x200000,
    SMFLAGS_FASTRUNDOWN = 0x400000,
    SMFLAGS_IMPLEMENTS_IAGILEOBJECT = 0x800000,
    SMFLAGS_ALLOW_ASTA_TO_ASTA_DEADLOCK_RISK = 0x1000000,
    SMFLAGS_SAFE_TO_QI_DURING_DISCONNECT = 0x2000000,
    SMFLAGS_CHECKSUSPEND = 0x4000000,
    SMFLAGS_DISABLE_ASYNC_REMOTING_FOR_WINRT_ASYNC = 0x8000000,
    SMFLAGS_PROXY_TO_INPROC_OBJECT = 0x10000000,
    SMFLAGS_MADE_WINRT_ASYNC_CALL = 0x20000000,
    SMFLAGS_RUNDOWN_OBJECT_OF_INTEREST = 0x40000000,
    SMFLAGS_ALL = 0x7FFFFFFF,
};
