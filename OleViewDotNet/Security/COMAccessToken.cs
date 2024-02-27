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
using NtApiDotNet.Win32;
using NtApiDotNet.Win32.Security;
using NtApiDotNet.Win32.Security.Native;
using System;
using System.Linq;

namespace OleViewDotNet.Security;

/// <summary>
/// Class to represet an access token for access checking.
/// </summary>
public sealed class COMAccessToken : IDisposable
{
    public NtToken Token { get; }

    public COMAccessToken() 
        : this(NtToken.OpenProcessToken())
    {
    }

    private COMAccessToken(NtToken token)
    {
        using (token)
        {
            Token = token.DuplicateToken(TokenType.Impersonation, 
                SecurityImpersonationLevel.Identification, TokenAccessRights.GenericAll);
        }
    }

    public void Dispose()
    {
        Token.Dispose();
    }

    public static COMAccessToken GetAnonymous()
    {
        return new COMAccessToken(TokenUtils.GetAnonymousToken());
    }

    public static COMAccessToken FromToken(NtToken token)
    {
        return new COMAccessToken(token.Duplicate(TokenAccessRights.Duplicate));
    }

    public static COMAccessToken FromProcess(int pid)
    {
        NtToken.EnableDebugPrivilege();
        return new COMAccessToken(NtToken.OpenProcessToken(pid, false, TokenAccessRights.Duplicate));
    }

    public static COMAccessToken GetFiltered(COMAccessToken base_token, 
        bool lua_token, bool disable_max_privs, bool write_restricted, 
        COMSid[] sids_to_disable, COMSid[] restricted_sids)
    {
        using NtToken token = base_token?.Token.Duplicate() ?? NtToken.OpenProcessToken();
        FilterTokenFlags flags = 0;
        if (lua_token)
            flags |= FilterTokenFlags.LuaToken;
        if (disable_max_privs)
            flags |= FilterTokenFlags.DisableMaxPrivileges;
        if (write_restricted)
            flags |= FilterTokenFlags.LuaToken;

        return new COMAccessToken(token.Filter(flags,
            sids_to_disable?.Select(s => s.Sid), Array.Empty<Luid>(), 
            restricted_sids?.Select(s => s.Sid)));
    }

    public static COMAccessToken Logon(COMCredentials credentials, TokenLogonType logon_type)
    {
        return new COMAccessToken(Win32Security.LsaLogonUser(credentials.UserName,
            credentials.Domain, credentials.Password,
            (SecurityLogonType)logon_type, Logon32Provider.Default));
    }

    public static COMAccessToken LogonS4U(COMCredentials credentials, TokenLogonType logon_type)
    {
        return new COMAccessToken(LogonUtils.LsaLogonS4U(credentials.UserName,
            credentials.Domain, (SecurityLogonType)logon_type));
    }

    public void SetLowIntegrityLevel()
    {
        Token.SetIntegrityLevel(TokenIntegrityLevel.Low);
    }

    public override string ToString()
    {
        return Token.ToString();
    }
}