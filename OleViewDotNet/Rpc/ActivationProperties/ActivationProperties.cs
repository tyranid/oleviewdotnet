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

using NtApiDotNet.Ndr.Marshal;
using OleViewDotNet.Marshaling;
using OleViewDotNet.Rpc.Clients;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OleViewDotNet.Rpc.ActivationProperties;

public abstract class ActivationProperties
{
    private readonly Guid m_clsid;
    private readonly Guid m_iid;

    public Guid ClassInfoClsid { get; set; }
    public int DestCtx { get; set; }
    public List<IActivationProperty> Properties { get; }

    protected ActivationProperties(COMObjRefCustom objref, Guid clsid, Guid iid)
    {
        if (objref is null)
        {
            throw new ArgumentNullException(nameof(objref));
        }

        if (objref.Clsid != clsid)
        {
            throw new ArgumentException("Invalid CLSID for activation properties.", nameof(objref));
        }

        if (objref.Iid != iid)
        {
            throw new ArgumentException("Invalid IID for activation properties.", nameof(objref));
        }

        m_clsid = clsid;
        m_iid = iid;
        List<IActivationProperty> properties = new();
        byte[] data = objref.ObjectData;
        if (data.Length < 8 || BitConverter.ToInt32(data, 0) > data.Length)
            throw new ArgumentException("Invalid size for properties data.");
        byte[] ndr_data = new byte[BitConverter.ToInt32(data, 0)];
        Buffer.BlockCopy(data, 8, ndr_data, 0, ndr_data.Length);
        NdrPickledType pickled_type = new(ndr_data);
        NdrUnmarshalBuffer buffer = new(pickled_type);
        var header = buffer.ReadStruct<CustomHeader>();
        ClassInfoClsid = header.classInfoClsid;
        DestCtx = header.destCtx;
        int ofs = header.headerSize + 8;
        for (int i = 0; i < header.cIfs; ++i)
        {
            int length = header.pSizes.GetValue()[i];
            Guid prop_clsid = header.pclsid.GetValue()[i];
            ndr_data = new byte[length];
            Buffer.BlockCopy(data, ofs, ndr_data, 0, length);
            ofs += length;

            if (prop_clsid == ActivationGuids.CLSID_InstantiationInfo)
            {
                properties.Add(new InstantiationInfo(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_ActivationContextInfo)
            {
                properties.Add(new ActivationContextInfo(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_SpecialSystemProperties)
            {
                properties.Add(new SpecialSystemProperties(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_ServerLocationInfo)
            {
                properties.Add(new LocationInfo(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_SecurityInfo)
            {
                properties.Add(new SecurityInfo(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_InstanceInfo)
            {
                properties.Add(new InstanceInfo(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_ScmRequestInfo)
            {
                properties.Add(new ScmRequestInfo(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_WinRTActivationProperties)
            {
                properties.Add(new ComWinRTActivationProperties(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_PropsOutInfo)
            {
                properties.Add(new PropsOut(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_ScmReplyInfo)
            {
                properties.Add(new ScmReplyInfo(ndr_data));
            }
            else
            {
                properties.Add(new UnknownActivationProperty(prop_clsid, data));
            }
        }
        Properties = properties;
    }

    public COMObjRefCustom ToObjRef()
    {
        var ret = new COMObjRefCustom();
        ret.Clsid = m_clsid;
        ret.Iid = m_iid;

        List<Guid> clsids = new();
        List<byte[]> ser = new();

        foreach (var prop in Properties)
        {
            clsids.Add(prop.PropertyClsid);
            byte[] data = prop.Serialize();
            ser.Add(data);
        }

        CustomHeader header = new();
        header.pclsid = clsids.ToArray();
        header.pSizes = ser.Select(b => b.Length).ToArray();
        header.cIfs = Properties.Count;
        byte[] header_data = header.Serialize();
        header.headerSize = header_data.Length;
        header.totalSize = header_data.Length + ser.Sum(b => b.Length);

        MemoryStream stm = new();
        BinaryWriter writer = new(stm);
        writer.Write(header.totalSize);
        writer.Write(0);
        writer.Write(header.Serialize());
        foreach (var ba in ser)
        {
            writer.Write(ba);
        }
        ret.ObjectData = stm.ToArray();
        ret.Reserved = ret.ObjectData.Length;
        
        return ret;
    }
}
