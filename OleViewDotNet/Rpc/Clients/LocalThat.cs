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

public struct LocalThat : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        throw new NotImplementedException();
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        marshalingSetId = u.ReadInt64();
        reserved = u.ReadInt32();
        pAsyncResponseBlock = u.ReadEmbeddedPointer(new Func<int>(u.ReadInt32), false);
        containerErrorInformation = u.ReadEmbeddedPointer(new Func<int>(u.ReadInt32), false);
        containerPassthroughData = u.ReadEmbeddedPointer(new Func<int>(u.ReadInt32), false);
    }
    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public long marshalingSetId;
    public int reserved;
    public NdrEmbeddedPointer<int> pAsyncResponseBlock;
    public NdrEmbeddedPointer<int> containerErrorInformation;
    public NdrEmbeddedPointer<int> containerPassthroughData;
}
