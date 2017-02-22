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
using System.IO;

namespace OleViewDotNet
{
    [Flags]
    public enum COMObjrefFlags
    {
        None = 0,
        Standard = 1,
        Handler = 2,
        Custom = 4,
        Extended = 8,
    }

    public enum RpcAuthnService : short
    {
        None          = 0,
        DCEPrivate   = 1,
        DCEPublic    = 2,
        DECPublic    = 4,
        GSS_Negotiate = 9,
        WinNT        = 10,
        GSS_SChannel = 14,
        GSS_Kerberos = 16,
        DPA          = 17,
        MSN          = 18,
        Digest       = 21,
        Kernel       =  20,
        NegoExtender = 30,
        PKU2U        = 31,
        LiveSSP     = 32,
        LiveXPSSP   = 35,
        MSOnline     = 82,
        MQ          = 100,
        Default     = -1,
    }

    public enum RpcTowerId : short
    {
        None = 0,
        Udp = 0x08,
	    Tcp = 0x07,
	    Ipx = 0x0E,
	    Spx = 0x0C,
	    NetBIOS = 0x12,
	    DNetNSP = 0x04,
	    LRPC = 0x10,
    }

    public class COMStringBinding
    {
        public RpcTowerId TowerId { get; set; }
        public string NetworkAddr { get; set; }

        public COMStringBinding()
        {
            TowerId = 0;
            NetworkAddr = String.Empty;
        }

        internal COMStringBinding(BinaryReader reader)
        {
            TowerId = (RpcTowerId)reader.ReadInt16();
            if (TowerId != RpcTowerId.None)
            {
                NetworkAddr = reader.ReadZString();
            }
            else
            {
                NetworkAddr = String.Empty;
            }
        }

        public void ToWriter(BinaryWriter writer)
        {
            writer.Write((short)TowerId);
            if (TowerId != 0)
            {
                writer.WriteZString(NetworkAddr);
            }
        }

        public override string ToString()
        {
            return String.Format("TowerId: {0} - NetworkAddr: {1}",
                TowerId, NetworkAddr);
        }
    }

    public class COMSecurityBinding
    {
        public RpcAuthnService AuthnSvc { get; set; }
        public string PrincName { get; set; }

        public COMSecurityBinding()
        {
            AuthnSvc = 0;
            PrincName = String.Empty;
        }

        internal COMSecurityBinding(BinaryReader reader)
        {
            AuthnSvc = (RpcAuthnService)reader.ReadInt16();
            if (AuthnSvc != 0)
            {
                // Reserved
                reader.ReadInt16();
                PrincName = reader.ReadZString();
            }
            else
            {
                PrincName = String.Empty;
            }
        }

        public void ToWriter(BinaryWriter writer)
        {
            writer.Write((short)AuthnSvc);
            if (AuthnSvc != 0)
            {
                writer.Write((ushort)0xFFFF);
                writer.WriteZString(PrincName);
            }
        }

        public override string ToString()
        {
            return String.Format("AuthnSvc: {0} - PrincName: {1}",
                AuthnSvc, PrincName);
        }
    }

    public class COMDualStringArray
    {
        public List<COMStringBinding> StringBindings { get; private set; }
        public List<COMSecurityBinding> SecurityBindings { get; private set; }

        public COMDualStringArray()
        {
            StringBindings = new List<COMStringBinding>();
            SecurityBindings = new List<COMSecurityBinding>();
        }

        internal COMDualStringArray(BinaryReader reader) : this()
        {            
            int num_entries = reader.ReadUInt16();
            int sec_offset = reader.ReadUInt16();

            if (num_entries > 0)
            {
                MemoryStream stm = new MemoryStream(reader.ReadAll(num_entries * 2));
                BinaryReader new_reader = new BinaryReader(stm);

                COMStringBinding str = new COMStringBinding(new_reader);
                while (str.TowerId != 0)
                {
                    StringBindings.Add(str);
                    str = new COMStringBinding(new_reader);
                }

                new_reader.BaseStream.Position = sec_offset * 2;
                COMSecurityBinding sec = new COMSecurityBinding(new_reader);
                while (sec.AuthnSvc != 0)
                {
                    SecurityBindings.Add(sec);
                    sec = new COMSecurityBinding(new_reader);
                }
            }
        }

        public void ToWriter(BinaryWriter writer)
        {
            MemoryStream stm = new MemoryStream();
            BinaryWriter new_writer = new BinaryWriter(stm);
            foreach (COMStringBinding str in StringBindings)
            {
                str.ToWriter(new_writer);
            }
            new COMStringBinding().ToWriter(new_writer);
            ushort ofs = (ushort)(stm.Position / 2);
            foreach (COMSecurityBinding sec in SecurityBindings)
            {
                sec.ToWriter(new_writer);
            }
            new COMSecurityBinding().ToWriter(new_writer);
            writer.Write((ushort)(stm.Length / 2));
            writer.Write(ofs);
            writer.Write(stm.ToArray());
        }
    }

    public abstract class COMObjRef
    {
        public const int OBJREF_MAGIC = 0x574f454d;
        
        public Guid Iid { get; set; }

        public COMObjrefFlags Flags
        {
            get
            {
                if (this is COMObjRefCustom)
                {
                    return COMObjrefFlags.Custom;
                }
                else if (this is COMObjRefStandard)
                {
                    return COMObjrefFlags.Standard;
                }
                else if (this is COMObjRefHandler)
                {
                    return COMObjrefFlags.Handler;
                }
                else
                {
                    return COMObjrefFlags.None;
                }
            }
        }

        public byte[] ToArray()
        {
            MemoryStream stm = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stm);
            writer.Write(OBJREF_MAGIC);
            writer.Write((int)Flags);
            writer.Write(Iid);
            Serialize(writer);
            return stm.ToArray();
        }

        protected abstract void Serialize(BinaryWriter writer);

        protected COMObjRef(Guid iid)
        {
            Iid = iid;
        }

        public static COMObjRef FromArray(byte[] arr)
        {
            MemoryStream stm = new MemoryStream(arr);
            BinaryReader reader = new BinaryReader(stm);
            int magic = reader.ReadInt32();
            if (magic != OBJREF_MAGIC)
            {
                throw new ArgumentException("Invalid OBJREF Magic");
            }            

            COMObjrefFlags flags = (COMObjrefFlags)reader.ReadInt32();
            Guid iid = reader.ReadGuid();
            switch (flags)
            {
                case COMObjrefFlags.Custom:
                    return new COMObjRefCustom(reader, iid);
                case COMObjrefFlags.Standard:
                    return new COMObjRefStandard(reader, iid);
                case COMObjrefFlags.Handler:
                    return new COMObjRefHandler(reader, iid);
                case COMObjrefFlags.Extended:
                default:
                    throw new ArgumentException("Invalid OBJREF Type Flags");
            }
        }
    }

    public class COMObjRefCustom : COMObjRef
    {
        public Guid Clsid { get; set; }
        public byte[] ObjectData { get; set; }

        public COMObjRefCustom() 
            : base(COMInterfaceEntry.IID_IUnknown)
        {
            ObjectData = new byte[0];
        }

        internal COMObjRefCustom(BinaryReader reader, Guid iid) 
            : base(iid)
        {
            Clsid = reader.ReadGuid();
            // Reserved.
            reader.ReadInt32();
            // Technically size but can be 0.
            reader.ReadInt32();
            // Read to end of stream.
            ObjectData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
        }

        protected override void Serialize(BinaryWriter writer)
        {
            writer.Write(Clsid);
            writer.Write(ObjectData.Length);
            writer.Write(0);
            writer.Write(ObjectData);
        }
    }

    public class COMStdObjRef
    {
        public int Flags { get; set; }
        public int PublicRefs { get; set; }
        public ulong Oxid { get; set; }
        public ulong Oid { get; set; }
        public Guid Ipid { get; set; }

        public COMStdObjRef()
        {
        }

        internal COMStdObjRef(BinaryReader reader)
        {
            Flags = reader.ReadInt32();
            PublicRefs = reader.ReadInt32();
            Oxid = reader.ReadUInt64();
            Oid = reader.ReadUInt64();
            Ipid = reader.ReadGuid();
        }

        public void ToWriter(BinaryWriter writer)
        {
            writer.Write(Flags);
            writer.Write(PublicRefs);
            writer.Write(Oxid);
            writer.Write(Oid);
            writer.Write(Ipid);
        }
    }

    public class COMObjRefStandard : COMObjRef
    {
        public COMStdObjRef StdObjRef { get; protected set; }
        public COMDualStringArray StringArray { get; protected set; }

        internal COMObjRefStandard(BinaryReader reader, Guid iid) 
            : base(iid)
        {
            StdObjRef = new COMStdObjRef(reader);
            StringArray = new COMDualStringArray(reader);
        }

        protected COMObjRefStandard(Guid iid) : base(iid)
        {
        }

        public COMObjRefStandard() : base(Guid.Empty)
        {
            StdObjRef = new COMStdObjRef();
            StringArray = new COMDualStringArray();
        }

        protected override void Serialize(BinaryWriter writer)
        {
            StdObjRef.ToWriter(writer);
            StringArray.ToWriter(writer);
        }
    }

    public class COMObjRefHandler : COMObjRefStandard
    {
        public Guid Clsid { get; set; }

        internal COMObjRefHandler(BinaryReader reader, Guid iid) 
            : base(iid)
        {
            StdObjRef = new COMStdObjRef(reader);
            Clsid = reader.ReadGuid();
            StringArray = new COMDualStringArray(reader);
        }

        public COMObjRefHandler() : base()
        {        
        }

        protected override void Serialize(BinaryWriter writer)
        {
            StdObjRef.ToWriter(writer);
            writer.Write(Clsid);
            StringArray.ToWriter(writer);
        }
    }
}
