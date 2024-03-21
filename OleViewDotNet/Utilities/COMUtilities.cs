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

using Microsoft.CSharp;
using NtApiDotNet;
using NtApiDotNet.Win32;
using OleViewDotNet.Database;
using OleViewDotNet.Forms;
using OleViewDotNet.Interop;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Processes;
using OleViewDotNet.Proxy;
using OleViewDotNet.Security;
using OleViewDotNet.Utilities.Format;
using OleViewDotNet.Wrappers;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OleViewDotNet.Utilities;

public static class COMUtilities
{
    private static Dictionary<Guid, Assembly> m_typelibs;
    private static Dictionary<string, Assembly> m_typelibsname;
    private static Dictionary<Guid, Type> m_iidtypes;

    static COMUtilities()
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        if (m_typelibsname is not null)
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

    public static string GetCategoryName(Guid catid)
    {
        Guid clsid = new("{0002E005-0000-0000-C000-000000000046}");
        Guid iid = typeof(ICatInformation).GUID;
        string strDesc = string.Empty;

        if (NativeMethods.CoCreateInstance(clsid, IntPtr.Zero, CLSCTX.INPROC_SERVER, iid, out IntPtr pCatMgr) == 0)
        {
            ICatInformation catInfo = (ICatInformation)Marshal.GetObjectForIUnknown(pCatMgr);

            try
            {
                catInfo.GetCategoryDesc(catid, 0, out IntPtr pStrDesc);
                strDesc = Marshal.PtrToStringUni(pStrDesc);
                Marshal.FreeCoTaskMem(pStrDesc);
            }
            catch (COMException)
            {
            }

            Marshal.ReleaseComObject(catInfo);
            Marshal.Release(pCatMgr);
        }

        if (string.IsNullOrWhiteSpace(strDesc))
        {
            if (catid == new Guid("59fb2056-d625-48d0-a944-1a85b5ab2640"))
            {
                strDesc = "AppContainer Compatible";
            }
            else
            {
                strDesc = catid.FormatGuid();
            }
        }

        return strDesc;
    }


    private static void RegisterTypeInterfaces(Assembly a)
    {
        Type[] types = a.GetTypes();

        foreach (Type t in types)
        {
            if (t.IsInterface && t.IsPublic && t.GetCustomAttribute<CoClassAttribute>() is null)
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
        foreach (Type t in asm.GetTypes().Where(x => x.IsPublic && x.IsInterface && IsComImport(x)))
        {
            if (t.GetCustomAttribute<ObsoleteAttribute>() is not null)
            {
                continue;
            }
            if (!m_iidtypes.ContainsKey(t.GUID))
            {
                m_iidtypes.Add(t.GUID, t);
            }
        }
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
        if (m_iidtypes is null)
        {
            LoadTypeLibAssemblies();
        }

        if (m_iidtypes.ContainsKey(iid))
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

        if (intf.ProxyClassEntry is null)
        {
            return null;
        }

        ConvertProxyToAssembly(COMProxyInterface.GetFromIID(intf), null);
        return GetInterfaceType(intf.Iid);
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

        ConvertProxyToAssembly(proxy, null);
        return GetInterfaceType(ipid.Iid);
    }

    public static void LoadTypesFromAssembly(Assembly assembly)
    {
        if (m_iidtypes is null)
        {
            LoadTypeLibAssemblies();
        }

        LoadBuiltinTypes(assembly);
    }

    public static void LoadTypeLibAssemblies()
    {
        if (m_typelibs is null)
        {
            try
            {
                string strTypeLibDir = ProgramSettings.GetTypeLibDirectory();
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
                        Guid typelib_guid = Marshal.GetTypeLibGuidForAssembly(a);
                        if (!m_typelibs.ContainsKey(typelib_guid))
                        {
                            m_typelibs.Add(typelib_guid, a);

                            lock (m_typelibsname)
                            {
                                m_typelibsname[a.FullName] = a;
                            }

                            RegisterTypeInterfaces(a);
                        }
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
        }
    }

    public static ITypeLib LoadTypeLib(string path)
    {
        return NativeMethods.LoadTypeLibEx(path, RegKind.RegKind_Default);
    }

    public static Assembly LoadTypeLib(string path, IProgress<Tuple<string, int>> progress)
    {
        ITypeLib typeLib = null;

        try
        {
            typeLib = NativeMethods.LoadTypeLibEx(path, RegKind.RegKind_Default);

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

    public static Assembly ConvertTypeLibToAssembly(ITypeLib typeLib, IProgress<Tuple<string, int>> progress)
    {
        if (m_typelibs is null)
        {
            progress?.Report(new Tuple<string, int>("Initializing Global Libraries", -1));
            LoadTypeLibAssemblies();
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
            RegisterTypeInterfaces(a);

            return a;
        }
    }

    private static void ConvertProxyToAssembly(IEnumerable<COMProxyInterface> entries, string output_path, IProgress<Tuple<string, int>> progress)
    {
        if (m_typelibs is null)
        {
            progress?.Report(Tuple.Create("Initializing Global Libraries", -1));
            LoadTypeLibAssemblies();
        }

        COMProxyFileConverter converter = new(output_path, progress);
        converter.AddProxy(entries);
        converter.Save();
    }

    public static void ConvertProxyToAssembly(COMProxyFile proxy, string output_path, IProgress<Tuple<string, int>> progress)
    {
        ConvertProxyToAssembly(proxy.Entries, output_path, progress);
    }

    public static void ConvertProxyToAssembly(COMProxyInterface proxy, string output_path, IProgress<Tuple<string, int>> progress)
    {
        ConvertProxyToAssembly(proxy.ProxyFile, output_path, progress);
    }

    public static void ConvertProxyToAssembly(COMIPIDEntry ipid, string output_path, IProgress<Tuple<string, int>> progress)
    {
        ConvertProxyToAssembly(ipid.ToProxyInstance(), output_path, progress);
    }

    public static Assembly ConvertProxyToAssembly(IEnumerable<COMProxyInterface> entries, IProgress<Tuple<string, int>> progress)
    {
        if (m_typelibs is null)
        {
            progress?.Report(new Tuple<string, int>("Initializing Global Libraries", -1));
            LoadTypeLibAssemblies();
        }

        COMProxyFileConverter converter = new($"{Guid.NewGuid()}.dll", progress);
        converter.AddProxy(entries);
        RegisterTypeInterfaces(converter.BuiltAssembly);
        return converter.BuiltAssembly;
    }

    public static Assembly ConvertProxyToAssembly(COMProxyFile proxy, IProgress<Tuple<string, int>> progress)
    {
        return ConvertProxyToAssembly(proxy.Entries, progress);
    }

    public static Assembly ConvertProxyToAssembly(COMProxyInterface proxy, IProgress<Tuple<string, int>> progress)
    {
        return ConvertProxyToAssembly(proxy.ProxyFile, progress);
    }

    public static Assembly ConvertProxyToAssembly(COMIPIDEntry ipid, IProgress<Tuple<string, int>> progress)
    {
        return ConvertProxyToAssembly(ipid.ToProxyInstance(), progress);
    }

    public static Type GetDispatchTypeInfo(IWin32Window parent, object comObj)
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
                try
                {
                    IDispatch disp = (IDispatch)comObj;

                    disp.GetTypeInfo(0, 0x409, out ITypeInfo ti);
                    ti.GetContainingTypeLib(out ITypeLib tl, out int iIndex);
                    Guid typelibGuid = Marshal.GetTypeLibGuid(tl);
                    Assembly asm = LoadTypeLib(parent, tl);

                    if (asm is not null)
                    {
                        string name = Marshal.GetTypeInfoName(ti);
                        ret = asm.GetTypes().First(t => t.Name == name);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
        }

        return ret;
    }

    public static bool IsComImport(Type t)
    {
        return t.GetCustomAttributes(typeof(ComImportAttribute), false).Length > 0 ||
            t.GetCustomAttributes(typeof(InterfaceTypeAttribute), false).Length > 0;
    }

    private static readonly Dictionary<Type, Type> _wrappers = new();

    private static CodeParameterDeclarationExpression GetParameter(ParameterInfo pi)
    {
        Type baseType = pi.ParameterType;

        if (baseType.IsByRef)
        {
            string name = baseType.FullName.TrimEnd('&');

            baseType = baseType.Assembly.GetType(name);
        }

        CodeParameterDeclarationExpression p = new(baseType, pi.Name);
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
        CodeMemberMethod method = new()
        {
            Attributes = MemberAttributes.Public | MemberAttributes.Final,
            Name = mi.Name,
            ReturnType = new CodeTypeReference(mi.ReturnType)
        };

        List<CodeExpression> parameters = new();

        foreach (ParameterInfo pi in mi.GetParameters())
        {
            CodeParameterDeclarationExpression p = GetParameter(pi);
            method.Parameters.Add(p);
            parameters.Add(new CodeDirectionExpression(p.Direction, new CodeVariableReferenceExpression(pi.Name)));
        }

        CodeMethodInvokeExpression invokeExpr = new(
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
        CodeMemberProperty prop = new();
        prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        prop.Name = pi.Name;
        prop.Type = new CodeTypeReference(pi.PropertyType);

        CodePropertyReferenceExpression propExpr = new(
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
        CodeTypeDeclaration type = new(t.Name + "Wrapper");
        CodeTypeReference typeRef = new(t);

        type.IsClass = true;
        type.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        type.BaseTypes.Add(typeRef);

        type.Members.Add(new CodeMemberField(typeRef, "_target"));

        CodeConstructor defaultConstructor = new();
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

        foreach (PropertyInfo pi in t.GetProperties())
        {
            type.Members.Add(CreateForwardingProperty(pi));
        }

        return type;
    }


    private static Type CreateWrapper(Type t)
    {
        Type ret = null;
        CodeCompileUnit unit = new();
        CodeNamespace ns = new();

        CSharpCodeProvider provider = new();

        CodeTypeDeclaration type = CreateWrapperTypeDeclaration(t);

        ns.Types.Add(type);
        unit.Namespaces.Add(ns);

        StringBuilder builder = new();
        CodeGeneratorOptions options = new();
        options.IndentString = "    ";
        options.BlankLinesBetweenMembers = false;

        TextWriter writer = new StringWriter(builder);

        provider.GenerateCodeFromCompileUnit(unit, writer, options);


        writer.Close();

        File.WriteAllText("dump.cs", builder.ToString());

        try
        {
            CompilerParameters compileParams = new();
            TempFileCollection tempFiles = new(Path.GetTempPath(), false);

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
                    Debug.WriteLine(e.ToString());
                }
            }
            else
            {
                ret = results.CompiledAssembly.GetType(t.Name + "Wrapper");
                if (ret is not null)
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

        if (instanceType is null)
        {
            instanceType = CreateWrapper(t);
        }

        if (instanceType is not null)
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
        IStreamImpl istm = new(stm);


        if (obj is IPersistStream ps)
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
        IStreamImpl istm = new(stm);


        if (obj is IPersistStream ps)
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
        using BinaryWriter writer = new(stm);
        Guid clsid = GetObjectClass(obj);

        writer.Write(clsid.ToByteArray());

        SaveObjectToStream(obj, stm);
    }

    public static object OleLoadFromStream(Stream stm, out Guid clsid)
    {
        using BinaryReader reader = new(stm);
        clsid = new Guid(reader.ReadBytes(16));

        object ret;

        int iError = NativeMethods.CoCreateInstance(clsid, IntPtr.Zero, CLSCTX.SERVER,
            COMKnownGuids.IID_IUnknown, out IntPtr pObj);

        if (iError != 0)
        {
            Marshal.ThrowExceptionForHR(iError);
        }

        ret = Marshal.GetObjectForIUnknown(pObj);
        Marshal.Release(pObj);

        LoadObjectFromStream(ret, stm);

        return ret;
    }

    public static object CreateFromMoniker(string moniker, BIND_OPTS3 bind_opts)
    {
        NativeMethods.CoGetObject(moniker, bind_opts, COMKnownGuids.IID_IUnknown, out object ret);
        return ret;
    }

    public static object CreateFromMoniker(string moniker, CLSCTX clsctx)
    {
        BIND_OPTS3 bind_opts = new()
        {
            dwClassContext = clsctx
        };
        return CreateFromMoniker(moniker, bind_opts);
    }

    public static object CreateFromSessionMoniker(Guid clsid, int session_id, bool factory)
    {
        return CreateFromMoniker($"session:{session_id}!{(factory ? "clsid" : "new")}:{clsid}", CLSCTX.LOCAL_SERVER);
    }

    public static object UnmarshalObject(Stream stm, Guid iid)
    {
        return NativeMethods.CoUnmarshalInterface(new IStreamImpl(stm), iid);
    }

    public static object UnmarshalObject(byte[] objref)
    {
        return UnmarshalObject(new MemoryStream(objref), COMKnownGuids.IID_IUnknown);
    }

    public static object UnmarshalObject(COMObjRef objref)
    {
        return UnmarshalObject(objref.ToArray());
    }

    public static Guid GetObjectClass(object p)
    {
        Guid ret = Guid.Empty;

        try
        {
            if (p is IPersist persist)
            {
                persist.GetClassID(out ret);
            }
            else if (p is IPersistStream stream)
            {
                stream.GetClassID(out ret);
            }
            else if (p is IPersistStreamInit init)
            {
                init.GetClassID(out ret);
            }
            else if (p is IPersistFile file)
            {
                file.GetClassID(out ret);
            }
            else if (p is IPersistMoniker moniker)
            {
                moniker.GetClassID(out ret);
            }
            else if (p is IPersistStorage storage)
            {
                storage.GetClassID(out ret);
            }
        }
        catch
        {
        }

        return ret;
    }

    public static string GetMonikerDisplayName(IMoniker pmk)
    {
        IBindCtx bindCtx = NativeMethods.CreateBindCtx(0);

        pmk.GetDisplayName(bindCtx, null, out string strDisplayName);
        Marshal.ReleaseComObject(bindCtx);

        return strDisplayName;
    }

    public static byte[] MarshalObject(object obj, Guid iid, MSHCTX mshctx, MSHLFLAGS mshflags)
    {
        MemoryStream stm = new();
        NativeMethods.CoMarshalInterface(new IStreamImpl(stm), iid, obj, mshctx, IntPtr.Zero, mshflags);
        return stm.ToArray();
    }

    public static byte[] MarshalObject(object obj)
    {
        return MarshalObject(obj, COMKnownGuids.IID_IUnknown, MSHCTX.DIFFERENTMACHINE, MSHLFLAGS.NORMAL);
    }

    public static COMObjRef MarshalObjectToObjRef(object obj, Guid iid, MSHCTX mshctx, MSHLFLAGS mshflags)
    {
        return COMObjRef.FromArray(MarshalObject(obj, iid, mshctx, mshflags));
    }

    public static COMObjRef MarshalObjectToObjRef(object obj)
    {
        return MarshalObjectToObjRef(obj, COMKnownGuids.IID_IUnknown, MSHCTX.DIFFERENTMACHINE, MSHLFLAGS.NORMAL);
    }

    private static string GetNextToken(string name, out string token)
    {
        token = null;
        if (name.Length == 0)
        {
            return name;
        }
        int end_index = name.IndexOf('_');
        if (end_index < 0)
        {
            token = name;
        }
        else
        {
            token = name.Substring(0, end_index);
        }
        return name.Substring(end_index + 1).TrimStart('_');
    }

    private static string GetNextToken(string name, out int token)
    {
        if (name.Length == 0 || !char.IsDigit(name[0]))
        {
            throw new InvalidDataException("Expected an integer");
        }
        int length = 0;
        while (char.IsDigit(name[length]))
        {
            length++;
        }

        token = int.Parse(name.Substring(0, length));

        return name.Substring(length).TrimStart('_');
    }

    private static string ReadType(ref string name)
    {
        name = GetNextToken(name, out string token);
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidDataException("Expected a type name");
        }

        if (char.IsLetter(token[0]))
        {
            return token;
        }
        else if (token[0] == '~')
        {
            StringBuilder builder = new();

            name = GetNextToken(name, out int type_count);
            builder.Append(token.Substring(1));
            builder.Append("<");
            List<string> types = new();
            for (int i = 0; i < type_count; ++i)
            {
                types.Add(ReadType(ref name));
            }
            builder.Append(string.Join(",", types));
            builder.Append(">");
            return builder.ToString();
        }
        else
        {
            throw new InvalidDataException("Expected a type name or a generic type");
        }
    }

    private static readonly ConcurrentDictionary<string, string> _demangled_names = new();

    private static string DemangleGenericType(string name)
    {
        name = name.Replace("__F", "~").Replace("__C", ".").TrimStart('_');
        return ReadType(ref name);
    }

    // TODO: This isn't exactly correct, but can't find any good documentation.
    public static string DemangleWinRTName(string name, Guid? iid = null, bool ignore_cache = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return iid?.ToString() ?? "IInvalidName";
        }
        name = name.Trim();
        if (!ignore_cache && _demangled_names.TryGetValue(name, out string result))
        {
            return result;
        }

        result = name;

        if (name.StartsWith("__x_") || name.StartsWith("___x_") || name.StartsWith("____x_"))
        {
            result = name.TrimStart('_').Substring(2).Replace("_C", ".");
        }
        else if (name.StartsWith("__F") || name.StartsWith("___F"))
        {
            try
            {
                result = DemangleGenericType(name);
            }
            catch (InvalidDataException)
            {
            }
        }

        return _demangled_names.GetOrAdd(name, result);
    }

    internal static COMRegistry LoadRegistry(IWin32Window window,
        Func<IProgress<Tuple<string, int>>, CancellationToken, object> worker)
    {
        using WaitingDialog loader = new(worker);
        if (loader.ShowDialog(window) == DialogResult.OK)
        {
            return loader.Result as COMRegistry;
        }
        else
        {
            throw loader.Error;
        }
    }

    internal static COMRegistry LoadRegistry(IWin32Window window, COMRegistryMode mode)
    {
        if (mode == COMRegistryMode.Diff)
        {
            throw new ArgumentException("Can't load a diff registry");
        }

        return LoadRegistry(window, (progress, token) => COMRegistry.Load(mode, null, progress));
    }

    internal static COMRegistry LoadRegistry(IWin32Window window, string database_file)
    {
        return LoadRegistry(window, (progress, token) => COMRegistry.Load(database_file, progress));
    }

    internal static COMRegistry DiffRegistry(IWin32Window window, COMRegistry left, COMRegistry right, COMRegistryDiffMode mode)
    {
        return LoadRegistry(window, (progress, token) => COMRegistry.Diff(left, right, mode, progress));
    }

    internal static Assembly LoadTypeLib(IWin32Window window, string path)
    {
        using WaitingDialog dlg = new((progress, token) => LoadTypeLib(path, progress), s => s);
        dlg.Text = $"Loading TypeLib {path}";
        dlg.CancelEnabled = false;
        if (dlg.ShowDialog(window) == DialogResult.OK)
        {
            return (Assembly)dlg.Result;
        }
        else if (dlg.Error is not null && dlg.Error is not OperationCanceledException)
        {
            EntryPoint.ShowError(window, dlg.Error);
        }
        return null;
    }

    internal static Assembly LoadTypeLib(IWin32Window window, ITypeLib typelib)
    {
        using WaitingDialog dlg = new((progress, token) => LoadTypeLib(typelib, progress), s => s);
        dlg.Text = "Loading TypeLib";
        dlg.CancelEnabled = false;
        if (dlg.ShowDialog(window) == DialogResult.OK)
        {
            return (Assembly)dlg.Result;
        }
        else if (dlg.Error is not null && dlg.Error is not OperationCanceledException)
        {
            EntryPoint.ShowError(window, dlg.Error);
        }
        return null;
    }

    internal static IEnumerable<COMProcessEntry> LoadProcesses(IEnumerable<Process> procs, IWin32Window window, COMRegistry registry)
    {
        using WaitingDialog dlg = new((progress, token) => COMProcessParser.GetProcesses(procs, COMProcessParserConfig.Default, progress, registry), s => s);
        dlg.Text = "Loading Processes";
        if (dlg.ShowDialog(window) == DialogResult.OK)
        {
            return (IEnumerable<COMProcessEntry>)dlg.Result;
        }
        else if (dlg.Error is not null && dlg.Error is not OperationCanceledException)
        {
            EntryPoint.ShowError(window, dlg.Error);
        }
        return null;
    }

    internal static IEnumerable<COMProcessEntry> LoadProcesses(IEnumerable<int> pids, IWin32Window window, COMRegistry registry)
    {
        return LoadProcesses(pids.Select(p => Process.GetProcessById(p)), window, registry);
    }

    internal static IEnumerable<COMProcessEntry> LoadProcesses(IWin32Window window, COMRegistry registry)
    {
        int current_pid = Process.GetCurrentProcess().Id;
        var procs = Process.GetProcesses().Where(p => p.Id != current_pid).OrderBy(p => p.ProcessName);
        return LoadProcesses(procs, window, registry);
    }

    private class ReportQueryProgress
    {
        private readonly int _total_count;
        private int _current;
        private readonly IProgress<Tuple<string, int>> _progress;
        private const int MINIMUM_REPORT_SIZE = 25;

        public ReportQueryProgress(IProgress<Tuple<string, int>> progress, int total_count)
        {
            _total_count = total_count;
            _progress = progress;
        }

        public void Report()
        {
            int current = Interlocked.Increment(ref _current);
            if (current % MINIMUM_REPORT_SIZE == 1)
            {
                _progress.Report(new Tuple<string, int>($"Querying Interfaces: {current} of {_total_count}",
                    100 * current / _total_count));
            }
        }
    }

    private static bool QueryAllInterfaces(IEnumerable<ICOMClassEntry> clsids, IProgress<Tuple<string, int>> progress, CancellationToken token, int concurrent_queries)
    {
        ParallelOptions po = new();
        po.CancellationToken = token;
        po.MaxDegreeOfParallelism = concurrent_queries;

        ReportQueryProgress query_progress = new(progress, clsids.Count());

        Parallel.ForEach(clsids, po, clsid =>
        {
            po.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                query_progress.Report();
                clsid.LoadSupportedInterfaces(false, null);
            }
            catch
            {
            }
        });

        return true;
    }

    internal static bool QueryAllInterfaces(IWin32Window parent, IEnumerable<ICOMClassEntry> clsids, IEnumerable<COMServerType> server_types, int concurrent_queries, bool refresh_interfaces)
    {
        using WaitingDialog dlg = new(
            (p, t) => QueryAllInterfaces(clsids.Where(c => (refresh_interfaces || !c.InterfacesLoaded) && server_types.Contains(c.DefaultServerType)),
                        p, t, concurrent_queries),
            s => s);
        dlg.Text = "Querying Interfaces";
        return dlg.ShowDialog(parent) == DialogResult.OK;
    }

    internal static Dictionary<int, HashSet<string>> GetServicePids()
    {
        var group = ServiceUtils.GetRunningServicesWithProcessIds().GroupBy(s => s.ProcessId);
        return group.ToDictionary(s => s.Key, g => new HashSet<string>(g.Select(s => s.Name), StringComparer.OrdinalIgnoreCase));
    }

    internal static bool IsAdministrator()
    {
        using WindowsIdentity id = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
    }

    internal static string GetCOMDllName()
    {
        if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "combase.dll")))
        {
            return "combase";
        }
        else
        {
            return "ole32";
        }
    }

    internal static int GetProcessIdFromIPid(Guid ipid)
    {
        return BitConverter.ToUInt16(ipid.ToByteArray(), 4);
    }

    internal static int GetApartmentIdFromIPid(Guid ipid)
    {
        return BitConverter.ToInt16(ipid.ToByteArray(), 6);
    }

    internal static string GetApartmentIdStringFromIPid(Guid ipid)
    {
        int appid = GetApartmentIdFromIPid(ipid);
        return appid switch
        {
            0 => "NTA",
            -1 => "MTA",
            _ => $"STA (Thread ID {appid})",
        };
    }

    public static ServerInformation? GetServerInformation(object obj, IEnumerable<COMInterfaceEntry> intfs)
    {
        IntPtr intf = Marshal.GetIUnknownForObject(obj);
        IntPtr proxy = IntPtr.Zero;
        try
        {
            foreach (var entry in intfs)
            {
                if (!entry.HasProxy)
                    continue;
                Guid iid = entry.Iid;
                if (Marshal.QueryInterface(intf, ref iid, out proxy) != 0)
                {
                    continue;
                }
                ServerInformation info = new();
                if (NativeMethods.CoDecodeProxy(Process.GetCurrentProcess().Id, 
                    proxy.ToInt64(), out info) == 0)
                {
                    return info;
                }
            }
        }
        finally
        {
            Marshal.Release(intf);
            if (proxy != IntPtr.Zero)
            {
                Marshal.Release(proxy);
            }
        }
        return null;
    }

    private static readonly Lazy<string> _assembly_version = new(() =>
    {
        Assembly asm = Assembly.GetCallingAssembly();
        return asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    });

    public static string GetVersion()
    {
        return _assembly_version.Value;
    }

    public static string FormatBitness(bool is64bit)
    {
        return is64bit ? "64 bit" : "32 bit";
    }

    public static T GetGuidEntry<T>(this IDictionary<Guid, T> dict, Guid guid)
    {
        if (dict.ContainsKey(guid))
        {
            return dict[guid];
        }
        return default;
    }

    public static IntPtr CreateInstance(Guid clsid, Guid iid, CLSCTX context, string server, COMAuthInfo auth_info = null)
    {
        IntPtr pInterface = IntPtr.Zero;
        int hr = 0;
        if (!string.IsNullOrWhiteSpace(server))
        {
            MULTI_QI[] qis = new MULTI_QI[1];
            qis[0] = new MULTI_QI(iid);
            using var list = new DisposableList();
            using var auth_info_buffer = auth_info?.ToBuffer(list);
            COSERVERINFO server_info = new(server, auth_info_buffer);
            try
            {
                hr = NativeMethods.CoCreateInstanceEx(clsid, IntPtr.Zero, CLSCTX.REMOTE_SERVER, server_info, 1, qis);
                if (hr == 0)
                {
                    hr = qis[0].HResult();
                    if (hr == 0)
                    {
                        pInterface = qis[0].GetObjectPointer();
                    }
                }
            }
            finally
            {
                ((IDisposable)qis[0]).Dispose();
            }
        }
        else
        {
            hr = NativeMethods.CoCreateInstance(clsid, IntPtr.Zero, context, iid, out pInterface);
        }

        if (hr != 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        return pInterface;
    }

    public static object CreateInstanceAsObject(Guid clsid, Guid iid, CLSCTX context, string server, COMAuthInfo auth_info = null)
    {
        IntPtr pObject = CreateInstance(clsid, iid, context, server, auth_info);
        object ret = null;

        if (pObject != IntPtr.Zero)
        {
            ret = Marshal.GetObjectForIUnknown(pObject);
            Marshal.Release(pObject);
        }

        return ret;
    }

    public static object CreateInstanceFromFactory(IClassFactoryWrapper factory, Guid iid)
    {
        factory.CreateInstance(null, iid, out object ret);
        return ret;
    }

    public static object CreateClassFactory(Guid clsid, Guid iid, CLSCTX context, string server, COMAuthInfo auth_info = null)
    {
        using var list = new DisposableList();
        using var auth_info_buffer = auth_info?.ToBuffer(list);
        COSERVERINFO server_info = !string.IsNullOrWhiteSpace(server) ? new COSERVERINFO(server, auth_info_buffer) : null;

        int hr = NativeMethods.CoGetClassObject(clsid, server_info is not null ? CLSCTX.REMOTE_SERVER
            : context, server_info, iid, out IntPtr obj);
        if (hr != 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        object ret = Marshal.GetObjectForIUnknown(obj);
        Marshal.Release(obj);
        return ret;
    }

    private static Guid CLSID_NewMoniker = new("ecabafc6-7f19-11d2-978e-0000f8757e2a");

    private static IMoniker ParseMoniker(IBindCtx bind_context, string moniker_string)
    {
        if (moniker_string == "new")
        {
            int hr = NativeMethods.CoCreateInstance(CLSID_NewMoniker, IntPtr.Zero,
                CLSCTX.INPROC_SERVER, COMKnownGuids.IID_IUnknown, out IntPtr unk);
            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            try
            {
                return (IMoniker)Marshal.GetObjectForIUnknown(unk);
            }
            finally
            {
                Marshal.Release(unk);
            }
        }
        else
        {
            if (moniker_string.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
                moniker_string.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                moniker_string.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
            {
                int hr = NativeMethods.CreateURLMonikerEx(null, moniker_string, out IMoniker moniker, CreateUrlMonikerFlags.Uniform);
                if (hr != 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
                return moniker;
            }

            return NativeMethods.MkParseDisplayName(bind_context, moniker_string, out int eaten);
        }
    }

    private static IMoniker ParseMoniker(IBindCtx bind_context, string moniker_string, bool composite)
    {
        if (composite)
        {
            IMoniker ret_moniker = null;
            foreach (string m in moniker_string.Split('!'))
            {
                IMoniker moniker = ParseMoniker(bind_context, m);
                ret_moniker?.ComposeWith(moniker, false, out moniker);
                ret_moniker = moniker;
            }
            return ret_moniker;
        }
        else
        {
            return ParseMoniker(bind_context, moniker_string);
        }
    }

    public static IMoniker ParseMoniker(string moniker_string, bool composite)
    {
        IBindCtx bind_context = NativeMethods.CreateBindCtx(0);
        return ParseMoniker(bind_context, moniker_string, composite);
    }

    public static object ParseAndBindMoniker(string moniker_string, bool composite)
    {
        IBindCtx bind_context = NativeMethods.CreateBindCtx(0);
        IMoniker moniker = ParseMoniker(bind_context, moniker_string, composite);
        Guid iid = COMKnownGuids.IID_IUnknown;
        moniker.BindToObject(bind_context, null, ref iid, out object comObj);
        return comObj;
    }

    public static bool IsProxy(object obj)
    {
        return obj is IRpcOptions;
    }

    public static RPCOPT_SERVER_LOCALITY_VALUES GetServerLocality(object obj)
    {
        if (obj is IRpcOptions opts)
        {
            opts.Query(obj, RPCOPT_PROPERTIES.SERVER_LOCALITY, out IntPtr value);
            return (RPCOPT_SERVER_LOCALITY_VALUES)value.ToInt32();
        }
        return RPCOPT_SERVER_LOCALITY_VALUES.PROCESS_LOCAL;
    }

    public static StorageWrapper CreateStorage(string name, STGM mode, STGFMT format)
    {
        Guid iid = typeof(IStorage).GUID;
        return new StorageWrapper(NativeMethods.StgCreateStorageEx(name, mode, format, 0, null, IntPtr.Zero, iid));
    }

    public static StorageWrapper CreateReadOnlyStorage(string name)
    {
        return CreateStorage(name, STGM.SHARE_EXCLUSIVE | STGM.READ, STGFMT.Storage);
    }

    public static StorageWrapper OpenStorage(string name, STGM mode, STGFMT format)
    {
        Guid iid = typeof(IStorage).GUID;
        return new StorageWrapper(NativeMethods.StgOpenStorageEx(name, mode, format, 0, null, IntPtr.Zero, iid));
    }

    public static StorageWrapper OpenReadOnlyStorage(string name)
    {
        return OpenStorage(name, STGM.SHARE_EXCLUSIVE | STGM.READ, STGFMT.Storage);
    }

    public static void RegisterActivationFilter(IActivationFilter filter)
    {
        int hr = NativeMethods.CoRegisterActivationFilter(filter);
        if (hr != 0)
        {
            throw new Win32Exception(hr);
        }
    }

    public static IRuntimeBroker CreateBroker(bool per_user)
    {
        if (per_user)
        {
            return (IRuntimeBroker)new PerUserRuntimeBrokerClass();
        }
        else
        {
            return (IRuntimeBroker)new RuntimeBrokerClass();
        }
    }
}
