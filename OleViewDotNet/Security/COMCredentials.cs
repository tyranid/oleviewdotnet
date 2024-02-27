//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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

using NtApiDotNet;
using OleViewDotNet.Interop;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace OleViewDotNet.Security;

public sealed class COMCredentials
{
    public string UserName { get; set; }
    public string Domain { get; set; }
    public SecureString Password { get; set; }

    private class DisposableString : IDisposable
    {
        public readonly IntPtr Pointer;
        public readonly int Length;

        public DisposableString(SecureString s)
        {
            if (s != null)
            {
                Pointer = Marshal.SecureStringToCoTaskMemUnicode(s);
                Length = s.Length * 2;
            }
        }

        void IDisposable.Dispose()
        {
            Marshal.ZeroFreeCoTaskMemUnicode(Pointer);
        }
    }

    internal SafeStructureInOutBuffer<COAUTHIDENTITY> ToBuffer(DisposableList list)
    {
        var user_buffer = list.AddResource(UserName != null ? Encoding.Unicode.GetBytes(UserName).ToBuffer() : SafeHGlobalBuffer.Null);
        var domain_buffer = list.AddResource(Domain != null ? Encoding.Unicode.GetBytes(Domain).ToBuffer() : SafeHGlobalBuffer.Null);
        var password_buffer = list.AddResource(new DisposableString(Password));

        return new COAUTHIDENTITY()
        {
            User = user_buffer.DangerousGetHandle(),
            UserLength = user_buffer.Length,
            Domain = domain_buffer.DangerousGetHandle(),
            DomainLength = domain_buffer.Length,
            Password = password_buffer.Pointer,
            PasswordLength = password_buffer.Length,
            Flags = COAUTHIDENTITY.SEC_WINNT_AUTH_IDENTITY_UNICODE
        }.ToBuffer();
    }
}
