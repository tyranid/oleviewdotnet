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

namespace OleViewDotNet.Interop;

public static class COMKnownGuids
{
    public static Guid IID_IUnknown => new("{00000000-0000-0000-C000-000000000046}");

    public static Guid IID_IMarshal => new("{00000003-0000-0000-C000-000000000046}");

    public static Guid IID_IMarshal2 => new("000001CF-0000-0000-C000-000000000046");

    public static Guid IID_IContextMarshaler => new("000001D8-0000-0000-C000-000000000046");

    public static Guid IID_IStdMarshalInfo => new("00000018-0000-0000-C000-000000000046");

    public static Guid IID_IMarshalEnvoy => new("000001C8-0000-0000-C000-000000000046");

    public static Guid IID_IDispatch => new("00020400-0000-0000-c000-000000000046");

    public static Guid IID_IOleControl => new("{b196b288-bab4-101a-b69c-00aa00341d07}");

    public static Guid IID_IPersistStream => typeof(IPersistStream).GUID;

    public static Guid IID_IPersistStreamInit => typeof(IPersistStreamInit).GUID;

    public static Guid IID_IPSFactoryBuffer => new("D5F569D0-593B-101A-B569-08002B2DBF7A");

    public static Guid IID_IInspectable => new("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90");
}