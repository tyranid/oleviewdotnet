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
using System.IO;

namespace OleViewDotNet.Interop.SxS;

public class ActCtxComTypeLibraryRedirection
{
    public Guid TypeLibraryId { get; }
    public string Name { get; }
    public string HelpDir { get; }
    public Version Version { get; }
    public int ResourceId { get; }
    public System.Runtime.InteropServices.ComTypes.LIBFLAGS LibraryFlags { get; }
    public string FullPath { get; }

    internal ActCtxComTypeLibraryRedirection(GuidSectionEntry<ACTIVATION_CONTEXT_DATA_COM_TYPE_LIBRARY_REDIRECTION> entry, ReadHandle handle, int base_offset)
    {
        TypeLibraryId = entry.Key;
        var ent = entry.Entry;
        Name = handle.ReadString(base_offset + ent.NameOffset, ent.NameLength);
        HelpDir = handle.ReadString(entry.Offset + ent.HelpDirOffset, ent.HelpDirLength);
        LibraryFlags = (System.Runtime.InteropServices.ComTypes.LIBFLAGS)ent.LibraryFlags;
        ResourceId = ent.ResourceId;
        Version = new Version(ent.MajorVersion, ent.MinorVersion);
        if (!string.IsNullOrWhiteSpace(entry.RosterEntry.FullPath))
        {
            FullPath = Path.Combine(entry.RosterEntry.FullPath, Name);
        }
        else
        {
            FullPath = Name;
        }
    }
}
