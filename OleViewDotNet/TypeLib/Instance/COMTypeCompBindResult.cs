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
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib.Instance;

using Marshal = System.Runtime.InteropServices.Marshal;

public class COMTypeCompBindResult : IDisposable
{
    public COMTypeInfoInstance TypeInfo { get; }
    public DESCKIND DescKind { get; }

    protected COMTypeCompBindResult(ITypeInfo type_info, DESCKIND desc_kind)
    {
        TypeInfo = type_info is not null ? new COMTypeInfoInstance(type_info) : null;
        DescKind = desc_kind;
    }

    internal static COMTypeCompBindResult GetBindResult(ITypeInfo type_info, DESCKIND desc_kind, BINDPTR bind_ptr)
    {
        return desc_kind switch
        {
            DESCKIND.DESCKIND_TYPECOMP => new COMTypeCompBindResultType(type_info, (ITypeComp)Marshal.GetObjectForIUnknown(bind_ptr.lptcomp)),
            DESCKIND.DESCKIND_VARDESC => new COMTypeCompBindResultVar(type_info, bind_ptr.lpvardesc),
            DESCKIND.DESCKIND_FUNCDESC => new COMTypeCompBindResultFunc(type_info, bind_ptr.lpfuncdesc),
            _ => new COMTypeCompBindResult(type_info, desc_kind),
        };
    }

    protected virtual void OnDispose()
    {
        TypeInfo.Dispose();
    }

    public void Dispose()
    {
        OnDispose();
    }
}
