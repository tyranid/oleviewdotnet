//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2024
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
using OleViewDotNet.Marshaling;
using System.Collections.Generic;

namespace OleViewDotNet.Rpc;

public sealed class ServerAliveResponse
{
    public COMVersion Version { get; }
    public IReadOnlyList<COMStringBinding> StringBindings { get; }
    public IReadOnlyList<COMSecurityBinding> SecurityBindings { get; }

    internal ServerAliveResponse(COMVERSION ver, COMDualStringArray dsa)
    {
        Version = new(ver.MajorVersion, ver.MinorVersion);
        StringBindings = (dsa?.StringBindings ?? new List<COMStringBinding>()).AsReadOnly();
        SecurityBindings = (dsa?.SecurityBindings ?? new List<COMSecurityBinding>()).AsReadOnly();
    }
}
