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
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.CodeDom;
using System.Text;
using System.CodeDom.Compiler;
using System.Linq;
using Microsoft.CSharp;
using OleViewDotNet.InterfaceViewers;
using System.ComponentModel;

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

    [ComImport, Guid("CB5BDC81-93C1-11CF-8F20-00805F2CD064"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
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

            return COMUtilities.ConvertTypeLibToAssembly((ITypeLib)tl);            
        }

        public void ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg)
        {
        }
    }

    public enum SecurityIntegrityLevel
    {
        Untrusted = 0,
        Low = 0x1000,
        Medium = 0x2000,
        High = 0x3000,
        System = 0x4000,
    }

    [Flags]
    public enum CLSCTX
    {
        CLSCTX_INPROC_SERVER = 0x1,
        CLSCTX_INPROC_HANDLER = 0x2,
        CLSCTX_LOCAL_SERVER = 0x4,
        CLSCTX_INPROC_SERVER16 = 0x8,
        CLSCTX_REMOTE_SERVER = 0x10,
        CLSCTX_INPROC_HANDLER16 = 0x20,
        CLSCTX_RESERVED1 = 0x40,
        CLSCTX_RESERVED2 = 0x80,
        CLSCTX_RESERVED3 = 0x100,
        CLSCTX_RESERVED4 = 0x200,
        CLSCTX_NO_CODE_DOWNLOAD = 0x400,
        CLSCTX_RESERVED5 = 0x800,
        CLSCTX_NO_CUSTOM_MARSHAL = 0x1000,
        CLSCTX_ENABLE_CODE_DOWNLOAD = 0x2000,
        CLSCTX_NO_FAILURE_LOG = 0x4000,
        CLSCTX_DISABLE_AAA = 0x8000,
        CLSCTX_ENABLE_AAA = 0x10000,
        CLSCTX_FROM_DEFAULT_CONTEXT = 0x20000,
        CLSCTX_ACTIVATE_32_BIT_SERVER = 0x40000,
        CLSCTX_ACTIVATE_64_BIT_SERVER = 0x80000,
        CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER,
        CLSCTX_ALL = CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER
    }

    [Flags]
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

    public static class COMUtilities
    {
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
        [DllImport("ole32.dll", EntryPoint = "CoUnmarshalInterface", CallingConvention = CallingConvention.StdCall)]
        public static extern int CoUnmarshalInterface(IStream stm, ref Guid riid, out IntPtr ppv);

        [return: MarshalAs(UnmanagedType.Interface)]
        [DllImport("ole32.dll", ExactSpelling=true, PreserveSig=false)]
        public static extern IBindCtx CreateBindCtx([In] uint reserved);

        [DllImport("ole32.dll", ExactSpelling=true, PreserveSig=false)]
        public static extern IMoniker CreateObjrefMoniker([MarshalAs(UnmanagedType.Interface)] object punk);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = false)]
        public extern static void SHCreateStreamOnFile(string pszFile, STGM grfMode, out IntPtr ppStm);

        [Flags]
        private enum SECURITY_INFORMATION 
        {
            OWNER_SECURITY_INFORMATION = 1,
            GROUP_SECURITY_INFORMATION = 2,
            DACL_SECURITY_INFORMATION = 4,
            LABEL_SECURITY_INFORMATION = 0x10,
        }

        const uint SDDL_REVISION_1 = 1;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true, SetLastError=true)]
        private extern static bool ConvertSecurityDescriptorToStringSecurityDescriptor(byte[] sd, uint rev, SECURITY_INFORMATION secinfo, out IntPtr str, out int length);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, PreserveSig = true)]
        private extern static IntPtr LocalFree(IntPtr hMem);

        public static string GetStringSDForSD(byte[] sd)
        {
            IntPtr sddl;
            int length;

            if(ConvertSecurityDescriptorToStringSecurityDescriptor(sd, SDDL_REVISION_1, 
                SECURITY_INFORMATION.DACL_SECURITY_INFORMATION | SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION | SECURITY_INFORMATION.LABEL_SECURITY_INFORMATION,
                out sddl, out length))
            {
                string ret = Marshal.PtrToStringUni(sddl);

                LocalFree(sddl);

                return ret;
            }
            else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public static string GetILForSD(byte[] sd)
        {
            if ((sd != null) && (sd.Length > 0))
            {
                IntPtr sddl;
                int length;

                if (ConvertSecurityDescriptorToStringSecurityDescriptor(sd, SDDL_REVISION_1,
                    SECURITY_INFORMATION.LABEL_SECURITY_INFORMATION,
                    out sddl, out length))
                {
                    string ret = Marshal.PtrToStringUni(sddl, length);

                    LocalFree(sddl);

                    return ret;
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            else
            {
                return null;
            }
        }

        public static bool SDHasAC(byte[] sd)
        {
            if (sd == null || sd.Length == 0)
            {
                return false;
            }

            string sddl = GetStringSDForSD(sd);
            return sddl.Contains("S-1-15-") || sddl.Contains(";AC)");
        }

        private static Dictionary<Guid, Assembly> m_typelibs;
        private static Dictionary<string, Assembly> m_typelibsname;
        private static Dictionary<Guid, Type> m_iidtypes;

        static COMUtilities()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (m_typelibsname != null)
            {
                lock (m_typelibsname)
                {
                    if (m_typelibsname.ContainsKey(args.Name))
                    {
                        return m_typelibsname[args.Name];
                    }
                }
            }

            return null;
        }

        public static string ReadStringFromKey(RegistryKey rootKey, string keyName, string valueName)
        {
            using (RegistryKey key = rootKey.OpenSubKey(keyName))
            {
                string valueString = "";

                if (key != null)
                {
                    object valueObject = key.GetValue(valueName);
                    if (valueObject != null)
                    {
                        valueString = valueObject.ToString();
                    }
                }

                return valueString;
            }
        }

        public static int ReadIntFromKey(RegistryKey rootKey, string keyName, string valueName)
        {
            string value = ReadStringFromKey(rootKey, keyName, valueName);
            if (value != null)
            {
                int ret;
                if (int.TryParse(value, out ret))
                {
                    return ret;
                }
            }
            return 0;
        }

        public static Guid ReadGuidFromKey(RegistryKey rootKey, string keyName, string valueName)
        {
            string guid = ReadStringFromKey(rootKey, keyName, valueName);
            if (guid != null && IsValidGUID(guid))
            {
                return new Guid(guid);
            }
            return Guid.Empty;
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
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
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

        public static string Get32bitExePath()
        {
            return Path.Combine(GetAppDirectory(), "OleViewDotNet32.exe");
        }

        public static string GetExePath()
        {
            return Path.Combine(GetAppDirectory(), "OleViewDotNet.exe");
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

        private static void LoadBuiltinTypes(Assembly asm)
        {
            foreach(Type t in asm.GetTypes().Where(x => x.IsPublic && x.IsInterface && IsComImport(x)))
            {
                if (!m_iidtypes.ContainsKey(t.GUID))
                {
                    m_iidtypes.Add(t.GUID, t);
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
                    m_typelibsname = new Dictionary<string, Assembly>();

                    foreach (string f in files)
                    {
                        try
                        {
                            Assembly a = Assembly.LoadFrom(f);
                            if (!m_typelibs.ContainsKey(Marshal.GetTypeLibGuidForAssembly(a)))
                            {
                                m_typelibs.Add(Marshal.GetTypeLibGuidForAssembly(a), a);

                                lock (m_typelibsname)
                                {
                                    m_typelibsname[a.FullName] = a;
                                }

                                RegisterTypeInterfaces(a);
                            }
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Trace.WriteLine(e.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.ToString());
                }

                LoadBuiltinTypes(Assembly.GetExecutingAssembly());
                LoadBuiltinTypes(typeof(int).Assembly);
            }
        }

        public static Assembly LoadTypeLib(string path)
        {
            ITypeLib typeLib = null;

            try
            {
                LoadTypeLibEx(path, RegKind.RegKind_Default, out typeLib);

                return ConvertTypeLibToAssembly(typeLib);
            }
            finally
            {
                if (typeLib != null)
                {
                    Marshal.ReleaseComObject(typeLib);
                }
            }
        }

        public static Assembly ConvertTypeLibToAssembly(ITypeLib typeLib)
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

                lock (m_typelibsname)
                {
                    m_typelibsname[a.FullName] = a;
                }

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
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }

            return ret;
        }

        public static RawSecurityDescriptor ReadSecurityDescriptorFromKey(RegistryKey key, string value)
        {
            byte[] data = (byte[])key.GetValue(value);

            if (data != null)
            {
                return new RawSecurityDescriptor(data, 0);
            }
            else
            {
                return null;
            }
        }

        public static bool IsComImport(Type t)
        {
            return t.GetCustomAttributes(typeof(ComImportAttribute), false).Length > 0;
        }

        private static Dictionary<Type, Type> _wrappers = new Dictionary<Type, Type>();        

        private static CodeParameterDeclarationExpression GetParameter(ParameterInfo pi)
        {
            Type baseType = pi.ParameterType;

            if (baseType.IsByRef)
            {
                string name = baseType.FullName.TrimEnd('&');

                baseType = baseType.Assembly.GetType(name);
            }

            CodeParameterDeclarationExpression p = new CodeParameterDeclarationExpression(baseType, pi.Name);
            FieldDirection d = FieldDirection.In;

            if ((pi.Attributes & ParameterAttributes.Out) == ParameterAttributes.Out)
            {
                d = FieldDirection.Out;
            }

            if ((pi.Attributes & ParameterAttributes.In) == ParameterAttributes.In)
            {
                if (d == FieldDirection.Out)
                {
                    d = FieldDirection.Ref;
                }
                else
                {
                    d = FieldDirection.In;
                }
            }

            p.Direction = d;

            return p;
        }

        private static CodeMemberMethod CreateForwardingMethod(MethodInfo mi)
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            method.Name = mi.Name;
            method.ReturnType = new CodeTypeReference(mi.ReturnType);

            List<CodeExpression> parameters = new List<CodeExpression>();

            foreach (ParameterInfo pi in mi.GetParameters())
            {
                CodeParameterDeclarationExpression p = GetParameter(pi);
                method.Parameters.Add(p);
                parameters.Add(new CodeDirectionExpression(p.Direction, new CodeVariableReferenceExpression(pi.Name)));
            }

            CodeMethodInvokeExpression invokeExpr = new CodeMethodInvokeExpression(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_target"),
                mi.Name, parameters.ToArray());

            if (mi.ReturnType != typeof(void))
            {
                method.Statements.Add(new CodeMethodReturnStatement(invokeExpr));
            }
            else
            {
                method.Statements.Add(invokeExpr);
            }

            return method;
        }

        private static CodeMemberProperty CreateForwardingProperty(PropertyInfo pi)
        {
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            prop.Name = pi.Name;
            prop.Type = new CodeTypeReference(pi.PropertyType);

            CodePropertyReferenceExpression propExpr = new CodePropertyReferenceExpression(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_target"),
                pi.Name);

            if (pi.CanRead)
            {
                prop.GetStatements.Add(new CodeMethodReturnStatement(propExpr));
            }

            if (pi.CanWrite)
            {
                prop.SetStatements.Add(new CodeAssignStatement(propExpr, new CodeVariableReferenceExpression("value")));
            }

            return prop;
        }

        private static CodeTypeDeclaration CreateWrapperTypeDeclaration(Type t)
        {
            CodeTypeDeclaration type = new CodeTypeDeclaration(t.Name + "Wrapper");
            CodeTypeReference typeRef = new CodeTypeReference(t);

            type.IsClass = true;
            type.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            type.BaseTypes.Add(typeRef);

            type.Members.Add(new CodeMemberField(typeRef, "_target"));

            CodeConstructor defaultConstructor = new CodeConstructor();
            defaultConstructor.Attributes = MemberAttributes.Public;
            defaultConstructor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object)), "target"));
            defaultConstructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_target"), new CodeCastExpression(typeRef, new CodeVariableReferenceExpression("target"))));
            type.Members.Add(defaultConstructor);

            foreach (MethodInfo mi in t.GetMethods())
            {
                if (!mi.IsSpecialName)
                {
                    type.Members.Add(CreateForwardingMethod(mi));
                }
            }

            foreach(PropertyInfo pi in t.GetProperties())
            {
                type.Members.Add(CreateForwardingProperty(pi));                
            }

            return type;
        }


        private static Type CreateWrapper(Type t)
        {
            Type ret = null;
            CodeCompileUnit unit = new CodeCompileUnit();
            CodeNamespace ns = new CodeNamespace();

            CSharpCodeProvider provider = new CSharpCodeProvider();

            CodeTypeDeclaration type = CreateWrapperTypeDeclaration(t);

            ns.Types.Add(type);
            unit.Namespaces.Add(ns);

            StringBuilder builder = new StringBuilder();
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.IndentString = "    ";
            options.BlankLinesBetweenMembers = false;
   
            TextWriter writer = new StringWriter(builder);

            provider.GenerateCodeFromCompileUnit(unit, writer, options);


            writer.Close();

            File.WriteAllText("dump.cs", builder.ToString());           

            try
            {
                CompilerParameters compileParams = new CompilerParameters();
                TempFileCollection tempFiles = new TempFileCollection(Path.GetTempPath(), false);

                compileParams.GenerateExecutable = false;
                compileParams.GenerateInMemory = true;
                compileParams.IncludeDebugInformation = true;
                compileParams.TempFiles = tempFiles;
                compileParams.ReferencedAssemblies.Add("System.dll");
                compileParams.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
                compileParams.ReferencedAssemblies.Add("System.Core.dll");                
                compileParams.ReferencedAssemblies.Add(t.Assembly.Location);

                CompilerResults results = provider.CompileAssemblyFromDom(compileParams, unit);

                if (results.Errors.HasErrors)
                {
                    foreach (CompilerError e in results.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());                        
                    }
                }
                else
                {
                    ret = results.CompiledAssembly.GetType(t.Name + "Wrapper");
                    if (ret != null)
                    {
                        lock (_wrappers)
                        {
                            _wrappers[t] = ret;
                        }
                    }
                }

            }
            catch (Exception)
            {                
            }

            return ret;
        }

        public static dynamic CreateDynamicCallWrapper(object target, Type t)
        {
            Type instanceType = null;

            lock (_wrappers)
            {
                if (_wrappers.ContainsKey(t))
                {
                    instanceType = _wrappers[t];
                }
            }

            if (instanceType == null)
            {
                instanceType = CreateWrapper(t);
            }

            if (instanceType != null)
            {
                return Activator.CreateInstance(instanceType, target);
            }
            else
            {
                return null;
            }
        }

        public static void SaveObjectToStream(object obj, Stream stm)
        {
            IStreamImpl istm = new IStreamImpl(stm);

            IPersistStream ps = obj as IPersistStream;

            if (ps != null)
            {
                ps.Save(istm, false);
            }
            else
            {
                IPersistStreamInit psi = (IPersistStreamInit)obj;

                psi.Save(istm, false);
            }
        }

        public static void LoadObjectFromStream(object obj, Stream stm)
        {
            IStreamImpl istm = new IStreamImpl(stm);

            IPersistStream ps = obj as IPersistStream;

            if (ps != null)
            {                
                ps.Load(istm);
            }
            else
            {
                IPersistStreamInit psi = (IPersistStreamInit)obj;

                psi.InitNew();
                psi.Load(istm);
            }
        }

        public static void OleSaveToStream(object obj, Stream stm)
        {
            using (BinaryWriter writer = new BinaryWriter(stm))
            {
                Guid clsid = GetObjectClass(obj);

                writer.Write(clsid.ToByteArray());

                SaveObjectToStream(obj, stm);
            }
        }

        public static object OleLoadFromStream(Stream stm)
        {
            using (BinaryReader reader = new BinaryReader(stm))
            {
                Guid clsid = new Guid(reader.ReadBytes(16));

                Guid unk = COMInterfaceEntry.IID_IUnknown;
                IntPtr pObj;
                object ret;

                int iError = COMUtilities.CoCreateInstance(ref clsid, IntPtr.Zero, CLSCTX.CLSCTX_SERVER,
                    ref unk, out pObj);

                if (iError != 0)
                {
                    Marshal.ThrowExceptionForHR(iError);
                }

                ret = Marshal.GetObjectForIUnknown(pObj);
                Marshal.Release(pObj);

                LoadObjectFromStream(ret, stm);

                return ret;
            }
        }

        public static Guid GetObjectClass(object p)
        {
            Guid ret = Guid.Empty;

            try
            {
                if (p is IPersist)
                {
                    ((IPersist)p).GetClassID(out ret);
                }
                else if (p is IPersistStream)
                {
                    ((IPersistStream)p).GetClassID(out ret);
                }
                else if (p is IPersistStreamInit)
                {
                    ((IPersistStreamInit)p).GetClassID(out ret);
                }
                else if (p is IPersistFile)
                {
                    ((IPersistFile)p).GetClassID(out ret);
                }
                else if (p is IPersistMoniker)
                {
                    ((IPersistMoniker)p).GetClassID(out ret);
                }
                else if (p is IPersistStorage)
                {
                    ((IPersistStorage)p).GetClassID(out ret);
                }
            }
            catch
            {
            }

            return ret;
        }

        public static string GetMonikerDisplayName(IMoniker pmk)
        {
            string strDisplayName;
            IBindCtx bindCtx = CreateBindCtx(0);

            pmk.GetDisplayName(bindCtx, null, out strDisplayName);

            Marshal.ReleaseComObject(bindCtx);

            return strDisplayName;
        }

        public static byte[] MarshalObject(object obj)
        {
            IMoniker mk;

            mk = COMUtilities.CreateObjrefMoniker(obj);

            string name = COMUtilities.GetMonikerDisplayName(mk).Substring(7).TrimEnd(':');

            Marshal.ReleaseComObject(mk);

            return Convert.FromBase64String(name);
        }

        public static string MemberInfoToString(MemberInfo member)
        {
            MethodInfo mi = member as MethodInfo;
            PropertyInfo prop = member as PropertyInfo;

            if (mi != null)
            {
                List<string> pars = new List<string>();
                ParameterInfo[] pis = mi.GetParameters();

                foreach (ParameterInfo pi in pis)
                {
                    string strDir = "";

                    if (pi.IsIn)
                    {
                        strDir += "in";
                    }

                    if (pi.IsOut)
                    {
                        strDir += "out";
                    }

                    if (strDir.Length == 0)
                    {
                        strDir = "in";
                    }

                    if (pi.IsRetval)
                    {
                        strDir += " retval";
                    }

                    if (pi.IsOptional)
                    {
                        strDir += " optional";
                    }

                    pars.Add(String.Format("[{0}] {1} {2}", strDir, pi.ParameterType.Name, pi.Name));
                }

                return String.Format("{0} {1}({2});", mi.ReturnType.Name, mi.Name, String.Join(", ", pars));
            }
            else if (prop != null)
            {
                return String.Format("{0} {1}", prop.PropertyType, prop.Name);
            }
            else
            {
                return null;
            }
        }

        public static bool HasSubkey(this RegistryKey key, string name)
        {
            using (RegistryKey subkey = key.OpenSubKey(name))
            {
                return subkey != null;
            }
        }
    }    
}
