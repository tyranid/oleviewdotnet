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

using NtApiDotNet.Ndr;
using OleViewDotNet.Utilities.Format;
using System;
using System.Collections.Generic;

namespace OleViewDotNet.Proxy;

public sealed class COMProxyInterfaceProcedureParameter
{
    private readonly COMProxyInterface m_intf;

    internal COMProxyInterfaceProcedureParameter(COMProxyInterface intf, NdrProcedureParameter entry)
    {
        m_intf = intf;
        Entry = entry;
    }

    public string Name
    {
        get => Entry.Name;
        set => Entry.Name = m_intf.CheckName(Entry.Name, value);
    }
    public NdrBaseTypeReference Type => Entry.Type;
    public bool IsIn => Entry.IsIn;
    public bool IsOut => Entry.IsOut;
    public bool IsInOut => Entry.IsInOut;

    public NdrProcedureParameter Entry { get; }
}
