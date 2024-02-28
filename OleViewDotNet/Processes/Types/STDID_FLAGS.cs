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
internal enum STDID_FLAGS
{
    STDID_SERVER = 0x0,
    STDID_CLIENT = 0x1,
    STDID_FREETHREADED = 0x2,
    STDID_HAVEID = 0x4,
    STDID_IGNOREID = 0x8,
    STDID_AGGREGATED = 0x10,
    STDID_INDESTRUCTOR = 0x100,
    STDID_LOCKEDINMEM = 0x200,
    STDID_DEFHANDLER = 0x400,
    STDID_AGGID = 0x800,
    STDID_STCMRSHL = 0x1000,
    STDID_REMOVEDFROMIDOBJ = 0x2000,
    STDID_SYSTEM = 0x4000,
    STDID_FTM = 0x8000,
    STDID_NOIEC = 0x10000,
    STDID_FASTRUNDOWN = 0x20000,
    STDID_AGILE_OOP_PROXY = 0x40000,
    STDID_SUPPRESS_WAKE_SET = 0x80000,
    STDID_IMPLEMENTS_IAGILEOBJECT = 0x100000,
    STDID_ALLOW_ASTA_TO_ASTA_DEADLOCK_RISK = 0x200000,
    STDID_DO_NOT_DISTURB_SET = 0x400000,
    STDID_DISABLE_ASYNC_REMOTING_FOR_WINRT_ASYNC = 0x800000,
    STDID_FAKE_OID_FOR_INTERNAL_PROXY = 0x1000000,
    STDID_RUNDOWN_OBJECT_OF_INTEREST = 0x2000000,
    STDID_ALL = 0x3FFFF1F,
};
