//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014, 2016
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

using OleViewDotNet.Utilities;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace OleViewDotNet;

public static class ProgramSettings
{
    private static readonly Lazy<ConfigFile> _config = new(ConfigFile.Load);

    [DataContract]
    private sealed class ConfigFile
    {
        [DataMember]
        public string DbgHelpPath { get; set; }
        [DataMember]
        public bool EnableSaveOnExit { get; set; }
        [DataMember]
        public string SymbolPath { get; set; }
        [DataMember]
        public bool ProxyParserResolveSymbols { get; set; }
        [DataMember]
        public bool SymbolsConfigured { get; set; }
        [DataMember]
        public string GuidFormat { get; set; }
        [DataMember]
        public bool ParseStubMethods { get; set; }
        [DataMember]
        public bool ResolveMethodNames { get; set; }
        [DataMember]
        public bool ParseRegisteredClasses { get; set; }
        [DataMember]
        public bool ParseClients { get; set; }
        [DataMember]
        public bool ParseActivationContext { get; set; }
        [DataMember]
        public bool AlwaysShowSourceCode { get; set; }
        [DataMember]
        public bool EnableAutoParsing { get; set; }

        public static ConfigFile Load()
        {
            try
            {
                using var reader = XmlReader.Create(Path.Combine(GetAppDataDirectory(), "config.xml"));
                DataContractSerializer serializer = new(typeof(ConfigFile));
                return (ConfigFile)serializer.ReadObject(reader);
            }
            catch
            {
                return new ConfigFile();
            }
        }

        public void Save()
        {
            try
            {
                string data_path = Path.Combine(GetAppDataDirectory());
                Directory.CreateDirectory(data_path);
                XmlWriterSettings settings = new()
                {
                    Indent = true
                };

                using XmlWriter writer = XmlWriter.Create(Path.Combine(data_path, "config.xml"), settings);
                DataContractSerializer serializer = new(typeof(ConfigFile));
                serializer.WriteObject(writer, this);
            }
            catch
            {
            }
        }
    }

    private static string GetDbgHelpPath()
    {
        if (!string.IsNullOrEmpty(_config.Value.DbgHelpPath))
            return _config.Value.DbgHelpPath;
        string path = Path.Combine(AppUtilities.GetNativeAppDirectory(), "dbghelp.dll");
        if (File.Exists(path))
            return path;
        return "dbghelp.dll";
    }

    private static string GetSymbolPath()
    {
        if (!string.IsNullOrEmpty(_config.Value.SymbolPath))
            return _config.Value.SymbolPath;
        string symbol_path = Environment.GetEnvironmentVariable("_NT_SYMBOL_PATH");
        if (!string.IsNullOrWhiteSpace(symbol_path))
            return symbol_path;
        return "srv*https://msdl.microsoft.com/download/symbols";
    }

    public static string DbgHelpPath
    {
        get => GetDbgHelpPath();
        set => _config.Value.DbgHelpPath = value;
    }

    public static bool EnableSaveOnExit
    {
        get => _config.Value.EnableSaveOnExit;
        set => _config.Value.EnableSaveOnExit = value;
    }

    public static string SymbolPath
    {
        get => GetSymbolPath();
        set => _config.Value.SymbolPath = value;
    }

    public static bool ProxyParserResolveSymbols
    {
        get => _config.Value.ProxyParserResolveSymbols;
        set => _config.Value.ProxyParserResolveSymbols = value;
    }

    public static bool SymbolsConfigured
    {
        get => _config.Value.SymbolsConfigured;
        set => _config.Value.SymbolsConfigured = value;
    }

    public static string GuidFormat
    {
        get => _config.Value.GuidFormat;
        set => _config.Value.GuidFormat = value;
    }

    public static bool ParseStubMethods
    {
        get => _config.Value.ParseStubMethods;
        set => _config.Value.ParseStubMethods = value;
    }

    public static bool ResolveMethodNames
    {
        get => _config.Value.ResolveMethodNames;
        set => _config.Value.ResolveMethodNames = value;
    }

    public static bool ParseRegisteredClasses
    {
        get => _config.Value.ParseRegisteredClasses;
        set => _config.Value.ParseRegisteredClasses = value;
    }

    public static bool ParseClients
    {
        get => _config.Value.ParseClients;
        set => _config.Value.ParseClients = value;
    }

    public static bool ParseActivationContext
    {
        get => _config.Value.ParseActivationContext;
        set => _config.Value.ParseActivationContext = value;
    }

    public static bool AlwaysShowSourceCode
    {
        get => _config.Value.AlwaysShowSourceCode;
        set => _config.Value.AlwaysShowSourceCode = value;
    }

    public static bool EnableAutoParsing
    {
        get => _config.Value.EnableAutoParsing;
        set => _config.Value.EnableAutoParsing = value;
    }

    public static void Save()
    {
        _config.Value.Save();
    }

    public static string GetAppDataDirectory()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "OleViewDotNet", AppUtilities.CurrentArchitecture.ToString());
    }

    public static string GetTypeLibDirectory()
    {
        return Path.Combine(GetAppDataDirectory(), "typelib");
    }

    public static string GetDefaultDatabasePath(bool create_directory)
    {
        string app_data = GetAppDataDirectory();
        if (create_directory)
        {
            Directory.CreateDirectory(app_data);
        }

        return Path.Combine(app_data, "default.db");
    }
}