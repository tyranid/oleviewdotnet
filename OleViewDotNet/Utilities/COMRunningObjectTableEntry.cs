//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
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

namespace OleViewDotNet.Utilities;

public sealed class COMRunningObjectTableEntry
{
    private readonly IRunningObjectTable m_rot;

    internal COMRunningObjectTableEntry(IRunningObjectTable rot, IMoniker moniker)
    {
        m_rot = rot;
        Moniker = moniker;
        Clsid = COMUtilities.GetObjectClass(moniker);
        DisplayName = COMUtilities.GetMonikerDisplayName(moniker);
    }

    public IMoniker Moniker { get; }

    public string DisplayName { get; }

    public Guid Clsid { get; }

    public object GetObject()
    {
        m_rot.GetObject(Moniker, out object obj).CheckHr();
        return obj;
    }
}
