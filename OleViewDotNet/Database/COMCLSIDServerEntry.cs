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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using OleViewDotNet.Utilities;

namespace OleViewDotNet.Database;

public class COMCLSIDServerEntry : IXmlSerializable
{
    internal const string APPID_HOSTED = "<APPID HOSTED>";
    internal bool AppIdHosted => ServerType == COMServerType.LocalServer32 && Server == APPID_HOSTED;

    /// <summary>
    /// The absolute path to the server.
    /// </summary>
    public string Server { get; private set; }
    /// <summary>
    /// Command line for local server.
    /// </summary>
    public string CommandLine { get; private set; }
    /// <summary>
    /// The type of server entry.
    /// </summary>
    public COMServerType ServerType { get; private set; }
    /// <summary>
    /// The threading model.
    /// </summary>
    public COMThreadingModel ThreadingModel { get; private set; }
    public COMCLSIDServerDotNetEntry DotNet { get; private set; }
    public bool HasDotNet => DotNet is not null;
    public string RawServer { get; private set; }

    public override bool Equals(object obj)
    {
        if (base.Equals(obj))
        {
            return true;
        }

        if (obj is not COMCLSIDServerEntry right)
        {
            return false;
        }

        return Server == right.Server
            && CommandLine == right.CommandLine
            && ServerType == right.ServerType 
            && ThreadingModel == right.ThreadingModel
            && RawServer == right.RawServer;
    }

    public override int GetHashCode()
    {
        return Server.GetHashCode() ^ CommandLine.GetHashCode() 
            ^ ServerType.GetHashCode() ^ ThreadingModel.GetHashCode()
            ^ RawServer.GetSafeHashCode();
    }

    private static bool IsInvalidFileName(string filename)
    {
        return filename.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
    }

    private static string ProcessFileName(string filename, bool process_command_line)
    {
        string temp = filename.Trim();

        if (temp.StartsWith("\""))
        {
            int lastIndex = temp.IndexOf('"', 1);
            if (lastIndex >= 1)
            {
                filename = temp.Substring(1, lastIndex - 1);
            }
        }
        else if (process_command_line && temp.Contains(" "))
        {
            if (!File.Exists(temp))
            {
                int index = temp.IndexOf(' ');
                while (index > 0)
                {
                    string name = temp.Substring(0, index);

                    if (File.Exists(name) || (Path.GetExtension(name) == string.Empty && File.Exists(name + ".exe")))
                    {
                        filename = name;
                        break;
                    }

                    index = temp.IndexOf(' ', index + 1);
                }


                if (index < 0)
                {
                    // We've run out of options, just take the first string
                    filename = temp.Split(' ')[0];
                }
            }
        }

        filename = filename.Trim();

        try
        {
            // Expand out any short filenames
            if (filename.Contains("~") && !IsInvalidFileName(filename))
            {
                filename = Path.GetFullPath(filename);
            }
        }
        catch (IOException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (ArgumentException)
        {
        }

        return filename;
    }

    private static COMThreadingModel ReadThreadingModel(RegistryKey key)
    {
        string threading_model = key.ReadString(valueName: "ThreadingModel");
        return threading_model.ToLower() switch
        {
            "both" => COMThreadingModel.Both,
            "free" => COMThreadingModel.Free,
            "neutral" => COMThreadingModel.Neutral,
            "apartment" => COMThreadingModel.Apartment,
            _ => COMThreadingModel.None,
        };
    }

    internal COMCLSIDServerEntry(COMServerType server_type, string server, COMThreadingModel threading_model)
    {
        Server = server;
        CommandLine = string.Empty;
        ServerType = server_type;
        ThreadingModel = threading_model;
    }

    internal COMCLSIDServerEntry(COMServerType server_type, string server, string commandLine)
        : this(server_type, server, COMThreadingModel.Apartment)
    {
        CommandLine = commandLine;
    }

    internal COMCLSIDServerEntry(COMServerType server_type, string server) 
        : this(server_type, server, COMThreadingModel.Apartment)
    {
    }

    internal COMCLSIDServerEntry(COMServerType server_type) 
        : this(server_type, string.Empty)
    {
    }

    internal COMCLSIDServerEntry() : this(COMServerType.UnknownServer)
    {
    }

    internal COMCLSIDServerEntry(RegistryKey key, COMServerType server_type)
        : this(server_type)
    {
        string server_string = key.ReadString();
        RawServer = key.ReadString(options: RegistryValueOptions.DoNotExpandEnvironmentNames);

        if (string.IsNullOrWhiteSpace(server_string))
        {
            // TODO: Support weird .NET registration which registers a .NET version string.
            return;
        }

        bool process_command_line = false;
        CommandLine = string.Empty;

        if (server_type == COMServerType.LocalServer32)
        {
            CommandLine = server_string;
            string executable = key.ReadString(valueName: "ServerExecutable");
            if (!string.IsNullOrWhiteSpace(executable))
            {
                server_string = executable;
            }
            else
            {
                process_command_line = true;
            }
            ThreadingModel = COMThreadingModel.Both;
        }
        else if (server_type == COMServerType.InProcServer32)
        {
            ThreadingModel = ReadThreadingModel(key);
            if (key.GetValue("Assembly") is not null)
            {
                DotNet = new COMCLSIDServerDotNetEntry(key);
            }
        }
        else
        {
            ThreadingModel = COMThreadingModel.Apartment;
        }

        Server = ProcessFileName(server_string, process_command_line);
    }

    XmlSchema IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        Server = reader.ReadString("server");
        CommandLine = reader.ReadString("cmdline");
        ServerType = reader.ReadEnum<COMServerType>("type");
        ThreadingModel = reader.ReadEnum<COMThreadingModel>("model");
        RawServer = reader.ReadString("rawserver");
        if (reader.ReadBool("dotnet"))
        {
            IEnumerable<COMCLSIDServerDotNetEntry> service = 
                reader.ReadSerializableObjects("dotnet", () => new COMCLSIDServerDotNetEntry());
            DotNet = service.FirstOrDefault();
        }
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteOptionalAttributeString("server", Server);
        writer.WriteOptionalAttributeString("cmdline", CommandLine);
        writer.WriteEnum("type", ServerType);
        writer.WriteEnum("model", ThreadingModel);
        writer.WriteOptionalAttributeString("rawserver", RawServer);
        if (DotNet is not null)
        {
            writer.WriteBool("dotnet", true);
            writer.WriteSerializableObjects("dotnet", 
                new COMCLSIDServerDotNetEntry[] { DotNet });
        }
    }

    public override string ToString()
    {
        return Server;
    }
}
