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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OleViewDotNet
{
    static class Program
    {
        enum EOLE_AUTHENTICATION_CAPABILITIES
        {
            EOAC_NONE = 0,
            EOAC_MUTUAL_AUTH = 0x1,
            EOAC_STATIC_CLOAKING = 0x20,
            EOAC_DYNAMIC_CLOAKING = 0x40,
            EOAC_ANY_AUTHORITY = 0x80,
            EOAC_MAKE_FULLSIC = 0x100,
            EOAC_DEFAULT = 0x800,
            EOAC_SECURE_REFS = 0x2,
            EOAC_ACCESS_CONTROL = 0x4,
            EOAC_APPID = 0x8,
            EOAC_DYNAMIC = 0x10,
            EOAC_REQUIRE_FULLSIC = 0x200,
            EOAC_AUTO_IMPERSONATE = 0x400,
            EOAC_NO_CUSTOM_MARSHAL = 0x2000,
            EOAC_DISABLE_AAA = 0x1000
        }

        enum AuthnLevel
        {
            RPC_C_AUTHN_LEVEL_DEFAULT = 0,
            RPC_C_AUTHN_LEVEL_NONE = 1,
            RPC_C_AUTHN_LEVEL_CONNECT = 2,
            RPC_C_AUTHN_LEVEL_CALL = 3,
            RPC_C_AUTHN_LEVEL_PKT = 4,
            RPC_C_AUTHN_LEVEL_PKT_INTEGRITY = 5,
            RPC_C_AUTHN_LEVEL_PKT_PRIVACY = 6
        }

        enum ImpLevel
        {
            RPC_C_IMP_LEVEL_DEFAULT = 0,
            RPC_C_IMP_LEVEL_ANONYMOUS = 1,
            RPC_C_IMP_LEVEL_IDENTIFY = 2,
            RPC_C_IMP_LEVEL_IMPERSONATE = 3,
            RPC_C_IMP_LEVEL_DELEGATE = 4,
        }

        [DllImport("ole32.dll")]
        static extern int CoInitializeSecurity(
            IntPtr pSecDesc,
            int cAuthSvc,
            IntPtr asAuthSvc,
            IntPtr pReserved1,
            AuthnLevel dwAuthnLevel,
            ImpLevel dwImpLevel,
            IntPtr pAuthList,
            EOLE_AUTHENTICATION_CAPABILITIES dwCapabilities,
            IntPtr pReserved3
        );

        // Run here to ensure it's called before the main thread.
        static readonly int _security_init = CoInitializeSecurity(IntPtr.Zero, -1, IntPtr.Zero, IntPtr.Zero, AuthnLevel.RPC_C_AUTHN_LEVEL_DEFAULT,
                ImpLevel.RPC_C_IMP_LEVEL_IMPERSONATE, IntPtr.Zero,
                EOLE_AUTHENTICATION_CAPABILITIES.EOAC_DYNAMIC_CLOAKING, IntPtr.Zero);

        /// <summary>
        /// The main entry point for the application.
        /// This is just a 32bit stub for running on 64bit Windows.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Debug.Assert(_security_init == 0);
            EntryPoint.Main(args);
        }
    }
}
