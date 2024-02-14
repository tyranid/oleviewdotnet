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

using NtApiDotNet;
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
        else if (type == VariantType.VT_SAFEARRAY)
        {
            return new COMTypeLibSafeArrayTypeDesc(Parse(type_info,
                desc.lpValue.GetStructure<TYPEDESC>()));
        }
        else if (type == VariantType.VT_USERDEFINED)
        {
            int refid = (int)desc.lpValue.ToInt64();
            var ref_type_info = type_info.GetRefTypeInfo(refid);
            return new COMTypeLibUserDefinedTypeDesc(ref_type_info.Parse());
        }
        else if (type == VariantType.VT_CARRAY)
        {
            var buffer = new SafeStructureInOutBuffer<ARRAYDESC>(desc.lpValue, 0, true, false);
            var res = buffer.Result;
            int additional_size = res.cDims * COMTypeLibUtils.GetTypeSize<SAFEARRAYBOUND>();
            buffer = new SafeStructureInOutBuffer<ARRAYDESC>(desc.lpValue, additional_size, true, false);
            var bounds = buffer.Data.ReadArray<SAFEARRAYBOUND>(0, res.cDims);
            return new COMTypeLibCArrayTypeDesc(Parse(type_info, res.tdescElem), bounds);
        }
        return new COMTypeLibTypeDesc(type);
    }

    internal COMTypeLibTypeDesc(VariantType type)
    {
        Type = type;
    }

    internal virtual string FormatType()
    {
        return Type switch
        {
            VariantType.VT_VOID => "void",
            VariantType.VT_DISPATCH => "IDispatch*",
            VariantType.VT_BSTR => "BSTR",
            VariantType.VT_I1 => "char",
            VariantType.VT_I2 => "short",
            VariantType.VT_I4 or VariantType.VT_INT => "int",
            VariantType.VT_I8 => "long long",
            VariantType.VT_UINT or VariantType.VT_UI4 => "unsigned int",
            VariantType.VT_UI1 => "unsigned char",
            VariantType.VT_UI2 => "unsigned short",
            VariantType.VT_UI8 => "unsigned long long",
            VariantType.VT_UNKNOWN => "IUnknown*",
            VariantType.VT_VARIANT => "VARIANT",
            VariantType.VT_HRESULT => "HRESULT",
            VariantType.VT_LPSTR => "LPSTR",
            VariantType.VT_LPWSTR => "LPWSTR",
            VariantType.VT_R4 => "float",
            VariantType.VT_R8 => "double",
            VariantType.VT_CY => "CURRENCY",
            VariantType.VT_BOOL => "VARIANT_BOOL",
            VariantType.VT_DATE => "DATE",
            VariantType.VT_SAFEARRAY => "SAFEARRAY",
            VariantType.VT_DECIMAL => "DECIMAL",
            VariantType.VT_ERROR => "HRESULT",
            _ => Type.ToString(),
        };
    }

    internal virtual string FormatPostName()
    {
        return string.Empty;
    }
}
