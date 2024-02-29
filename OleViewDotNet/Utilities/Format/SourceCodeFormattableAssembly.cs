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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Utilities.Format;

internal sealed class SourceCodeFormattableAssembly : ICOMSourceCodeFormattable
{
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
        return ret.Select(t => new SourceCodeFormattableType(t));
    }

    public SourceCodeFormattableAssembly(Assembly assembly, bool com_visible)
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
        return types.Select(t => new SourceCodeFormattableType(t));
    }

    internal IEnumerable<SourceCodeFormattableType> GetComEnums()
    {
        var types = m_assembly.GetTypes().Where(t => t.IsEnum);
        if (ComVisible)
        {
            types = types.Where(t => Marshal.IsTypeVisibleFromCom(t));
        }
        return types.Select(t => new SourceCodeFormattableType(t));
    }

    bool ICOMSourceCodeFormattable.IsFormattable => true;

    public bool ComVisible { get; }

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
}