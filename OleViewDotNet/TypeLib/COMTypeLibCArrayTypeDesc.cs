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
using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet.TypeLib;

public class COMTypeLibCArrayTypeDesc : COMTypeLibTypeDesc
{
    public COMTypeLibTypeDesc ElementType { get; }
    public IReadOnlyList<int> Dimensions { get; }

    internal COMTypeLibCArrayTypeDesc(COMTypeLibTypeDesc element_type, SAFEARRAYBOUND[] bounds) : base(VariantType.VT_CARRAY)
    {
        ElementType = element_type;
        Dimensions = bounds.Select(b => b.cElements).ToList().AsReadOnly();
    }

    internal override string FormatType()
    {
        return $"{ElementType.FormatType()}";
    }

    internal override string FormatPostName()
    {
        return string.Join("", Dimensions.Select(d => $"[{d}]"));
    }
}
