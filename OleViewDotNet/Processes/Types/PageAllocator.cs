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

namespace OleViewDotNet.Processes.Types;

internal class PageAllocator
{
    public IntPtr[] Pages { get; private set; }
    public int EntrySize { get; private set; }
    public int EntriesPerPage { get; private set; }

    private void Init<T>(NtProcess process, IntPtr ipid_table) where T : IPageAllocator, new()
    {
        IPageAllocator page_alloc = process.ReadStruct<T>(ipid_table.ToInt64());
        Pages = page_alloc.ReadPages(process);
        EntrySize = page_alloc.EntrySize;
        EntriesPerPage = page_alloc.EntriesPerPage;
    }

    public PageAllocator(NtProcess process, IntPtr ipid_table)
    {
        if (process.Is64Bit)
        {
            Init<CInternalPageAllocator>(process, ipid_table);
        }
        else
        {
            Init<CInternalPageAllocator32>(process, ipid_table);
        }
    }
}
