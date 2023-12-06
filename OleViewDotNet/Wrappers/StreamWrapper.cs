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
using OleViewDotNet.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Wrappers;

public sealed class StreamWrapper : Stream
{
    private readonly ComTypes.IStream _stm;

    public StreamWrapper(ComTypes.IStream stm)
    {
        _stm = stm;
    }

    private ComTypes.STATSTG Stat()
    {
        _stm.Stat(out ComTypes.STATSTG stat, 1);
        return stat;
    }

    protected override void Dispose(bool disposing)
    {
        Marshal.FinalReleaseComObject(_stm);
        base.Dispose(disposing);
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite
    {
        get
        {
            STGM mode = (STGM)Stat().grfMode;
            return (mode & STGM.WRITE) == STGM.WRITE || (mode & STGM.READWRITE) == STGM.READWRITE;
        }
    }

    public override long Length => Stat().cbSize;

    public override long Position
    {
        get
        {
            return Seek(0, SeekOrigin.Current);
        }

        set
        {
            // STREAM_SEEK_SET == 0
            _stm.Seek(value, 0, IntPtr.Zero);
        }
    }

    public override void Flush()
    {
        _stm.Commit(0);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (offset == 0)
        {
            using var len = new SafeStructureInOutBuffer<int>();
            _stm.Read(buffer, count, len.DangerousGetHandle());
            return len.Result;
        }
        else
        {
            using var len = new SafeStructureInOutBuffer<int>();
            byte[] temp_buffer = new byte[count];
            _stm.Read(temp_buffer, count, len.DangerousGetHandle());
            int read_len = len.Result;
            Buffer.BlockCopy(temp_buffer, 0, buffer, offset, count);
            return read_len;
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        using var buffer = new SafeStructureInOutBuffer<long>();
        _stm.Seek(0, (int)origin, buffer.DangerousGetHandle());
        return buffer.Result;
    }

    public override void SetLength(long value)
    {
        _stm.SetSize(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _stm.Write(buffer, count, IntPtr.Zero);
    }

    public IStreamWrapper Object => new IStreamWrapper(_stm);
}
