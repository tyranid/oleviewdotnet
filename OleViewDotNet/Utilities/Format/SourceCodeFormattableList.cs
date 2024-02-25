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

using System.Collections.Generic;
using System.Linq;

namespace OleViewDotNet.Utilities.Format;

internal sealed class SourceCodeFormattableList : ICOMSourceCodeFormattable
{
    private readonly List<ICOMSourceCodeFormattable> m_objs;

    public SourceCodeFormattableList(IEnumerable<ICOMSourceCodeFormattable> objs)
    {
        m_objs = objs.ToList();
    }

    public bool IsFormattable => m_objs.Count > 0;

    void ICOMSourceCodeFormattable.Format(COMSourceCodeBuilder builder)
    {
        foreach (var obj in m_objs)
        {
            obj.Format(builder);
            builder.AppendLine();
        }
    }
}