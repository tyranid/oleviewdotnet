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

using Microsoft.Win32;
using NtApiDotNet;
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using OleViewDotNet.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;

namespace OleViewDotNet.Utilities;

internal static class RegistryUtilities
{
    public static object ReadObject(this RegistryKey rootKey, string keyName = null, string valueName = null, RegistryValueOptions options = RegistryValueOptions.None)
    {
        RegistryKey key = rootKey;

        try
        {
            if (keyName is not null)
            {
                key = rootKey.OpenSubKey(keyName);
            }

            if (key is not null)
            {
                return key.GetValue(valueName, null, options);
            }
        }
        finally
        {
            if (key is not null && key != rootKey)
            {
                key.Close();
            }
        }
        return null;
    }

    public static string ReadString(this RegistryKey rootKey, string keyName = null, string valueName = null, RegistryValueOptions options = RegistryValueOptions.None)
    {
        object valueObject = rootKey.ReadObject(keyName, valueName, options);
        string valueString = string.Empty;
        if (valueObject is not null)
        {
            valueString = valueObject.ToString();
        }

        int first_nul = valueString.IndexOf('\0');
        if (first_nul >= 0)
        {
            valueString = valueString.Substring(0, first_nul);
        }

        return valueString;
    }

    public static string ReadStringPath(this RegistryKey rootKey, string basePath, string keyName = null, string valueName = null, RegistryValueOptions options = RegistryValueOptions.None)
    {
        string filePath = rootKey.ReadString(keyName, valueName, options);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }
        return Path.Combine(basePath, filePath);
    }

    public static int ReadInt(this RegistryKey rootKey, string keyName, string valueName)
    {
        object obj = rootKey.ReadObject(keyName, valueName, RegistryValueOptions.None);
        if (obj is null)
        {
            return 0;
        }

        if (obj is int i)
        {
            return i;
        }

        if (obj is uint u)
        {
            return (int)u;
        }

        if (int.TryParse(obj.ToString(), out int ret))
        {
            return ret;
        }

        return 0;
    }

    public static bool ReadBool(this RegistryKey rootKey, string valueName = null, string keyName = null)
    {
        return rootKey.ReadInt(keyName, valueName) != 0;
    }

    public static Guid ReadGuid(this RegistryKey rootKey, string keyName, string valueName)
    {
        string guid = rootKey.ReadString(keyName, valueName);
        if (guid is not null && Guid.TryParse(guid, out Guid ret))
        {
            return ret;
        }

        return Guid.Empty;
    }

    public static COMSecurityDescriptor ReadSecurityDescriptor(this RegistryKey rootKey, string valueName = null, string keyName = null)
    {
        if (rootKey.ReadObject(keyName, valueName) is not byte[] ba)
            return null;
        var sd = SecurityDescriptor.Parse(ba, false);
        if (!sd.IsSuccess)
            return null;
        return new COMSecurityDescriptor(sd.Result);
    }

    public static IEnumerable<RegistryValue> ReadValues(this RegistryKey rootKey, string keyName = null)
    {
        RegistryKey key = rootKey;

        try
        {
            if (keyName is not null)
            {
                key = rootKey.OpenSubKey(keyName);
            }

            if (key is not null)
            {
                yield return new RegistryValue("", key.GetValue(null));
                foreach (var valueName in key.GetValueNames())
                {
                    yield return new RegistryValue(valueName, key.GetValue(valueName));
                }
            }
        }
        finally
        {
            if (key is not null && key != rootKey)
            {
                key.Close();
            }
        }
    }

    public static IEnumerable<string> ReadValueNames(this RegistryKey rootKey, string keyName = null)
    {
        RegistryKey key = rootKey;

        try
        {
            if (keyName is not null)
            {
                key = rootKey.OpenSubKey(keyName);
            }

            if (key is not null)
            {
                return key.GetValueNames();
            }
        }
        finally
        {
            if (key is not null && key != rootKey)
            {
                key.Close();
            }
        }

        return new string[0];
    }

    public static COMRegistryEntrySource GetSource(this RegistryKey key)
    {
        using NtKey native_key = NtKey.FromHandle(key.Handle.DangerousGetHandle(), false);
        string full_path = native_key.FullPath;
        if (full_path.StartsWith(@"\Registry\Machine\", StringComparison.OrdinalIgnoreCase))
        {
            return COMRegistryEntrySource.LocalMachine;
        }
        else if (full_path.StartsWith(@"\Registry\User\", StringComparison.OrdinalIgnoreCase))
        {
            return COMRegistryEntrySource.User;
        }
        return COMRegistryEntrySource.Unknown;
    }

    public static RegistryKey OpenSubKeySafe(this RegistryKey rootKey, string keyName)
    {
        try
        {
            return rootKey.OpenSubKey(keyName);
        }
        catch (SecurityException)
        {
            return null;
        }
    }

    public static bool HasSubkey(this RegistryKey key, string name)
    {
        using RegistryKey subkey = key.OpenSubKey(name);
        return subkey is not null;
    }
}
