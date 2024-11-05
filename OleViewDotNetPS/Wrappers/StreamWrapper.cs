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
using System.IO;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNetPS.Wrappers;

internal sealed class StreamWrapper : Stream
{
    private readonly IStreamWrapper _stm;

    public StreamWrapper(IStreamWrapper stm)
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
        get => _stm.Seek(0, SeekOrigin.Current);
        set => _stm.Seek(value, SeekOrigin.Begin);
    }

    public override void Flush()
    {
        _stm.Commit(0);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        byte[] buf = _stm.Read(count);
        Buffer.BlockCopy(buf, 0, buffer, offset, buf.Length);
        return buf.Length;
    }

    public override void SetLength(long value)
    {
        _stm.SetSize(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        byte[] buf = new byte[count];
        Buffer.BlockCopy(buffer, offset, buf, 0, count);
        _stm.Write(buf);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _stm.Seek(offset, origin);
    }
}
