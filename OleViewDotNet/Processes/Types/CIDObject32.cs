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
internal struct CIDObject32 : IIDObject
{
    public int VTablePtr;
    public SHashChain32 _pidChain;
    [ChainOffset]
    public SHashChain32 _oidChain;
    public int _dwState;
    public int _cRefs;
    public int _pServer;
    public int _pServerCtx; // CObjectContext* 
    public Guid _oid;
    public int _aptID;
    public int _pStdWrapper; // CStdWrapper* 
    public int _pStdID; // CStdIdentity* 
    public int _cCalls;
    public int _cLocks;
    public SHashChain32 _oidUnpinReqChain;
    public int _dwOidUnpinReqState;
    public int _pvObjectTrackCookie; // void* 

    IIDObject IIDObject.GetNextOid(NtProcess process, IntPtr head_ptr)
    {
        if (_oidChain.pNext == head_ptr.ToInt32())
            return null;
        return process.ReadStruct<CIDObject>(_oidChain.pNext);
    }

    Guid IIDObject.GetOid()
    {
        return _oid;
    }

    IStdWrapper IIDObject.GetStdWrapper(NtProcess process)
    {
        if (_pStdWrapper == 0)
            return null;
        return process.ReadStruct<CStdWrapper32>(_pStdWrapper);
    }

    IStdIdentity IIDObject.GetStdIdentity(NtProcess process)
    {
        if (_pStdID == 0)
        {
            return null;
        }

        if (AppUtilities.IsWindows10RS3OrLess)
        {
            return process.ReadStruct<CStdIdentity32>(_pStdID);
        }
        else
        {
            return process.ReadStruct<CStdIdentity32RS4>(_pStdID);
        }
    }
}
