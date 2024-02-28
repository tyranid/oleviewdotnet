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
using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet.Database;

internal class COMPackagedClassEntry
{
    // Class\{Clsid}
    public Guid Clsid { get; }
    public string AutoConvertTo { get; }
    public string ConversionReadable { get; }
    public string ConversionReadWritable { get; }
    public string DataFormats { get; }
    public string DefaultFormatName { get; }
    public string DefaultIcon { get; }
    public string DisplayName { get; }
    public string DllPath { get; }
    public bool EnableOleDefaultHandler { get; }
    public List<Guid> ImplementedCategories { get; }
    public bool InsertableObject { get; }
    public string MiscStatusAspects { get; }
    public string MiscStatusDefault { get; }
    public string ProgId { get; }
    public int ServerId { get; }
    public string ShortDisplayName { get; }
    public COMThreadingModel Threading { get; }
    public string ToolboxBitmap32 { get; }
    public List<Tuple<string, string>> Verbs { get; }
    public string VersionIndependentProgId { get; }

    private static Guid? ReadOptionalGuid(string value)
    {
        if (Guid.TryParse(value, out Guid result))
        {
            return result;
        }
        return null;
    }

    internal COMPackagedClassEntry(Guid clsid, string packagePath, RegistryKey rootKey)
    {
        Clsid = clsid;
        AutoConvertTo = rootKey.ReadString(valueName: "AutoConvertTo");
        ConversionReadable = rootKey.ReadString(valueName: "ConversionReadable");
        ConversionReadWritable = rootKey.ReadString(valueName: "ConversionReadWritable");
        ConversionReadWritable = rootKey.ReadString(valueName: "ConversionReadWritable");
        DataFormats = rootKey.ReadString(valueName: "DataFormats");
        DefaultFormatName = rootKey.ReadString(valueName: "DefaultFormatName");
        DefaultIcon = rootKey.ReadString(valueName: "DefaultIcon");
        DisplayName = rootKey.ReadString(valueName: "DisplayName");
        DllPath = rootKey.ReadStringPath(packagePath, valueName: "DllPath");
        EnableOleDefaultHandler = rootKey.ReadBool("EnableOleDefaultHandler");
        ImplementedCategories = rootKey.ReadValueNames("ImplementedCategories")
            .Select(n => ReadOptionalGuid(n)).Where(g => g.HasValue)
            .Select(g => g.Value).ToList();
        InsertableObject = rootKey.ReadBool(valueName: "InsertableObject");
        MiscStatusAspects = rootKey.ReadString(valueName: "MiscStatusAspects");
        MiscStatusDefault = rootKey.ReadString(valueName: "MiscStatusDefault");
        ProgId = rootKey.ReadString(valueName: "ProgId");
        ServerId = rootKey.ReadInt(null, "ServerId");
        ShortDisplayName = rootKey.ReadString(valueName: "ShortDisplayName");
        Threading = (COMThreadingModel)rootKey.ReadInt(null, "Threading");
        ToolboxBitmap32 = rootKey.ReadString(valueName: "ToolboxBitmap32");
        Verbs = rootKey.ReadValues("Verbs").Select(v => Tuple.Create(v.Name, v.Value.ToString())).ToList();
        VersionIndependentProgId = rootKey.ReadString(valueName: "VersionIndependentProgId");
    }
}
