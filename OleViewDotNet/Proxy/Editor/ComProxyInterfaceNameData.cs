//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2018
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

using NtApiDotNet.Ndr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace OleViewDotNet.Proxy.Editor;

[DataContract]
public sealed class COMProxyInterfaceNameData
{
    [DataMember]
    public Guid Iid { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public List<COMProxyStructureNameData> Structures { get; set; }
    [DataMember]
    public List<COMProxyProcedureNameData> Procedures { get; set; }

    public COMProxyInterfaceNameData()
    {
    }

    internal COMProxyInterfaceNameData(COMProxyInterface proxy)
    {
        Iid = proxy.Iid;
        Name = proxy.Name;
        Structures = proxy.ComplexTypes.OfType<NdrBaseStructureTypeReference>()
            .Select((s, i) => new COMProxyStructureNameData(s, i)).ToList();
        Procedures = proxy.Procedures.Select((p,i) => new COMProxyProcedureNameData(p, i)).ToList();
    }

    internal void UpdateNames(COMProxyInterface proxy)
    {
        if (Structures is not null)
        {
            var structures = proxy.ComplexTypes.OfType<NdrBaseStructureTypeReference>().ToList();
            foreach (var s in Structures)
            {
                if (structures.Count > s.Index)
                {
                    s.UpdateNames(structures[s.Index]);
                }
            }
        }

        if (Procedures is not null)
        {
            var procedures = proxy.Procedures.ToList();
            foreach (var p in Procedures)
            {
                if (procedures.Count > p.Index)
                {
                    p.UpdateNames(procedures[p.Index]);
                }
            }
        }
    }

    public string ToXml()
    {
        DataContractSerializer ser = new(typeof(COMProxyInterfaceNameData));
        XmlWriterSettings settings = new()
        {
            OmitXmlDeclaration = true,
            Indent = true
        };
        StringBuilder builder = new();
        using (XmlWriter writer = XmlWriter.Create(builder, settings))
        {
            ser.WriteObject(writer, this);
        }
        return builder.ToString();
    }

    public static COMProxyInterfaceNameData Parse(string xml)
    {
        DataContractSerializer ser = new(typeof(COMProxyInterfaceNameData));
        using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
        {
            return (COMProxyInterfaceNameData)ser.ReadObject(reader);
        }
    }
}

