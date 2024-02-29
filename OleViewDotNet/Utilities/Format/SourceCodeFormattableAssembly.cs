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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Policy;

namespace OleViewDotNet.Utilities.Format;

internal sealed class SourceCodeFormattableAssembly : Assembly, ICOMSourceCodeFormattable
{
    #region Private Members
    private readonly Assembly m_assembly;

    private static IEnumerable<SourceCodeFormattableType> GetComTypes(IEnumerable<Type> types, bool com_visible)
    {
        IEnumerable<Type> ret;
        if (com_visible)
        {
            ret = types.Where(t => Marshal.IsTypeVisibleFromCom(t));
        }
        else
        {
            ret = types.Where(t => Attribute.IsDefined(t, typeof(ComImportAttribute)));
        }
        return ret.Select(t => t.ToFormattable());
    }
    #endregion

    #region Internal Members
    internal SourceCodeFormattableAssembly(Assembly assembly, bool com_visible)
    {
        m_assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        ComVisible = com_visible;
    }

    internal IEnumerable<SourceCodeFormattableType> GetComClasses()
    {
        return GetComTypes(m_assembly.GetTypes().Where(t => t.IsClass), ComVisible);
    }

    internal IEnumerable<SourceCodeFormattableType> GetComInterfaces()
    {
        return GetComTypes(m_assembly.GetTypes().Where(t => t.IsInterface), ComVisible);
    }

    internal IEnumerable<SourceCodeFormattableType> GetComStructs()
    {
        var types = m_assembly.GetTypes().Where(t => t.IsValueType && !t.IsEnum);
        if (ComVisible)
        {
            types = types.Where(t => Marshal.IsTypeVisibleFromCom(t));
        }
        return types.Select(t => t.ToFormattable());
    }

    internal IEnumerable<SourceCodeFormattableType> GetComEnums()
    {
        var types = m_assembly.GetTypes().Where(t => t.IsEnum);
        if (ComVisible)
        {
            types = types.Where(t => Marshal.IsTypeVisibleFromCom(t));
        }
        return types.Select(t => t.ToFormattable());
    }

    internal bool ComVisible { get; }
    #endregion

    #region Public Properties

    public override string CodeBase => m_assembly.CodeBase;

    public override string EscapedCodeBase => m_assembly.EscapedCodeBase;

    public override string FullName => m_assembly.FullName;

    public override MethodInfo EntryPoint => m_assembly.EntryPoint;

    public override IEnumerable<Type> ExportedTypes => m_assembly.ExportedTypes.Select(t => t.ToFormattable());

    public override IEnumerable<TypeInfo> DefinedTypes => m_assembly.DefinedTypes;

    public override Evidence Evidence => m_assembly.Evidence;

    public override PermissionSet PermissionSet => m_assembly.PermissionSet;

    public override SecurityRuleSet SecurityRuleSet => m_assembly.SecurityRuleSet;

    public override Module ManifestModule => m_assembly.ManifestModule;

    public override IEnumerable<CustomAttributeData> CustomAttributes => m_assembly.CustomAttributes;

    public override bool ReflectionOnly => m_assembly.ReflectionOnly;

    public override IEnumerable<Module> Modules => m_assembly.Modules;

    public override string Location => m_assembly.Location;

    public override string ImageRuntimeVersion => m_assembly.ImageRuntimeVersion;

    public override bool GlobalAssemblyCache => m_assembly.GlobalAssemblyCache;

    public override long HostContext => m_assembly.HostContext;

    public override bool IsDynamic => m_assembly.IsDynamic;
    #endregion

    #region ICOMSourceCodeFormattable Implementation
    bool ICOMSourceCodeFormattable.IsFormattable => true;

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        if (!builder.InterfacesOnly)
        {
            builder.AppendTypes(GetComStructs());
            builder.AppendTypes(GetComEnums());
            builder.AppendTypes(GetComClasses());
        }
        builder.AppendTypes(GetComInterfaces());
    }
    #endregion

    #region Public Methods
    public override AssemblyName GetName()
    {
        return m_assembly.GetName();
    }

    public override AssemblyName GetName(bool copiedName)
    {
        return m_assembly.GetName(copiedName);
    }

    public override Type GetType(string name)
    {
        return m_assembly.GetType(name).ToFormattable();
    }

    public override Type GetType(string name, bool throwOnError)
    {
        return m_assembly.GetType(name, throwOnError).ToFormattable();
    }

    public override Type GetType(string name, bool throwOnError, bool ignoreCase)
    {
        return m_assembly.GetType(name, throwOnError, ignoreCase).ToFormattable();
    }

    public override Type[] GetExportedTypes()
    {
        return m_assembly.GetExportedTypes().Select(t => t.ToFormattable()).ToArray();
    }

    public override Type[] GetTypes()
    {
        return m_assembly.GetTypes().Select(t => t.ToFormattable()).ToArray();
    }

    public override Stream GetManifestResourceStream(Type type, string name)
    {
        return m_assembly.GetManifestResourceStream(type, name);
    }

    public override Stream GetManifestResourceStream(string name)
    {
        return m_assembly.GetManifestResourceStream(name);
    }

    public override Assembly GetSatelliteAssembly(CultureInfo culture)
    {
        return m_assembly.GetSatelliteAssembly(culture);
    }

    public override Assembly GetSatelliteAssembly(CultureInfo culture, Version version)
    {
        return m_assembly.GetSatelliteAssembly(culture, version);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        m_assembly.GetObjectData(info, context);
    }

    public override object[] GetCustomAttributes(bool inherit)
    {
        return m_assembly.GetCustomAttributes(inherit);
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
    {
        return m_assembly.GetCustomAttributes(attributeType, inherit);
    }

    public override bool IsDefined(Type attributeType, bool inherit)
    {
        return m_assembly.IsDefined(attributeType, inherit);
    }

    public override IList<CustomAttributeData> GetCustomAttributesData()
    {
        return m_assembly.GetCustomAttributesData();
    }

    public override Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore)
    {
        return m_assembly.LoadModule(moduleName, rawModule, rawSymbolStore);
    }

    public override object CreateInstance(string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
    {
        return m_assembly.CreateInstance(typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
    }

    public override Module[] GetLoadedModules(bool getResourceModules)
    {
        return m_assembly.GetLoadedModules(getResourceModules);
    }

    public override Module[] GetModules(bool getResourceModules)
    {
        return m_assembly.GetModules(getResourceModules);
    }

    public override Module GetModule(string name)
    {
        return m_assembly.GetModule(name);
    }

    public override FileStream GetFile(string name)
    {
        return m_assembly.GetFile(name);
    }

    public override FileStream[] GetFiles()
    {
        return m_assembly.GetFiles();
    }

    public override FileStream[] GetFiles(bool getResourceModules)
    {
        return m_assembly.GetFiles(getResourceModules);
    }

    public override string[] GetManifestResourceNames()
    {
        return m_assembly.GetManifestResourceNames();
    }

    public override AssemblyName[] GetReferencedAssemblies()
    {
        return m_assembly.GetReferencedAssemblies();
    }

    public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
    {
        return m_assembly.GetManifestResourceInfo(resourceName);
    }

    public override string ToString()
    {
        return m_assembly.ToString();
    }
    #endregion
}