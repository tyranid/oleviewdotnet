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

using NtApiDotNet;
using OleViewDotNet.Database;
using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNetPS.Wrappers;

[Flags]
public enum StreamLockType
{
    None = 0,
    Write = 1,
    Exclusive = 2,
    OnlyOnce = 4
}

public class IStreamWrapper : BaseComWrapper<IStream>
{
    public IStreamWrapper(object obj, COMRegistry registry) : base(obj, registry)
    {
    }

    public byte[] Read(int cb)
    {
        byte[] ret = new byte[cb];
        using var buf = new SafeStructureInOutBuffer<int>();
        _object.Read(ret, cb, buf.DangerousGetHandle());
        Array.Resize(ref ret, buf.Result);
        return ret;
    }

    public int Write(byte[] pv)
    {
        using var buf = new SafeStructureInOutBuffer<int>();
        _object.Write(pv, pv.Length, buf.DangerousGetHandle());
        return buf.Result;
    }

    public long Seek(long dlibMove, SeekOrigin dwOrigin)
    {
        using var buf = new SafeStructureInOutBuffer<long>();
        _object.Seek(dlibMove, (int)dwOrigin, buf.DangerousGetHandle());
        return buf.Result;
    }

    public void SetSize(long libNewSize)
    {
        _object.SetSize(libNewSize);
    }

    public void CopyTo(IStreamWrapper pstm, long cb, out int pcbRead, out int pcbWritten)
    {
        using SafeStructureInOutBuffer<int> read_buf = new();
        using SafeStructureInOutBuffer<int> write_buf = new();

        _object.CopyTo(pstm.UnwrapTyped(), cb, read_buf.DangerousGetHandle(), write_buf.DangerousGetHandle());
        pcbRead = read_buf.Result;
        pcbWritten = write_buf.Result;
    }

    public void Commit(int grfCommitFlags)
    {
        _object.Commit(grfCommitFlags);
    }

    public void Revert()
    {
        _object.Revert();
    }

    public void LockRegion(long libOffset, long cb, StreamLockType dwLockType)
    {
        _object.LockRegion(libOffset, cb, (int)dwLockType);
    }

    public void UnlockRegion(long libOffset, long cb, StreamLockType dwLockType)
    {
        _object.UnlockRegion(libOffset, cb, (int)dwLockType);
    }

    public STATSTG Stat(int grfStatFlag)
    {
        _object.Stat(out STATSTG pstatstg, grfStatFlag);
        return pstatstg;
    }

    public IStreamWrapper Clone()
    {
        _object.Clone(out IStream stm);
        return new IStreamWrapper(stm, m_registry);
    }

    public Stream GetStream()
    {
        return new StreamWrapper(this);
    }
}
