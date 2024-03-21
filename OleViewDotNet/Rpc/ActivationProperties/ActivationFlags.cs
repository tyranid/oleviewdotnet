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

[Flags]
public enum ActivationFlags
{
    NONE = 0x0,
    DISABLE_AAA = 0x2,
    ACTIVATE_X86_SERVER = 0x4,
    ACTIVATE_X64_SERVER = 0x8,
    SERVERSIDE_ACTIVATION = 0x10,
    NO_FAILURE_LOG = 0x20,
    ENABLE_CLOAKING = 0x40,
    REG_CATALOG_ONLY = 0x100,
    SXS_CATALOG_ONLY = 0x200,
    WINRT_LOCAL_SERVER = 0x400,
    WINRT_PER_USER_OK = 0x800,
    ACTIVATE_PSCLSID_FROM_PACKAGE = 0x1000,
    APPCONTAINER = 0x2000,
    IS_SXS_CLASS = 0x4000,
    BACKGROUND_MIXED_ACTIVATION = 0x8000,
    BACKGROUND_PURE_ACTIVATION = 0x10000,
    BACKGROUND_SYSTEM_ACTIVATION = 0x20000,
    DESIGNMODE_ACTIVATION = 0x40000,
    ACTIVATE_AS_IU = 0x80000,
    ACTIVATE_SHARED_AAP_SERVER = 0x100000,
    DISABLE_REMOTE_ACTIVATION = 0x200000,
    DESIGNMODE_V2_ACTIVATION = 0x400000,
    ACTIVATE_NATIVE_SERVER = 0x800000,
    ACTIVATE_ARM32_SERVER = 0x1000000,
    ACTIVATE_PACKAGED_IMPLEMENTATION_CLASS = 0x2000000,
    ALLOW_LOWER_TRUST_REGISTRATION = 0x4000000,
};
