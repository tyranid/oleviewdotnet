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

public struct AsyncResponseBlock : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(asyncStatus);
        m.WriteInt32(reserved1);
        m.WriteInt64(uServerCausalityTraceId);
        m.WriteGuid(completionTraceActivity);
        m.WriteInt32(reserved2);
        m.WriteEmbeddedPointer(pOutcomeDetails, m.WriteStruct);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        asyncStatus = u.ReadInt32();
        reserved1 = u.ReadInt32();
        uServerCausalityTraceId = u.ReadInt64();
        completionTraceActivity = u.ReadGuid();
        reserved2 = u.ReadInt32();
        pOutcomeDetails = u.ReadEmbeddedPointer(u.ReadStruct<OutcomeDetails>, false);
    }
    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public int asyncStatus;
    public int reserved1;
    public long uServerCausalityTraceId;
    public Guid completionTraceActivity;
    public int reserved2;
    public NdrEmbeddedPointer<OutcomeDetails> pOutcomeDetails;
    public static AsyncResponseBlock CreateDefault()
    {
        return new AsyncResponseBlock();
    }
    public AsyncResponseBlock(int Member0, int Member4, long Member8, Guid Member10, int Member20, OutcomeDetails? Member28)
    {
        this.asyncStatus = Member0;
        this.reserved1 = Member4;
        this.uServerCausalityTraceId = Member8;
        this.completionTraceActivity = Member10;
        this.reserved2 = Member20;
        this.pOutcomeDetails = Member28;
    }
}

