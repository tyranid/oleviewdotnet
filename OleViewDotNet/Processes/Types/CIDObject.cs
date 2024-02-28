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
using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Processes.Types;

[StructLayout(LayoutKind.Sequential)]
internal struct CIDObject : IIDObject
{
    public IntPtr VTablePtr;
    public SHashChain _pidChain;
    [ChainOffset]
    public SHashChain _oidChain;
    public int _dwState;
    public int _cRefs;
    public IntPtr _pServer;
    public IntPtr _pServerCtx; // CObjectContext* 
    public Guid _oid;
    public int _aptID;
    public IntPtr _pStdWrapper; // CStdWrapper* 
    public IntPtr _pStdID; // CStdIdentity* 
    public int _cCalls;
    public int _cLocks;
    public SHashChain _oidUnpinReqChain;
    public int _dwOidUnpinReqState;
    public IntPtr _pvObjectTrackCookie; // void* 

    IIDObject IIDObject.GetNextOid(NtProcess process, IntPtr head_ptr)
    {
        if (_oidChain.pNext == head_ptr)
            return null;
        return process.ReadStruct<CIDObject>(_oidChain.pNext.ToInt64());
    }

    Guid IIDObject.GetOid()
    {
        return _oid;
    }

    IStdIdentity IIDObject.GetStdIdentity(NtProcess process)
    {
        if (_pStdID == IntPtr.Zero)
        {
            return null;
        }
        return process.ReadStruct<CStdIdentity>(_pStdID.ToInt64());
    }

    IStdWrapper IIDObject.GetStdWrapper(NtProcess process)
    {
        if (_pStdWrapper == IntPtr.Zero)
            return null;
        return process.ReadStruct<CStdWrapper>(_pStdWrapper.ToInt64());
    }
}
