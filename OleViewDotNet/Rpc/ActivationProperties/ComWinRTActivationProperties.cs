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
using OleViewDotNet.Rpc.Clients;
using System;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class ComWinRTActivationProperties : IActivationProperty
{
    private ComWinRTActivationPropertiesData m_inner;

    public ComWinRTActivationProperties(NdrPickledType pickled_type)
    {
        m_inner = new NdrUnmarshalBuffer(pickled_type).ReadStruct<ComWinRTActivationPropertiesData>();
    }

    public Guid PropertyClsid => ActivationGuids.CLSID_WinRTActivationProperties;

    public byte[] Serialize()
    {
        NdrMarshalBuffer m = new();
        m.WriteStruct(m_inner);
        return m.ToPickledType().ToArray();
    }
}