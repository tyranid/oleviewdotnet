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

using OleViewDotNet.Interop;
using OleViewDotNet.Rpc.Clients;
using System;

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class SpecialSystemProperties : IActivationProperty
{
    private SpecialPropertiesData m_inner;

    internal SpecialSystemProperties(byte[] data)
    {
        data.Deserialize(out m_inner);
    }

    public SpecialSystemProperties()
    {
    }

    public int SessionId { get => m_inner.dwSessionId; set => m_inner.dwSessionId = value; }
    public bool RemoteThisSessionId { get => m_inner.fRemoteThisSessionId != 0; set => m_inner.fRemoteThisSessionId = value ? 1 : 0; }
    public bool ClientImpersonating { get => m_inner.fClientImpersonating != 0; set => m_inner.fClientImpersonating = value ? 1 : 0; }
    public RPC_AUTHN_LEVEL DefaultAuthnLvl { get => (RPC_AUTHN_LEVEL)m_inner.dwDefaultAuthnLvl; set => m_inner.dwDefaultAuthnLvl = (int)value; }
    public Guid? Partition
    {
        get => m_inner.fPartitionIDPresent != 0 ? m_inner.guidPartition : null;
        set
        {
            if (value.HasValue)
            {
                m_inner.guidPartition = value.Value;
                m_inner.fPartitionIDPresent = 1;
            }
            else
            {
                m_inner.guidPartition = Guid.Empty;
                m_inner.fPartitionIDPresent = 0;
            }
        }
    }
    public ProcessRequestType PRTFlags { get => (ProcessRequestType)m_inner.dwPRTFlags; set => m_inner.dwPRTFlags = (int)value; }
    public CLSCTX OrigClsctx { get => (CLSCTX)m_inner.dwOrigClsctx; set => m_inner.dwOrigClsctx = (int)value; }
    public SpecialSystemPropertiesFlags Flags { get => (SpecialSystemPropertiesFlags)m_inner.dwFlags; set => m_inner.dwFlags = (int)value; }
    public int Pid { get => m_inner.dwPid; set => m_inner.dwPid = value; }
    public long Hwnd { get => m_inner.hwnd; set => m_inner.hwnd = value; }
    public int ServiceId { get => m_inner.ulServiceId; set => m_inner.ulServiceId = value; }

    public Guid PropertyClsid => ActivationGuids.CLSID_SpecialSystemProperties;

    public byte[] Serialize()
    {
        return m_inner.Serialize();
    }
}
