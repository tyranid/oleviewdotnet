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

using OleViewDotNet.Rpc.Clients;
using System;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class LocationInfo : IActivationProperty
{
    private LocationInfoData m_inner;

    internal LocationInfo(byte[] data)
    {
        data.Deserialize(out m_inner);
    }

    public LocationInfo()
    {
    }

    public Guid PropertyClsid => ActivationGuids.CLSID_ServerLocationInfo;

    public string MachineName
    {
        get => m_inner.machineName;
        set => m_inner.machineName = value;
    }

    public int ProcessId
    {
        get => m_inner.processId;
        set => m_inner.processId = value;
    }

    public int ApartmentId
    {
        get => m_inner.apartmentId;
        set => m_inner.apartmentId = value;
    }

    public int ContextId
    {
        get => m_inner.contextId;
        set => m_inner.contextId = value;
    }

    public byte[] Serialize()
    {
        return m_inner.Serialize();
    }
}