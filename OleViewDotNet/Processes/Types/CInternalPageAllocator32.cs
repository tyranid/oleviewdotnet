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
using System.Linq;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Processes.Types;

[StructLayout(LayoutKind.Sequential)]
internal struct CInternalPageAllocator32 : IPageAllocator
{
    public int _cPages;
    public int _pPageListStart;
    public int _pPageListEnd;
    public int _dwFlags;
    public PageEntry32 _ListHead;
    public int _cEntries;
    public int _cbPerEntry;
    public ushort _cEntriesPerPage;
    public int _pLock;

    int IPageAllocator.Pages => _cPages;

    int IPageAllocator.EntrySize => _cbPerEntry;

    int IPageAllocator.EntriesPerPage => _cEntriesPerPage;
    IntPtr[] IPageAllocator.ReadPages(NtProcess process)
    {
        return process.ReadMemoryArray<int>(_pPageListStart, _cPages).Select(i => new IntPtr(i)).ToArray();
    }
};
