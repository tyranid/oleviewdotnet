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
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Utilities;

public static class AppUtilities
{
    public static ProgramArchitecture CurrentArchitecture => RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X86 => ProgramArchitecture.X86,
        Architecture.X64 => ProgramArchitecture.X64,
        Architecture.Arm64 => ProgramArchitecture.Arm64,
        _ => ProgramArchitecture.X64,
    };

    internal static readonly bool IsWindows81OrLess = Environment.OSVersion.Version < new Version(6, 4);
    internal static readonly bool IsWindows10RS2OrLess = Environment.OSVersion.Version < new Version(10, 0, 16299);
    internal static readonly bool IsWindows10RS3OrLess = Environment.OSVersion.Version < new Version(10, 0, 17134);
    internal static readonly bool IsWindows10RS4OrLess = Environment.OSVersion.Version < new Version(10, 0, 17763);
    internal static readonly bool IsWindows101909OrLess = Environment.OSVersion.Version < new Version(10, 0, 19041);
    internal static readonly bool IsWindows1121H2OrLess = Environment.OSVersion.Version < new Version(10, 0, 22000);

    public static string GetAppDirectory()
    {
        return Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
    }

    public static string GetNativeAppDirectory()
    {
        return Path.Combine(GetAppDirectory(), CurrentArchitecture.ToString());
    }

    public static string Get32bitExePath()
    {
        string path = Path.Combine(GetAppDirectory(), "OleViewDotNet32.exe");
        if (!File.Exists(path))
        {
            path = GetExePath();
        }
        return path;
    }

    public static string GetExePath()
    {
        return Path.Combine(GetAppDirectory(), "OleViewDotNet.exe");
    }

    public static string GetPluginDirectory()
    {
        return Path.Combine(GetAppDirectory(), "plugin", CurrentArchitecture.ToString());
    }

    internal static Win32ProcessConfig GetConfigForArchitecture(ProgramArchitecture arch, string command_line)
    {
        Win32ProcessConfig config = new()
        {
            CommandLine = $"OleViewDotNet {command_line}",
            ApplicationName = GetExePath()
        };

        if (IsWindows1121H2OrLess)
        {
            if (arch == ProgramArchitecture.X86)
            {
                config.ApplicationName = Get32bitExePath();
            }
        }
        else
        {
            config.MachineType = arch switch
            {
                ProgramArchitecture.Arm64 => DllMachineType.ARM64,
                ProgramArchitecture.X64 => DllMachineType.AMD64,
                ProgramArchitecture.X86 => DllMachineType.I386,
                _ => throw new ArgumentException("Unsupported architecture."),
            };
        }
        return config;
    }

    public static void StartArchProcess(ProgramArchitecture arch, string command_line)
    {
        Win32ProcessConfig config = GetConfigForArchitecture(arch, command_line);
        Win32Process.CreateProcess(config).Dispose();
    }

    public static void StartProcess(string command_line)
    {
        StartArchProcess(CurrentArchitecture, command_line);
    }
}
