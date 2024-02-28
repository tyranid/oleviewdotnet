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
using System.Runtime.InteropServices;

namespace OleViewDotNet.Processes.Types;

[StructLayout(LayoutKind.Sequential)]
internal struct OXIDInfoNative2004
{
    public int _dwTid;
    public int _dwPid;
    public int _dwAuthnHint;
    public COMVERSION _dcomVersion;
    public CONTAINERVERSION _containerVersion;
    public Guid _ipidRemUnknown;
    public int _dwFlags;
    public IntPtr _psa;
    public Guid _guidProcessIdentifier;
    public long _processHostId;
    public int _clientDependencyBehavior;
    public IntPtr _packageFullName;
    public IntPtr _userSid;
    public IntPtr _appcontainerSid;
    public ulong _primaryOxid;
    public Guid _primaryIpidRemUnknown;
}
