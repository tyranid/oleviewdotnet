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

public struct CONTAINERVERSION : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(version);
        m.WriteInt64(capabilityFlags);
        m.WriteEmbeddedPointer(extensions, m.WriteStruct);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        version = u.ReadInt32();
        capabilityFlags = u.ReadInt64();
        extensions = u.ReadEmbeddedPointer(u.ReadStruct<CONTAINER_EXTENT_ARRAY>, false);
    }
    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public int version;
    public long capabilityFlags;
    public NdrEmbeddedPointer<CONTAINER_EXTENT_ARRAY> extensions;
    public static CONTAINERVERSION CreateDefault()
    {
        return new CONTAINERVERSION();
    }
    public CONTAINERVERSION(int version, long capabilityFlags, CONTAINER_EXTENT_ARRAY? extensions)
    {
        this.version = version;
        this.capabilityFlags = capabilityFlags;
        this.extensions = extensions;
    }
}
