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

public abstract class BaseComWrapper<T> : BaseComWrapper, IDisposable where T : class
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

    void IDisposable.Dispose()
    {
        Marshal.ReleaseComObject(_object);
    }
}

public static class COMWrapperFactory
{
    private static readonly AssemblyName _name = new("ComWrapperTypes");
    private static readonly AssemblyBuilder _builder = AppDomain.CurrentDomain.DefineDynamicAssembly(_name, AssemblyBuilderAccess.RunAndSave);
    private static readonly ModuleBuilder _module = _builder.DefineDynamicModule(_name.Name, _name.Name + ".dll");
    private static readonly Dictionary<Type, Type> _generated_intfs = new();
    private static readonly Dictionary<Guid, Type> _types = new() {
        { typeof(IUnknown).GUID, typeof(IUnknownWrapper) },
        { typeof(IClassFactory).GUID, typeof(IClassFactoryWrapper) },
        { typeof(IActivationFactory).GUID, typeof(IActivationFactoryWrapper) },
        { typeof(IMoniker).GUID, typeof(IMonikerWrapper) },
        { typeof(IBindCtx).GUID, typeof(IBindCtxWrapper) },
        { typeof(IEnumMoniker).GUID, typeof(IEnumMonikerWrapper) },
        { typeof(IEnumString).GUID, typeof(IEnumStringWrapper) },
        { typeof(IRunningObjectTable).GUID, typeof(IRunningObjectTableWrapper) },
        { typeof(IStream).GUID, typeof(IStreamWrapper) } };
    private static readonly MethodInfo _unwrap_method = typeof(COMWrapperFactory).GetMethod("UnwrapTyped");
    private static readonly Dictionary<Type, ConstructorInfo> _constructors = new();

    private static bool FilterStructuredTypes(Type t)
    {
        if (!t.IsValueType || t.IsPrimitive || t == typeof(Guid) || t == typeof(void))
        {
            return false;
        }

        return true;
    }

    public static BaseComWrapper<T> Wrap<T>(object obj) where T : class
    {
        return (BaseComWrapper<T>)Wrap(obj, typeof(T));
    }

    public static BaseComWrapper Wrap(object obj, Guid iid, COMRegistry registry)
    {
        return Wrap(obj, COMUtilities.GetInterfaceType(iid, registry));
    }

    public static BaseComWrapper Wrap(object obj, Guid iid)
    {
        return Wrap(obj, iid, null);
    }

    public static BaseComWrapper Wrap(object obj, COMInterfaceEntry intf)
    {
        return Wrap(obj, COMUtilities.GetInterfaceType(intf));
    }

    public static BaseComWrapper Wrap(object obj, COMInterfaceInstance intf)
    {
        return Wrap(obj, COMUtilities.GetInterfaceType(intf.InterfaceEntry));
    }

    public static BaseComWrapper Wrap(object obj, COMIPIDEntry ipid)
    {
        return Wrap(obj, COMUtilities.GetInterfaceType(ipid));
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
        foreach(var local in locals)
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
        if (intf_type.IsByRef)
        {
            intf_type = intf_type.GetElementType();
        }
        return COMUtilities.IsComImport(intf_type) && intf_type.IsInterface && intf_type.IsPublic && !intf_type.Assembly.ReflectionOnly;
    }

    private static Type CreateType(Type intf_type, Queue<Tuple<Guid, TypeBuilder>> fixup_queue)
    {
        if (intf_type is null)
        {
            throw new ArgumentNullException("No interface type available", nameof(intf_type));
        }

        if (!IsComInterfaceType(intf_type))
        {
            throw new ArgumentException("Wrapper type must be a public COM interface and not reflection only.", nameof(intf_type));
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
            Type base_type = typeof(BaseComWrapper<>).MakeGenericType(intf_type);
            TypeBuilder tb = _module.DefineType(
                $"{intf_type.Name}Wrapper",
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
                var methbuilder = tb.DefineMethod($"New_{type.Name}", MethodAttributes.Public | MethodAttributes.Static,
                        type, new Type[0]);
                var ilgen = methbuilder.GetILGenerator();
                var local = ilgen.DeclareLocal(type);
                ilgen.Emit(OpCodes.Ldloca_S, local.LocalIndex);
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
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (intf_type is null)
        {
            throw new ArgumentNullException(nameof(intf_type), "No type available for wrapper.");
        }

        if (!Marshal.IsComObject(obj) && !intf_type.IsAssignableFrom(obj.GetType()))
        {
            throw new ArgumentException("Object must be a COM object or assignable from interface type.", nameof(obj));
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

    public static T UnwrapTyped<T>(this BaseComWrapper<T> obj) where T : class
    {
        return (T)obj?.Unwrap();
    }

    public static void DumpAssembly()
    {
        _builder.Save(_name.Name + ".dll");
    }
}
