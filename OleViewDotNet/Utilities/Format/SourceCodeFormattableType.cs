//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
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
using System.Runtime.InteropServices;
using System.Text;

namespace OleViewDotNet.Utilities.Format;

internal sealed class SourceCodeFormattableType : TypeDelegator, ICOMSourceCodeFormattable
{
    bool ICOMSourceCodeFormattable.IsFormattable => true;

    private static string RemoveGenericPart(string name)
    {
        int index = name.LastIndexOf('`');
        if (index >= 0)
            return name.Substring(0, index);
        return name;
    }

    private static string ConvertTypeToName(Type t)
    {
        if (t == typeof(string))
        {
            return "string";
        }
        else if (t == typeof(byte))
        {
            return "byte";
        }
        else if (t == typeof(sbyte))
        {
            return "sbyte";
        }
        else if (t == typeof(short))
        {
            return "short";
        }
        else if (t == typeof(ushort))
        {
            return "ushort";
        }
        else if (t == typeof(int))
        {
            return "int";
        }
        else if (t == typeof(uint))
        {
            return "uint";
        }
        else if (t == typeof(long))
        {
            return "long";
        }
        else if (t == typeof(ulong))
        {
            return "ulong";
        }
        else if (t == typeof(void))
        {
            return "void";
        }
        else if (t == typeof(object))
        {
            return "object";
        }
        else if (t == typeof(bool))
        {
            return "bool";
        }
        else if (t.IsConstructedGenericType)
        {
            StringBuilder builder = new();
            builder.Append(RemoveGenericPart(t.Name));
            builder.Append('<');
            builder.Append(string.Join(",", t.GenericTypeArguments.Select(g => ConvertTypeToName(g))));
            builder.Append('>');
            return builder.ToString();
        }

        return t.Name;
    }

    private static string FormatParameters(IEnumerable<ParameterInfo> pis)
    {
        List<string> pars = new();
        foreach (ParameterInfo pi in pis)
        {
            List<string> dirs = new();

            if (pi.IsOut)
            {
                dirs.Add("Out");
                if (pi.IsIn)
                {
                    dirs.Add("In");
                }
            }

            if (pi.IsRetval)
            {
                dirs.Add("Retval");
            }

            if (pi.IsOptional)
            {
                dirs.Add("Optional");
            }

            string text = $"{ConvertTypeToName(pi.ParameterType)} {pi.Name}";

            if (dirs.Count > 0)
            {
                text = $"[{string.Join(",", dirs)}] {text}";
            }
            pars.Add(text);
        }
        return string.Join(", ", pars);
    }

    private static string MemberInfoToString(MemberInfo member)
    {
        if (member is MethodInfo mi)
        {
            return $"{ConvertTypeToName(mi.ReturnType)} {mi.Name}({FormatParameters(mi.GetParameters())});";
        }
        else if (member is PropertyInfo prop)
        {
            List<string> propdirs = new();
            if (prop.CanRead)
            {
                propdirs.Add("get;");
            }

            if (prop.CanWrite)
            {
                propdirs.Add("set;");
            }

            ParameterInfo[] index_params = prop.GetIndexParameters();
            string ps = string.Empty;
            if (index_params.Length > 0)
            {
                ps = $"({FormatParameters(index_params)})";
            }

            return $"{ConvertTypeToName(prop.PropertyType)} {prop.Name}{ps} {{ {string.Join(" ", propdirs)} }}";
        }
        else if (member is FieldInfo fi)
        {
            return $"{ConvertTypeToName(fi.FieldType)} {fi.Name};";
        }
        else if (member is EventInfo ei)
        {
            return $"event {ConvertTypeToName(ei.EventHandlerType)} {ei.Name};";
        }
        else
        {
            return null;
        }
    }

    private static void EmitMember(COMSourceCodeBuilder builder, MemberInfo mi)
    {
        string name = MemberInfoToString(mi);
        if (!string.IsNullOrWhiteSpace(name))
        {
            if (mi is FieldInfo)
            {
                builder.AppendLine($"{name};");
            }
            else
            {
                builder.AppendLine(name);
            }
        }
    }

    private static Dictionary<MethodInfo, string> MapMethodNamesToCOM(IEnumerable<MethodInfo> mis)
    {
        HashSet<string> matched_names = new();
        Dictionary<MethodInfo, string> ret = new();
        foreach (MethodInfo mi in mis.Reverse())
        {
            int count = 2;
            string name = mi.Name;
            while (!matched_names.Add(name))
            {
                name = $"{mi.Name}_{count++}";
            }
            ret.Add(mi, name);
        }
        return ret;
    }

    private static Dictionary<string, object> GetEnumValues(Type enum_type)
    {
        return enum_type.GetFields(BindingFlags.Public | BindingFlags.Static).ToDictionary(e => e.Name, e => e.GetRawConstantValue());
    }

    private static void FormatComType(COMSourceCodeBuilder builder, Type t)
    {
        try
        {
            if (t.IsInterface)
            {
                builder.AppendLine($"[Guid(\"{t.GUID}\")]");
                builder.AppendLine($"interface {t.Name}");
            }
            else if (t.IsEnum)
            {
                builder.AppendLine($"enum {t.Name}");
            }
            else if (t.IsClass)
            {
                builder.AppendLine($"[Guid(\"{t.GUID}\")]");
                ClassInterfaceAttribute class_attr = t.GetCustomAttribute<ClassInterfaceAttribute>();
                if (class_attr is not null)
                {
                    builder.AppendLine($"[ClassInterface(ClassInterfaceType.{class_attr.Value})]");
                }
                builder.AppendLine($"class {t.Name}");
            }
            else
            {
                builder.AppendLine($"struct {t.Name}");
            }
            builder.AppendLine("{");

            if (t.IsInterface || t.IsClass)
            {
                MethodInfo[] methods = t.GetMethods().Where(
                    m => !m.IsStatic && (m.Attributes & MethodAttributes.SpecialName) == 0).ToArray();
                if (methods.Length > 0)
                {
                    using (builder.PushIndent(4))
                    {
                        builder.AppendCommentLine("/* Methods */");
                        Dictionary<MethodInfo, string> name_mapping = new();
                        if (t.IsClass)
                        {
                            name_mapping = MapMethodNamesToCOM(methods);
                        }

                        foreach (MethodInfo mi in methods)
                        {
                            if (name_mapping.ContainsKey(mi) && name_mapping[mi] != mi.Name)
                            {
                                builder.AppendCommentLine($"/* Exposed as {name_mapping[mi]} */");
                            }

                            EmitMember(builder, mi);
                        }
                    }
                }

                var props = t.GetProperties().Where(p => !(p.GetMethod?.IsStatic ?? false));
                if (props.Any())
                {
                    using (builder.PushIndent(4))
                    {
                        builder.AppendCommentLine("/* Properties */");
                        foreach (PropertyInfo pi in props)
                        {
                            EmitMember(builder, pi);
                        }
                    }
                }

                var evs = t.GetEvents();
                if (evs.Length > 0)
                {
                    using (builder.PushIndent(4))
                    {
                        builder.AppendCommentLine("/* Events */");
                        foreach (EventInfo ei in evs)
                        {
                            EmitMember(builder, ei);
                        }
                    }
                }
            }
            else if (t.IsEnum)
            {
                using (builder.PushIndent(4))
                {
                    foreach (var pair in GetEnumValues(t))
                    {
                        try
                        {
                            builder.AppendLine($"{pair.Key} = {pair.Value},");
                        }
                        catch
                        {
                            builder.AppendLine($"{pair.Key},");
                        }
                    }
                }
            }
            else
            {
                FieldInfo[] fields = t.GetFields();
                if (fields.Length > 0)
                {
                    using (builder.PushIndent(4))
                    {
                        builder.AppendCommentLine("/* Fields */");
                        foreach (FieldInfo fi in fields)
                        {
                            EmitMember(builder, fi);
                        }
                    }
                }
            }

            builder.AppendLine("}");
        }
        catch (InvalidOperationException)
        {
        }
    }

    public SourceCodeFormattableType(Type type) : base(type)
    {
    }

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        FormatComType(builder, typeImpl);
    }
}
