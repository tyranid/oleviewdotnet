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
using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

/// <summary>
/// Base class to represent a type library ITypeInfo definition.
/// </summary>
public class COMTypeLibTypeInfo : ICOMGuid, ICOMSourceCodeFormattable
{
    #region Private Members
    private readonly COMTypeLibDocumentation _doc;
    private readonly TYPEATTR _attr;
    private bool _parsed;

    private protected virtual void OnParse(COMTypeLibParser.TypeInfo type_info, TYPEATTR attr)
    {
    }

    private protected bool HasTypeFlag(TYPEFLAGS flag)
    {
        return _attr.wTypeFlags.HasFlag(flag);
    }

    private protected ICollection<string> GetTypeAttributes(params string[] additional_attrs)
    {
        List<string> attrs = new(additional_attrs);
        if (Uuid != Guid.Empty)
            attrs.Add($"uuid({Uuid.ToString().ToUpper()})");
        if (_attr.wMajorVerNum != 0 || _attr.wMinorVerNum != 0)
            attrs.Add($"version({_attr.wMajorVerNum}.{_attr.wMinorVerNum})");
        if (HasTypeFlag(TYPEFLAGS.TYPEFLAG_FDUAL))
            attrs.Add("dual");
        if (HasTypeFlag(TYPEFLAGS.TYPEFLAG_FOLEAUTOMATION))
            attrs.Add("oleautomation");
        if (HasTypeFlag(TYPEFLAGS.TYPEFLAG_FHIDDEN))
            attrs.Add("hidden");
        if (HasTypeFlag(TYPEFLAGS.TYPEFLAG_FAGGREGATABLE))
            attrs.Add("aggregatable");
        if (HasTypeFlag(TYPEFLAGS.TYPEFLAG_FAPPOBJECT))
            attrs.Add("appobject");
        if (HasTypeFlag(TYPEFLAGS.TYPEFLAG_FCONTROL))
            attrs.Add("control");
        if (HasTypeFlag(TYPEFLAGS.TYPEFLAG_FNONEXTENSIBLE))
            attrs.Add("nonextensible");
        if (HasTypeFlag(TYPEFLAGS.TYPEFLAG_FRESTRICTED))
            attrs.Add("restricted");
        if (HasTypeFlag(TYPEFLAGS.TYPEFLAG_FAPPOBJECT))
            attrs.Add("appobject");
        if (HasTypeFlag(TYPEFLAGS.TYPEFLAG_FPROXY))
            attrs.Add("proxy");

        return attrs;
    }
    #endregion

    #region Internal Members
    internal COMTypeLibTypeInfo(COMTypeLibDocumentation doc, TYPEATTR attr)
    {
        _doc = doc;
        _attr = attr;
    }

    internal void Parse(COMTypeLibParser.TypeInfo type_info)
    {
        if (_parsed)
        {
            return;
        }

        _parsed = true;
        OnParse(type_info, _attr);
    }

    internal virtual void FormatInternal(COMSourceCodeBuilder builder)
    {
    }

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        FormatInternal(builder);
    }
    #endregion

    #region Public Properties
    public string Name => _doc.Name ?? string.Empty;
    public string DocString => _doc.DocString ?? string.Empty;
    public int HelpContext => _doc.HelpContext;
    public string HelpFile => _doc.HelpFile ?? string.Empty;
    public Guid Uuid => _attr.guid;
    public TYPEKIND Kind => _attr.typekind;
    public TYPEFLAGS Flags => _attr.wTypeFlags;
    public COMTypeLibReference TypeLib { get; internal set; }
    Guid ICOMGuid.ComGuid => Uuid;
    bool ICOMSourceCodeFormattable.IsFormattable => true;
    #endregion

    #region Public Methods
    public string Format()
    {
        COMSourceCodeBuilder builder = new();
        FormatInternal(builder);
        return builder.ToString();
    }
    #endregion
}
