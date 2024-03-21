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

using OleViewDotNet.Database;
using OleViewDotNet.Forms;
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace OleViewDotNet.InterfaceViewers;

internal interface ITypeViewerFactory
{
    Guid Iid { get; }
    string IidName { get; }
    Control CreateInstance(COMRegistry registry, ICOMClassEntry entry, string strObjName, ObjectEntry pObject);
}

/// <summary>
/// Simple base implementation to reduce the amount of code to write
/// </summary>
internal abstract class BaseTypeViewerFactory : ITypeViewerFactory
{
    public BaseTypeViewerFactory(string strName, Guid iid)
    {
        IidName = strName;
        Iid = iid;
    }

    public BaseTypeViewerFactory(Type type)
    {
        IidName = type.Name;
        Iid = type.GUID;
    }

    public string IidName { get; private set; }
    public Guid Iid { get; private set; }
    abstract public Control CreateInstance(COMRegistry registry, ICOMClassEntry entry, string strObjName, ObjectEntry pObject);
}

/// <summary>
/// Generic factory implementation for use if we have a predefined type
/// </summary>
/// <typeparam name="T">The interface type to create the factory for</typeparam>
internal class GenericTypeViewerFactory<T> : BaseTypeViewerFactory
{
    public GenericTypeViewerFactory() : base(typeof(T))
    {
    }

    public override Control CreateInstance(COMRegistry registry, ICOMClassEntry entry, string strObjName, ObjectEntry pObject)
    {
        return new TypedObjectViewer(registry, strObjName, pObject, typeof(T));
    }
}

/// <summary>
/// Generic factory implementation for use when we have a instantiated type object
/// </summary>
internal class InstanceTypeViewerFactory : BaseTypeViewerFactory
{
    private readonly Type m_type;
    public InstanceTypeViewerFactory(Type t)
        : base(t)
    {
        m_type = t;
    }

    public override Control CreateInstance(COMRegistry registry,
        ICOMClassEntry entry, string strObjName, ObjectEntry pObject)
    {
        return new TypedObjectViewer(registry, strObjName, pObject, m_type);
    }
}

internal class InterfaceViewers
{
    private static Dictionary<Guid, ITypeViewerFactory> m_viewfactory;

    private static void LoadInterfaceViewersFromAssembly(Assembly a)
    {
        Type[] types = a.GetTypes();
        foreach (Type t in types)
        {
            /* Only allow non-generic implemented classes */
            if (t.IsClass && (!t.IsAbstract) && (!t.IsGenericType))
            {
                Type[] interfaces = t.GetInterfaces();

                foreach (Type i in interfaces)
                {
                    if (i == typeof(ITypeViewerFactory))
                    {
                        ITypeViewerFactory factory;

                        try
                        {
                            ConstructorInfo con = t.GetConstructor(new Type[0]);
                            if (con is not null)
                            {
                                factory = (ITypeViewerFactory)con.Invoke(new object[0]);
                                if (factory is not null)
                                {
                                    m_viewfactory.Add(factory.Iid, factory);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                        }
                        break;
                    }
                }
            }
        }
    }

    public static void LoadInterfaceViewers()
    {
        if (m_viewfactory is null)
        {
            m_viewfactory = new Dictionary<Guid, ITypeViewerFactory>();

            try
            {
                /* See if we have any registered in the current assembly */
                LoadInterfaceViewersFromAssembly(Assembly.GetExecutingAssembly());
            }
            catch
            {
            }

            try
            {
                string[] plugins = Directory.GetFiles(AppUtilities.GetPluginDirectory(), "*.dll");

                foreach (string p in plugins)
                {
                    try
                    {
                        Assembly a = Assembly.LoadFile(p);
                        LoadInterfaceViewersFromAssembly(a);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }

    public static ITypeViewerFactory GetInterfaceViewer(Guid iid)
    {
        if (m_viewfactory is null)
        {
            LoadInterfaceViewers();
        }

        if (m_viewfactory.ContainsKey(iid))
        {
            return m_viewfactory[iid];
        }
        else
        {
            return null;
        }
    }

    public static void AddFactory(ITypeViewerFactory factory)
    {
        if (GetInterfaceViewer(factory.Iid) is null)
        {
            m_viewfactory.Add(factory.Iid, factory);
        }
    }
}
