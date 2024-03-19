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

namespace OleViewDotNet.Rpc.ActivationProperties;

internal static class ActivationPropertySerializer
{
    public static void Deserialize<T>(this byte[] data, out T value) where T : struct, INdrStructure
    {
        value = new NdrUnmarshalBuffer(new NdrPickledType(data)).ReadStruct<T>();
    }

    public static byte[] Serialize<T>(this T value) where T : struct, INdrStructure
    {
        var m = new NdrMarshalBuffer();
        m.WriteStruct(value);
        return m.ToPickledType().ToArray();
    }
}
