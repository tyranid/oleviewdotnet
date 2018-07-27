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
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet
{
    public abstract class BaseComWrapper
    {
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

    public abstract class BaseComWrapper<T> : BaseComWrapper
    {
        protected readonly T _object;

        protected BaseComWrapper(object obj) 
            : base(typeof(T).GUID, typeof(T).Name)
        {
            _object = (T)obj;
        }

        public override BaseComWrapper QueryInterface(Guid iid)
        {
            return ComWrapperFactory.Wrap(_object, COMUtilities.GetInterfaceType(iid));
        }

        public override object Unwrap()
        {
            return _object;
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

    public static class ComWrapperFactory
    {
        private static AssemblyName _name = new AssemblyName("ComWrapperTypes");
        private static AssemblyBuilder _builder = AppDomain.CurrentDomain.DefineDynamicAssembly(_name, AssemblyBuilderAccess.Run);
        private static ModuleBuilder _module = _builder.DefineDynamicModule(_name.Name);
        private static Dictionary<Type, Type> _generated_intfs = new Dictionary<Type, Type>();
        private static Dictionary<Guid, Type> _types = new Dictionary<Guid, Type>() {
            { typeof(IUnknown).GUID, typeof(IUnknownWrapper) },
            { typeof(IClassFactory).GUID, typeof(IClassFactoryWrapper) } };

        public static Type GenerateNonReflectionInterface(Type intf_type)
        {
            if (!intf_type.Assembly.ReflectionOnly)
            {
                return intf_type;
            }

            if (_generated_intfs.ContainsKey(intf_type))
            {
                return _generated_intfs[intf_type];
            }

            TypeBuilder tb = _module.DefineType(
                 intf_type.Name,
                 TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
            _generated_intfs[intf_type] = tb;

            var com_import_attr = new CustomAttributeBuilder(typeof(ComImportAttribute).GetConstructor(Type.EmptyTypes), new object[0]);
            tb.SetCustomAttribute(com_import_attr);
            var guid_attr = new CustomAttributeBuilder(typeof(GuidAttribute).GetConstructor(new Type[] { typeof(string) }), new object[] { intf_type.GUID.ToString() });
            tb.SetCustomAttribute(guid_attr);

            foreach (var mi in intf_type.GetMethods())
            {
                string name = mi.Name;
                var methbuilder = tb.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual, 
                    GenerateNonReflectionInterface(mi.ReturnType), mi.GetParameters().Select(p => GenerateNonReflectionInterface(p.ParameterType)).ToArray());
            }
            // TODO: Emit properties.
            _generated_intfs[intf_type] = tb.CreateType();
            return _generated_intfs[intf_type];
        }

        public static BaseComWrapper<T> Wrap<T>(object obj)
        {
            return (BaseComWrapper<T>)Wrap(obj, typeof(T));
        }

        public static BaseComWrapper Wrap(object obj, Guid iid)
        {
            return Wrap(obj, COMUtilities.GetInterfaceType(iid));
        }

        private static MethodBuilder GenerateForwardingMethod(TypeBuilder tb, MethodInfo mi, MethodAttributes attributes, Type base_type)
        {
            string name = mi.Name;
            switch (name)
            {
                case "QueryInterface":
                case "Unwrap":
                    name = name + "_real";
                    break;
            }
            var methbuilder = tb.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Virtual | attributes, 
                mi.ReturnType, mi.GetParameters().Select(p => p.ParameterType).ToArray());
            var ilgen = methbuilder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldfld, base_type.GetField("_object", BindingFlags.Instance | BindingFlags.NonPublic));
            for (int i = 0; i < mi.GetParameters().Length; ++i)
            {
                ilgen.Emit(OpCodes.Ldarg, i + 1);
            }
            ilgen.Emit(OpCodes.Callvirt, mi);
            ilgen.Emit(OpCodes.Ret);
            return methbuilder;
        }

        public static BaseComWrapper Wrap(object obj, Type intf_type)
        {
            if (intf_type == null)
            {
                throw new ArgumentNullException("No interface type available", nameof(intf_type));
            }

            if (!COMUtilities.IsComImport(intf_type) || !intf_type.IsInterface || !intf_type.IsPublic)
            {
                throw new ArgumentException("Wrapper type must be a public COM interface");
            }

            if (intf_type.Assembly.ReflectionOnly)
            {
                throw new ArgumentException("Interface type cant be reflection only", nameof(intf_type));
            }

            if (!Marshal.IsComObject(obj))
            {
                throw new ArgumentException("Object must be a COM object", nameof(obj));
            }

            if (!_types.ContainsKey(intf_type.GUID))
            {
                Type base_type = typeof(BaseComWrapper<>).MakeGenericType(intf_type);
                TypeBuilder tb = _module.DefineType(
                    $"{intf_type.Name}Wrapper",
                     TypeAttributes.Public | TypeAttributes.Sealed, base_type);
                tb.AddInterfaceImplementation(intf_type);
                var con = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object) });
                var conil = con.GetILGenerator();
                conil.Emit(OpCodes.Ldarg_0);
                conil.Emit(OpCodes.Ldarg_1);
                conil.Emit(OpCodes.Call,
                    base_type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object) }, null));
                conil.Emit(OpCodes.Ret);
                foreach (var mi in intf_type.GetMethods().Where(m => (m.Attributes & MethodAttributes.SpecialName) == 0))
                {
                    GenerateForwardingMethod(tb, mi, 0, base_type);
                }

                foreach (var pi in intf_type.GetProperties())
                {
                    var pb = tb.DefineProperty(pi.Name, PropertyAttributes.None, pi.PropertyType, pi.GetIndexParameters().Select(p => p.ParameterType).ToArray());
                    if (pi.CanRead)
                    {
                        var get_method = GenerateForwardingMethod(tb, pi.GetMethod, MethodAttributes.HideBySig | MethodAttributes.SpecialName, base_type);
                        pb.SetGetMethod(get_method);
                    }
                    if (pi.CanWrite)
                    {
                        var set_method = GenerateForwardingMethod(tb, pi.SetMethod, MethodAttributes.HideBySig | MethodAttributes.SpecialName, base_type);
                        pb.SetSetMethod(set_method);
                    }
                }

                _types[intf_type.GUID] = tb.CreateType();
            }
            return (BaseComWrapper)Activator.CreateInstance(_types[intf_type.GUID], obj);
        }

        public static object Unwrap(object obj)
        {
            if (obj is BaseComWrapper wrapper)
            {
                return wrapper.Unwrap();
            }
            return obj;
        }

    }
}
