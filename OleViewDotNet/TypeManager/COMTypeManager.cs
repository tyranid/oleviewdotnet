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
using OleViewDotNet.Proxy;
using OleViewDotNet.TypeLib.Instance;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeManager;

public static class COMTypeManager
{
    #region Private Members
    private static readonly Lazy<bool> m_loadtypes = new(LoadTypes);
    private static readonly ConcurrentDictionary<Guid, Assembly> m_typelibs = new();
    private static readonly ConcurrentDictionary<string, Assembly> m_typelibsname = new();
    private static readonly ConcurrentDictionary<Guid, Type> m_iidtypes = new();
    private static ICOMObjectWrapperScriptingFactory m_factory;

    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        if (m_typelibsname.TryGetValue(args.Name, out Assembly asm))
        {
            return asm;
        }
        return null;
    }

    private static bool ScriptingEnabled => m_factory is not null;

    private static bool LoadTypes()
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        try
        {
            string strTypeLibDir = ProgramSettings.GetTypeLibDirectory();
            Directory.CreateDirectory(strTypeLibDir);
            string[] files = Directory.GetFiles(strTypeLibDir, "*.dll");

            foreach (string f in files)
            {
                try
                {
                    Assembly a = Assembly.LoadFrom(f);
                    Guid typelib_guid = Marshal.GetTypeLibGuidForAssembly(a);
                    if (m_typelibs.TryAdd(typelib_guid, a))
                    {
                        m_typelibsname[a.FullName] = a;
                    }

                    LoadTypes(a);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.ToString());
        }

        LoadTypes(Assembly.GetExecutingAssembly());
        LoadTypes(typeof(int).Assembly);
        return true;
    }

    private static void LoadTypes(Assembly asm)
    {
        foreach (Type t in asm.GetTypes().Where(x => x.IsPublic && x.IsInterface && IsComImport(x)))
        {
            if (t.GetCustomAttribute<ObsoleteAttribute>() is not null)
            {
                continue;
            }
            m_iidtypes.TryAdd(t.GUID, t);
        }
    }

    private static ICOMObjectWrapper Wrap(object obj, Guid iid, Type type, COMRegistry registry)
    {
        if (obj is ICOMObjectWrapper obj_wrapper)
        {
            obj = obj_wrapper.Unwrap();
        }

        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (!Marshal.IsComObject(obj))
        {
            throw new ArgumentException("Object must be a COM object.", nameof(obj));
        }

        if (!COMUtilities.SupportsInterface(obj, iid))
        {
            throw new ArgumentException($"Object doesn't support interface {iid}.", nameof(iid));
        }

        if (type is null)
        {
            return new COMObjectWrapper(obj, iid, null, null);
        }
        type = m_factory?.CreateType(type, iid) ?? type;
        try
        {
            if (typeof(ICOMObjectWrapper).IsAssignableFrom(type))
            {
                return (ICOMObjectWrapper)Activator.CreateInstance(type, obj, registry);
            }
        }
        catch
        {
        }
        return new COMObjectWrapper(obj, iid, type, registry);
    }
    #endregion

    #region Public Static Methods
    public static bool IsComImport(Type t)
    {
        return t.GetCustomAttributes(typeof(ComImportAttribute), false).Length > 0 ||
            t.GetCustomAttributes(typeof(InterfaceTypeAttribute), false).Length > 0;
    }

    public static bool IsComInterfaceType(Type t)
    {
        return IsComImport(t) && t.IsInterface && !t.Assembly.ReflectionOnly;
    }

    public static Type GetInterfaceType(Guid iid, COMRegistry registry)
    {
        if (registry is not null && registry.Interfaces.ContainsKey(iid))
        {
            return GetInterfaceType(registry.Interfaces[iid]);
        }

        return GetInterfaceType(iid);
    }

    public static Type GetInterfaceType(Guid iid)
    {
        if (m_loadtypes.Value && m_iidtypes.ContainsKey(iid))
        {
            return m_iidtypes[iid];
        }

        return null;
    }

    public static Type GetInterfaceType(COMInterfaceEntry intf)
    {
        if (intf is null)
        {
            return null;
        }

        Type type = GetInterfaceType(intf.Iid);
        if (type is not null)
        {
            return type;
        }

        if (intf.HasTypeLib)
        {
            using var type_lib = COMTypeLibInstance.FromFile(intf.TypeLibVersionEntry.NativePath);
            using var type_info = type_lib.GetTypeInfoOfGuid(intf.Iid);
            return m_iidtypes.GetOrAdd(intf.Iid, _ => type_info.ToType());
        }

        if (intf.HasRuntimeType && intf.TryGetRuntimeType(out Type runtime_type))
        {
            return runtime_type;
        }

        if (intf.ProxyClassEntry is null)
        {
            return null;
        }

        var proxy = COMProxyInterface.GetFromIID(intf, intf.HasTypeLib);
        return m_iidtypes.GetOrAdd(intf.Iid, _ => proxy.CreateClientType(ScriptingEnabled));
    }

    public static Type GetInterfaceType(COMIPIDEntry ipid)
    {
        if (ipid is null)
        {
            return null;
        }

        Type type = GetInterfaceType(ipid.Iid);
        if (type is not null)
        {
            return type;
        }

        COMProxyFile proxy = ipid.ToProxyInstance();
        if (proxy is null)
        {
            return null;
        }

        m_iidtypes.TryAdd(ipid.Iid, proxy.Entries.Where(e => e.Iid == ipid.Iid).First().CreateClientType(ScriptingEnabled));
        return GetInterfaceType(ipid.Iid);
    }

    public static void LoadTypesFromAssembly(Assembly assembly, bool copy_to_cache = false)
    {
        LoadTypes(assembly);
        if (copy_to_cache)
        {
            string asm_path = assembly.Location;
            string path = Path.Combine(ProgramSettings.GetTypeLibDirectory(), Path.GetFileName(asm_path));
            File.Copy(asm_path, path);
        }
    }

    public static void LoadTypesFromAssembly(string path, bool copy_to_cache = false)
    {
        LoadTypesFromAssembly(Assembly.LoadFrom(path), copy_to_cache);
    }

    public static Assembly ConvertTypeLibToAssembly(ITypeLib typeLib, IProgress<Tuple<string, int>> progress)
    {
        if (!m_loadtypes.Value)
        {
            throw new InvalidOperationException("Couldn't initialize types.");
        }

        if (m_typelibs.ContainsKey(Marshal.GetTypeLibGuid(typeLib)))
        {
            return m_typelibs[Marshal.GetTypeLibGuid(typeLib)];
        }
        else
        {
            string strAssemblyPath = ProgramSettings.GetTypeLibDirectory();
            strAssemblyPath = Path.Combine(strAssemblyPath, Marshal.GetTypeLibGuid(typeLib).ToString() + ".dll");

            TypeLibConverter conv = new();
            AssemblyBuilder asm = conv.ConvertTypeLibToAssembly(typeLib, strAssemblyPath, TypeLibImporterFlags.ReflectionOnlyLoading,
                                    new TypeLibCallback(progress), null, null, null, null);
            asm.Save(Path.GetFileName(strAssemblyPath));
            Assembly a = Assembly.LoadFile(strAssemblyPath);

            m_typelibs[Marshal.GetTypeLibGuid(typeLib)] = a;
            lock (m_typelibsname)
            {
                m_typelibsname[a.FullName] = a;
            }
            LoadTypes(a);

            return a;
        }
    }

    public static Assembly LoadTypeLib(string path, IProgress<Tuple<string, int>> progress)
    {
        ITypeLib typeLib = null;

        try
        {
            typeLib = NativeMethods.LoadTypeLibEx(path, RegKind.None);

            return ConvertTypeLibToAssembly(typeLib, progress);
        }
        finally
        {
            if (typeLib is not null)
            {
                Marshal.ReleaseComObject(typeLib);
            }
        }
    }

    public static Assembly LoadTypeLib(ITypeLib typeLib, IProgress<Tuple<string, int>> progress)
    {
        try
        {
            return ConvertTypeLibToAssembly(typeLib, progress);
        }
        finally
        {
            if (typeLib is not null)
            {
                Marshal.ReleaseComObject(typeLib);
            }
        }
    }

    public static Type GetDispatchTypeInfo(object obj, IProgress<Tuple<string, int>> progress)
    {
        if (!obj.GetType().IsCOMObject)
        {
            return obj.GetType();
        }
        else
        {
            if (obj is not IDispatch disp)
            {
                return null;
            }

            disp.GetTypeInfo(0, 0x409, out ITypeInfo ti);
            ti.GetContainingTypeLib(out ITypeLib tl, out int iIndex);
            Guid typelibGuid = Marshal.GetTypeLibGuid(tl);
            Assembly asm = LoadTypeLib(tl, progress) ?? throw new InvalidOperationException("Couldn't convert the assembly.");
            string name = Marshal.GetTypeInfoName(ti);
            return asm.GetTypes().FirstOrDefault(t => t.Name == name);
        }
    }

    public static void SetScriptingFactory(ICOMObjectWrapperScriptingFactory factory)
    {
        m_factory = factory;
    }

    public static ICOMObjectWrapper Wrap(object obj, COMInterfaceEntry intf)
    {
        return Wrap(obj, intf.Iid, intf.Database);
    }

    public static ICOMObjectWrapper Wrap(object obj, COMInterfaceInstance intf)
    {
        return Wrap(obj, intf.Iid, intf.Database);
    }

    public static ICOMObjectWrapper Wrap(object obj, COMIPIDEntry ipid)
    {
        return Wrap(obj, ipid.Iid, ipid.Database);
    }

    public static ICOMObjectWrapper Wrap(object obj, Type intf_type, COMRegistry registry)
    {
        if (intf_type is null)
        {
            throw new ArgumentNullException(nameof(intf_type));
        }

        if (!IsComInterfaceType(intf_type))
        {
            throw new ArgumentException("Type must be a COM interface.");
        }
        return Wrap(obj, intf_type.GUID, intf_type, registry);
    }

    public static ICOMObjectWrapper Wrap(object obj, Guid iid, COMRegistry registry)
    {
        return Wrap(obj, iid, GetInterfaceType(iid, registry), registry);
    }

    public static object Unwrap(object obj)
    {
        if (obj is null)
        {
            return null;
        }

        if (Marshal.IsComObject(obj))
        {
            return obj;
        }

        if (obj is ICOMObjectWrapper wrapper)
        {
            return wrapper.Unwrap();
        }
        throw new ArgumentException("Unknown wrapped object.", nameof(obj));
    }
    #endregion

    #region Internal Members
    internal static void FlushIidType(Guid iid)
    {
        m_iidtypes?.TryRemove(iid, out _);
    }
    #endregion
}
