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

using OleViewDotNet.Marshaling;
using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Interop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct COAUTHINFO
{
    public RpcAuthnService dwAuthnSvc;
    public int dwAuthzSvc;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string pwszServerPrincName;
    public RPC_AUTHN_LEVEL dwAuthnLevel;
    public RPC_IMP_LEVEL dwImpersonationLevel;
    public IntPtr pAuthIdentityData;
    public RPC_C_QOS_CAPABILITIES dwCapabilities;
}
