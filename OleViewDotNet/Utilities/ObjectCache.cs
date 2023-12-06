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
using System;
using System.Collections.Generic;

namespace OleViewDotNet.Utilities;

internal static class ObjectCache
{
    private static readonly List<ObjectEntry> m_objects = new();

    public static ObjectEntry Add(string name, object instance, COMInterfaceEntry[] interfaces)
    {
        ObjectEntry ret = new(name, instance, interfaces);
        m_objects.Add(ret);

        return ret;
    }

    public static void Remove(ObjectEntry instance)
    {
        m_objects.Remove(instance);
    }

    public static ObjectEntry[] Objects => m_objects.ToArray();

    public static ObjectEntry GetObjectByName(string name)
    {
        return m_objects.Find(o => string.Equals(name, o.Name, StringComparison.CurrentCultureIgnoreCase));
    }

    public static ObjectEntry GetObjectByGuid(Guid id)
    {
        return m_objects.Find(o => o.Id == id);
    }
}
