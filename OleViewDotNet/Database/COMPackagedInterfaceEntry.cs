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

using Microsoft.Win32;
using OleViewDotNet.Utilities;
using System;

namespace OleViewDotNet.Database;

internal class COMPackagedInterfaceEntry
{
    public Guid Iid { get; }
    public Guid ProxyStubCLSID { get; }
    public bool UseUniversalMarshaler { get; }
    public Guid SynchronousInterface { get; }
    public Guid AsynchronousInterface { get; }
    public Guid TypeLibId { get; }
    public string TypeLibVersionNumber { get; }

    internal COMPackagedInterfaceEntry(Guid iid, RegistryKey rootKey)
    {
        Iid = iid;
        ProxyStubCLSID = rootKey.ReadGuid(null, "ProxyStubCLSID");
        UseUniversalMarshaler = rootKey.ReadBool(valueName: "UseUniversalMarshaler");
        SynchronousInterface = rootKey.ReadGuid(null, "SynchronousInterface");
        AsynchronousInterface = rootKey.ReadGuid(null, "AsynchronousInterface");
        TypeLibId = rootKey.ReadGuid(null, "TypeLibId");
        TypeLibVersionNumber = rootKey.ReadString(valueName: "TypeLibVersionNumber");
    }
}
