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

using OleViewDotNet.Interop;
using System;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

/// <summary>
/// Class to represent a type library reference.
/// </summary>
public class COMTypeLibReference
{
    #region Private Members
    private protected readonly COMTypeLibDocumentation _doc;
    private protected readonly TYPELIBATTR _attr;
    #endregion

    #region Internal Members
    internal COMTypeLibReference(COMTypeLibDocumentation doc, TYPELIBATTR attr)
    {
        _doc = doc;
        _attr = attr;
    }
    #endregion

    #region Public Properties
    public string Name => _doc.Name ?? string.Empty;
    public string DocString => _doc.DocString ?? string.Empty;
    public int HelpContext => _doc.HelpContext;
    public string HelpFile => _doc.HelpFile ?? string.Empty;
    public Guid TypeLibId => _attr.guid;
    public COMVersion Version => new(_attr.wMajorVerNum, _attr.wMinorVerNum);
    #endregion

    #region Public Methods
    public override string ToString()
    {
        return $"{Name} - {Version}";
    }
    #endregion
}
