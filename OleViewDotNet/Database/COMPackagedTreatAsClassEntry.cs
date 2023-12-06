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

internal class COMPackagedTreatAsClassEntry
{
    // TreatAsClass\{Clsid}
    public string AutoConvertTo { get; }
    public string DisplayName { get; }
    public Guid TreatAs { get; }

    internal COMPackagedTreatAsClassEntry(RegistryKey rootKey)
    {
        AutoConvertTo = rootKey.ReadString(valueName: "AutoConvertTo");
        DisplayName = rootKey.ReadString(valueName: "DisplayName");
        TreatAs = rootKey.ReadGuid(null, "TreatAs");
    }
}
