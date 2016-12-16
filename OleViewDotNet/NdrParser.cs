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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

namespace OleViewDotNet
{
    public enum NdrFormatCharacter : byte
    {
        FC_ZERO,
        FC_BYTE,                    // 0x01
        FC_CHAR,                    // 0x02
        FC_SMALL,                   // 0x03
        FC_USMALL,                  // 0x04
        FC_WCHAR,                   // 0x05
        FC_SHORT,                   // 0x06
        FC_USHORT,                  // 0x07
        FC_LONG,                    // 0x08
        FC_ULONG,                   // 0x09
        FC_FLOAT,                   // 0x0a
        FC_HYPER,                   // 0x0b
        FC_DOUBLE,                  // 0x0c
        FC_ENUM16,                  // 0x0d
        FC_ENUM32,                  // 0x0e
        FC_IGNORE,                  // 0x0f
        FC_ERROR_STATUS_T,          // 0x10
        FC_RP,                      // 0x11
        FC_UP,                      // 0x12
        FC_OP,                      // 0x13
        FC_FP,                      // 0x14

        FC_STRUCT,                  // 0x15
        FC_PSTRUCT,                 // 0x16

        FC_CSTRUCT,                 // 0x17


        FC_CPSTRUCT,                // 0x18


        FC_CVSTRUCT,                // 0x19

        FC_BOGUS_STRUCT,            // 0x1a

        FC_CARRAY,                  // 0x1b


        FC_CVARRAY,                 // 0x1c

        FC_SMFARRAY,                // 0x1d
        FC_LGFARRAY,                // 0x1e

        FC_SMVARRAY,                // 0x1f
        FC_LGVARRAY,                // 0x20

        FC_BOGUS_ARRAY,             // 0x21

        FC_C_CSTRING,               // 0x22
        FC_C_BSTRING,               // 0x23
        FC_C_SSTRING,               // 0x24
        FC_C_WSTRING,               // 0x25
        FC_CSTRING,                 // 0x26
        FC_BSTRING,                 // 0x27
        FC_SSTRING,                 // 0x28
        FC_WSTRING,                 // 0x29

        FC_ENCAPSULATED_UNION,      // 0x2a
        FC_NON_ENCAPSULATED_UNION,  // 0x2b

        FC_BYTE_COUNT_POINTER,      // 0x2c

        FC_TRANSMIT_AS,             // 0x2d
        FC_REPRESENT_AS,            // 0x2e

        FC_IP,                      // 0x2f

        FC_BIND_CONTEXT,            // 0x30
        FC_BIND_GENERIC,            // 0x31
        FC_BIND_PRIMITIVE,          // 0x32
        FC_AUTO_HANDLE,             // 0x33
        FC_CALLBACK_HANDLE,         // 0x34
        FC_UNUSED1,                 // 0x35

        FC_POINTER,                 // 0x36

        FC_ALIGNM2,                 // 0x37
        FC_ALIGNM4,                 // 0x38
        FC_ALIGNM8,                 // 0x39

        FC_UNUSED2,                 // 0x3a
        FC_UNUSED3,                 // 0x3b
        FC_UNUSED4,                 // 0x3c
        FC_STRUCTPAD1,              // 0x3d
        FC_STRUCTPAD2,              // 0x3e
        FC_STRUCTPAD3,              // 0x3f
        FC_STRUCTPAD4,              // 0x40
        FC_STRUCTPAD5,              // 0x41
        FC_STRUCTPAD6,              // 0x42
        FC_STRUCTPAD7,              // 0x43
        FC_STRING_SIZED,            // 0x44
        FC_UNUSED5,                 // 0x45
        FC_NO_REPEAT,               // 0x46
        FC_FIXED_REPEAT,            // 0x47
        FC_VARIABLE_REPEAT,         // 0x48
        FC_FIXED_OFFSET,            // 0x49
        FC_VARIABLE_OFFSET,         // 0x4a
        FC_PP,                      // 0x4b
        FC_EMBEDDED_COMPLEX,        // 0x4c
        FC_IN_PARAM,                // 0x4d
        FC_IN_PARAM_BASETYPE,       // 0x4e
        FC_IN_PARAM_NO_FREE_INST,   // 0x4d
        FC_IN_OUT_PARAM,            // 0x50
        FC_OUT_PARAM,               // 0x51
        FC_RETURN_PARAM,            // 0x52
        FC_RETURN_PARAM_BASETYPE,   // 0x53
        FC_DEREFERENCE,             // 0x54
        FC_DIV_2,                   // 0x55
        FC_MULT_2,                  // 0x56
        FC_ADD_1,                   // 0x57
        FC_SUB_1,                   // 0x58
        FC_CALLBACK,                // 0x59
        FC_CONSTANT_IID,            // 0x5a
        FC_END,                     // 0x5b
        FC_PAD,                     // 0x5c
        FC_SPLIT_DEREFERENCE = 0x74,      // 0x74
        FC_SPLIT_DIV_2,                   // 0x75
        FC_SPLIT_MULT_2,                  // 0x76
        FC_SPLIT_ADD_1,                   // 0x77
        FC_SPLIT_SUB_1,                   // 0x78
        FC_SPLIT_CALLBACK,                // 0x79
        FC_HARD_STRUCT = 0xb1,      // 0xb1
        FC_TRANSMIT_AS_PTR,         // 0xb2
        FC_REPRESENT_AS_PTR,        // 0xb3
        FC_USER_MARSHAL,            // 0xb4
        FC_PIPE,                    // 0xb5
        FC_BLKHOLE,                 // 0xb6
        FC_RANGE,                   // 0xb7     
        FC_INT3264,                 // 0xb8     
        FC_UINT3264,                // 0xb9    
        FC_END_OF_UNIVERSE          // 0xba
    }


    [Flags]
    public enum NdrInterpreterFlags : byte
    {
        FullPtrUsed             =  0x01,
        RpcSsAllocUsed          =  0x02,
        ObjectProc              =  0x04,
        HasRpcFlags             =  0x08,
        IgnoreObjectException   =  0x10,
        HasCommOrFault          =  0x20,
        UseNewInitRoutines      =  0x40,       
    }

    [Flags]
    public enum NdrParamAttributes : ushort
    {
        MustSize            = 0x0001,
        MustFree            = 0x0002,
        IsPipe              = 0x0004,
        IsIn                = 0x0008,
        IsOut               = 0x0010,
        IsReturn            = 0x0020,
        IsBasetype          = 0x0040,
        IsByValue           = 0x0080,
        IsSimpleRef         = 0x0100,
        IsDontCallFreeInst  = 0x0200,
        SaveForAsyncFinish  = 0x0400,
        ServerAllocSizeMask = 0xe000
    }

    [Flags]
    public enum NdrInterpreterOptFlags : byte
    {
        ServerMustSize      = 0x01,
        ClientMustSize      = 0x02,
        HasReturn           = 0x04,
        HasPipes            = 0x08,
        HasAsyncUuid        = 0x20,
        HasExtensions       = 0x40,
        HasAsyncHandle      = 0x80,
    }

    [Flags]
    public enum NdrInterpreterOptFlags2 : byte
    {
        HasNewCorrDesc      = 0x01,
        ClientCorrCheck     = 0x02,
        ServerCorrCheck     = 0x04,
        HasNotify           = 0x08,
        HasNotify2          = 0x10,
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct NdrDComOi2ProcHeader
    {
        // Old Oi Header
        public NdrFormatCharacter HandleType;
        public NdrInterpreterFlags OldOiFlags;
        public ushort RpcFlagsLow;
        public ushort RpcFlagsHi;
        public ushort ProcNum;
        public ushort StackSize;
        // New Oi2 Header
        public ushort ClientBufferSize;
        public ushort ServerBufferSize;
        public NdrInterpreterOptFlags Oi2Flags;
        public byte NumberParams;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NdrProcHeaderExts
    {
        public byte Size;
        public NdrInterpreterOptFlags2 Flags2;
        public ushort ClientCorrHint;
        public ushort ServerCorrHint;
        public ushort NotifyIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NdrProcHeaderExts64
    {
        public byte Size;
        public NdrInterpreterOptFlags2 Flags2;
        public ushort ClientCorrHint;
        public ushort ServerCorrHint;
        public ushort NotifyIndex;
        public ushort FloatArgMask;
    }

    [Flags]
    public enum NdrPointerFlags : byte
    {
        FC_ALLOCATE_ALL_NODES       = 0x01,
        FC_DONT_FREE                = 0x02,
        FC_ALLOCED_ON_STACK         = 0x04,
        FC_SIMPLE_POINTER           = 0x08,
        FC_POINTER_DEREF            = 0x10,
    }

    [Flags]
    public enum NdrUserMarshalFlags : byte
    {
        USER_MARSHAL_POINTER        = 0xc0,
        USER_MARSHAL_UNIQUE         = 0x80,
        USER_MARSHAL_REF            = 0x40,
        USER_MARSHAL_IID            = 0x20
    }

    class SafeBufferWrapper : SafeBuffer
    {
        public SafeBufferWrapper(IntPtr buffer) 
            : base(false)
        {
            this.Initialize(uint.MaxValue);
            handle = buffer;
        }

        protected override bool ReleaseHandle()
        {
            return true;
        }
    }

    public class NdrInterfacePointerTypeReference : NdrBaseTypeReference
    {
        public Guid Iid { get; private set; }

        public bool IsConstant { get; private set; }

        internal NdrInterfacePointerTypeReference(BinaryReader reader) : base(NdrFormatCharacter.FC_IP)
        {
            NdrFormatCharacter type = ReadFormat(reader);
            if (type == NdrFormatCharacter.FC_CONSTANT_IID)
            {
                Iid = new Guid(reader.ReadAll(16));
                IsConstant = true;
            }
            else
            {
                Iid = COMInterfaceEntry.IID_IUnknown;
                // Ignore the additional conformance data.
            }
        }

        public override string FormatType(IDictionary<Guid, string> iids_to_names)
        {
            if (IsConstant)
            {
                if (iids_to_names.ContainsKey(Iid))
                {
                    return String.Format("{0}*", iids_to_names[Iid]);
                }
                return String.Format("/* Unknown IID {0} */ IUnknown*", Iid);
            }
            else
            {
                return String.Format("IUnknown*");
            }
        }
    }

    public class NdrPointerTypeReference : NdrBaseTypeReference
    {
        public NdrBaseTypeReference Type { get; private set; }
        public NdrPointerFlags Flags { get; private set; }

        internal NdrPointerTypeReference(MIDL_STUB_DESC stub_desc, NdrFormatCharacter format, BinaryReader reader, IntPtr type_desc) : base(format)
        {
            Flags = (NdrPointerFlags)reader.ReadByte();
            if ((Flags & NdrPointerFlags.FC_SIMPLE_POINTER) == NdrPointerFlags.FC_SIMPLE_POINTER)
            {
                Type = new NdrBaseTypeReference(ReadFormat(reader));
            }
            else
            {
                long curr_pos = reader.BaseStream.Position;
                short ofs = (short)(reader.ReadInt16() + curr_pos);
                Type = NdrBaseTypeReference.Read(stub_desc, type_desc, ofs);
            }
        }

        public override string FormatType(IDictionary<Guid, string> iids_to_names)
        {
            return String.Format("{0}*", Type.FormatType(iids_to_names));
        }
    }    

    public class NdrStringTypeReference : NdrBaseTypeReference
    {
        public int StringSize { get; private set; }

        internal NdrStringTypeReference(NdrFormatCharacter format, BinaryReader reader) : base(format)
        {
            switch (format)
            {
                case NdrFormatCharacter.FC_BSTRING:
                case NdrFormatCharacter.FC_CSTRING:
                case NdrFormatCharacter.FC_WSTRING:
                    StringSize = reader.ReadUInt16();
                    break;
            }
        }

        public override string FormatType(IDictionary<Guid, string> iids_to_names)
        {
            if (StringSize > 0)
            {
                return String.Format("{0}[{1}]", base.FormatType(iids_to_names), StringSize);
            }
            else
            {
                return String.Format("{0}", base.FormatType(iids_to_names));
            }
        }
    }

    public class NdrStructureStringTypeReferece : NdrBaseTypeReference
    {
        public int ElementSize { get; private set; }
        public int NumberOfElements { get; private set; }
        internal NdrStructureStringTypeReferece(NdrFormatCharacter format, BinaryReader reader) : base(format)
        {
            ElementSize = reader.ReadByte();
            if (format == NdrFormatCharacter.FC_SSTRING)
            {
                NumberOfElements = reader.ReadUInt16();
            }
        }

        public override string FormatType(IDictionary<Guid, string> iids_to_names)
        {
            return String.Format("{0}<{1}>[{2}]", base.FormatType(iids_to_names), ElementSize, NumberOfElements);
        }
    }

    public class NdrUserMarshalTypeReference : NdrBaseTypeReference
    {
        public NdrUserMarshalFlags Flags { get; private set; }
        public int QuadrupleIndex { get; private set; }
        public int UserTypeMemorySite { get; private set; }
        public int TransmittedTypeBufferSize { get; private set; }
        public NdrBaseTypeReference Type { get; private set; }

        internal NdrUserMarshalTypeReference(MIDL_STUB_DESC stub_desc, BinaryReader reader, IntPtr type_desc) 
            : base(NdrFormatCharacter.FC_USER_MARSHAL)
        {
            Flags = (NdrUserMarshalFlags)(reader.ReadByte() & 0xF0);
            QuadrupleIndex = reader.ReadUInt16();
            UserTypeMemorySite = reader.ReadUInt16();
            TransmittedTypeBufferSize = reader.ReadUInt16();
            long last_ofs = reader.BaseStream.Position;
            short ofs = (short)(last_ofs + reader.ReadInt16());
            Type = NdrBaseTypeReference.Read(stub_desc, type_desc, ofs);
        }

        public override string FormatType(IDictionary<Guid, string> iids_to_names)
        {
            if ((Flags & NdrUserMarshalFlags.USER_MARSHAL_POINTER) != 0)
            {
                return String.Format("{0}*", base.FormatType(iids_to_names));
            }
            return base.FormatType(iids_to_names);
        }
    }

    public enum KnownUserMarshalType
    {
        BSTR,
        LPSAFEARRAY,
        VARIANT,
        HWND,
    }

    public class NdrKnownUserMarshalTypeReference : NdrBaseTypeReference
    {
        public KnownUserMarshalType Type { get; private set; }

        public NdrKnownUserMarshalTypeReference(KnownUserMarshalType type) 
            : base(NdrFormatCharacter.FC_USER_MARSHAL)
        {
            Type = type;
        }

        public override string FormatType(IDictionary<Guid, string> iids_to_names)
        {
            return Type.ToString();
        }
    }
    
    public class NdrUnknownTypeReference : NdrBaseTypeReference
    {
        internal NdrUnknownTypeReference(NdrFormatCharacter format) : base(format)
        {
        }

        public override string FormatType(IDictionary<Guid, string> iids_to_names)
        {
            return String.Format("/* Unhandled */ {0}", base.FormatType(iids_to_names));
        }
    }

    public class NdrBaseTypeReference
    {
        public NdrFormatCharacter Format { get; private set; }

        protected internal NdrBaseTypeReference(NdrFormatCharacter format)
        {
            Format = format;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Format, GetType().Name);
        }

        public virtual string FormatType(IDictionary<Guid, string> iids_to_names)
        {
            switch (Format)
            {
                case NdrFormatCharacter.FC_BYTE:
                    return "byte";
                case NdrFormatCharacter.FC_CHAR:
                    return "sbyte";
                case NdrFormatCharacter.FC_WCHAR:
                    return "char";
                case NdrFormatCharacter.FC_SHORT:
                    return "short";
                case NdrFormatCharacter.FC_USHORT:
                    return "ushort";
                case NdrFormatCharacter.FC_LONG:
                    return "int";
                case NdrFormatCharacter.FC_ULONG:
                    return "uint";
                case NdrFormatCharacter.FC_FLOAT:
                    return "float";
                case NdrFormatCharacter.FC_HYPER:
                    return "long";
                case NdrFormatCharacter.FC_DOUBLE:
                    return "double";                
                case NdrFormatCharacter.FC_INT3264:
                    return "IntPtr";
                case NdrFormatCharacter.FC_UINT3264:
                    return "UIntPtr";
                case NdrFormatCharacter.FC_C_WSTRING:
                case NdrFormatCharacter.FC_C_CSTRING:
                case NdrFormatCharacter.FC_WSTRING:
                case NdrFormatCharacter.FC_CSTRING:
                    return "string";
            }

            return String.Format("{0}", Format);
        }

        internal static NdrFormatCharacter ReadFormat(BinaryReader reader)
        {
            return (NdrFormatCharacter)reader.ReadByte();
        }

        private class StandardUserMarshalers
        {
            public IntPtr BSTR_UserSizePtr;
            public IntPtr BSTR_UserSize64Ptr;
            public IntPtr VARIANT_UserSizePtr;
            public IntPtr VARIANT_UserSize64Ptr;
            public IntPtr LPSAFEARRAY_UserSizePtr;
            public IntPtr LPSAFEARRAY_UserSize64Ptr;
            public IntPtr HWND_UserSizePtr;
            public IntPtr HWND_UserSize64Ptr;

            public StandardUserMarshalers()
            {
                using (SafeLibraryHandle lib = COMUtilities.SafeLoadLibrary("oleaut32.dll"))
                {
                    BSTR_UserSizePtr = lib.GetFunctionPointer("BSTR_UserSize");
                    BSTR_UserSize64Ptr = lib.GetFunctionPointer("BSTR_UserSize64");
                    VARIANT_UserSizePtr = lib.GetFunctionPointer("VARIANT_UserSize");
                    VARIANT_UserSize64Ptr = lib.GetFunctionPointer("VARIANT_UserSize64");
                    LPSAFEARRAY_UserSizePtr = lib.GetFunctionPointer("LPSAFEARRAY_UserSize");
                    LPSAFEARRAY_UserSize64Ptr = lib.GetFunctionPointer("LPSAFEARRAY_UserSize64");
                    HWND_UserSizePtr = lib.GetFunctionPointer("HWND_UserSize");
                    HWND_UserSize64Ptr = lib.GetFunctionPointer("HWND_UserSize64");
                }
            }
        }

        private static StandardUserMarshalers m_marshalers;

        private static NdrBaseTypeReference FixupUserMarshal(MIDL_STUB_DESC stub_desc, NdrUserMarshalTypeReference type)
        {
            if (stub_desc.aUserMarshalQuadruple == IntPtr.Zero)
            {
                return type;
            }

            using (SafeLibraryHandle module = COMUtilities.SafeGetModuleHandle(stub_desc.aUserMarshalQuadruple))
            {
                if (module == null)
                {
                    return type;
                }

                if (m_marshalers == null)
                {
                    m_marshalers = new StandardUserMarshalers();
                }

                IntPtr usersize_ptr = COMUtilities.GetTargetAddress(module.DangerousGetHandle(), Marshal.ReadIntPtr(stub_desc.aUserMarshalQuadruple, type.QuadrupleIndex * IntPtr.Size * 4));
                if ((usersize_ptr == m_marshalers.BSTR_UserSizePtr) || (usersize_ptr == m_marshalers.BSTR_UserSize64Ptr))
                {
                    return new NdrKnownUserMarshalTypeReference(KnownUserMarshalType.BSTR);
                }
                if ((usersize_ptr == m_marshalers.VARIANT_UserSizePtr) || (usersize_ptr == m_marshalers.VARIANT_UserSize64Ptr))
                {
                    return new NdrKnownUserMarshalTypeReference(KnownUserMarshalType.VARIANT);
                }
                if ((usersize_ptr == m_marshalers.LPSAFEARRAY_UserSizePtr) || (usersize_ptr == m_marshalers.LPSAFEARRAY_UserSize64Ptr))
                {
                    return new NdrKnownUserMarshalTypeReference(KnownUserMarshalType.LPSAFEARRAY);
                }
                if ((usersize_ptr == m_marshalers.HWND_UserSizePtr) || (usersize_ptr == m_marshalers.HWND_UserSize64Ptr))
                {
                    return new NdrKnownUserMarshalTypeReference(KnownUserMarshalType.HWND);
                }
            }
            return type;
        }

        internal static NdrBaseTypeReference Read(MIDL_STUB_DESC stub_desc, IntPtr type_desc, int ofs)
        {
            UnmanagedMemoryStream stm = new UnmanagedMemoryStream(new SafeBufferWrapper(type_desc), 0, int.MaxValue);
            stm.Position = ofs;
            BinaryReader reader = new BinaryReader(stm);
            NdrFormatCharacter format = (NdrFormatCharacter)reader.ReadByte();

            switch (format)
            {
                case NdrFormatCharacter.FC_BYTE:
                case NdrFormatCharacter.FC_CHAR:
                case NdrFormatCharacter.FC_SMALL:
                case NdrFormatCharacter.FC_USMALL:
                case NdrFormatCharacter.FC_WCHAR:
                case NdrFormatCharacter.FC_SHORT:
                case NdrFormatCharacter.FC_USHORT:
                case NdrFormatCharacter.FC_LONG:
                case NdrFormatCharacter.FC_ULONG:
                case NdrFormatCharacter.FC_FLOAT:
                case NdrFormatCharacter.FC_HYPER:
                case NdrFormatCharacter.FC_DOUBLE:
                case NdrFormatCharacter.FC_ENUM16:
                case NdrFormatCharacter.FC_ENUM32:
                case NdrFormatCharacter.FC_ERROR_STATUS_T:
                case NdrFormatCharacter.FC_INT3264:
                case NdrFormatCharacter.FC_UINT3264:
                    return new NdrBaseTypeReference(format);
                case NdrFormatCharacter.FC_END:
                    return null;
                case NdrFormatCharacter.FC_OP:
                case NdrFormatCharacter.FC_UP:
                case NdrFormatCharacter.FC_RP:
                case NdrFormatCharacter.FC_FP:
                    return new NdrPointerTypeReference(stub_desc, format, reader, type_desc);
                case NdrFormatCharacter.FC_IP:
                    return new NdrInterfacePointerTypeReference(reader);
                case NdrFormatCharacter.FC_C_CSTRING:
                case NdrFormatCharacter.FC_C_BSTRING:
                case NdrFormatCharacter.FC_C_WSTRING:
                case NdrFormatCharacter.FC_CSTRING:
                case NdrFormatCharacter.FC_BSTRING:
                case NdrFormatCharacter.FC_WSTRING:
                    return new NdrStringTypeReference(format, reader);
                case NdrFormatCharacter.FC_C_SSTRING:
                case NdrFormatCharacter.FC_SSTRING:
                    return new NdrStructureStringTypeReferece(format, reader);
                case NdrFormatCharacter.FC_USER_MARSHAL:
                    return FixupUserMarshal(stub_desc, new NdrUserMarshalTypeReference(stub_desc, reader, type_desc));
                default:
                    return new NdrUnknownTypeReference(format);
            }
        }
    }

    public class NdrProcedureParameter
    {
        public NdrParamAttributes Attributes { get; private set; }
        public NdrBaseTypeReference Type { get; private set; }
        public int SizeOffset { get; private set; }

        internal NdrProcedureParameter(MIDL_STUB_DESC stub_desc, BinaryReader reader, IntPtr type_desc)
        {
            Attributes = (NdrParamAttributes)reader.ReadUInt16();
            SizeOffset = reader.ReadUInt16();
            if ((Attributes & NdrParamAttributes.IsBasetype) == 0)
            {
                int type_ofs = reader.ReadUInt16();
                Type = NdrBaseTypeReference.Read(stub_desc, type_desc, type_ofs);
            }
            else
            {
                Type = new NdrBaseTypeReference((NdrFormatCharacter)reader.ReadByte());
                // Remove padding.
                reader.ReadByte();
            }
        }

        public string Format(IDictionary<Guid, string> iids_to_names)
        {
            List<string> attributes = new List<string>();
            if ((Attributes & NdrParamAttributes.IsIn) != 0)
            {
                attributes.Add("In");
            }
            if ((Attributes & NdrParamAttributes.IsOut) != 0)
            {
                attributes.Add("Out");
            }
            if ((Attributes & NdrParamAttributes.IsReturn) != 0)
            {
                attributes.Add("RetVal");
            }

            string type_format = (Attributes & NdrParamAttributes.IsSimpleRef) == 0 
                ? Type.FormatType(iids_to_names) : String.Format("{0}*", Type.FormatType(iids_to_names));

            if (attributes.Count > 0)
            {
                return String.Format("[{0}] {1}", string.Join(", ", attributes), type_format);
            }
            else
            {
                return type_format;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", Type, Attributes);
        }
    }

    public class NdrProcedureDefinition
    {
        public NdrProcedureParameter[] Params { get; private set; }
        public NdrProcedureParameter ReturnValue { get; private set; }
        public int ProcNum { get; private set; }

        public string FormatProcedure(IDictionary<Guid, string> iids_to_names)
        {
            return String.Format("{0} Proc{1}({2});", ReturnValue == null ? "void" : ReturnValue.Type.FormatType(iids_to_names),
                ProcNum, string.Join(", ", Params.Select(p => p.Format(iids_to_names))));
        }
        
        internal NdrProcedureDefinition(MIDL_STUB_DESC stub_desc, IntPtr proc_desc, IntPtr type_desc)
        {
            NdrDComOi2ProcHeader header = Marshal.PtrToStructure<NdrDComOi2ProcHeader>(proc_desc);
            proc_desc += Marshal.SizeOf(header);
            if ((header.OldOiFlags & NdrInterpreterFlags.UseNewInitRoutines) == 0)
            {
                throw new ArgumentException("Don't support old style NDR marshaling");
            }

            if ((header.OldOiFlags & NdrInterpreterFlags.ObjectProc) == 0)
            {
                throw new ArgumentException("Only support Object Procedures");
            }

            if (header.HandleType != NdrFormatCharacter.FC_AUTO_HANDLE)
            {
                throw new ArgumentException("HandleType must be FC_AUTO_HANDLE");
            }
            ProcNum = header.ProcNum;

            if ((header.Oi2Flags & NdrInterpreterOptFlags.HasExtensions) == NdrInterpreterOptFlags.HasExtensions)
            {
                // Just ignore extensions.
                byte ext_size = Marshal.ReadByte(proc_desc);
                proc_desc += ext_size;
            }

            List<NdrProcedureParameter> ps = new List<NdrProcedureParameter>();

            UnmanagedMemoryStream stm = new UnmanagedMemoryStream(new SafeBufferWrapper(proc_desc), 0, int.MaxValue);
            BinaryReader reader = new BinaryReader(stm);
            bool has_return = (header.Oi2Flags & NdrInterpreterOptFlags.HasReturn) == NdrInterpreterOptFlags.HasReturn;
            int param_count = has_return ? header.NumberParams - 1 : header.NumberParams;
            for (int param = 0; param < param_count; ++param)
            {
                ps.Add(new NdrProcedureParameter(stub_desc, reader, type_desc));
            }
            Params = ps.ToArray();
            if (has_return)
            {
                ReturnValue = new NdrProcedureParameter(stub_desc, reader, type_desc);
            }
        }
    }
}
