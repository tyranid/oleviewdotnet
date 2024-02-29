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

using OleViewDotNet;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OleViewDotNet32;

public static class Program
{
    public enum RPC_AUTHN_LEVEL
    {
        DEFAULT = 0
    }

    public enum RPC_IMP_LEVEL
    {
        IMPERSONATE = 3
    }

    [Flags]
    public enum EOLE_AUTHENTICATION_CAPABILITIES
    {
        DYNAMIC_CLOAKING = 0x40
    }

    [DllImport("ole32.dll")]
    static extern int CoInitializeSecurity(
        IntPtr pSecDesc,
        int cAuthSvc,
        IntPtr asAuthSvc,
        IntPtr pReserved1,
        RPC_AUTHN_LEVEL dwAuthnLevel,
        RPC_IMP_LEVEL dwImpLevel,
        IntPtr pAuthList,
        EOLE_AUTHENTICATION_CAPABILITIES dwCapabilities,
        IntPtr pReserved3
    );

    // Run here to ensure it's called before the main thread.
    static readonly int _security_init = CoInitializeSecurity(IntPtr.Zero, -1, IntPtr.Zero, IntPtr.Zero, RPC_AUTHN_LEVEL.DEFAULT,
            RPC_IMP_LEVEL.IMPERSONATE, IntPtr.Zero,
            EOLE_AUTHENTICATION_CAPABILITIES.DYNAMIC_CLOAKING, IntPtr.Zero);

    [STAThread]
    public static void Main(string[] args)
    {
        Debug.Assert(_security_init == 0);
        EntryPoint.Run(args);
    }
}
