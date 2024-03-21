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

using NtApiDotNet;
using NtApiDotNet.Win32;
using OleViewDotNet.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace OleViewDotNet.Utilities;

internal static class MiscUtilities
{
    public static string EscapeString(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;
        StringBuilder builder = new();
        foreach (char ch in s)
        {
            switch (ch)
            {
                case '\\':
                    builder.Append(@"\\");
                    break;
                case '\r':
                    builder.Append(@"\r");
                    break;
                case '\n':
                    builder.Append(@"\n");
                    break;
                case '\t':
                    builder.Append(@"\t");
                    break;
                case '\f':
                    builder.Append(@"\f");
                    break;
                case '\v':
                    builder.Append(@"\v");
                    break;
                case '\b':
                    builder.Append(@"\b");
                    break;
                case '\0':
                    builder.Append(@"\0");
                    break;
                case '"':
                    builder.Append("\\\"");
                    break;
                default:
                    builder.Append(ch);
                    break;
            }
        }
        return builder.ToString();
    }

    public static IEnumerable<T[]> Partition<T>(this IEnumerable<T> values, int partition_size)
    {
        List<T> list = new();
        foreach (var value in values)
        {
            list.Add(value);
            if (list.Count > partition_size)
            {
                yield return list.ToArray();
                list.Clear();
            }
        }
        if (list.Count > 0)
            yield return list.ToArray();
    }

    internal static bool EqualsDictionary<K, V>(IReadOnlyDictionary<K, V> left, IReadOnlyDictionary<K, V> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        foreach (var pair in left)
        {
            if (!right.ContainsKey(pair.Key))
            {
                return false;
            }

            if (!right[pair.Key].Equals(pair.Value))
            {
                return false;
            }
        }

        return true;
    }

    internal static int GetHashCodeDictionary<K, V>(IReadOnlyDictionary<K, V> dict)
    {
        int hash_code = 0;
        foreach (var pair in dict)
        {
            hash_code ^= pair.Key.GetHashCode() ^ pair.Value.GetHashCode();
        }
        return hash_code;
    }

    public static string GetFileName(string path)
    {
        int index = path.LastIndexOf('\\');
        if (index < 0)
        {
            index = path.LastIndexOf('/');
        }
        if (index < 0)
        {
            return path;
        }
        return path.Substring(index + 1);
    }

    public static string GetProcessNameById(int pid)
    {
        return NtSystemInfo.GetProcessIdImagePath(pid, false)
            .Map(s => GetFileName(s)).GetResultOrDefault(string.Empty);
    }

    public static string GetPackagePath(string packageId)
    {
        int length = 0;
        Win32Error result = NativeMethods.PackageIdFromFullName(packageId, 0, ref length, SafeHGlobalBuffer.Null);
        if (result != Win32Error.ERROR_INSUFFICIENT_BUFFER)
        {
            return string.Empty;
        }

        using var buffer = new SafeHGlobalBuffer(length);
        result = NativeMethods.PackageIdFromFullName(packageId,
        0, ref length, buffer);
        if (result != 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new(260);
        length = builder.Capacity;
        result = NativeMethods.GetPackagePath(buffer, 0, ref length, builder);
        if (result != Win32Error.SUCCESS)
        {
            return string.Empty;
        }

        return builder.ToString();
    }

    internal static int GetSafeHashCode<T>(this T obj) where T : class
    {
        if (obj is null)
        {
            return 0;
        }
        return obj.GetHashCode();
    }

    internal static int GetEnumHashCode<T>(this IEnumerable<T> e)
    {
        return e.Aggregate(0, (s, o) => s ^ o.GetHashCode());
    }

    public static void CopyTextToClipboard(string text)
    {
        int tries = 10;
        while (tries > 0)
        {
            try
            {
                Clipboard.SetText(text);
                break;
            }
            catch (ExternalException)
            {
            }
            Thread.Sleep(100);
            tries--;
        }
    }

    public static string GuidToString(Guid guid, GuidFormat format_type)
    {
        return format_type switch
        {
            GuidFormat.Object => $"<object id=\"obj\" classid=\"clsid:{guid}\">NO OBJECT</object>",
            GuidFormat.String => guid.FormatGuid(),
            GuidFormat.Structure => $"GUID guidObject = {guid:X};",
            GuidFormat.HexString => string.Join(" ", guid.ToByteArray().Select(b => $"{b:X02}")),
            GuidFormat.CSGuid => $"Guid guidObject = new Guid(\"{guid}\");",
            GuidFormat.CSGuidAttribute => $"[Guid(\"{guid}\")]",
            GuidFormat.RpcUuid => $"[uuid(\"{guid}\")]",
            _ => throw new ArgumentException("Invalid guid string type", nameof(format_type)),
        };
    }

    public static void CopyGuidToClipboard(Guid guid, GuidFormat guid_format)
    {
        CopyTextToClipboard(GuidToString(guid, guid_format));
    }

    internal static string FormatGuid(this Guid guid)
    {
        return guid.ToString(ProgramSettings.GuidFormat).ToUpper();
    }

    internal static string FormatComClassNameAsCIdentifier(string comClassName)
    {
        string re = "CLSID_" + Regex.Replace(comClassName, @"[^a-zA-Z0-9]", "_");
        re = Regex.Replace(re, "__+", "_");
        return re;
    }

    internal static string FormatGuidAsCStruct(string comClassName, Guid guidToFormat)
    {
        string id = FormatComClassNameAsCIdentifier(comClassName);
        string re = GuidToString(guidToFormat, GuidFormat.Structure);
        return re.Replace("guidObject", id);
    }

    internal static string FormatGuidDefault(this Guid guid)
    {
        return guid.ToString().ToUpper();
    }

    internal static OptionalGuidClass ToOptional(this Guid? guid)
    {
        return guid.HasValue ? new(guid.Value) : null;
    }
}
