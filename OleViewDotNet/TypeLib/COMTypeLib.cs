//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
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
using OleViewDotNet.Interop;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

/// <summary>
/// Class to represent information in a COM type library.
/// </summary>
public sealed class COMTypeLib
{
    #region Private Members
    private readonly COMTypeLibDocumentation _doc;
    private readonly TYPELIBATTR _attr;
    #endregion

    #region Internal Members
    internal COMTypeLib(COMTypeLibDocumentation doc, TYPELIBATTR attr, List<COMTypeLibTypeInfo> types)
    {
        _doc = doc;
        _attr = attr;
        Types = types.AsReadOnly();
        var interfaces = types.OfType<COMTypeLibInterface>().ToDictionary(i => i.Uuid);
        var dispatch = Types.OfType<COMTypeLibDispatch>().ToList();
        foreach (var disp in dispatch.Where(d => d.DualInterface != null))
        {
            if (!interfaces.ContainsKey(disp.DualInterface.Uuid))
            {
                interfaces.Add(disp.DualInterface.Uuid, disp.DualInterface);
            }
        }

        Interfaces = interfaces.Values.ToList().AsReadOnly();
        Dispatch = dispatch.AsReadOnly();
        Enums = types.OfType<COMTypeLibEnum>().ToList().AsReadOnly();
        Records = types.OfType<COMTypeLibRecord>().ToList().AsReadOnly();
        Aliases = types.OfType<COMTypeLibAlias>().ToList().AsReadOnly();
    }
    #endregion

    #region Public Static Methods
    public static COMTypeLib FromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));
        }

        using COMTypeLibParser parser = new(NativeMethods.LoadTypeLibEx(path, RegKind.RegKind_Default));
        return parser.Parse();
    }
    #endregion

    #region Public Properties
    public string Name => _doc.Name ?? string.Empty;
    public string DocString => _doc.DocString ?? string.Empty;
    public int HelpContext => _doc.HelpContext;
    public string HelpFile => _doc.HelpFile ?? string.Empty;
    public Guid TypeLibId => _attr.guid;
    public COMVersion Version => new(_attr.wMajorVerNum, _attr.wMinorVerNum);
    public IReadOnlyList<COMTypeLibInterface> Interfaces { get; }
    public IReadOnlyList<COMTypeLibDispatch> Dispatch { get; }
    public IReadOnlyList<COMTypeLibEnum> Enums { get; }
    public IReadOnlyList<COMTypeLibRecord> Records { get; }
    public IReadOnlyList<COMTypeLibAlias> Aliases { get; }
    public IReadOnlyList<COMTypeLibTypeInfo> Types { get; }
    #endregion

    #region Public Methods
    public string Format()
    {
        SourceCodeBuilder builder = new();
        List<string> attrs = new()
        {
            $"uuid({TypeLibId.ToString().ToUpper()})",
            $"version({Version})"
        };
        attrs.AddRange(_doc.GetAttrs());
        builder.AppendAttributes(attrs);
        builder.AppendLine($"library {Name} {{");
        using (builder.PushIndent(4))
        {
            builder.FormatTypes(Aliases);
            builder.FormatTypes(Enums);
            builder.FormatTypes(Interfaces);
            builder.FormatTypes(Dispatch);
            builder.FormatTypes(Records);
        }
        builder.AppendLine("};");
        return builder.ToString();
    }
    #endregion
}
