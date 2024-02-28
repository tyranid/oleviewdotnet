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
internal struct IFaceEntry : IIFaceEntry
{
    public IntPtr _pNext; // IFaceEntry*
    public IntPtr _pProxy; // void* 
    public IntPtr _pRpcProxy; // IRpcProxyBuffer* 
    public IntPtr _pRpcStub; // IRpcStubBuffer* 
    public IntPtr _pServer; // void* 
    public Guid _iid;
    public IntPtr _pCtxChnl; // CCtxChnl* 
    public IntPtr _pHead; // CtxEntry* 
    public IntPtr _pFreeList; // CtxEntry* 
    public IntPtr _pInterceptor; // ICallInterceptor*
    public IntPtr _pUnkInner; // IUnknown* 

    Guid IIFaceEntry.GetIid()
    {
        return _iid;
    }

    IIFaceEntry IIFaceEntry.GetNext(NtProcess process)
    {
        if (_pNext == IntPtr.Zero)
            return null;
        return process.ReadStruct<IFaceEntry>(_pNext.ToInt64());
    }

    IntPtr IIFaceEntry.GetProxy()
    {
        return _pProxy;
    }
}
