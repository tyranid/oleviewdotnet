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

public struct OutcomeDetailsUnion : INdrNonEncapsulatedUnion
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        throw new NotImplementedException();
    }
    void INdrNonEncapsulatedUnion.Marshal(NdrMarshalBuffer m, long l)
    {
        Selector = (int)l;
        m.WriteInt32(Selector);
        switch (Selector)
        {
            case 0:
                m.WriteStruct(successDetails);
                break;
            case 1:
                m.WriteStruct(failureDetails);
                break;
            default:
                throw new ArgumentException("No matching union selector when marshaling Union_15");
        }
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        Selector = u.ReadInt32();
        switch (Selector)
        {
            case 0:
                successDetails = u.ReadStruct<SuccessDetailsResult>();
                break;
            case 1:
                failureDetails = u.ReadStruct<FailureDetailsResult>();
                break;
            default:
                throw new ArgumentException("No matching union selector when marshaling Union_15");
        }
    }

    int INdrStructure.GetAlignment()
    {
        return 1;
    }
    private int Selector;
    public SuccessDetailsResult successDetails;
    public FailureDetailsResult failureDetails;
    public static OutcomeDetailsUnion CreateDefault()
    {
        return new OutcomeDetailsUnion();
    }
    public OutcomeDetailsUnion(int Selector, SuccessDetailsResult Arm_0, FailureDetailsResult Arm_1)
    {
        this.Selector = Selector;
        this.successDetails = Arm_0;
        this.failureDetails = Arm_1;
    }
}
