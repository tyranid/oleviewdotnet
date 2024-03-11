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

using OleViewDotNet.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace OleViewDotNet.Marshaling;

public class COMObjRefStandard : COMObjRef
{
    internal COMStdObjRef _stdobjref;
    internal COMDualStringArray _stringarray;

    public COMStdObjRefFlags StdFlags { get => _stdobjref.StdFlags; set => _stdobjref.StdFlags = value; }
    public int PublicRefs { get => _stdobjref.PublicRefs; set => _stdobjref.PublicRefs = value; }
    public ulong Oxid { get => _stdobjref.Oxid; set => _stdobjref.Oxid = value; }
    public ulong Oid { get => _stdobjref.Oid; set => _stdobjref.Oid = value; }
    public Guid Ipid { get => _stdobjref.Ipid; set => _stdobjref.Ipid = value; }

    public List<COMStringBinding> StringBindings => _stringarray.StringBindings;
    public List<COMSecurityBinding> SecurityBindings => _stringarray.SecurityBindings;

    public int ProcessId => COMUtilities.GetProcessIdFromIPid(Ipid);
    public string ProcessName => MiscUtilities.GetProcessNameById(ProcessId);
    public int ApartmentId => COMUtilities.GetApartmentIdFromIPid(Ipid);
    public string ApartmentName => COMUtilities.GetApartmentIdStringFromIPid(Ipid);

    internal COMObjRefStandard(BinaryReader reader, Guid iid)
        : base(iid)
    {
        _stdobjref = new COMStdObjRef(reader);
        _stringarray = new COMDualStringArray(reader);
    }

    protected COMObjRefStandard(Guid iid) : base(iid)
    {
    }

    protected COMObjRefStandard(COMObjRefStandard std) : base(std.Iid)
    {
        _stdobjref = std._stdobjref.Clone();
        _stringarray = std._stringarray.Clone();
    }

    public COMObjRefStandard() : base(Guid.Empty)
    {
        _stdobjref = new COMStdObjRef();
        _stringarray = new COMDualStringArray();
    }

    protected override void Serialize(BinaryWriter writer)
    {
        _stdobjref.ToWriter(writer);
        _stringarray.ToWriter(writer);
    }

    public COMObjRefHandler ToHandler(Guid clsid)
    {
        return new COMObjRefHandler(clsid, this);
    }
}
