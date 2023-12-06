//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2019
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

namespace OleViewDotNet.Interop.SxS;

public class ActCtxComInterfaceRedirection
{
    public Guid Iid { get; }
    public Guid ProxyStubClsid32 { get; }
    public int NumMethods { get; }
    public Guid TypeLibraryId { get; }
    public Guid BaseInterface { get; }
    public string Name { get; }

    internal ActCtxComInterfaceRedirection(GuidSectionEntry<ACTIVATION_CONTEXT_DATA_COM_INTERFACE_REDIRECTION> entry, ReadHandle handle, int base_offset)
    {
        Iid = entry.Key;
        var ent = entry.Entry;
        ProxyStubClsid32 = ent.ProxyStubClsid32;
        NumMethods = ent.NumMethods;
        TypeLibraryId = ent.TypeLibraryId;
        BaseInterface = ent.BaseInterface;
        Name = handle.ReadString(entry.Offset + ent.NameOffset, ent.NameLength);
    }
}
