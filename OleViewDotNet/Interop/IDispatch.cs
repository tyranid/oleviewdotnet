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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Interop;

[Guid("00020400-0000-0000-c000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IDispatch
{
    void GetTypeInfoCount(out uint pctinfo);
    void GetTypeInfo(uint iTypeInfo, uint lcid, out ITypeInfo type_info);
    void GetIDsOfNames(in Guid riid, string[] rszNames, uint cNames, uint lcid, ref int[] dispIDs);
    void Invoke(int dispIdMember, in Guid riid, uint lcid, ushort wFlags, System.Runtime.InteropServices.ComTypes.DISPPARAMS[] pDispParams,
                out VariantWrapper pVarResult, ref System.Runtime.InteropServices.ComTypes.EXCEPINFO pExcepInfo, out uint puArgErr);
}
