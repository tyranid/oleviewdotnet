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

namespace OleViewDotNet.Rpc.ActivationProperties;

public sealed class ActivationPropertiesIn
{
    public Guid ClassInfoClsid { get; set; }
    public int DestCtx { get; set; }
    public List<IActivationProperty> Properties { get; }

    public ActivationPropertiesIn(COMObjRefCustom objref)
    {
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
            Guid clsid = header.pclsid.GetValue()[i];
            ndr_data = new byte[length];
            Buffer.BlockCopy(data, ofs, ndr_data, 0, length);
            ofs += length;

            pickled_type = new(ndr_data);
            if (clsid == ActivationGuids.CLSID_InstantiationInfo)
            {
                properties.Add(new InstantiationInfo(pickled_type));
            }
            else if (clsid == ActivationGuids.CLSID_ActivationContextInfo)
            {
                properties.Add(new ActivationContextInfo(pickled_type));
            }
            else if (clsid == ActivationGuids.CLSID_SpecialSystemProperties)
            {
                properties.Add(new SpecialSystemProperties(pickled_type));
            }
            else if (clsid == ActivationGuids.CLSID_ServerLocationInfo)
            {
                properties.Add(new LocationInfo(pickled_type));
            }
            else if (clsid == ActivationGuids.CLSID_SecurityInfo)
            {
                properties.Add(new SecurityInfo(pickled_type));
            }
            else if (clsid == ActivationGuids.CLSID_InstanceInfo)
            {
                properties.Add(new InstanceInfo(pickled_type));
            }
            else if (clsid == ActivationGuids.CLSID_ScmRequestInfo)
            {
                properties.Add(new ScmRequestInfo(pickled_type));
            }
            else if (clsid == ActivationGuids.CLSID_WinRTActivationProperties)
            {
                properties.Add(new ComWinRTActivationProperties(pickled_type));
            }
            else
            {
                properties.Add(new UnknownActivationProperty(clsid, data));
            }
        }
        Properties = properties;
    }
}
