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

using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace OleViewDotNet.Forms;

internal static class FormUtils
{
    public static string FormatObject(object obj, bool is_return)
    {
        if (obj is null)
        {
            return "<null>";
        }

        if (is_return && obj is int hr)
        {
            return $"0x{hr:X08} - {new Win32Exception(hr).Message}";
        }
        else if (obj is IEnumerable list)
        {
            return string.Join(", ", list.OfType<object>().ToArray());
        }
        return obj.ToString();
    }
}