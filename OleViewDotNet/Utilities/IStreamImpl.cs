//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
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

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Utilities;

internal class IStreamImpl : IStream, IDisposable
{
    private readonly Stream m_stream;

    public IStreamImpl(Stream stream)
    {
        m_stream = stream;
    }

    public IStreamImpl(string strFileName, FileMode mode, FileAccess access, FileShare share)
    {
        m_stream = File.Open(strFileName, mode, access, share);
    }

    public void Dispose()
    {
        m_stream.Dispose();
    }

    public void Close()
    {
        Dispose();
    }

    public void Clone(out IStream pStm)
    {
        throw new NotImplementedException();
    }

    public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG statStg, int grfFlags)
    {
        statStg = new System.Runtime.InteropServices.ComTypes.STATSTG
        {
            cbSize = m_stream.Length
        };
    }

    public void UnlockRegion(long libOffset, long cb, int dwLockType)
    {
    }

    public void LockRegion(long libOffset, long cb, int dwLockType)
    {
    }

    public void Revert()
    {
    }

    public void Commit(int grfCommitFlags)
    {
    }

    public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
    {
        throw new NotImplementedException();
    }

    public void SetSize(long lSize)
    {
        m_stream.SetLength(lSize);
    }

    public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
    {
        var origin = dwOrigin switch
        {
            0 => SeekOrigin.Begin,
            1 => SeekOrigin.Current,
            2 => SeekOrigin.End,
            _ => throw new ArgumentException(),
        };
        m_stream.Seek(dlibMove, origin);
        if (plibNewPosition != IntPtr.Zero)
        {
            Marshal.WriteInt64(plibNewPosition, m_stream.Position);
        }
    }

    public void Read(byte[] pv, int cb, IntPtr pcbRead)
    {
        if (pv is null)
            return;
        int readCount = m_stream.Read(pv, 0, cb);
        if (pcbRead != IntPtr.Zero)
        {
            Marshal.WriteInt32(pcbRead, readCount);
        }
    }

    public void Write(byte[] pv, int cb, IntPtr pcbWritten)
    {
        if (pv is null)
            return;
        m_stream.Write(pv, 0, cb);
        if (pcbWritten != IntPtr.Zero)
        {
            Marshal.WriteInt32(pcbWritten, cb);
        }
    }
}
