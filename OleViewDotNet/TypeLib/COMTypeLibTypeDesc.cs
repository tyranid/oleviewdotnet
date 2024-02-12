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
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

public class COMTypeLibTypeDesc
{
    public VariantType Type { get; }

    internal static COMTypeLibTypeDesc Parse(COMTypeLibParser.TypeInfo type_info, TYPEDESC desc)
    {
        VariantType type = (VariantType)desc.vt;
        if (type == VariantType.VT_PTR)
        {
            return new COMTypeLibPointerTypeDesc(
                Parse(type_info, 
                desc.lpValue.GetStructure<TYPEDESC>()));
        }
        else if (type == VariantType.VT_USERDEFINED)
        {
            int refid = (int)desc.lpValue.ToInt64();
            var ref_type_info = type_info.GetRefTypeInfo(refid);
            return new COMTypeLibUserDefinedTypeDesc(ref_type_info.Parse());
        }
        return new COMTypeLibTypeDesc(type);
    }

    internal COMTypeLibTypeDesc(VariantType type)
    {
        Type = type;
    }
}
