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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OleViewDotNet.Utilities;

public static class WinRTNameUtils
{
    private static readonly ConcurrentDictionary<string, string> _demangled_names = new();

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

    private static string DemangleGenericType(string name)
    {
        name = name.Replace("__F", "~").Replace("__C", ".").TrimStart('_');
        return ReadType(ref name);
    }

    // TODO: This isn't exactly correct, but can't find any good documentation.
    public static string DemangleName(string name, Guid? iid = null, bool ignore_cache = false)
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

}