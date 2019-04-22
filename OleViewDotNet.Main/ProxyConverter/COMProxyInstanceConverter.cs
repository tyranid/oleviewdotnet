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

using NtApiDotNet.Ndr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Builder
{
    public class COMProxyInstanceConverter
    {
        private readonly AssemblyName m_name;
        private readonly AssemblyBuilder m_builder;
        private readonly ModuleBuilder m_module;
        private readonly Dictionary<Guid, Type> m_types;
        private readonly Queue<TypeBuilder> m_fixup;
        private readonly Dictionary<Guid, NdrComProxyDefinition> m_proxies;
        private readonly string m_output_path;

        private static CustomAttributeBuilder CreateAttribute<T>(params object[] args) where T : Attribute
        {
            var con = typeof(T).GetConstructor(args.Select(p => p.GetType()).ToArray());
            if (con == null)
            {
                throw new ArgumentException("Can't find suitable constructor");
            }

            return new CustomAttributeBuilder(con, args);
        }

        private class TypeDescriptor
        {
            public Type BuiltinType { get; }
            public List<CustomAttributeBuilder> CustomAttributes { get; }
            public ParameterAttributes Attributes { get; }
            public int PointerCount { get; }

            public TypeDescriptor(TypeDescriptor typeDescriptor)
            {
                BuiltinType = typeDescriptor.BuiltinType;
                CustomAttributes = new List<CustomAttributeBuilder>(typeDescriptor.CustomAttributes);
                Attributes = typeDescriptor.Attributes;
                PointerCount = typeDescriptor.PointerCount + 1;
            }

            public TypeDescriptor(Type builtinType, 
                NdrProcedureParameter pm,
                params CustomAttributeBuilder[] customAttributes)
            {
                ParameterAttributes attributes = ParameterAttributes.None;
                BuiltinType = builtinType;
                if (pm.Attributes.HasFlag(NdrParamAttributes.IsSimpleRef))
                {
                    PointerCount = 1;
                }

                CustomAttributes = new List<CustomAttributeBuilder>(customAttributes);
                if (pm.Attributes.HasFlag(NdrParamAttributes.IsIn))
                {
                    CustomAttributes.Add(CreateAttribute<InAttribute>());
                    attributes |= ParameterAttributes.In;
                }

                if (pm.Attributes.HasFlag(NdrParamAttributes.IsOut))
                {
                    CustomAttributes.Add(CreateAttribute<OutAttribute>());
                    attributes |= ParameterAttributes.Out;
                }
                Attributes = attributes;
            }

            public Type GetParameterType()
            {
                if (PointerCount == 0)
                {
                    return BuiltinType;
                }

                if (PointerCount == 1)
                {
                    if (BuiltinType.IsValueType)
                    {
                        return BuiltinType.MakeByRefType();
                    }
                    return BuiltinType;
                }

                if (PointerCount == 2 && !BuiltinType.IsValueType)
                {
                    return BuiltinType.MakeByRefType();
                }

                // Could implement as constant sized array.
                // Return a generic pointer for now.
                return typeof(IntPtr).MakeByRefType();
            }
        }

        private COMProxyInstanceConverter(string output_path)
        {
            m_output_path = output_path;
            m_name = new AssemblyName(Path.GetFileNameWithoutExtension(output_path));
            m_builder = AppDomain.CurrentDomain.DefineDynamicAssembly(m_name, AssemblyBuilderAccess.Save);
            m_module = m_builder.DefineDynamicModule(m_name.Name, m_name.Name + ".dll");
            m_types = new Dictionary<Guid, Type>();
            m_proxies = new Dictionary<Guid, NdrComProxyDefinition>();
            m_fixup = new Queue<TypeBuilder>();
        }

        public void Save()
        {
            m_builder.Save(m_output_path);
        }

        private CustomAttributeBuilder CreateMarshalAsAttribute(UnmanagedType unmanagedType)
        {
            return new CustomAttributeBuilder(typeof(MarshalAsAttribute).GetConstructor(new Type[] { typeof(UnmanagedType) }), new object[] { unmanagedType });
        }

        private TypeDescriptor GetTypeDescriptor(NdrProcedureParameter procParam, NdrBaseTypeReference baseType = null)
        {
            if (baseType == null)
            {
                baseType = procParam.Type;
            }
            NdrFormatCharacter format = baseType.Format;
            switch (format)
            {
                case NdrFormatCharacter.FC_BYTE:
                case NdrFormatCharacter.FC_USMALL:
                    return new TypeDescriptor(typeof(byte), procParam);
                case NdrFormatCharacter.FC_SMALL:
                case NdrFormatCharacter.FC_CHAR:
                    return new TypeDescriptor(typeof(sbyte), procParam);
                case NdrFormatCharacter.FC_WCHAR:
                    return new TypeDescriptor(typeof(char), procParam);
                case NdrFormatCharacter.FC_SHORT:
                    return new TypeDescriptor(typeof(short), procParam);
                case NdrFormatCharacter.FC_USHORT:
                    return new TypeDescriptor(typeof(ushort), procParam);
                case NdrFormatCharacter.FC_ENUM16:
                case NdrFormatCharacter.FC_LONG:
                case NdrFormatCharacter.FC_ENUM32:
                    return new TypeDescriptor(typeof(int), procParam);
                case NdrFormatCharacter.FC_ULONG:
                case NdrFormatCharacter.FC_ERROR_STATUS_T:
                    return new TypeDescriptor(typeof(uint), procParam);
                case NdrFormatCharacter.FC_FLOAT:
                    return new TypeDescriptor(typeof(float), procParam);
                case NdrFormatCharacter.FC_HYPER:
                    return new TypeDescriptor(typeof(long), procParam);
                case NdrFormatCharacter.FC_DOUBLE:
                    return new TypeDescriptor(typeof(double), procParam);
                case NdrFormatCharacter.FC_INT3264:
                    return new TypeDescriptor(typeof(IntPtr), procParam);
                case NdrFormatCharacter.FC_UINT3264:
                    return new TypeDescriptor(typeof(UIntPtr), procParam);
                case NdrFormatCharacter.FC_C_WSTRING:
                    return new TypeDescriptor(typeof(string), procParam, CreateMarshalAsAttribute(UnmanagedType.LPWStr));
                case NdrFormatCharacter.FC_C_CSTRING:
                    return new TypeDescriptor(typeof(string), procParam, CreateMarshalAsAttribute(UnmanagedType.LPStr));
            }

            if (baseType is NdrKnownTypeReference known_type)
            {
                switch (known_type.KnownType)
                {
                    case NdrKnownTypes.BSTR:
                        return new TypeDescriptor(typeof(string), procParam, CreateMarshalAsAttribute(UnmanagedType.BStr));
                    case NdrKnownTypes.GUID:
                        return new TypeDescriptor(typeof(Guid), procParam);
                    case NdrKnownTypes.HSTRING:
                        return new TypeDescriptor(typeof(string), procParam, CreateMarshalAsAttribute(UnmanagedType.HString));
                    case NdrKnownTypes.VARIANT:
                        return new TypeDescriptor(typeof(object), procParam, CreateMarshalAsAttribute(UnmanagedType.Struct));
                }
            }

            if (baseType is NdrPointerTypeReference pointer_type)
            {
                return new TypeDescriptor(GetTypeDescriptor(procParam, pointer_type.Type));
            }

            if (baseType is NdrInterfacePointerTypeReference interface_pointer)
            {
                if (interface_pointer.IsConstant && m_proxies.ContainsKey(interface_pointer.Iid))
                {
                    return new TypeDescriptor(CreateType(m_proxies[interface_pointer.Iid]), procParam, CreateMarshalAsAttribute(UnmanagedType.Interface));
                }
                return new TypeDescriptor(typeof(object), procParam, CreateMarshalAsAttribute(UnmanagedType.IUnknown));
            }

            return new TypeDescriptor(typeof(IntPtr), procParam);
        }

        private ComInterfaceType GetInterfaceTypeAndProcs(NdrComProxyDefinition proxy, List<NdrProcedureDefinition> procs)
        {
            ComInterfaceType type = ComInterfaceType.InterfaceIsIUnknown;
            if (proxy.BaseIid == COMInterfaceEntry.IID_IDispatch)
            {
                type = ComInterfaceType.InterfaceIsIDispatch;
            }
            else if (proxy.BaseIid == COMInterfaceEntry.IID_IInspectable)
            {
                type = ComInterfaceType.InterfaceIsIInspectable;
            }
            else if (m_proxies.ContainsKey(proxy.BaseIid))
            {
                type = GetInterfaceTypeAndProcs(m_proxies[proxy.BaseIid], procs);
            }

            procs.AddRange(proxy.Procedures);
            return type;
        }

        private CustomAttributeBuilder GetInterfaceTypeAttribute(NdrComProxyDefinition proxy, List<NdrProcedureDefinition> procs)
        {
            return CreateAttribute<InterfaceTypeAttribute>(GetInterfaceTypeAndProcs(proxy, procs));
        }

        private Type CreateType(NdrComProxyDefinition intf)
        {
            if (m_types.ContainsKey(intf.Iid))
            {
                return m_types[intf.Iid];
            }

            TypeBuilder tb = m_module.DefineType(
                        intf.Name,
                TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
            m_types[intf.Iid] = tb;
            tb.SetCustomAttribute(CreateAttribute<ComImportAttribute>());
            tb.SetCustomAttribute(CreateAttribute<GuidAttribute>(intf.Iid.ToString()));
            List<NdrProcedureDefinition> procs = new List<NdrProcedureDefinition>();
            tb.SetCustomAttribute(GetInterfaceTypeAttribute(intf, procs));

            foreach (var proc in procs)
            {
                string name = proc.Name;
                var ret_type = GetTypeDescriptor(proc.ReturnValue);
                var param_types = proc.Params.Select(p => GetTypeDescriptor(p)).ToList();
                var methbuilder = tb.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    ret_type.BuiltinType, param_types.Select(p => p.GetParameterType()).ToArray());
                for (int i = 0; i < param_types.Count; ++i)
                {
                    var param_builder = methbuilder.DefineParameter(i + 1, param_types[i].Attributes, $"p{i}");
                    foreach (var attr in param_types[i].CustomAttributes)
                    {
                        param_builder.SetCustomAttribute(attr);
                    }
                }
            }

            m_fixup.Enqueue(tb);

            return m_types[intf.Iid];
        }

        private void AddProxy(COMProxyInstance proxy)
        {
            foreach (var entry in proxy.Entries)
            {
                m_proxies.Add(entry.Iid, entry);
            }

            foreach (var entry in m_proxies.Values)
            {
                CreateType(entry);
            }

            while(m_fixup.Count > 0)
            {
                m_fixup.Dequeue().CreateType();
            }
        }

        public static Assembly Convert(COMProxyInstance proxy, string output_path)
        {
            COMProxyInstanceConverter converter = new COMProxyInstanceConverter(output_path);
            converter.AddProxy(proxy);
            converter.Save();
            return converter.m_builder;
        }
    }
}
