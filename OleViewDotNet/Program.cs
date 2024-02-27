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

using OleViewDotNet.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OleViewDotNet;

internal static class Program
{
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

    /// <summary>
    /// The main entry point for the application.
    /// This is just a 32bit stub for running on 64bit Windows.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        Debug.Assert(_security_init == 0);
        EntryPoint.Run(args);
    }
}
