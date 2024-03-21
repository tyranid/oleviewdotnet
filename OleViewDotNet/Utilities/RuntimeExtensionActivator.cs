//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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
using OleViewDotNet.Database;
using OleViewDotNet.Interop;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Utilities;

public class RuntimeExtensionActivator
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Blob
    {
        public int cbSize;
        public IntPtr pBlobData;
    }

    private readonly IExtensionRegistration _reg;
    private readonly IExtensionActivationContext _context;
    private readonly string _packageId;

    public object Activate()
    {
        return _reg.Activate();
    }

    public ulong HostId { get { return _context.HostId; } set { _context.HostId = value; } }
    public ulong UserContext { get { return _context.UserContext; } set { _context.UserContext = value; } }
    public ulong ComponentProcessId { get { return _context.ComponentProcessId; } set { _context.ComponentProcessId = value; } }
    public ulong RacActivationTokenId { get { return _context.RacActivationTokenId; } set { _context.RacActivationTokenId = value; } }
    public IntPtr LpacAttributes { get { return _context.LpacAttributes; } set { _context.LpacAttributes = value; } }
    public ulong ConsoleHandlesId { get { return _context.ConsoleHandlesId; } set { _context.ConsoleHandlesId = value; } }
    public uint AAMActivationId { get { return _context.AAMActivationId; } set { _context.AAMActivationId = value; } }

    public void RegisterConsoleHandles(SafeHandle stdInputHandle, SafeHandle stdOutputHandle, SafeHandle stdErrorHandle)
    {
        _context.ConsoleHandlesId = NativeMethods.CoRegisterConsoleHandles(stdInputHandle, stdOutputHandle, stdErrorHandle);
    }

    public void RegisterRacActivationToken(NtToken racActivationToken)
    {
        _context.RacActivationTokenId = NativeMethods.CoRegisterRacActivationToken(racActivationToken.Handle);
    }

    public void UseExistingHostId()
    {
        using (var procs = NtProcess.GetSessionProcesses(ProcessAccessRights.QueryLimitedInformation).ToDisposableList())
        {
            foreach (var proc in procs)
            {
                using var result = NtToken.OpenProcessToken(proc, TokenAccessRights.Query, false);
                if (!result.IsSuccess)
                {
                    continue;
                }

                var token = result.Result;
                string packageId = token.PackageFullName.TrimEnd('\0');

                if (!_packageId.Equals(packageId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var sysappid = token.GetSecurityAttributeByName("WIN://SYSAPPID");
                var hostid = token.GetSecurityAttributeByName("WIN://PKGHOSTID");
                if (hostid is null || sysappid.Values.Count() != 3)
                {
                    continue;
                }

                HostId = hostid.Values.Cast<ulong>().First();
                return;
            }
        }

        throw new ArgumentException("Can't find existing host to use for activation");
    }

    public void SetLpacAttributes(byte[] attrs)
    {
        int struct_size = Marshal.SizeOf(typeof(Blob));
        int total_size = attrs.Length + struct_size;
        IntPtr buffer = Marshal.AllocHGlobal(total_size);
        try
        {
            Blob blob = new() { cbSize = attrs.Length, pBlobData = buffer + struct_size };
            Marshal.StructureToPtr(blob, buffer, false);
            Marshal.Copy(attrs, 0, buffer + struct_size, attrs.Length);
            _context.LpacAttributes = buffer;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public RuntimeExtensionActivator(string contractId, string packageId, string activatableClassId)
    {
        if (AppUtilities.IsWindows10RS2OrLess)
        {
            throw new ArgumentException("Only supports runtime extension activation on Windows 10 RS3 and above");
        }

        _reg = NativeMethods.RoGetExtensionRegistration(contractId, packageId, activatableClassId);
        _context = (IExtensionActivationContext)_reg;
        _packageId = packageId;
    }

    public RuntimeExtensionActivator(COMRuntimeExtensionEntry extension)
        : this(extension.ContractId, extension.PackageId, extension.AppId)
    {
    }
}
