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
using OleViewDotNet.Marshaling;
using OleViewDotNet.Rpc.Clients;
using System;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class ActivationContextInfo : IActivationProperty
{
    private ActivationContextInfoData m_inner;

    public ActivationContextInfo(byte[] data)
    {
        data.Deserialize(out m_inner);
    }

    public ActivationContextInfo()
    {
    }

    public int ClientOK { get => m_inner.clientOK; set => m_inner.clientOK = value; }
    public COMObjRef ClientCtx { get => m_inner.pIFDClientCtx.ToObjRef(); set => m_inner.pIFDClientCtx = value.ToPointer(); }
    public COMObjRef PrototypeCtx { get => m_inner.pIFDPrototypeCtx.ToObjRef(); set => m_inner.pIFDPrototypeCtx = value.ToPointer(); }

    public Guid PropertyClsid => ActivationGuids.CLSID_ActivationContextInfo;

    public byte[] Serialize()
    {
        return m_inner.Serialize();
    }
}
