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

namespace OleViewDotNet.Interop;

public readonly struct COMVersion : IEquatable<COMVersion>
{
    public readonly ushort Major;
    public readonly ushort Minor;

    internal COMVersion(ushort major, ushort minor)
    {
        Major = major;
        Minor = minor;
    }

    internal COMVersion(short major, short minor)
    {
        Major = (ushort)major;
        Minor = (ushort)minor;
    }

    public static COMVersion Parse(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException($"'{nameof(version)}' cannot be null or whitespace.", nameof(version));
        }

        if (!TryParse(version, out COMVersion ret))
        {
            throw new FormatException("Invalid version string.");
        }
        return ret;
    }

    public static bool TryParse(string version, out COMVersion ret)
    {
        ret = new();
        if (string.IsNullOrWhiteSpace(version))
        {
            return false;
        }

        string[] vals = version.Split('.');
        if (vals.Length != 2)
        {
            return false;
        }

        if (!ushort.TryParse(vals[0], out ushort major) || !ushort.TryParse(vals[1], out ushort minor))
        {
            return false;
        }

        ret = new(major, minor);
        return true;
    }

    public override bool Equals(object obj)
    {
        return obj is COMVersion version && Equals(version);
    }

    public bool Equals(COMVersion other)
    {
        return Major == other.Major &&
               Minor == other.Minor;
    }

    public override int GetHashCode()
    {
        int hashCode = 317314336;
        hashCode = hashCode * -1521134295 + Major.GetHashCode();
        hashCode = hashCode * -1521134295 + Minor.GetHashCode();
        return hashCode;
    }

    public override string ToString()
    {
        return $"{Major}.{Minor}";
    }

    public static bool operator ==(COMVersion left, COMVersion right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(COMVersion left, COMVersion right)
    {
        return !(left == right);
    }
}
