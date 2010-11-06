//    This file is part of OleViewDotNet.
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
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace OleViewDotNet
{
    [Guid("0002E013-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICatInformation
    {
        int EnumCategories(int lcid, out IntPtr ppEnum);
        void GetCategoryDesc(ref Guid refCatID, int lcid, out IntPtr strDesc);     
    }

    [Guid("00020400-0000-0000-c000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDispatch
    {
        void GetTypeInfoCount(out uint pctinfo);
        void GetTypeInfo(uint iTypeInfo, uint lcid, out IntPtr pTypeInfo);
        void GetIDsOfNames(ref Guid riid, string[] rszNames, uint cNames, uint lcid, ref int[] dispIDs);
        void Invoke(int dispIdMember, ref Guid riid, uint lcid, ushort wFlags, System.Runtime.InteropServices.ComTypes.DISPPARAMS[] pDispParams,
                    out VariantWrapper pVarResult, ref System.Runtime.InteropServices.ComTypes.EXCEPINFO pExcepInfo, out uint puArgErr);
    }

    public enum ObjectSafetyFlags
    {
        INTERFACESAFE_FOR_UNTRUSTED_CALLER	= 0x00000001,
        INTERFACESAFE_FOR_UNTRUSTED_DATA = 	0x00000002,
        INTERFACE_USES_DISPEX = 0x00000004,
        INTERFACE_USES_SECURITY_MANAGER = 0x00000008
    }

    [Guid("CB5BDC81-93C1-11CF-8F20-00805F2CD064"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IObjectSafety
    {
        void GetInterfaceSafetyOptions(ref Guid riid, out uint pdwSupportedOptions, out uint pdwEnabledOptions);
        void SetInterfaceSafetyOptions(ref Guid riid, uint dwOptionSetMask, uint dwEnabledOptions);
    }

    class TypeLibCallback : ITypeLibImporterNotifySink
    {
        public Assembly ResolveRef(object tl)
        {
            System.Diagnostics.Debug.WriteLine(tl.ToString());

            return null;
        }

        public void ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg)
        {
            //System.Diagnostics.Debug.WriteLine(String.Format("{0} {1} {2}", eventKind.ToString(), eventCode,
            //        eventMsg));
        }
    }
   
    public class COMUtilities
    {
        public enum CLSCTX {
            CLSCTX_INPROC_SERVER        = 0x1, 
            CLSCTX_INPROC_HANDLER       = 0x2, 
            CLSCTX_LOCAL_SERVER         = 0x4, 
            CLSCTX_INPROC_SERVER16      = 0x8,
            CLSCTX_REMOTE_SERVER        = 0x10,
            CLSCTX_INPROC_HANDLER16     = 0x20,
            CLSCTX_RESERVED1            = 0x40,
            CLSCTX_RESERVED2            = 0x80,
            CLSCTX_RESERVED3            = 0x100,
            CLSCTX_RESERVED4            = 0x200,
            CLSCTX_NO_CODE_DOWNLOAD     = 0x400,
            CLSCTX_RESERVED5            = 0x800,
            CLSCTX_NO_CUSTOM_MARSHAL    = 0x1000,
            CLSCTX_ENABLE_CODE_DOWNLOAD = 0x2000,
            CLSCTX_NO_FAILURE_LOG       = 0x4000,
            CLSCTX_DISABLE_AAA          = 0x8000,
            CLSCTX_ENABLE_AAA           = 0x10000,
            CLSCTX_FROM_DEFAULT_CONTEXT = 0x20000,
            CLSCTX_ACTIVATE_32_BIT_SERVER = 0x40000,
            CLSCTX_ACTIVATE_64_BIT_SERVER = 0x80000,
            CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER,
            CLSCTX_ALL = CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER
        }

        public enum STGM
        {
            STGM_READ = 0x00000000, 
            STGM_WRITE = 0x00000001,
            STGM_READWRITE = 0x00000002,
            STGM_SHARE_DENY_NONE = 0x00000040,
            STGM_SHARE_DENY_READ = 0x00000030,
            STGM_SHARE_DENY_WRITE = 0x00000020,
            STGM_SHARE_EXCLUSIVE = 0x00000010,
            STGM_PRIORITY = 0x00040000,
            STGM_CREATE = 0x00001000,
            STGM_CONVERT = 0x00020000,
            STGM_FAILIFTHERE = STGM_READ,
            STGM_DIRECT = STGM_READ,
            STGM_TRANSACTED = 0x00010000,
            STGM_NOSCRATCH = 0x00100000,
            STGM_NOSNAPSHOT = 0x00200000,
            STGM_SIMPLE = 0x08000000,
            STGM_DIRECT_SWMR = 0x00400000,
            STGM_DELETEONRELEASE = 0x04000000
        }

        private enum RegKind
        {
            RegKind_Default = 0,
            RegKind_Register = 1,
            RegKind_None = 2
        }

        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void LoadTypeLibEx(String strTypeLibName, RegKind regKind,
            [MarshalAs(UnmanagedType.Interface)] out ITypeLib typeLib);
        [DllImport("ole32.dll", EntryPoint = "CoCreateInstance", CallingConvention = CallingConvention.StdCall)]        
        public static extern int CoCreateInstance(ref Guid rclsid, IntPtr pUnkOuter, CLSCTX dwClsContext, ref Guid riid, out IntPtr ppv);
        [DllImport("ole32.dll", EntryPoint = "CoGetClassObject", CallingConvention = CallingConvention.StdCall)]
        public static extern int CoGetClassObject(ref Guid rclsid, CLSCTX dwClsContext, IntPtr pServerInfo, ref Guid riid, out IntPtr ppv);

        [return: MarshalAs(UnmanagedType.Interface)]
        [DllImport("ole32.dll", ExactSpelling=true, PreserveSig=false)]
        public static extern IBindCtx CreateBindCtx([In] uint reserved);


        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = false)]
        public extern static void SHCreateStreamOnFile(string pszFile, COMUtilities.STGM grfMode, out IntPtr ppStm);

        private static Dictionary<Guid, Assembly> m_typelibs;
        private static Dictionary<Guid, Type> m_iidtypes;

        public static string ReadStringFromKey(RegistryKey rootKey, string keyName, string valueName)
        {
            RegistryKey key = rootKey.OpenSubKey(keyName);
            string valueString = "";

            if (key != null)
            {
                object valueObject = key.GetValue(valueName);
                if (valueObject != null)
                {
                    valueString = valueObject.ToString();        
                }
                key.Close();
            }

            return valueString;
        }

        public static Guid ReadGuidFromKey(RegistryKey rootKey, string keyName, string valueName)
        {
            Guid ret = Guid.Empty;
            RegistryKey key = rootKey.OpenSubKey(keyName);            

            if (key != null)
            {
                object valueObject = key.GetValue(valueName);
                if (valueObject != null)
                {                    
                    string valueString = valueObject.ToString();
                    if (IsValidGUID(valueString))
                    {
                        ret = new Guid(valueString);
                    }
                }
                key.Close();
            }

            return ret;
        }

        public static string GetCategoryName(Guid catid)
        {
            Guid clsid = new Guid("{0002E005-0000-0000-C000-000000000046}");
            Guid iid = typeof(ICatInformation).GUID;
            IntPtr pCatMgr;
            string strDesc = catid.ToString("B");

            if (CoCreateInstance(ref clsid, IntPtr.Zero, CLSCTX.CLSCTX_INPROC_SERVER, ref iid, out pCatMgr) == 0)
            {
                ICatInformation catInfo = (ICatInformation)Marshal.GetObjectForIUnknown(pCatMgr);
                IntPtr pStrDesc;

                try
                {
                    catInfo.GetCategoryDesc(ref catid, 0, out pStrDesc);                
                    strDesc = Marshal.PtrToStringUni(pStrDesc);
                    Marshal.FreeCoTaskMem(pStrDesc);
                }
                catch(COMException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }

                Marshal.ReleaseComObject(catInfo);
                Marshal.Release(pCatMgr);
            }

            return strDesc;
        }

        public static string GetAppDirectory()
        {
            return Path.GetDirectoryName(new Uri(Assembly.GetCallingAssembly().CodeBase).LocalPath);
        }

        public static string GetAppDataDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OleViewDotNet");
        }

        public static string GetTypeLibDirectory()
        {
            return Path.Combine(GetAppDataDirectory(), "typelib");
        }

        public static string GetPluginDirectory()
        {
            return Path.Combine(GetAppDirectory(), "plugin");
        }

        private static Regex m_guidRegex = null;

        public static bool IsValidGUID(string guid)
        {
            if (m_guidRegex == null)
            {
                m_guidRegex = new Regex("[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}");
            }

            return m_guidRegex.IsMatch(guid);
        }

        private static void RegisterTypeInterfaces(Assembly a)
        {
            Type[] types = a.GetTypes();

            foreach (Type t in types)
            {
                if (t.IsInterface)
                {
                    InterfaceViewers.InterfaceViewers.AddFactory(new InterfaceViewers.InstanceTypeViewerFactory(t));
                    if (!m_iidtypes.ContainsKey(t.GUID))
                    {
                        m_iidtypes.Add(t.GUID, t);
                    }
                }
            }
        }

        public static Type GetInterfaceType(Guid iid)
        {
            if (m_iidtypes == null)
            {
                LoadTypeLibAssemblies();
            }

            if (m_iidtypes.ContainsKey(iid))
            {
                return m_iidtypes[iid];                
            }
            return null;
        }

        public static void LoadTypeLibAssemblies()
        {
            if (m_typelibs == null)
            {
                try
                {
                    string strTypeLibDir = GetTypeLibDirectory();
                    Directory.CreateDirectory(strTypeLibDir);
                    string[] files = Directory.GetFiles(strTypeLibDir, "*.dll");

                    m_typelibs = new Dictionary<Guid, Assembly>();
                    m_iidtypes = new Dictionary<Guid, Type>();

                    foreach (string f in files)
                    {
                        try
                        {
                            Assembly a = Assembly.LoadFrom(f);
                            if (!m_typelibs.ContainsKey(Marshal.GetTypeLibGuidForAssembly(a)))
                            {
                                m_typelibs.Add(Marshal.GetTypeLibGuidForAssembly(a), a);
                                RegisterTypeInterfaces(a);
                            }
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine(e.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }
        }

        private static Assembly ConvertTypeLibToAssembly(ITypeLib typeLib)
        {
            if (m_typelibs == null)
            {
                LoadTypeLibAssemblies();
            }

            if (m_typelibs.ContainsKey(Marshal.GetTypeLibGuid(typeLib)))
            {
                return m_typelibs[Marshal.GetTypeLibGuid(typeLib)];
            }
            else
            {
                string strAssemblyPath = GetTypeLibDirectory();

                strAssemblyPath = Path.Combine(strAssemblyPath, Marshal.GetTypeLibName(typeLib) + ".dll");

                TypeLibConverter conv = new TypeLibConverter();
                AssemblyBuilder asm = conv.ConvertTypeLibToAssembly(typeLib, strAssemblyPath, TypeLibImporterFlags.ReflectionOnlyLoading,
                                        new TypeLibCallback(), null, null, null, null);
                asm.Save(Path.GetFileName(strAssemblyPath));
              
                Assembly a = Assembly.LoadFile(strAssemblyPath);

                m_typelibs[Marshal.GetTypeLibGuid(typeLib)] = a;
                RegisterTypeInterfaces(a);

                return a;
            }
        }

        public static Type GetDispatchTypeInfo(object comObj)
        {
            Type ret = null;

            try
            {
                if (!comObj.GetType().IsCOMObject)
                {
                    ret = comObj.GetType();
                }
                else
                {
                    if (ret == null)
                    {                        
                        IntPtr typeInfo = IntPtr.Zero;

                        try
                        {
                            IDispatch disp = (IDispatch)comObj;

                            disp.GetTypeInfo(0, 0x409, out typeInfo);
                            ITypeInfo ti = (ITypeInfo)Marshal.GetObjectForIUnknown(typeInfo);
                            ITypeLib tl = null;
                            int iIndex = 0;
                            ti.GetContainingTypeLib(out tl, out iIndex);

                            Guid typelibGuid = Marshal.GetTypeLibGuid(tl);
                            Assembly asm = ConvertTypeLibToAssembly(tl);

                            if (asm != null)
                            {
                                ret = asm.GetType(Marshal.GetTypeLibName(tl) + "." + Marshal.GetTypeInfoName(ti));
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                        }
                        finally
                        {
                            if(typeInfo != IntPtr.Zero)
                            {
                                Marshal.Release(typeInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            return ret;
        }
    }    
}
