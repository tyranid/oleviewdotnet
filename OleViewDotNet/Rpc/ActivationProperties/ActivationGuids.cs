//    Copyright (C) James Forshaw 2024
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

namespace OleViewDotNet.Rpc.ActivationProperties;

public static class ActivationGuids
{
    public static readonly Guid CLSID_ActivationContextInfo = new("000001a5-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_ActivationPropertiesIn = new("00000338-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_ActivationPropertiesOut = new("00000339-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_CONTEXT_EXTENSION = new("00000334-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_ContextMarshaler = new("0000033b-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_ERROR_EXTENSION = new("0000031c-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_ErrorObject = new("0000031b-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_InstanceInfo = new("000001ad-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_InstantiationInfo = new("000001ab-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_PropsOutInfo = new("00000339-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_ScmReplyInfo = new("000001b6-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_ScmRequestInfo = new("000001aa-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_SecurityInfo = new("000001a6-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_ServerLocationInfo = new("000001a4-0000-0000-c000-000000000046");
    public static readonly Guid CLSID_SpecialSystemProperties = new("000001b9-0000-0000-c000-000000000046");
    public static readonly Guid IID_IActivation = new("4d9f4ab8-7d1c-11cf-861e-0020af6e7c57");
    public static readonly Guid IID_IActivationPropertiesIn = new("000001A2-0000-0000-C000-000000000046");
    public static readonly Guid IID_IActivationPropertiesOut = new("000001A3-0000-0000-C000-000000000046");
    public static readonly Guid IID_IContext = new("000001c0-0000-0000-C000-000000000046");
    public static readonly Guid IID_IObjectExporter = new("99fcfec4-5260-101b-bbcb-00aa0021347a");
    public static readonly Guid IID_IRemoteSCMActivator = new("000001A0-0000-0000-C000-000000000046");
    public static readonly Guid IID_IRemUnknown = new("00000131-0000-0000-C000-000000000046");
    public static readonly Guid IID_IRemUnknown2 = new("00000143-0000-0000-C000-000000000046");
    public static readonly Guid IID_IUnknown = new("00000000-0000-0000-C000-000000000046");
    public static readonly Guid CLSID_WinRTActivationProperties = new("9fc104cb-0379-43b5-a1a0-ce1260700e0a");
    public static readonly Guid CLSID_ExtensionActivationContextProperties = new("071742DC-587E-4930-81D5-8A7DE8547529");
}