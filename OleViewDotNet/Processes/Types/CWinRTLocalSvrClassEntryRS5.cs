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
internal struct CWinRTLocalSvrClassEntryRS5 : IWinRTLocalSvrClassEntry
{
    [ChainOffset]
    public SActivatableClassIdHashNode _hashNode;
    public IntPtr _pRegChain; // CClassCache::CWinRTLocalSvrClassEntry* 
    public IntPtr _pActivationFactoryCallback;
    public int _dwFlags;
    public int _dwScmReg;
    public int _hApt;
    public int _dwSig;
    public int _cLocks;
    public IntPtr _pObjServer; // CObjServer* 
    public IntPtr _cookie;
    public bool _suspended;
    public int _ulServiceId;
    public IntPtr _activatableClassId;
    public IntPtr _packageFullName; // Microsoft::WRL::Wrappers::HString 

    string IWinRTLocalSvrClassEntry.GetActivatableClassId(NtProcess process)
    {
        if (_activatableClassId == IntPtr.Zero)
            return string.Empty;
        return process.ReadHString(_activatableClassId);
    }

    IntPtr IWinRTLocalSvrClassEntry.GetActivationFactoryCallback()
    {
        return _pActivationFactoryCallback;
    }

    Guid IWinRTLocalSvrClassEntry.GetClsid()
    {
        return Guid.Empty;
    }

    string IWinRTLocalSvrClassEntry.GetPackageFullName(NtProcess process)
    {
        if (_packageFullName == IntPtr.Zero)
            return string.Empty;
        return process.ReadHString(_packageFullName);
    }
}
