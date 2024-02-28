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

using NtApiDotNet;
using NtApiDotNet.Win32;
using OleViewDotNet.Database;
using OleViewDotNet.Processes.Types;
using System;

namespace OleViewDotNet.Processes;

public class COMRuntimeActivableClassEntry
{
    private readonly COMRegistry m_registry;

    public Guid Clsid { get; }
    public string ActivatableClassId { get; }
    public string FullPackageName { get; }
    public string ActivationFactoryCallback { get; }
    public string ActivationFactoryCallbackPointer { get; }
    public COMRuntimeClassEntry ClassEntry
    {
        get
        {
            if (m_registry.RuntimeClasses.ContainsKey(ActivatableClassId))
            {
                return m_registry.RuntimeClasses[ActivatableClassId];
            }
            return null;
        }
    }

    internal COMRuntimeActivableClassEntry(IWinRTLocalSvrClassEntry entry, NtProcess process, ISymbolResolver resolver, COMRegistry registry)
    {
        Clsid = entry.GetClsid();
        ActivatableClassId = entry.GetActivatableClassId(process);
        FullPackageName = entry.GetPackageFullName(process);
        IntPtr callback = entry.GetActivationFactoryCallback();
        ActivationFactoryCallback = resolver.GetSymbolForAddress(callback, true);
        ActivationFactoryCallbackPointer = resolver.GetModuleRelativeAddress(callback);
        m_registry = registry;
    }

    public override string ToString()
    {
        return $"ActivatableClass: {ActivatableClassId}";
    }
}
