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
using System.Linq;

namespace OleViewDotNet.Utilities;

internal class ObjectEntry
{
    public string Name { get; private set; }
    public object Instance { get; private set; }
    public Guid Id { get; private set; }
    public KeyValuePair<Guid, string>[] Interfaces { get; private set; }

    public override string ToString()
    {
        return Name;
    }

    public ObjectEntry(COMRegistry registry, string name, object instance)
        : this(name, instance, registry.GetInterfacesForObject(instance).OrderBy(i => i.Name).ToArray())
    {
    }

    public ObjectEntry(string name, object instance, COMInterfaceEntry[] interfaces)
    {
        Name = name;
        Instance = instance;
        Id = Guid.NewGuid();

        Interfaces = new KeyValuePair<Guid, string>[interfaces.Length];
        int pos = 0;
        foreach (COMInterfaceEntry ent in interfaces)
        {
            Interfaces[pos++] = new KeyValuePair<Guid, string>(ent.Iid, ent.Name);
        }
    }
}
