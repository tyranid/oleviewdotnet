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

internal struct CustomPrivResolverInfo : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteUInt64(OxidServer);
        m.WriteEmbeddedPointer(pServerORBindings, m.WriteStruct);
        m.WriteStruct(OxidInfo);
        m.WriteUInt64(LocalMidOfRemote);
        m.WriteInt32(DllServerModel);
        m.WriteEmbeddedPointer(pwszDllServer, m.WriteTerminatedString);
        m.WriteInt32(FoundInROT);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        OxidServer = u.ReadUInt64();
        pServerORBindings = u.ReadEmbeddedPointer(u.ReadStruct<DUALSTRINGARRAY>, false);
        OxidInfo = u.ReadStruct<INTERNAL_OXID_INFO>();
        LocalMidOfRemote = u.ReadUInt64();
        DllServerModel = u.ReadInt32();
        pwszDllServer = u.ReadEmbeddedPointer(u.ReadConformantVaryingString, false);
        FoundInROT = u.ReadInt32();
    }
    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public ulong OxidServer;
    public NdrEmbeddedPointer<DUALSTRINGARRAY> pServerORBindings;
    public INTERNAL_OXID_INFO OxidInfo;
    public ulong LocalMidOfRemote;
    public int DllServerModel;
    public NdrEmbeddedPointer<string> pwszDllServer;
    public int FoundInROT;
    public static CustomPrivResolverInfo CreateDefault()
    {
        return new CustomPrivResolverInfo();
    }
}
