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
using System.Text;

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
        Unknown20           = 0x20,
        ExtendedCorrDesc    = 0x40,
        Unknown80           = 0x80,
        Valid = HasNewCorrDesc | ClientCorrCheck | ServerCorrCheck | HasNotify | HasNotify2 | ExtendedCorrDesc
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

    [Flags]
    public enum NdrCorrelationType : byte
    {
        FC_NORMAL_CONFORMANCE = 0,
        FC_POINTER_CONFORMANCE = 0x10,
        FC_TOP_LEVEL_CONFORMANCE = 0x20,
        FC_CONSTANT_CONFORMANCE = 0x40,
        FC_TOP_LEVEL_MULTID_CONFORMANCE = 0x80,
    }

    [Flags]
    public enum NdrCorrelationFlags : byte
    {
        Early = 0x1,
        Split = 0x2,
        IsIidIs = 0x4,
        DontCheck = 0x8
    }

    public class NdrCorrelationDescriptor
    {
        public NdrCorrelationType CorrelationType { get; private set; }
        public NdrFormatCharacter ValueType { get; private set; }
        public NdrFormatCharacter Operator { get; private set; }
        public int Offset { get; private set; }
        public NdrCorrelationFlags Flags { get; private set; }
        public bool IsValid { get; private set; }

        internal NdrCorrelationDescriptor(NdrParseContext context, BinaryReader reader)
        {
            byte type_byte = reader.ReadByte();
            byte op_byte = reader.ReadByte();
            int offset = reader.ReadInt16();
            byte flags = 0;
            if (context.CorrDescSize > 4)
            {
                flags = reader.ReadByte();
                reader.ReadByte();

                // Read padding.
                reader.ReadAll(context.CorrDescSize - 6);
            }

            if (type_byte != 0xFF || op_byte != 0xFF || offset != -1)
            {
                IsValid = true;
                CorrelationType = (NdrCorrelationType)(type_byte & 0xF0);
                ValueType = (NdrFormatCharacter)(type_byte & 0xF);
                Operator = (NdrFormatCharacter)op_byte;
                Offset = offset;
                Flags = (NdrCorrelationFlags)flags;
            }
        }
    }

    class SafeBufferWrapper : SafeBuffer
    {
        public SafeBufferWrapper(IntPtr buffer) 
            : base(false)
        {
            this.Initialize(int.MaxValue);
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

        public NdrCorrelationDescriptor IidIsDescriptor { get; private set; }

        internal NdrInterfacePointerTypeReference(NdrParseContext context, BinaryReader reader) : base(NdrFormatCharacter.FC_IP)
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
                IidIsDescriptor = new NdrCorrelationDescriptor(context, reader);
            }
        }

        public override string FormatType(NdrFormatContext context)
        {
            if (IsConstant)
            {
                if (context.IidsToNames.ContainsKey(Iid))
                {
                    return String.Format("{0}*", context.IidsToNames[Iid]);
                }
                return String.Format("/* Unknown IID {0} */ IUnknown*", Iid);
            }
            else
            {
                return String.Format("/* iid_is param offset: {0} */ IUnknown*", IidIsDescriptor.Offset);
            }
        }
    }

    public class NdrPointerTypeReference : NdrBaseTypeReference
    {
        public NdrBaseTypeReference Type { get; private set; }
        public NdrPointerFlags Flags { get; private set; }

        internal NdrPointerTypeReference(NdrParseContext context, NdrFormatCharacter format, BinaryReader reader) : base(format)
        {
            Flags = (NdrPointerFlags)reader.ReadByte();
            if ((Flags & NdrPointerFlags.FC_SIMPLE_POINTER) == NdrPointerFlags.FC_SIMPLE_POINTER)
            {
                Type = new NdrBaseTypeReference(ReadFormat(reader));
            }
            else
            {
                Type = NdrBaseTypeReference.Read(context, ReadTypeOffset(reader));
            }
        }

        public override string FormatType(NdrFormatContext context)
        {
            return String.Format("{0}*", Type.FormatType(context));
        }

        public override int GetSize()
        {
            return IntPtr.Size;
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

        public override string FormatType(NdrFormatContext context)
        {
            if (StringSize > 0)
            {
                return String.Format("{0}[{1}]", base.FormatType(context), StringSize);
            }
            else
            {
                return String.Format("{0}", base.FormatType(context));
            }
        }

        public override int GetSize()
        {
            return StringSize;
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

        public override string FormatType(NdrFormatContext context)
        {
            return String.Format("{0}<{1}>[{2}]", base.FormatType(context), ElementSize, NumberOfElements);
        }
    }

    public class NdrUserMarshalTypeReference : NdrBaseTypeReference
    {
        public NdrUserMarshalFlags Flags { get; private set; }
        public int QuadrupleIndex { get; private set; }
        public int UserTypeMemorySite { get; private set; }
        public int TransmittedTypeBufferSize { get; private set; }
        public NdrBaseTypeReference Type { get; private set; }

        internal NdrUserMarshalTypeReference(NdrParseContext context, BinaryReader reader) 
            : base(NdrFormatCharacter.FC_USER_MARSHAL)
        {
            Flags = (NdrUserMarshalFlags)(reader.ReadByte() & 0xF0);
            QuadrupleIndex = reader.ReadUInt16();
            UserTypeMemorySite = reader.ReadUInt16();
            TransmittedTypeBufferSize = reader.ReadUInt16();
            Type = NdrBaseTypeReference.Read(context, ReadTypeOffset(reader));
        }

        public override string FormatType(NdrFormatContext context)
        {
            if ((Flags & NdrUserMarshalFlags.USER_MARSHAL_POINTER) != 0)
            {
                return String.Format("{0}*", base.FormatType(context));
            }
            return base.FormatType(context);
        }
    }

    public enum NdrKnownTypes
    {
        None,

        // OLEAUT32
        BSTR,
        LPSAFEARRAY,
        VARIANT,
        HWND,
        GUID,

        // OLE32
        HENHMETAFILE,
        HMETAFILEPICT,
        HMETAFILE,
        SNB,
        STGMEDIUM,

        // COMBASE
        CLIPFORMAT,
        HACCEL,
        HBITMAP,
        HBRUSH,
        HDC,
        HGLOBAL,
        HICON,
        HMENU,
        HMONITOR,
        HPALETTE,
        HRGN,
        HSTRING,
        WdtpInterfacePointer,
    }

    public class NdrKnownTypeReference : NdrBaseTypeReference
    {
        public NdrKnownTypes KnownType { get; private set; }

        public NdrKnownTypeReference(NdrKnownTypes type) 
            : base(NdrFormatCharacter.FC_USER_MARSHAL)
        {
            KnownType = type;
        }

        public override string FormatType(NdrFormatContext context)
        {
            return KnownType.ToString();
        }

        public override int GetSize()
        {
            switch (KnownType)
            {
                case NdrKnownTypes.GUID:
                    return 16;
                case NdrKnownTypes.BSTR:
                case NdrKnownTypes.LPSAFEARRAY:
                case NdrKnownTypes.HWND:
                case NdrKnownTypes.HENHMETAFILE:
                case NdrKnownTypes.HMETAFILEPICT:
                case NdrKnownTypes.HMETAFILE:
                case NdrKnownTypes.HACCEL:
                case NdrKnownTypes.HBITMAP:
                case NdrKnownTypes.HBRUSH:
                case NdrKnownTypes.HDC:
                case NdrKnownTypes.HGLOBAL:
                case NdrKnownTypes.HICON:
                case NdrKnownTypes.HMENU:
                case NdrKnownTypes.HMONITOR:
                case NdrKnownTypes.HPALETTE:
                case NdrKnownTypes.HRGN:
                case NdrKnownTypes.HSTRING:
                case NdrKnownTypes.WdtpInterfacePointer:
                    return IntPtr.Size;
                case NdrKnownTypes.VARIANT:
                    return Environment.Is64BitProcess ? 24 : 16;
                case NdrKnownTypes.SNB:
                case NdrKnownTypes.CLIPFORMAT:
                    return 4;
                case NdrKnownTypes.STGMEDIUM:
                    return Environment.Is64BitProcess ? 24 : 12;
                default:
                    throw new ArgumentException("Unknown Known Type");
            }
        }
    }
    
    public class NdrUnknownTypeReference : NdrBaseTypeReference
    {
        static HashSet<NdrFormatCharacter> _formats = new HashSet<NdrFormatCharacter>();
        internal NdrUnknownTypeReference(NdrFormatCharacter format) : base(format)
        {
            if (_formats.Add(format))
            {
                System.Diagnostics.Trace.WriteLine(String.Format("{0}", format));
            }
        }

        public override string FormatType(NdrFormatContext context)
        {
            return String.Format("/* Unhandled */ {0}", base.FormatType(context));
        }
    }

    public class NdrStructureMember
    {
        public NdrBaseTypeReference MemberType { get; private set; }
        public int Offset { get; private set; }

        internal NdrStructureMember(NdrBaseTypeReference member_type, int offset)
        {
            MemberType = member_type;
            Offset = offset;
        }

        public string FormatMember(NdrFormatContext context)
        {
            return String.Format("/* Offset: {0} */ {1}", Offset, MemberType.FormatType(context));
        }
    }
    
    public class NdrBaseStructureTypeReference : NdrBaseTypeReference
    {
        protected List<NdrBaseTypeReference> _members;
        public int Alignment { get; private set; }
        public int MemorySize { get; private set; }

        private IEnumerable<NdrStructureMember> GetMembers()
        {
            int current_offset = 0;
            foreach (var type in _members)
            {
                if (!(type is NdrStructurePaddingTypeReference))
                {
                    yield return new NdrStructureMember(type, current_offset);
                }
                current_offset += type.GetSize();
            }
        }

        public IEnumerable<NdrBaseTypeReference> MembersTypes { get { return _members.AsReadOnly(); } }

        public IEnumerable<NdrStructureMember> Members { get { return GetMembers(); } }

        internal NdrBaseStructureTypeReference(NdrFormatCharacter format, BinaryReader reader)
            : base(format)
        {
            Alignment = reader.ReadByte();
            MemorySize = reader.ReadUInt16();
            _members = new List<NdrBaseTypeReference>();
        }

        internal void ReadMemberInfo(NdrParseContext context, BinaryReader reader)
        {
            NdrBaseTypeReference curr_type;
            while ((curr_type = Read(context, reader)) != null)
            {
                _members.Add(curr_type);
            }
        }

        public override string FormatType(NdrFormatContext context)
        {
            if (context.StructsToNames.ContainsKey(this))
            {
                return String.Format("struct {0}", context.StructsToNames[this]);
            }
            return base.FormatType(context);
        }

        public virtual string FormatStruct(NdrFormatContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(FormatType(context)).AppendLine(" {");
            foreach (var member in Members.Select((m, i) => String.Format("{0} Member{1}", m.FormatMember(context), i)))
            {
                builder.Append("    ").AppendLine(member);
            }
            builder.AppendLine("};");
            return builder.ToString();
        }

        public override int GetSize()
        {
            return MemorySize;
        }
    }    

    public class NdrSimpleStructureTypeReference : NdrBaseStructureTypeReference
    {
        internal NdrSimpleStructureTypeReference(NdrParseContext context, BinaryReader reader) 
            : base(NdrFormatCharacter.FC_STRUCT, reader)
        {
            ReadMemberInfo(context, reader);
        }
    }

    public class NdrConformantStructureTypeReference : NdrBaseStructureTypeReference
    {
        internal NdrConformantStructureTypeReference(NdrParseContext context, BinaryReader reader)
            : base(NdrFormatCharacter.FC_CSTRUCT, reader)
        {
            NdrBaseTypeReference array = Read(context, ReadTypeOffset(reader));
            ReadMemberInfo(context, reader);
            if (array != null)
            {
                _members.Add(array);
            }
        }
    }

    public class NdrBogusStructureTypeReference : NdrBaseStructureTypeReference
    {
        internal NdrBogusStructureTypeReference(NdrParseContext context, BinaryReader reader) 
            : base(NdrFormatCharacter.FC_BOGUS_STRUCT, reader)
        {
            NdrBaseTypeReference array = Read(context, ReadTypeOffset(reader));
            int pointer_ofs = ReadTypeOffset(reader);
            ReadMemberInfo(context, reader);
            if (pointer_ofs >= 0)
            {
                BinaryReader pointer_reader = GetReader(context, pointer_ofs);
                for (int i = 0; i < _members.Count; ++i)
                {
                    if (_members[i].Format == NdrFormatCharacter.FC_POINTER)
                    {
                        _members[i] = Read(context, reader);
                    }
                }
            }

            if (array != null)
            {
                _members.Add(array);
            }
        }
    }

    public class NdrHardStructureTypeReference : NdrBaseStructureTypeReference
    {
        public int EnumOffset { get; private set; }
        public int CopySize { get; private set; }
        public int MemCopyIncr { get; private set; }
        public int UnionDescOffset { get; private set; }

        internal NdrHardStructureTypeReference(NdrParseContext context, BinaryReader reader) 
            : base(NdrFormatCharacter.FC_HARD_STRUCT, reader)
        {
            // Reserved.
            reader.ReadInt32();
            EnumOffset = reader.ReadInt16();
            //CopySize = reader.ReadInt16();
            //MemCopyIncr = reader.ReadInt16();
            UnionDescOffset = reader.ReadInt16();
            ReadMemberInfo(context, reader);
        }
    }

    public class NdrBaseArrayTypeReference : NdrBaseTypeReference
    {
        public int Alignment { get; private set; }
        public NdrBaseTypeReference ElementType { get; private set; }

        internal NdrBaseArrayTypeReference(NdrParseContext context, NdrFormatCharacter format, BinaryReader reader) : base(format)
        {
            Alignment = reader.ReadByte();
        }

        internal void ReadElementType(NdrParseContext context, BinaryReader reader)
        {
            ElementType = Read(context, reader);
        }

        protected virtual int GetArraySize()
        {
            return 0;
        }

        public override string FormatType(NdrFormatContext context)
        {
            int array_size = GetArraySize();
            return String.Format("{0}[{1}]", ElementType.FormatType(context), array_size == 0 ? String.Empty : array_size.ToString());
        }
    }

    public class NdrSimpleArrayTypeReference : NdrBaseArrayTypeReference
    {
        public int TotalSize { get; private set; }

        internal NdrSimpleArrayTypeReference(NdrParseContext context, NdrFormatCharacter format, BinaryReader reader) : base(context, format, reader)
        {
            if (format == NdrFormatCharacter.FC_SMFARRAY)
            {
                TotalSize = reader.ReadUInt16();
            }
            else
            {
                TotalSize = reader.ReadInt32();
            }

            ReadElementType(context, reader);
        }

        public override int GetSize()
        {
            return TotalSize;
        }

        protected override int GetArraySize()
        {
            return TotalSize;
        }
    }

    public class NdrConformantArrayTypeReference : NdrBaseArrayTypeReference
    {
        public int ElementSize { get; private set; }
        public NdrCorrelationDescriptor ConformanceDescriptor { get; private set; }
        public NdrCorrelationDescriptor VarianceDescriptor { get; private set; }

        internal NdrConformantArrayTypeReference(NdrFormatCharacter format, NdrParseContext context, 
            BinaryReader reader) : base(context, format, reader)
        {
            ElementSize = reader.ReadInt16();
            ConformanceDescriptor = new NdrCorrelationDescriptor(context, reader);
            if (format == NdrFormatCharacter.FC_CVARRAY)
            {
                VarianceDescriptor = new NdrCorrelationDescriptor(context, reader);
            }
            ReadElementType(context, reader);
        }
    }

    public class NdrBogusArrayTypeReference : NdrBaseArrayTypeReference
    {
        public int NumberofElements { get; private set; }
        public NdrCorrelationDescriptor ConformanceDescriptor { get; private set; }
        public NdrCorrelationDescriptor VarianceDescriptor { get; private set; }

        internal NdrBogusArrayTypeReference(NdrParseContext context, BinaryReader reader) 
            : base(context, NdrFormatCharacter.FC_BOGUS_ARRAY, reader)
        {
            NumberofElements = reader.ReadInt16();
            ConformanceDescriptor = new NdrCorrelationDescriptor(context, reader);
            VarianceDescriptor = new NdrCorrelationDescriptor(context, reader);
            ReadElementType(context, reader);
        }

        protected override int GetArraySize()
        {
            return NumberofElements;
        }

        public override int GetSize()
        {
            return NumberofElements * ElementType.GetSize();
        }
    }

    public class NdrStructurePaddingTypeReference : NdrBaseTypeReference
    {
        internal NdrStructurePaddingTypeReference(NdrFormatCharacter format) : base(format)
        {
        }

        public override int GetSize()
        {
            switch(Format)
            {
                case NdrFormatCharacter.FC_STRUCTPAD1:
                    return 1;
                case NdrFormatCharacter.FC_STRUCTPAD2:
                    return 2;
                case NdrFormatCharacter.FC_STRUCTPAD3:
                    return 3;
                case NdrFormatCharacter.FC_STRUCTPAD4:
                    return 4;
                case NdrFormatCharacter.FC_STRUCTPAD5:
                    return 5;
                case NdrFormatCharacter.FC_STRUCTPAD6:
                    return 6;
                case NdrFormatCharacter.FC_STRUCTPAD7:
                    return 7;
                default:
                    throw new InvalidOperationException("Format must be a padding character");
            }
        }
    }

    public class NdrPointerInfoInstance
    {
        public int OffsetInMemory { get; private set; }
        public int OffsetInBuffer { get; private set; }
        public NdrPointerTypeReference PointerType { get; private set; }

        internal NdrPointerInfoInstance(NdrParseContext context, BinaryReader reader)
        {
            OffsetInMemory = reader.ReadInt16();
            OffsetInBuffer = reader.ReadInt16();
            PointerType = NdrBaseTypeReference.Read(context, reader) as NdrPointerTypeReference;
        }
    }

    public class NdrPointerInfoTypeReference : NdrBaseTypeReference
    {
        public NdrFormatCharacter BasePointerType { get; private set; }
        public NdrFormatCharacter SubPointerType { get; private set; }
        public int Iterations { get; private set; }
        public int Increment { get; private set; }
        public int OffsetToArray { get; private set; }
        public IEnumerable<NdrPointerInfoInstance> PointerInstances { get; private set; }

        private IEnumerable<NdrPointerInfoInstance> ReadComplex(NdrParseContext context, BinaryReader reader, bool has_interations)
        {
            if (has_interations)
            {
                Iterations = reader.ReadInt16();
            }

            Increment = reader.ReadInt16();
            OffsetToArray = reader.ReadInt16();
            int num_of_pointers = reader.ReadInt16();
            while (num_of_pointers > 0)
            {
                yield return new NdrPointerInfoInstance(context, reader);
                num_of_pointers--;
            }
        }

        internal NdrPointerInfoTypeReference(NdrParseContext context, BinaryReader reader) 
            : base(NdrFormatCharacter.FC_PP)
        {
            List<NdrPointerInfoInstance> instances = new List<NdrPointerInfoInstance>();
            reader.ReadByte(); // Padding.
            BasePointerType = (NdrFormatCharacter)reader.ReadByte();
            SubPointerType = (NdrFormatCharacter)reader.ReadByte();
            
            switch (BasePointerType)
            {
                case NdrFormatCharacter.FC_NO_REPEAT:
                    instances.Add(new NdrPointerInfoInstance(context, reader));
                    break;
                case NdrFormatCharacter.FC_FIXED_REPEAT:
                    instances.AddRange(ReadComplex(context, reader, true));
                    break;
                case NdrFormatCharacter.FC_VARIABLE_REPEAT:
                    instances.AddRange(ReadComplex(context, reader, false));
                    break;
            }

            while ((NdrFormatCharacter)reader.ReadByte() != NdrFormatCharacter.FC_END)
            {
            }

            PointerInstances = instances;
        }
    }

    public class NdrRangeTypeReference : NdrBaseTypeReference
    {
        public NdrBaseTypeReference RangeType { get; private set; }
        public int MinValue { get; private set; }
        public int MaxValue { get; private set; }

        public NdrRangeTypeReference(BinaryReader reader) : base(NdrFormatCharacter.FC_RANGE)
        {
            RangeType = new NdrBaseTypeReference((NdrFormatCharacter)reader.ReadByte());
            MinValue = reader.ReadInt32();
            MaxValue = reader.ReadInt32();
        }

        public override string FormatType(NdrFormatContext context)
        {
            return String.Format("/* range: {0},{1} */ {2}", MinValue, MaxValue, RangeType.FormatType(context));
        }

        public override int GetSize()
        {
            return RangeType.GetSize();
        }
    }

    public class NdrIndirectTypeReference : NdrBaseTypeReference
    {
        NdrBaseTypeReference _ref_type;

        internal void FixupType(NdrBaseTypeReference ref_type)
        {
            _ref_type = ref_type;
        }

        internal NdrIndirectTypeReference() : base(NdrFormatCharacter.FC_ZERO)
        {
        }

        public override string FormatType(NdrFormatContext context)
        {
            return _ref_type.FormatType(context);
        }

        public override int GetSize()
        {
            return _ref_type.GetSize();
        }

        public override string ToString()
        {
            return _ref_type.ToString();
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

        public virtual string FormatType(NdrFormatContext context)
        {
            switch (Format)
            {
                case NdrFormatCharacter.FC_BYTE:
                    return "byte";
                case NdrFormatCharacter.FC_CHAR:
                    return "sbyte";
                case NdrFormatCharacter.FC_WCHAR:
                    return "wchar_t";
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
                case NdrFormatCharacter.FC_WSTRING:
                    return "wchar_t";
                case NdrFormatCharacter.FC_C_CSTRING:
                case NdrFormatCharacter.FC_CSTRING:
                    return "char";
                case NdrFormatCharacter.FC_ENUM16:
                    return "/* enum */ short";
                case NdrFormatCharacter.FC_ENUM32:
                    return "/* enum */ int";
            }

            return String.Format("{0}", Format);
        }

        internal static NdrFormatCharacter ReadFormat(BinaryReader reader)
        {
            return (NdrFormatCharacter)reader.ReadByte();
        }

        private class StandardUserMarshaler
        {
            public NdrKnownTypes KnownType { get; private set; }
            public bool IsMatch(IntPtr ptr)
            {
                return ptr == _size_ptr || ptr == _size_64_ptr;
            }

            public StandardUserMarshaler(SafeLibraryHandle lib, NdrKnownTypes known_type)
            {
                _size_ptr = lib.GetFunctionPointer(String.Format("{0}_UserSize", known_type));
                _size_64_ptr = lib.GetFunctionPointer(String.Format("{0}_UserSize64", known_type));
                KnownType = known_type;
            }

            private IntPtr _size_ptr;
            private IntPtr _size_64_ptr;
        }

        private class StandardUserMarshalers
        {
            List<StandardUserMarshaler> _marshalers;

            public StandardUserMarshalers()
            {
                _marshalers = new List<StandardUserMarshaler>();
                using (SafeLibraryHandle lib = COMUtilities.SafeLoadLibrary("oleaut32.dll"))
                {
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.BSTR));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.VARIANT));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.LPSAFEARRAY));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HWND));
                }

                using (SafeLibraryHandle lib = COMUtilities.SafeLoadLibrary("ole32.dll"))
                {
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HENHMETAFILE));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HMETAFILEPICT));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HMETAFILE));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.SNB));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.STGMEDIUM));
                }

                using (SafeLibraryHandle lib = COMUtilities.SafeLoadLibrary("combase.dll"))
                {
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.CLIPFORMAT));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HACCEL));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HBITMAP));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HBRUSH));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HDC));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HGLOBAL));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HICON));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HMENU));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HMONITOR));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HPALETTE));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HRGN));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.HSTRING));
                    _marshalers.Add(new StandardUserMarshaler(lib, NdrKnownTypes.WdtpInterfacePointer));
                }
            }

            public NdrKnownTypes GetKnownType(IntPtr ptr)
            {
                foreach (var marshaler in _marshalers)
                {
                    if (marshaler.IsMatch(ptr))
                    {
                        return marshaler.KnownType;
                    }
                }
                return NdrKnownTypes.None;
            }
        }

        private static StandardUserMarshalers m_marshalers;

        private static NdrBaseTypeReference FixupUserMarshal(NdrParseContext context, NdrUserMarshalTypeReference type)
        {
            if (context.StubDesc.aUserMarshalQuadruple == IntPtr.Zero)
            {
                return type;
            }

            using (SafeLibraryHandle module = COMUtilities.SafeGetModuleHandle(context.StubDesc.aUserMarshalQuadruple))
            {
                if (module == null)
                {
                    return type;
                }

                if (m_marshalers == null)
                {
                    m_marshalers = new StandardUserMarshalers();
                }

                IntPtr usersize_ptr = COMUtilities.GetTargetAddress(module.DangerousGetHandle(), Marshal.ReadIntPtr(context.StubDesc.aUserMarshalQuadruple, type.QuadrupleIndex * IntPtr.Size * 4));

                NdrKnownTypes known_type = m_marshalers.GetKnownType(usersize_ptr);
                if (known_type != NdrKnownTypes.None)
                {
                    return new NdrKnownTypeReference(known_type);
                }

                //if ((usersize_ptr == m_marshalers.BSTR_UserSizePtr) || (usersize_ptr == m_marshalers.BSTR_UserSize64Ptr))
                //{
                //    return new NdrKnownTypeReference(NdrKnownTypes.BSTR);
                //}
                //if ((usersize_ptr == m_marshalers.VARIANT_UserSizePtr) || (usersize_ptr == m_marshalers.VARIANT_UserSize64Ptr))
                //{
                //    return new NdrKnownTypeReference(NdrKnownTypes.VARIANT);
                //}
                //if ((usersize_ptr == m_marshalers.LPSAFEARRAY_UserSizePtr) || (usersize_ptr == m_marshalers.LPSAFEARRAY_UserSize64Ptr))
                //{
                //    return new NdrKnownTypeReference(NdrKnownTypes.LPSAFEARRAY);
                //}
                //if ((usersize_ptr == m_marshalers.HWND_UserSizePtr) || (usersize_ptr == m_marshalers.HWND_UserSize64Ptr))
                //{
                //    return new NdrKnownTypeReference(NdrKnownTypes.HWND);
                //}
            }
            return type;
        }

        internal static NdrBaseTypeReference FixupSimpleStructureType(NdrSimpleStructureTypeReference type)
        {
            if (type.MemorySize == 16 &&
                type.MembersTypes.Count() == 4)
            {
                NdrBaseTypeReference[] members = type.MembersTypes.ToArray();
                if (members[0].Format == NdrFormatCharacter.FC_LONG 
                    && members[1].Format == NdrFormatCharacter.FC_SHORT 
                    && members[2].Format == NdrFormatCharacter.FC_SHORT 
                    && members[3] is NdrSimpleArrayTypeReference)
                {
                    NdrSimpleArrayTypeReference array = members[3] as NdrSimpleArrayTypeReference;
                    if (array.TotalSize == 8 && array.ElementType.Format == NdrFormatCharacter.FC_BYTE)
                    {
                        return new NdrKnownTypeReference(NdrKnownTypes.GUID);
                    }
                }
            }

            return type;
        }

        internal static int ReadTypeOffset(BinaryReader reader)
        {
            long curr_ofs = reader.BaseStream.Position;
            int ofs = reader.ReadInt16();
            if (ofs == 0)
            {
                return -1;
            }
            return (int)(curr_ofs + ofs);
        }

        public virtual int GetSize()
        {
            switch (Format)
            {
                case NdrFormatCharacter.FC_BYTE:
                case NdrFormatCharacter.FC_CHAR:
                    return 1;
                case NdrFormatCharacter.FC_SMALL:
                case NdrFormatCharacter.FC_USMALL:
                case NdrFormatCharacter.FC_WCHAR:
                case NdrFormatCharacter.FC_SHORT:
                case NdrFormatCharacter.FC_USHORT:
                case NdrFormatCharacter.FC_ENUM16:
                    return 2;
                case NdrFormatCharacter.FC_LONG:
                case NdrFormatCharacter.FC_ULONG:
                case NdrFormatCharacter.FC_FLOAT:
                case NdrFormatCharacter.FC_ENUM32:
                case NdrFormatCharacter.FC_ERROR_STATUS_T:
                    return 4;
                case NdrFormatCharacter.FC_HYPER:
                case NdrFormatCharacter.FC_DOUBLE:
                    return 8;
                case NdrFormatCharacter.FC_INT3264:
                case NdrFormatCharacter.FC_UINT3264:
                    return IntPtr.Size;
                default:
                    return 0;
            }
        }

        internal static NdrBaseTypeReference Read(NdrParseContext context, BinaryReader reader)
        {
            NdrFormatCharacter format = (NdrFormatCharacter)reader.ReadByte();
            
            // Loop to consume padding values.
            while (true)
            {
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
                        return new NdrPointerTypeReference(context, format, reader);
                    case NdrFormatCharacter.FC_IP:
                        return new NdrInterfacePointerTypeReference(context, reader);
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
                        return FixupUserMarshal(context, new NdrUserMarshalTypeReference(context, reader));
                    case NdrFormatCharacter.FC_EMBEDDED_COMPLEX:
                        reader.ReadByte(); // Padding
                        return Read(context, ReadTypeOffset(reader));
                    case NdrFormatCharacter.FC_STRUCT:
                        return FixupSimpleStructureType(new NdrSimpleStructureTypeReference(context, reader));
                    case NdrFormatCharacter.FC_CSTRUCT:
                        return new NdrConformantStructureTypeReference(context, reader);
                    case NdrFormatCharacter.FC_BOGUS_STRUCT:
                        return new NdrBogusStructureTypeReference(context, reader);
                    case NdrFormatCharacter.FC_HARD_STRUCT:
                        return new NdrHardStructureTypeReference(context, reader);
                    case NdrFormatCharacter.FC_PP:
                        return new NdrPointerInfoTypeReference(context, reader);
                    case NdrFormatCharacter.FC_SMFARRAY:
                    case NdrFormatCharacter.FC_LGFARRAY:
                        return new NdrSimpleArrayTypeReference(context, format, reader);
                    case NdrFormatCharacter.FC_CARRAY:
                    case NdrFormatCharacter.FC_CVARRAY:
                        return new NdrConformantArrayTypeReference(format, context, reader);
                    case NdrFormatCharacter.FC_BOGUS_ARRAY:
                        return new NdrBogusArrayTypeReference(context, reader);
                    case NdrFormatCharacter.FC_RANGE:
                        return new NdrRangeTypeReference(reader);
                    // Skipping padding types.
                    case NdrFormatCharacter.FC_PAD:
                        break;
                    case NdrFormatCharacter.FC_STRUCTPAD1:
                    case NdrFormatCharacter.FC_STRUCTPAD2:
                    case NdrFormatCharacter.FC_STRUCTPAD3:
                    case NdrFormatCharacter.FC_STRUCTPAD4:
                    case NdrFormatCharacter.FC_STRUCTPAD5:
                    case NdrFormatCharacter.FC_STRUCTPAD6:
                    case NdrFormatCharacter.FC_STRUCTPAD7:
                        return new NdrStructurePaddingTypeReference(format);
                    default:
                        return new NdrUnknownTypeReference(format);
                }

                format = (NdrFormatCharacter)reader.ReadByte();
            }
        }

        internal static BinaryReader GetReader(NdrParseContext context, int ofs)
        {
            UnmanagedMemoryStream stm = new UnmanagedMemoryStream(
                new SafeBufferWrapper(context.TypeDesc), 0, int.MaxValue);
            stm.Position = ofs;
            return new BinaryReader(stm);
        }

        internal static NdrBaseTypeReference Read(NdrParseContext context, int ofs)
        {
            if (ofs < 0)
            {
                return null;
            }

            IntPtr type_ofs = context.TypeDesc + ofs;
            if (context.TypeCache.ContainsKey(type_ofs))
            {
                return context.TypeCache[type_ofs];
            }

            // Add a pending refence type, this is used only if the current type refers to itself (or indirectly).
            NdrIndirectTypeReference ref_type = new NdrIndirectTypeReference();
            context.TypeCache.Add(type_ofs, ref_type);
            
            NdrBaseTypeReference ret = Read(context, GetReader(context, ofs));
            ref_type.FixupType(ret);
            // Replace type cache entry with real value.
            context.TypeCache[type_ofs] = ret;
            return ret;
        }
    }

    public class NdrProcedureParameter
    {
        public NdrParamAttributes Attributes { get; private set; }
        public NdrBaseTypeReference Type { get; private set; }
        public int Offset { get; private set; }

        internal NdrProcedureParameter(NdrParseContext context, BinaryReader reader)
        {
            Attributes = (NdrParamAttributes)reader.ReadUInt16();
            Offset = reader.ReadUInt16();
            if ((Attributes & NdrParamAttributes.IsBasetype) == 0)
            {
                int type_ofs = reader.ReadUInt16();
                Type = NdrBaseTypeReference.Read(context, type_ofs);
            }
            else
            {
                Type = new NdrBaseTypeReference((NdrFormatCharacter)reader.ReadByte());
                // Remove padding.
                reader.ReadByte();
            }
        }

        public string Format(NdrFormatContext context)
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
                ? Type.FormatType(context) : String.Format("{0}*", Type.FormatType(context));

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

    public class NdrFormatContext
    {
        public IDictionary<Guid, string> IidsToNames { get; private set; }
        public IDictionary<NdrBaseStructureTypeReference, string> StructsToNames { get; private set; }

        public NdrFormatContext(IDictionary<Guid, string> iids_to_names, 
            IDictionary<NdrBaseStructureTypeReference, string> structs_to_names)
        {
            IidsToNames = iids_to_names;
            StructsToNames = structs_to_names;
        }
    }

    internal class NdrParseContext
    {
        public Dictionary<IntPtr, NdrBaseTypeReference> TypeCache { get; private set; }
        public MIDL_STUB_DESC StubDesc { get; private set; }
        public IntPtr TypeDesc { get; private set; }        
        public int CorrDescSize { get; private set; }

        internal NdrParseContext(Dictionary<IntPtr, NdrBaseTypeReference> type_cache, MIDL_STUB_DESC stub_desc, IntPtr type_desc, int desc_size)//, bool is_robust)
        {
            TypeCache = type_cache;
            StubDesc = stub_desc;
            TypeDesc = type_desc;
            CorrDescSize = desc_size;
        }
    }

    public class NdrProcedureDefinition
    {
        public NdrProcedureParameter[] Params { get; private set; }
        public NdrProcedureParameter ReturnValue { get; private set; }
        public int ProcNum { get; private set; }

        public string FormatProcedure(NdrFormatContext context)
        {
            string return_value;

            if (ReturnValue == null)
            {
                return_value = "void";
            }
            else if (ReturnValue.Type.Format == NdrFormatCharacter.FC_LONG)
            {
                return_value = "HRESULT";
            }
            else
            {
                return_value = ReturnValue.Type.FormatType(context);
            }

            return String.Format("{0} Proc{1}({2});", return_value,
                ProcNum, string.Join(", ", Params.Select((p, i) => String.Format("/* Stack Offset: {0} */ {1} p{2}", p.Offset, p.Format(context), i))));
        }
        
        internal NdrProcedureDefinition(Dictionary<IntPtr, NdrBaseTypeReference> type_cache, MIDL_STUB_DESC stub_desc, IntPtr proc_desc, IntPtr type_desc)
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
            NdrProcHeaderExts exts = new NdrProcHeaderExts();
            if ((header.Oi2Flags & NdrInterpreterOptFlags.HasExtensions) == NdrInterpreterOptFlags.HasExtensions)
            {
                // Just ignore extensions.
                int ext_size = Marshal.ReadByte(proc_desc);
                if (Marshal.SizeOf(typeof(NdrProcHeaderExts)) <= ext_size)
                {
                    exts = Marshal.PtrToStructure<NdrProcHeaderExts>(proc_desc);
                }
                proc_desc += ext_size;
            }

            if ((exts.Flags2 & ~NdrInterpreterOptFlags2.Valid) != 0)
            {
                //System.Diagnostics.Trace.WriteLine(exts.Flags2.ToString());
            }

            int desc_size = 4;
            if ((exts.Flags2 & NdrInterpreterOptFlags2.HasNewCorrDesc) != 0)
            {
                desc_size = 6;
                if ((exts.Flags2 & NdrInterpreterOptFlags2.ExtendedCorrDesc) != 0)
                {
                    desc_size = 16;
                }
            }
            NdrParseContext context = new NdrParseContext(type_cache, stub_desc, type_desc, desc_size);

            List<NdrProcedureParameter> ps = new List<NdrProcedureParameter>();

            UnmanagedMemoryStream stm = new UnmanagedMemoryStream(new SafeBufferWrapper(proc_desc), 0, int.MaxValue);
            BinaryReader reader = new BinaryReader(stm);
            bool has_return = (header.Oi2Flags & NdrInterpreterOptFlags.HasReturn) == NdrInterpreterOptFlags.HasReturn;
            int param_count = has_return ? header.NumberParams - 1 : header.NumberParams;
            for (int param = 0; param < param_count; ++param)
            {
                ps.Add(new NdrProcedureParameter(context, reader));
            }
            Params = ps.ToArray();
            if (has_return)
            {
                ReturnValue = new NdrProcedureParameter(context, reader);
            }
        }
    }
}
