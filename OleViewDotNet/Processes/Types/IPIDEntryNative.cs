//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017
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

using NtApiDotNet;
using OleViewDotNet.Utilities;
using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Processes.Types;

[StructLayout(LayoutKind.Sequential)]
internal struct IPIDEntryNative : IPIDEntryNativeInterface
{
    public IntPtr pNextIPID;
    public uint dwFlags;
    public int cStrongRefs;
    public int cWeakRefs;
    public int cPrivateRefs;
    public IntPtr pv;
    public IntPtr pStub;
    public IntPtr pOXIDEntry;
    public Guid ipid;
    public Guid iid;
    public IntPtr pChnl;
    public IntPtr pIRCEntry;
    public IntPtr pOIDFLink;
    public IntPtr pOIDBLink;

    uint IPIDEntryNativeInterface.Flags => dwFlags;

    IntPtr IPIDEntryNativeInterface.Interface => pv;

    IntPtr IPIDEntryNativeInterface.Stub => pStub;

    Guid IPIDEntryNativeInterface.Ipid => ipid;

    Guid IPIDEntryNativeInterface.Iid => iid;

    int IPIDEntryNativeInterface.StrongRefs => cStrongRefs;

    int IPIDEntryNativeInterface.WeakRefs => cWeakRefs;

    int IPIDEntryNativeInterface.PrivateRefs => cPrivateRefs;

    IPIDEntryNativeInterface IPIDEntryNativeInterface.GetNext(NtProcess process)
    {
        if (pNextIPID == IntPtr.Zero)
            return null;
        return process.ReadStruct<IPIDEntryNative>(pNextIPID.ToInt64());
    }

    IOXIDEntry IPIDEntryNativeInterface.GetOxidEntry(NtProcess process)
    {
        if (AppUtilities.IsWindows101909OrLess)
            return process.ReadStruct<OXIDEntryNative>(pOXIDEntry.ToInt64());
        return process.ReadStruct<OXIDEntryNative2004>(pOXIDEntry.ToInt64());
    }
};
