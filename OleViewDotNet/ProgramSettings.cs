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

using OleViewDotNet.Properties;
using OleViewDotNet.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OleViewDotNet;

internal static class ProgramSettings
{
    public static bool IsX86 => COMUtilities.CurrentArchitecture == ProgramArchitecture.X86;

    public static bool IsAmd64 => COMUtilities.CurrentArchitecture == ProgramArchitecture.X64;

    public static bool IsArm64 => COMUtilities.CurrentArchitecture == ProgramArchitecture.Arm64;

    public static string DbgHelpPath
    {
        get
        {
            if (IsX86)
            {
                return Settings.Default.DbgHelpPath32;
            }
            else if (IsAmd64)
            {
                return Settings.Default.DbgHelpPath64;
            }
            else if (IsArm64)
            {
                return Settings.Default.DbgHelpPathArm64;
            }
            return "dbghelp.dll";
        }
        set
        {
            if (IsX86)
            {
                Settings.Default.DbgHelpPath32 = value;
            }
            else if (IsAmd64)
            {
                Settings.Default.DbgHelpPath64 = value;
            }
            else if (IsArm64)
            {
                Settings.Default.DbgHelpPathArm64 = value;
            }
        }
    }

    public static bool EnableSaveOnExit
    {
        get
        {
            if (IsX86)
            {
                return Settings.Default.EnableSaveOnExit32;
            }
            else if (IsAmd64)
            {
                return Settings.Default.EnableSaveOnExit64;
            }
            else if (IsArm64)
            {
                return Settings.Default.EnableSaveOnExitArm64;
            }
            return false;
        }
        set
        {
            if (IsX86)
            {
                Settings.Default.EnableSaveOnExit32 = value;
            }
            else if (IsAmd64)
            {
                Settings.Default.EnableSaveOnExit64 = value;
            }
            else if (IsArm64)
            {
                Settings.Default.EnableSaveOnExitArm64 = value;
            }
        }
    }

    public static void Save(IWin32Window window)
    {
        try
        {
            Settings.Default.Save();
        }
        catch (Exception ex)
        {
            EntryPoint.ShowError(window, ex);
        }
    }
}