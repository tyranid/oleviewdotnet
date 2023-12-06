//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2019
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

namespace OleViewDotNet.Interop.SxS;

[Flags]
public enum ACTIVATION_CONTEXT_DATA_DLL_REDIRECTION_PATH_FLAGS
{
    INCLUDES_BASE_NAME = 0x00000001,
    OMITS_ASSEMBLY_ROOT = 0x00000002,
    EXPAND = 0x00000004,
    SYSTEM_DEFAULT_REDIRECTED_SYSTEM32_DLL = 0x00000008,
}
