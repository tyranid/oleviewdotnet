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

namespace OleViewDotNet.Processes;

public class COMProcessParserConfig
{
    public string DbgHelpPath { get; }
    public string SymbolPath { get; }
    public bool ParseStubMethods { get; }
    public bool ResolveMethodNames { get; }
    public bool ParseRegisteredClasses { get; }
    public bool ParseClients { get; }
    public bool ParseActivationContext { get; }

    public COMProcessParserConfig(string dbghelp_path, string symbol_path,
        bool parse_stubs_methods, bool resolve_method_names,
        bool parse_registered_classes, bool parse_clients,
        bool parse_activation_context)
    {
        DbgHelpPath = dbghelp_path;
        SymbolPath = symbol_path;
        ParseStubMethods = parse_stubs_methods;
        ResolveMethodNames = resolve_method_names;
        ParseRegisteredClasses = parse_registered_classes;
        ParseClients = parse_clients;
        ParseActivationContext = parse_activation_context;
    }
}
