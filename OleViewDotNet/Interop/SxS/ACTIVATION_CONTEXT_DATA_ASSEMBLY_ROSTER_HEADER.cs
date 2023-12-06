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
internal struct ACTIVATION_CONTEXT_DATA_ASSEMBLY_ROSTER_HEADER
{
    public int HeaderSize;
    public uint HashAlgorithm;
    public int EntryCount;               // Entry 0 is reserved; this is the number of assemblies plus 1.
    public int FirstEntryOffset;         // From ACTIVATION_CONTEXT_DATA base
    public int AssemblyInformationSectionOffset; // Offset from the ACTIVATION_CONTEXT_DATA base to the
                                                 // header of the assembly information string section.  Needed because
                                                 // the roster entries contain the offsets from the ACTIVATION_CONTEXT_DATA
                                                 // to the assembly information structs, but those structs contain offsets
                                                 // from their section base to the strings etc.
}
