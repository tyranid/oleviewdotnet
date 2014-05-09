using System;
using System.Collections.Generic;
using System.Text;
using WeifenLuo.WinFormsUI.Docking;
using System.Reflection;
using System.IO;
using System.Windows.Forms;

namespace OleViewDotNet.InterfaceViewers
{
    interface ITypeViewerFactory
    {
        Guid Iid { get; }
        string IidName { get; }        
        Control CreateInstance(string strObjName, ObjectEntry pObject);
    }

    /// <summary>
    /// Simple base implementation to reduce the amount of code to write
    /// </summary>
    abstract class BaseTypeViewerFactory : ITypeViewerFactory
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
        abstract public Control CreateInstance(string strObjName, ObjectEntry pObject);            
    }

    /// <summary>
    /// Generic factory implementation for use if we have a predefined type
    /// </summary>
    /// <typeparam name="T">The interface type to create the factory for</typeparam>
    class GenericTypeViewerFactory<T> : BaseTypeViewerFactory
    {        
        public GenericTypeViewerFactory() : base(typeof(T))
        {
        }

        public override Control CreateInstance(string strObjName, ObjectEntry pObject)
        {
            return new TypedObjectViewer(strObjName, pObject, typeof(T));
        }
    }

    /// <summary>
    /// Generic factory implementation for use when we have a instantiated type object
    /// </summary>
    class InstanceTypeViewerFactory : BaseTypeViewerFactory
    {
        Type m_type;
        public InstanceTypeViewerFactory(Type t)
            : base(t)
        {
            m_type = t;
        }

        public override Control CreateInstance(string strObjName, ObjectEntry pObject)
        {
            return new TypedObjectViewer(strObjName, pObject, m_type);
        }
    }

    class InterfaceViewers
    {
        private static Dictionary<Guid, ITypeViewerFactory> m_viewfactory;

        private static void LoadInterfaceViewersFromAssembly(Assembly a)
        {
            Type[] types = a.GetTypes();
            foreach (Type t in types)
            {
                /* Only allow non-generic implemented classes */
                if ((t.IsClass) && (!t.IsAbstract) && (!t.IsGenericType))
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
                                if (con != null)
                                {
                                    factory = (ITypeViewerFactory)con.Invoke(new object[0]);
                                    if (factory != null)
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
            if (m_viewfactory == null)
            {
                m_viewfactory = new Dictionary<Guid, ITypeViewerFactory>();

                try
                {
                    /* See if we have any registered in the current assembly */
                    LoadInterfaceViewersFromAssembly(Assembly.GetExecutingAssembly());
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }

                try
                {
                    string[] plugins = Directory.GetFiles(COMUtilities.GetPluginDirectory(), "*.dll");

                    foreach (string p in plugins)
                    {
                        try
                        {
                            Assembly a = Assembly.LoadFile(p);
                            LoadInterfaceViewersFromAssembly(a);
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine(e.ToString());
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
            if (m_viewfactory == null)
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
            if (GetInterfaceViewer(factory.Iid) == null)
            {
                m_viewfactory.Add(factory.Iid, factory);
            }
        }
    }
}
