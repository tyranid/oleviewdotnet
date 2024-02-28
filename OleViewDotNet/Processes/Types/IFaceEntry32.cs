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
internal struct IFaceEntry32 : IIFaceEntry
{
    public int _pNext; // IFaceEntry*
    public int _pProxy; // void* 
    public int _pRpcProxy; // IRpcProxyBuffer* 
    public int _pRpcStub; // IRpcStubBuffer* 
    public int _pServer; // void* 
    public Guid _iid;
    public int _pCtxChnl; // CCtxChnl* 
    public int _pHead; // CtxEntry* 
    public int _pFreeList; // CtxEntry* 
    public int _pInterceptor; // ICallInterceptor*
    public int _pUnkInner; // IUnknown* 

    Guid IIFaceEntry.GetIid()
    {
        return _iid;
    }

    IIFaceEntry IIFaceEntry.GetNext(NtProcess process)
    {
        if (_pNext == 0)
            return null;
        return process.ReadStruct<IFaceEntry32>(_pNext);
    }

    IntPtr IIFaceEntry.GetProxy()
    {
        return new IntPtr(_pProxy);
    }
}
