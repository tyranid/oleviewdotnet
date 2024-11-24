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

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib.Instance;

internal sealed class COMTypeLibParserContext
{
    public ConcurrentDictionary<Guid, COMTypeLibInterface> ParsedIntfs { get; }
    public ConcurrentDictionary<Guid, COMTypeLibDispatch> ParsedDisp { get; }
    public ConcurrentDictionary<Tuple<string, TYPEKIND>, COMTypeLibTypeInfo> NamedTypes { get; }
    public ConcurrentDictionary<TYPELIBATTR, COMTypeLibReference> RefTypeLibs = new();

    internal COMTypeLibReference GetTypeLibReference(COMTypeLibInstance type_lib)
    {
        return RefTypeLibs.GetOrAdd(type_lib.LibAttr,
            a => new COMTypeLibReference(type_lib.Documentation, a));
    }

    public COMTypeLibParserContext()
    {
        ParsedIntfs = new();
        ParsedDisp = new();
        NamedTypes = new();
        RefTypeLibs = new();
    }
}
