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

internal struct customREMOTE_REPLY_SCM_INFO : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt64(Oxid);
        m.WriteEmbeddedPointer(pdsaOxidBindings, m.WriteStruct);
        m.WriteGuid(ipidRemUnknown);
        m.WriteInt32(authnHint);
        m.WriteStruct(serverVersion);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        Oxid = u.ReadInt64();
        pdsaOxidBindings = u.ReadEmbeddedPointer(u.ReadStruct<DUALSTRINGARRAY>, false);
        ipidRemUnknown = u.ReadGuid();
        authnHint = u.ReadInt32();
        serverVersion = u.ReadStruct<COMVERSION>();
    }

    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public long Oxid;
    public NdrEmbeddedPointer<DUALSTRINGARRAY> pdsaOxidBindings;
    public Guid ipidRemUnknown;
    public int authnHint;
    public COMVERSION serverVersion;
    public static customREMOTE_REPLY_SCM_INFO CreateDefault()
    {
        return new customREMOTE_REPLY_SCM_INFO();
    }
    public customREMOTE_REPLY_SCM_INFO(long Oxid, DUALSTRINGARRAY? pdsaOxidBindings, Guid ipidRemUnknown, int authnHint, COMVERSION serverVersion)
    {
        this.Oxid = Oxid;
        this.pdsaOxidBindings = pdsaOxidBindings;
        this.ipidRemUnknown = ipidRemUnknown;
        this.authnHint = authnHint;
        this.serverVersion = serverVersion;
    }
}
