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
using OleViewDotNet.Proxy;
using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

/// <summary>
/// Class to represent information in a COM type library.
/// </summary>
public sealed class COMTypeLib : COMTypeLibReference, ICOMGuid, ICOMSourceCodeFormattable
{
    #region Private Members
    private void FormatInternal(COMSourceCodeBuilder builder)
    {
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
            if (!builder.InterfacesOnly)
            {
                builder.AppendObjects(Aliases);
                builder.AppendObjects(Enums);
                builder.AppendObjects(Records);
                builder.AppendObjects(Unions);
                builder.AppendObjects(Modules);
                builder.AppendObjects(Classes);
            }
            builder.AppendObjects(Interfaces);
            builder.AppendObjects(Dispatch);
        }
        builder.AppendLine("};");
    }
    #endregion

    #region Internal Members
    internal COMTypeLib(string path, COMTypeLibDocumentation doc, TYPELIBATTR attr, List<COMTypeLibTypeInfo> types) 
        : base(doc, attr)
    {
        Path = path ?? string.Empty;
        types.ForEach(t => t.TypeLib = this);
        Types = types.AsReadOnly();
        var interfaces = types.OfType<COMTypeLibInterface>().ToDictionary(i => i.Uuid);
        var dispatch = Types.OfType<COMTypeLibDispatch>().ToList();
        foreach (var disp in dispatch.Where(d => d.DualInterface is not null))
        {
            if (!interfaces.ContainsKey(disp.DualInterface.Uuid))
            {
                disp.DualInterface.TypeLib = this;
                interfaces.Add(disp.DualInterface.Uuid, disp.DualInterface);
            }
        }

        Interfaces = interfaces.Values.ToList().AsReadOnly();
        Dispatch = dispatch.AsReadOnly();
        Enums = types.OfType<COMTypeLibEnum>().ToList().AsReadOnly();
        Records = types.OfType<COMTypeLibRecord>().ToList().AsReadOnly();
        Aliases = types.OfType<COMTypeLibAlias>().ToList().AsReadOnly();
        Unions = types.OfType<COMTypeLibUnion>().ToList().AsReadOnly();
        Modules = types.OfType<COMTypeLibModule>().ToList().AsReadOnly();
        Classes = types.OfType<COMTypeLibCoClass>().ToList().AsReadOnly();
        ComplexTypes = types.OfType<COMTypeLibComplexType>().ToList().AsReadOnly();
    }

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        FormatInternal(builder);
    }
    #endregion

    #region Public Static Methods
    public static COMTypeLib FromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));
        }

        using COMTypeLibParser parser = new(path);
        return parser.Parse();
    }

    public static COMTypeLib FromObject(object obj)
    {
        IDispatch disp = (IDispatch)obj;

        disp.GetTypeInfo(0, 0x409, out ITypeInfo type_info);
        type_info.GetContainingTypeLib(out ITypeLib type_lib, out _);
        using COMTypeLibParser parser = new(type_lib);
        return parser.Parse();
    }
    #endregion

    #region Public Properties
    public string Path { get; }
    public IReadOnlyList<COMTypeLibInterface> Interfaces { get; }
    public IReadOnlyList<COMTypeLibDispatch> Dispatch { get; }
    public IReadOnlyList<COMTypeLibEnum> Enums { get; }
    public IReadOnlyList<COMTypeLibRecord> Records { get; }
    public IReadOnlyList<COMTypeLibAlias> Aliases { get; }
    public IReadOnlyList<COMTypeLibUnion> Unions { get; }
    public IReadOnlyList<COMTypeLibModule> Modules { get; }
    public IReadOnlyList<COMTypeLibTypeInfo> Types { get; }
    public IReadOnlyList<COMTypeLibCoClass> Classes { get; }
    public IReadOnlyList<COMTypeLibComplexType> ComplexTypes { get; }
    public IReadOnlyDictionary<Guid, COMTypeLibInterface> InterfacesByIid => Interfaces.ToDictionary(i => i.Uuid);
    Guid ICOMGuid.ComGuid => TypeLibId;
    bool ICOMSourceCodeFormattable.IsFormattable => true;
    #endregion

    #region Public Methods
    public string FormatText(ProxyFormatterFlags flags = ProxyFormatterFlags.None)
    {
        COMSourceCodeBuilder builder = new();
        builder.InterfacesOnly = flags.HasFlag(ProxyFormatterFlags.RemoveComplexTypes);
        FormatInternal(builder);
        return builder.ToString();
    }
    #endregion
}
