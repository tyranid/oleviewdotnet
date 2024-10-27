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
using System;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNetPS.Wrappers;

public class IStreamWrapper : BaseComWrapper<IStream>
{
    public IStreamWrapper(object obj, COMRegistry registry) : base(obj, registry)
    {
    }

    public void Read(byte[] pv, int cb, IntPtr pcbRead)
    {
        _object.Read(pv, cb, pcbRead);
    }

    public void Write(byte[] pv, int cb, IntPtr pcbWritten)
    {
        _object.Write(pv, cb, pcbWritten);
    }

    public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
    {
        _object.Seek(dlibMove, dwOrigin, plibNewPosition);
    }

    public void SetSize(long libNewSize)
    {
        _object.SetSize(libNewSize);
    }

    public void CopyTo(IStreamWrapper pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
    {
        _object.CopyTo(pstm.UnwrapTyped(), cb, pcbRead, pcbWritten);
    }

    public void Commit(int grfCommitFlags)
    {
        _object.Commit(grfCommitFlags);
    }

    public void Revert()
    {
        _object.Revert();
    }

    public void LockRegion(long libOffset, long cb, int dwLockType)
    {
        _object.LockRegion(libOffset, cb, dwLockType);
    }

    public void UnlockRegion(long libOffset, long cb, int dwLockType)
    {
        _object.UnlockRegion(libOffset, cb, dwLockType);
    }

    public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
    {
        Stat(out pstatstg, grfStatFlag);
    }

    public IStreamWrapper Clone()
    {
        _object.Clone(out IStream stm);
        return new IStreamWrapper(stm, m_registry);
    }
}
