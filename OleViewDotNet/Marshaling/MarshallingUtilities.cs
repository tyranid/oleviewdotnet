//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
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

using System;
using System.IO;
using System.Text;

namespace OleViewDotNet.Marshaling;

internal static class MarshallingUtilities
{
    internal static byte[] ReadAll(this BinaryReader reader, int length)
    {
        byte[] ret = reader.ReadBytes(length);
        if (ret.Length != length)
        {
            throw new EndOfStreamException();
        }
        return ret;
    }

    internal static Guid ReadGuid(this BinaryReader reader)
    {
        return new Guid(reader.ReadAll(16));
    }

    internal static char ReadUnicodeChar(this BinaryReader reader)
    {
        return BitConverter.ToChar(reader.ReadAll(2), 0);
    }

    internal static void Write(this BinaryWriter writer, Guid guid)
    {
        writer.Write(guid.ToByteArray());
    }

    internal static string ReadZString(this BinaryReader reader)
    {
        StringBuilder builder = new();
        char ch = reader.ReadUnicodeChar();
        while (ch != 0)
        {
            builder.Append(ch);
            ch = reader.ReadUnicodeChar();
        }
        return builder.ToString();
    }

    internal static void WriteZString(this BinaryWriter writer, string str)
    {
        writer.Write(Encoding.Unicode.GetBytes(str + "\0"));
    }
}
