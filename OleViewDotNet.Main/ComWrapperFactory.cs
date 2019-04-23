//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace OleViewDotNet
{
    public abstract class BaseComWrapper
    {
        internal IEnumerable<COMInterfaceEntry> _interfaces;

        public string InterfaceName { get; }
        public Guid Iid { get; }
        public abstract BaseComWrapper QueryInterface(Guid iid);
        public abstract object Unwrap();

        protected BaseComWrapper(Guid iid, string name)
        {
            InterfaceName = name;
            Iid = iid;
        }
    }

    public abstract class BaseComWrapper<T> : BaseComWrapper, IDisposable
    {
        protected readonly T _object;

        protected BaseComWrapper(object obj) 
            : base(typeof(T).GUID, typeof(T).Name)
        {
            System.Diagnostics.Debug.Assert(typeof(T).IsInterface);
            _object = (T)obj;
        }

        public override BaseComWrapper QueryInterface(Guid iid)
        {
            return COMWrapperFactory.Wrap(_object, COMUtilities.GetInterfaceType(iid));
        }

        public override object Unwrap()
        {
            return _object;
        }

        public static implicit operator T(BaseComWrapper<T> wrapper)
        {
            return wrapper._object;
        }

        void IDisposable.Dispose()
        {
            Marshal.ReleaseComObject(_object);
        }
    }

    public class IUnknownWrapper : BaseComWrapper<IUnknown>
    {
        public IUnknownWrapper(object obj) : base(obj)
        {
        }
    }

    public class IClassFactoryWrapper : BaseComWrapper<IClassFactory>, IClassFactory
    {
        public IClassFactoryWrapper(object obj) : base(obj)
        {
        }

        public void CreateInstance(object pUnkOuter, ref Guid riid, out object ppvObject)
        {
            _object.CreateInstance(pUnkOuter, ref riid, out ppvObject);
        }

        public void LockServer(bool fLock)
        {
            _object.LockServer(fLock);
        }
    }

    public static class COMWrapperFactory
    {
        private static AssemblyName _name = new AssemblyName("ComWrapperTypes");
        private static AssemblyBuilder _builder = AppDomain.CurrentDomain.DefineDynamicAssembly(_name, AssemblyBuilderAccess.RunAndSave);
        private static ModuleBuilder _module = _builder.DefineDynamicModule(_name.Name, _name.Name + ".dll");
        private static Dictionary<Type, Type> _generated_intfs = new Dictionary<Type, Type>();
        private static Dictionary<Guid, Type> _types = new Dictionary<Guid, Type>() {
            { typeof(IUnknown).GUID, typeof(IUnknownWrapper) },
            { typeof(IClassFactory).GUID, typeof(IClassFactoryWrapper) } };

        private static bool FilterStructuredTypes(Type t)
        {
            if (!t.IsValueType || t.IsPrimitive || t == typeof(Guid) || t == typeof(void))
            {
                return false;
            }

            return true;
        }

        public static BaseComWrapper<T> Wrap<T>(object obj)
        {
            return (BaseComWrapper<T>)Wrap(obj, typeof(T));
        }

        public static BaseComWrapper Wrap(object obj, Guid iid)
        {
            return Wrap(obj, COMUtilities.GetInterfaceType(iid));
        }

        private static bool AddType(this HashSet<Type> types, Type t)
        {
            if (t.IsByRef)
            {
                t = t.GetElementType();
            }
            return types.Add(t);
        }

        private static string GenerateName(this HashSet<string> names, MemberInfo member)
        {
            string ret = member.Name;
            if (!names.Add(ret))
            {
                int count = 1;
                while (count < 1024)
                {
                    ret = $"{member.Name}_{count++}";
                    if (names.Add(ret))
                    {
                        break;
                    }
                }
                if (count == 1024)
                {
                    throw new ArgumentException($"Can't generate a unique name for {member.Name}");
                }
            }
            return ret;
        }

        private static MethodBuilder GenerateForwardingMethod(TypeBuilder tb, MethodInfo mi, MethodAttributes attributes, Type base_type, HashSet<Type> structured_types, HashSet<string> names)
        {
            string name = names.GenerateName(mi);
            Type[] param_types = mi.GetParameters().Select(p => p.ParameterType).ToArray();
            foreach (var t in param_types)
            {
                structured_types.AddType(t);
            }
            structured_types.AddType(mi.ReturnType);

            var methbuilder = tb.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Virtual | attributes, 
                mi.ReturnType, param_types);

            var ps = mi.GetParameters();
            for (int i = 0; i < ps.Length; ++i)
            {
                methbuilder.DefineParameter(i + 1, ps[i].Attributes, ps[i].Name);
            }
            var ilgen = methbuilder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldfld, base_type.GetField("_object", BindingFlags.Instance | BindingFlags.NonPublic));
            for (int i = 0; i < ps.Length; ++i)
            {
                ilgen.Emit(OpCodes.Ldarg, i + 1);
            }
            ilgen.Emit(OpCodes.Callvirt, mi);
            ilgen.Emit(OpCodes.Ret);
            return methbuilder;
        }

        private static bool IsComInterfaceType(Type intf_type)
        {
            return COMUtilities.IsComImport(intf_type) && intf_type.IsInterface && intf_type.IsPublic && !intf_type.Assembly.ReflectionOnly;
        }

        private static Type CreateType(Type intf_type, Queue<Tuple<Guid, TypeBuilder>> fixup_queue)
        {
            if (intf_type == null)
            {
                throw new ArgumentNullException("No interface type available", nameof(intf_type));
            }

            if (!IsComInterfaceType(intf_type))
            {
                throw new ArgumentException("Wrapper type must be a public COM interface and not reflection only.", nameof(intf_type));
            }

            HashSet<Type> structured_types = new HashSet<Type>();
            bool created_queue = false;
            if (fixup_queue == null)
            {
                fixup_queue = new Queue<Tuple<Guid, TypeBuilder>>();
                created_queue = true;
            }

            if (!_types.ContainsKey(intf_type.GUID))
            {
                Type base_type = typeof(BaseComWrapper<>).MakeGenericType(intf_type);
                TypeBuilder tb = _module.DefineType(
                    $"{intf_type.Name}Wrapper",
                     TypeAttributes.Public | TypeAttributes.Sealed, base_type);
                _types[intf_type.GUID] = tb;
                HashSet<string> names = new HashSet<string>(base_type.GetMembers().Select(m => m.Name));
                var con = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object) });
                con.DefineParameter(1, ParameterAttributes.In, "obj");
                var conil = con.GetILGenerator();
                conil.Emit(OpCodes.Ldarg_0);
                conil.Emit(OpCodes.Ldarg_1);
                conil.Emit(OpCodes.Call,
                    base_type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object) }, null));
                conil.Emit(OpCodes.Ret);
                foreach (var mi in intf_type.GetMethods().Where(m => (m.Attributes & MethodAttributes.SpecialName) == 0))
                {
                    GenerateForwardingMethod(tb, mi, 0, base_type, structured_types, names);
                }

                // TODO: Should change interface parameters to wrapper types.
                foreach (var pi in intf_type.GetProperties())
                {
                    string name = names.GenerateName(pi);
                    var pb = tb.DefineProperty(name, PropertyAttributes.None, pi.PropertyType, pi.GetIndexParameters().Select(p => p.ParameterType).ToArray());
                    if (pi.CanRead)
                    {
                        var get_method = GenerateForwardingMethod(tb, pi.GetMethod, MethodAttributes.HideBySig | MethodAttributes.SpecialName, base_type, structured_types, names);
                        pb.SetGetMethod(get_method);
                    }
                    if (pi.CanWrite)
                    {
                        var set_method = GenerateForwardingMethod(tb, pi.SetMethod, MethodAttributes.HideBySig | MethodAttributes.SpecialName, base_type, structured_types, names);
                        pb.SetSetMethod(set_method);
                    }
                }

                foreach (var type in structured_types.Where(FilterStructuredTypes))
                {
                    var methbuilder = tb.DefineMethod($"New_{type.Name}", MethodAttributes.Public | MethodAttributes.Static,
                            type, new Type[0]);
                    var ilgen = methbuilder.GetILGenerator();
                    ilgen.DeclareLocal(type);
                    ilgen.Emit(OpCodes.Ldloca_S, 0);
                    ilgen.Emit(OpCodes.Initobj, type);
                    ilgen.Emit(OpCodes.Ldloc_0);
                    ilgen.Emit(OpCodes.Ret);
                }

                fixup_queue.Enqueue(Tuple.Create(intf_type.GUID, tb));
            }

            if (created_queue)
            {
                while (fixup_queue.Count > 0)
                {
                    var entry = fixup_queue.Dequeue();
                    _types[entry.Item1] = entry.Item2.CreateType();
                }
            }

            return _types[intf_type.GUID];
        }

        public static BaseComWrapper Wrap(object obj, Type intf_type)
        {
            if (!Marshal.IsComObject(obj))
            {
                throw new ArgumentException("Object must be a COM object", nameof(obj));
            }

            return (BaseComWrapper)Activator.CreateInstance(CreateType(intf_type, null), obj);
        }

        public static object Unwrap(object obj)
        {
            if (obj is BaseComWrapper wrapper)
            {
                return wrapper.Unwrap();
            }
            return obj;
        }

        public static void DumpAssembly()
        {
            _builder.Save(_name.Name + ".dll");
        }
    }
}
