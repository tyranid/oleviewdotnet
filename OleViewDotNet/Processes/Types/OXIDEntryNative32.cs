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
using OleViewDotNet.Marshaling;
using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Processes.Types;

[StructLayout(LayoutKind.Sequential)]
internal struct OXIDEntryNative32 : IOXIDEntry
{
    public int _pNext;
    public int _pPrev;
    public int _dwPid;
    public int _dwTid;
    public Guid _moxid;
    public long _mid;
    public Guid _ipidRundown;
    public int _dwFlags;
    public int _hServerSTA;
    public int _pParentApt;
    public int _pSharedDefaultHandle;
    public int _pAuthId;
    public int _pBinding;
    public int _dwAuthnHint;
    public int _dwAuthnSvc;
    public int _pMIDEntry;
    public int _pRUSTA;
    public int _cRefs;
    public int _hComplete;
    public int _cCalls;
    public int _cResolverRef;
    public int _dwExpiredTime;
    private COMVERSION _version;
    public int _pAppContainerServerSecurityDescriptor;
    public int _ulMarshaledTargetInfoLength;
    public int _pMarshaledTargetInfo;
    public int _pszServerPackageFullName;
    public Guid _guidProcessIdentifier;

    int IOXIDEntry.Pid => _dwPid;

    int IOXIDEntry.Tid => _dwTid;

    Guid IOXIDEntry.MOxid => _moxid;

    long IOXIDEntry.Mid => _mid;

    IntPtr IOXIDEntry.ServerSTAHwnd => new IntPtr(_hServerSTA);

    COMDualStringArray IOXIDEntry.GetBinding(NtProcess process)
    {
        if (_pBinding == 0)
            return new COMDualStringArray();
        try
        {
            return new COMDualStringArray(new IntPtr(_pBinding), process);
        }
        catch (NtException)
        {
            return new COMDualStringArray();
        }
    }
}
