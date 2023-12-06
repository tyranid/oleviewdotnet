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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Interop;

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIInspectable), Guid("905754d7-1cc5-404e-a9d4-c517bc35e88d")]
public interface IExtensionRegistration
{
    string ActivatableClassId
    {
        [return: MarshalAs(UnmanagedType.HString)]
        get;
    }

    string ContractId
    {
        [return: MarshalAs(UnmanagedType.HString)]
        get;
    }

    string PackageId
    {
        [return: MarshalAs(UnmanagedType.HString)]
        get;
    }

    string ExtensionId
    {
        [return: MarshalAs(UnmanagedType.HString)]
        get;
    }


    object ActivatableClassRegistration // Windows::Foundation::IActivatableClassRegistration
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        get;
    }

    string Vendor
    {
        [return: MarshalAs(UnmanagedType.HString)]
        get;
    }

    string Icon
    {
        [return: MarshalAs(UnmanagedType.HString)]
        get;
    }

    string DisplayName
    {
        [return: MarshalAs(UnmanagedType.HString)]
        get;
    }

    string Description
    {
        [return: MarshalAs(UnmanagedType.HString)]
        get;
    }

    RegistrationScope RegistrationScope
    {
        get;
    }

    IReadOnlyDictionary<string, object> Attributes
    {
        get;
    }

    int OutOfProcActivationFlags
    {
        get; set;
    }

    [return: MarshalAs(UnmanagedType.IInspectable)]
    object Activate();
}
