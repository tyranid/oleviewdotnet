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
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.Utilities;

public static class COMTypeManager
{
    #region Private Members
    private static readonly ConcurrentDictionary<Guid, Assembly> m_typelibs = new();
    private static readonly ConcurrentDictionary<string, Assembly> m_typelibsname = new();
    private static readonly ConcurrentDictionary<Guid, Type> m_iidtypes = new();

    static COMTypeManager()
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        LoadTypes();
    }

    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        if (m_typelibsname.TryGetValue(args.Name, out Assembly asm))
        {
            return asm;
        }
        return null;
    }

    private static bool LoadTypes()
    {
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

                    RegisterTypeInterfaces(a);
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

        LoadBuiltinTypes(Assembly.GetExecutingAssembly());
        LoadBuiltinTypes(typeof(int).Assembly);
        return true;
    }

    private static void RegisterTypeInterfaces(Assembly a)
    {
        Type[] types = a.GetTypes();

        foreach (Type t in types)
        {
            if (t.IsInterface && t.IsPublic && t.GetCustomAttribute<CoClassAttribute>() is null)
            {
                InterfaceViewers.InterfaceViewers.AddFactory(new InterfaceViewers.InstanceTypeViewerFactory(t));
                m_iidtypes.TryAdd(t.GUID, t);
            }
        }
    }

    private static void LoadBuiltinTypes(Assembly asm)
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
    #endregion

    #region Public Static Methods
    public static bool IsComImport(Type t)
    {
        return t.GetCustomAttributes(typeof(ComImportAttribute), false).Length > 0 ||
            t.GetCustomAttributes(typeof(InterfaceTypeAttribute), false).Length > 0;
    }

    public static Type GetInterfaceType(Guid iid, COMRegistry registry, bool scripting = false)
    {
        if (registry is not null && registry.Interfaces.ContainsKey(iid))
        {
            return GetInterfaceType(registry.Interfaces[iid], scripting);
        }

        return GetInterfaceType(iid);
    }

    public static Type GetInterfaceType(Guid iid)
    {
        if (m_iidtypes.ContainsKey(iid))
        {
            return m_iidtypes[iid];
        }

        return null;
    }

    public static Type GetInterfaceType(COMInterfaceEntry intf, bool scripting = false)
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
            return type_info.ToType();
        }

        if (intf.ProxyClassEntry is null)
        {
            return null;
        }

        var proxy = COMProxyInterface.GetFromIID(intf, intf.HasTypeLib);
        m_iidtypes.TryAdd(intf.Iid, proxy.CreateClientType(scripting));
        return GetInterfaceType(intf.Iid);
    }

    public static Type GetInterfaceType(COMIPIDEntry ipid, bool scripting = false)
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

        m_iidtypes.TryAdd(ipid.Iid, proxy.Entries.Where(e => e.Iid == ipid.Iid).First().CreateClientType(scripting));
        return GetInterfaceType(ipid.Iid);
    }

    public static void LoadTypesFromAssembly(Assembly assembly)
    {
        LoadBuiltinTypes(assembly);
    }

    public static Assembly ConvertTypeLibToAssembly(ITypeLib typeLib, IProgress<Tuple<string, int>> progress)
    {
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
            RegisterTypeInterfaces(a);

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
            IDispatch disp = (IDispatch)obj;

            disp.GetTypeInfo(0, 0x409, out ITypeInfo ti);
            ti.GetContainingTypeLib(out ITypeLib tl, out int iIndex);
            Guid typelibGuid = Marshal.GetTypeLibGuid(tl);
            Assembly asm = LoadTypeLib(tl, progress) ?? throw new InvalidOperationException("Couldn't convert the assembly.");
            string name = Marshal.GetTypeInfoName(ti);
            return asm.GetTypes().First(t => t.Name == name);
        }
    }
    #endregion

    #region Internal Members
    internal static void FlushIidType(Guid iid)
    {
        m_iidtypes?.TryRemove(iid, out _);
    }
    #endregion
}