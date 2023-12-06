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
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Interop;

[ComImport, Guid("79EAC9C9-BAF9-11CE-8C82-00AA004BA90B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IPersistMoniker
{
    void GetClassID(out Guid pClassID);
    void GetCurMoniker(out IMoniker pMoniker);
    [PreserveSig]
    int IsDirty();
    void Load(bool fFullyAvailable, IMoniker pimkName, IBindCtx pibc, STGM grfMode);
    void Save(IMoniker pimkName, IBindCtx pibc, bool fRemember);
    void SaveCompleted(IMoniker pimkName, IBindCtx pibc);
}
