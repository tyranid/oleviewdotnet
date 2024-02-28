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
internal struct CStdWrapper32 : IStdWrapper
{
    public int VTablePtr;
    public uint _dwState;
    public int _cRefs;
    public int _cCalls;
    public int _cIFaces;
    public int _pIFaceHead; // IFaceEntry* 
    public int _pCtxEntryHead; // CtxEntry* 
    public int _pCtxFreeList; // CtxEntry* 
    public int _pServer; // IUnknown* 
    public int _pID; // CIDObject* 
    public int _pVtableAddress; // void* 

    int IStdWrapper.GetIFaceCount()
    {
        return _cIFaces;
    }

    IIFaceEntry IStdWrapper.GetIFaceHead(NtProcess process)
    {
        if (_pIFaceHead == 0)
            return null;
        return process.ReadStruct<IFaceEntry32>(_pIFaceHead);
    }

    IntPtr IStdWrapper.GetVtableAddress()
    {
        return new IntPtr(_pVtableAddress);
    }
}
