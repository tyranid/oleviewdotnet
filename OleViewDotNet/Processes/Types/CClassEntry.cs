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

using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Processes.Types;

[StructLayout(LayoutKind.Sequential)]
internal struct CClassEntry : ICClassEntry
{
    public IntPtr vfptr; // CClassCache::CCollectableVtbl* 
    public IntPtr _pNextCollectee; // CClassCache::CCollectable* 
    public ulong _qwTickLastTouched;

    // SMultiGUIDHashNode _hashNode;
    public IntPtr pNext; // SHashChain* 
    public IntPtr pPrev; // SHashChain* 
    public int cGUID;
    public IntPtr aGUID; // _GUID* 
                         // END SMultiGUIDHashNode _hashNode;
    public Guid guids1;
    public Guid guids2;
    public uint _dwSig;
    public uint _dwFlags;
    public IntPtr _pTreatAsList; // CClassCache::CClassEntry* 
    public IntPtr _pBCEListFront; // CClassCache::CBaseClassEntry* 
    public IntPtr _pBCEListBack; // CClassCache::CBaseClassEntry* 
    public uint _cLocks;
    public uint _dwFailedContexts;
    public IntPtr _pCI; // IComClassInfo* 

    Guid[] ICClassEntry.GetGuids()
    {
        Guid[] ret = new Guid[2];
        ret[0] = guids1;
        ret[1] = guids2;
        return ret;
    }
}
