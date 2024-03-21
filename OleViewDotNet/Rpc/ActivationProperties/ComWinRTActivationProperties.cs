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

public sealed class ComWinRTActivationProperties : IActivationProperty
{
    private ComWinRTActivationPropertiesData m_inner;

    public ComWinRTActivationProperties()
    {
    }

    internal ComWinRTActivationProperties(byte[] data)
    {
        data.Deserialize(out m_inner);
    }

    public string ActivatableClassId 
    { 
        get => m_inner.winrtActivationPropertiesData.activatableClassId;
        set => m_inner.winrtActivationPropertiesData.activatableClassId = string.IsNullOrEmpty(value) ? null : value; 
    }

    public string PackageFullName
    {
        get => m_inner.winrtActivationPropertiesData.packageFullName;
        set => m_inner.winrtActivationPropertiesData.packageFullName = string.IsNullOrEmpty(value) ? null : value;
    }

    public ulong UserContext 
    { 
        get => m_inner.userContextPropertiesData.userContext; 
        set => m_inner.userContextPropertiesData.userContext = value; 
    }

    public byte[] ProcessMitigationPolicy 
    {
        get => m_inner.rtbProcessMitigationPolcyBlob?.GetValue().pBlobData?.GetValue();
        set
        {
            if (value is null)
                m_inner.rtbProcessMitigationPolcyBlob = null;
            else
                m_inner.rtbProcessMitigationPolcyBlob = new BLOB(value.Length, value);
        }
    }

    public Guid PropertyClsid => ActivationGuids.CLSID_WinRTActivationProperties;

    public byte[] Serialize()
    {
        return m_inner.Serialize();
    }
}
