//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2024
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

using NtApiDotNet.Ndr.Marshal;

namespace OleViewDotNet.Rpc.Clients;

internal struct COMVERSION : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt16(MajorVersion);
        m.WriteInt16(MinorVersion);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        MajorVersion = u.ReadInt16();
        MinorVersion = u.ReadInt16();
    }

    int INdrStructure.GetAlignment()
    {
        return 2;
    }

    public COMVERSION(short major, short minor)
    {
        MajorVersion = major;
        MinorVersion = minor;
    }

    public COMVERSION(ushort major, ushort minor) 
        : this((short)major, (short)minor)
    {
    }

    public short MajorVersion;
    public short MinorVersion;
}
