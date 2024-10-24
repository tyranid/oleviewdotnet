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
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OleViewDotNet.Viewers;

internal static class InterfaceViewers
{
    private static readonly Lazy<Dictionary<Guid, ITypeViewerFactory>> m_viewfactory = new(LoadInterfaceViewers);

    private static void LoadInterfaceViewersFromAssembly(Dictionary<Guid, ITypeViewerFactory> viewfactory, Assembly a)
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
                                    viewfactory.Add(factory.Iid, factory);
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

    public static Dictionary<Guid, ITypeViewerFactory> LoadInterfaceViewers()
    {
        Dictionary<Guid, ITypeViewerFactory> viewfactory = new();

        try
        {
            /* See if we have any registered in the current assembly */
            LoadInterfaceViewersFromAssembly(viewfactory, Assembly.GetExecutingAssembly());
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
                    LoadInterfaceViewersFromAssembly(viewfactory, a);
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
        }
        return viewfactory;
    }

    public static ITypeViewerFactory GetInterfaceViewer(COMInterfaceEntry intf)
    {
        if (m_viewfactory.Value.ContainsKey(intf.Iid))
        {
            return m_viewfactory.Value[intf.Iid];
        }
        else
        {
            return null;
        }
    }

    public static void AddFactory(ITypeViewerFactory factory)
    {
        if (!m_viewfactory.Value.ContainsKey(factory.Iid))
        {
            m_viewfactory.Value.Add(factory.Iid, factory);
        }
    }
}
