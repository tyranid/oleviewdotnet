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

using NtApiDotNet.Win32.Rpc.Transport;
using System.IO;

namespace OleViewDotNet.Marshaling;

public class COMSecurityBinding
{
    public RpcAuthnService AuthnSvc { get; set; }
    public string PrincName { get; set; }

    public COMSecurityBinding() : this(0, string.Empty)
    {
    }

    public COMSecurityBinding(RpcAuthnService authn_svc, string princ_name)
    {
        AuthnSvc = authn_svc;
        PrincName = princ_name;
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
            PrincName = string.Empty;
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
        return $"AuthnSvc: {AuthnSvc} - PrincName: {PrincName}";
    }

    internal COMSecurityBinding Clone()
    {
        return (COMSecurityBinding)MemberwiseClone();
    }
}
