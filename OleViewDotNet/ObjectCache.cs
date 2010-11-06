//    This file is part of OleViewDotNet.
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

namespace OleViewDotNet
{    
    class ObjectEntry
    {
        public string Name { get; private set; }
        public object Instance { get; private set; }
        public Guid Id { get; private set; }
        public KeyValuePair<Guid, string>[] Interfaces { get; private set; }

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
     
        public object QueryInterface(string name)
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
                DynamicMethod method = new DynamicMethod("CastTypeInstance", type, new Type[] { typeof(object) });
                ILGenerator gen = method.GetILGenerator(256);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Castclass, type);
                gen.Emit(OpCodes.Ret);
                o = method.Invoke(null, new object[] { Instance });                
            }

            return o;
        }
    }

    class ObjectCache
    {
        private static Dictionary<object, ObjectEntry> m_objects = new Dictionary<object, ObjectEntry>();

        public static void Add(string name, object instance, COMInterfaceEntry[] interfaces)
        {
            m_objects.Add(instance, new ObjectEntry(name, instance, interfaces));
        }

        public static void Remove(object instance)
        {
            m_objects.Remove(instance);
        }

        public static ObjectEntry[] Objects
        {
            get
            {
                int pos = 0;
                ObjectEntry[] o = new ObjectEntry[m_objects.Count];
                foreach (KeyValuePair<object, ObjectEntry> pair in m_objects)
                {
                    o[pos++] = pair.Value;
                }

                return o;
            }
        }

        public static ObjectEntry GetObjectByName(string name)
        {                
            foreach (KeyValuePair<object, ObjectEntry> pair in m_objects)
            {
                if (String.Compare(pair.Value.Name, name, true) == 0)
                {
                    return pair.Value;
                }
            }
            
            return null;
        }

        public static ObjectEntry GetObjectByGuid(Guid id)
        {
            foreach (KeyValuePair<object, ObjectEntry> pair in m_objects)
            {
                if (pair.Value.Id == id)
                {
                    return pair.Value;
                }
            }

            return null;
        }
    }
}
