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
internal struct CStdWrapper : IStdWrapper
{
    public IntPtr VTablePtr;
    public uint _dwState;
    public int _cRefs;
    public int _cCalls;
    public int _cIFaces;
    public IntPtr _pIFaceHead; // IFaceEntry* 
    public IntPtr _pCtxEntryHead; // CtxEntry* 
    public IntPtr _pCtxFreeList; // CtxEntry* 
    public IntPtr _pServer; // IUnknown* 
    public IntPtr _pID; // CIDObject* 
    public IntPtr _pVtableAddress; // void* 

    int IStdWrapper.GetIFaceCount()
    {
        return _cIFaces;
    }

    IIFaceEntry IStdWrapper.GetIFaceHead(NtProcess process)
    {
        if (_pIFaceHead == IntPtr.Zero)
            return null;
        return process.ReadStruct<IFaceEntry>(_pIFaceHead.ToInt64());
    }

    IntPtr IStdWrapper.GetVtableAddress()
    {
        return _pVtableAddress;
    }
}
