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

namespace OleViewDotNet.TypeLib.Instance;

public sealed class COMTypeCompInstance : IDisposable
{
    private readonly ITypeComp m_type_comp;

    private static int CalcHash(string name)
    {
        return NativeMethods.LHashValOfNameSys(Environment.Is64BitProcess ? SYSKIND.SYS_WIN64 : SYSKIND.SYS_WIN32,
                0, name);
    }

    internal COMTypeCompInstance(ITypeComp type_comp)
    {
        m_type_comp = type_comp;
    }

    public COMTypeCompBindResult Bind(string name, int? hash_val = null, INVOKEKIND flags = 0)
    {
        m_type_comp.Bind(name, hash_val ?? CalcHash(name), (short)flags, out ITypeInfo ppTInfo, out DESCKIND pDescKind, out BINDPTR pBindPtr);
        return COMTypeCompBindResult.GetBindResult(ppTInfo, pDescKind, pBindPtr);
    }

    public COMTypeCompBindResult BindType(string name, int? hash_val = null)
    {
        m_type_comp.BindType(name, hash_val ?? CalcHash(name), out ITypeInfo ppTInfo, out ITypeComp ppTComp);
        return new COMTypeCompBindResultType(ppTInfo, ppTComp);
    }

    public void Dispose()
    {
        m_type_comp.ReleaseComObject();
    }
}

