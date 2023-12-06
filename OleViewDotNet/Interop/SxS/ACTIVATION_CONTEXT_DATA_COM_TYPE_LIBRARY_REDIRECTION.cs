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
internal struct ACTIVATION_CONTEXT_DATA_COM_TYPE_LIBRARY_REDIRECTION
{
    public int Size;
    public int Flags;
    public int NameLength; // in bytes
    public int NameOffset; // offset from section header
    public ushort ResourceId; // Resource ID of type library resource in PE
    public ushort LibraryFlags; // flags, as defined by the LIBFLAGS enumeration in oaidl.h
    public int HelpDirLength; // in bytes
    public int HelpDirOffset; // offset from ACTIVATION_CONTEXT_DATA_COM_TYPE_LIBRARY_REDIRECTION
    public ushort MajorVersion;
    public ushort MinorVersion;
}
