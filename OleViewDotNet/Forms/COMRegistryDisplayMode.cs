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

namespace OleViewDotNet.Forms;

/// <summary>
/// Enumeration to indicate what to display
/// </summary>
internal enum COMRegistryDisplayMode
{
    CLSIDs,
    ProgIDs,
    CLSIDsByName,
    CLSIDsByServer,
    CLSIDsByLocalServer,
    CLSIDsWithSurrogate,
    Interfaces,
    InterfacesByName,
    ImplementedCategories,
    PreApproved,
    IELowRights,
    LocalServices,
    AppIDs,
    Typelibs,
    AppIDsWithIL,
    MimeTypes,
    AppIDsWithAC,
    ProxyCLSIDs,
    Processes,
    RuntimeClasses,
    RuntimeServers,
    RuntimeInterfaces,
    RuntimeInterfacesTree,
}
