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
using System.Collections.Generic;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class PropsOut : IActivationProperty
{
    private PropsOutInfo m_inner;

    public IReadOnlyList<ActivationResult> Results
    {
        get
        {
            if (m_inner.cIfs == 0 || m_inner.piid is null || m_inner.ppIntfData is null || m_inner.phresults is null)
            {
                return Array.Empty<ActivationResult>();
            }
            List<ActivationResult> results = new();
            Guid[] iids = m_inner.piid.GetValue();
            MInterfacePointer?[] itfs = m_inner.ppIntfData.GetValue();
            int[] hrs = m_inner.phresults.GetValue();
            for (int i = 0; i < m_inner.cIfs; ++i)
            {
                COMObjRef objref = itfs[i].HasValue ? COMObjRef.FromArray(itfs[i].Value.abData) : null;
                results.Add(new(objref, iids[i], hrs[i]));
            }
            return results.AsReadOnly();
        }
    }

    internal PropsOut(byte[] data)
    {
        data.Deserialize(out m_inner);
    }

    public Guid PropertyClsid => ActivationGuids.CLSID_PropsOutInfo;

    public byte[] Serialize()
    {
        return m_inner.Serialize();
    }
}