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

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace OleViewDotNet
{    
    class ObjectEntry
    {
        public string Name { get; private set; }
        public object Instance { get; private set; }
        public Guid Id { get; private set; }
        public KeyValuePair<Guid, string>[] Interfaces { get; private set; }

        public override string ToString()
        {
            return Name;
        }

        public ObjectEntry(string name, object instance)
            : this(name, instance, COMRegistry.Instance.GetInterfacesForObject(instance))
        {

        }

        public ObjectEntry(string name, object instance, COMInterfaceEntry[] interfaces)
        {
            Name = name;
            Instance = instance;
            Id = Guid.NewGuid();

            Interfaces = new KeyValuePair<Guid,string>[interfaces.Length];
            int pos = 0;
            foreach (COMInterfaceEntry ent in interfaces)
            {
                Interfaces[pos++] = new KeyValuePair<Guid, string>(ent.Iid, ent.Name);
            }
        }        
     
        public dynamic QueryInterface(string name)
        {
            Guid iid = Guid.Empty;
            object o = null;
            if (COMUtilities.IsValidGUID(name))
            {
                iid = new Guid(name);
            }
            else
            {
                foreach (KeyValuePair<Guid, string> pair in Interfaces)
                {
                    if (String.Compare(pair.Value, name, true) == 0)
                    {
                        iid = pair.Key;
                        break;
                    }
                }
            }            

            Type type = COMUtilities.GetInterfaceType(iid);
            if (type != null)
            {                
                o = new DynamicComObjectWrapper(type, Instance);
            }

            return o;
        }
    }

    static class ObjectCache
    {
        private static List<ObjectEntry> m_objects = new List<ObjectEntry>();

        public static ObjectEntry Add(string name, object instance, COMInterfaceEntry[] interfaces)
        {
            ObjectEntry ret = new ObjectEntry(name, instance, interfaces);
            m_objects.Add(ret);

            return ret;
        }

        public static void Remove(ObjectEntry instance)
        {
            m_objects.Remove(instance);
        }

        public static ObjectEntry[] Objects
        {
            get
            {
                return m_objects.ToArray();
            }
        }

        public static ObjectEntry GetObjectByName(string name)
        {
            return m_objects.Find(o => String.Equals(name, o.Name, StringComparison.CurrentCultureIgnoreCase));
        }

        public static ObjectEntry GetObjectByGuid(Guid id)
        {
            return m_objects.Find(o => o.Id == id);
        }
    }
}
