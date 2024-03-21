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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

namespace OleViewDotNet.Utilities;

public sealed class RuntimeMetadata
{
    #region Private Members
    private readonly Dictionary<Guid, Type> m_interfaces;
    private readonly Dictionary<string, Type> m_classes;
    private readonly List<Assembly> m_assemblies = new();

    private static readonly Dictionary<string, Assembly> _cached_reflection_assemblies = new();
    private static readonly Lazy<RuntimeMetadata> m_metadata = new(CreateMetadata);

    private RuntimeMetadata() 
        : this(new(), new(), new())
    {
    }

    private RuntimeMetadata(Dictionary<Guid, Type> interfaces, Dictionary<string, Type> classes, List<Assembly> assemblies)
    {
        m_interfaces = interfaces;
        m_classes = classes;
        m_assemblies = assemblies;
    }

    private static RuntimeMetadata LoadMetadata()
    {
        string base_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WinMetaData");
        if (!Directory.Exists(base_path))
        {
            return null;
        }

        Dictionary<Guid, Type> interfaces = new();
        Dictionary<string, Type> classes = new();
        List<Assembly> assemblies = new();

        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (s, a) => CurrentDomain_ReflectionOnlyAssemblyResolve(base_path, a);
        WindowsRuntimeMetadata.ReflectionOnlyNamespaceResolve += (s, a) => WindowsRuntimeMetadata_ReflectionOnlyNamespaceResolve(base_path, a);
        DirectoryInfo dir = new(base_path);
        foreach (FileInfo file in dir.GetFiles("*.winmd"))
        {
            try
            {
                Assembly asm = Assembly.ReflectionOnlyLoadFrom(file.FullName);
                Type type = asm.GetTypes().FirstOrDefault();
                if (type is null)
                {
                    continue;
                }
                // Convert to a non-reflection only assembly.
                asm = Type.GetType(type.AssemblyQualifiedName)?.Assembly ?? asm;
                assemblies.Add(asm);
                Type[] types = asm.GetTypes();
                foreach (Type t in types)
                {
                    if (t.IsInterface)
                    {
                        foreach (var attr in t.GetCustomAttributesData())
                        {
                            if (attr.AttributeType.FullName == "Windows.Foundation.Metadata.GuidAttribute")
                            {
                                interfaces[t.GUID] = t;
                            }
                        }
                    }
                    else if (t.IsClass && t.IsPublic)
                    {
                        classes[t.FullName] = t;
                    }
                }
            }
            catch
            {
            }
        }
        return new RuntimeMetadata(interfaces, classes, assemblies);
    }

    private static RuntimeMetadata CreateMetadata()
    {
        try
        {
            return LoadMetadata();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return new();
    }

    private static Assembly ResolveAssembly(string base_path, string name)
    {
        if (_cached_reflection_assemblies.ContainsKey(name))
        {
            return _cached_reflection_assemblies[name];
        }

        Assembly asm;
        if (name.Contains(","))
        {
            asm = Assembly.ReflectionOnlyLoad(name);
        }
        else
        {
            string full_path = Path.Combine(base_path, $"{name}.winmd");
            if (File.Exists(full_path))
            {
                asm = Assembly.ReflectionOnlyLoadFrom(full_path);
            }
            else
            {
                int last_index = name.LastIndexOf('.');
                if (last_index < 0)
                {
                    return null;
                }
                asm = ResolveAssembly(base_path, name.Substring(0, last_index));
            }
        }

        _cached_reflection_assemblies[name] = asm;
        return _cached_reflection_assemblies[name];
    }

    private static void WindowsRuntimeMetadata_ReflectionOnlyNamespaceResolve(string base_path, NamespaceResolveEventArgs e)
    {
        e.ResolvedAssemblies.Add(ResolveAssembly(base_path, e.NamespaceName));
    }

    private static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(string base_path, ResolveEventArgs args)
    {
        return ResolveAssembly(base_path, args.Name);
    }
    #endregion

    #region Public Static Members
    public static IReadOnlyList<Assembly> Assemblies => m_metadata.Value.m_assemblies.AsReadOnly();
    public static IReadOnlyDictionary<Guid, Type> Interfaces => m_metadata.Value.m_interfaces;
    public static IReadOnlyDictionary<string, Type> Classes => m_metadata.Value.m_classes;
    #endregion
}