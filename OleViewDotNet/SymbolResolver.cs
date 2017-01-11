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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace OleViewDotNet
{
    enum SymTagEnum
    {
        SymTagNull,
        SymTagExe,
        SymTagCompiland,
        SymTagCompilandDetails,
        SymTagCompilandEnv,
        SymTagFunction,
        SymTagBlock,
        SymTagData,
        SymTagAnnotation,
        SymTagLabel,
        SymTagPublicSymbol,
        SymTagUDT,
        SymTagEnum,
        SymTagFunctionType,
        SymTagPointerType,
        SymTagArrayType,
        SymTagBaseType,
        SymTagTypedef,
        SymTagBaseClass,
        SymTagFriend,
        SymTagFunctionArgType,
        SymTagFuncDebugStart,
        SymTagFuncDebugEnd,
        SymTagUsingNamespace,
        SymTagVTableShape,
        SymTagVTable,
        SymTagCustom,
        SymTagThunk,
        SymTagCustomType,
        SymTagManagedType,
        SymTagDimension
    }

    [StructLayout(LayoutKind.Sequential)]
    class SYMBOL_INFO
    {
        public int SizeOfStruct;
        public int TypeIndex;        // Type Index of symbol
        public long Reserved1;
        public long Reserved2;
        public int Index;
        public int Size;
        public long ModBase;          // Base Address of module comtaining this symbol
        public int Flags;
        public long Value;            // Value of symbol, ValuePresent should be 1
        public long Address;          // Address of symbol including base address of module
        public int Register;         // register holding value or pointer to value
        public int Scope;            // scope of the symbol
        public SymTagEnum Tag;              // pdb classification
        public int NameLen;          // Actual length of name
        public int MaxNameLen;
        public char Name;

        public const int MAX_SYM_NAME = 2000;

        public SYMBOL_INFO()
        {
            SizeOfStruct = Marshal.SizeOf(typeof(SYMBOL_INFO));
        }

        public SYMBOL_INFO(int max_name_len) : this()
        {
            MaxNameLen = max_name_len;
        }
    }

    sealed class SymbolResolver : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool SymInitializeW(
            IntPtr hProcess,
            [MarshalAs(UnmanagedType.LPWStr)] string UserSearchPath,
            [MarshalAs(UnmanagedType.Bool)] bool fInvadeProcess
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool SymCleanup(
            IntPtr hProcess
        );
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool SymFromNameW(
              IntPtr hProcess,
              string Name,
              SafeBuffer Symbol
            );

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool EnumModules(
            string ModuleName,
            long BaseOfDll,
            IntPtr UserContext);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool SymEnumerateModulesW64(
              IntPtr hProcess,
              EnumModules EnumModulesCallback,
              IntPtr UserContext
            );

        [Flags]
        enum SymOptions : uint
        {
            CASE_INSENSITIVE          = 0x00000001,
            UNDNAME                   = 0x00000002,
            DEFERRED_LOADS            = 0x00000004,
            NO_CPP                    = 0x00000008,
            LOAD_LINES                = 0x00000010,
            OMAP_FIND_NEAREST         = 0x00000020,
            LOAD_ANYTHING             = 0x00000040,
            IGNORE_CVREC              = 0x00000080,
            NO_UNQUALIFIED_LOADS      = 0x00000100,
            FAIL_CRITICAL_ERRORS      = 0x00000200,
            EXACT_SYMBOLS             = 0x00000400,
            ALLOW_ABSOLUTE_SYMBOLS    = 0x00000800,
            IGNORE_NT_SYMPATH         = 0x00001000,
            INCLUDE_32BIT_MODULES     = 0x00002000,
            PUBLICS_ONLY              = 0x00004000,
            NO_PUBLICS                = 0x00008000,
            AUTO_PUBLICS              = 0x00010000,
            NO_IMAGE_SEARCH           = 0x00020000,
            SECURE                    = 0x00040000,
            NO_PROMPTS                = 0x00080000,
            OVERWRITE                 = 0x00100000,
            IGNORE_IMAGEDIR           = 0x00200000,
            FLAT_DIRECTORY            = 0x00400000,
            FAVOR_COMPRESSED          = 0x00800000,
            ALLOW_ZERO_ADDRESS        = 0x01000000,
            DISABLE_SYMSRV_AUTODETECT = 0x02000000,
            READONLY_CACHE            = 0x04000000,
            SYMPATH_LAST              = 0x08000000,
            DISABLE_FAST_SYMBOLS      = 0x10000000,
            DISABLE_SYMSRV_TIMEOUT    = 0x20000000,
            DISABLE_SRVSTAR_ON_STARTUP = 0x40000000,
            DEBUG                     = 0x80000000,
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int SymSetOptions(
            SymOptions SymOptions
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeLibraryHandle LoadLibrary(string filename);

        static SafeLibraryHandle SafeLoadLibrary(string filename)
        {
            SafeLibraryHandle lib = LoadLibrary(filename);
            if (lib.IsInvalid)
            {
                throw new Win32Exception();
            }
            return lib;
        }

        private SafeLibraryHandle _dbghelp_lib;
        private SymInitializeW _sym_init;
        private SymCleanup _sym_cleanup;
        private SymFromNameW _sym_from_name;
        private SymSetOptions _sym_set_options;
        private SymEnumerateModulesW64 _sym_enum_modules;
        private IntPtr _process;

        private void GetFunc<T>(ref T f) where T : class
        {
            f = _dbghelp_lib.GetFunctionPointer<T>();
        }

        static SafeStructureBuffer<SYMBOL_INFO> AllocateSymInfo()
        {
            return new SafeStructureBuffer<SYMBOL_INFO>(new SYMBOL_INFO(SYMBOL_INFO.MAX_SYM_NAME), SYMBOL_INFO.MAX_SYM_NAME * 2);
        }

        static string GetNameFromSymbolInfo(IntPtr buffer)
        {
            IntPtr ofs = Marshal.OffsetOf<SYMBOL_INFO>("Name");
            return Marshal.PtrToStringUni(buffer + ofs.ToInt32());
        }

        public SymbolResolver(string dbghelp_path, IntPtr process, string symbol_path)
        {
            _process = process;
            _dbghelp_lib = SafeLoadLibrary(dbghelp_path);
            GetFunc(ref _sym_init);
            GetFunc(ref _sym_cleanup);
            GetFunc(ref _sym_from_name);
            GetFunc(ref _sym_set_options);
            GetFunc(ref _sym_enum_modules);

            _sym_set_options(SymOptions.INCLUDE_32BIT_MODULES | SymOptions.UNDNAME | SymOptions.DEFERRED_LOADS);

            if (!_sym_init(process, symbol_path, true))
            {
                throw new Win32Exception();
            }
        }

        public IEnumerable<Tuple<string, IntPtr>> GetLoadedModules()
        {
            List<Tuple<string, IntPtr>> modules = new List<Tuple<string, IntPtr>>();

            if (!_sym_enum_modules(_process, (s, m, p) =>
                {
                    modules.Add(new Tuple<string, IntPtr>(s, new IntPtr(m)));
                    return true;
                }, IntPtr.Zero))
            {
                throw new Win32Exception();
            }
            return modules.AsReadOnly();
        }

        public IntPtr GetLoadedModuleAddress(string module_name)
        {
            IEnumerable<Tuple<string, IntPtr>> modules = GetLoadedModules().Where(m => m.Item1 == "combase");
            if (modules.Count() > 0)
            {
                return modules.First().Item2;
            }
            return IntPtr.Zero;
        }

        public IntPtr GetAddressOfSymbol(string name)
        {
            using (SafeStructureBuffer<SYMBOL_INFO> sym_info = AllocateSymInfo())
            {
                if (!_sym_from_name(_process, name, sym_info))
                {
                    return IntPtr.Zero;
                }
                return new IntPtr(sym_info.Result.Address);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; 

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
                _sym_cleanup?.Invoke(_process);
                _dbghelp_lib?.Close();
            }
        }

        ~SymbolResolver()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
