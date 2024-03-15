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
using NtApiDotNet.Win32.Rpc;
using System;

namespace OleViewDotNet.Rpc.Clients;

internal struct SpecialPropertiesData : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(dwSessionId);
        m.WriteInt32(fRemoteThisSessionId);
        m.WriteInt32(fClientImpersonating);
        m.WriteInt32(fPartitionIDPresent);
        m.WriteInt32(dwDefaultAuthnLvl);
        m.WriteGuid(guidPartition);
        m.WriteInt32(dwPRTFlags);
        m.WriteInt32(dwOrigClsctx);
        m.WriteInt32(dwFlags);
        m.WriteInt32(dwPid);
        m.WriteInt64(hwnd);
        m.WriteInt32(ulServiceId);
        m.WriteFixedPrimitiveArray(RpcUtils.CheckNull(Reserved3, "Reserved3"), 4);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        dwSessionId = u.ReadInt32();
        fRemoteThisSessionId = u.ReadInt32();
        fClientImpersonating = u.ReadInt32();
        fPartitionIDPresent = u.ReadInt32();
        dwDefaultAuthnLvl = u.ReadInt32();
        guidPartition = u.ReadGuid();
        dwPRTFlags = u.ReadInt32();
        dwOrigClsctx = u.ReadInt32();
        dwFlags = u.ReadInt32();
        dwPid = u.ReadInt32();
        hwnd = u.ReadInt64();
        ulServiceId = u.ReadInt32();
        Reserved3 = u.ReadFixedPrimitiveArray<int>(4);
    }
    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public int dwSessionId;
    public int fRemoteThisSessionId;
    public int fClientImpersonating;
    public int fPartitionIDPresent;
    public int dwDefaultAuthnLvl;
    public Guid guidPartition;
    public int dwPRTFlags;
    public int dwOrigClsctx;
    public int dwFlags;
    public int dwPid;
    public long hwnd;
    public int ulServiceId;
    public int[] Reserved3;

    public static SpecialPropertiesData CreateDefault()
    {
        SpecialPropertiesData ret = new SpecialPropertiesData();
        ret.Reserved3 = new int[4];
        return ret;
    }
    public SpecialPropertiesData(int dwSessionId, int fRemoteThisSessionId, int fClientImpersonating, int fPartitionIDPresent, int dwDefaultAuthnLvl, Guid guidPartition, int dwPRTFlags, int dwOrigClsctx, int dwFlags, int dwPid, long hwnd, int ulServiceId, int[] Reserved3)
    {
        this.dwSessionId = dwSessionId;
        this.fRemoteThisSessionId = fRemoteThisSessionId;
        this.fClientImpersonating = fClientImpersonating;
        this.fPartitionIDPresent = fPartitionIDPresent;
        this.dwDefaultAuthnLvl = dwDefaultAuthnLvl;
        this.guidPartition = guidPartition;
        this.dwPRTFlags = dwPRTFlags;
        this.dwOrigClsctx = dwOrigClsctx;
        this.dwFlags = dwFlags;
        this.dwPid = dwPid;
        this.hwnd = hwnd;
        this.ulServiceId = ulServiceId;
        this.Reserved3 = Reserved3;
    }
}
