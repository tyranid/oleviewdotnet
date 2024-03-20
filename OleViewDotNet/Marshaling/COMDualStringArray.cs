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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OleViewDotNet.Marshaling;

internal class COMDualStringArray
{
    public List<COMStringBinding> StringBindings { get; private set; }
    public List<COMSecurityBinding> SecurityBindings { get; private set; }

    public COMDualStringArray()
    {
        StringBindings = new List<COMStringBinding>();
        SecurityBindings = new List<COMSecurityBinding>();
    }

    private void ReadEntries(BinaryReader new_reader, int sec_offset)
    {
        COMStringBinding str = new(new_reader);
        if (str.TowerId == RpcTowerId.StringBinding)
        {
            StringBindings.Add(str);
        }
        else
        {
            while (str.TowerId != 0)
            {
                StringBindings.Add(str);
                str = new COMStringBinding(new_reader);
            }
        }

        new_reader.BaseStream.Position = sec_offset * 2;
        COMSecurityBinding sec = new(new_reader);
        while (sec.AuthnSvc != 0)
        {
            SecurityBindings.Add(sec);
            sec = new COMSecurityBinding(new_reader);
        }
    }

    public COMDualStringArray(IntPtr ptr, NtProcess process) : this()
    {
        int num_entries = process.ReadMemory<ushort>(ptr.ToInt64());
        int sec_offset = process.ReadMemory<ushort>(ptr.ToInt64() + 2);
        if (num_entries > 0)
        {
            MemoryStream stm = new(process.ReadMemory(ptr.ToInt64() + 4, num_entries * 2));
            ReadEntries(new BinaryReader(stm), sec_offset);
        }
    }

    internal COMDualStringArray(BinaryReader reader) : this()
    {
        int num_entries = reader.ReadUInt16();
        int sec_offset = reader.ReadUInt16();

        if (num_entries > 0)
        {
            MemoryStream stm = new(reader.ReadAll(num_entries * 2));
            BinaryReader new_reader = new(stm);
            ReadEntries(new_reader, sec_offset);
        }
    }

    public void ToWriter(BinaryWriter writer)
    {
        MemoryStream stm = new();
        BinaryWriter new_writer = new(stm);
        if (StringBindings.Count > 0)
        {
            foreach (COMStringBinding str in StringBindings)
            {
                str.ToWriter(new_writer);
            }
            new COMStringBinding().ToWriter(new_writer);
        }
        ushort ofs = (ushort)(stm.Position / 2);
        if (SecurityBindings.Count > 0)
        {
            foreach (COMSecurityBinding sec in SecurityBindings)
            {
                sec.ToWriter(new_writer);
            }
            new COMSecurityBinding().ToWriter(new_writer);
        }
        writer.Write((ushort)(stm.Length / 2));
        writer.Write(ofs);
        writer.Write(stm.ToArray());
    }

    internal COMDualStringArray Clone()
    {
        COMDualStringArray ret = new();
        ret.StringBindings.AddRange(StringBindings.Select(b => b.Clone()));
        ret.SecurityBindings.AddRange(SecurityBindings.Select(b => b.Clone()));
        return ret;
    }
}
