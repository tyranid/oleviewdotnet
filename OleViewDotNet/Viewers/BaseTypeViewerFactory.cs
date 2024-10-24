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

using OleViewDotNet.Database;
using OleViewDotNet.Utilities;
using System;
using System.Windows.Forms;

namespace OleViewDotNet.Viewers;

/// <summary>
/// Simple base implementation to reduce the amount of code to write
/// </summary>
internal abstract class BaseTypeViewerFactory : ITypeViewerFactory
{
    public BaseTypeViewerFactory(string strName, Guid iid)
    {
        IidName = strName;
        Iid = iid;
    }

    public BaseTypeViewerFactory(Type type)
    {
        IidName = type.Name;
        Iid = type.GUID;
    }

    public string IidName { get; private set; }
    public Guid Iid { get; private set; }
    abstract public Control CreateInstance(COMRegistry registry, ICOMClassEntry entry, string strObjName, ObjectEntry pObject);
}
