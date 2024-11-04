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

using OleViewDotNet.Interop;
using OleViewDotNet.TypeManager;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OleViewDotNet.Utilities;

/// <summary>
/// Class to access the global assembly cache.
/// </summary>
public static class GlobalAssemblyCache
{
    private static readonly Lazy<List<Assembly>> m_assemblies = new(LoadAssemblies);
    private static readonly Lazy<Dictionary<Guid, Type>> m_intfs = new(LoadInterfaces);

    private static List<Assembly> LoadAssemblies()
    {
        List<Assembly> ret = new();
        try
        {
            foreach (var name in GetNames())
            {
                try
                {
                    ret.Add(Assembly.Load(name));
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
        return ret;
    }

    private static Dictionary<Guid, Type> LoadInterfaces()
    {
        Dictionary<Guid, Type> ret = new();
        foreach (var asm in m_assemblies.Value)
        {
            foreach (var type in asm.GetExportedTypes())
            {
                if (type.IsInterface && COMTypeManager.IsComImport(type) && 
                    type.GetCustomAttribute<ObsoleteAttribute>() == null && !ret.ContainsKey(type.GUID))
                {
                    ret.Add(type.GUID, type);
                }
            }
        }
        return ret;
    }

    /// <summary>
    /// Get the list of assembly names in the cache.
    /// </summary>
    /// <returns>The list of assembly names.</returns>
    public static IEnumerable<AssemblyName> GetNames()
    {
        NativeMethods.CreateAssemblyEnum(out IAssemblyEnum e, null, null, ASM_CACHE_FLAGS.ASM_CACHE_GAC, IntPtr.Zero);
        while (e.GetNextAssembly(IntPtr.Zero, out IAssemblyName name, 0) == 0)
        {
            StringBuilder builder = new(1000);
            int len = 1000;

            name.GetDisplayName(builder, ref len, ASM_DISPLAY_FLAGS.ASM_DISPLAYF_FULL);
            builder.Length = len;
            yield return new AssemblyName(builder.ToString());
        }
    }

    public static IReadOnlyList<Assembly> Assemblies => m_assemblies.Value.AsReadOnly();
    public static IReadOnlyDictionary<Guid, Type> Interfaces => m_intfs.Value;
}