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

internal struct ComWinRTActivationPropertiesData : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteStruct(winrtActivationPropertiesData);
        m.WriteStruct(userContextPropertiesData);
        m.WriteEmbeddedPointer(rtbProcessMitigationPolcyBlob, m.WriteStruct);
        m.WriteStruct(negotiatedContainerVersion);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        winrtActivationPropertiesData = u.ReadStruct<WinRTActivationPropertiesData>();
        userContextPropertiesData = u.ReadStruct<UserContextPropertiesData>();
        rtbProcessMitigationPolcyBlob = u.ReadEmbeddedPointer(u.ReadStruct<BLOB>, false);
        negotiatedContainerVersion = u.ReadStruct<CONTAINERVERSION>();
    }
    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public WinRTActivationPropertiesData winrtActivationPropertiesData;
    public UserContextPropertiesData userContextPropertiesData;
    public NdrEmbeddedPointer<BLOB> rtbProcessMitigationPolcyBlob;
    public CONTAINERVERSION negotiatedContainerVersion;
    public static ComWinRTActivationPropertiesData CreateDefault()
    {
        return new ComWinRTActivationPropertiesData();
    }
    public ComWinRTActivationPropertiesData(WinRTActivationPropertiesData winrtActivationPropertiesData, UserContextPropertiesData userContextPropertiesData, BLOB? rtbProcessMitigationPolcyBlob, CONTAINERVERSION negotiatedContainerVersion)
    {
        this.winrtActivationPropertiesData = winrtActivationPropertiesData;
        this.userContextPropertiesData = userContextPropertiesData;
        this.rtbProcessMitigationPolcyBlob = rtbProcessMitigationPolcyBlob;
        this.negotiatedContainerVersion = negotiatedContainerVersion;
    }
}
