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

namespace OleViewDotNet.Security;

/// <summary>
/// Class to represent a COM security descriptor.
/// </summary>
public sealed class COMSecurityDescriptor
{
    public SecurityDescriptor SecurityDescriptor { get; }

    public COMSid Owner => new(SecurityDescriptor.Owner.Sid);

    public COMSecurityDescriptor(string sddl) 
        : this(new SecurityDescriptor(sddl))
    {
    }

    public COMSecurityDescriptor(SecurityDescriptor sd)
    {
        SecurityDescriptor = sd;
    }

    public COMSecurityDescriptor(byte[] ba)
    {
        SecurityDescriptor = new(ba);
    }

    public string ToBase64()
    {
        return SecurityDescriptor.ToBase64();
    }

    public string ToSddl()
    {
        return SecurityDescriptor.ToSddl();
    }

    public override string ToString()
    {
        return SecurityDescriptor.ToString();
    }
}