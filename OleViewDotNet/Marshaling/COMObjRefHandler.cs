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

public class COMObjRefHandler : COMObjRefStandard
{
    public Guid Clsid { get; set; }

    internal COMObjRefHandler(BinaryReader reader, Guid iid)
        : base(iid)
    {
        _stdobjref = new COMStdObjRef(reader);
        Clsid = reader.ReadGuid();
        _stringarray = new COMDualStringArray(reader);
    }

    internal COMObjRefHandler(Guid clsid, COMObjRefStandard std) : base(std)
    {
        Clsid = clsid;
    }

    public COMObjRefHandler() : base()
    {
    }

    protected override void Serialize(BinaryWriter writer)
    {
        _stdobjref.ToWriter(writer);
        writer.Write(Clsid);
        _stringarray.ToWriter(writer);
    }
}
