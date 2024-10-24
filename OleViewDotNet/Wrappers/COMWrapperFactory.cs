﻿//    This file is part of OleViewDotNet.
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
using OleViewDotNet.Processes;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;

namespace OleViewDotNet.Wrappers;

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
        { typeof(IInspectable).GUID, typeof(IInspectableWrapper) }
    };
    private static readonly Dictionary<Guid, Type> _public_types = new();
    private static readonly MethodInfo _unwrap_method = typeof(COMWrapperFactory).GetMethod("UnwrapTyped");
    private static readonly Dictionary<Type, ConstructorInfo> _constructors = new();

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
            ret = CreateType(type, fixup_queue);
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
        intf_type = intf_type.Deref();
        return COMTypeManager.IsComImport(intf_type) && intf_type.IsInterface && !intf_type.Assembly.ReflectionOnly;
    }

    private static bool IsDefined(Type intf_type, MemberInfo member)
    {
        if (!typeof(RpcClientBase).IsAssignableFrom(intf_type))
        {
            return true;
        }
        return member.DeclaringType == intf_type;
    }

    private static Type CreateType(Type intf_type, Queue<Tuple<Guid, TypeBuilder>> fixup_queue)
    {
        if (intf_type is null)
        {
            throw new ArgumentNullException("No interface type available", nameof(intf_type));
        }

        bool is_rpc_client = typeof(RpcClientBase).IsAssignableFrom(intf_type);
        if (!is_rpc_client)
        {
            if (IsComInterfaceType(intf_type))
            {
                intf_type = CreatePublicInterface(intf_type, null);
            }
            else
            {
                throw new ArgumentException("Wrapper type must be a COM interface or an RPC client and not reflection only.", nameof(intf_type));
            }
        }

        HashSet<Type> structured_types = new();
        bool created_queue = false;
        if (fixup_queue is null)
        {
            fixup_queue = new Queue<Tuple<Guid, TypeBuilder>>();
            created_queue = true;
        }

        if (!_types.ContainsKey(intf_type.GUID))
        {
            Type base_type = is_rpc_client ? typeof(BaseComRpcWrapper<>).MakeGenericType(intf_type) : typeof(BaseComWrapper<>).MakeGenericType(intf_type);
            string type_name = intf_type.FullName;
            if (!type_name.StartsWith($"{ASSEMBLY_NAME}."))
            {
                type_name = $"{ASSEMBLY_NAME}." + type_name;
            }
            TypeBuilder tb = _module.DefineType($"{type_name}Wrapper",
                 TypeAttributes.Public | TypeAttributes.Sealed, base_type);
            _types[intf_type.GUID] = tb;
            HashSet<string> names = new(base_type.GetMembers().Select(m => m.Name));
            var con = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object) });
            _constructors[tb] = con;
            con.DefineParameter(1, ParameterAttributes.In, "obj");
            var conil = con.GetILGenerator();
            conil.Emit(OpCodes.Ldarg_0);
            conil.Emit(OpCodes.Ldarg_1);
            conil.Emit(OpCodes.Call,
                base_type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object) }, null));
            conil.Emit(OpCodes.Ret);
            foreach (var mi in intf_type.GetMethods().Where(m => (m.Attributes & MethodAttributes.SpecialName) == 0))
            {
                if (!IsDefined(intf_type, mi))
                {
                    continue;
                }
                GenerateForwardingMethod(tb, mi, 0, base_type, structured_types, names, fixup_queue);
            }

            foreach (var pi in intf_type.GetProperties())
            {
                if (!IsDefined(intf_type, pi))
                {
                    continue;
                }
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

            if (EnableScripting && !is_rpc_client)
            {
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

    private static void AddCustomAttribute<T>(this TypeBuilder builder, params object[] args) where T : Attribute
    {
        ConstructorInfo con = typeof(T).GetConstructor(args.Select(a => a.GetType()).ToArray());
        builder.SetCustomAttribute(new(con, args));
    }

    private static Type AddInterfaceType(this Queue<Tuple<Guid, TypeBuilder>> fixup_queue, Type type)
    {
        bool by_ref = type.IsByRef;
        type = type.Deref();

        if (IsComInterfaceType(type) && !type.IsPublic)
        {
            type = CreatePublicInterface(type, fixup_queue);
        }
        return by_ref ? type.MakeByRefType() : type;
    }

    public static Type CreatePublicInterface(Type intf_type, Queue<Tuple<Guid, TypeBuilder>> fixup_queue)
    {
        if (intf_type is null)
        {
            throw new ArgumentNullException("No interface type available", nameof(intf_type));
        }

        if (intf_type.IsPublic)
        {
            return intf_type;
        }

        HashSet<Type> structured_types = new();
        bool created_queue = false;
        if (fixup_queue is null)
        {
            fixup_queue = new();
            created_queue = true;
        }

        if (!_public_types.ContainsKey(intf_type.GUID))
        {
            TypeBuilder tb = _module.DefineType($"{ASSEMBLY_NAME}.{intf_type.FullName}", TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
            _public_types[intf_type.GUID] = tb;
            tb.AddCustomAttribute<ComImportAttribute>();
            tb.AddCustomAttribute<GuidAttribute>(intf_type.GUID.ToString());
            tb.AddCustomAttribute<InterfaceTypeAttribute>(ComInterfaceType.InterfaceIsIInspectable);
            Dictionary<string, MethodBuilder> method_cache = new();
            foreach (var member in intf_type.GetMembers())
            {
                if (member is MethodInfo method)
                {
                    var ps = method.GetParameters();
                    MethodBuilder m_builder = tb.DefineMethod(method.Name, method.Attributes, fixup_queue.AddInterfaceType(method.ReturnType), ps.Select(m => fixup_queue.AddInterfaceType(m.ParameterType)).ToArray());
                    for (int i = 0; i < ps.Length; ++i)
                    {
                        m_builder.DefineParameter(i + 1, ps[i].Attributes, ps[i].Name);
                    }
                    method_cache[method.Name] = m_builder;
                }
                else if (member is PropertyInfo property)
                {
                    var p_builder = tb.DefineProperty(property.Name, property.Attributes, fixup_queue.AddInterfaceType(property.PropertyType), 
                        property.GetIndexParameters().Select(p => fixup_queue.AddInterfaceType(p.ParameterType)).ToArray());
                    if (property.CanRead)
                    {
                        p_builder.SetGetMethod(method_cache[property.GetGetMethod().Name]);
                    }
                    if (property.CanWrite)
                    {
                        p_builder.SetSetMethod(method_cache[property.GetSetMethod().Name]);
                    }
                }
                else if (member is EventInfo ev)
                {
                    var e_builder = tb.DefineEvent(ev.Name, ev.Attributes, fixup_queue.AddInterfaceType(ev.EventHandlerType));
                    var ev_add = ev.GetAddMethod();
                    if (ev_add is not null)
                    {
                        e_builder.SetAddOnMethod(method_cache[ev_add.Name]);
                    }
                    var ev_rem = ev.GetRemoveMethod();
                    if (ev_rem is not null)
                    {
                        e_builder.SetRemoveOnMethod(method_cache[ev_rem.Name]);
                    }
                }
            }

            fixup_queue.Enqueue(Tuple.Create(intf_type.GUID, tb));
            if (created_queue)
            {
                while (fixup_queue.Count > 0)
                {
                    var entry = fixup_queue.Dequeue();
                    _public_types[entry.Item1] = entry.Item2.CreateType();
                }
            }
        }

        return _public_types[intf_type.GUID];
    }

    private static BaseComWrapper Wrap(object obj, Type intf_type, COMRegistry registry)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (intf_type is null)
        {
            throw new ArgumentNullException(nameof(intf_type), "No type available for wrapper.");
        }

        if (!Marshal.IsComObject(obj))
        {
            throw new ArgumentException("Object must be a COM object or assignable from interface type.", nameof(obj));
        }

        Type type = CreateType(intf_type, null);
        if (typeof(BaseComRpcWrapper).IsAssignableFrom(type) && !COMUtilities.IsProxy(obj))
        {
            type = typeof(IUnknownWrapper);
        }

        var wrapper = (BaseComWrapper)Activator.CreateInstance(type, obj);
        wrapper.SetDatabase(registry);
        return wrapper;
    }
    #endregion

    #region Public Static Members
    public static BaseComWrapper<T> Wrap<T>(object obj) where T : class
    {
        return (BaseComWrapper<T>)Wrap(obj, typeof(T));
    }

    public static BaseComWrapper Wrap(object obj, Guid iid, COMRegistry registry = null)
    {
        return Wrap(obj, COMTypeManager.GetInterfaceType(iid, registry, EnableScripting), registry);
    }

    public static BaseComWrapper Wrap(object obj, COMInterfaceEntry intf)
    {
        return Wrap(obj, COMTypeManager.GetInterfaceType(intf, EnableScripting), intf.Database);
    }

    public static BaseComWrapper Wrap(object obj, COMInterfaceInstance intf)
    {
        return Wrap(obj, COMTypeManager.GetInterfaceType(intf.InterfaceEntry, EnableScripting), intf.Database);
    }

    public static BaseComWrapper Wrap(object obj, COMIPIDEntry ipid)
    {
        return Wrap(obj, COMTypeManager.GetInterfaceType(ipid, EnableScripting), null);
    }

    public static BaseComWrapper Wrap(object obj, Type intf_type)
    {
        return Wrap(obj, intf_type, null);
    }

    public static object Unwrap(object obj)
    {
        if (obj is BaseComWrapper wrapper)
        {
            return wrapper.Unwrap();
        }
        return obj;
    }

    public static T UnwrapTyped<T>(this BaseComWrapper<T> obj) where T : class
    {
        return (T)obj?.Unwrap();
    }

    public static void DumpAssembly()
    {
        _builder.Save(_name.Name + ".dll");
    }

    public static bool EnableScripting { get; set; }

    internal static void FlushProxyType(Guid iid)
    {
        _types.Remove(iid);
        COMTypeManager.FlushIidType(iid);
    }
    #endregion
}
