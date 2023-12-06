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

namespace OleViewDotNet.Interop.SxS;

public enum ACTIVATION_CONTEXT_SECTION_ID
{
    UNKNOWN = 0,
    ASSEMBLY_INFORMATION = 1,
    DLL_REDIRECTION = 2,
    CLASS_REDIRECTION = 3,
    COM_SERVER_REDIRECTION = 4,
    COM_INTERFACE_REDIRECTION = 5,
    COM_TYPE_LIBRARY_REDIRECTION = 6,
    COM_PROGID_REDIRECTION = 7,
    GLOBAL_OBJECT_RENAME_TABLE = 8,
    CLR_SURROGATES = 9,
    APPLICATION_SETTINGS = 10,
    COMPATIBILITY_INFO = 11,
}
