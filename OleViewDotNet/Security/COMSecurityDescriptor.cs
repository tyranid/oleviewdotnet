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

    public bool HasContainerAccess
    {
        get
        {
            if (SecurityDescriptor.DaclPresent)
            {
                foreach (var ace in SecurityDescriptor.Dacl)
                {
                    if (ace.Mask.IsAccessGranted(COMAccessRights.ActivateContainer | COMAccessRights.ExecuteContainer))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

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

    public override bool Equals(object obj)
    {
        return obj is COMSecurityDescriptor descriptor &&
               ToBase64() == descriptor.ToBase64();
    }

    public override int GetHashCode()
    {
        return -1748118390 + ToBase64().GetHashCode();
    }
}