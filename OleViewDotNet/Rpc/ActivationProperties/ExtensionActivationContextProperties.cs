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

public sealed class ExtensionActivationContextProperties : IActivationProperty
{
    private ExtensionActivationContextPropertiesData m_inner;

    public Guid PropertyClsid => ActivationGuids.CLSID_ExtensionActivationContextProperties;

    public ulong HostId { get => m_inner.hostId; set => m_inner.hostId = value; }
    public ulong UserContext
    {
        get => m_inner.userContextProperties.userContext;
        set => m_inner.userContextProperties.userContext = value;
    }
    public Guid ComponentProcessId { get => m_inner.componentProcessId; set => m_inner.componentProcessId = value; }
    public ulong RacActivationTokenId { get => m_inner.racActivationTokenId; set => m_inner.racActivationTokenId = value; }

    public byte[] LpacAttributes
    {
        get => m_inner.lpacAttributes?.GetValue().pBlobData?.GetValue();
        set
        {
            if (value is null)
                m_inner.lpacAttributes = null;
            else
                m_inner.lpacAttributes = new BLOB(value.Length, value);
        }
    }

    public ulong ConsoleHandlesId { get => m_inner.consoleHandlesId; set => m_inner.consoleHandlesId = value; }
    public ulong AamActivationId { get => m_inner.aamActivationId; set => m_inner.aamActivationId = value; }
    public bool RunFullTrust { get => m_inner.runFullTrust != 0; set => m_inner.runFullTrust = value ? 1 : 0; }

    internal ExtensionActivationContextProperties(byte[] data)
    {
        data.Deserialize(out m_inner);
    }

    public byte[] Serialize()
    {
        return m_inner.Serialize();
    }
}