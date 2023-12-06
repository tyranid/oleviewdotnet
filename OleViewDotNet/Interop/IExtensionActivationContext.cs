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

namespace OleViewDotNet.Interop;

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("533148e2-ee0a-4b06-8500-7fda28f92ae2")]
internal interface IExtensionActivationContext
{
    ulong HostId { get; set; }
    ulong UserContext { get; set; }
    ulong ComponentProcessId { get; set; }
    ulong RacActivationTokenId { get; set; }
    IntPtr LpacAttributes { get; set; }
    ulong ConsoleHandlesId { get; set; }
    uint AAMActivationId { get; set; }
}
