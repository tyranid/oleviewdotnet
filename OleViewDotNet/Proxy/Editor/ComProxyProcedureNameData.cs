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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace OleViewDotNet.Proxy.Editor;

[DataContract]
public sealed class COMProxyProcedureNameData
{
    [DataMember]
    public int Index { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public List<COMProxyProcedureParameterNameData> Parameters { get; set; }

    public COMProxyProcedureNameData()
    {
    }

    internal COMProxyProcedureNameData(COMProxyInterfaceProcedure procedure, int index)
    {
        Index = index;
        Name = procedure.Name;
        Parameters = procedure.Parameters.Select((p, i) => new COMProxyProcedureParameterNameData(p, i)).ToList();
    }

    internal void UpdateNames(COMProxyInterfaceProcedure procedure)
    {
        procedure.Name = Name;
        
        if (Parameters is not null)
        {
            var ps = procedure.Parameters;
            foreach (var p in Parameters)
            {
                if (ps.Count > p.Index)
                {
                    ps[p.Index].Name = p.Name;
                }
            }
        }
    }
}

