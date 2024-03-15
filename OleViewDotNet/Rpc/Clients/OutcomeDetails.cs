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

public struct OutcomeDetails : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(outcome);
        m.WriteUnion(outcomeResult, outcome);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        outcome = u.ReadInt32();
        outcomeResult = u.ReadStruct<OutcomeDetailsUnion>();
    }

    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public int outcome;
    public OutcomeDetailsUnion outcomeResult;
    public static OutcomeDetails CreateDefault()
    {
        return new OutcomeDetails();
    }
    public OutcomeDetails(int Member0, OutcomeDetailsUnion Member8)
    {
        this.outcome = Member0;
        this.outcomeResult = Member8;
    }
}
