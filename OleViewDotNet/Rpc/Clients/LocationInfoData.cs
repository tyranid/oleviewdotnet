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

internal struct LocationInfoData : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteEmbeddedPointer(machineName, m.WriteTerminatedString);
        m.WriteInt32(processId);
        m.WriteInt32(apartmentId);
        m.WriteInt32(contextId);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        machineName = u.ReadEmbeddedPointer(u.ReadConformantVaryingString, false);
        processId = u.ReadInt32();
        apartmentId = u.ReadInt32();
        contextId = u.ReadInt32();
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public NdrEmbeddedPointer<string> machineName;
    public int processId;
    public int apartmentId;
    public int contextId;

    public static LocationInfoData CreateDefault()
    {
        return new LocationInfoData();
    }
}
