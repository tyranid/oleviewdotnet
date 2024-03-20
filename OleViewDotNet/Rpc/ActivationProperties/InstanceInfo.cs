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

using OleViewDotNet.Marshaling;
using OleViewDotNet.Rpc.Clients;
using System;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class InstanceInfo : IActivationProperty
{
    private InstanceInfoData m_inner;

    internal InstanceInfo(byte[] data)
    {
        data.Deserialize(out m_inner);
    }

    public InstanceInfo()
    {
    }

    public Guid PropertyClsid => ActivationGuids.CLSID_InstanceInfo;

    public string FileName { get => m_inner.fileName; set => m_inner.fileName = value; }
    public int Mode { get => m_inner.mode; set => m_inner.mode = value; }
    public COMObjRef IfdROT { get => m_inner.ifdROT.ToObjRef(); set => m_inner.ifdROT = value.ToPointer(); }
    public COMObjRef IfdStg { get => m_inner.ifdStg.ToObjRef(); set => m_inner.ifdStg = value.ToPointer(); }

    public byte[] Serialize()
    {
        return m_inner.Serialize();
    }
}