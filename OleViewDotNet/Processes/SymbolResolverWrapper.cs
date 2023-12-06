//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2017
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
using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;

namespace OleViewDotNet.Processes;

internal class SymbolResolverWrapper : ISymbolResolver
{
    private readonly ISymbolResolver _resolver;
    private readonly SymbolLoadedModule _base_module;
    private readonly Dictionary<string, int> _resolved;
    private static readonly Dictionary<string, int> _resolved_32bit = new();
    private static readonly Dictionary<string, int> _resolved_64bit = new();
    private static readonly string _dllname = COMUtilities.GetCOMDllName();
    private static readonly string _dllprefix = $"{_dllname}!";

    public SymbolResolverWrapper(bool is64bit, ISymbolResolver resolver)
    {
        _resolver = resolver;
        foreach (var module in _resolver.GetLoadedModules())
        {
            if (module.Name.Equals(_dllname, StringComparison.OrdinalIgnoreCase))
            {
                _base_module = module;
                break;
            }
        }

        _resolved = is64bit ? _resolved_64bit : _resolved_32bit;
    }

    internal static Dictionary<string, int> GetResolved32Bit()
    {
        return _resolved_32bit;
    }

    internal static Dictionary<string, int> GetResolvedNative()
    {
        if (Environment.Is64BitProcess)
        {
            return _resolved_64bit;
        }
        else
        {
            return _resolved_32bit;
        }
    }

    public IEnumerable<SymbolLoadedModule> GetLoadedModules()
    {
        return _resolver.GetLoadedModules();
    }

    public IEnumerable<SymbolLoadedModule> GetLoadedModules(bool refresh)
    {
        return _resolver.GetLoadedModules(refresh);
    }

    public SymbolLoadedModule GetModuleForAddress(IntPtr address)
    {
        return _resolver.GetModuleForAddress(address);
    }

    public SymbolLoadedModule GetModuleForAddress(IntPtr address, bool refresh)
    {
        return _resolver.GetModuleForAddress(address, refresh);
    }

    public string GetModuleRelativeAddress(IntPtr address)
    {
        return _resolver.GetModuleRelativeAddress(address);
    }

    public string GetModuleRelativeAddress(IntPtr address, bool refresh)
    {
        return _resolver.GetModuleRelativeAddress(address, refresh);
    }

    public IntPtr GetAddressOfSymbol(string symbol)
    {
        if (_resolved.ContainsKey(symbol) && _base_module != null && symbol.StartsWith(_dllprefix))
        {
            return _base_module.BaseAddress + _resolved[symbol];
        }

        IntPtr ret = _resolver.GetAddressOfSymbol(symbol);
        if (ret != IntPtr.Zero && symbol.StartsWith(_dllprefix))
        {
            _resolved[symbol] = (int)(ret.ToInt64() - _base_module.BaseAddress.ToInt64());
        }

        return ret;
    }

    public string GetSymbolForAddress(IntPtr address)
    {
        return _resolver.GetSymbolForAddress(address);
    }

    public string GetSymbolForAddress(IntPtr address, bool generate_fake_symbol)
    {
        return _resolver.GetSymbolForAddress(address, generate_fake_symbol);
    }

    public string GetSymbolForAddress(IntPtr address, bool generate_fake_symbol, bool return_name_only)
    {
        return _resolver.GetSymbolForAddress(address, generate_fake_symbol, return_name_only);
    }

    public void Dispose()
    {
        _resolver.Dispose();
    }

    public void ReloadModuleList()
    {
        _resolver.ReloadModuleList();
    }

    public void LoadModule(string module_path, IntPtr base_address)
    {
        _resolver.LoadModule(module_path, base_address);
    }
}
