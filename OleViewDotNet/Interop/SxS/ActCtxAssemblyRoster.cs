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

public class ActCtxAssemblyRoster
{
    public string AssemblyName { get; }
    public string AssemblyDirectoryName { get; }
    public string FullPath { get; }

    private static readonly string SXS_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "WinSxS");

    internal ActCtxAssemblyRoster(ACTIVATION_CONTEXT_DATA_ASSEMBLY_ROSTER_ENTRY entry, ReadHandle handle, int base_offset)
    {
        AssemblyName = string.Empty;
        AssemblyDirectoryName = string.Empty;
        FullPath = string.Empty;

        if ((entry.Flags & ACTIVATION_CONTEXT_DATA_ASSEMBLY_ROSTER_ENTRY_FLAGS.INVALID) != 0)
        {
            return;
        }

        AssemblyName = handle.ReadString(entry.AssemblyNameOffset, entry.AssemblyNameLength);
        if (entry.AssemblyInformationOffset == 0)
        {
            return;
        }

        var info = handle.ReadStructure<ACTIVATION_CONTEXT_DATA_ASSEMBLY_INFORMATION>(entry.AssemblyInformationOffset);
        AssemblyDirectoryName = handle.ReadString(base_offset + info.AssemblyDirectoryNameOffset, info.AssemblyDirectoryNameLength);
        FullPath = Path.Combine(SXS_FOLDER, AssemblyDirectoryName);
    }
}
