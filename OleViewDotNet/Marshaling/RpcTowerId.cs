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

// Note that most of these won't actually work.
public enum RpcTowerId : short
{
    None = 0,
    DNetNSP = 0x04, // ncacn_dnet_dsp
    Tcp = 0x07,     // ncacg_ip_tcp
    Udp = 0x08,     // ncacn_ip_udp
    NetbiosTcp = 0x09, // ncacn_nb_tcp
    Spx = 0x0C,         // ncacn_spx
    NetbiosIpx = 0xD,   // ncacn_np_ipx
    Ipx = 0x0E,         // ncacg_ipx
    NamedPipe = 0xF,    // ncacn_np
    LRPC = 0x10,        // ncalrpc
    NetBIOS = 0x13,     // ncacn_nb_nb
    AppleTalkDSP = 0x16,// ncacn_at_dsp
    AppleTalkDDP = 0x17,// ncacg_at_ddp
    BanyanVinesSPP = 0x1A, // ncacn_vns_spp
    MessageQueue = 0x1D,   // ncadg_mq
    Http = 0x1F,           // ncacn_http
    Container = 0x21,      // ncacn_hvsocket
    StringBinding = 110,
}
