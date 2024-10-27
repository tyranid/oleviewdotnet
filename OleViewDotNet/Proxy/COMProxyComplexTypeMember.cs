//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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

using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;

namespace OleViewDotNet.Proxy;

public abstract class COMProxyComplexTypeMember
{
    #region Private Members
    private readonly COMProxyInterface m_intf;
    #endregion

    #region Protected Members
    protected COMProxyComplexTypeMember(COMProxyInterface intf)
    {
        m_intf = intf;
    }
    protected abstract string GetName();
    protected abstract void SetName(string name);
    #endregion

    #region Public Properties
    public string Name { get => GetName(); set => SetName(m_intf?.CheckName(GetName(), value) ?? value); }
    #endregion
}
