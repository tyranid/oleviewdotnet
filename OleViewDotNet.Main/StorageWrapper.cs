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
using OleViewDotNet.Wrappers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet
{
    public sealed class STATSTGWrapper
    {
        private static DateTime FromFileTime(ComTypes.FILETIME filetime)
        {
            long result = (uint)filetime.dwLowDateTime | (((long)filetime.dwHighDateTime) << 32);
            return DateTime.FromFileTime(result);
        }

        internal STATSTGWrapper(string name, ComTypes.STATSTG stat, byte[] bytes)
        {
            Name = name;
            Type = (STGTY)stat.type;
            Size = stat.cbSize;
            ModifiedTime = FromFileTime(stat.mtime);
            CreationTime = FromFileTime(stat.ctime);
            AccessTime = FromFileTime(stat.atime);
            Mode = (STGM)stat.grfMode;
            LocksSupported = stat.grfLocksSupported;
            Clsid = stat.clsid;
            StateBits = stat.grfStateBits;
            Bytes = bytes;
        }

        public string Name { get; private set; }
        public STGTY Type { get; private set; }
        public long Size { get; private set; }
        public DateTime ModifiedTime { get; private set; }
        public DateTime CreationTime { get; private set; }
        public DateTime AccessTime { get; private set; }
        public STGM Mode { get; private set; }
        public int LocksSupported { get; private set; }
        public Guid Clsid { get; private set; }
        public int StateBits { get; private set; }
        [Browsable(false)]
        public byte[] Bytes { get; private set; }
    }

    public sealed class StreamWrapper : Stream
    {
        private ComTypes.IStream _stm;

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
                return ((mode & STGM.WRITE) == STGM.WRITE) || ((mode & STGM.READWRITE) == STGM.READWRITE);
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
                using (var len = new SafeStructureInOutBuffer<int>())
                {
                    _stm.Read(buffer, count, len.DangerousGetHandle());
                    return len.Result;
                }
            }
            else
            {
                using (var len = new SafeStructureInOutBuffer<int>())
                {
                    byte[] temp_buffer = new byte[count];
                    _stm.Read(temp_buffer, count, len.DangerousGetHandle());
                    int read_len = len.Result;
                    Buffer.BlockCopy(temp_buffer, 0, buffer, offset, count);
                    return read_len;
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            using (var buffer = new SafeStructureInOutBuffer<long>())
            {
                _stm.Seek(0, (int)origin, buffer.DangerousGetHandle());
                return buffer.Result;
            }
        }

        public override void SetLength(long value)
        {
            _stm.SetSize(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stm.Write(buffer, count, IntPtr.Zero);
        }

        public IStreamWrapper Object { get { return new IStreamWrapper(_stm); } }
    }

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
            using (var stm = OpenStream(name, STGM.READ | STGM.SHARE_EXCLUSIVE))
            {
                long length = stm.Length;
                byte[] ret = new byte[stm.Length];
                stm.Read(ret, 0, ret.Length);
                return ret;
            }
        }

        public IEnumerable<STATSTGWrapper> EnumElements(bool read_stream_data)
        {
            List<STATSTGWrapper> ret = new List<STATSTGWrapper>();
            _stg.EnumElements(0, IntPtr.Zero, 0, out IEnumSTATSTG enum_stg);
            try
            {
                ComTypes.STATSTG[] stat = new ComTypes.STATSTG[1];
                uint fetched;
                while (enum_stg.Next(1, stat, out fetched) == 0)
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
                ComTypes.STATSTG stg_stat = new ComTypes.STATSTG();
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
                _stg.SetClass(ref value);
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

        public BaseComWrapper<IStorage> Object { get { return COMWrapperFactory.Wrap<IStorage>(_stg); } }
    }
}
