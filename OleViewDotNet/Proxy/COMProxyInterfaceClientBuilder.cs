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

using Microsoft.CSharp;
using NtApiDotNet.Win32.Rpc;
using OleViewDotNet.Database;
using OleViewDotNet.Rpc;
using OleViewDotNet.TypeManager;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace OleViewDotNet.Proxy;

internal static class COMProxyInterfaceClientBuilder
{
    private const string ASSEMBLY_NAME = "ComProxyRpcTypes";
    private static readonly AssemblyName _name = new(ASSEMBLY_NAME);
    private static readonly AssemblyBuilder _builder = AppDomain.CurrentDomain.DefineDynamicAssembly(_name, AssemblyBuilderAccess.RunAndSave);
    private static readonly ModuleBuilder _module = _builder.DefineDynamicModule(_name.Name, _name.Name + ".dll");
    private static readonly ConcurrentDictionary<Guid, Type> m_types = new();
    private static readonly ConcurrentDictionary<Guid, Type> m_scripting_types = new();

    private static Type CreateRpcClientType(COMProxyInterface intf, bool scripting)
    {
        RpcClientBuilderArguments args = CreateBuilderArgs(intf, scripting);
        Type base_type = RpcClientBuilder.BuildAssembly(intf.RpcProxy, args, provider: new CSharpCodeProvider(), ignore_cache: true)
            .GetTypes().Where(t => typeof(RpcClientBase).IsAssignableFrom(t)).First();

        string type_name = $"{ASSEMBLY_NAME}.{Guid.NewGuid().ToString().Replace('-', '_')}";
        TypeBuilder tb = _module.DefineType(type_name,
                TypeAttributes.Public | TypeAttributes.Sealed, base_type);
        Type wrapper_intf = typeof(ICOMObjectWrapper);
        tb.AddInterfaceImplementation(wrapper_intf);

        var con = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(object), typeof(COMRegistry) });
        con.DefineParameter(1, ParameterAttributes.None, "obj");
        con.DefineParameter(2, ParameterAttributes.None, "registry");
        var conil = con.GetILGenerator();
        conil.Emit(OpCodes.Ldarg_0);
        conil.Emit(OpCodes.Call, base_type.GetConstructor(Type.EmptyTypes));

        var connect_mi = typeof(RpcComUtils).GetMethod(nameof(RpcComUtils.ConnectClient), BindingFlags.Static | BindingFlags.Public);
        conil.Emit(OpCodes.Ldarg_0);
        conil.Emit(OpCodes.Ldarg_1);
        conil.Emit(OpCodes.Ldarg_2);
        conil.Emit(OpCodes.Call, connect_mi);
        conil.Emit(OpCodes.Ret);

        MethodInfo unwrap_intf = wrapper_intf.GetMethod("Unwrap");
        var unwrap = tb.DefineMethod($"{wrapper_intf.FullName}.{unwrap_intf.Name}", MethodAttributes.Private |
            MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot, unwrap_intf.ReturnType, Type.EmptyTypes);
        var unwrapil = unwrap.GetILGenerator();
        var unwrap_mi = typeof(RpcComUtils).GetMethod(nameof(RpcComUtils.Unwrap), BindingFlags.Static | BindingFlags.Public);
        unwrapil.Emit(OpCodes.Ldarg_0);
        unwrapil.Emit(OpCodes.Call, unwrap_mi);
        unwrapil.Emit(OpCodes.Ret);
        tb.DefineMethodOverride(unwrap, unwrap_intf);

        PropertyInfo iid_pi = wrapper_intf.GetProperty("Iid");
        MethodInfo iid_mi = iid_pi.GetMethod;
        MethodInfo intf_id_mi = base_type.GetProperty("InterfaceId").GetMethod;
        var get_iid = tb.DefineMethod(iid_mi.Name, MethodAttributes.Private | MethodAttributes.Final |
            MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual |
            MethodAttributes.SpecialName, iid_mi.ReturnType, Type.EmptyTypes);
        var get_iidil = get_iid.GetILGenerator();
        get_iidil.Emit(OpCodes.Ldarg_0);
        get_iidil.Emit(OpCodes.Call, intf_id_mi);
        get_iidil.Emit(OpCodes.Ret);
        tb.DefineMethodOverride(get_iid, iid_mi);

        var get_iid_prop = tb.DefineProperty($"{wrapper_intf.FullName}.{iid_pi.Name}", 0, iid_pi.PropertyType, Type.EmptyTypes);
        get_iid_prop.SetGetMethod(get_iid);
        return tb.CreateType();
    }

    private static RpcClientBuilderArguments CreateBuilderArgs(COMProxyInterface intf, bool scripting)
    {
        RpcClientBuilderArguments args = new();
        args.Flags = RpcClientBuilderFlags.UnsignedChar |
            RpcClientBuilderFlags.NoNamespace | RpcClientBuilderFlags.ComObject;
        args.ClientName = $"{intf.Name.Replace('.', '_')}_RpcClient";
        if (scripting)
        {
            args.Flags |= RpcClientBuilderFlags.GenerateConstructorProperties |
                RpcClientBuilderFlags.StructureReturn |
                RpcClientBuilderFlags.HideWrappedMethods;
        }
        return args;
    }

    public static Type CreateClientType(COMProxyInterface intf, bool scripting)
    {
        var dict = scripting ? m_scripting_types : m_types;
        return dict.GetOrAdd(intf.Iid, _ => CreateRpcClientType(intf, scripting));
    }

    public static string BuildClientSource(COMProxyInterface intf, bool scripting)
    {
        var args = CreateBuilderArgs(intf, scripting);
        return RpcClientBuilder.BuildSource(intf.RpcProxy, args, provider: new CSharpCodeProvider());
    }

    public static void FlushIidType(Guid iid)
    {
        m_types.TryRemove(iid, out _);
        m_scripting_types.TryRemove(iid, out _);
    }
}