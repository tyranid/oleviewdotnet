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

using OleViewDotNet.Interop;
using OleViewDotNet.Utilities;
using System;
using System.IO;

namespace OleViewDotNet.Marshaling;

public abstract class COMObjRef
{
    public const int OBJREF_MAGIC = 0x574f454d;

    public Guid Iid { get; set; }

    public COMObjrefFlags Flags
    {
        get
        {
            if (this is COMObjRefCustom)
            {
                return COMObjrefFlags.Custom;
            }
            else if (this is COMObjRefHandler)
            {
                return COMObjrefFlags.Handler;
            }
            else if (this is COMObjRefStandard)
            {
                return COMObjrefFlags.Standard;
            }
            else
            {
                return COMObjrefFlags.None;
            }
        }
    }

    public byte[] ToArray()
    {
        MemoryStream stm = new();
        BinaryWriter writer = new(stm);
        writer.Write(OBJREF_MAGIC);
        writer.Write((int)Flags);
        writer.Write(Iid);
        Serialize(writer);
        return stm.ToArray();
    }

    public string ToMoniker()
    {
        return $"objref:{Convert.ToBase64String(ToArray())}:";
    }

    protected abstract void Serialize(BinaryWriter writer);

    protected COMObjRef(Guid iid)
    {
        Iid = iid;
    }

    public static COMObjRef FromArray(byte[] arr)
    {
        MemoryStream stm = new(arr);
        BinaryReader reader = new(stm);
        int magic = reader.ReadInt32();
        if (magic != OBJREF_MAGIC)
        {
            throw new ArgumentException("Invalid OBJREF Magic");
        }

        COMObjrefFlags flags = (COMObjrefFlags)reader.ReadInt32();
        Guid iid = reader.ReadGuid();
        return flags switch
        {
            COMObjrefFlags.Custom => new COMObjRefCustom(reader, iid),
            COMObjrefFlags.Standard => new COMObjRefStandard(reader, iid),
            COMObjrefFlags.Handler => new COMObjRefHandler(reader, iid),
            _ => throw new ArgumentException("Invalid OBJREF Type Flags"),
        };
    }

    public static COMObjRef FromObject(object obj, Guid iid, MSHCTX mshctx, MSHLFLAGS mshflags)
    {
        return FromArray(COMUtilities.MarshalObject(obj, iid, mshctx, mshflags));
    }
}