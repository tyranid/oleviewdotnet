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

using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNetPS.Wrappers;

/// <summary>
/// A wrapper object for an IStorage.
/// </summary>
public sealed class IStorageWrapper : BaseComWrapper<IStorage>
{
    public IStorageWrapper(object obj, COMRegistry registry) 
        : base(obj, typeof(IStorage).GUID, "IStorage", registry)
    {
    }

    public IStorageWrapper OpenReadOnlyStorage(string name)
    {
        return OpenStorage(name, STGM.SHARE_EXCLUSIVE | STGM.READ);
    }

    public IStorageWrapper OpenStorage(string name, STGM mode)
    {
        return new IStorageWrapper(_object.OpenStorage(name, IntPtr.Zero,
            mode, IntPtr.Zero, 0), m_registry);
    }

    public IStorageWrapper CreateStorage(string name, STGM mode)
    {
        return new IStorageWrapper(_object.CreateStorage(name, mode, 0, 0), m_registry);
    }

    public IStorageWrapper CreateStore(string name)
    {
        return CreateStorage(name, STGM.SHARE_EXCLUSIVE | STGM.READWRITE);
    }

    public IStreamWrapper OpenStream(string name, STGM mode)
    {
        return new IStreamWrapper(_object.OpenStream(name, IntPtr.Zero, mode, 0), m_registry);
    }

    public IStreamWrapper OpenReadOnlyStream(string name)
    {
        return OpenStream(name, STGM.SHARE_EXCLUSIVE | STGM.READ);
    }

    public IStreamWrapper OpenReadWriteStream(string name)
    {
        return OpenStream(name, STGM.SHARE_EXCLUSIVE | STGM.READWRITE);
    }

    public IStreamWrapper CreateStream(string name, STGM mode)
    {
        return new IStreamWrapper(_object.CreateStream(name, mode, 0, 0), m_registry);
    }

    public IStreamWrapper CreateStream(string name)
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
        _object.EnumElements(0, IntPtr.Zero, 0, out IEnumSTATSTG enum_object);
        try
        {
            ComTypes.STATSTG[] stat = new ComTypes.STATSTG[1];
            while (enum_object.Next(1, stat, out uint fetched) == 0)
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
            Marshal.ReleaseComObject(enum_object);
        }
        return ret;
    }

    public STATSTGWrapper Stat
    {
        get
        {
            _object.Stat(out ComTypes.STATSTG stg_stat, 0);
            return new STATSTGWrapper(stg_stat.pwcsName, stg_stat, new byte[0]);
        }
    }

    public Guid Clsid
    {
        get => Stat.Clsid;
        set => _object.SetClass(value);
    }

    public void RenameElement(string old_name, string new_name)
    {
        _object.RenameElement(old_name, new_name);
    }

    public void DestroyElement(string name)
    {
        _object.DestroyElement(name);
    }

    public void Commit(STGC stgc)
    {
        _object.Commit((int)stgc);
    }

    public void Revert()
    {
        _object.Revert();
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
        _object.SetElementTimes(string.IsNullOrEmpty(name) ? null : name,
            DateTimeToFileTime(ctime), DateTimeToFileTime(atime), DateTimeToFileTime(mtime));
    }
}
