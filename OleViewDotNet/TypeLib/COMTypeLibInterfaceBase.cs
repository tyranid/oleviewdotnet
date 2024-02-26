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

using OleViewDotNet.Proxy;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib;

public abstract class COMTypeLibInterfaceBase : COMTypeLibTypeInfo
{
    #region Private Members
    private protected override void OnParse(COMTypeLibParser.TypeInfo type_info, TYPEATTR attr)
    {
        List<COMTypeLibMethod> methods = new();
        for (int i = 0; i < attr.cFuncs; ++i)
        {
            methods.Add(new(type_info, i));
        }
        Methods = methods.AsReadOnly();
        List<COMTypeLibInterface> impl_intfs = new();
        for (int i = 0; i < attr.cImplTypes; ++i)
        {
            impl_intfs.Add(type_info.ParseRefInterface(i));
        }
        ImplementedInterfaces = impl_intfs.AsReadOnly();
    }
    #endregion

    #region Public Properties
    public IReadOnlyList<COMTypeLibInterface> ImplementedInterfaces { get; private set; }
    public IReadOnlyList<COMTypeLibMethod> Methods { get; private set; }
    #endregion

    #region Internal Members
    internal COMTypeLibInterfaceBase(COMTypeLibDocumentation doc, TYPEATTR attr)
        : base(doc, attr)
    {
    }
    #endregion


    #region IProxyFormatter Implementation
    public string FormatText(ProxyFormatterFlags flags = ProxyFormatterFlags.None)
    {
        return Format();
    }
    #endregion
}
