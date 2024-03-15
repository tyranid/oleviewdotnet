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

internal struct InstantiationInfoData : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteGuid(classId);
        m.WriteInt32(classCtx);
        m.WriteInt32(actvflags);
        m.WriteInt32(fIsSurrogate);
        m.WriteInt32(cIID);
        m.WriteInt32(instFlag);
        m.WriteEmbeddedPointer(pIID, (g, l) => m.WriteConformantArrayCallback(g, m.WriteGuid, l), cIID);
        m.WriteInt32(thisSize);
        m.WriteStruct(clientCOMVersion);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        classId = u.ReadGuid();
        classCtx = u.ReadInt32();
        actvflags = u.ReadInt32();
        fIsSurrogate = u.ReadInt32();
        cIID = u.ReadInt32();
        instFlag = u.ReadInt32();
        pIID = u.ReadEmbeddedPointer(() => u.ReadConformantArrayCallback(u.ReadGuid), false);
        thisSize = u.ReadInt32();
        clientCOMVersion = u.ReadStruct<COMVERSION>();
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public Guid classId;
    public int classCtx;
    public int actvflags;
    public int fIsSurrogate;
    public int cIID;
    public int instFlag;
    public NdrEmbeddedPointer<Guid[]> pIID;
    public int thisSize;
    public COMVERSION clientCOMVersion;

    public static InstantiationInfoData CreateDefault()
    {
        return new InstantiationInfoData();
    }
    public InstantiationInfoData(Guid classId, int classCtx, int actvflags, int fIsSurrogate, int cIID, int instFlag, Guid[] pIID, int thisSize, COMVERSION clientCOMVersion)
    {
        this.classId = classId;
        this.classCtx = classCtx;
        this.actvflags = actvflags;
        this.fIsSurrogate = fIsSurrogate;
        this.cIID = cIID;
        this.instFlag = instFlag;
        this.pIID = pIID;
        this.thisSize = thisSize;
        this.clientCOMVersion = clientCOMVersion;
    }
}
