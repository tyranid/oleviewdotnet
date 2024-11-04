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

namespace OleViewDotNet.Interop;

[Flags]
internal enum ASM_DISPLAY_FLAGS
{
    ASM_DISPLAYF_VERSION = 0x01,
    ASM_DISPLAYF_CULTURE = 0x02,
    ASM_DISPLAYF_PUBLIC_KEY_TOKEN = 0x04,
    ASM_DISPLAYF_PUBLIC_KEY = 0x08,
    ASM_DISPLAYF_CUSTOM = 0x10,
    ASM_DISPLAYF_PROCESSORARCHITECTURE = 0x20,
    ASM_DISPLAYF_LANGUAGEID = 0x40,
    ASM_DISPLAYF_RETARGET = 0x80,
    ASM_DISPLAYF_CONFIG_MASK = 0x100,
    ASM_DISPLAYF_MVID = 0x200,
    ASM_DISPLAYF_FULL =
                      ASM_DISPLAYF_VERSION |
                      ASM_DISPLAYF_CULTURE |
                      ASM_DISPLAYF_PUBLIC_KEY_TOKEN |
                      ASM_DISPLAYF_RETARGET |
                      ASM_DISPLAYF_PROCESSORARCHITECTURE

}
