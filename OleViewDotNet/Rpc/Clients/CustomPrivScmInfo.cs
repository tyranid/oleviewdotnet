//    This file is part of OleViewDotNet.
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

namespace OleViewDotNet.Rpc.Clients;

internal struct CustomPrivScmInfo : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteInt32(Apartment);
        m.WriteEmbeddedPointer(pwszWinstaDesktop, m.WriteTerminatedString);
        m.WriteInt64(ProcessSignature);
        m.WriteEmbeddedPointer(pEnvBlock, m.WriteConformantArray, (long)EnvBlockLength);
        m.WriteInt32(EnvBlockLength);
    }

    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        Apartment = u.ReadInt32();
        pwszWinstaDesktop = u.ReadEmbeddedPointer(u.ReadConformantVaryingString, false);
        ProcessSignature = u.ReadInt64();
        pEnvBlock = u.ReadEmbeddedPointer(u.ReadConformantArray<char>, false);
        EnvBlockLength = u.ReadInt32();
    }

    int INdrStructure.GetAlignment()
    {
        return 8;
    }
    public int Apartment;
    public NdrEmbeddedPointer<string> pwszWinstaDesktop;
    public long ProcessSignature;
    public NdrEmbeddedPointer<char[]> pEnvBlock;
    public int EnvBlockLength;
    public static CustomPrivScmInfo CreateDefault()
    {
        return new CustomPrivScmInfo();
    }
    public CustomPrivScmInfo(int Apartment, string pwszWinstaDesktop, long ProcessSignature, char[] pEnvBlock, int EnvBlockLength)
    {
        this.Apartment = Apartment;
        this.pwszWinstaDesktop = pwszWinstaDesktop;
        this.ProcessSignature = ProcessSignature;
        this.pEnvBlock = pEnvBlock;
        this.EnvBlockLength = EnvBlockLength;
    }
}
