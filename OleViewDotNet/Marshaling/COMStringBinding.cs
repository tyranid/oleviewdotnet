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
using System.IO;

namespace OleViewDotNet.Marshaling;

public class COMStringBinding
{
    public RpcTowerId TowerId { get; set; }
    public string NetworkAddr { get; set; }

    public COMStringBinding() : this(0, string.Empty)
    {
    }

    public COMStringBinding(RpcTowerId tower_id, string network_addr)
    {
        TowerId = tower_id;
        NetworkAddr = network_addr;
    }

    internal COMStringBinding(BinaryReader reader, bool direct_string)
    {
        if (direct_string)
        {
            try
            {
                TowerId = RpcTowerId.StringBinding;
                NetworkAddr = reader.ReadZString();
            }
            catch (EndOfStreamException)
            {
                NetworkAddr = string.Empty;
            }
        }
        else
        {
            TowerId = (RpcTowerId)reader.ReadInt16();
            if (TowerId != RpcTowerId.None)
            {
                NetworkAddr = reader.ReadZString();
            }
            else
            {
                NetworkAddr = string.Empty;
            }
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
        return $"TowerId: {TowerId} - NetworkAddr: {NetworkAddr}";
    }

    internal COMStringBinding Clone()
    {
        return (COMStringBinding)MemberwiseClone();
    }
}
