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
using System;
using System.Collections.Generic;

namespace OleViewDotNet.Security;

/// <summary>
/// SID class for COM.
/// </summary>
public sealed class COMSid : IEquatable<COMSid>
{
    internal Sid Sid { get; }

    internal COMSid(Sid sid)
    {
        Sid = sid;
    }

    public string Name => Sid.Name;

    public override string ToString()
    {
        return Sid.ToString();
    }

    public static bool TryParse(string name, out COMSid sid)
    {
        sid = null;
        if (name.ToLower() == "localsystem")
        {
            sid = new(KnownSids.LocalSystem);
            return true;
        }

        var value = Sid.Parse(name, false);
        if (!value.IsSuccess)
            value = NtSecurity.LookupAccountName(null, name, false);

        if (!value.IsSuccess)
            return false;

        sid = new(value.Result);
        return true;
    }

    public static COMSid Parse(string name)
    {
        if (!TryParse(name, out COMSid sid))
            throw new ArgumentException("Invalid SID string or account name.", nameof(name));
        return sid;
    }

    public static COMSid CurrentUser => new(NtToken.CurrentUser.Sid);

    public override bool Equals(object obj)
    {
        return Equals(obj as COMSid);
    }

    public bool Equals(COMSid other)
    {
        return other is not null &&
               EqualityComparer<Sid>.Default.Equals(Sid, other.Sid);
    }

    public override int GetHashCode()
    {
        return -591745145 + EqualityComparer<Sid>.Default.GetHashCode(Sid);
    }

    public static bool operator ==(COMSid left, COMSid right)
    {
        return EqualityComparer<COMSid>.Default.Equals(left, right);
    }

    public static bool operator !=(COMSid left, COMSid right)
    {
        return !(left == right);
    }
}
