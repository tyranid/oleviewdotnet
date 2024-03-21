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

using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Utilities;
using System;

namespace OleViewDotNet.Processes;

public class COMProcessClassRegistration
{
    public string Name => ClassEntry?.Name ?? string.Empty;
    public Guid Clsid { get; private set; }
    public COMCLSIDEntry ClassEntry { get; private set; }
    public IntPtr ClassFactory { get; private set; }
    public string VTable { get; private set; }
    public COMProcessClassApartment Apartment { get; private set; }
    public REGCLS RegFlags { get; private set; }
    public uint Cookie { get; private set; }
    public int ThreadId { get; private set; }
    public CLSCTX Context { get; private set; }
    public int ProcessID => Process.ProcessId;
    public string ProcessName => Process.Name;
    public bool Registered => ClassEntry is not null;
    public bool LocalServer => Context.HasFlag(CLSCTX.LOCAL_SERVER);

    public COMProcessEntry Process { get; internal set; }

    internal COMProcessClassRegistration(
        Guid clsid, IntPtr class_factory, string vtable,
        REGCLS regflags, uint cookie, int thread_id,
        CLSCTX context, COMProcessClassApartment apartment,
        COMRegistry registry)
    {
        Clsid = clsid;
        ClassFactory = class_factory;
        VTable = vtable;
        Apartment = apartment;
        RegFlags = regflags;
        Cookie = cookie;
        ThreadId = thread_id;
        Context = context;
        ClassEntry = registry?.Clsids.GetGuidEntry(Clsid);
    }

    public override string ToString()
    {
        return $"Class: {Clsid}";
    }
}
