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
using System;

namespace OleViewDotNet.Rpc.Clients;

internal struct InstanceInfoData : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteEmbeddedPointer(fileName, new Action<string>(m.WriteTerminatedString));
        m.WriteInt32(mode);
        m.WriteEmbeddedPointer(ifdROT, m.WriteStruct);
        m.WriteEmbeddedPointer(ifdStg, m.WriteStruct);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        fileName = u.ReadEmbeddedPointer(new Func<string>(u.ReadConformantVaryingString), false);
        mode = u.ReadInt32();
        ifdROT = u.ReadEmbeddedPointer(u.ReadStruct<MInterfacePointer>, false);
        ifdStg = u.ReadEmbeddedPointer(u.ReadStruct<MInterfacePointer>, false);
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public NdrEmbeddedPointer<string> fileName;
    public int mode;
    public NdrEmbeddedPointer<MInterfacePointer> ifdROT;
    public NdrEmbeddedPointer<MInterfacePointer> ifdStg;

    public static InstanceInfoData CreateDefault()
    {
        return new InstanceInfoData();
    }
    public InstanceInfoData(string fileName, int mode, MInterfacePointer? ifdROT, MInterfacePointer? ifdStg)
    {
        this.fileName = fileName;
        this.mode = mode;
        this.ifdROT = ifdROT;
        this.ifdStg = ifdStg;
    }
}
