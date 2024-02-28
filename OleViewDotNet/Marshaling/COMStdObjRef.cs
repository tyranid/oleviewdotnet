//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017
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
using System.IO;

namespace OleViewDotNet.Marshaling;

internal class COMStdObjRef
{
    public COMStdObjRefFlags StdFlags { get; set; }
    public int PublicRefs { get; set; }
    public ulong Oxid { get; set; }
    public ulong Oid { get; set; }
    public Guid Ipid { get; set; }

    public COMStdObjRef()
    {
    }

    internal COMStdObjRef(BinaryReader reader)
    {
        StdFlags = (COMStdObjRefFlags)reader.ReadInt32();
        PublicRefs = reader.ReadInt32();
        Oxid = reader.ReadUInt64();
        Oid = reader.ReadUInt64();
        Ipid = reader.ReadGuid();
    }

    public void ToWriter(BinaryWriter writer)
    {
        writer.Write((int)StdFlags);
        writer.Write(PublicRefs);
        writer.Write(Oxid);
        writer.Write(Oid);
        writer.Write(Ipid);
    }

    internal COMStdObjRef Clone()
    {
        return (COMStdObjRef)MemberwiseClone();
    }
}
