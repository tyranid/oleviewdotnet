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

using System.Runtime.InteropServices;

namespace OleViewDotNet.Interop.SxS;

[StructLayout(LayoutKind.Sequential)]
internal struct ACTIVATION_CONTEXT_GUID_SECTION_HEADER
{
    public uint Magic;
    public int HeaderSize;               // in bytes
    public int FormatVersion;
    public int DataFormatVersion;
    public ACTIVATION_CONTEXT_GUID_SECTION_FLAGS Flags;
    public int ElementCount;
    public int ElementListOffset;        // offset from section header
    public int SearchStructureOffset;    // offset from section header
    public int UserDataOffset;           // offset from section header
    public int UserDataSize;             // in bytes
}
