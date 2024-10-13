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
    private static Guid GetGuid<T>()
    {
        return typeof(T).GUID;
    }

    public static Guid IID_IUnknown => GetGuid<IUnknown>();

    public static Guid IID_IMarshal => new("{00000003-0000-0000-C000-000000000046}");

    public static Guid IID_IMarshal2 => new("000001CF-0000-0000-C000-000000000046");

    public static Guid IID_IContextMarshaler => new("000001D8-0000-0000-C000-000000000046");

    public static Guid IID_IStdMarshalInfo => new("00000018-0000-0000-C000-000000000046");

    public static Guid IID_IMarshalEnvoy => new("000001C8-0000-0000-C000-000000000046");

    public static Guid IID_IDispatch => GetGuid<IDispatch>();

    public static Guid IID_IOleControl => new("{b196b288-bab4-101a-b69c-00aa00341d07}");

    public static Guid IID_IPersistStream => GetGuid<IPersistStream>();

    public static Guid IID_IPersistStreamInit => GetGuid<IPersistStreamInit>();

    public static Guid IID_IPSFactoryBuffer => GetGuid<IPSFactoryBuffer>();

    public static Guid IID_IInspectable => GetGuid<IInspectable>();

    public static Guid IID_IClassFactory => GetGuid<IClassFactory>();

    public static Guid CLSID_PSAutomation = new("00020424-0000-0000-C000-000000000046");

    public static Guid CLSID_PSDispatch = new("00020420-0000-0000-C000-000000000046");

    public static readonly Guid CATID_TrustedMarshaler = new("00000003-0000-0000-C000-000000000046");

    public static readonly Guid CATID_SafeForScripting = new("7DD95801-9882-11CF-9FA9-00AA006C42C4");

    public static readonly Guid CATID_SafeForInitializing = new("7DD95802-9882-11CF-9FA9-00AA006C42C4");

    public static readonly Guid CATID_Control = new("{40FC6ED4-2438-11CF-A3DB-080036F12502}");

    public static readonly Guid CATID_Insertable = new("{40FC6ED3-2438-11CF-A3DB-080036F12502}");

    public static readonly Guid CATID_Document = new("{40fc6ed8-2438-11cf-a3db-080036f12502}");

    public static readonly Guid IID_IProxyManager = new("{00000008-0000-0000-C000-000000000046}");
}