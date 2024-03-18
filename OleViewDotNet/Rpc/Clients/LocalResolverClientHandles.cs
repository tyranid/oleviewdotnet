//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2024
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
using NtApiDotNet.Ndr.Marshal;
using NtApiDotNet.Win32;
using System;
using System.Runtime.InteropServices;

namespace OleViewDotNet.Rpc.Clients;

internal sealed class LocalResolverClientHandles
{
    private readonly static Lazy<LocalResolverClientHandles> m_instance = 
        new(() => new LocalResolverClientHandles());

    private static ISymbolResolver GetResolver()
    {
        string dbghelp = ProgramSettings.DbgHelpPath;
        if (string.IsNullOrWhiteSpace(dbghelp))
        {
            return null;
        }

        return SymbolResolver.Create(NtProcess.Current, dbghelp, ProgramSettings.SymbolPath);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ContextHandle
    {
        public IntPtr Ptr;
        public uint Magic;
        public int Attributes;
        public Guid Uuid;
    }

    private LocalResolverClientHandles()
    {
        using ISymbolResolver resolver = GetResolver();
        if (resolver is null)
            return;

        IntPtr ptr = resolver.GetAddressOfSymbol("combase!CRpcResolver::_ph");
        if (ptr != IntPtr.Zero)
        {
            ptr = Marshal.ReadIntPtr(ptr);
            if (ptr != IntPtr.Zero)
            {
                ContextHandle handle = Marshal.PtrToStructure<ContextHandle>(ptr);
                if (handle.Magic == 0xFEDCBA98U)
                {
                    Handle = new NdrContextHandle(handle.Attributes, handle.Uuid);
                }
            }
        }

        ptr = resolver.GetAddressOfSymbol("combase!CRpcResolver::_ProcessSignature");
        if (ptr == IntPtr.Zero)
            return;
        ProcessSignature = Marshal.ReadInt64(ptr);
    }

    public static LocalResolverClientHandles Instance => m_instance.Value;

    public NdrContextHandle Handle { get; }
    public long ProcessSignature { get; }
}