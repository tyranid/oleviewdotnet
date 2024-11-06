//    Copyright (C) James Forshaw 2014. 2016
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
using System.Reflection.Emit;

namespace OleViewDotNet.Utilities;

public static class DynamicTypeBuilder
{
    private const string ASSEMBLY_NAME = "OleViewDynamicTypes";
    private static readonly AssemblyName _name = new(ASSEMBLY_NAME);
    private static readonly AssemblyBuilder _builder = AppDomain.CurrentDomain.DefineDynamicAssembly(_name, AssemblyBuilderAccess.RunAndSave);
    private static readonly ModuleBuilder _module = _builder.DefineDynamicModule(_name.Name, _name.Name + ".dll");
    private static readonly HashSet<string> _type_names = new();

    private static char ReplaceChar(char ch)
    {
        if (char.IsLetterOrDigit(ch) || ch == '.' || ch == '_')
        {
            return ch;
        }
        return '_';
    }

    private static string CleanupName(string name)
    {
        return new string(name.Select(c => ReplaceChar(c)).ToArray());
    }

    private static string CreateTypeName(string intf_name)
    {
        intf_name = CleanupName(intf_name);
        string name = $"{ASSEMBLY_NAME}.{intf_name}";
        if (_type_names.Add(name))
        {
            return name;
        }

        for (int i = 0; i < 100; ++i)
        {
            name = $"{ASSEMBLY_NAME}.{intf_name}_{i}";
            if (_type_names.Add(name))
            {
                return name;
            }
        }
        return $"{ASSEMBLY_NAME}.{Guid.NewGuid().ToString().Replace('-', '_')}"; ;
    }

    public static TypeBuilder DefineType(string name, TypeAttributes attributes, Type base_type)
    {
        string type_name = CreateTypeName(name);
        return _module.DefineType(type_name, attributes, base_type);
    }

    public static void DumpAssembly()
    {
        _builder.Save(_name.Name + ".dll");
    }
}