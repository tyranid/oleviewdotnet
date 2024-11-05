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

using NtApiDotNet.Win32.Rpc;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.TypeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;

namespace OleViewDotNetPS.Wrappers;

public static class COMWrapperFactory
{
    #region Private Members
    private const string ASSEMBLY_NAME = "ComWrapperTypes";
    private static readonly AssemblyName _name = new(ASSEMBLY_NAME);
    private static readonly AssemblyBuilder _builder = AppDomain.CurrentDomain.DefineDynamicAssembly(_name, AssemblyBuilderAccess.RunAndSave);
    private static readonly ModuleBuilder _module = _builder.DefineDynamicModule(_name.Name, _name.Name + ".dll");
    private static readonly Dictionary<Guid, Type> _types = new() {
        { typeof(IUnknown).GUID, typeof(IUnknownWrapper) },
        { typeof(IClassFactory).GUID, typeof(IClassFactoryWrapper) },
        { typeof(IActivationFactory).GUID, typeof(IActivationFactoryWrapper) },
        { typeof(IMoniker).GUID, typeof(IMonikerWrapper) },
        { typeof(IBindCtx).GUID, typeof(IBindCtxWrapper) },
        { typeof(IEnumMoniker).GUID, typeof(IEnumMonikerWrapper) },
        { typeof(IEnumString).GUID, typeof(IEnumStringWrapper) },
        { typeof(IRunningObjectTable).GUID, typeof(IRunningObjectTableWrapper) },
        { typeof(IStream).GUID, typeof(IStreamWrapper) },
        { typeof(IInspectable).GUID, typeof(IInspectableWrapper) },
        { typeof(IStorage).GUID, typeof(IStorageWrapper) },
    };
    private static readonly Dictionary<Guid, Type> _public_types = new();
    private static readonly MethodInfo _unwrap_method = typeof(COMWrapperFactory).GetMethod("UnwrapTyped");
    private static readonly Dictionary<Type, ConstructorInfo> _constructors = new();
    private static readonly MethodInfo _get_builder_method = typeof(BaseComReflectionWrapper).GetMethod("Get", BindingFlags.NonPublic | BindingFlags.Instance,
        null, new[] { typeof(int) }, null);
    private static readonly MethodInfo _get_method = typeof(MethodInfoWrapper).GetMethod("Get");
    private static readonly MethodInfo _get_ret_method = typeof(MethodInfoWrapper).GetMethod("GetRet");
    private static readonly MethodInfo _set_ref_method = typeof(MethodInfoWrapper).GetMethod("SetRef");
    private static readonly MethodInfo _set_method = typeof(MethodInfoWrapper).GetMethod("Set");
    private static readonly MethodInfo _invoke_method = typeof(MethodInfoWrapper).GetMethod("Invoke");

    private static bool FilterStructuredTypes(Type t)
    {
        if (!t.IsValueType || t.IsPrimitive || t == typeof(Guid) || t == typeof(void))
        {
            return false;
        }

        if (t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return false;
        }

        if (t.Assembly == typeof(RpcClientBase).Assembly)
        {
            return false;
        }

        return true;
    }

    private static bool AddType(this HashSet<Type> types, Type t)
    {
        if (t.IsByRef)
        {
            t = t.GetElementType();
        }
        return types.Add(UnwrapType(t));
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

    private static Type Deref(this Type t)
    {
        if (!t.IsByRef)
        {
            return t;
        }
        return t.GetElementType();
    }

    private static Type GetParameterType(this ParameterInfo pi, Queue<Tuple<Guid, TypeBuilder>> fixup_queue)
    {
        Type ret = pi.ParameterType;
        if (!pi.IsOut && ret.IsByRef)
        {
            Type value_type = ret.GetElementType();
            if (value_type.IsValueType)
            {
                return value_type;
            }
        }
        else if (IsComInterfaceType(pi.ParameterType))
        {
            Type type = pi.ParameterType.Deref();
            ret = CreateType(type, type.GUID, fixup_queue);
            if (pi.ParameterType.IsByRef)
            {
                ret = ret.MakeByRefType();
            }
        }
        return ret;
    }

    private static ConstructorInfo GetConstructor(Type type)
    {
        type = type.Deref();
        if (!_constructors.ContainsKey(type))
        {
            _constructors[type] = type.GetConstructor(new[] { typeof(object) });
        }
        return _constructors[type];
    }

    private static Type UnwrapType(Type type)
    {
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return type.GenericTypeArguments[0];
        }

        return type;
    }

    private static MethodBuilder GenerateForwardingMethod(TypeBuilder tb, MethodInfo mi, MethodAttributes attributes,
        Type base_type, HashSet<Type> structured_types, HashSet<string> names, Queue<Tuple<Guid, TypeBuilder>> fixup_queue)
    {
        string name = names.GenerateName(mi);
        var param_info = mi.GetParameters();
        var param_types = param_info.Select(p => p.GetParameterType(fixup_queue)).ToArray();
        foreach (var t in param_types)
        {
            structured_types.AddType(t);
        }
        structured_types.AddType(mi.ReturnType);

        var methbuilder = tb.DefineMethod(name, MethodAttributes.Public | MethodAttributes.HideBySig | attributes,
            mi.ReturnType, param_types);

        for (int i = 0; i < param_info.Length; ++i)
        {
            methbuilder.DefineParameter(i + 1, param_info[i].Attributes, param_info[i].Name);
        }

        List<Tuple<int, int>> locals = new();
        var ilgen = methbuilder.GetILGenerator();
        ilgen.Emit(OpCodes.Ldarg_0);
        ilgen.Emit(OpCodes.Ldfld, base_type.GetField("_object", BindingFlags.Instance | BindingFlags.NonPublic));
        for (int i = 0; i < param_info.Length; ++i)
        {
            if (param_info[i].ParameterType.IsByRef && !param_types[i].IsByRef)
            {
                ilgen.Emit(OpCodes.Ldarga, i + 1);
            }
            else
            {
                if (typeof(BaseComWrapper).IsAssignableFrom(param_types[i].Deref()))
                {
                    if (!param_info[i].IsOut)
                    {
                        ilgen.Emit(OpCodes.Ldarg, i + 1);
                        ilgen.Emit(OpCodes.Call, _unwrap_method.MakeGenericMethod(param_info[i].ParameterType));
                        ilgen.Emit(OpCodes.Castclass, param_info[i].ParameterType);
                    }
                    else
                    {
                        LocalBuilder local = ilgen.DeclareLocal(param_info[i].ParameterType.Deref());
                        ilgen.Emit(OpCodes.Ldloca, local.LocalIndex);
                        locals.Add(Tuple.Create(i, local.LocalIndex));
                    }
                }
                else
                {
                    ilgen.Emit(OpCodes.Ldarg, i + 1);
                }
            }
        }
        ilgen.Emit(OpCodes.Callvirt, mi);
        foreach (var local in locals)
        {
            var con = GetConstructor(param_types[local.Item1]);
            ilgen.Emit(OpCodes.Ldarg, local.Item1 + 1);
            ilgen.Emit(OpCodes.Ldloc, local.Item2);
            ilgen.Emit(OpCodes.Newobj, con);
            ilgen.Emit(OpCodes.Stind_Ref);
        }
        ilgen.Emit(OpCodes.Ret);
        return methbuilder;
    }

    private static bool IsComInterfaceType(Type intf_type)
    {
        return COMTypeManager.IsComInterfaceType(intf_type.Deref());
    }

    private static void CreateWrapperType(Type intf_type, Guid iid, Queue<Tuple<Guid, TypeBuilder>> fixup_queue)
    {
        HashSet<Type> structured_types = new();
        Type base_type = typeof(BaseComWrapper<>).MakeGenericType(intf_type);
        string type_name = $"{ASSEMBLY_NAME}.{Guid.NewGuid().ToString().Replace('-', '_')}";
        TypeBuilder tb = _module.DefineType(type_name,
             TypeAttributes.Public | TypeAttributes.Sealed, base_type);
        _types[iid] = tb;
        HashSet<string> names = new(base_type.GetMembers().Select(m => m.Name));
        var con = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object), typeof(COMRegistry) });
        _constructors[tb] = con;
        con.DefineParameter(1, ParameterAttributes.None, "obj");
        con.DefineParameter(2, ParameterAttributes.None, "registry");
        var conil = con.GetILGenerator();
        conil.Emit(OpCodes.Ldarg_0);
        conil.Emit(OpCodes.Ldarg_1);
        conil.Emit(OpCodes.Ldarg_2);
        conil.Emit(OpCodes.Call,
            base_type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object), typeof(COMRegistry) }, null));
        conil.Emit(OpCodes.Ret);
        foreach (var mi in intf_type.GetMethods().Where(m => (m.Attributes & MethodAttributes.SpecialName) == 0))
        {
            GenerateForwardingMethod(tb, mi, 0, base_type, structured_types, names, fixup_queue);
        }

        foreach (var pi in intf_type.GetProperties())
        {
            string name = names.GenerateName(pi);
            var pb = tb.DefineProperty(name, PropertyAttributes.None, pi.PropertyType, pi.GetIndexParameters().Select(p => p.ParameterType).ToArray());
            if (pi.CanRead)
            {
                var get_method = GenerateForwardingMethod(tb, pi.GetMethod, MethodAttributes.SpecialName, base_type, structured_types, names, fixup_queue);
                pb.SetGetMethod(get_method);
            }
            if (pi.CanWrite)
            {
                var set_method = GenerateForwardingMethod(tb, pi.SetMethod, MethodAttributes.SpecialName, base_type, structured_types, names, fixup_queue);
                pb.SetSetMethod(set_method);
            }
        }

        foreach (var type in structured_types.Where(FilterStructuredTypes))
        {
            var methbuilder = tb.DefineMethod($"New_{type.Name}", MethodAttributes.Public,
                    type, new Type[0]);
            var ilgen = methbuilder.GetILGenerator();
            var local = ilgen.DeclareLocal(type);
            ilgen.Emit(OpCodes.Ldloca_S, local.LocalIndex);
            ilgen.Emit(OpCodes.Initobj, type);
            ilgen.Emit(OpCodes.Ldloc_0);
            ilgen.Emit(OpCodes.Ret);
        }

        fixup_queue.Enqueue(Tuple.Create(iid, tb));
    }

    private static Type GetReflectionType(this Type type)
    {
        if (type.Deref().IsPublic)
        {
            return type;
        }
        
        if (type.IsByRef)
        {
            return typeof(object).MakeByRefType();
        }
        return typeof(object);
    }

    private static MethodBuilder GenerateReflectionMethod(TypeBuilder tb, MethodInfo mi, int index, MethodAttributes attributes,
        HashSet<string> names, Queue<Tuple<Guid, TypeBuilder>> fixup_queue, Dictionary<MethodInfo, MethodBuilder> method_map)
    {
        string name = names.GenerateName(mi);
        var param_info = mi.GetParameters();
        var param_types = param_info.Select(p => p.ParameterType.GetReflectionType()).ToArray();

        var methbuilder = tb.DefineMethod(name, MethodAttributes.Public | MethodAttributes.HideBySig | attributes,
            mi.ReturnType.GetReflectionType(), param_types);

        method_map.Add(mi, methbuilder);

        for (int i = 0; i < param_info.Length; ++i)
        {
            methbuilder.DefineParameter(i + 1, param_info[i].Attributes, param_info[i].Name);
        }

        var ilgen = methbuilder.GetILGenerator();
        ilgen.Emit(OpCodes.Ldarg_0);
        ilgen.Emit(OpCodes.Ldc_I4, index);
        ilgen.Emit(OpCodes.Call, _get_builder_method);
        for (int i = 0; i < param_info.Length; ++i)
        {
            if (!param_info[i].IsOut)
            {
                ilgen.Emit(OpCodes.Dup);
                ilgen.Emit(OpCodes.Ldc_I4, i);
                ilgen.Emit(OpCodes.Ldarg, i + 1);
                MethodInfo set_method = param_types[i].IsByRef ? _set_ref_method : _set_method;
                ilgen.Emit(OpCodes.Call, set_method.MakeGenericMethod(param_types[i].Deref()));
            }
        }
        ilgen.Emit(OpCodes.Dup);
        ilgen.Emit(OpCodes.Call, _invoke_method);
        for (int i = 0; i < param_info.Length; ++i)
        {
            if (param_types[i].IsByRef)
            {
                ilgen.Emit(OpCodes.Dup);
                ilgen.Emit(OpCodes.Ldc_I4, i);
                ilgen.Emit(OpCodes.Ldarg, i + 1);
                ilgen.Emit(OpCodes.Call, _get_method.MakeGenericMethod(param_types[i].Deref()));
            }
        }
        if (methbuilder.ReturnType != typeof(void))
        {
            ilgen.Emit(OpCodes.Call, _get_ret_method.MakeGenericMethod(methbuilder.ReturnType));
        }
        else
        {
            ilgen.Emit(OpCodes.Pop);
        }
        ilgen.Emit(OpCodes.Ret);
        return methbuilder;
    }

    private static void CreateReflectionWrapperType(Type intf_type, Guid iid, Queue<Tuple<Guid, TypeBuilder>> fixup_queue)
    {
        Type base_type = typeof(BaseComReflectionWrapper);
        string type_name = $"{ASSEMBLY_NAME}.{Guid.NewGuid().ToString().Replace('-', '_')}";
        TypeBuilder tb = _module.DefineType(type_name,
             TypeAttributes.Public | TypeAttributes.Sealed, base_type);
        _types[iid] = tb;

        HashSet<string> names = new(base_type.GetMembers().Select(m => m.Name));
        Dictionary<MethodInfo, MethodBuilder> method_map = new();
        var con = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object), typeof(COMRegistry) });
        _constructors[tb] = con;
        con.DefineParameter(1, ParameterAttributes.None, "obj");
        con.DefineParameter(2, ParameterAttributes.None, "registry");
        var conil = con.GetILGenerator();
        conil.Emit(OpCodes.Ldarg_0);
        conil.Emit(OpCodes.Ldarg_1);
        conil.Emit(OpCodes.Ldstr, intf_type.AssemblyQualifiedName);
        conil.Emit(OpCodes.Ldarg_2);
        conil.Emit(OpCodes.Call,
            base_type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object), typeof(string), typeof(COMRegistry) }, null));
        conil.Emit(OpCodes.Ret);
        MethodInfo[] methods = intf_type.GetMethods();
        for (int i = 0; i < methods.Length; ++i)
        {
            GenerateReflectionMethod(tb, methods[i], i, methods[i].Attributes & MethodAttributes.SpecialName, names, fixup_queue, method_map);
        }

        foreach (var pi in intf_type.GetProperties())
        {
            string name = names.GenerateName(pi);
            var pb = tb.DefineProperty(name, PropertyAttributes.None, pi.PropertyType, pi.GetIndexParameters().Select(p => p.ParameterType).ToArray());
            if (pi.CanRead)
            {
                pb.SetGetMethod(method_map[pi.GetMethod]);
            }
            if (pi.CanWrite)
            {
                pb.SetSetMethod(method_map[pi.SetMethod]);
            }
        }

        fixup_queue.Enqueue(Tuple.Create(iid, tb));
    }

    private static Type CreateType(Type intf_type, Guid iid, Queue<Tuple<Guid, TypeBuilder>> fixup_queue)
    {
        if (intf_type is null)
        {
            throw new ArgumentNullException("No interface type available", nameof(intf_type));
        }

        if (!IsComInterfaceType(intf_type))
        {
            throw new ArgumentException("Wrapper type must be a COM interface and not reflection only.", nameof(intf_type));
        }

        bool created_queue = false;
        if (fixup_queue is null)
        {
            fixup_queue = new Queue<Tuple<Guid, TypeBuilder>>();
            created_queue = true;
        }

        if (!_types.ContainsKey(iid))
        {
            if (!intf_type.IsPublic)
            {
                CreateReflectionWrapperType(intf_type, iid, fixup_queue);
            }
            else
            {
                CreateWrapperType(intf_type, iid, fixup_queue);
            }
        }

        if (created_queue)
        {
            while (fixup_queue.Count > 0)
            {
                var entry = fixup_queue.Dequeue();
                _types[entry.Item1] = entry.Item2.CreateType();
            }
        }

        return _types[iid];
    }
    #endregion

    #region Public Static Members
    public static T UnwrapTyped<T>(this BaseComWrapper<T> obj) where T : class
    {
        return (T)obj?.Unwrap();
    }

    public static void DumpAssembly()
    {
        _builder.Save(_name.Name + ".dll");
    }

    private class ScriptingFactory : ICOMObjectWrapperScriptingFactory
    {
        public Type CreateType(Type intf_type, Guid iid)
        {
            if (!intf_type.IsInterface)
            {
                return intf_type;
            }
            return COMWrapperFactory.CreateType(intf_type, iid, null);
        }
    }

    public static void EnableScripting()
    {
        COMTypeManager.SetScriptingFactory(new ScriptingFactory());
    }
    #endregion
}
