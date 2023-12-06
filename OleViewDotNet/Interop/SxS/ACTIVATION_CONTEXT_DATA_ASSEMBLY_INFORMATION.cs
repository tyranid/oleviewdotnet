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

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct ACTIVATION_CONTEXT_DATA_ASSEMBLY_INFORMATION
{
    public int Size;                                 // size of this structure, in bytes
    public ACTIVATION_CONTEXT_DATA_ASSEMBLY_INFORMATION_FLAGS Flags;
    public int EncodedAssemblyIdentityLength;        // in bytes
    public int EncodedAssemblyIdentityOffset;        // offset from section header base

    public int ManifestPathType;
    public int ManifestPathLength;                   // in bytes
    public int ManifestPathOffset;                   // offset from section header base
    public long ManifestLastWriteTime;
    public int PolicyPathType;
    public int PolicyPathLength;                     // in bytes
    public int PolicyPathOffset;                     // offset from section header base
    public long PolicyLastWriteTime;
    public int MetadataSatelliteRosterIndex;
    public int Unused2;
    public int ManifestVersionMajor;
    public int ManifestVersionMinor;
    public int PolicyVersionMajor;
    public int PolicyVersionMinor;
    public int AssemblyDirectoryNameLength; // in bytes
    public int AssemblyDirectoryNameOffset; // from section header base
    public int NumOfFilesInAssembly;
    public int LanguageLength; // in bytes
    public int LanguageOffset; // from section header base
    private readonly ACTCTX_REQUESTED_RUN_LEVEL RunLevel;
    public int UiAccess;
}
