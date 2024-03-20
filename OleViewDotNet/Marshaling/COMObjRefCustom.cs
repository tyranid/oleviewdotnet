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
using System;
using System.IO;

namespace OleViewDotNet.Marshaling;

public class COMObjRefCustom : COMObjRef
{
    public Guid Clsid { get; set; }
    public int Reserved { get; set; }
    public byte[] ExtensionData { get; set; }
    public byte[] ObjectData { get; set; }

    public COMObjRefCustom()
        : base(COMKnownGuids.IID_IUnknown)
    {
        ObjectData = new byte[0];
        ExtensionData = new byte[0];
    }

    internal COMObjRefCustom(BinaryReader reader, Guid iid)
        : base(iid)
    {
        Clsid = reader.ReadGuid();
        // Size of extension data but can be 0.
        int extension = reader.ReadInt32();
        ExtensionData = new byte[extension];
        Reserved = reader.ReadInt32();
        if (extension > 0)
        {
            ExtensionData = reader.ReadAll(extension);
        }
        // Read to end of stream.
        ObjectData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
    }

    protected override void Serialize(BinaryWriter writer)
    {
        writer.Write(Clsid);
        writer.Write(ExtensionData.Length);
        writer.Write(Reserved);
        writer.Write(ExtensionData);
        writer.Write(ObjectData);
    }
}
