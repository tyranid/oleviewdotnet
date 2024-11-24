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

using NtApiDotNet;
using OleViewDotNet.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib.Instance;

public sealed class COMTypeLibInstance : IDisposable
{
    private readonly string m_path;
    private readonly ITypeLib m_type_lib;
    private readonly ITypeLib2 m_type_lib2;

    private ITypeLib2 GetTypeLib2()
    {
        return m_type_lib2 ?? throw new NotSupportedException("Method is not supported.");
    }

    internal COMTypeLibInstance(ITypeLib type_lib, string path)
    {
        m_type_lib = type_lib;
        m_type_lib2 = type_lib as ITypeLib2;
        m_path = path ?? string.Empty;
    }

    public static COMTypeLibInstance FromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));
        }

        return new(NativeMethods.LoadTypeLibEx(path, RegKind.None), path);
    }

    public static COMTypeLibInstance FromObject(object obj)
    {
        using var type_info = COMTypeInfoInstance.FromObject(obj);
        return type_info.GetContainingTypeLib();
    }

    public static COMTypeLibInstance FromRegistered(Guid type_lib_id,
        COMVersion version, int lcid)
    {
        return new(NativeMethods.LoadRegTypeLib(type_lib_id, version.Major, version.Minor, lcid), string.Empty);
    }

    public int GetTypeInfoCount()
    {
        return m_type_lib.GetTypeInfoCount();
    }

    public COMTypeInfoInstance GetTypeInfo(int index)
    {
        m_type_lib.GetTypeInfo(index, out ITypeInfo ppTI);
        return new(ppTI);
    }

    public TYPEKIND GetTypeInfoType(int index)
    {
        m_type_lib.GetTypeInfoType(index, out TYPEKIND pTKind);
        return pTKind;
    }

    public COMTypeInfoInstance GetTypeInfoOfGuid(Guid guid)
    {
        m_type_lib.GetTypeInfoOfGuid(ref guid, out ITypeInfo ppTInfo);
        return new(ppTInfo);
    }

    public TYPELIBATTR LibAttr
    {
        get
        {
            m_type_lib.GetLibAttr(out IntPtr ppTLibAttr);
            try
            {
                return ppTLibAttr.GetStructure<TYPELIBATTR>();
            }
            finally
            {
                m_type_lib.ReleaseTLibAttr(ppTLibAttr);
            }
        }
    }

    public COMTypeDocumentation Documentation => GetDocumentation(-1);

    internal ITypeLib Instance => m_type_lib;

    public COMTypeCompInstance GetTypeComp()
    {
        m_type_lib.GetTypeComp(out ITypeComp ppTComp);
        return new(ppTComp);
    }

    public COMTypeDocumentation GetDocumentation(int index)
    {
        return new(m_type_lib, index);
    }

    public bool IsName(string szNameBuf, int lHashVal)
    {
        return m_type_lib.IsName(szNameBuf, lHashVal);
    }

    public void FindName(string szNameBuf, int lHashVal, ITypeInfo[] ppTInfo, int[] rgMemId, ref short pcFound)
    {
        m_type_lib.FindName(szNameBuf, lHashVal, ppTInfo, rgMemId, ref pcFound);
    }

    public object GetCustData(Guid guid)
    {
        GetTypeLib2().GetCustData(ref guid, out object pVarVal);
        return pVarVal;
    }

    public void GetDocumentation2(int index, out string pbstrHelpString, out int pdwHelpStringContext, out string pbstrHelpStringDll)
    {
        // The definition of GetDocumentation2 seems to be wrong.
        GetTypeLib2().GetDocumentation2(index, out pbstrHelpString, out pdwHelpStringContext, out pbstrHelpStringDll);
    }

    public void GetLibStatistics(IntPtr pcUniqueNames, out int pcchUniqueNames)
    {
        GetTypeLib2().GetLibStatistics(pcUniqueNames, out pcchUniqueNames);
    }

    public IReadOnlyList<COMTypeCustomDataItem> GetAllCustData()
    {
        using var buffer = new SafeStructureInOutBuffer<CUSTDATA>();

        GetTypeLib2().GetAllCustData(buffer.DangerousGetHandle());
        try
        {
            var custdata = buffer.Result;
            return custdata.prgCustData.ReadArray<CUSTDATAITEM>(custdata.cCustData).Select(i => new COMTypeCustomDataItem(i)).ToList().AsReadOnly();
        }
        finally
        {
            NativeMethods.ClearCustData(buffer.DangerousGetHandle());
        }
    }

    public void RegisterTypeLibForUser(string help_path = null)
    {
        if (m_path is null)
        {
            throw new ArgumentNullException("To registry type library must be loaded from a path.");
        }
        NativeMethods.RegisterTypeLibForUser(m_type_lib, m_path, help_path);
    }

    public void RegisterTypeLib(string help_path = null)
    {
        if (m_path is null)
        {
            throw new ArgumentNullException("To registry type library must be loaded from a path.");
        }
        NativeMethods.RegisterTypeLib(m_type_lib, m_path, help_path);
    }

    public override string ToString()
    {
        return Documentation.Name;
    }

    void IDisposable.Dispose()
    {
        m_type_lib.ReleaseComObject();
        m_type_lib2?.ReleaseComObject();
    }

    public static void RegisterTypeLib(string path, string help_path = null)
    {
        using var type_lib = FromFile(path);
        type_lib.RegisterTypeLib(help_path);
    }

    public static void RegisterTypeLibForUser(string path, string help_path = null)
    {
        using var type_lib = FromFile(path);
        type_lib.RegisterTypeLibForUser(help_path);
    }

    public static void UnRegisterTypeLib(Guid lib_id,
                                         COMVersion version,
                                         int lcid,
                                         SYSKIND syskind)
    {
        NativeMethods.UnRegisterTypeLib(lib_id, version.Major, version.Minor, lcid, syskind);
    }

    public static void UnRegisterTypeLibForUser(Guid lib_id,
        COMVersion version,
        int lcid,
        SYSKIND syskind)
    {
        NativeMethods.UnRegisterTypeLibForUser(lib_id, version.Major, version.Minor, lcid, syskind);
    }
}

