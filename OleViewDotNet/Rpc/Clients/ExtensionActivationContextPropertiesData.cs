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
        m.WriteInt64(hostId);
        m.WriteStruct(userContextProperties);
        m.WriteGuid(componentProcessId);
        m.WriteInt64(racActivationTokenId);
        m.WriteEmbeddedPointer(lpacAttributes, m.WriteStruct);
        m.WriteInt64(consoleHandlesId);
        m.WriteInt64(aamActivationId);
        m.WriteInt32(runFullTrust);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        hostId = u.ReadInt64();
        userContextProperties = u.ReadStruct<UserContextPropertiesData>();
        componentProcessId = u.ReadGuid();
        racActivationTokenId = u.ReadInt64();
        lpacAttributes = u.ReadEmbeddedPointer(u.ReadStruct<BLOB>, false);
        consoleHandlesId = u.ReadInt64();
        aamActivationId = u.ReadInt64();
        runFullTrust = u.ReadInt32();
    }

    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public long hostId;
    public UserContextPropertiesData userContextProperties;
    public Guid componentProcessId;
    public long racActivationTokenId;
    public NdrEmbeddedPointer<BLOB> lpacAttributes;
    public long consoleHandlesId;
    public long aamActivationId;
    public int runFullTrust;
    public static ExtensionActivationContextPropertiesData CreateDefault()
    {
        return new();
    }
}
