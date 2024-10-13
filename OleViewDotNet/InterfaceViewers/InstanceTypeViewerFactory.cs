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
using OleViewDotNet.Forms;
using OleViewDotNet.Utilities;
using System;
using System.Windows.Forms;

namespace OleViewDotNet.InterfaceViewers;

/// <summary>
/// Generic factory implementation for use when we have a instantiated type object
/// </summary>
internal class InstanceTypeViewerFactory : BaseTypeViewerFactory
{
    private readonly Type m_type;
    public InstanceTypeViewerFactory(Type t)
        : base(t)
    {
        m_type = t;
    }

    public override Control CreateInstance(COMRegistry registry,
        ICOMClassEntry entry, string strObjName, ObjectEntry pObject)
    {
        return new TypedObjectViewer(registry, strObjName, pObject, m_type);
    }
}
