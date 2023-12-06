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
using System.Runtime.InteropServices;

namespace OleViewDotNet.Interop.SxS;

[StructLayout(LayoutKind.Sequential)]
internal struct ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION
{
    public int Size;
    public ACTIVATION_CONTEXT_DATA_COM_SERVER_FLAGS Flags;
    public ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION_THREADING_MODEL ThreadingModel;
    public Guid ReferenceClsid;
    public Guid ConfiguredClsid;
    public Guid ImplementedClsid;
    public Guid TypeLibraryId;
    public int ModuleLength; // in bytes
    public int ModuleOffset; // offset from section base because this can be shared across multiple entries
    public int ProgIdLength; // in bytes
    public int ProgIdOffset; // offset from ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION because this is never shared
    public int ShimDataLength; // in bytes
    public int ShimDataOffset; // offset from ACTIVATION_CONTEXT_DATA_COM_SERVER_REDIRECTION because this is not shared
    public int MiscStatusDefault;
    public int MiscStatusContent;
    public int MiscStatusThumbnail;
    public int MiscStatusIcon;
    public int MiscStatusDocPrint;
}
