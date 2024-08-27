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

using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib.Parser;

using Marshal = System.Runtime.InteropServices.Marshal;

public sealed class COMTypeCompBindResult
{
    public COMTypeInfoInstance TypeInfo { get; }
    public COMTypeCompInstance TypeComp { get; }
    public FUNCDESC? FuncDesc { get; }
    public VARDESC? VarDesc { get; }

    internal COMTypeCompBindResult(ITypeInfo type_info, ITypeComp type_comp)
    {
        TypeInfo = new COMTypeInfoInstance(type_info);
        TypeComp = new COMTypeCompInstance(type_comp);
    }

    internal COMTypeCompBindResult(ITypeInfo type_info, DESCKIND desc_kind, BINDPTR bind_ptr)
    {
        TypeInfo = new(type_info);
        switch (desc_kind)
        {
            case DESCKIND.DESCKIND_TYPECOMP:
                TypeComp = new COMTypeCompInstance((ITypeComp)Marshal.GetObjectForIUnknown(bind_ptr.lptcomp));
                break;
            case DESCKIND.DESCKIND_VARDESC:
                VarDesc = bind_ptr.lpvardesc.GetStructure<VARDESC>();
                break;
            case DESCKIND.DESCKIND_FUNCDESC:
                FuncDesc = bind_ptr.lpfuncdesc.GetStructure<FUNCDESC>();
                break;
        }
    }
}

