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
using OleViewDotNet.Interop;
using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Processes.Types;

[StructLayout(LayoutKind.Sequential)]
internal struct CLSvrClassEntry32 : ICLSvrClassEntry
{
    public int vfptr; // CClassCache::CBaseClassEntryVtbl*
    public int _pNext; // CClassCache::CBaseClassEntry*
    public int _pPrev; // CClassCache::CBaseClassEntry* 
    public int _pClassEntry; // CClassCache::CClassEntry* 
    public CLSCTX _dwContext;
    public int _dwSig;
    public int _pNextLSvr; // CClassCache::CLSvrClassEntry* 
    public int _pPrevLSvr; // CClassCache::CLSvrClassEntry*
    public int _pUnk; // IUnknown* 
    public REGCLS _dwRegFlags;
    public uint _dwFlags;
    public uint _dwScmReg;
    public uint _hApt;
    public int _hWndDdeServer;
    public int _pObjServer; // CObjServer*
    public uint _dwCookie;
    public uint _cUsing;
    public uint _ulServiceId;

    ICClassEntry ICLSvrClassEntry.GetClassEntry(NtProcess process)
    {
        if (_pClassEntry == 0)
        {
            return null;
        }
        return process.ReadStruct<CClassEntry32>(_pClassEntry);
    }

    IntPtr ICLSvrClassEntry.GetIUnknown()
    {
        return new IntPtr(_pUnk);
    }

    IntPtr ICLSvrClassEntry.GetNext()
    {
        return new IntPtr(_pNextLSvr);
    }
    uint ICLSvrClassEntry.GetCookie()
    {
        return _dwCookie;
    }

    REGCLS ICLSvrClassEntry.GetRegFlags()
    {
        return _dwRegFlags;
    }

    CLSCTX ICLSvrClassEntry.GetContext()
    {
        return _dwContext;
    }
}
