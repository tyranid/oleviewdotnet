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

public struct ExtensionActivationContextPropertiesData : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteUInt64(hostId);
        m.WriteStruct(userContextProperties);
        m.WriteGuid(componentProcessId);
        m.WriteUInt64(racActivationTokenId);
        m.WriteEmbeddedPointer(lpacAttributes, m.WriteStruct);
        m.WriteUInt64(consoleHandlesId);
        m.WriteUInt64(aamActivationId);
        m.WriteInt32(runFullTrust);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        hostId = u.ReadUInt64();
        userContextProperties = u.ReadStruct<UserContextPropertiesData>();
        componentProcessId = u.ReadGuid();
        racActivationTokenId = u.ReadUInt64();
        lpacAttributes = u.ReadEmbeddedPointer(u.ReadStruct<BLOB>, false);
        consoleHandlesId = u.ReadUInt64();
        aamActivationId = u.ReadUInt64();
        runFullTrust = u.ReadInt32();
    }

    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public ulong hostId;
    public UserContextPropertiesData userContextProperties;
    public Guid componentProcessId;
    public ulong racActivationTokenId;
    public NdrEmbeddedPointer<BLOB> lpacAttributes;
    public ulong consoleHandlesId;
    public ulong aamActivationId;
    public int runFullTrust;
    public static ExtensionActivationContextPropertiesData CreateDefault()
    {
        return new();
    }
}
