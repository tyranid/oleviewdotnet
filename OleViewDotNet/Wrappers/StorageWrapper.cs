//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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

using OleViewDotNet.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Wrappers;

/// <summary>
/// A wrapper object for an IStorage.
/// </summary>
public sealed class StorageWrapper : IDisposable
{
    private readonly IStorage _stg;

    public StorageWrapper(IStorage stg)
    {
        _stg = stg;
    }

    public StorageWrapper OpenReadOnlyStorage(string name)
    {
        return OpenStorage(name, STGM.SHARE_EXCLUSIVE | STGM.READ);
    }

    public StorageWrapper OpenStorage(string name, STGM mode)
    {
        return new StorageWrapper(_stg.OpenStorage(name, IntPtr.Zero,
            mode, IntPtr.Zero, 0));
    }

    public StorageWrapper CreateStorage(string name, STGM mode)
    {
        return new StorageWrapper(_stg.CreateStorage(name, mode, 0, 0));
    }

    public StorageWrapper CreateStore(string name)
    {
        return CreateStorage(name, STGM.SHARE_EXCLUSIVE | STGM.READWRITE);
    }

    public StreamWrapper OpenStream(string name, STGM mode)
    {
        return new StreamWrapper(_stg.OpenStream(name, IntPtr.Zero, mode, 0));
    }

    public StreamWrapper OpenReadOnlyStream(string name)
    {
        return OpenStream(name, STGM.SHARE_EXCLUSIVE | STGM.READ);
    }

    public StreamWrapper OpenReadWriteStream(string name)
    {
        return OpenStream(name, STGM.SHARE_EXCLUSIVE | STGM.READWRITE);
    }

    public StreamWrapper CreateStream(string name, STGM mode)
    {
        return new StreamWrapper(_stg.CreateStream(name, mode, 0, 0));
    }

    public StreamWrapper CreateStream(string name)
    {
        return CreateStream(name, STGM.SHARE_EXCLUSIVE | STGM.READWRITE);
    }

    public IEnumerable<STATSTGWrapper> EnumElements()
    {
        return EnumElements(false);
    }

    public byte[] ReadStream(string name)
    {
        using var stm = OpenStream(name, STGM.READ | STGM.SHARE_EXCLUSIVE);
        long length = stm.Length;
        byte[] ret = new byte[stm.Length];
        stm.Read(ret, 0, ret.Length);
        return ret;
    }

    public IEnumerable<STATSTGWrapper> EnumElements(bool read_stream_data)
    {
        List<STATSTGWrapper> ret = new();
        _stg.EnumElements(0, IntPtr.Zero, 0, out IEnumSTATSTG enum_stg);
        try
        {
            ComTypes.STATSTG[] stat = new ComTypes.STATSTG[1];
            while (enum_stg.Next(1, stat, out uint fetched) == 0)
            {
                STGTY type = (STGTY)stat[0].type;
                byte[] bytes = new byte[0];
                if (read_stream_data && type == STGTY.Stream)
                {
                    bytes = ReadStream(stat[0].pwcsName);
                }
                ret.Add(new STATSTGWrapper(stat[0].pwcsName, stat[0], bytes));
            }
        }
        finally
        {
            Marshal.ReleaseComObject(enum_stg);
        }
        return ret;
    }

    public void Close()
    {
        Dispose();
    }

    public void Dispose()
    {
        Marshal.FinalReleaseComObject(_stg);
    }

    public STATSTGWrapper Stat
    {
        get
        {
            ComTypes.STATSTG stg_stat = new();
            _stg.Stat(out stg_stat, 0);
            return new STATSTGWrapper(stg_stat.pwcsName, stg_stat, new byte[0]);
        }
    }

    public Guid Clsid
    {
        get
        {
            return Stat.Clsid;
        }
        set
        {
            _stg.SetClass(value);
        }
    }

    public void RenameElement(string old_name, string new_name)
    {
        _stg.RenameElement(old_name, new_name);
    }

    public void DestroyElement(string name)
    {
        _stg.DestroyElement(name);
    }

    public void Commit(STGC stgc)
    {
        _stg.Commit((int)stgc);
    }

    public void Revert()
    {
        _stg.Revert();
    }

    private static FILETIMEOptional DateTimeToFileTime(DateTime? dt)
    {
        return dt.HasValue ? new FILETIMEOptional(dt.Value) : null;
    }

    public void SetElementTimes(
        string name,
        DateTime? ctime,
        DateTime? atime,
        DateTime? mtime)
    {
        _stg.SetElementTimes(string.IsNullOrEmpty(name) ? null : name,
            DateTimeToFileTime(ctime), DateTimeToFileTime(atime), DateTimeToFileTime(mtime));
    }

    public BaseComWrapper<IStorage> Object => COMWrapperFactory.Wrap<IStorage>(_stg);
}
