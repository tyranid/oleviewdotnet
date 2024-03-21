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
using OleViewDotNet.Interop;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OleViewDotNet.Proxy;

public sealed class COMProxyFileConverter
{
    private readonly AssemblyName m_name;
    private readonly AssemblyBuilder m_builder;
    private readonly ModuleBuilder m_module;
    private readonly Dictionary<Guid, Type> m_types;
    private readonly Queue<TypeBuilder> m_fixup;
    private readonly Dictionary<Guid, NdrComProxyDefinition> m_proxies;
    private readonly Dictionary<NdrBaseStructureTypeReference, Type> m_structs;
    private readonly string m_output_path;
    private readonly IProgress<Tuple<string, int>> m_progress;

    private static readonly Regex _identifier_regex = new(@"[^a-zA-Z0-9_\.]");

    public static string MakeIdentifier(string id)
    {
        id = _identifier_regex.Replace(id, "_");
        if (!char.IsLetter(id[0]) && id[0] != '_')
        {
            id = "_" + id;
        }

        return id;
    }

    private static CustomAttributeBuilder CreateAttribute<T>(params object[] args) where T : Attribute
    {
        var con = typeof(T).GetConstructor(args.Select(p => p.GetType()).ToArray());
        if (con is null)
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
            NdrParamAttributes attributes,
            params CustomAttributeBuilder[] customAttributes)
        {
            ParameterAttributes real_attributes = ParameterAttributes.None;
            BuiltinType = builtinType;
            if (attributes.HasFlag(NdrParamAttributes.IsSimpleRef))
            {
                PointerCount = 1;
            }

            CustomAttributes = new List<CustomAttributeBuilder>(customAttributes);
            if (attributes.HasFlag(NdrParamAttributes.IsIn))
            {
                CustomAttributes.Add(CreateAttribute<InAttribute>());
                real_attributes |= ParameterAttributes.In;
            }

            if (attributes.HasFlag(NdrParamAttributes.IsOut))
            {
                CustomAttributes.Add(CreateAttribute<OutAttribute>());
                real_attributes |= ParameterAttributes.Out;
            }
            Attributes = real_attributes;
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

    private void ReportProgress(string progress)
    {
        m_progress?.Report(Tuple.Create(progress, -1));
    }

    public COMProxyFileConverter(string output_path, IProgress<Tuple<string, int>> progress)
    {
        m_output_path = Path.GetFullPath(output_path);
        m_name = new AssemblyName(Path.GetFileNameWithoutExtension(output_path));
        m_builder = AppDomain.CurrentDomain.DefineDynamicAssembly(m_name, AssemblyBuilderAccess.Save, Path.GetDirectoryName(output_path));
        m_module = m_builder.DefineDynamicModule(m_name.Name, m_name.Name + ".dll");
        m_types = new Dictionary<Guid, Type>();
        m_proxies = new Dictionary<Guid, NdrComProxyDefinition>();
        m_fixup = new Queue<TypeBuilder>();
        m_structs = new Dictionary<NdrBaseStructureTypeReference, Type>();
        m_progress = progress;
    }

    public void Save()
    {
        m_builder.Save(Path.GetFileName(m_output_path));
    }

    public Assembly BuiltAssembly => m_builder;

    private CustomAttributeBuilder CreateMarshalAsAttribute(UnmanagedType unmanagedType)
    {
        return new CustomAttributeBuilder(typeof(MarshalAsAttribute).GetConstructor(new Type[] { typeof(UnmanagedType) }), new object[] { unmanagedType });
    }

    private TypeDescriptor GetTypeDescriptor(NdrProcedureParameter procParam)
    {
        return GetTypeDescriptor(procParam.Attributes, procParam.Type);
    }

    private TypeDescriptor GetTypeDescriptor(NdrParamAttributes paramAttributes, NdrBaseTypeReference baseType, bool field_type = false)
    {
        if (baseType is NdrSimpleTypeReference || baseType is NdrBaseStringTypeReference)
        {
            switch (baseType.Format)
            {
                case NdrFormatCharacter.FC_BYTE:
                case NdrFormatCharacter.FC_USMALL:
                    return new TypeDescriptor(typeof(byte), paramAttributes);
                case NdrFormatCharacter.FC_SMALL:
                case NdrFormatCharacter.FC_CHAR:
                    return new TypeDescriptor(typeof(sbyte), paramAttributes);
                case NdrFormatCharacter.FC_WCHAR:
                    return new TypeDescriptor(typeof(char), paramAttributes);
                case NdrFormatCharacter.FC_SHORT:
                    return new TypeDescriptor(typeof(short), paramAttributes);
                case NdrFormatCharacter.FC_USHORT:
                    return new TypeDescriptor(typeof(ushort), paramAttributes);
                case NdrFormatCharacter.FC_ENUM16:
                case NdrFormatCharacter.FC_LONG:
                case NdrFormatCharacter.FC_ENUM32:
                    return new TypeDescriptor(typeof(int), paramAttributes);
                case NdrFormatCharacter.FC_ULONG:
                case NdrFormatCharacter.FC_ERROR_STATUS_T:
                    return new TypeDescriptor(typeof(uint), paramAttributes);
                case NdrFormatCharacter.FC_FLOAT:
                    return new TypeDescriptor(typeof(float), paramAttributes);
                case NdrFormatCharacter.FC_HYPER:
                    return new TypeDescriptor(typeof(long), paramAttributes);
                case NdrFormatCharacter.FC_DOUBLE:
                    return new TypeDescriptor(typeof(double), paramAttributes);
                case NdrFormatCharacter.FC_INT3264:
                    return new TypeDescriptor(typeof(IntPtr), paramAttributes);
                case NdrFormatCharacter.FC_UINT3264:
                    return new TypeDescriptor(typeof(UIntPtr), paramAttributes);
                case NdrFormatCharacter.FC_C_WSTRING:
                    return new TypeDescriptor(typeof(string), paramAttributes, CreateMarshalAsAttribute(UnmanagedType.LPWStr));
                case NdrFormatCharacter.FC_C_CSTRING:
                    return new TypeDescriptor(typeof(string), paramAttributes, CreateMarshalAsAttribute(UnmanagedType.LPStr));
            }
        }
        else if (baseType is NdrKnownTypeReference known_type)
        {
            switch (known_type.KnownType)
            {
                case NdrKnownTypes.BSTR:
                    return new TypeDescriptor(typeof(string), paramAttributes, CreateMarshalAsAttribute(UnmanagedType.BStr));
                case NdrKnownTypes.GUID:
                    return new TypeDescriptor(typeof(Guid), paramAttributes);
                case NdrKnownTypes.HSTRING:
                    return new TypeDescriptor(typeof(string), paramAttributes, CreateMarshalAsAttribute(UnmanagedType.HString));
                case NdrKnownTypes.VARIANT:
                    return new TypeDescriptor(typeof(object), paramAttributes, CreateMarshalAsAttribute(UnmanagedType.Struct));
            }
        }
        else if (baseType is NdrPointerTypeReference pointer_type)
        {
            if (field_type)
                return new TypeDescriptor(typeof(IntPtr), paramAttributes);
            return new TypeDescriptor(GetTypeDescriptor(paramAttributes, pointer_type.Type));
        }
        else if (baseType is NdrInterfacePointerTypeReference interface_pointer)
        {
            TypeDescriptor intf_p;
            if (interface_pointer.IsConstant && m_proxies.ContainsKey(interface_pointer.Iid))
            {
                intf_p = new TypeDescriptor(CreateInterface(m_proxies[interface_pointer.Iid]), paramAttributes, CreateMarshalAsAttribute(UnmanagedType.Interface));
            }
            else
            {
                intf_p = new TypeDescriptor(typeof(object), paramAttributes, CreateMarshalAsAttribute(UnmanagedType.IUnknown));
            }
            // Interface pointer should have a pointer value of 1.
            return new TypeDescriptor(intf_p);
        }
        else if (baseType is NdrBaseStructureTypeReference struct_type)
        {
            return new TypeDescriptor(CreateStruct(struct_type), paramAttributes);
        }

        if (field_type)
        {
            return null;
        }
        return new TypeDescriptor(typeof(IntPtr), paramAttributes);
    }

    private ComInterfaceType GetInterfaceTypeAndProcs(NdrComProxyDefinition proxy, List<NdrProcedureDefinition> procs)
    {
        ComInterfaceType type = ComInterfaceType.InterfaceIsIUnknown;
        if (proxy.BaseIid == COMKnownGuids.IID_IDispatch)
        {
            type = ComInterfaceType.InterfaceIsIDispatch;
        }
        else if (proxy.BaseIid == COMKnownGuids.IID_IInspectable)
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

    private Type CreateInterface(NdrComProxyDefinition intf)
    {
        if (m_types.ContainsKey(intf.Iid))
        {
            return m_types[intf.Iid];
        }

        Type existing_type = COMUtilities.GetInterfaceType(intf.Iid);
        if (existing_type is not null)
        {
            m_types[intf.Iid] = existing_type;
            return existing_type;
        }

        ReportProgress($"Creating Interface {intf.Name}");

        TypeBuilder tb = m_module.DefineType(
                    MakeIdentifier(intf.Name),
            TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);
        m_types[intf.Iid] = tb;
        tb.SetCustomAttribute(CreateAttribute<ComImportAttribute>());
        tb.SetCustomAttribute(CreateAttribute<GuidAttribute>(intf.Iid.ToString()));
        List<NdrProcedureDefinition> procs = new();
        tb.SetCustomAttribute(GetInterfaceTypeAttribute(intf, procs));

        foreach (var proc in procs)
        {
            string name = MakeIdentifier(proc.Name);
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

    private Type CreateStruct(NdrBaseStructureTypeReference struct_type)
    {
        if (m_structs.ContainsKey(struct_type))
        {
            return m_structs[struct_type];
        }

        ReportProgress($"Creating Type {struct_type.Name}");

        TypeBuilder tb = m_module.DefineType(
                    MakeIdentifier(struct_type.Name),
            TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.Sealed);
        tb.SetParent(typeof(ValueType));
        m_structs[struct_type] = tb;

        foreach (var member in struct_type.Members)
        {
            var type_desc = GetTypeDescriptor(0, member.MemberType, true);
            if (type_desc is null)
            {
                break;
            }
            var field = tb.DefineField(MakeIdentifier(member.Name), type_desc.BuiltinType, FieldAttributes.Public);
            foreach (var attr in type_desc.CustomAttributes)
            {
                field.SetCustomAttribute(attr);
            }
        }

        m_fixup.Enqueue(tb);
        return m_structs[struct_type];
    }

    public void AddProxy(IEnumerable<COMProxyInterface> entries)
    {
        foreach (var entry in entries)
        {
            m_proxies.Add(entry.Iid, entry.Entry);
        }

        foreach (var entry in entries)
        {
            CreateInterface(entry.Entry);
        }

        var fixup = m_fixup.ToArray();
        for (int i = 0; i < fixup.Length; ++i)
        {
            fixup[i].CreateType();
        }
    }

    public void AddProxy(COMProxyFile proxy)
    {
        AddProxy(proxy.Entries);
    }
}
