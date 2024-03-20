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
internal class OXIDEntryNative2004_32 : IOXIDEntry
{
    public int _flink;
    public int _blink;
    public int m_isInList;
    public OXIDInfoNative2004_32 info;
    public long _mid;
    public Guid _ipidRundown;
    public Guid _moxid;
    public byte _registered;
    public byte _stopped;
    public byte _pendingRelease;
    public byte _remotingInitialized;
    public int _hServerSTA;

    int IOXIDEntry.Pid => info._dwPid;

    int IOXIDEntry.Tid => info._dwTid;

    Guid IOXIDEntry.MOxid => _moxid;

    long IOXIDEntry.Mid => _mid;

    IntPtr IOXIDEntry.ServerSTAHwnd => new IntPtr(_hServerSTA);

    COMDualStringArray IOXIDEntry.GetBinding(NtProcess process)
    {
        if (info._psa == 0)
            return new COMDualStringArray();
        try
        {
            return new COMDualStringArray(new IntPtr(info._psa), process);
        }
        catch (NtException)
        {
            return new COMDualStringArray();
        }
    }
}
