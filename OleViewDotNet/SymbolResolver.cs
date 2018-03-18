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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OleViewDotNet
{
    //sealed class SymbolLoadedModule
    //{
    //    public string Name { get; private set; }
    //    public IntPtr BaseAddress { get; private set; }
    //    public int ImageSize { get; private set; }

    //    public SymbolLoadedModule(string name, IntPtr base_address, int image_size)
    //    {
    //        Name = name;
    //        BaseAddress = base_address;
    //        ImageSize = image_size;
    //    }
    //}

    //interface ISymbolResolver
    //{
    //    IEnumerable<SymbolLoadedModule> GetLoadedModules();
    //    SymbolLoadedModule GetModuleForAddress(IntPtr address);
    //    string GetModuleRelativeAddress(IntPtr address);
    //    IntPtr GetAddressOfSymbol(string name);
    //    string GetSymbolForAddress(IntPtr address);
    //}

    //sealed class SymbolResolver : ISymbolResolver, IDisposable
    //{
    //    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    //    delegate bool SymInitializeW(
    //        IntPtr hProcess,
    //        [MarshalAs(UnmanagedType.LPWStr)] string UserSearchPath,
    //        [MarshalAs(UnmanagedType.Bool)] bool fInvadeProcess
    //    );

    //    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    //    delegate bool SymCleanup(
    //        IntPtr hProcess
    //    );

    //    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    //    delegate bool SymFromNameW(
    //          IntPtr hProcess,
    //          string Name,
    //          SafeBuffer Symbol
    //        );

    //    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    //    delegate bool EnumModules(
    //        string ModuleName,
    //        long BaseOfDll,
    //        IntPtr UserContext);

    //    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    //    delegate bool SymEnumerateModulesW64(
    //          IntPtr hProcess,
    //          EnumModules EnumModulesCallback,
    //          IntPtr UserContext
    //        );

    //    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    //    delegate bool SymFromAddrW(
    //        IntPtr hProcess,
    //        long Address,
    //        out long Displacement,
    //        SafeBuffer Symbol
    //    );

    //    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    //    delegate bool SymGetModuleInfoW64(
    //          IntPtr hProcess,
    //          long dwAddr,
    //          ref IMAGEHLP_MODULE64 ModuleInfo
    //        );

    //    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
    //    delegate long SymLoadModule64(
    //          IntPtr hProcess,
    //          IntPtr hFile,
    //          string ImageName,
    //          string ModuleName,
    //          long BaseOfDll,
    //          int SizeOfDll
    //        );

    //    enum SymTagEnum
    //    {
    //        SymTagNull,
    //        SymTagExe,
    //        SymTagCompiland,
    //        SymTagCompilandDetails,
    //        SymTagCompilandEnv,
    //        SymTagFunction,
    //        SymTagBlock,
    //        SymTagData,
    //        SymTagAnnotation,
    //        SymTagLabel,
    //        SymTagPublicSymbol,
    //        SymTagUDT,
    //        SymTagEnum,
    //        SymTagFunctionType,
    //        SymTagPointerType,
    //        SymTagArrayType,
    //        SymTagBaseType,
    //        SymTagTypedef,
    //        SymTagBaseClass,
    //        SymTagFriend,
    //        SymTagFunctionArgType,
    //        SymTagFuncDebugStart,
    //        SymTagFuncDebugEnd,
    //        SymTagUsingNamespace,
    //        SymTagVTableShape,
    //        SymTagVTable,
    //        SymTagCustom,
    //        SymTagThunk,
    //        SymTagCustomType,
    //        SymTagManagedType,
    //        SymTagDimension
    //    }

    //    [StructLayout(LayoutKind.Sequential)]
    //    class SYMBOL_INFO
    //    {
    //        public int SizeOfStruct;
    //        public int TypeIndex;        // Type Index of symbol
    //        public long Reserved1;
    //        public long Reserved2;
    //        public int Index;
    //        public int Size;
    //        public long ModBase;          // Base Address of module comtaining this symbol
    //        public int Flags;
    //        public long Value;            // Value of symbol, ValuePresent should be 1
    //        public long Address;          // Address of symbol including base address of module
    //        public int Register;         // register holding value or pointer to value
    //        public int Scope;            // scope of the symbol
    //        public SymTagEnum Tag;              // pdb classification
    //        public int NameLen;          // Actual length of name
    //        public int MaxNameLen;
    //        public char Name;

    //        public const int MAX_SYM_NAME = 2000;

    //        public SYMBOL_INFO()
    //        {
    //            SizeOfStruct = Marshal.SizeOf(typeof(SYMBOL_INFO));
    //        }

    //        public SYMBOL_INFO(int max_name_len) : this()
    //        {
    //            MaxNameLen = max_name_len;
    //        }
    //    }

    //    [Flags]
    //    enum SymOptions : uint
    //    {
    //        CASE_INSENSITIVE          = 0x00000001,
    //        UNDNAME                   = 0x00000002,
    //        DEFERRED_LOADS            = 0x00000004,
    //        NO_CPP                    = 0x00000008,
    //        LOAD_LINES                = 0x00000010,
    //        OMAP_FIND_NEAREST         = 0x00000020,
    //        LOAD_ANYTHING             = 0x00000040,
    //        IGNORE_CVREC              = 0x00000080,
    //        NO_UNQUALIFIED_LOADS      = 0x00000100,
    //        FAIL_CRITICAL_ERRORS      = 0x00000200,
    //        EXACT_SYMBOLS             = 0x00000400,
    //        ALLOW_ABSOLUTE_SYMBOLS    = 0x00000800,
    //        IGNORE_NT_SYMPATH         = 0x00001000,
    //        INCLUDE_32BIT_MODULES     = 0x00002000,
    //        PUBLICS_ONLY              = 0x00004000,
    //        NO_PUBLICS                = 0x00008000,
    //        AUTO_PUBLICS              = 0x00010000,
    //        NO_IMAGE_SEARCH           = 0x00020000,
    //        SECURE                    = 0x00040000,
    //        NO_PROMPTS                = 0x00080000,
    //        OVERWRITE                 = 0x00100000,
    //        IGNORE_IMAGEDIR           = 0x00200000,
    //        FLAT_DIRECTORY            = 0x00400000,
    //        FAVOR_COMPRESSED          = 0x00800000,
    //        ALLOW_ZERO_ADDRESS        = 0x01000000,
    //        DISABLE_SYMSRV_AUTODETECT = 0x02000000,
    //        READONLY_CACHE            = 0x04000000,
    //        SYMPATH_LAST              = 0x08000000,
    //        DISABLE_FAST_SYMBOLS      = 0x10000000,
    //        DISABLE_SYMSRV_TIMEOUT    = 0x20000000,
    //        DISABLE_SRVSTAR_ON_STARTUP = 0x40000000,
    //        DEBUG                     = 0x80000000,
    //    }

    //    enum SYM_TYPE
    //    {
    //        SymNone = 0,
    //        SymCoff,
    //        SymCv,
    //        SymPdb,
    //        SymExport,
    //        SymDeferred,
    //        SymSym, 
    //        SymDia,
    //        SymVirtual,
    //        NumSymTypes
    //    }

    //    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    //    struct IMAGEHLP_MODULE64
    //    {
    //        public int SizeOfStruct;
    //        public long BaseOfImage;
    //        public int ImageSize;
    //        public int TimeDateStamp;
    //        public int CheckSum;
    //        public int NumSyms;
    //        public SYM_TYPE SymType;
    //        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    //        public string ModuleName;
    //        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    //        public string ImageName;
    //        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    //        public string LoadedImageName;
    //        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    //        public string LoadedPdbName;
    //        public int CVSig;
    //        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260*3)]
    //        public string CVData;
    //        public int PdbSig;
    //        public Guid PdbSig70;
    //        public int PdbAge;
    //        [MarshalAs(UnmanagedType.Bool)]
    //        public bool PdbUnmatched;
    //        [MarshalAs(UnmanagedType.Bool)]
    //        public bool DbgUnmatched;
    //        [MarshalAs(UnmanagedType.Bool)]
    //        public bool LineNumbers;
    //        [MarshalAs(UnmanagedType.Bool)]
    //        public bool GlobalSymbols;
    //        [MarshalAs(UnmanagedType.Bool)]
    //        public bool TypeInfo;
    //        [MarshalAs(UnmanagedType.Bool)]
    //        public bool SourceIndexed;
    //        [MarshalAs(UnmanagedType.Bool)]
    //        public bool Publics;
    //        public int MachineType;
    //        public int Reserved;
    //    }

    //    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
    //    delegate int SymSetOptions(
    //        SymOptions SymOptions
    //    );
        
    //    [Flags]
    //    enum EnumProcessModulesFilter
    //    {
    //        LIST_MODULES_DEFAULT = 0x00,
    //        LIST_MODULES_32BIT = 0x01,
    //        LIST_MODULES_64BIT = 0x02,        
    //        LIST_MODULES_ALL = LIST_MODULES_32BIT | LIST_MODULES_64BIT,
    //    }

    //    [DllImport("Psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    //    private static extern bool EnumProcessModulesEx(
    //          SafeProcessHandle hProcess,
    //          [Out] IntPtr[] lphModule,
    //          int cb,
    //          out int lpcbNeeded,
    //          EnumProcessModulesFilter dwFilterFlag
    //        );

    //    [DllImport("Psapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    //    private static extern int GetModuleFileNameEx(
    //          SafeProcessHandle hProcess,
    //          IntPtr hModule,
    //          StringBuilder lpFilename,
    //          int nSize
    //        );

    //    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    //    private static extern SafeLibraryHandle LoadLibrary(string filename);

    //    private SafeLibraryHandle _dbghelp_lib;
    //    private SymInitializeW _sym_init;
    //    private SymCleanup _sym_cleanup;
    //    private SymFromNameW _sym_from_name;
    //    private SymSetOptions _sym_set_options;
    //    private SymEnumerateModulesW64 _sym_enum_modules;
    //    private SymFromAddrW _sym_from_addr;
    //    private SymGetModuleInfoW64 _sym_get_module_info;
    //    private SymLoadModule64 _sym_load_module;
    //    private IntPtr _process;

    //    private void GetFunc<T>(ref T f) where T : class
    //    {
    //        f = _dbghelp_lib.GetFunctionPointer<T>();
    //    }

    //    static SafeStructureInOutBuffer<SYMBOL_INFO> AllocateSymInfo()
    //    {
    //        return new SafeStructureInOutBuffer<SYMBOL_INFO>(new SYMBOL_INFO(SYMBOL_INFO.MAX_SYM_NAME), SYMBOL_INFO.MAX_SYM_NAME * 2, true);
    //    }

    //    static string GetNameFromSymbolInfo(SafeBuffer buffer)
    //    {
    //        IntPtr ofs = Marshal.OffsetOf<SYMBOL_INFO>("Name");
    //        return Marshal.PtrToStringUni(buffer.DangerousGetHandle() + ofs.ToInt32());
    //    }

    //    public SymbolResolver(string dbghelp_path, SafeProcessHandle process, string symbol_path)
    //    {
    //        _process = process.DangerousGetHandle();
    //        _dbghelp_lib = COMUtilities.SafeLoadLibrary(dbghelp_path);
    //        GetFunc(ref _sym_init);
    //        GetFunc(ref _sym_cleanup);
    //        GetFunc(ref _sym_from_name);
    //        GetFunc(ref _sym_set_options);
    //        GetFunc(ref _sym_enum_modules);
    //        GetFunc(ref _sym_from_addr);
    //        GetFunc(ref _sym_get_module_info);
    //        GetFunc(ref _sym_load_module);

    //        _sym_set_options(SymOptions.INCLUDE_32BIT_MODULES | SymOptions.UNDNAME | SymOptions.DEFERRED_LOADS);

    //        if (!_sym_init(_process, symbol_path, true))
    //        {
    //            // If SymInitialize failed then we'll have to bootstrap modules manually.
    //            if (!_sym_init(_process, symbol_path, false))
    //            {
    //                throw new Win32Exception();
    //            }
                
    //            IntPtr[] modules = new IntPtr[1024];
    //            int return_length;
    //            if (EnumProcessModulesEx(process, modules, modules.Length * IntPtr.Size, out return_length,
    //                process.Is64Bit ? EnumProcessModulesFilter.LIST_MODULES_64BIT : EnumProcessModulesFilter.LIST_MODULES_32BIT))
    //            {
    //                foreach (IntPtr module in modules.Take(return_length / IntPtr.Size))
    //                {
    //                    StringBuilder dllpath = new StringBuilder(260);
    //                    if (GetModuleFileNameEx(process, module, dllpath, dllpath.Capacity) > 0)
    //                    {
    //                        if (_sym_load_module(_process, IntPtr.Zero, dllpath.ToString(), 
    //                            Path.GetFileNameWithoutExtension(dllpath.ToString()), module.ToInt64(), 0) == 0)
    //                        {
    //                            System.Diagnostics.Debug.WriteLine(String.Format("Couldn't load {0}", dllpath));
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    private IMAGEHLP_MODULE64 GetModuleInfo(long base_address)
    //    {
    //        IMAGEHLP_MODULE64 module = new IMAGEHLP_MODULE64();
    //        module.SizeOfStruct = Marshal.SizeOf(module);
    //        if (_sym_get_module_info(_process, base_address, ref module))
    //        {
    //            return module;
    //        }
    //        return new IMAGEHLP_MODULE64();
    //    }

    //    private IEnumerable<SymbolLoadedModule> GetLoadedModulesInternal()
    //    {
    //        List<SymbolLoadedModule> modules = new List<SymbolLoadedModule>();

    //        if (!_sym_enum_modules(_process, (s, m, p) =>
    //        {
    //            modules.Add(new SymbolLoadedModule(s, new IntPtr(m), GetModuleInfo(m).ImageSize));
    //            return true;
    //        }, IntPtr.Zero))
    //        {
    //            throw new Win32Exception();
    //        }
    //        return modules.AsReadOnly();
    //    }

    //    private IEnumerable<SymbolLoadedModule> _loaded_modules;

    //    public IEnumerable<SymbolLoadedModule> GetLoadedModules()
    //    {
    //        if (_loaded_modules == null)
    //        {
    //            _loaded_modules = GetLoadedModulesInternal().OrderBy(s => s.BaseAddress.ToInt64());
    //        }
    //        return _loaded_modules;
    //    }

    //    public SymbolLoadedModule GetModuleForAddress(IntPtr address)
    //    {
    //        long check_addr = address.ToInt64();

    //        foreach (SymbolLoadedModule module in GetLoadedModules())
    //        {
    //            long base_address = module.BaseAddress.ToInt64();
    //            if (check_addr >= base_address && check_addr < base_address + module.ImageSize)
    //            {
    //                return module;
    //            }
    //        }

    //        return null;
    //    }

    //    public string GetModuleRelativeAddress(IntPtr address)
    //    {
    //        SymbolLoadedModule module = GetModuleForAddress(address);
    //        if (module == null)
    //        {
    //            return String.Format("0x{0:X}", address.ToInt64());
    //        }

    //        return String.Format("{0}+0x{1:X}", module.Name, address.ToInt64() - module.BaseAddress.ToInt64());
    //    }

    //    public IntPtr GetAddressOfSymbol(string name)
    //    {
    //        using (SafeStructureInOutBuffer<SYMBOL_INFO> sym_info = AllocateSymInfo())
    //        {
    //            if (!_sym_from_name(_process, name, sym_info))
    //            {
    //                return IntPtr.Zero;
    //            }
    //            return new IntPtr(sym_info.Result.Address);
    //        }
    //    }

    //    public string GetSymbolForAddress(IntPtr address)
    //    {
    //        using (SafeStructureInOutBuffer<SYMBOL_INFO> sym_info = AllocateSymInfo())
    //        {
    //            long displacement;
    //            if (_sym_from_addr(_process, address.ToInt64(), out displacement, sym_info))
    //            {
    //                return GetNameFromSymbolInfo(sym_info);
    //            }
    //            // Perhaps should return module+X?
    //            return String.Format("0x{0:X}", address.ToInt64());
    //        }
    //    }

    //    #region IDisposable Support
    //    private bool disposedValue = false; 

    //    void Dispose(bool disposing)
    //    {
    //        if (!disposedValue)
    //        {
    //            disposedValue = true;
    //            _sym_cleanup?.Invoke(_process);
    //            _dbghelp_lib?.Close();
    //        }
    //    }

    //    ~SymbolResolver()
    //    {
    //        Dispose(false);
    //    }

    //    void IDisposable.Dispose()
    //    {
    //        Dispose(true);
    //        GC.SuppressFinalize(this);
    //    }

    //    #endregion
    //}
}
