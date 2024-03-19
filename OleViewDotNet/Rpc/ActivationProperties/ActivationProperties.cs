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
    public Guid ClassInfoClsid { get; set; }
    public int DestCtx { get; set; }
    public List<IActivationProperty> Properties { get; }

    protected T FindProperty<T>() where T : IActivationProperty
    {
        return Properties.OfType<T>().FirstOrDefault();
    }

    protected T FindOrCreateProperty<T>() where T : IActivationProperty, new()
    {
        if (!Properties.OfType<T>().Any())
        {
            Properties.Add(new T());
        }
        return Properties.OfType<T>().FirstOrDefault();
    }

    protected ActivationProperties()
    {
        Properties = new();
    }

    protected ActivationProperties(COMObjRefCustom objref)
    {
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
        Properties = new();
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
                Properties.Add(new InstantiationInfo(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_ActivationContextInfo)
            {
                Properties.Add(new ActivationContextInfo(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_SpecialSystemProperties)
            {
                Properties.Add(new SpecialSystemProperties(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_ServerLocationInfo)
            {
                Properties.Add(new LocationInfo(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_SecurityInfo)
            {
                Properties.Add(new SecurityInfo(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_InstanceInfo)
            {
                Properties.Add(new InstanceInfo(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_ScmRequestInfo)
            {
                Properties.Add(new ScmRequestInfo(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_WinRTActivationProperties)
            {
                Properties.Add(new ComWinRTActivationProperties(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_ExtensionActivationContextProperties)
            {
                Properties.Add(new ExtensionActivationContextProperties(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_PropsOutInfo)
            {
                Properties.Add(new PropsOut(ndr_data));
            }
            else if (prop_clsid == ActivationGuids.CLSID_ScmReplyInfo)
            {
                Properties.Add(new ScmReplyInfo(ndr_data));
            }
            else
            {
                Properties.Add(new UnknownActivationProperty(prop_clsid, data));
            }
        }
    }

    public static ActivationProperties Parse(byte[] objref)
    {
        return Parse(COMObjRef.FromArray(objref));
    }

    public static ActivationProperties Parse(COMObjRef objref)
    {
        if (objref is not COMObjRefCustom objref_custom)
        {
            throw new ArgumentException("OBJREF must be custom marshaled.", nameof(objref));
        }

        if (objref_custom.Clsid == ActivationGuids.CLSID_ActivationPropertiesIn)
        {
            if (objref_custom.Iid != ActivationGuids.IID_IActivationPropertiesIn)
            {
                throw new ArgumentException("Invalid IID for IActivationPropertiesIn.", nameof(objref));
            }
            return new ActivationPropertiesIn(objref_custom);
        }
        else if (objref_custom.Clsid == ActivationGuids.CLSID_ActivationPropertiesOut)
        {
            if (objref_custom.Iid != ActivationGuids.IID_IActivationPropertiesOut)
            {
                throw new ArgumentException("Invalid IID for IActivationPropertiesOut.", nameof(objref));
            }
            return new ActivationPropertiesOut(objref_custom);
        }
        else
        {
            throw new ArgumentException("Unknown CLSID for activation properties.", nameof(objref));
        }
    }

    public COMObjRefCustom ToObjRef()
    {
        List<Guid> clsids = new();
        List<byte[]> ser = new();

        foreach (var prop in Properties)
        {
            clsids.Add(prop.PropertyClsid);
            ser.Add(prop.Serialize());
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

        bool act_in = this is ActivationPropertiesIn;
        byte[] data = stm.ToArray();
        return new()
        {
            Clsid = act_in ? ActivationGuids.CLSID_ActivationPropertiesIn : ActivationGuids.CLSID_ActivationPropertiesOut,
            Iid = act_in ? ActivationGuids.IID_IActivationPropertiesIn : ActivationGuids.IID_IActivationPropertiesOut,
            ObjectData = data,
            Reserved = data.Length
        };
    }
}
