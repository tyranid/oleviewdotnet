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

namespace OleViewDotNet.Marshaling;

public enum RpcAuthnService : short
{
    None = 0,
    DCEPrivate = 1,
    DCEPublic = 2,
    DECPublic = 4,
    GSS_Negotiate = 9,
    WinNT = 10,
    GSS_SChannel = 14,
    GSS_Kerberos = 16,
    DPA = 17,
    MSN = 18,
    Kernel = 20,
    Digest = 21,
    Unknown22 = 22,
    NegoExtender = 30,
    PKU2U = 31,
    LiveSSP = 32,
    LiveXPSSP = 35,
    MSOnline = 82,
    MQ = 100,
    Default = -1,
}
