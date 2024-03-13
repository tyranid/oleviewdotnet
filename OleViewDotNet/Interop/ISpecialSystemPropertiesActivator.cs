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

using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Interop;

[Guid("000001B9-0000-0000-c000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface ISpecialSystemPropertiesActivator
{
    void SetSessionId(int dwSessionId, int bUseConsole, int fRemoteThisSessionId);
    void GetSessionId(out int pdwSessionId, out int pbUseConsole);
    void GetSessionId2(out int pdwSessionId, out int pbUseConsole, out int pfRemoteThisSessionId);
    void SetClientImpersonating(int fClientImpersonating);
    int GetClientImpersonating();
    void SetPartitionId(in Guid guidPartition);
    Guid GetPartitionId();
    void SetProcessRequestType(ProcessRequestType dwPRT);
    ProcessRequestType GetProcessRequestType();
    void SetOrigClsctx(CLSCTX dwOrigClsctx);
    CLSCTX GetOrigClsctx();
    RPC_AUTHN_LEVEL GetDefaultAuthenticationLevel();
    void SetDefaultAuthenticationLevel(RPC_AUTHN_LEVEL dwDefaultAuthnLvl);
    void GetLUARunLevel(out RunLevel pdwLUARunLevel, out IntPtr phwnd);
    void SetLUARunLevel(RunLevel dwLUARunLevel, IntPtr hwnd);
}
