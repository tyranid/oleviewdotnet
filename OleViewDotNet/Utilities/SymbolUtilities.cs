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

using NtApiDotNet.Win32;
using OleViewDotNet.Database;
using OleViewDotNet.Processes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace OleViewDotNet.Utilities;

public static class SymbolUtilities
{
    private static string GetFileMD5(string file)
    {
        using var stm = File.OpenRead(file);
        return BitConverter.ToString(MD5.Create().ComputeHash(stm)).Replace("-", string.Empty);
    }

    private static void AddToDictionary(Dictionary<string, int> base_dict, Dictionary<string, int> add_dict)
    {
        foreach (var pair in add_dict)
        {
            base_dict[pair.Key] = pair.Value;
        }
    }

    private static bool _cached_symbols_configured;

    public static void SetupCachedSymbols()
    {
        if (!_cached_symbols_configured)
        {
            _cached_symbols_configured = true;
            // Load any supported symbol files.
            AddToDictionary(SymbolResolverWrapper.GetResolvedNative(), GetSymbolFile(true));
            if (Environment.Is64BitProcess)
            {
                AddToDictionary(SymbolResolverWrapper.GetResolved32Bit(), GetSymbolFile(false));
            }
        }
    }

    public static void ClearCachedSymbols()
    {
        _cached_symbols_configured = false;
        SymbolResolverWrapper.GetResolvedNative().Clear();
        SymbolResolverWrapper.GetResolved32Bit().Clear();
    }

    public static Dictionary<string, int> GetSymbolFile(bool native)
    {
        var ret = new Dictionary<string, int>();
        try
        {
            string system_path = native ? Environment.SystemDirectory : Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
            string dll_path = Path.Combine(system_path, $"{COMUtilities.GetCOMDllName()}.dll");
            string symbol_path = Path.Combine(COMUtilities.GetAppDirectory(), "symbol_cache", $"{GetFileMD5(dll_path)}.sym");
            foreach (var line in File.ReadAllLines(symbol_path).Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith("#")))
            {
                string[] parts = line.Split(new char[] { ' ' }, 2);
                if (parts.Length != 2)
                {
                    continue;
                }
                if (!int.TryParse(parts[0], out int address))
                {
                    continue;
                }
                ret[parts[1]] = address;
            }
        }
        catch
        {
        }
        return ret;
    }

    public static void GenerateSymbolFile(string symbol_dir)
    {
        COMProcessParserConfig config = new()
        {
            ParseStubMethods = true,
            ResolveMethodNames = true,
            ParseRegisteredClasses = true,
            ParseClients = true,
            ParseActivationContext = false,
        };

        var proc = COMProcessParser.ParseProcess(Process.GetCurrentProcess().Id, config, COMRegistry.Load(COMRegistryMode.UserOnly));
        Dictionary<string, int> entries;
        if (Environment.Is64BitProcess)
        {
            entries = SymbolResolverWrapper.GetResolvedNative();
        }
        else
        {
            entries = SymbolResolverWrapper.GetResolved32Bit();
        }

        string dll_name = COMUtilities.GetCOMDllName();
        var symbols = entries.Where(p => p.Key.StartsWith(dll_name));

        if (!symbols.Any())
        {
            throw new ArgumentException("Couldn't parse the process information. Incorrect symbols?");
        }

        var module = SafeLoadLibraryHandle.GetModuleHandle(dll_name);
        string output_file = Path.Combine(symbol_dir, $"{GetFileMD5(module.FullPath)}.sym");
        List<string> lines = new();
        lines.Add($"# {Environment.OSVersion.VersionString} {module.FullPath} {FileVersionInfo.GetVersionInfo(module.FullPath).FileVersion}");
        lines.AddRange(symbols.Select(p => $"{p.Value} {p.Key}"));
        File.WriteAllLines(output_file, lines);
    }
}