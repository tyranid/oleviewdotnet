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

    public static class ComWrapperFactory
    {
        private static AssemblyName _name = new AssemblyName("ComWrapperTypes");
        private static AssemblyBuilder _builder = AppDomain.CurrentDomain.DefineDynamicAssembly(_name, AssemblyBuilderAccess.Run);
        private static ModuleBuilder _module = _builder.DefineDynamicModule(_name.Name);
        private static Dictionary<Guid, Type> _types = new Dictionary<Guid, Type>() { { typeof(IUnknown).GUID, typeof(IUnknownWrapper) } };

        public static BaseComWrapper<T> Wrap<T>(object obj)
        {
            return (BaseComWrapper<T>)Wrap(obj, typeof(T));
        }

        public static BaseComWrapper Wrap(object obj, Guid iid)
        {
            return Wrap(obj, COMUtilities.GetInterfaceType(iid));
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
                foreach (var mi in typeof(IClassFactory).GetMethods())
                {
                    string name = mi.Name;
                    switch (name)
                    {
                        case "QueryInterface":
                        case "Unwrap":
                            name = name + "_real";
                            break;
                    }
                    var methbuilder = tb.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Virtual, mi.ReturnType, mi.GetParameters().Select(p => p.ParameterType).ToArray());
                    var ilgen = methbuilder.GetILGenerator();
                    ilgen.Emit(OpCodes.Ldarg_0);
                    ilgen.Emit(OpCodes.Ldfld, base_type.GetField("_object", BindingFlags.Instance | BindingFlags.NonPublic));
                    for (int i = 0; i < mi.GetParameters().Length; ++i)
                    {
                        ilgen.Emit(OpCodes.Ldarg, i + 1);
                    }
                    ilgen.Emit(OpCodes.Callvirt, mi);
                    ilgen.Emit(OpCodes.Ret);
                }
                _types[intf_type.GUID] = tb.CreateType();
            }
            return (BaseComWrapper)Activator.CreateInstance(_types[intf_type.GUID], obj);
        }
    }
}
