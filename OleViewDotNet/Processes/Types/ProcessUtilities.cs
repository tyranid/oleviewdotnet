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
using OleViewDotNet.Interop;
using OleViewDotNet.Utilities;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace OleViewDotNet.Processes.Types;

internal static class ProcessUtilities
{
    public static string ReadHStringFull(this NtProcess process, long address)
    {
        uint callback(IntPtr c, long r, int l, IntPtr ba)
        {
            try
            {
                byte[] data = process.ReadMemory(r, l, true);
                Marshal.Copy(data, 0, ba, l);
                return 0;
            }
            catch (NtException)
            {
                return 0x8000FFFF;
            }
        }

        var machine = AppUtilities.CurrentArchitecture switch
        {
            ProgramArchitecture.X64 => DllMachineType.AMD64,
            ProgramArchitecture.X86 => DllMachineType.I386,
            ProgramArchitecture.Arm64 => DllMachineType.ARM64,
            _ => DllMachineType.AMD64,
        };

        if (NativeMethods.WindowsInspectString2(address, machine, callback, IntPtr.Zero, out int length, out long target_addr) == 0)
        {
            return Encoding.Unicode.GetString(process.ReadMemory(target_addr, length * 2));
        }

        return string.Empty;
    }

    public static string ReadHString(this NtProcess process, IntPtr address)
    {
        uint callback(IntPtr c, IntPtr r, int l, IntPtr ba)
        {
            try
            {
                byte[] data = process.ReadMemory(r.ToInt64(), l, true);
                Marshal.Copy(data, 0, ba, l);
                return 0;
            }
            catch (NtException)
            {
                return 0x8000FFFF;
            }
        }

        var machine = AppUtilities.CurrentArchitecture switch
        {
            ProgramArchitecture.X64 => DllMachineType.AMD64,
            ProgramArchitecture.X86 => DllMachineType.I386,
            ProgramArchitecture.Arm64 => DllMachineType.ARM64,
            _ => DllMachineType.AMD64,
        };

        if (NativeMethods.WindowsInspectString(address, machine, callback, IntPtr.Zero, out int length, out IntPtr target_addr) == 0)
        {
            return Encoding.Unicode.GetString(process.ReadMemory(target_addr.ToInt64(), length * 2));
        }

        return string.Empty;
    }

    public static string ReadZString(this NtProcess process, long address)
    {
        if (address == 0)
            return string.Empty;

        StringBuilder builder = new();
        char c = process.ReadMemory<char>(address);
        while (c != 0)
        {
            builder.Append(c);
            address += 2;
            c = process.ReadMemory<char>(address);
        }
        return builder.ToString();
    }

    internal static T ReadStruct<T>(this NtProcess process, long address) where T : new()
    {
        if (address <= 0)
        {
            return new T();
        }
        try
        {
            return process.ReadMemory<T>(address);
        }
        catch (NtException)
        {
            Debug.WriteLine($"Error reading address {address:X}");
            return new T();
        }
    }

}