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

internal struct ScmRequestInfoData : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteEmbeddedPointer(pScmInfo, m.WriteStruct);
        m.WriteEmbeddedPointer(remoteRequest, m.WriteStruct);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        pScmInfo = u.ReadEmbeddedPointer(u.ReadStruct<CustomPrivScmInfo>, false);
        remoteRequest = u.ReadEmbeddedPointer(u.ReadStruct<customREMOTE_REQUEST_SCM_INFO>, false);
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public NdrEmbeddedPointer<CustomPrivScmInfo> pScmInfo;
    public NdrEmbeddedPointer<customREMOTE_REQUEST_SCM_INFO> remoteRequest;

    public static ScmRequestInfoData CreateDefault()
    {
        return new ScmRequestInfoData();
    }
    public ScmRequestInfoData(CustomPrivScmInfo? pScmInfo, customREMOTE_REQUEST_SCM_INFO? remoteRequest)
    {
        this.pScmInfo = pScmInfo;
        this.remoteRequest = remoteRequest;
    }
}
