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

public sealed class COMTypeCompInstance : IDisposable
{
    private readonly ITypeComp m_type_comp;

    internal COMTypeCompInstance(ITypeComp type_comp)
    {
        m_type_comp = type_comp;
    }

    public COMTypeCompBindResult Bind(string szName, int lHashVal, short wFlags)
    {
        m_type_comp.Bind(szName, lHashVal, wFlags, out ITypeInfo ppTInfo, out DESCKIND pDescKind, out BINDPTR pBindPtr);
        return new COMTypeCompBindResult(ppTInfo, pDescKind, pBindPtr);
    }

    public COMTypeCompBindResult BindType(string szName, int lHashVal)
    {
        m_type_comp.BindType(szName, lHashVal, out ITypeInfo ppTInfo, out ITypeComp ppTComp);
        return new COMTypeCompBindResult(ppTInfo, ppTComp);
    }

    void IDisposable.Dispose()
    {
        m_type_comp.ReleaseComObject();
    }
}

