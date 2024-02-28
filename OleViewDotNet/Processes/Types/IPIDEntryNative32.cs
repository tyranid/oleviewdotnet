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
internal struct IPIDEntryNative32 : IPIDEntryNativeInterface
{
    public int pNextIPID;
    public uint dwFlags;
    public int cStrongRefs;
    public int cWeakRefs;
    public int cPrivateRefs;
    public int pv;
    public int pStub;
    public int pOXIDEntry;
    public Guid ipid;
    public Guid iid;
    public int pChnl;
    public int pIRCEntry;
    public int pOIDFLink;
    public int pOIDBLink;

    uint IPIDEntryNativeInterface.Flags => dwFlags;

    IntPtr IPIDEntryNativeInterface.Interface => new IntPtr(pv);

    IntPtr IPIDEntryNativeInterface.Stub => new IntPtr(pStub);

    Guid IPIDEntryNativeInterface.Ipid => ipid;

    Guid IPIDEntryNativeInterface.Iid => iid;

    int IPIDEntryNativeInterface.StrongRefs => cStrongRefs;

    int IPIDEntryNativeInterface.WeakRefs => cWeakRefs;

    int IPIDEntryNativeInterface.PrivateRefs => cPrivateRefs;

    IPIDEntryNativeInterface IPIDEntryNativeInterface.GetNext(NtProcess process)
    {
        if (pNextIPID == 0)
            return null;
        return process.ReadStruct<IPIDEntryNative32>(pNextIPID);
    }

    IOXIDEntry IPIDEntryNativeInterface.GetOxidEntry(NtProcess process)
    {
        if (AppUtilities.IsWindows101909OrLess)
            return process.ReadStruct<OXIDEntryNative32>(pOXIDEntry);
        return process.ReadStruct<OXIDEntryNative2004_32>(pOXIDEntry);
    }
};
