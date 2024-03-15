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

internal struct _COAUTHIDENTITY : INdrStructure
{
    void INdrStructure.Marshal(NdrMarshalBuffer m)
    {
        m.WriteEmbeddedPointer(User, m.WriteConformantArray, (long)(UserLength + 1));
        m.WriteInt32(UserLength);
        m.WriteEmbeddedPointer(Domain, m.WriteConformantArray, (long)(DomainLength + 1));
        m.WriteInt32(DomainLength);
        m.WriteEmbeddedPointer(Password, m.WriteConformantArray, (long)(PasswordLength + 1));
        m.WriteInt32(PasswordLength);
        m.WriteInt32(Flags);
    }
    void INdrStructure.Unmarshal(NdrUnmarshalBuffer u)
    {
        User = u.ReadEmbeddedPointer(u.ReadConformantArray<short>, false);
        UserLength = u.ReadInt32();
        Domain = u.ReadEmbeddedPointer(u.ReadConformantArray<short>, false);
        DomainLength = u.ReadInt32();
        Password = u.ReadEmbeddedPointer(u.ReadConformantArray<short>, false);
        PasswordLength = u.ReadInt32();
        Flags = u.ReadInt32();
    }
    int INdrStructure.GetAlignment()
    {
        return 4;
    }
    public NdrEmbeddedPointer<short[]> User;
    public int UserLength;
    public NdrEmbeddedPointer<short[]> Domain;
    public int DomainLength;
    public NdrEmbeddedPointer<short[]> Password;
    public int PasswordLength;
    public int Flags;
    public static _COAUTHIDENTITY CreateDefault()
    {
        return new _COAUTHIDENTITY();
    }
    public _COAUTHIDENTITY(short[] User, int UserLength, short[] Domain, int DomainLength, short[] Password, int PasswordLength, int Flags)
    {
        this.User = User;
        this.UserLength = UserLength;
        this.Domain = Domain;
        this.DomainLength = DomainLength;
        this.Password = Password;
        this.PasswordLength = PasswordLength;
        this.Flags = Flags;
    }
}
