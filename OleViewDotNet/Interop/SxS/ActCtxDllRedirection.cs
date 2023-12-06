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

using System.Linq;

namespace OleViewDotNet.Interop.SxS;

public class ActCtxDllRedirection
{
    public string Name { get; }
    public string Path { get; }
    public ACTIVATION_CONTEXT_DATA_DLL_REDIRECTION_PATH_FLAGS Flags { get; }
    internal ActCtxDllRedirection(StringSectionEntry<ACTIVATION_CONTEXT_DATA_DLL_REDIRECTION> entry, ReadHandle handle, int base_offset)
    {
        Name = entry.Key;
        Path = string.Join(@"\",
            handle.ReadArray<ACTIVATION_CONTEXT_DATA_DLL_REDIRECTION_PATH_SEGMENT>(base_offset + entry.Entry.PathSegmentOffset,
            entry.Entry.PathSegmentCount).Select(e => handle.ReadString(base_offset + e.Offset, e.Length)));
        Flags = entry.Entry.Flags;
    }
}
