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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OleViewDotNet.TypeLib.Instance;

public sealed class COMTypeVariableDescriptor
{
    internal readonly IntPtr m_ptr;

    public VARDESC Descriptor { get; }

    internal COMTypeVariableDescriptor(IntPtr ptr)
    {
        m_ptr = ptr;
        Descriptor = ptr.GetStructure<VARDESC>();
    }
}

public sealed class COMTypeFunctionDescriptor
{
    internal readonly IntPtr m_ptr;

    public FUNCDESC Descriptor { get; }

    internal COMTypeFunctionDescriptor(IntPtr ptr)
    {
        m_ptr = ptr;
        Descriptor = ptr.GetStructure<FUNCDESC>();
    }
}

public sealed class COMTypeInfoInstance : IDisposable
{
    private readonly ITypeInfo m_type_info;
    private readonly ITypeInfo2 m_type_info2;
    private readonly ConcurrentDictionary<int, COMTypeVariableDescriptor> m_var_desc = new();
    private readonly ConcurrentDictionary<int, COMTypeFunctionDescriptor> m_func_desc = new();

    private ITypeInfo2 GetTypeInfo2()
    {
        return m_type_info2 ?? throw new NotSupportedException("Method is not supported.");
    }

    internal COMTypeInfoInstance(ITypeInfo type_info)
    {
        m_type_info = type_info;
        m_type_info2 = m_type_info as ITypeInfo2;
    }

    public static COMTypeInfoInstance FromObject(object obj)
    {
        IDispatch disp = (IDispatch)obj;
        disp.GetTypeInfo(0, 0x409, out ITypeInfo type_info);
        return new(type_info);
    }

    public TYPEATTR TypeAttr
    {
        get
        {
            m_type_info.GetTypeAttr(out IntPtr ppTypeAttr);
            try
            {
                return ppTypeAttr.GetStructure<TYPEATTR>();
            }
            finally
            {
                m_type_info.ReleaseTypeAttr(ppTypeAttr);
            }
        }
    }

    public COMTypeDocumentation Documentation => GetDocumentation(-1);

    internal ITypeInfo Instance => m_type_info;

    public COMTypeCompInstance GetTypeComp()
    {
        m_type_info.GetTypeComp(out ITypeComp ppTComp);
        return new(ppTComp);
    }

    public COMTypeFunctionDescriptor GetFuncDesc(int index)
    {
        return m_func_desc.GetOrAdd(index, i =>
        {
            m_type_info.GetFuncDesc(index, out IntPtr ppFuncDesc);
            return new(ppFuncDesc);
        });
    }

    public COMTypeVariableDescriptor GetVarDesc(int index)
    {
        return m_var_desc.GetOrAdd(index, i =>
        {
            m_type_info.GetVarDesc(index, out IntPtr ppVarDesc);
            return new(ppVarDesc);
        });
    }

    public IReadOnlyList<string> GetNames(int memid, int max_names)
    {
        string[] names = new string[max_names];

        m_type_info.GetNames(memid, names, names.Length, out int length);
        return names.Take(length).ToList().AsReadOnly();
    }

    public int GetRefTypeOfImplType(int index)
    {
        m_type_info.GetRefTypeOfImplType(index, out int href);
        return href;
    }

    public IMPLTYPEFLAGS GetImplTypeFlags(int index)
    {
        m_type_info.GetImplTypeFlags(index, out IMPLTYPEFLAGS pImplTypeFlags);
        return pImplTypeFlags;
    }

    public IReadOnlyList<int> GetIDsOfNames(IEnumerable<string> names)
    {
        string[] names_arr = names.ToArray();
        int[] ids = new int[names_arr.Length];

        m_type_info.GetIDsOfNames(names_arr, names_arr.Length, ids);
        return ids.ToList().AsReadOnly();
    }

    public void Invoke(object pvInstance, int memid, short wFlags, DISPPARAMS pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, out int puArgErr)
    {
        m_type_info.Invoke(pvInstance, memid, wFlags, ref pDispParams, pVarResult, pExcepInfo, out puArgErr);
    }

    public COMTypeDocumentation GetDocumentation(int index)
    {
        return new COMTypeDocumentation(m_type_info, index);
    }

    public void GetDllEntry(int memid, INVOKEKIND invKind, IntPtr pBstrDllName, IntPtr pBstrName, IntPtr pwOrdinal)
    {
        m_type_info.GetDllEntry(memid, invKind, pBstrDllName, pBstrName, pwOrdinal);
    }

    public COMTypeInfoInstance GetRefTypeInfo(int href)
    {
        m_type_info.GetRefTypeInfo(href, out ITypeInfo ppTI);
        return new COMTypeInfoInstance(ppTI);
    }

    public IntPtr AddressOfMember(int memid, INVOKEKIND kind)
    {
        m_type_info.AddressOfMember(memid, kind, out IntPtr ppv);
        return ppv;
    }

    public object CreateInstance(object unk_outer, Guid riid)
    {
        m_type_info.CreateInstance(unk_outer, ref riid, out object ppvObj);
        return ppvObj;
    }

    public string GetMops(int memid)
    {
        m_type_info.GetMops(memid, out string pBstrMops);
        return pBstrMops;
    }

    public COMTypeLibInstance GetContainingTypeLib(out int pIndex)
    {
        m_type_info.GetContainingTypeLib(out ITypeLib ppTLB, out pIndex);
        return new(ppTLB);
    }

    public COMTypeLibInstance GetContainingTypeLib()
    {
        return GetContainingTypeLib(out _);
    }

    public TYPEKIND TypeKind
    {
        get
        {
            GetTypeInfo2().GetTypeKind(out TYPEKIND pTypeKind);
            return pTypeKind;
        }
    }

    public TYPEFLAGS TypeFlags
    {
        get
        {
            GetTypeInfo2().GetTypeFlags(out int pTypeFlags);
            return (TYPEFLAGS)pTypeFlags;
        }
    }

    public int GetFuncIndexOfMemId(int memid, INVOKEKIND invKind)
    {
        GetTypeInfo2().GetFuncIndexOfMemId(memid, invKind, out int pFuncIndex);
        return pFuncIndex;
    }

    public int GetVarIndexOfMemId(int memid)
    {
        GetTypeInfo2().GetVarIndexOfMemId(memid, out int pVarIndex);
        return pVarIndex;
    }

    public object GetCustData(Guid guid)
    {
        GetTypeInfo2().GetCustData(ref guid, out object pVarVal);
        return pVarVal;
    }

    public object GetFuncCustData(int index, Guid guid)
    {
        GetTypeInfo2().GetFuncCustData(index, ref guid, out object pVarVal);
        return pVarVal;
    }

    public object GetParamCustData(int indexFunc, int indexParam, Guid guid)
    {
        GetTypeInfo2().GetParamCustData(indexFunc, indexParam, ref guid, out object pVarVal);
        return pVarVal;
    }

    public object GetVarCustData(int index, Guid guid)
    {
        GetTypeInfo2().GetVarCustData(index, ref guid, out object pVarVal);
        return pVarVal;
    }

    public object GetImplTypeCustData(int index, Guid guid)
    {
        GetTypeInfo2().GetImplTypeCustData(index, ref guid, out object pVarVal);
        return pVarVal;
    }

    public void GetDocumentation2(int memid, out string pbstrHelpString, out int pdwHelpStringContext, out string pbstrHelpStringDll)
    {
        // The definition of GetDocumentation2 seems to be wrong.
        GetTypeInfo2().GetDocumentation2(memid, out pbstrHelpString, out pdwHelpStringContext, out pbstrHelpStringDll);
    }

    public IReadOnlyList<COMTypeCustomDataItem> GetAllCustData()
    {
        return GetAllCustData(GetTypeInfo2().GetAllCustData);
    }

    public IReadOnlyList<COMTypeCustomDataItem> GetAllFuncCustData(int index)
    {
        return GetAllCustData(p => GetTypeInfo2().GetAllFuncCustData(index, p));
    }

    public IReadOnlyList<COMTypeCustomDataItem> GetAllParamCustData(int index_func, int index_param)
    {
        return GetAllCustData(p => GetTypeInfo2().GetAllParamCustData(index_func, index_param, p));
    }

    public IReadOnlyList<COMTypeCustomDataItem> GetAllVarCustData(int index)
    {
        return GetAllCustData(p => GetTypeInfo2().GetAllVarCustData(index, p));
    }

    public IReadOnlyList<COMTypeCustomDataItem> GetAllImplTypeCustData(int index)
    {
        return GetAllCustData(p => GetTypeInfo2().GetAllImplTypeCustData(index, p));
    }

    private static IReadOnlyList<COMTypeCustomDataItem> GetAllCustData(Action<IntPtr> get_all_cust_data)
    {
        using var buffer = new SafeStructureInOutBuffer<CUSTDATA>();

        get_all_cust_data(buffer.DangerousGetHandle());
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

    void IDisposable.Dispose()
    {
        foreach (var desc in m_var_desc.Values)
        {
            m_type_info.ReleaseVarDesc(desc.m_ptr);
        }
        m_var_desc.Clear();
        foreach (var desc in m_func_desc.Values)
        {
            m_type_info.ReleaseFuncDesc(desc.m_ptr);
        }
        m_func_desc.Clear();
        m_type_info.ReleaseComObject();
        m_type_info2?.ReleaseComObject();
    }
}

